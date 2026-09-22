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

// Vertical FOV (degrees) of the part of the camera image visible on screen, given the tracker's tan(fovY / 2)
// for the full video frame and the <video>'s object-fit: cover crop.
export function displayFovY(tanHalfFovY, videoWidth, videoHeight, viewWidth, viewHeight) {
  const cover = Math.max(viewWidth / videoWidth, viewHeight / videoHeight);
  return 2 * Math.atan(tanHalfFovY * viewHeight / (videoHeight * cover)) * 180 / Math.PI;
}
