const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');
const source = fs.readFileSync(path.join(__dirname, '../Assets/WebGLTemplates/AR-Renderer/js/camera-selection.js'), 'utf8');

function setup(saved, failure) {
    const calls = [], elements = [];
    let stopped = false, reloaded = false;
    const track = { getSettings: () => ({ deviceId: 'main' }), stop: () => { stopped = true; } };
    const stream = { getVideoTracks: () => [track], getTracks: () => [track] };
    const context = {
        window: {}, console,
        sessionStorage: { getItem: () => saved, setItem: (_, value) => { saved = value; }, removeItem: () => { saved = null; } },
        location: { reload: () => { reloaded = true; } },
        navigator: { mediaDevices: {
            getUserMedia: async constraints => {
                calls.push(JSON.parse(JSON.stringify(constraints)));
                if (failure && calls.length === 1) throw Object.assign(new Error('camera error'), { name: failure });
                return stream;
            },
            enumerateDevices: async () => [
                { kind: 'videoinput', deviceId: 'main', label: 'Back camera' },
                { kind: 'videoinput', deviceId: 'ultra', label: 'Ultra wide' },
                { kind: 'audioinput', deviceId: 'mic' }
            ]
        } },
        document: {
            getElementById: () => null,
            body: { appendChild() {} },
            createElement(tag) {
                const element = { tag, style: {}, children: [], setAttribute() {},
                    appendChild(child) { this.children.push(child); },
                    addEventListener(_, callback) { this.change = callback; } };
                elements.push(element);
                return element;
            }
        }
    };
    vm.runInNewContext(source, context);
    return { open: () => context.window.PUCmonCamera.open(), calls, elements, stream,
        state: () => ({ saved, stopped, reloaded }) };
}

(async () => {
    const initial = setup(null);
    assert.equal(await initial.open(), initial.stream);
    assert.deepEqual(initial.calls[0], { audio: false, video: { facingMode: 'environment' } });
    const select = initial.elements.find(e => e.tag === 'select');
    assert.equal(select.children.length, 2);
    assert.equal(select.children[0].selected, true);
    select.value = 'ultra';
    select.change();
    assert.deepEqual(initial.state(), { saved: 'ultra', stopped: true, reloaded: true });

    const chosen = setup('main');
    await chosen.open();
    assert.deepEqual(chosen.calls[0].video, { deviceId: { exact: 'main' } });
    const missing = setup('removed', 'OverconstrainedError');
    await missing.open();
    assert.equal(missing.state().saved, null);
    assert.deepEqual(missing.calls[1].video, { facingMode: 'environment' });
    const denied = setup('main', 'NotAllowedError');
    await assert.rejects(denied.open(), { name: 'NotAllowedError' });
    assert.equal(denied.calls.length, 1);
    console.log('Camera selection: default, explicit lens, restart, missing device and permission denial passed.');
})().catch(error => { console.error(error); process.exitCode = 1; });
