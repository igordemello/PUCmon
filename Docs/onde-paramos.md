# Onde paramos (passagem de bastão)

> Escrito em 30/10/2026, no fim de uma sessão longa no PC de mesa, para continuar no notebook. Leia junto com o
> `CLAUDE.md` (como o projeto funciona) e o `Docs/plano-pos-game.md` (o que o jogo deve ter e em que pé está).
> Quando esta tarefa acabar, atualize ou apague este arquivo.

## Em uma frase

A tela da câmera roda a **5–10 FPS no celular** do Igor, com ou sem PUCmon na tela. Já pusemos medição no próprio
aparelho (`?debug`) e chaves para isolar a causa. **Falta o Igor medir no celular**, e depois corrigir o que os números
apontarem.

## O que foi feito nesta sessão (já commitado)

1. **Ajustes no protótipo, para seguir o documento do jogo:**
   - Cada PUCmon tem 4 ações, nos níveis 1 a 4; o nível 0 não libera nada.
   - O professor se chama “DR. BELTRAN O.”.
2. **Economia num asset editável por leigo** (`Assets/Jogo/Resources/Economia`):
   - Com os números de agora, o nível máximo sai em ~4 h.
   - O botão **Atualizar o servidor** do Inspector gera o `Supabase/4-economia.sql`.
   - ⚠️ Não sei se o Igor já rodou esse SQL no Supabase de verdade. **Pergunte.** Sem ele, o servidor segue com a
     economia antiga.
3. **Otimização da tela de captura, sem mudar a qualidade** (detalhes e números no plano, seção “Desempenho”):
   - o rastreador ficou mais rápido;
   - a câmera abre no toque em Registrar;
   - cada cartaz é preparado só quando aparece;
   - cada modelo 3D carrega só quando o cartaz dele aparece (`LazyModel` + `LazyModelBuild`, com os `.glb` em
     `Assets/Modelos/Resources`).
4. **Medição no aparelho:**
   - O `?debug` agora mostra o FPS da câmera, do motor e da Unity, e os ms por quadro de cada parte.
   - Chaves de teste: `?nopix` (sem cópia do quadro para o rastreador denso) e `?dpr=N` (resolução menor).
   - Código no `Assets/WebGLTemplates/FreeWebAR/index.html`.
5. **Docs:**
   - o `CLAUDE.md`;
   - este arquivo;
   - o plano atualizado;
   - o `Supabase/mock-supabase.mjs`, um Supabase falso para testes.

O build com tudo isso (Supabase de verdade, 28 cartazes) já está **publicado no GitHub Pages**: repo
`C:\Users\igoro\OneDrive\Documentos\GitHub\pucmon.dev`, que no PC de mesa estava em dia com o `origin`. É o mesmo build
que está em `Builds/FreeWebAR` no PC de mesa (pasta fora do git).

## Próximo passo: o Igor mede no celular

Abrir no celular o link do GitHub Pages do `pucmon.dev` com cada uma destas terminações, apontando para qualquer
coisa (e depois para um cartaz):

1. `?debug`
2. `?debug&nopix`
3. `?debug&dpr=1`
4. `?debug&nopix&dpr=1`

De cada uma, um print do texto no canto superior esquerdo, que tem estas linhas:

```
aberta: <câmera> 960x720
câmera 29 fps (pedidos 30) · motor 59 fps · Unity 59 fps
por quadro: Unity 2.4 ms · motor 8.0 ms · rastreador 0.0 ms
```

Pergunte também **o modelo do celular e o navegador** (Safari no iPhone? Chrome no Android? Samsung Internet?).

Referência medida no PC, com a câmera simulada:

- normal: motor 54 fps · Unity 54 fps; Unity 2,3 ms · motor 14,5 ms · rastreador 4,6 ms;
- `?nopix&dpr=1`: câmera 29 fps (pedidos 30) · motor 59 · Unity 59; Unity 2,4 ms · motor 8,0 ms · rastreador 0.

## Como ler os números e o que fazer

O que cada medida conta:

