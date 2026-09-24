// node "Assets/FreeWebAR/Tests~/freewebar.test.mjs"
// toCardPose: 8th Wall image target -> Unity camera space, Zappar-style card (full image 2 units tall, centred).
import assert from 'node:assert/strict';
import {
  toCardPose, mainBackCamera, pinCamera, frameIntrinsics, templateToCard, poseToHomography, homographyToPose,
} from '../../WebGLTemplates/FreeWebAR/freewebar.js';

const near = (got, want, what) => got.forEach((v, i) => assert.ok(Math.abs(v - want[i]) < 1e-9, `${what}: ${got} != ${want}`));
const identity = { x: 0, y: 0, z: 0, w: 1 };
const camera = { position: { x: 0, y: 2, z: 0 }, rotation: identity };  // the engine's default camera
const crop = (left, top, width, height, originalWidth, originalHeight, isRotated = false) =>
  ({ left, top, width, height, originalWidth, originalHeight, isRotated });

// Uncropped 480x640 image, 0.5 world units tall, 3 in front of the camera: card is 2 units tall -> x4.
near(toCardPose(camera, { position: { x: 0, y: 2, z: 3 }, rotation: identity, scale: 0.5, properties: crop(0, 0, 480, 640, 480, 640) }),
  [0, 0, 12, 0, 0, 1, 0, 1, 0], 'facing the camera');

// The crop is the top half of a 480x1280 image, so the card centre is below the crop centre.
// Crop 640 px = 1 world unit; card = 1280 px = 2 units -> world units map 1:1, centre 320 px = 0.5 units lower.
near(toCardPose(camera, { position: { x: 0, y: 2, z: 3 }, rotation: identity, scale: 1, properties: crop(0, 0, 480, 640, 480, 1280) }),
  [0, -0.5, 3, 0, 0, 1, 0, 1, 0], 'offset crop');

// Camera turned 90° about Y with the image straight ahead of it and facing it: same result in camera space.
const turn = { x: 0, y: Math.SQRT1_2, z: 0, w: Math.SQRT1_2 };
near(toCardPose({ position: { x: 1, y: 2, z: 0 }, rotation: turn },
  { position: { x: 4, y: 2, z: 0 }, rotation: turn, scale: 0.5, properties: crop(0, 0, 480, 640, 480, 640) }),
  [0, 0, 12, 0, 0, 1, 0, 1, 0], 'turned camera');

// Landscape 1280x960 image: the CLI turns it 90° clockwise (960x1280) and crops there. A 480x640 crop at the
// turned image's top-left is the original's bottom-left 640x480 block: card centre is right of and above it.
// Crop height (original orientation) 480 px = 0.75 world units at scale 1 -> card units 2/960 per px.
const pose = toCardPose(camera, { position: { x: 0, y: 2, z: 3 }, rotation: identity, scale: 1, properties: crop(0, 0, 480, 640, 960, 1280, true) });
const units = 2 * 640 / 960;
near(pose, [(640 - 320) * 2 / 960, (720 - 480) * 2 / 960, 3 * units, 0, 0, 1, 0, 1, 0], 'landscape crop');

// Camera choice: the main rear lens, never the 0.5x ultra-wide or a virtual multi-lens camera.
const pick = labels => mainBackCamera(labels.map(label => ({ label })))?.label;
assert.equal(pick(['Front Camera', 'Back Camera', 'Back Dual Wide Camera', 'Back Ultra Wide Camera',
  'Back Telephoto Camera', 'Back Triple Camera', 'Back Dual Camera']), 'Back Camera');                  // iPhone Pro, EN
assert.equal(pick(['Câmera Frontal', 'Câmera Traseira Tripla', 'Câmera Traseira Dupla Grande-angular',
  'Câmera Traseira Ultra-angular', 'Câmera Traseira', 'Câmera Traseira Teleobjetiva']), 'Câmera Traseira'); // iPhone Pro, PT
assert.equal(pick(['Cámara frontal', 'Cámara trasera doble', 'Cámara trasera']), 'Cámara trasera');     // iPhone, ES
assert.equal(pick(['camera2 1, facing front', 'camera2 2, facing back', 'camera2 0, facing back']),
  'camera2 0, facing back');                                                                              // Android
