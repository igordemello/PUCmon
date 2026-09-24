// node "Assets/FreeWebAR/Tests~/tracker.test.mjs"
// PlanarTracker on a synthetic low-texture card (flat colour + one dark blob + a few bars, like the test art):
// render a camera frame through a known homography with noise and a brightness change, start from a wrong guess,
// and check the tracker lands every card corner within 0.2 px.
import assert from 'node:assert/strict';
import { PlanarTracker, pyramid, apply3, mul3 } from '../../WebGLTemplates/FreeWebAR/tracker.js';

// 300x424 card: mid-grey, a dark blob, three dark bars ("text").
const tw = 300, th = 424, tdata = new Uint8Array(tw * th);
for (let y = 0; y < th; y++) for (let x = 0; x < tw; x++) {
  const bx = (x - 160) / 90, by = (y - 250) / 110;
  const blob = bx * bx + by * by + 0.35 * Math.sin(4 * Math.atan2(by, bx)) < 1;
  const bar = y > 50 && y < 70 && ((x > 40 && x < 110) || (x > 130 && x < 180) || (x > 200 && x < 260));
  tdata[y * tw + x] = blob || bar ? 20 : 120;
}
const template = { data: tdata, width: tw, height: th };

let seed = 3;
const rand = () => ((seed = (seed * 16807) % 2147483647) / 2147483647);

// Camera frame 640x480: the card seen through G (normalized template -> frame px), on a textured table.
function render(G, gain = 1.3, bias = 25, noise = 6) {
  const W = 640, H = 480, out = new Float32Array(W * H), Gi = inverse(G);
  for (let y = 0; y < H; y++) for (let x = 0; x < W; x++) {
    const [u, v] = apply3(Gi, x + 0.5, y + 0.5);
    const tx = u * th / 2 + tw / 2 - 0.5, ty = v * th / 2 + th / 2 - 0.5;
    let val = 90 + 30 * Math.sin(x * 0.07) * Math.cos(y * 0.05);  // table
    if (tx >= 0 && ty >= 0 && tx < tw - 1 && ty < th - 1) {
      const xi = Math.floor(tx), yi = Math.floor(ty), fx = tx - xi, fy = ty - yi, i = yi * tw + xi;
      val = (tdata[i] * (1 - fx) + tdata[i + 1] * fx) * (1 - fy) + (tdata[i + tw] * (1 - fx) + tdata[i + tw + 1] * fx) * fy;
      val = gain * val + bias;
    }
    out[y * W + x] = Math.min(255, Math.max(0, val + (rand() - 0.5) * 2 * noise));
  }
  return { data: out, width: W, height: H };
}
function inverse(m) {
  const [a, b, c, d, e, f, g, h, i] = m;
  const A = e * i - f * h, B = -(d * i - f * g), C = d * h - e * g, det = a * A + b * B + c * C;
  return [A, -(b * i - c * h), b * f - c * e, B, a * i - c * g, -(a * f - c * d), C, -(a * h - b * g), a * e - b * d].map(v => v / det);
}
const corners = G => [[-tw / th, -1], [tw / th, -1], [tw / th, 1], [-tw / th, 1]].map(([u, v]) => apply3(G, u, v));
const cornerError = (A, B) => Math.max(...corners(A).map((p, i) => Math.hypot(p[0] - corners(B)[i][0], p[1] - corners(B)[i][1])));

const tracker = new PlanarTracker(template);
// True pose: card ~300 px tall, tilted (perspective terms), rotated.
const truth = [110 * Math.cos(0.3), -130 * Math.sin(0.3), 330, 110 * Math.sin(0.3), 130 * Math.cos(0.3), 240, 0.12, -0.08, 1];
const frame = pyramid(render(truth));
// Starting guess ~20 px and a few degrees off (what the detector or the previous frame hands over).
const guess = mul3(truth, [1.04, 0.05, 0.12, -0.04, 0.97, -0.1, 0.03, 0.02, 1]);
assert.ok(cornerError(guess, truth) > 15, 'guess must start far off');
const t0 = performance.now();
const { G, score, visible } = tracker.track(frame, guess);
const ms = performance.now() - t0;
const err = cornerError(G, truth);
assert.ok(err < 0.2, `corner error ${err.toFixed(3)} px`);
assert.ok(score > 0.95 && visible > 0.99, `score ${score}, visible ${visible}`);

// A frame without the card must not look like a match.
const empty = pyramid(render([1, 0, 5000, 0, 1, 5000, 0, 0, 1]));
assert.ok(tracker.track(empty, truth).score < 0.5, 'no card, no match');

console.log(`tracker.test: ok (corner error ${err.toFixed(3)} px, ZNCC ${score.toFixed(3)}, ${ms.toFixed(1)} ms)`);