- **câmera N fps**: quadros novos que o vídeo entregou no último segundo (`getVideoPlaybackQuality`);
- **pedidos**: o que a câmera prometeu (`getSettings().frameRate`);
- **motor**: quantas vezes por segundo o motor da 8th Wall mandou um quadro para nós (`onUpdate`);
- **Unity**: quantas vezes por segundo o laço da Unity rodou;
- **ms por quadro**: tempo de CPU dentro do callback de cada um. O motor conta todo `requestAnimationFrame` fora da
  Unity, e isso inclui a leitura do quadro da GPU.

| O que aparecer | Causa provável | Onde mexer |
|---|---|---|
| **câmera** baixa (≈ o FPS que se vê), “pedidos” 30 | A câmera entrega pouco: pouca luz (o aparelho alonga a exposição) ou resolução alta demais | Testar com mais luz. Em `xr/xr.js` (motor vendorizado, **não editar**) o iPhone pede `min` 960x720 e pode abrir resolução bem maior: ver a resolução na linha `aberta:`. Para limitar, passar restrições próprias antes do `XR8.run` (veja a API de câmera da 8th Wall) ou trocar o vídeo depois de aberto com `applyConstraints` |
| **motor** com muitos ms, e `nopix` melhora muito | A leitura síncrona do quadro (`CameraPixelArray`, `readPixels`) trava a GPU | `index.html`, linha do `XR8.CameraPixelArray.pipelineModule({ luminance: true, maxDimension: 480 })`. Opções: ler um quadro sim, outro não; leitura assíncrona (WebGL2 `PIXEL_PACK_BUFFER` + `fenceSync`, um quadro de atraso); baixar `maxDimension` só nesses aparelhos. Não pode piorar a precisão do rastreio sem o Igor aprovar |
| **motor** com muitos ms, e `nopix` não muda | O próprio motor da 8th Wall (detecção de imagem em WASM) | Rodar a detecção com menos frequência enquanto o rastreador denso está seguindo; perfilar com `chrome://inspect` (Android) |
| Tudo com poucos ms, mas FPS baixo, e `dpr=1` resolve | A GPU não dá conta de pintar a tela em DPR 2 | Limitar o DPR (`maxDpr` no `index.html`, padrão 2) a 1,5, ou só no canvas da Unity |
| **Unity** com muitos ms | Algo da cena do AR: shaders (aura, buraco na parede), sombras, UI | Perfilar a Unity (Development Build + Profiler conectado) |
| Tudo bom no `debug`, mas parece travado | Aquecimento ou economia de bateria, ou o FPS medido não é o que se vê | Testar com o celular frio e fora do modo de economia |

Depois de corrigir:

1. Gerar o build (veja “Build” no `CLAUDE.md`).
2. Copiar `Builds/FreeWebAR` para o `pucmon.dev`, sem apagar o `.git` dele.
3. Deixar o commit e o push com o Igor.

As chaves `?debug`, `?nopix` e `?dpr` podem ficar no código: sem elas, nada muda.

## Ofertas esperando resposta do Igor

- **Comprimir as texturas dos modelos:**
  - Hoje o glTFast as embute sem compressão: ~190 MB de memória de vídeo com os 28 carregados.
  - Com os modelos sob demanda, só pesam os que já apareceram.
  - Muda a qualidade, então **só com aprovação**.
- **Decisões assumidas da raid** (plano, seção 0): tamanho da sala, nível 0 só Protege, fim da raid e placar. Ele
  pode mudar qualquer uma.
- **Fase 3** (raid solo offline com a G-HYDRA): é a próxima do plano, depois do FPS. Falta a arte da G-HYDRA, e talvez
  seja preciso simplificar a mecânica das ações.

## O que só existia no PC de mesa (e não vem pelo git)

- **`Builds/`**: os builds ficam fora do git. Gere de novo no notebook.
- **A cópia de rascunho do projeto** (`%TEMP%\claude\…\scratchpad\proj`):
  - servia para gerar builds em lote sem fechar o Unity aberto do Igor;
  - também tinha remendos de teste: Supabase apontando para o falso, login automático e captura sem redirecionar
    para o login.
  - Como refazer está no `CLAUDE.md`, “Testar no PC”.
- **Os servidores de teste** já estão no repo:
  - `Supabase/mock-supabase.mjs`;
  - `Docs/artigo/reproducao/devserver.mjs` e `fakecam3d.js` (câmera simulada).
- **A memória do Claude** no PC de mesa: as regras importantes dela estão no `CLAUDE.md`, em “Regras de trabalho”.
