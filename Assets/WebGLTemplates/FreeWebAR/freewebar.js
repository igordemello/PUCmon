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
