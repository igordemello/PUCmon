// Dense planar tracker: inverse-compositional Lucas-Kanade on a homography (Baker & Matthews, "Lucas-Kanade 20 Years
// On", 2004), coarse to fine, with affine brightness compensation and Huber weights. It aligns every strong-gradient
// pixel of the target, so it holds low-texture art (large flat areas, strong edges) with sub-pixel precision, where
// keypoint trackers run short of points and the tilt turns ambiguous.
//
// Coordinates: the template is addressed in normalized coordinates x~ = (pixel centre - image centre) / (height / 2),
// the same at every pyramid level. G (3x3, row-major) maps x~ to frame pixels (pixel centres at +0.5).

const LEVELS = 3;

// Scratch arrays: a caller that builds a pyramid every frame passes a pool (any object) so the same buffers are
// reused instead of allocating ~1 MB per frame, which makes the garbage collector stutter on phones.
function buffer(pool, key, size) {
  if (!pool) return new Float32Array(size);
  const b = pool[key];
  return b && b.length === size ? b : (pool[key] = new Float32Array(size));
}

// Gray image {data, width, height} -> half size (2x2 box filter).
function half({ data, width, height }, pool, key) {
  const w = width >> 1, h = height >> 1, out = buffer(pool, key, w * h);
  for (let y = 0; y < h; y++) {
    const r0 = 2 * y * width, r1 = r0 + width;
    for (let x = 0; x < w; x++) {
      const i = 2 * x;
      out[y * w + x] = 0.25 * (data[r0 + i] + data[r0 + i + 1] + data[r1 + i] + data[r1 + i + 1]);
    }
  }
  return { data: out, width: w, height: h };
}

// Shrink with exact area averaging (fractional pixel coverage), so output pixel x covers source [x*s, (x+1)*s).
export function resize({ data, width, height }, w, h) {
  const along = (n, m) => {  // source weights for each of m outputs over n inputs
    const s = n / m, spans = [];
    for (let o = 0; o < m; o++) {
      const a = o * s, b = (o + 1) * s, taps = [];
      for (let i = Math.floor(a); i < Math.min(n, Math.ceil(b)); i++) taps.push([i, (Math.min(b, i + 1) - Math.max(a, i)) / s]);
      spans.push(taps);
    }
    return spans;
  };
  const cols = along(width, w), rows = along(height, h), tmp = new Float32Array(w * height), out = new Float32Array(w * h);
  for (let y = 0; y < height; y++) for (let x = 0; x < w; x++) {
    let s = 0;
    for (const [i, k] of cols[x]) s += k * data[y * width + i];
    tmp[y * w + x] = s;
  }
  for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) {
    let s = 0;
    for (const [i, k] of rows[y]) s += k * tmp[i * w + x];
    out[y * w + x] = s;
  }
  return { data: out, width: w, height: h };
}

// Separable [1 4 6 4 1] / 16 blur: widens edges so LK converges from further away, and makes the template and the
// (optically blurred) camera image look alike.
function blur({ data, width: w, height: h }, pool, key) {
  const tmp = buffer(pool, key + 't', w * h), out = buffer(pool, key + 'o', w * h);
  const at = (x, n) => (x < 0 ? 0 : x >= n ? n - 1 : x);
  for (let y = 0; y < h; y++) {  // rows: clamped ends, fast interior
    const r = y * w;
    for (let x = 0; x < w; x++) {
      if (x === 2 && w > 4) {
        for (; x < w - 2; x++) {
          const i = r + x;
          tmp[i] = (data[i - 2] + data[i + 2] + 4 * (data[i - 1] + data[i + 1]) + 6 * data[i]) * 0.0625;
        }
        if (x >= w) break;
      }
      tmp[r + x] = (data[r + at(x - 2, w)] + data[r + at(x + 2, w)] + 4 * (data[r + at(x - 1, w)] + data[r + at(x + 1, w)]) + 6 * data[r + x]) * 0.0625;
    }
  }
  for (let y = 0; y < h; y++) {  // columns
    const up2 = at(y - 2, h) * w, up1 = at(y - 1, h) * w, r = y * w, dn1 = at(y + 1, h) * w, dn2 = at(y + 2, h) * w;
    for (let x = 0; x < w; x++) {
      out[r + x] = (tmp[up2 + x] + tmp[dn2 + x] + 4 * (tmp[up1 + x] + tmp[dn1 + x]) + 6 * tmp[r + x]) * 0.0625;
    }
  }
  return { data: out, width: w, height: h };
}

