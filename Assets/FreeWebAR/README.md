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
   **AR Camera**) aplicam a projeção, movem o **Image Target** (com uma entrada suave de 0,2 s, ajustável) e disparam
   `onTargetFound` / `onTargetLost`.

O espaço do alvo é o mesmo da Zappar: imagem inteira centrada na origem, **2 unidades de altura**, conteúdo "saindo"
da imagem em **-Z local**. Monte o conteúdo como filho do **Image Target**.

Medido com uma câmera sintética 3D (cartão inclinando até 40°, ruído e tremor de mão), comparando o conteúdo com o
quadro exibido, com a imagem vermelha de teste: cantos a 0,5 px e um ponto 3D acima do cartão a 1,6 px em movimento,
sem nenhum episódio de inclinação errada. O rastreador gasta ~2–4 ms por quadro.

## Trocar a imagem-alvo

1. No terminal: `npx @8thwall/image-target-cli@latest`. Informe o caminho da imagem, escolha `flat`, aceite o
   recorte padrão, dê uma pasta de saída e um nome (ex.: `cartao`).
2. Copie `cartao.json` e `cartao_luminance.*` para `Assets/WebGLTemplates/FreeWebAR/image-targets/`.
3. Em `index.html`, troque `TARGETS` para `['image-targets/cartao.json']`.
4. Troque a textura de `Target Preview.mat` e ajuste a escala X do *Preview Object* para `2 × largura / altura`.

O rastreamento usa o recorte 3:4 central da imagem (4:3 se ela for paisagem). A posição do conteúdo continua relativa
à imagem inteira. Quanto mais bordas e detalhe a arte tiver, mais rápido ela é encontrada.

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
