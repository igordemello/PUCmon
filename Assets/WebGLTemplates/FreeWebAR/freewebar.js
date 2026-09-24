// Rotate vector v by unit quaternion q ({x, y, z, w}): q * v * q^-1.
function rotate(q, [x, y, z]) {
  const ix = q.w * x + q.y * z - q.z * y, iy = q.w * y + q.z * x - q.x * z;
  const iz = q.w * z + q.x * y - q.y * x, iw = -q.x * x - q.y * y - q.z * z;
  return [
    ix * q.w - iw * q.x - iy * q.z + iz * q.y,
    iy * q.w - iw * q.y - iz * q.x + ix * q.z,
    iz * q.w - iw * q.z - ix * q.y + iy * q.x,
  ];
}

// Phones with several rear lenses: facingMode 'environment' may open the 0.5x ultra-wide, or iPhone Pro's virtual
// Dual/Triple camera that jumps to it up close. The engine's iOS fix looks for the English label "Back Camera" only
// (a Portuguese iPhone says "Câmera Traseira"), then falls back to the 2nd device. Pick the main lens from the
// enumerateDevices() labels instead; undefined = nothing to choose, keep the browser's pick.
export function mainBackCamera(cams) {
  // Android: "camera2 0, facing back" is the main sensor; higher numbers are ultra-wide / tele / depth.
  const android = cams.map(c => [c, /(\d+),\s*facing back/i.exec(c.label)]).filter(([, m]) => m)
    .sort((a, b) => a[1][1] - b[1][1]);
  if (android.length) return android.length > 1 ? android[0][0] : undefined;
  // iOS, any language: all rear lens labels share a word the front camera's lacks ("Back", "Traseira", "Trasera"...),
  // the most common word that is not in every label. The main lens is the one with no extra qualifiers: fewest words.
  if (cams.length < 3) return undefined;  // front + one back: the browser already opens the right one
  const words = cams.map(c => c.label.split(/\s+/).filter(Boolean));
  const count = new Map();
  for (const w of words.flatMap(ws => [...new Set(ws)])) count.set(w, (count.get(w) || 0) + 1);
  const rear = [...count].filter(([, n]) => n < cams.length).sort((a, b) => b[1] - a[1])[0]?.[0];
  let best;
  cams.forEach((c, i) => { if (words[i].includes(rear) && (!best || words[i].length < best.n)) best = { c, n: words[i].length }; });
  return best?.c;
}

// Make every video getUserMedia request open this device (the engine asks by facingMode, and re-asks on iOS).
export function pinCamera(mediaDevices, deviceId) {
  const open = mediaDevices.getUserMedia.bind(mediaDevices);
  mediaDevices.getUserMedia = constraints => open(constraints?.video
    ? { ...constraints, video: { ...(constraints.video === true ? {} : constraints.video), deviceId: { exact: deviceId } } }
    : constraints);
}

// ---- Dense tracking bridge -------------------------------------------------------------------------------------
// Frame camera: OpenCV convention (x right, y down, z forward, pixels). Card plane coordinates: X right, Y down, in
// card units (full image 2 units tall, origin at its centre), Z into the card.

// Pixel intrinsics of the CPU camera frame (fw x fh, uncropped) from the engine's GL projection for the canvas
// (cw x ch), which shows the frame scaled to cover the canvas.
export function frameIntrinsics(P, cw, ch, fw, fh) {
  const s = Math.max(cw / fw, ch / fh);  // canvas px per frame px
  return { fx: P[0] * cw / (2 * s), fy: P[5] * ch / (2 * s), cx: fw / 2 - P[8] * cw / (2 * s), cy: fh / 2 + P[9] * ch / (2 * s) };
}

// Normalized coordinates of the tracked template (the image-target-cli luminance crop: x~ = (x - w/2) / (h/2))
// -> card plane coordinates, as a 3x3 affine (row-major).
export function templateToCard(c) {
  const [W, H] = c.isRotated ? [c.originalHeight, c.originalWidth] : [c.originalWidth, c.originalHeight];
  const k = c.height / H;
  if (!c.isRotated)
    return [k, 0, (c.left + c.width / 2 - W / 2) * 2 / H, 0, k, (c.top + c.height / 2 - H / 2) * 2 / H, 0, 0, 1];
  // Crop taken from the image turned 90° clockwise: turned (x, y) = original (H - y, x).
  return [0, k, (c.top + c.height / 2 - W / 2) * 2 / H, -k, 0, (H / 2 - c.left - c.width / 2) * 2 / H, 0, 0, 1];
}

const flipY = ([x, y, z]) => [x, -y, z];  // Unity camera space <-> OpenCV camera space
const cross = (a, b) => [a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0]];

