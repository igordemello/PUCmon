mergeInto(LibraryManager.library, {
  // Copies window.freeWebARPose (27 floats, written by the FreeWebAR WebGL template) into a C# float[27].
  FreeWebAR_ReadPose: function (ptr) {
    var pose = window.freeWebARPose;
    if (!pose) return 0;
    HEAPF32.set(pose, ptr >> 2);
    return 1;
  },

  // Name of the page's image target number `index` (the image's asset name), as a C# string.
  FreeWebAR_TargetName: function (index) {
    var name = (window.freeWebARTargets || [])[index] || '';
    var size = lengthBytesUTF8(name) + 1, buffer = _malloc(size);
    stringToUTF8(name, buffer, size);
    return buffer;
  },

  // Unity ends each frame with an alpha-only clear to 1. Skipping it keeps the pixels the camera
  // cleared to Color.clear transparent, so the camera feed canvas behind this one shows through.
  glClear: function (mask) {
    if (mask === 0x4000) {
      var m = GLctx.getParameter(GLctx.COLOR_WRITEMASK);
      if (!m[0] && !m[1] && !m[2] && m[3]) return;
    }
    GLctx.clear(mask);
  }
});
