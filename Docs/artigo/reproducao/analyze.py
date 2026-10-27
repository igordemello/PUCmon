# Summaries of the experiment logs in results/: one row per (mode, condition, image).
import json, glob, math, os, sys, statistics as st
def pct(v, p):
    v = sorted(v); return v[min(len(v) - 1, int(p * len(v)))] if v else float('nan')
def rms(v): return math.sqrt(sum(x * x for x in v) / len(v)) if v else float('nan')
def summarize(path):
    d = json.load(open(path)); S = d['samples']; ok = [r for r in S if len(r) > 2]
    corner, top, cnorm, tnorm, ms, dense = [], [], [], [], [], 0
    for r in ok:
        t, frame, h, m, dn, *e = r
        ek = [math.hypot(e[2 * k], e[2 * k + 1]) for k in range(5)]
        c = math.sqrt(sum(x * x for x in ek[:4]) / 4)
        corner.append(c); top.append(ek[4]); cnorm.append(100 * c / h); tnorm.append(100 * ek[4] / h)
        if m is not None and dn: ms.append(m)
        dense += dn
    jit = []  # frame-to-frame change of the corner error vectors (bias removed): visible shake
    for a, b in zip(ok, ok[1:]):
        if a[1] == b[1] or b[0] - a[0] > 80: continue  # same shown frame, or a gap
        ea, eb = a[5:], b[5:]
        jit.append(math.sqrt(sum((eb[2 * k] - ea[2 * k]) ** 2 + (eb[2 * k + 1] - ea[2 * k + 1]) ** 2 for k in range(4)) / 4))
    # Bias (mean error vector per corner over the run) vs noise (what is left around it).
    mu = [[st.mean(r[5 + 2 * k + c] for r in ok) for c in (0, 1)] for k in range(4)] if ok else None
    bias = math.sqrt(sum(m[0] ** 2 + m[1] ** 2 for m in mu) / 4) if ok else float('nan')
    noise = math.sqrt(sum((r[5 + 2 * k] - mu[k][0]) ** 2 + (r[6 + 2 * k] - mu[k][1]) ** 2 for r in ok for k in range(4)) / (4 * len(ok))) if ok else float('nan')
    return dict(bias=bias, noise=noise, img=d['img'], cond='dense' if d['dense'] else 'engine', mode=d['mode'], found=d['found'],
                tracked=100 * len(ok) / len(S), n=len(ok), corner_rms=rms(corner), corner_med=st.median(corner) if corner else float('nan'),
                corner_p90=pct(corner, .9), top_rms=rms(top), top_p90=pct(top, .9), cnorm_rms=rms(cnorm), tnorm_rms=rms(tnorm),
                jitter=rms(jit), ms_mean=st.mean(ms) if ms else None, ms_p95=pct(ms, .95) if ms else None,
                dense_share=100 * dense / len(ok) if ok else 0, card_h=st.median([r[2] for r in ok]) if ok else None)
if __name__ == '__main__':
    rows = [summarize(p) for p in sorted(glob.glob(os.path.join(os.path.dirname(__file__), 'results', '[ms][ot]*_*.json')))]
    json.dump(rows, open(os.path.join(os.path.dirname(__file__), 'summary.json'), 'w'), indent=1)
    for r in rows:
        print(f"{r['mode']:5} {r['cond']:6} {r['img'][:12]:12} trk {r['tracked']:5.1f}% cRMS {r['corner_rms']:6.2f} p90 {r['corner_p90']:6.2f} "
              f"top {r['top_rms']:6.2f} c% {r['cnorm_rms']:5.2f} jit {r['jitter']:5.2f} ms {r['ms_mean'] or 0:5.1f} dense {r['dense_share']:5.1f}% h {r['card_h']}")
