# Free WebAR (MindAR + Unity WebGL)

Rastreamento de imagem no navegador, sem Zappar e sem assinatura. O [MindAR](https://github.com/hiukim/mind-ar-js)
(MIT, open source) roda na página e detecta a imagem impressa. A Unity só desenha o conteúdo por cima do vídeo da
câmera, com fundo transparente. Nada depende de serviço externo: tudo fica dentro do build.

## Como funciona

- `WebGLTemplates/FreeWebAR/index.html`: abre a lente principal da câmera traseira em HD (nunca a 0.5x), rastreia
  uma cópia reduzida do vídeo (640 px) com o MindAR, suaviza a pose e a escreve em `window.freeWebARPose`.
- `WebGLTemplates/FreeWebAR/freewebar.js`: conversão da pose para a Unity, filtro anti-tremor (One Euro por canal:
  posição, inclinação e giro) e escolha da lente.
- `FreeWebAR.jslib` + `WebARImageTracker.cs` (na **AR Camera**): leem a pose a cada frame, movem o **Image Target**
  e disparam `onTargetFound` / `onTargetLost`.
- O espaço do alvo é o mesmo da Zappar: imagem centrada na origem, **2 unidades de altura**, conteúdo "saindo"
  da imagem em **-Z local**. Monte o conteúdo como filho do **Image Target**.

## Trocar a imagem-alvo

1. Abra <https://hiukim.github.io/mind-ar-js-doc/tools/compile>, envie a imagem (uns 1000 px de largura bastam)
   e baixe o `targets.mind`.
2. Substitua `Assets/WebGLTemplates/FreeWebAR/targets.mind`.
3. Troque a textura de `Target Preview.mat` e ajuste a escala X do *Preview Object* para `2 × largura / altura`.

Imagem boa = muito detalhe e contraste espalhados pela arte toda (ilustração, textura, texto pequeno). Áreas grandes
de cor lisa rastreiam mal em **qualquer** rastreador, inclusive na Zappar.

## Build e publicação (Netlify)

1. *File > Build Settings > WebGL > Build* (o template **FreeWebAR** e o Gzip com *Decompression Fallback* já
   estão configurados no Player Settings).
2. Arraste a pasta gerada para <https://app.netlify.com/drop>. O Netlify já dá HTTPS, que a câmera exige.

## Ajuste fino no celular

Parâmetros na URL, sem rebuild (ex.: `https://seu-site.netlify.app/?debug&tmin=0.3`):

| Parâmetro | Padrão | Efeito |
|---|---|---|
| `debug` | — | Lista as câmeras (▶ = a aberta) e a resolução |
| `cam=N` | auto | Força a câmera N da lista do `debug` |
| `tmin` / `tbeta` | 0.5 / 3 | Inclinação: menor `tmin` = mais firme parado; maior `tbeta` = menos atraso ao inclinar |
| `pmin` / `pbeta` | 4 / 200 | Posição (idem) |
| `rmin` / `rbeta` | 4 / 50 | Giro no plano (idem) |
| `kf` | 0 | 0 = rastreia no keyframe de 256 px (mais firme); 1 = 128 px, o padrão do MindAR |
| `track` | 640 | Resolução de rastreamento (maior = mais preciso, mais CPU) |
| `fov` | 60 | Abertura da lente no lado maior, em graus |
| `miss` | 5 | Quadros que o conteúdo continua visível depois de perder a imagem |

Testes (pose, filtro e escolha de câmera): `node "Assets/FreeWebAR/Tests~/freewebar.test.mjs"`.
