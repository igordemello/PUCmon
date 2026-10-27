# Fills the paper template's {{TABLES}} from results/ and writes the HTML: python build_paper.py <template> <out.html>
import json, os, sys, statistics as st
from analyze import summarize

HERE = os.path.dirname(os.path.abspath(__file__))
R = {(r['mode'], r['cond'], r['img']): r for r in (summarize(os.path.join(HERE, 'results', f)) for f in os.listdir(os.path.join(HERE, 'results')) if f.startswith(('move_', 'still_')))}
F = {f[:-5]: summarize(os.path.join(HERE, 'results', f)) for f in os.listdir(os.path.join(HERE, 'results')) if f.startswith('fov63')}
ABL = json.load(open(os.path.join(HERE, 'results', 'ablation.json'), encoding='utf8'))
IMGS = ['1.png', '2.png', '3.png', '4.png', '5.png', '6.png']
B = 'IMAGEM PARA TESTE AR UNITY.png'
REG = {'still': 'parada', 'move': 'movimento'}
COND = {'dense': 'denso', 'engine': 'motor'}

def n(v, d=2): return f'{v:.{d}f}'.replace('.', ',')
def pm(rows, k, d=2): return f"{n(st.mean(r[k] for r in rows), d)} ± {n(st.stdev(r[k] for r in rows), d)}"

T = {}
# Table 1: T1-T6 aggregated
body = []
for mode in ['still', 'move']:
    for cond in ['dense', 'engine']:
        rows = [R[(mode, cond, i)] for i in IMGS]
        ms = n(st.mean(r['ms_mean'] for r in rows), 1) + f" ({n(max(r['ms_p95'] for r in rows), 1)})" if cond == 'dense' else '—'
        body.append(f"<tr{' class=sep' if cond == 'dense' and mode == 'move' else ''}><td>{REG[mode]}</td><td>{COND[cond]}</td><td>{pm(rows, 'corner_rms')}</td><td>{pm(rows, 'cnorm_rms')}</td>"
                    f"<td>{pm(rows, 'corner_p90')}</td><td>{pm(rows, 'top_rms')}</td><td>{pm(rows, 'jitter')}</td><td>{pm(rows, 'bias')}</td><td>{pm(rows, 'noise')}</td><td>{ms}</td></tr>")
T['TAB_AGG'] = ''.join(body)

# Table 2: intrinsics control
def frow(label, mode, cond, img, key):
    a = R[(mode, cond, img)]; b = F.get(key)
    cb = f"{n(b['corner_rms'])} ({n(b['cnorm_rms'])}%)" if b else '—'
    tb = n(b['top_rms']) if b else '—'
    return f"<tr><td>{label}</td><td>{REG[mode]}</td><td>{COND[cond]}</td><td>{n(a['corner_rms'])} ({n(a['cnorm_rms'])}%)</td><td>{n(a['top_rms'])}</td><td>{cb}</td><td>{tb}</td></tr>"
T['TAB_FOV'] = ''.join([frow('T1', 'still', 'dense', '1.png', 'fov63_dense_1_png'), frow('T1', 'still', 'engine', '1.png', 'fov63_engine_1_png'),
                        frow('T4', 'still', 'dense', '4.png', 'fov63_dense_4_png'), frow('B', 'still', 'dense', B, 'fov63_dense_IMAGEM_PARA_TESTE_AR_UNITY_png'),
                        frow('T1', 'move', 'dense', '1.png', 'fov63move_dense_1_png'), frow('T1', 'move', 'engine', '1.png', 'fov63move_engine_1_png')])

# Table 3: ablation
names = [('full', 'completo (implantado)'), ('sem-fotometria', 'sem compensação fotométrica'), ('sem-Huber', 'sem Huber (L2 puro)'),
         ('um-nível', 'um nível (sem pirâmide)'), ('Huber-MAD', 'Huber, escala pela mediana (MAD)'), ('IRLS', 'Huber-MAD + fotometria ponderada'),
         ('Tukey', 'Tukey + MAD + fotometria ponderada')]
body = []
for key, label in names:
    cells = []
    for scene in ['limpa', 'fotometria', 'oclusão']:
        rr = [x for x in ABL if x['variant'] == key and x['scene'] == scene]
        g = lambda s: 100 * next(x for x in rr if x['sigma'] == s)['conv']
        cells += [f"{g(4):.0f}", f"{g(12):.0f}", n(100 * st.mean(x['conv'] for x in rr), 1)]
    rr = [x for x in ABL if x['variant'] == key]
    acc = 100 * next(x for x in rr if x['scene'] == 'oclusão' and x['sigma'] == 1)['acc']
    body.append(f"<tr><td class=l>{label}</td>" + ''.join(f'<td>{c}</td>' for c in cells) + f"<td>{acc:.0f}</td><td>{n(st.median(x['msMed'] for x in rr), 1)}</td></tr>")
T['TAB_ABL'] = ''.join(body)

# Table A1: per target
body = []
for mode in ['still', 'move']:
    for img in IMGS + [B]:
        d, e = R[(mode, 'dense', img)], R[(mode, 'engine', img)]
        lab = 'T' + img[0] if img[0].isdigit() else 'B'
        body.append(f"<tr{' class=sep' if img == '1.png' and mode == 'move' else ''}><td>{REG[mode]}</td><td>{lab}</td>"
                    f"<td>{n(d['tracked'], 0)}</td><td>{n(d['corner_rms'])}</td><td>{n(d['cnorm_rms'])}</td><td>{n(d['jitter'])}</td><td>{d['found']}</td>"
                    f"<td>{n(e['tracked'], 0)}</td><td>{n(e['corner_rms'])}</td><td>{n(e['cnorm_rms'])}</td><td>{n(e['jitter'])}</td><td>{e['found']}</td></tr>")
T['TAB_ALVOS'] = ''.join(body)

html = open(sys.argv[1], encoding='utf8').read()
for k, v in T.items():
    html = html.replace('{{' + k + '}}', v)
assert '{{' not in html, 'unfilled placeholder'
open(sys.argv[2], 'w', encoding='utf8').write(html)
print('ok')
