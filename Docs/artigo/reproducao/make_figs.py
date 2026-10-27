# Paper figures (SVG) from results/: python make_figs.py <out dir>
import json, math, os, sys, statistics as st
from analyze import summarize

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)
DENSE, ENGINE, INK, GRID = '#1d4f91', '#c0392b', '#222', '#d9d9d9'
FONT = "font-family='Times New Roman, Times, serif'"


def text(x, y, s, size=11, anchor='middle', extra=''):
    return f"<text x='{x:.1f}' y='{y:.1f}' font-size='{size}' text-anchor='{anchor}' fill='{INK}' {FONT} {extra}>{s}</text>"


class Axes:
    """A plot area: data -> SVG coordinates, with grid, ticks and labels."""
    def __init__(self, x, y, w, h, xr, yr, log=False):
        self.x, self.y, self.w, self.h, self.xr, self.yr, self.log = x, y, w, h, xr, yr, log
    def X(self, v): return self.x + (v - self.xr[0]) / (self.xr[1] - self.xr[0]) * self.w
    def Y(self, v):
        f = (lambda q: math.log10(q)) if self.log else (lambda q: q)
        return self.y + self.h - (f(v) - f(self.yr[0])) / (f(self.yr[1]) - f(self.yr[0])) * self.h
    def frame(self, xticks, yticks, xlabel, ylabel, title=None, xfmt=str, yfmt=str):
        s = []
        for v in yticks:
            s.append(f"<line x1='{self.x}' x2='{self.x + self.w}' y1='{self.Y(v):.1f}' y2='{self.Y(v):.1f}' stroke='{GRID}' stroke-width='0.6'/>")
            s.append(text(self.x - 5, self.Y(v) + 3.5, yfmt(v), 10, 'end'))
        for v in xticks:
            s.append(text(self.X(v), self.y + self.h + 14, xfmt(v), 10))
        s.append(f"<rect x='{self.x}' y='{self.y}' width='{self.w}' height='{self.h}' fill='none' stroke='{INK}' stroke-width='0.8'/>")
        s.append(text(self.x + self.w / 2, self.y + self.h + 30, xlabel, 11))
        s.append(text(self.x - 38, self.y + self.h / 2, ylabel, 11, 'middle', f"transform='rotate(-90 {self.x - 38} {self.y + self.h / 2})'"))
        if title: s.append(text(self.x + self.w / 2, self.y - 7, title, 11.5, 'middle', "font-weight='bold'"))
        return s


def svg(name, w, h, body):
    open(os.path.join(OUT, name), 'w', encoding='utf8').write(
        f"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {w} {h}' width='{w}' height='{h}'>"
        f"<rect width='{w}' height='{h}' fill='white'/>{''.join(body)}</svg>")


def legend(x, y, items):
    s = []
    for i, (label, color, dash, marker) in enumerate(items):
        yy = y + 15 * i
        s.append(f"<line x1='{x}' x2='{x + 22}' y1='{yy}' y2='{yy}' stroke='{color}' stroke-width='1.8' stroke-dasharray='{dash}'/>")
        if marker: s.append(marker(x + 11, yy, color))
        s.append(text(x + 27, yy + 3.5, label, 10, 'start'))
    return s


# ---- Fig. A: corner error (% of card height) per image, still and moving, log scale ----------------------------
rows = [summarize(p) for p in sorted(__import__('glob').glob(os.path.join(HERE, 'results', '[ms][ot]*_*.json')))]
imgs = ['1.png', '2.png', '3.png', '4.png', '5.png', '6.png', 'IMAGEM PARA TESTE AR UNITY.png']
label = lambda i: 'T' + i[0] if i[0].isdigit() else 'B'
body, W = [], 700
for p, mode in enumerate(['still', 'move']):
    ax = Axes(70 + p * 335, 28, 270, 170, (0, len(imgs)), (0.1, 30), log=True)
    body += ax.frame([], [0.1, 0.3, 1, 3, 10, 30], '', 'erro RMS dos cantos (% da altura)' if p == 0 else '',
                     'Câmera parada' if mode == 'still' else 'Câmera em movimento', yfmt=lambda v: f'{v:g}')
    for i, img in enumerate(imgs):
        body.append(text(ax.X(i + 0.5), ax.y + ax.h + 14, label(img), 10))
        for j, (cond, color) in enumerate([('dense', DENSE), ('engine', ENGINE)]):
            r = next(r for r in rows if r['img'] == img and r['mode'] == mode and r['cond'] == cond)
            v, x0 = r['cnorm_rms'], ax.X(i + 0.16 + 0.34 * j)
            body.append(f"<rect x='{x0:.1f}' y='{ax.Y(v):.1f}' width='{0.34 * ax.w / len(imgs):.1f}' height='{ax.Y(0.1) - ax.Y(v):.1f}' fill='{color}'/>")
            if r['tracked'] < 99.5:
                body.append(text(x0 + 0.17 * ax.w / len(imgs), ax.Y(v) - 4, f"{r['tracked']:.0f}%*", 8.5))
