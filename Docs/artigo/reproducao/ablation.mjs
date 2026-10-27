// Offline ablation of the dense tracker (Baker-Matthews protocol): render each target under a random homography,
// perturb the 4 corners by sigma px, align, count convergence (< 1 px corner RMS). node ablation.mjs <project>
import fs from 'node:fs';
import zlib from 'node:zlib';
import path from 'node:path';
import { pathToFileURL, fileURLToPath } from 'node:url';

const project = process.argv[2];
const { PlanarTracker, pyramid, resize, apply3, mul3, inv3 } = await import(pathToFileURL(path.join(project, 'Assets/WebGLTemplates/FreeWebAR/tracker.js')));

// 8-bit PNG (gray/RGB/RGBA, non-interlaced) -> gray Float32 from the first channel (targets are already gray).
function readPng(file) {
  const b = fs.readFileSync(file), chunks = [];
  let w, h, ct, p = 8;
  while (p < b.length) {
    const n = b.readUInt32BE(p), type = b.toString('ascii', p + 4, p + 8), d = b.subarray(p + 8, p + 8 + n);
    if (type === 'IHDR') { w = d.readUInt32BE(0); h = d.readUInt32BE(4); ct = d[9]; }
    if (type === 'IDAT') chunks.push(d);
    p += 12 + n;
  }
  const bpp = { 0: 1, 2: 3, 4: 2, 6: 4 }[ct], raw = zlib.inflateSync(Buffer.concat(chunks)), stride = w * bpp;
  const px = new Uint8Array(h * stride);
  for (let y = 0; y < h; y++) {
    const f = raw[y * (stride + 1)], src = raw.subarray(y * (stride + 1) + 1, (y + 1) * (stride + 1)), o = y * stride;
    for (let x = 0; x < stride; x++) {
      const a = x >= bpp ? px[o + x - bpp] : 0, up = y ? px[o - stride + x] : 0, c = x >= bpp && y ? px[o - stride + x - bpp] : 0;
      const pr = (() => { const q = a + up - c, pa = Math.abs(q - a), pb = Math.abs(q - up), pc = Math.abs(q - c); return pa <= pb && pa <= pc ? a : pb <= pc ? up : c; })();
      px[o + x] = (src[x] + [0, a, up, (a + up) >> 1, pr][f]) & 255;
    }
  }
  const data = new Float32Array(w * h);
  for (let i = 0; i < w * h; i++) data[i] = px[i * bpp];
  return { data, width: w, height: h };
}

// Seeded RNG so the run is repeatable.
let seed = 12345;
const rnd = () => ((seed = (seed * 1664525 + 1013904223) >>> 0) / 4294967296);
const gauss = () => Math.sqrt(-2 * Math.log(rnd() + 1e-12)) * Math.cos(2 * Math.PI * rnd());
const U = (a, b) => a + (b - a) * rnd();

// Homography through 4 point pairs (DLT, h33 = 1).
function homography4(src, dst) {
  const A = [], bb = [];
  src.forEach(([x, y], i) => {
    const [u, v] = dst[i];
    A.push([x, y, 1, 0, 0, 0, -u * x, -u * y]); bb.push(u);
    A.push([0, 0, 0, x, y, 1, -v * x, -v * y]); bb.push(v);
  });
  for (let c = 0; c < 8; c++) {
    let p = c; for (let r = c + 1; r < 8; r++) if (Math.abs(A[r][c]) > Math.abs(A[p][c])) p = r;
    [A[c], A[p]] = [A[p], A[c]]; [bb[c], bb[p]] = [bb[p], bb[c]];
    for (let r = c + 1; r < 8; r++) { const f = A[r][c] / A[c][c]; for (let k = c; k < 8; k++) A[r][k] -= f * A[c][k]; bb[r] -= f * bb[c]; }
  }
  const x = new Array(8);
  for (let r = 7; r >= 0; r--) { let s = bb[r]; for (let k = r + 1; k < 8; k++) s -= A[r][k] * x[k]; x[r] = s / A[r][r]; }
  return [...x, 1];
}

