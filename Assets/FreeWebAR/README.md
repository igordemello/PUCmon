# Free WebAR (8th Wall open source + rastreador denso + Unity WebGL)

Rastreamento de imagem no navegador, sem Zappar e sem assinatura. Tudo fica dentro do build: nenhuma chave, conta ou
serviço externo. A Unity só desenha o conteúdo por cima do vídeo da câmera, com fundo transparente.

## Como funciona

1. **Câmera e detecção:** o motor da [8th Wall](https://github.com/8thwall/8thwall), aberto sob licença MIT em 2026
   (`xr/LICENSE`). Ele abre a câmera, conhece a lente de cada aparelho e encontra a imagem no quadro.
2. **Rastreamento quadro a quadro:** `tracker.js`, um alinhamento direto denso (Lucas-Kanade composicional inverso
   sobre a homografia, de grosso para fino, com compensação de brilho/contraste e pesos robustos). Ele usa todos os
   pixels de borda da arte, então segura imagens com pouca textura com precisão sub-pixel, e a inclinação sai da
   homografia sem a ambiguidade "espelhada" dos rastreadores de pontos.
3. **Sincronia:** a pose é calculada sobre o mesmo quadro que aparece na tela, então o conteúdo não "escorrega"
   atrás do vídeo.
4. `freewebar.js` converte tudo para o espaço da câmera da Unity. `FreeWebAR.jslib` + `WebARImageTracker.cs` (na
   **AR Camera**) aplicam a projeção, movem o `ImageTarget` do cartaz visto (com uma entrada suave de 0,2 s,
   ajustável), ativam só ele e disparam `onFound` / `onLost`.

O espaço do alvo é o mesmo da Zappar: imagem inteira centrada na origem, **2 unidades de altura**, conteúdo "saindo"
da imagem em **-Z local**. Monte o conteúdo como filho do objeto com `ImageTarget`.

Medido com uma câmera sintética 3D (cartão inclinando até 40°, ruído e tremor de mão), comparando o conteúdo com o
quadro exibido, com a imagem vermelha de teste: cantos a 0,5 px e um ponto 3D acima do cartão a 1,6 px em movimento,
sem nenhum episódio de inclinação errada; com `image1`/`image2`: cantos a 0,5–0,7 px, encontrados em 0,2–0,4 s. O rastreador gasta ~2–4 ms por quadro.

## Cartazes (vários, um de cada vez)

Cada cartaz é um objeto da cena com o componente `ImageTarget`:

1. Coloque a imagem em `Assets/ImagesToTracker` (mínimo 480×640 px).
2. Crie um GameObject vazio, adicione `ImageTarget` e arraste a imagem para o campo **Image**.
3. Ponha o conteúdo (modelos, textos, etc.) como filho dele. Os cartazes `image1`/`image2` da `SampleScene` têm um
   filho *Content (Y = out of the card)* girado -90° em X: dentro dele o cartaz é o "chão" (Y para fora da imagem, Z
   para o topo), o jeito mais fácil de pôr um modelo em pé sobre ele.
4. Use `onFound` / `onLost` do componente para reagir (ex.: o cartaz antigo chama `ImageTrackingFunctions`).

No build WebGL, `Editor/ImageTargetBuild.cs` gera sozinho os dados de rastreamento (`image-targets/` no build) de
todas as imagens usadas na cena; não há mais CLI nem lista para editar. Aparece um cartaz por vez: o que está sendo
seguido fica até sair de vista, e então vale o próximo que a câmera encontrar. O nome do asset da imagem identifica o
cartaz, então não repita nomes.

O rastreamento usa o recorte 3:4 central da imagem (4:3 se ela for paisagem). A posição do conteúdo continua relativa
à imagem inteira. Quanto mais bordas e detalhe a arte tiver, mais rápido ela é encontrada. Modelos `.glb`/`.gltf`
entram pelo pacote glTFast (já no `manifest.json`): basta soltá-los em `Assets`.

## Build e publicação (Netlify)

1. *File > Build Settings > WebGL > Build* (o template **FreeWebAR** e o Gzip com *Decompression Fallback* já
   estão configurados no Player Settings).
2. Arraste a pasta gerada para <https://app.netlify.com/drop>. O Netlify já dá HTTPS, que a câmera exige.

## Câmera e diagnóstico

Em celular com várias lentes, a página escolhe a lente principal pelo nome das câmeras, em qualquer idioma: nunca a
0.5x, nem a câmera "Dupla/Tripla" do iPhone Pro, que troca para a 0.5x de perto.

Com `?debug` no fim do link aparecem a lista de câmeras (◀ = a escolhida), a câmera aberta, a resolução e o estado
(`denso`, `8th Wall` ou `procurando`) com a correlação e o tempo do rastreador. `?cam=N` força a câmera N da lista.

Testes: `node "Assets/FreeWebAR/Tests~/freewebar.test.mjs"` e `node "Assets/FreeWebAR/Tests~/tracker.test.mjs"`.
