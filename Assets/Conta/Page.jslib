mergeInto(LibraryManager.library, {
  // Hides the page's loading screen (see pucmonPageReady in the FreeWebAR WebGL template).
  PUCmon_PageReady: function () {
    if (window.pucmonPageReady) window.pucmonPageReady();
  }
});
