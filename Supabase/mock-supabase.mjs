// Test-only stand-in for Supabase: any login works; captures and the "cuidar" function (same rules as
// Supabase/4-economia.sql with the Economia asset's default numbers) live in memory.
// POST /__seed {criatura: {afinidade, fome, humor}} sets a capture's state. node Supabase/mock-supabase.mjs 8768
import http from 'node:http';

const rows = new Map();  // criatura -> row
const session = () => ({ access_token: 'test', refresh_token: 'test', expires_at: Math.floor(Date.now() / 1000) + 3600,
  user: { id: 'test', email: 'teste@pucmon.invalid', user_metadata: { usuario: 'teste' } } });
const fresh = criatura => ({ criatura, afinidade: 0, fome: 60, humor: 20, atualizado_em: new Date().toISOString(), carinho_em: null });

function cuidar({ p_criatura, p_tipo, p_desempenho = 1 }) {
  const c = rows.get(p_criatura);
  if (!c) return null;
  const hours = (Date.now() - Date.parse(c.atualizado_em)) / 3600e3;
  c.fome = Math.max(0, c.fome - hours * 100 / 4);
  c.humor = Math.max(0, c.humor - hours * 100 / 4);
  const d = Math.min(1, Math.max(0, p_desempenho));
  let ganho = 0;
  if (p_tipo === 'alimentar') { if (c.fome < 80) ganho = 30; c.fome = Math.min(100, c.fome + 30); }
  else if (p_tipo === 'brincar') { if (c.humor < 80) ganho = Math.round(20 + 20 * d); c.humor = Math.min(100, c.humor + 20 + 20 * d); }
  else if (p_tipo === 'carinho') {
    if (!c.carinho_em || Date.now() - Date.parse(c.carinho_em) >= 600e3) { ganho = 5; c.carinho_em = new Date().toISOString(); }
    c.humor = Math.min(100, c.humor + 2);
  }
  c.afinidade += ganho;
  c.atualizado_em = new Date().toISOString();
  return { ...c, ganho };
}

http.createServer((req, res) => {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Headers', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
  if (req.method === 'OPTIONS') return res.end();
  let body = '';
  req.on('data', c => body += c).on('end', () => {
    const path = new URL(req.url, 'http://x').pathname;
    console.log(req.method, req.url, body);
    res.setHeader('Content-Type', 'application/json');
    if (path === '/__seed') {
      for (const [k, v] of Object.entries(JSON.parse(body))) rows.set(k, { ...(rows.get(k) || fresh(k)), ...v, atualizado_em: new Date().toISOString() });
      return res.end(JSON.stringify([...rows.values()]));
    }
    if (path.startsWith('/auth/v1/logout')) { res.writeHead(204); return res.end(); }
    if (path.startsWith('/auth/v1/')) return res.end(JSON.stringify(session()));
    if (path === '/rest/v1/capturas' && req.method === 'GET') return res.end(JSON.stringify([...rows.values()]));
    if (path === '/rest/v1/capturas' && req.method === 'POST') {
      const { criatura, ...extra } = JSON.parse(body);
      if (Object.keys(extra).length) { res.writeHead(403); return res.end('{"message":"only criatura may be inserted"}'); }
      if (!rows.has(criatura)) rows.set(criatura, fresh(criatura));
      res.writeHead(201);
      return res.end();
    }
    if (path === '/rest/v1/rpc/cuidar') {
      const out = cuidar(JSON.parse(body));
      if (!out) { res.writeHead(400); return res.end('{"message":"PUCmon não capturado"}'); }
      return res.end(JSON.stringify(out));
    }
    res.writeHead(404);
    res.end('{}');
  });
}).listen(Number(process.argv[2] || 8768), () => console.log('mock supabase on', process.argv[2] || 8768));
