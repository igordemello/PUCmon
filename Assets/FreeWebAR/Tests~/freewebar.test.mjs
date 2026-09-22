// node "Assets/FreeWebAR/Tests~/freewebar.test.mjs"
// Pose: projects target points through the Unity pose + FOV and checks they land on the same screen pixel
// as MindAR's own pinhole model (K * [R|t] on marker pixels), for several screen crops.
import assert from 'node:assert/strict';
import { toUnityPose, displayFovY, PoseFilter, mainBackCamera } from '../../WebGLTemplates/FreeWebAR/freewebar.js';

const W = 480, H = 640;                                  // tracker input
const f = (H / 2) / Math.tan(Math.PI / 8);               // MindAR Controller: fovy = 45°
const markerW = 1000, markerH = 1415;

const mul = (a, b) => a.map(r => b[0].map((_, j) => r.reduce((s, v, k) => s + v * b[k][j], 0)));
const rx = a => [[1, 0, 0], [0, Math.cos(a), -Math.sin(a)], [0, Math.sin(a), Math.cos(a)]];
const ry = a => [[Math.cos(a), 0, Math.sin(a)], [0, 1, 0], [-Math.sin(a), 0, Math.cos(a)]];
const rz = a => [[Math.cos(a), -Math.sin(a), 0], [Math.sin(a), Math.cos(a), 0], [0, 0, 1]];
const cross = (a, b) => [a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0]];

// modelViewTransform: marker pixels (x right, y down, z into the image) -> camera (x right, y down, z forward).
const R = mul(mul(rz(0.2), ry(-0.5)), rx(0.35)), t = [-150, 90, 2600];
const mv = R.map((row, i) => [...row, t[i]]);

// Copy of MindAR Controller._glModelViewMatrix.
const h = markerH, m = mv;
const world = [
  m[0][0], -m[1][0], -m[2][0], 0,
  -m[0][1], m[1][1], m[2][1], 0,
  -m[0][2], m[1][2], m[2][2], 0,
  m[0][1] * h + m[0][3], -(m[1][1] * h + m[1][3]), -(m[2][1] * h + m[2][3]), 1,
];

const u = toUnityPose(world, markerW, markerH);
const pos = u.slice(0, 3), fwd = u.slice(3, 6), up = u.slice(6, 9), right = cross(up, fwd);
const a = markerW / markerH;

for (const [vw, vh] of [[W, H], [390, 844], [800, 600]]) {             // same, taller and wider screens
  const cover = Math.max(vw / W, vh / H);
  const tanHalf = Math.tan(displayFovY(Math.tan(Math.PI / 8), W, H, vw, vh) * Math.PI / 360);
  for (const [X, Y, Z] of [[0, 0, 0], [-a, 1, 0], [a, -1, 0], [0.3, 0.2, -0.5]]) {  // centre, corners, above the card
    // Unity: target point -> camera space -> NDC.
    const c = [0, 1, 2].map(i => pos[i] + X * right[i] + Y * up[i] + Z * fwd[i]);
    assert.ok(c[2] > 0, 'target must be in front of the Unity camera');
    const ndc = [c[0] / c[2] / (tanHalf * vw / vh), c[1] / c[2] / tanHalf];
    // MindAR ground truth: the same point in marker pixels -> video pixel -> cover-fitted screen -> NDC.
    const p = [markerW / 2 + X * markerH / 2, markerH / 2 - Y * markerH / 2, Z * markerH / 2];
    const q = [0, 1, 2].map(i => R[i][0] * p[0] + R[i][1] * p[1] + R[i][2] * p[2] + t[i]);
    const px = f * q[0] / q[2] + W / 2, py = f * q[1] / q[2] + H / 2;
    const expected = [(px - W / 2) * cover / (vw / 2), -(py - H / 2) * cover / (vh / 2)];
    for (let i = 0; i < 2; i++)
      assert.ok(Math.abs(ndc[i] - expected[i]) < 1e-9, `view ${vw}x${vh} point ${[X, Y, Z]}: ${ndc} != ${expected}`);
  }
}
// PoseFilter: much less shake while still, and keeps up with a move (30 Hz updates, deterministic noise).
let seed = 1;
const rand = () => ((seed = (seed * 16807) % 2147483647) / 2147483647 - 0.5);
const noisy = (x, n) => {
  const tilt = 0.03 * rand();  // ~1.7° of tilt noise, like a low-texture target
  return [x + n * rand(), n * rand(), 5 + 5 * n * rand(), Math.sin(tilt), 0, Math.cos(tilt), 0, 1, 0];
};
const rms = a => Math.sqrt(a.reduce((s, v) => s + v * v, 0) / a.length);
{
  const filter = new PoseFilter(), raw = [], out = [];  // the tilt (forward) channel is the heavily smoothed one
  for (let i = 0; i < 300; i++) {
    const r = noisy(0, 0.01);
    raw.push(r[3]); out.push(filter.filter(i / 30, r)[3]);
  }
  assert.ok(rms(out.slice(30)) < rms(raw) / 2, `still: tilt shake ${rms(out.slice(30))} vs raw ${rms(raw)}`);
}
{
  const filter = new PoseFilter();
  let last;
  for (let i = 0; i < 60; i++) last = filter.filter(i / 30, noisy(i / 30 * 2.5, 0));  // 2.5 units/s sideways (1.25 target heights/s)
  assert.ok(Math.abs(last[0] - 59 / 30 * 2.5) < 0.1, `moving: lags ${59 / 30 * 2.5 - last[0]} units behind`);
}
// Camera choice: the main back lens, never the 0.5x ultra-wide or a virtual multi-lens camera.
const pick = labels => mainBackCamera(labels.map(label => ({ label })))?.label;
assert.equal(pick(['camera2 1, facing front', 'camera2 2, facing back', 'camera2 0, facing back', 'camera2 3, facing back']),
  'camera2 0, facing back');                                                                // Android Chrome
assert.equal(pick(['Camera 1, Facing front, Orientation 270', 'Camera 0, Facing back, Orientation 90']),
  'Camera 0, Facing back, Orientation 90');                                                 // Android Firefox
assert.equal(pick(['Front Camera', 'Back Triple Camera', 'Back Dual Wide Camera', 'Back Ultra Wide Camera',
  'Back Telephoto Camera', 'Back Camera']), 'Back Camera');                                 // iPhone Pro, English
assert.equal(pick(['Câmera Frontal', 'Câmera Traseira Tripla', 'Câmera Traseira Dupla Grande-Angular',
  'Câmera Traseira Ultra-Angular', 'Câmera Traseira']), 'Câmera Traseira');                // iPhone Pro, Portuguese
assert.equal(pick(['HD Webcam (1bcf:28c4)']), undefined);                                  // PC: keep the browser's pick
console.log('freewebar.test: ok');
