# Free WebAR (8th Wall open source + Unity WebGL)

Rastreamento de imagem no navegador, sem Zappar e sem assinatura. O motor é o da
[8th Wall](https://github.com/8thwall/8thwall), aberto sob licença MIT em 2026 (`xr/LICENSE`). A página roda o
motor, que abre a câmera e rastreia a imagem impressa. A Unity só desenha o conteúdo por cima do vídeo, com fundo
transparente. Tudo fica dentro do build: nenhuma chave, conta ou serviço externo.

## Como funciona

- `WebGLTemplates/FreeWebAR/index.html`: roda o motor (`xr/xr.js` + `xr/xr-tracking.js`) sem rastreamento de
  mundo, então o conteúdo fica preso à imagem e não "desliza". A cada quadro, escreve em `window.freeWebARPose` a pose
  do cartão e a matriz de projeção da câmera.
- `freewebar.js`: converte a pose do motor para o espaço da câmera da Unity.
- `FreeWebAR.jslib` + `WebARImageTracker.cs` (na **AR Camera**): aplicam a projeção à câmera, movem o
  **Image Target** e disparam `onTargetFound` / `onTargetLost`.
- O espaço do alvo é o mesmo da Zappar: imagem inteira centrada na origem, **2 unidades de altura**, conteúdo
  "saindo" da imagem em **-Z local**. Monte o conteúdo como filho do **Image Target**.

## Trocar a imagem-alvo

1. No terminal: `npx @8thwall/image-target-cli@latest`. Informe o caminho da imagem, escolha `flat`, aceite o
   recorte padrão, dê uma pasta de saída e um nome (ex.: `cartao`).
2. Copie `cartao.json` e `cartao_luminance.*` para `Assets/WebGLTemplates/FreeWebAR/image-targets/`.
3. Em `index.html`, troque `TARGETS` para `['image-targets/cartao.json']`.
4. Troque a textura de `Target Preview.mat` e ajuste a escala X do *Preview Object* para `2 × largura / altura`.

O motor rastreia o recorte 3:4 central da imagem (4:3 se ela for paisagem). A posição do conteúdo continua relativa
à imagem inteira.

**A arte é o que mais pesa.** Imagem boa tem muito detalhe e contraste espalhados pela área toda: ilustração,
textura, texto pequeno. Áreas grandes de cor lisa deixam a inclinação ambígua em **qualquer** rastreador. Com a imagem
vermelha de teste, o conteúdo 3D às vezes inclina para o lado errado; com uma ilustração detalhada, isso não acontece.

## Build e publicação (Netlify)

1. *File > Build Settings > WebGL > Build* (o template **FreeWebAR** e o Gzip com *Decompression Fallback* já
   estão configurados no Player Settings).
2. Arraste a pasta gerada para <https://app.netlify.com/drop>. O Netlify já dá HTTPS, que a câmera exige.

Com `?debug` no fim do link aparecem a câmera aberta, a resolução e o estado do rastreamento.

Testes da conversão de pose: `node "Assets/FreeWebAR/Tests~/freewebar.test.mjs"`.
