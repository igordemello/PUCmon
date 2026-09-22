// MindAR world matrix (three.js: column-major, marker pixels, origin at the image's bottom-left, +Z to the viewer)
// -> Unity camera-space pose of a Zappar-style target: origin at the image centre, 2 units tall, -Z to the viewer.
// Unity is left-handed (camera looks down +Z), so both sides are mirrored in Z: Unity = S * three * S, S = diag(1, 1, -1).
// Returns [position xyz, forward xyz, up xyz].
export function toUnityPose(m, width, height) {
  const s = 2 / height, cx = width / 2, cy = height / 2;
  return [
    s * (m[0] * cx + m[4] * cy + m[12]), s * (m[1] * cx + m[5] * cy + m[13]), -s * (m[2] * cx + m[6] * cy + m[14]),
    -m[8], -m[9], m[10],
    m[4], m[5], -m[6],
  ];
}

// One Euro filter (Casiez et al. 2012) on a pose from toUnityPose, in physical units. MindAR's own filter runs per
// matrix element with millisecond units, so the jitter itself raises its cutoff and it barely smooths anything.
// Three channels, each with its own speed (distance-relative 1/s for position, rad/s for the vectors):
//  - position and up (in-plane roll) are measured precisely: light smoothing, so the overlay stays glued in motion;
//  - forward, the card's normal, carries the tilt, the noisiest part of a planar pose: heavy smoothing.
//    (Unity's LookRotation keeps forward exact and only uses up's component across it, so tilt noise in up drops out.)
// For each: min cutoff (Hz) = smoothing at rest, beta = how fast the cutoff opens with speed (less lag).
// Defaults measured with a synthetic camera (noise + hand tremor + motion), screen-space error vs the true pose.
export const FILTER_DEFAULTS = { pos: [4, 200], tilt: [0.5, 3], roll: [4, 50] };

export class PoseFilter {
  constructor({ pos, tilt, roll } = FILTER_DEFAULTS, dCutoff = 1) {
    this.channels = [pos, tilt, roll].map(([minCutoff, beta], i) => ({ i: i * 3, minCutoff, beta, speed: 0 }));
    this.dCutoff = dCutoff;
    this.reset();
  }

  reset() {
    this.pose = null;
  }

  filter(t, raw) {
    if (!this.pose) {
      this.pose = raw.slice();
      this.t = t;
      for (const c of this.channels) c.speed = 0;
      return this.pose;
    }
    const dt = Math.max(t - this.t, 1e-3), p = this.pose, out = p.slice();
    const alpha = cutoff => { const r = 2 * Math.PI * cutoff * dt; return r / (r + 1); };
    const lerp = (a, b, k) => a + (b - a) * k;
    for (const c of this.channels) {
      const [x, y, z] = [c.i, c.i + 1, c.i + 2];
      const change = c.i === 0
        ? Math.hypot(raw[x] - p[x], raw[y] - p[y], raw[z] - p[z]) / Math.hypot(p[x], p[y], p[z])
        : Math.acos(Math.min(1, raw[x] * p[x] + raw[y] * p[y] + raw[z] * p[z]));
      c.speed = lerp(c.speed, change / dt, alpha(this.dCutoff));
      const k = alpha(c.minCutoff + c.beta * c.speed);
      for (const j of [x, y, z]) out[j] = lerp(p[j], raw[j], k);
    }
    for (const i of [3, 6]) {  // keep forward/up unit length
      const n = Math.hypot(out[i], out[i + 1], out[i + 2]);
      out[i] /= n; out[i + 1] /= n; out[i + 2] /= n;
    }
    Object.assign(this, { pose: out, t });
    return out;
  }
}

// facingMode 'environment' may open the 0.5x ultra-wide (Android) or a virtual Dual/Triple camera that switches to
// it up close (iPhone Pro): distorted and out of focus near the card. Pick the main back lens from the labels of
// enumerateDevices() instead (readable once camera permission is granted). undefined = keep what the browser chose.
export function mainBackCamera(cams) {
  // Android: "camera2 0, facing back" is the main sensor; higher numbers are ultra-wide / tele / depth.
  const android = cams.map(c => [c, /(\d+),\s*facing back/i.exec(c.label)]).filter(([, m]) => m)
    .sort((a, b) => a[1][1] - b[1][1])[0];
  if (android) return android[0];
  // iOS (labels follow the phone's language): the plain back camera has no lens qualifier.
  return cams.find(c => /back|rear|traseira|trasera/i.test(c.label) &&
    !/ultra|wide|angular|tele|dual|dupla|triple|tripla|macro/i.test(c.label));
}

// Vertical FOV (degrees) of the part of the camera image visible on screen, given the tracker's tan(fovY / 2)
// for the full video frame and the <video>'s object-fit: cover crop.
export function displayFovY(tanHalfFovY, videoWidth, videoHeight, viewWidth, viewHeight) {
  const cover = Math.max(viewWidth / videoWidth, viewHeight / videoHeight);
  return 2 * Math.atan(tanHalfFovY * viewHeight / (videoHeight * cover)) * 180 / Math.PI;
}
