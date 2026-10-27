// Dev-only static server. node devserver.mjs <root> <port>
// GET  /__tools/*        -> files from this script's folder (fake camera)
// ?fake3d on an .html     -> injects /__tools/fakecam3d.js before anything else runs
import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const [root, port = '8080'] = process.argv.slice(2);
const tools = path.dirname(fileURLToPath(import.meta.url));
const types = { '.html': 'text/html', '.js': 'text/javascript', '.mjs': 'text/javascript', '.wasm': 'application/wasm',
  '.png': 'image/png', '.jpg': 'image/jpeg', '.json': 'application/json', '.css': 'text/css', '.ttf': 'font/ttf' };

http.createServer((req, res) => {
  const url = new URL(req.url, 'http://x');
  if (req.method === 'POST' && url.pathname === '/__save') {  // experiment data -> <tools>/results/<file>
    const chunks = [];
    req.on('data', c => chunks.push(c)).on('end', () => {
      fs.writeFileSync(path.join(tools, 'results', path.basename(url.searchParams.get('file'))), Buffer.concat(chunks));
      res.end('ok');
    });
    return;
  }
  let file = url.pathname.startsWith('/__tools/') ? path.join(tools, url.pathname.slice(9)) : path.join(root, decodeURIComponent(url.pathname));
  if (fs.existsSync(file) && fs.statSync(file).isDirectory()) file = path.join(file, 'index.html');
  if (!fs.existsSync(file)) { res.writeHead(404); res.end(); return; }
  let body = fs.readFileSync(file);
  const ext = path.extname(file);
  if (ext === '.html' && url.searchParams.has('fake3d'))
    body = body.toString().replace('<head>', '<head><script src="/__tools/fakecam3d.js"></script>');
  res.writeHead(200, { 'Content-Type': types[ext] || 'application/octet-stream', 'Cache-Control': 'no-store' });
  res.end(body);
}).listen(Number(port), () => console.log(`serving ${root} on http://localhost:${port}`));