// Unity pose [position, forward, up] -> homography G from normalized template coordinates to frame pixels.
export function poseToHomography(pose, K, A) {
  const p = flipY(pose.slice(0, 3)), f = pose.slice(3, 6), u = pose.slice(6, 9);
  const r = flipY(cross(u, f)), d = flipY(u).map(v => -v);  // card X (right) and Y (down) axes, OpenCV space
  const Kc = [K.fx, 0, K.cx, 0, K.fy, K.cy, 0, 0, 1];
  const RT = [r[0], d[0], p[0], r[1], d[1], p[1], r[2], d[2], p[2]];
  return mul3(mul3(Kc, RT), A);
}

// Homography G (normalized template -> frame px) -> Unity pose [position, forward, up]. Unique for a plane with
// known intrinsics: G = K [r1 r2 t] A up to scale.
export function homographyToPose(G, K, A) {
  const Ki = [1 / K.fx, 0, -K.cx / K.fx, 0, 1 / K.fy, -K.cy / K.fy, 0, 0, 1];
  const M = mul3(mul3(Ki, G), inv3(A));
  let c1 = [M[0], M[3], M[6]], c2 = [M[1], M[4], M[7]], t = [M[2], M[5], M[8]];
  let s = 2 / (Math.hypot(...c1) + Math.hypot(...c2));
  if (t[2] < 0) s = -s;  // the card is in front of the camera
  [c1, c2, t] = [c1, c2, t].map(v => v.map(x => x * s));
  // Nearest rotation to [c1 c2 c1xc2] (polar decomposition by averaging with the inverse transpose).
  let R = [c1, c2, cross(c1, c2)];
  for (let i = 0; i < 8; i++) {
    const [a, b, c] = R, det = a[0] * (b[1] * c[2] - b[2] * c[1]) - a[1] * (b[0] * c[2] - b[2] * c[0]) + a[2] * (b[0] * c[1] - b[1] * c[0]);
    const cof = [cross(b, c), cross(c, a), cross(a, b)];  // rows of det * inverse transpose, column vectors here
    R = R.map((v, j) => v.map((x, k) => 0.5 * (x + cof[j][k] / det)));
  }
  const [r1, r2, r3] = R;
  return [...flipY(t), ...flipY(r3), ...flipY(r2).map(v => -v)];
}

function mul3(a, b) {
  return [0, 1, 2].flatMap(r => [0, 1, 2].map(c => a[3 * r] * b[c] + a[3 * r + 1] * b[3 + c] + a[3 * r + 2] * b[6 + c]));
}

function inv3(m) {
  const [a, b, c, d, e, f, g, h, i] = m;
  const A = e * i - f * h, B = -(d * i - f * g), C = d * h - e * g, det = a * A + b * B + c * C;
  return [A, -(b * i - c * h), b * f - c * e, B, a * i - c * g, -(a * f - c * d), C, -(a * h - b * g), a * e - b * d].map(v => v / det);
}

// 8th Wall image target -> Unity camera-space pose of a Zappar-style card, returned as [position, forward, up].
// 8th Wall (configured with leftHandedAxes, like Unity) reports the camera and the image in its world: the image is
// the 3:4 crop made by image-target-cli, in the original image's orientation, facing the camera along -Z like
// Zappar's, with world units = `scale` / crop height in px. The card is the full original image, centred on the
// origin and 2 units tall.
export function toCardPose(camera, image) {
  const c = image.properties, toCam = { x: -camera.rotation.x, y: -camera.rotation.y, z: -camera.rotation.z, w: camera.rotation.w };
  const local = v => rotate(toCam, rotate(image.rotation, v));  // card axes in camera space
  // Landscape images are cropped after a 90° clockwise turn: crop numbers are in the turned image's pixels.
  const [W, H] = c.isRotated ? [c.originalHeight, c.originalWidth] : [c.originalWidth, c.originalHeight];
  const [cx, cy] = c.isRotated ? [c.top + c.height / 2, H - c.left - c.width / 2] : [c.left + c.width / 2, c.top + c.height / 2];
  const units = 2 * c.height / (H * image.scale);  // world -> card units
  const rel = rotate(toCam, [image.position.x - camera.position.x, image.position.y - camera.position.y, image.position.z - camera.position.z]);
  const toCentre = local([(W / 2 - cx) * 2 / H, (cy - H / 2) * 2 / H, 0]);  // crop centre -> image centre (x right, y up)
  return [...rel.map((v, i) => v * units + toCentre[i]), ...local([0, 0, 1]), ...local([0, 1, 0])];
}