// Blurred pyramid, finest first. Frames and the template go through the same filters.
export function pyramid(image, levels = LEVELS, pool) {
  const out = [blur(image, pool, 'b0')];
  while (out.length < levels) out.push(blur(half(out[out.length - 1], pool, 'h' + out.length), pool, 'b' + out.length));
  return out;
}

// Pixels of one template level with a usable gradient (evenly thinned to maxPoints), away from the border.
// Each point: x~, y~, T, and the 8 steepest-descent terms dT/dp of the homography warp at p = 0.
// (sx, sy): original-template pixels per pixel of this level, so x~ is exact whatever the rounding of level sizes.
function templatePoints({ data, width, height }, maxPoints, sx, sy, W, H) {
  const all = [];
  for (let y = 2; y < height - 2; y++) {
    for (let x = 2; x < width - 2; x++) {
      const i = y * width + x;
      const gx = (data[i + 1] - data[i - 1]) / 2, gy = (data[i + width] - data[i - width]) / 2;
      if (gx * gx + gy * gy > 4) all.push([x, y, gx, gy, data[i]]);  // |gradient| > 2 grey levels per pixel
    }
  }
  const step = Math.max(1, all.length / maxPoints), pts = [];
  for (let s = 0; s < all.length; s += step) pts.push(all[Math.floor(s)]);
  const out = new Float32Array(pts.length * 11);
  pts.forEach(([x, y, gx, gy, t], k) => {
    const u = ((x + 0.5) * sx - W / 2) / (H / 2), v = ((y + 0.5) * sy - H / 2) / (H / 2);
    const Gx = gx * (H / 2) / sx, Gy = gy * (H / 2) / sy;  // gradient per normalized unit
    const d = Gx * u + Gy * v;
    out.set([u, v, t, Gx * u, Gy * u, Gx * v, Gy * v, Gx, Gy, -u * d, -v * d], k * 11);
  });
  return out;
}

// Solve the symmetric 8x8 system A x = b (Gaussian elimination with partial pivoting). Returns null if singular.
function solve8(A, b) {
  const n = 8, M = A.map((row, i) => [...row, b[i]]);
  for (let c = 0; c < n; c++) {
    let p = c;
    for (let r = c + 1; r < n; r++) if (Math.abs(M[r][c]) > Math.abs(M[p][c])) p = r;
    if (Math.abs(M[p][c]) < 1e-12) return null;
    [M[c], M[p]] = [M[p], M[c]];
    for (let r = c + 1; r < n; r++) {
      const f = M[r][c] / M[c][c];
      for (let k = c; k <= n; k++) M[r][k] -= f * M[c][k];
    }
  }
  const x = new Array(n);
  for (let r = n - 1; r >= 0; r--) {
    let s = M[r][n];
    for (let k = r + 1; k < n; k++) s -= M[r][k] * x[k];
    x[r] = s / M[r][r];
  }
  return x;
}

export const mul3 = (a, b) => [0, 1, 2].flatMap(r => [0, 1, 2].map(c => a[3 * r] * b[c] + a[3 * r + 1] * b[3 + c] + a[3 * r + 2] * b[6 + c]));

export function inv3(m) {
  const [a, b, c, d, e, f, g, h, i] = m;
  const A = e * i - f * h, B = -(d * i - f * g), C = d * h - e * g, det = a * A + b * B + c * C;
  return [A, -(b * i - c * h), b * f - c * e, B, a * i - c * g, -(a * f - c * d), C, -(a * h - b * g), a * e - b * d].map(v => v / det);
}

export const apply3 = (m, x, y) => {
  const w = m[6] * x + m[7] * y + m[8];
  return [(m[0] * x + m[1] * y + m[2]) / w, (m[3] * x + m[4] * y + m[5]) / w];
};

export class PlanarTracker {
  // template: gray {data, width, height} of the target (any size); it is worked at `height` px tall.
  constructor(template, { height = 240, points = [2400, 1200, 600] } = {}) {
    const { width: W, height: H } = template;
    const base = resize(template, Math.round(W * height / H), height);
    this.aspect = W / H;
    this.levels = pyramid(base).map((img, l) => {
      const sx = (W / base.width) * (1 << l), sy = (H / base.height) * (1 << l);  // original px per level px
      return { halfHeight: H / 2 / sy, pts: templatePoints(img, points[l], sx, sy, W, H) };
    });
    this.pointCount = this.levels.map(l => l.pts.length / 11);
    const most = Math.max(...this.pointCount);
    this.samples = new Float32Array(most);  // per-point scratch reused every frame
    this.inFrame = new Uint8Array(most);
  }