// Camera frame like CameraPixelArray on a 720x1280 portrait camera at maxDimension 480; vertical FOV 65°.
const FW = 270, FH = 480, f = (FH / 2) / Math.tan(32.5 * Math.PI / 180);
function randomPose(aspect) {
  const [rx, ry, rz] = [U(-0.7, 0.7), U(-0.7, 0.7), U(-0.45, 0.45)];  // up to 40° tilt, 25° roll
  const [cx, sx, cy, sy, cz, sz] = [Math.cos(rx), Math.sin(rx), Math.cos(ry), Math.sin(ry), Math.cos(rz), Math.sin(rz)];
  const R = mul3(mul3([1, 0, 0, 0, cx, -sx, 0, sx, cx], [cy, 0, sy, 0, 1, 0, -sy, 0, cy]), [cz, -sz, 0, sz, cz, 0, 0, 0, 1]);
  const heightPx = U(110, 260), Z = 2 * f / heightPx;  // card 2 units tall
  const t = [U(-0.25, 0.25) * FW / f * Z, U(-0.2, 0.2) * FH / f * Z, Z];
  const K = [f, 0, FW / 2, 0, f, FH / 2, 0, 0, 1];
  return mul3(K, [R[0], R[1], t[0], R[3], R[4], t[1], R[6], R[7], t[2]]);  // normalized template (u, v, 1) -> px
}

// Frame: the target under G (2x2 supersampled bilinear on a pre-shrunk copy), camera gain/offset, sensor noise,
// optional occluder over part of the card, on a textured background.
function render(T, G, { gain, offset, noise, occlude }) {
  const scale = Math.max(1, T.height / (2 * Math.hypot(...[0, 1].map(i => apply3(G, 0, 1)[i] - apply3(G, 0, -1)[i])) / 2));
  const S = resize(T, Math.max(8, Math.round(T.width / scale)), Math.max(8, Math.round(T.height / scale)));
  const Gi = inv3(G), half = S.height / 2, out = new Float32Array(FW * FH);
  const occ = occlude && (() => { const v0 = U(-1, 0.6); return [-9, v0, 9, v0 + 0.4]; })();  // a band over 20% of the card
  for (let y = 0; y < FH; y++) for (let x = 0; x < FW; x++) {
    let s = 0;
    for (const [dx, dy] of [[0.25, 0.25], [0.75, 0.25], [0.25, 0.75], [0.75, 0.75]]) {
      const [u, v] = apply3(Gi, x + dx, y + dy);
      const px = u * half + S.width / 2 - 0.5, py = v * half + S.height / 2 - 0.5, xi = Math.floor(px), yi = Math.floor(py);
      let val;
      if (xi < 0 || yi < 0 || xi >= S.width - 1 || yi >= S.height - 1) val = 120 + 60 * Math.sin(x * 0.07) * Math.cos(y * 0.05);
      else if (occ && u > occ[0] && u < occ[2] && v > occ[1] && v < occ[3]) val = 200;  // a hand / sticker
      else {
        const fx = px - xi, fy = py - yi, i = yi * S.width + xi, d = S.data;
        val = gain * ((d[i] * (1 - fx) + d[i + 1] * fx) * (1 - fy) + (d[i + S.width] * (1 - fx) + d[i + S.width + 1] * fx) * fy) + offset;
      }
      s += val;
    }
    out[y * FW + x] = Math.min(255, Math.max(0, s / 4 + noise * gauss()));
  }
  return { data: out, width: FW, height: FH };
}

