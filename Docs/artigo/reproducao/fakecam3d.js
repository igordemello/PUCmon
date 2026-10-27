// Dev-only 3D fake camera: renders the target card with WebGL in true perspective (tilt, roll, distance) over a
// cluttered table, adds sensor noise, and replaces getUserMedia with it. Knows the exact card pose, so it can
// measure how far the page's AR overlay lands from the real card on screen.
//
// Query params: img (target image url), noise (0-1, default .06), camfov (true lens FOV on the long side, deg, 65)
// Page contract: window.freeWebARPose = [tracked, px,py,pz, fx,fy,fz, ux,uy,uz, projection 16, ...] Unity camera
// space (left-handed, card: centre origin, full image 2 units tall, content towards the viewer on -Z).
(() => {
  const qs = new URLSearchParams(location.search);
  const [W, H] = qs.has('land') ? [1280, 720] : [720, 1280];  // ?land: landscape webcam
  const NOISE = Number(qs.get('noise') ?? 0.06);
  const TRUE_FOV = Number(qs.get('camfov') || 65);

  // --- 60 Hz rAF even when the Browser pane is hidden (it otherwise starves or drops callbacks) ---
  let queue = [], lastFrame = 0, pumping = false;
  const pump = new MessageChannel();
  pump.port1.onmessage = () => {
    const now = performance.now();
    if (now - lastFrame >= 1000 / 60) { lastFrame = now; const due = queue; queue = []; due.forEach(cb => cb(now)); }
    if (queue.length) pump.port2.postMessage(0); else pumping = false;
  };
  window.requestAnimationFrame = cb => { queue.push(cb); if (!pumping) { pumping = true; pump.port2.postMessage(0); } return 0; };

  const canvas = document.createElement('canvas');
  canvas.width = W; canvas.height = H;
  const gl = canvas.getContext('webgl2', { preserveDrawingBuffer: true, antialias: true });

  const shader = (type, src) => { const s = gl.createShader(type); gl.shaderSource(s, src); gl.compileShader(s); if (!gl.getShaderParameter(s, gl.COMPILE_STATUS)) throw gl.getShaderInfoLog(s); return s; };
  const prog = gl.createProgram();
  gl.attachShader(prog, shader(gl.VERTEX_SHADER, `#version 300 es
    in vec2 pos; in vec2 uv; uniform mat4 mvp; out vec2 vuv;
    void main() { vuv = uv; gl_Position = mvp * vec4(pos, 0.0, 1.0); }`));
  gl.attachShader(prog, shader(gl.FRAGMENT_SHADER, `#version 300 es
    precision highp float; in vec2 vuv; uniform sampler2D tex; uniform float seed, noise; out vec4 o;
    float h(vec2 p) { return fract(sin(dot(p, vec2(12.9898, 78.233)) + seed) * 43758.5453); }
    void main() { vec3 c = texture(tex, vuv).rgb; c += (h(gl_FragCoord.xy) - 0.5) * noise * 2.0; o = vec4(c, 1.0); }`));
  gl.bindAttribLocation(prog, 0, 'pos'); gl.bindAttribLocation(prog, 1, 'uv');
  gl.linkProgram(prog);
  const U = n => gl.getUniformLocation(prog, n);

  const quad = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER, quad);
  gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([-1, -1, 0, 0, 1, -1, 1, 0, -1, 1, 0, 1, 1, 1, 1, 1]), gl.STATIC_DRAW);
  gl.enableVertexAttribArray(0); gl.vertexAttribPointer(0, 2, gl.FLOAT, false, 16, 0);
  gl.enableVertexAttribArray(1); gl.vertexAttribPointer(1, 2, gl.FLOAT, false, 16, 8);

  const texture = source => {
    const t = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, t);
    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, source);
    gl.generateMipmap(gl.TEXTURE_2D);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR_MIPMAP_LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    return t;
  };
  const bgCanvas = document.createElement('canvas');
  bgCanvas.width = bgCanvas.height = 1024;
  const b = bgCanvas.getContext('2d');
  b.fillStyle = '#8a8378'; b.fillRect(0, 0, 1024, 1024);
  let seed = 7; const rnd = () => (seed = (seed * 16807) % 2147483647) / 2147483647;
  for (let i = 0; i < 900; i++) {
    b.fillStyle = `hsl(${rnd() * 360},20%,${30 + rnd() * 50}%)`;
    b.fillRect(rnd() * 1024, rnd() * 1024, 4 + rnd() * 30, 4 + rnd() * 30);
  }
  const bgTex = texture(bgCanvas);
  let cardTex = null, aspect = 1;
  const img = new Image();
  img.onload = () => {
    const s = Math.min(1, 1024 / img.naturalHeight), c = document.createElement('canvas');  // mip-friendly size
    c.width = Math.round(img.naturalWidth * s); c.height = Math.round(img.naturalHeight * s);
    c.getContext('2d').drawImage(img, 0, 0, c.width, c.height);
    cardTex = texture(c); aspect = img.naturalWidth / img.naturalHeight;
  };
  img.src = qs.get('img') || '/Assets/ImagesToTracker/1.png';
  window.fakecam3dSetImage = url => { img.src = url; };

  // --- tiny column-major mat4 helpers (GL, right-handed, camera looks down -Z) ---
  const mul = (a, b) => { const o = new Array(16); for (let c = 0; c < 4; c++) for (let r = 0; r < 4; r++) { let s = 0; for (let k = 0; k < 4; k++) s += a[k * 4 + r] * b[c * 4 + k]; o[c * 4 + r] = s; } return o; };
  const persp = (fovy, asp, n, f) => { const t = 1 / Math.tan(fovy / 2); return [t / asp, 0, 0, 0, 0, t, 0, 0, 0, 0, (f + n) / (n - f), -1, 0, 0, 2 * f * n / (n - f), 0]; };
  const rot = (rx, ry, rz) => {
    const [cx, sx, cy, sy, cz, sz] = [Math.cos(rx), Math.sin(rx), Math.cos(ry), Math.sin(ry), Math.cos(rz), Math.sin(rz)];
    const X = [1, 0, 0, 0, 0, cx, sx, 0, 0, -sx, cx, 0, 0, 0, 0, 1], Y = [cy, 0, -sy, 0, 0, 1, 0, 0, sy, 0, cy, 0, 0, 0, 0, 1], Z = [cz, sz, 0, 0, -sz, cz, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1];
    return mul(mul(Z, Y), X);
  };
  const trs = (t, r, s) => { const m = mul([1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, t[0], t[1], t[2], 1], r); return mul(m, [s[0], 0, 0, 0, 0, s[1], 0, 0, 0, 0, s[2], 0, 0, 0, 0, 1]); };
  const xf = (m, v) => [0, 1, 2, 3].map(r => m[r] * v[0] + m[4 + r] * v[1] + m[8 + r] * v[2] + m[12 + r] * v[3]);
  // Camera: TRUE_FOV across the frame's long side.
  const P = persp(2 * Math.atan(Math.tan(TRUE_FOV * Math.PI / 360) * H / Math.max(W, H)), W / H, 0.05, 100);

  // --- card motion (GL camera space): distance in card heights (card is 2 units tall) ---
  const fc = window.fakecam3d = { mode: 'move', tremor: 0.004, t0: performance.now() };
  fc.poseAt = t => {
    if (fc.mode === 'still') return fc.still;
    // Tilts up to 40°, roll ±25°, lateral drift, card 25-45% of the frame height; incommensurate frequencies.
    return {
      t: [0.7 * Math.sin(t * 0.37), 0.6 * Math.sin(t * 0.29 + 1), -(4.5 + 1.2 * Math.sin(t * 0.21))],
      r: [0.7 * Math.sin(t * 0.43), 0.6 * Math.sin(t * 0.31 + 2), 0.45 * Math.sin(t * 0.19)],
    };
  };
  fc.setStill = (t, r) => { fc.mode = 'still'; fc.still = { t, r }; };
  fc.pose = fc.poseAt(0);
  let last = 0;
  function draw(now) {
    if (now - last >= 32) {  // ~30 fps camera; draws the table even before the card image loads (real video has size at once)
      last = now;
      let p = fc.poseAt((now - fc.t0) / 1000);
      if (fc.tremor) {  // hand tremor: real motion, part of the truth
        const j = () => (Math.random() - 0.5) * fc.tremor;
        p = { t: [p.t[0] + j() * 6, p.t[1] + j() * 6, p.t[2] + j() * 6], r: [p.r[0] + j(), p.r[1] + j(), p.r[2] + j()] };
      }
      fc.pose = p;
      fc.frameId = ((fc.frameId || 0) + 1) % 4096;
      (fc.history ||= new Map()).set(fc.frameId, p);
      fc.history.delete((fc.frameId + 4096 - 90) % 4096);
      gl.viewport(0, 0, W, H);
      gl.clearColor(0, 0, 0, 1); gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
      gl.useProgram(prog);
      gl.uniform1f(U('seed'), Math.random() * 100); gl.uniform1f(U('noise'), NOISE);
      // table: big plane behind the card, same tilt so it reads like the card lies on it
      gl.bindTexture(gl.TEXTURE_2D, bgTex);
      gl.uniformMatrix4fv(U('mvp'), false, mul(P, trs([p.t[0], p.t[1], p.t[2] - 0.02], rot(...p.r), [9, 9, 1])));
      gl.drawArrays(gl.TRIANGLE_STRIP, 0, 4);
      if (cardTex) {
        gl.bindTexture(gl.TEXTURE_2D, cardTex);
        gl.uniformMatrix4fv(U('mvp'), false, mul(P, trs(p.t, rot(...p.r), [aspect, 1, 1])));
        gl.drawArrays(gl.TRIANGLE_STRIP, 0, 4);
      }
      // Frame id as 12 black/white 16 px squares along the top-left edge (read back from the frame the page shows).
      gl.enable(gl.SCISSOR_TEST);
      for (let b = 0; b < 12; b++) {
        const v = (fc.frameId >> b) & 1;
        gl.scissor(b * 16, H - 16, 16, 16); gl.clearColor(v, v, v, 1); gl.clear(gl.COLOR_BUFFER_BIT);
      }
      gl.disable(gl.SCISSOR_TEST);
      fc.frames = (fc.frames || 0) + 1;
    }
    requestAnimationFrame(draw);
  }
  requestAnimationFrame(draw);
  // A live source stream; each getUserMedia gets its own clone, like a real camera.
  const live = canvas.captureStream(30);
  navigator.mediaDevices.getUserMedia = async () => live.clone();
  navigator.mediaDevices.enumerateDevices = async () => [{ kind: 'videoinput', label: 'Fake 3D camera', deviceId: 'fake' }];

  // Which camera frame is on screen: decode the id from the CPU copy of the shown frame (needs CameraPixelArray).
  window.addEventListener('xrloaded', () => XR8.addCameraPipelineModule({
    name: 'harness-frame-id',
    onUpdate: ({ processGpuResult }) => {
      const c = processGpuResult?.camerapixelarray;
      if (!c?.pixels) return;
      const k = c.cols / W;
      let id = 0;
      for (let b = 0; b < 12; b++) if (c.pixels[Math.floor(8 * k) * c.cols + Math.floor((b * 16 + 8) * k)] > 128) id |= 1 << b;
      fc.shownFrame = id;
    },
  }));

  // --- measurement ---
  // Card-local points: 4 corners + a point half a card-height above the centre (like 3D content).
  const pts = () => [[-aspect, 1, 0], [aspect, 1, 0], [aspect, -1, 0], [-aspect, -1, 0], [0, 0, 1]];
  const view = () => document.getElementById('unity-canvas') || document.body;
  const toScreen = (ndc, vw, vh) => [(ndc[0] + 1) / 2 * vw, (1 - ndc[1]) / 2 * vh];
  const truthScreen = () => {
    const el = view(), vw = el.clientWidth || innerWidth, vh = el.clientHeight || innerHeight;
    const p = (fc.useShown && fc.history?.get(fc.shownFrame)) || fc.pose;
    const cover = Math.max(vw / W, vh / H), M = mul(P, trs(p.t, rot(...p.r), [1, 1, 1]));
    return pts().map(q => {
      const c = xf(M, [q[0], q[1], q[2], 1]);
      const vx = (c[0] / c[3] + 1) / 2 * W, vy = (1 - c[1] / c[3]) / 2 * H;  // video px
      return [(vx - W / 2) * cover + vw / 2, (vy - H / 2) * cover + vh / 2];
    });
  };
  const measuredScreen = () => {
    const q = window.freeWebARPose;
    if (!q || !q[0] || q.length < 26) return null;
    const el = view(), vw = el.clientWidth || innerWidth, vh = el.clientHeight || innerHeight;
    const proj = Array.from(q.slice(10, 26));
    const pos = [q[1], q[2], q[3]], fwd = [q[4], q[5], q[6]], up = [q[7], q[8], q[9]];
    const right = [up[1] * fwd[2] - up[2] * fwd[1], up[2] * fwd[0] - up[0] * fwd[2], up[0] * fwd[1] - up[1] * fwd[0]];
    // card-local (GL: +Z to the viewer) -> Zappar-local (-Z to the viewer) -> Unity camera space -> GL camera space
    return pts().map(([X, Y, Z]) => {
      const u = [0, 1, 2].map(i => pos[i] + X * right[i] + Y * up[i] - Z * fwd[i]);
      const c = xf(proj, [u[0], u[1], -u[2], 1]);
      return toScreen([c[0] / c[3], c[1] / c[3]], vw, vh);
    });
  };
  fc.sample = () => ({ truth: truthScreen(), got: measuredScreen() });
  fc.waitTracked = async (ms = 30000) => {
    const t0 = performance.now(); let since = null;
    while (performance.now() - t0 < ms) {
      if (!window.freeWebARPose?.[0]) since = null; else if (since === null) since = performance.now(); else if (performance.now() - since > 1500) return Math.round(since - t0);
      await new Promise(r => setTimeout(r, 100));
    }
    return -1;
  };
  canvas.dataset.fake = '1';
})();
