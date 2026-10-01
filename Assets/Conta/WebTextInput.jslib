mergeInto(LibraryManager.library, {
  // Puts a real <input> over a Unity text field while it is being edited, styled like it: the phone's own keyboard,
  // autofill, selection and IME. `geometry` = 7 floats (see PUCmon_PlaceText). Returns an id for the calls below.
  PUCmon_EditText__deps: ['PUCmon_PlaceText'],
  PUCmon_EditText: function (text, type, autocomplete, maxLength, geometry, color, background) {
    var s = window.pucmonText;
    if (!s) {
      s = window.pucmonText = { id: 0 };
      // Ends the current edit: 2 = done, 3 = done with Enter. Removing the input also closes the keyboard.
      s.close = function (state) {
        var input = s.input;
        if (!input) return;
        s.input = null;
        s.state = state;
        input.remove();
      };
      // Unity handles a touch or click synchronously, inside the event, and may start a new edit right there.
      // Before that (capture phase): a touch or click anywhere but on the input ends the current edit.
      // (Not on resize: opening the keyboard resizes the page on some phones.)
      var end = function (e) { if (e.target !== s.input) s.close(2); };
      window.addEventListener('touchstart', end, true);
      window.addEventListener('mousedown', end, true);
      // After it: if that event started an edit, keep the browser from moving the focus back to the game.
      var keep = function (e) { if (s.fresh) e.preventDefault(); };
      window.addEventListener('touchstart', keep, { passive: false });
      window.addEventListener('mousedown', keep);
    }
    s.close(2);
    var input = document.createElement('input');
    input.type = UTF8ToString(type);
    input.value = UTF8ToString(text);
    input.autocomplete = UTF8ToString(autocomplete);
    input.autocapitalize = 'off';
    input.setAttribute('autocorrect', 'off');  // Safari
    input.spellcheck = false;
    if (maxLength > 0) input.maxLength = maxLength;
    input.style.cssText = 'position:fixed; z-index:4; box-sizing:border-box; margin:0; border:0; outline:0;' +
      'pointer-events:none; font-family:Arial,"Liberation Sans",sans-serif;' +
      'color:' + UTF8ToString(color) + '; background:' + UTF8ToString(background) + '; caret-color:' + UTF8ToString(color);
    document.body.appendChild(input);

    var id = ++s.id;
    s.fresh = true;
    setTimeout(function () { s.fresh = false; }, 0);
    s.input = input; s.text = input.value; s.changed = false; s.state = 1;  // 1 editing, 2 done, 3 submitted (Enter)
    _PUCmon_PlaceText(id, geometry);
    input.addEventListener('input', function () { if (s.id === id) { s.text = input.value; s.changed = true; } });
    input.addEventListener('keydown', function (e) { if (e.key === 'Enter' && s.input === input) s.close(3); });
    input.addEventListener('blur', function () { if (s.input === input) s.close(2); });
    // Called from inside the touch, so phones (iOS too) open the keyboard. When the finger lifts, let the input take
    // taps (moving the caret) and swallow that touch's emulated mouse events.
    input.focus();
    var lift = function (e) {
      if (e.type === 'touchend') e.preventDefault();
      input.style.pointerEvents = 'auto';
      input.focus();
    };
    window.addEventListener('touchend', lift, { once: true, passive: false });
    window.addEventListener('mouseup', lift, { once: true });
    setTimeout(function () { input.style.pointerEvents = 'auto'; }, 300);  // a tap too quick for the above: the next one lands on the input
    return id;
  },

  // Moves edit `id`'s input over its field. `geometry` (Unity screen pixels, from the top left): x, y, width, height,
  // font size, side padding, corner radius.
  PUCmon_PlaceText: function (id, geometry) {
    var s = window.pucmonText;
    if (!s || s.id !== id || !s.input) return;
    var g = HEAPF32.subarray(geometry >> 2, (geometry >> 2) + 7);
    var canvas = Module.canvas, r = canvas.getBoundingClientRect(), k = r.width / canvas.width, st = s.input.style;
    st.left = r.left + g[0] * k + 'px';
    st.top = r.top + g[1] * k + 'px';
    st.width = g[2] * k + 'px';
    st.height = g[3] * k + 'px';
    st.fontSize = g[4] * k + 'px';
    st.padding = '0 ' + g[5] * k + 'px';
    st.borderRadius = g[6] * k + 'px';
  },

  // 1 while edit `id` is going on, 2 when it ended, 3 when it ended with Enter.
  PUCmon_TextState: function (id) {
    var s = window.pucmonText;
    return s && s.id === id ? s.state : 2;
  },

  // The text typed in edit `id` since the last call, or null.
  PUCmon_TakeText: function (id) {
    var s = window.pucmonText;
    if (!s || s.id !== id || !s.changed) return 0;
    s.changed = false;
    var size = lengthBytesUTF8(s.text) + 1, buffer = _malloc(size);
    stringToUTF8(s.text, buffer, size);
    return buffer;
  }
});
