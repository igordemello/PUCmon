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