// The tracker's iteration with switches: photometric = affine brightness fit, robust = Huber weights.
function makeIterate(photometric, robust, mad, irls, tukey) {
  const absE = new Float32Array(4096), wPrev = new Float32Array(4096);
  return function (img, scale, pts, G, iterations) {
    const { data, width, height } = img, n = pts.length / 11, I = this.samples, ok = this.inFrame;
    let score = 0, visible = 0;
    wPrev.fill(1);
    for (let it = 0; it < iterations; it++) {
      let cnt = 0, sT = 0, sI = 0, sTT = 0, sTI = 0, sw = 0, wT = 0, wI = 0, wTT = 0, wTI = 0;
      for (let k = 0; k < n; k++) {
        const o = 11 * k, u = pts[o], v = pts[o + 1], z = G[6] * u + G[7] * v + G[8];
        const x = ((G[0] * u + G[1] * v + G[2]) / z) * scale - 0.5, y = ((G[3] * u + G[4] * v + G[5]) / z) * scale - 0.5;
        const xi = Math.floor(x), yi = Math.floor(y);
        if (z <= 0 || xi < 0 || yi < 0 || xi >= width - 1 || yi >= height - 1) { ok[k] = 0; continue; }
        const fx = x - xi, fy = y - yi, i = yi * width + xi;
        I[k] = (data[i] * (1 - fx) + data[i + 1] * fx) * (1 - fy) + (data[i + width] * (1 - fx) + data[i + width + 1] * fx) * fy;
        ok[k] = 1; cnt++;
        const t = pts[o + 2]; sT += t; sI += I[k]; sTT += t * t; sTI += t * I[k];
        const w = wPrev[k]; sw += w; wT += w * t; wI += w * I[k]; wTT += w * t * t; wTI += w * t * I[k];
      }
      if (cnt < 20) return null;
      const mT = sT / cnt, mI = sI / cnt, vT = sTT / cnt - mT * mT, cTI = sTI / cnt - mT * mI;
      if (vT <= 1e-6) return null;
      let a = Math.abs(cTI) > 1e-6 ? cTI / vT : 1, b = mI - a * mT;
      if (irls) { const m1 = wT / sw, m2 = wI / sw, v1 = wTT / sw - m1 * m1, c12 = wTI / sw - m1 * m2; if (v1 > 1e-6 && Math.abs(c12) > 1e-6) { a = c12 / v1; b = m2 - a * m1; } }
      if (!photometric) { a = 1; b = 0; }
      let absSum = 0, sII = 0, m = 0;
      for (let k = 0; k < n; k++) if (ok[k]) { const e = (I[k] - b) / a - pts[11 * k + 2]; absSum += Math.abs(e); absE[m++] = Math.abs(e); sII += (I[k] - mI) ** 2; }
      visible = cnt / n;
      score = cTI / Math.sqrt(vT * (sII / cnt) + 1e-9);
      const huber = mad ? 1.5 * 1.4826 * absE.subarray(0, m).sort()[m >> 1] + 1e-3 : 1.5 * 1.253 * absSum / cnt + 1e-3;
      const H = Array.from({ length: 8 }, () => new Array(8).fill(0)), g = new Array(8).fill(0);
      for (let k = 0; k < n; k++) {
        if (!ok[k]) continue;
        const o = 11 * k, e = (I[k] - b) / a - pts[o + 2], ae = Math.abs(e);
        const c = huber / 1.5 * 4.685, wk = tukey ? (ae < c ? (1 - (ae / c) ** 2) ** 2 : 0) : !robust || ae <= huber ? 1 : huber / ae;
        wPrev[k] = wk;
        for (let r = 0; r < 8; r++) { const sr = pts[o + 3 + r] * wk; g[r] += sr * e; for (let c = r; c < 8; c++) H[r][c] += sr * pts[o + 3 + c]; }
      }
      for (let r = 0; r < 8; r++) for (let c = 0; c < r; c++) H[r][c] = H[c][r];
      const dp = (() => {  // same 8x8 solve as tracker.js
        const M = H.map((row, i) => [...row, g[i]]);
        for (let c = 0; c < 8; c++) {
          let p = c; for (let r = c + 1; r < 8; r++) if (Math.abs(M[r][c]) > Math.abs(M[p][c])) p = r;
          if (Math.abs(M[p][c]) < 1e-12) return null;
          [M[c], M[p]] = [M[p], M[c]];
          for (let r = c + 1; r < 8; r++) { const q = M[r][c] / M[c][c]; for (let k = c; k <= 8; k++) M[r][k] -= q * M[c][k]; }
        }
        const x = new Array(8);
        for (let r = 7; r >= 0; r--) { let s = M[r][8]; for (let k = r + 1; k < 8; k++) s -= M[r][k] * x[k]; x[r] = s / M[r][r]; }
        return x;
      })();
      if (!dp) return null;
      G = mul3(G, inv3([1 + dp[0], dp[2], dp[4], dp[1], 1 + dp[3], dp[5], dp[6], dp[7], 1]));
      if (Math.max(...dp.map(Math.abs)) < 1e-4) break;
    }
    return { G, score, visible };
  };
}