  // Refine G (normalized template -> frame px) against a frame pyramid (from pyramid()). Returns
  // {G, score: zero-mean normalized cross-correlation on the finest level, visible: share of points in frame}.
  track(frame, G, { iterations = 12 } = {}) {
    if (!G.every(Number.isFinite)) return { G, score: 0, visible: 0 };
    G = G.slice();
    let score = 0, visible = 0;
    for (let l = this.levels.length - 1; l >= 0; l--) {
      const { halfHeight, pts } = this.levels[l];
      // Frame level whose pixels best match this template level's (avoids aliasing on small or far targets).
      const [x0, y0] = apply3(G, 0, -1), [x1, y1] = apply3(G, 0, 1);
      const framePxPerTemplatePx = Math.hypot(x1 - x0, y1 - y0) / 2 / halfHeight;
      const L = Math.max(0, Math.min(frame.length - 1, Math.round(Math.log2(Math.max(framePxPerTemplatePx, 1e-6)))));
      const res = this.iterate(frame[L], 1 / (1 << L), pts, G, iterations);
      if (!res) return { G, score: 0, visible: 0 };
      G = res.G; score = res.score; visible = res.visible;
    }
    return { G, score, visible };
  }

  iterate(img, scale, pts, G, iterations) {
    const { data, width, height } = img, n = pts.length / 11, I = this.samples, ok = this.inFrame;
    let score = 0, visible = 0;
    for (let it = 0; it < iterations; it++) {
      // Sample the frame at the warped template points.
      let cnt = 0, sT = 0, sI = 0, sTT = 0, sTI = 0;
      for (let k = 0; k < n; k++) {
        const o = 11 * k, u = pts[o], v = pts[o + 1];
        const z = G[6] * u + G[7] * v + G[8];
        const x = ((G[0] * u + G[1] * v + G[2]) / z) * scale - 0.5, y = ((G[3] * u + G[4] * v + G[5]) / z) * scale - 0.5;
        const xi = Math.floor(x), yi = Math.floor(y);
        if (z <= 0 || xi < 0 || yi < 0 || xi >= width - 1 || yi >= height - 1) { ok[k] = 0; continue; }
        const fx = x - xi, fy = y - yi, i = yi * width + xi;
        I[k] = (data[i] * (1 - fx) + data[i + 1] * fx) * (1 - fy) + (data[i + width] * (1 - fx) + data[i + width + 1] * fx) * fy;
        ok[k] = 1; cnt++;
        const t = pts[o + 2]; sT += t; sI += I[k]; sTT += t * t; sTI += t * I[k];
      }
      if (cnt < 20) return null;
      // Brightness/contrast of the camera: fit I ~ a T + b, then compare I' = (I - b) / a with T.
      const mT = sT / cnt, mI = sI / cnt, vT = sTT / cnt - mT * mT, cTI = sTI / cnt - mT * mI;
      if (vT <= 1e-6) return null;
      const a = Math.abs(cTI) > 1e-6 ? cTI / vT : 1, b = mI - a * mT;
      let absSum = 0, sII = 0;
      for (let k = 0; k < n; k++) if (ok[k]) { const e = (I[k] - b) / a - pts[11 * k + 2]; absSum += Math.abs(e); sII += (I[k] - mI) ** 2; }
      visible = cnt / n;
      score = cTI / Math.sqrt(vT * (sII / cnt) + 1e-9);  // ZNCC
      // Huber weights (scale from the mean absolute residual), then the weighted Gauss-Newton step.
      const huber = 1.5 * 1.253 * absSum / cnt + 1e-3;
      const H = Array.from({ length: 8 }, () => new Array(8).fill(0)), g = new Array(8).fill(0);
      for (let k = 0; k < n; k++) {
        if (!ok[k]) continue;
        const o = 11 * k, e = (I[k] - b) / a - pts[o + 2], ae = Math.abs(e);
        const wk = ae <= huber ? 1 : huber / ae;
        for (let r = 0; r < 8; r++) {
          const sr = pts[o + 3 + r] * wk;
          g[r] += sr * e;
          for (let c = r; c < 8; c++) H[r][c] += sr * pts[o + 3 + c];
        }
      }
      for (let r = 0; r < 8; r++) for (let c = 0; c < r; c++) H[r][c] = H[c][r];
      const dp = solve8(H, g);
      if (!dp) return null;
      // G <- G * W(dp)^-1 (inverse compositional update).
      const W = [1 + dp[0], dp[2], dp[4], dp[1], 1 + dp[3], dp[5], dp[6], dp[7], 1];
      G = mul3(G, inv3(W));
      if (Math.max(...dp.map(Math.abs)) < 1e-4) break;
    }
    return { G, score, visible };
  }
}
