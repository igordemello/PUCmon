mergeInto(LibraryManager.library, {
	LoadScene: function (scene, index, targetImg) {
		let s = UTF8ToString(scene);
		let i = UTF8ToString(index);
		let t = UTF8ToString(targetImg);
		window.SCENE_LOADER.LoadScene(s, i, t);
	},
	
	CaptureTarget_cmd: function(index, controller,){
		let i = UTF8ToString(index);
		let c = UTF8ToString(controller);
		window.SCENE_LOADER.ScanSurface(i, c);
	}
});