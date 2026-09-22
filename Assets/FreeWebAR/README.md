# Free WebAR (MindAR + Unity WebGL)

Rastreamento de imagem no navegador, sem Zappar e sem assinatura. O [MindAR](https://github.com/hiukim/mind-ar-js)
(MIT, open source) roda na página e detecta a imagem impressa. A Unity só desenha o conteúdo por cima do vídeo da
câmera, com fundo transparente. Nada depende de serviço externo: tudo fica dentro do build.

## Como funciona

- `WebGLTemplates/FreeWebAR/index.html`: abre a câmera traseira em HD, rastreia uma cópia reduzida do vídeo
  (640 px) com o MindAR e escreve a pose em `window.freeWebARPose`.
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

Parâmetros na URL, sem rebuild: `?mincf=0.0001&beta=10` (filtro: menos tremido x menos atraso),
`?track=800` (mais resolução de rastreamento, mais CPU), `?miss=10` (tempo que o conteúdo fica visível ao perder a imagem).

Teste da conversão de pose: `node "Assets/FreeWebAR/Tests~/pose.test.mjs"`.