const dir = path.join(project, 'Builds/FreeWebAR/image-targets');
const targets = fs.readdirSync(dir).filter(n => n.endsWith('_luminance.png')).map(n => ({ name: n.replace('_luminance.png', ''), img: readPng(path.join(dir, n)) }));
const variants = {
  full: { photometric: true, robust: true, levels: 3 },
  'sem-fotometria': { photometric: false, robust: true, levels: 3 },
  'sem-Huber': { photometric: true, robust: false, levels: 3 },
  'um-nível': { photometric: true, robust: true, levels: 1 },
  'Huber-MAD': { photometric: true, robust: true, levels: 3, mad: true },
  'IRLS': { photometric: true, robust: true, levels: 3, mad: true, irls: true },
  'Tukey': { photometric: true, robust: true, levels: 3, mad: true, irls: true, tukey: true },
};
const sigmas = [1, 2, 4, 6, 8, 10, 12, 16, 20], TRIALS = Number(process.argv[3] || 40);
const results = [];
for (const scene of (process.argv[4] || 'limpa,fotometria,oclusão').split(',')) {
  for (const sigma of sigmas) {
    const tally = Object.fromEntries(Object.keys(variants).map(v => [v, { ok: 0, conv: 0, acc: 0, n: 0, err: [], ms: [], falseAccept: 0 }]));
    for (let trial = 0; trial < TRIALS; trial++) {
      const tgt = targets[trial % targets.length], aspect = tgt.img.width / tgt.img.height;
      const G = randomPose(aspect);
      const photo = scene === 'limpa' ? { gain: 1, offset: 0 } : { gain: U(0.5, 1.4), offset: U(-40, 40) };
      const frame = pyramid(render(tgt.img, G, { ...photo, noise: 3, occlude: scene === 'oclusão' }));
      const corners = [[-aspect, -1], [aspect, -1], [aspect, 1], [-aspect, 1]];
      const truth = corners.map(([u, v]) => apply3(G, u, v));
      const G0 = homography4(corners, truth.map(([x, y]) => [x + sigma * gauss(), y + sigma * gauss()]));
      for (const [name, v] of Object.entries(variants)) {
        const tr = tgt[name] ||= (() => { const t = new PlanarTracker(tgt.img); t.levels = t.levels.slice(0, v.levels); t.iterate = makeIterate(v.photometric, v.robust, v.mad, v.irls, v.tukey); return t; })();
        const t0 = process.hrtime.bigint();
        const res = tr.track(frame, G0);
        const ms = Number(process.hrtime.bigint() - t0) / 1e6;
        const err = Math.sqrt(corners.reduce((s, [u, w], k) => { const [x, y] = apply3(res.G, u, w); return s + (x - truth[k][0]) ** 2 + (y - truth[k][1]) ** 2; }, 0) / 4);
        const accepted = res.score > 0.7 && res.visible > 0.25, T = tally[name];
        T.n++; T.ms.push(ms);
        if (err < 1) { T.conv++; T.err.push(err); }
        if (accepted) T.acc++;
        if (accepted && err < 1) T.ok++;
        if (accepted && err > 3) T.falseAccept++;
      }
    }
    for (const [name, T] of Object.entries(tally)) {
      const e = T.err.sort((a, b) => a - b), m = T.ms.sort((a, b) => a - b);
      results.push({ scene, sigma, variant: name, rate: T.ok / T.n, conv: T.conv / T.n, acc: T.acc / T.n, medErr: e[Math.floor(e.length / 2)] ?? null, falseAccept: T.falseAccept / T.n, msMed: m[Math.floor(m.length / 2)] });
    }
    process.stderr.write(`${scene} sigma ${sigma}: ` + Object.entries(tally).map(([k, T]) => `${k} ${(100 * T.conv / T.n).toFixed(0)}/${(100 * T.ok / T.n).toFixed(0)}%`).join('  ') + '\n');
  }
}
fs.writeFileSync(path.join(path.dirname(fileURLToPath(import.meta.url)), 'results', 'ablation.json'), JSON.stringify(results, null, 1));
