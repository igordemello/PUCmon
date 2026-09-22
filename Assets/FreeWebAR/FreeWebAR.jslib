mergeInto(LibraryManager.library, {
  // Copies window.freeWebARPose (written by the FreeWebAR WebGL template) into a C# float[11].
  FreeWebAR_ReadPose: function (ptr) {
    var pose = window.freeWebARPose;
    if (!pose) return 0;
    HEAPF32.set(pose, ptr >> 2);
    return 1;
  },

  // Unity ends each frame with an alpha-only clear to 1. Skipping it keeps the pixels the camera
  // cleared to Color.clear transparent, so the camera <video> behind the canvas shows through.
  glClear: function (mask) {
    if (mask === 0x4000) {
      var m = GLctx.getParameter(GLctx.COLOR_WRITEMASK);
      if (!m[0] && !m[1] && !m[2] && m[3]) return;
    }
    GLctx.clear(mask);
  }
});