body += [f"<rect x='300' y='232' width='10' height='10' fill='{DENSE}'/>", text(314, 241, 'denso (proposto)', 10.5, 'start'),
         f"<rect x='420' y='232' width='10' height='10' fill='{ENGINE}'/>", text(434, 241, 'motor (8th Wall)', 10.5, 'start')]
svg('fig-erro-por-imagem.svg', W, 252, body)

# ---- Fig. B: error over time, T1 moving -------------------------------------------------------------------------
def series(path):
    d = json.load(open(path)); out = []
    for r in d['samples']:
        if len(r) > 2:
            e = r[5:]
            out.append((r[0] / 1000, math.sqrt(sum(e[2 * k] ** 2 + e[2 * k + 1] ** 2 for k in range(4)) / 4)))
    return out
body = []
ax = Axes(60, 15, 610, 160, (0, 12), (0, 14))
body += ax.frame(range(0, 13, 2), range(0, 15, 2), 'tempo (s)', 'erro RMS dos cantos (px)')
for cond, color in [('engine', ENGINE), ('dense', DENSE)]:
    pts = series(os.path.join(HERE, 'results', f'move_{cond}_1_png.json'))
    body.append(f"<polyline fill='none' stroke='{color}' stroke-width='1.2' points='" + ' '.join(f'{ax.X(t):.1f},{ax.Y(min(e, 14)):.1f}' for t, e in pts) + "'/>")
body += legend(480, 108, [('motor (8th Wall)', ENGINE, '', None), ('denso (proposto)', DENSE, '', None)])
svg('fig-serie-temporal.svg', 700, 225, body)

# ---- Fig. C: ablation, convergence rate vs initial corner perturbation -------------------------------------------
abl = json.load(open(os.path.join(HERE, 'results', 'ablation.json'), encoding='utf8'))
variants = [('full', 'completo (implantado)', INK, '', 'o'), ('sem-fotometria', 'sem compensação fotométrica', '#e67e22', '5,3', 's'),
            ('sem-Huber', 'sem Huber (L2)', '#7f8c8d', '2,2', None), ('um-nível', 'um nível (sem pirâmide)', '#8e44ad', '6,2,2,2', 'd'),
            ('Tukey', 'Tukey + MAD + fotometria robusta', '#16a085', '', 't')]
def mark(kind):
    if kind == 'o': return lambda x, y, c: f"<circle cx='{x:.1f}' cy='{y:.1f}' r='2.6' fill='{c}'/>"
    if kind == 's': return lambda x, y, c: f"<rect x='{x - 2.5:.1f}' y='{y - 2.5:.1f}' width='5' height='5' fill='{c}'/>"
    if kind == 'd': return lambda x, y, c: f"<path d='M{x:.1f},{y - 3.3:.1f} l3.3,3.3 l-3.3,3.3 l-3.3,-3.3z' fill='{c}'/>"
    if kind == 't': return lambda x, y, c: f"<path d='M{x:.1f},{y - 3.3:.1f} l3.3,5.6 l-6.6,0z' fill='{c}'/>"
    return None
body = []
scenes = [('limpa', 'Só ruído do sensor'), ('fotometria', 'Ganho e offset de brilho'), ('oclusão', 'Oclusão de 20% + brilho')]
for p, (scene, title) in enumerate(scenes):
    ax = Axes(55 + p * 220, 25, 180, 150, (0, 20), (0, 100))
    body += ax.frame([0, 4, 8, 12, 16, 20], [0, 25, 50, 75, 100], 'σ da perturbação (px)', 'convergência (%)' if p == 0 else '', title)
    for key, name, color, dash, mk in variants:
        pts = [(r['sigma'], 100 * r['conv']) for r in abl if r['scene'] == scene and r['variant'] == key]
        body.append(f"<polyline fill='none' stroke='{color}' stroke-width='1.4' stroke-dasharray='{dash}' points='" + ' '.join(f'{ax.X(s):.1f},{ax.Y(v):.1f}' for s, v in pts) + "'/>")
        m = mark(mk)
        if m: body += [m(ax.X(s), ax.Y(v), color) for s, v in pts]
body += legend(70, 232, [(n, c, d, mark(m)) for _, n, c, d, m in variants[:3]])
body += legend(360, 232, [(n, c, d, mark(m)) for _, n, c, d, m in variants[3:]])
svg('fig-ablacao.svg', 700, 280, body)
print('ok')