assert.equal(pick(['camera2 1, facing front', 'camera2 0, facing back']), undefined);                   // one rear lens
assert.equal(pick(['Front Camera', 'Back Camera']), undefined);
assert.equal(pick(['', '', '']), undefined);                                                             // no permission yet

// pinCamera: the engine's facingMode requests (and iOS re-open) get the chosen deviceId.
const seen = [];
const md = { getUserMedia: async c => seen.push(c) };
pinCamera(md, 'main');
await md.getUserMedia({ video: { facingMode: ['environment'], width: { min: 1280 }, deviceId: 'ultra' } });
await md.getUserMedia({ video: true });
assert.deepEqual(seen, [
  { video: { facingMode: ['environment'], width: { min: 1280 }, deviceId: { exact: 'main' } } },
  { video: { deviceId: { exact: 'main' } } },
]);
// Dense-tracking bridge. A tilted card pose (Unity camera space) and a portrait camera frame.
{
  const rot = (ax, ay, az) => {  // Unity pose axes from Euler angles
    const [cx, sx, cy, sy, cz, sz] = [Math.cos(ax), Math.sin(ax), Math.cos(ay), Math.sin(ay), Math.cos(az), Math.sin(az)];
    const R = [[cy * cz + sy * sx * sz, -cy * sz + sy * sx * cz, sy * cx], [cx * sz, cx * cz, -sx], [-sy * cz + cy * sx * sz, sy * sz + cy * sx * cz, cy * cx]];
    return { fwd: [R[0][2], R[1][2], R[2][2]], up: [R[0][1], R[1][1], R[2][1]], right: [R[0][0], R[1][0], R[2][0]] };
  };
  const { fwd, up, right } = rot(0.5, -0.3, 0.2);
  const pose = [0.4, -0.3, 5.5, ...fwd, ...up];
  // Frame intrinsics from a 60°-vertical GL projection shown on a 390x844 canvas, frame 360x480 (cover crop).
  const t = 1 / Math.tan(Math.PI / 6), P = [t * 844 / 390, 0, 0, 0, 0, t, 0, 0, 0, 0, -1, -1, 0, 0, -0.1, 0];
  const K = frameIntrinsics(P, 390, 844, 360, 480);
  const s = Math.max(390 / 360, 844 / 480);
  near([K.fx, K.fy, K.cx, K.cy], [t * 844 / 2 / s, t * 844 / 2 / s, 180, 240], 'frame intrinsics');
  const props = crop(0, 101, 2480, 3307, 2480, 3508), A = templateToCard(props);
  const G = poseToHomography(pose, K, A);
  near(homographyToPose(G, K, A), pose, 'pose -> homography -> pose');
  // G sends the luminance template's corners where the pose + camera project the card's crop corners.
  const aspect = 2480 / 3307;
  for (const [u, v] of [[-aspect, -1], [aspect, -1], [aspect, 1], [-aspect, 1]]) {
    const [gx, gy, gw] = [0, 1, 2].map(r => G[3 * r] * u + G[3 * r + 1] * v + G[3 * r + 2]);
    const X = (u * 3307 / 2 + 1240 - 1240) * 2 / 3508, Y = (v * 3307 / 2 + 101 + 3307 / 2 - 1754) * 2 / 3508;  // card units, Y down
    const c = [0, 1, 2].map(i => pose[i] + X * right[i] - Y * up[i]);  // Unity camera space
    const px = [K.fx * c[0] / c[2] + K.cx, -K.fy * c[1] / c[2] + K.cy];
    near([gx / gw, gy / gw], px, `template corner ${u},${v}`);
  }
  // Landscape target: the template is the crop of the image turned clockwise; its top-left corner is the
  // original's bottom-left region. 1280x960 original, crop = whole turned image (960x1280).
  const Ar = templateToCard(crop(0, 0, 960, 1280, 960, 1280, true));
  const tl = [Ar[0] * -0.75 + Ar[1] * -1 + Ar[2], Ar[3] * -0.75 + Ar[4] * -1 + Ar[5]];
  near(tl, [-1280 / 960, 1], 'turned template top-left = card bottom-left');
}
console.log('freewebar.test: ok');
