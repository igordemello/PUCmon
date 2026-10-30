# PUCmon

Jogo de criaturas em **realidade aumentada no navegador**, ambientado na PUC-Rio. Cada PUCmon representa um curso ou
outra coisa da PUC. O jogador aponta o celular para o cartaz dele no campus, nocauteia o PUCmon a pedradas para
registrá-lo, e depois cuida dele no Laboratório (um tamagochi). A raid contra a G-HYDRA ainda está por fazer.

- **O que o jogo deve ter e em que pé está:** `Docs/plano-pos-game.md`. É o documento-guia: siga-o e mantenha a
  coluna de estado em dia.
- **Trabalho em andamento:** `Docs/onde-paramos.md`, se existir. Leia antes de qualquer coisa.
- **Como funciona o AR:** `Assets/FreeWebAR/README.md`.
- **Artigo sobre o rastreador:** `Docs/artigo/`.
- **Ideia hipotética, nada implementado:** `Docs/plano-vr-stand.md`.

## Regras de trabalho

1. **Fale sempre em português (pt-BR)** com o Igor (o usuário): respostas, resumos, avisos curtos e perguntas.
   - Código, nomes e comentários de código ficam em **inglês**, como já está.
   - Textos do jogo, docs, SQL e dicas do Inspector ficam em **português**.
2. **Não faça commit nem push sem ele pedir.** Os commits dele vão direto na `main`, com a mensagem `commit`.
3. **Nunca publique no Netlify.** Cada publicação gasta créditos do plano grátis, e sem créditos o site sai do ar.
   - Builds de teste vão para o GitHub Pages: o repo `pucmon.dev` (`..\pucmon.dev`, ao lado deste) tem na raiz o
     conteúdo de `Builds/FreeWebAR`.
   - Copie o build para lá **sem apagar o `.git`** e deixe o commit e o push com o Igor.
4. **Não teste contra o Supabase de verdade:** não crie contas nem faça login nele. Use o `Supabase/mock-supabase.mjs`
   (veja “Testar no PC”).
   - O SQL de produção é o Igor quem roda, no painel do Supabase.
5. **O Unity do Igor costuma estar aberto neste projeto.**
   - Não o feche nem mate o processo.
   - Para gerar build em lote, use uma **cópia** do projeto (veja “Build”). O Unity trava o projeto aberto.
6. **Siga o estilo do código ao redor.** Código enxuto, `/// <summary>` em inglês simples no topo de cada classe e
   comentário só onde explica um porquê.
7. **Qualidade visual é sagrada.** Otimização “sem mudar a qualidade” quer dizer sem mudar a qualidade: nada de
   comprimir textura, baixar resolução ou simplificar shader sem aprovação explícita.
8. **Mova sempre o `.meta` junto com o asset**, e dentro do Unity quando der. O GUID do `.meta` é o que liga cenas e
   assets.
9. **Não edite o motor vendorizado** (`Assets/WebGLTemplates/FreeWebAR/xr/*.js`). Ele é mantido byte a byte (sem
   CRLF, pelo `.gitattributes`).
10. Antes de dizer que algo funciona, **teste de verdade**:
    - no navegador, com a câmera simulada;
    - nos testes `node` do rastreador;
    - ou num build.

    Se não deu para testar, diga isso.

## Stack

| Peça | O quê |
|---|---|
| Motor | **Unity 2022.3.8f1** (`C:\Program Files\Unity\Hub\Editor\2022.3.8f1\Editor\Unity.exe`), alvo **WebGL** |
| AR | Motor aberto da **8th Wall** (MIT, vendorizado em `xr/`) + o **nosso rastreador denso** (`tracker.js`), num template WebGL próprio (`FreeWebAR`). Não usa Zappar nem serviço pago |
| Modelos 3D | `.glb` pelo **glTFast 6.18** |
| UI | uGUI + **TextMeshPro** |
| Back-end | **Supabase**: Auth com usuário e senha, tabela `capturas` com RLS e a função `cuidar` |
| Hospedagem | **Netlify** para a versão oficial (plano grátis por créditos) e **GitHub Pages** (`pucmon.dev`) para testes |
| Player Settings | Template `PROJECT:FreeWebAR`, compressão Gzip com *Decompression Fallback*, cache de dados ligado |

## Mapa do repositório

```
Assets/
  Conta/            conta e página: Account (login, coleção, cuidar), Supabase (HTTP), LoginScreen,
                    Page(.jslib) (tela de carregamento da página), WebTextInput(.jslib) (teclado do celular
                    em campos TMP), Squish (botão que afunda)
  Jogo/             o jogo: HomeScreen (início, Laboratório, busca), CaptureScreen (UI do AR) +
                    CaptureMinigame (pedradas), CareScreen (recinto/tamagochi e Ficha), Bond (amizade
                    de 1 PUCmon), Catalog + Catalogo.asset (os 28 PUCmons), Economy +
                    Resources/Economia.asset (números do cuidado), Dialogue (falas do Dr. Beltran O.),
                    CreatureCard, Drift, LazyModel
    Editor/         NewPucmon (menu PUCmon > Novo PUCmon...), PortraitMaker (retratos 2D; menu
                    PUCmon > Gerar imagens 2D), EconomyEditor (Inspector da economia + SQL),
                    LazyModelBuild (modelos sob demanda no build)
    Retratos/       retratos pixel-art gerados pelo PortraitMaker (não edite à mão)
    UI/             sprites da interface
  FreeWebAR/        ponte Unity <-> página: ImageTarget, WebARImageTracker (na AR Camera),
                    FreeWebAR.jslib, Editor/ImageTargetBuild (dados de rastreio no build),
                    Tests~/ (testes node do rastreador e da ponte), README.md
  WebGLTemplates/FreeWebAR/
                    a página: index.html (câmera, pipeline da 8th Wall, rastreador, debug, tela de
                    carregamento), tracker.js (rastreador denso), freewebar.js (conversão de pose,
                    escolha de lente), xr/ (motor 8th Wall, NÃO editar), fonts/
  Efeitos/          WallHole (buraco na parede, gerado em código), shaders Aura, DepthMask, WallHole
  ImagesToTracker/  os cartazes (PNG); o NOME do arquivo é o id do PUCmon em todo lugar
  Modelos/Resources/ os .glb dos PUCmons (em Resources para carregar sob demanda)
  Scenes/           Login, Inicio, SampleScene (a cena do AR)
Supabase/           SQL na ordem: schema.sql, 2-colecao.sql, 3-nomes.sql, 4-economia.sql (gerado);
                    mock-supabase.mjs (Supabase falso para testes)
Docs/               plano-pos-game.md (+ .pdf, que pode estar desatualizado), onde-paramos.md,
                    plano-vr-stand.md, artigo/ (artigo do rastreador; reproducao/ tem o devserver
                    e a câmera simulada)
Builds/             saída dos builds (fora do git)
```

## Arquitetura

### Cenas e fluxo

- **`Login`** (`LoginScreen`):
  - abas Entrar e Criar conta;
  - se o aparelho lembra uma sessão, entra direto e vai para `Inicio`.
- **`Inicio`** (`HomeScreen`):
  - **Registrar** abre a câmera e carrega a `SampleScene`;
  - **Laboratório** mostra a grade da coleção (`CreatureCard`, com busca sem acento);
  - no Laboratório, o **Recinto** (`CareScreen`) tem alimentar, brincar e carinho, e a **Ficha** mostra a
    representação, a descrição e as ações, com as trancadas mostrando o nível que falta.
  - `HomeScreen.openCollection = true` antes de carregar a cena abre direto no Laboratório.
- **`SampleScene`** (`CaptureScreen` + `CaptureMinigame`):
  - tem 28 `ImageTarget`s, um por cartaz, cada um com o modelo saindo do buraco na parede (`WallHole`);
  - na primeira vez, o Dr. Beltran O. explica a missão;
  - pedradas tiram a barra de consciência, e no nocaute o PUCmon é registrado (`Account.Capture`).
- **Tela de carregamento:** cada cena chama `Page.Ready()` quando sabe o que mostrar. Até lá, a tela de carregamento
  da página cobre tudo.

### Realidade aumentada (página + Unity)

1. A página (`index.html`) roda o pipeline da 8th Wall:
   - `GlTextureRenderer` desenha a câmera atrás do canvas transparente da Unity;
   - `XrController` detecta as imagens, com `disableWorldTracking`;
   - `CameraPixelArray` copia a luminância do quadro, em até 480 px, para o nosso rastreador;
   - o `unityBridge` recebe o resultado.
2. A 8th Wall **encontra** o cartaz. O `tracker.js` o **segue** quadro a quadro com precisão sub-pixel (Lucas–Kanade
   inverso-composicional sobre a homografia, com pesos de Huber).
   - A pose é calculada sobre o mesmo quadro mostrado na tela.
   - Ela vai para `window.freeWebARPose` (27 floats).
3. O `WebARImageTracker`, na AR Camera, lê a pose pelo `FreeWebAR.jslib` a cada quadro, aplica a projeção e move ou
   ativa o `ImageTarget` do cartaz visto. Um cartaz por vez.
   - O espaço do alvo é igual ao da Zappar: a imagem centrada, com 2 unidades de altura, e o conteúdo em -Z local.
4. Os dados de rastreio são gerados no build pelo `ImageTargetBuild` (`image-targets/targets.json` + luminância de
   cada cartaz). Não há lista para manter.
5. **Desempenho**, já otimizado:
   - `WebARImageTracker.OpenCamera()` abre a câmera no toque em Registrar, enquanto a cena carrega;
   - cada cartaz só ganha seu rastreador denso quando é visto pela primeira vez (`prepare` no `index.html`);
   - no build, o `LazyModelBuild` troca cada modelo da cena por um `LazyModel`, que faz `Resources.LoadAsync` do
     `.glb` na primeira vez que o cartaz aparece. A cena salva no disco continua com os modelos.
   - `CaptureScreen` não começa o minijogo antes de o modelo estar pronto (`LazyModel.IsReady`).
6. **Câmera:**
   - Em celular com várias lentes, `freewebar.js` (`mainBackCamera`) escolhe a lente principal pelo nome, em
     qualquer idioma.
   - Quem escolhe a resolução é o motor (`xr/xr.js`):
     - Android Chrome pede 960x720 exatos;
     - iOS pede `min` 960x720;
     - alguns Samsung pedem 640x480.

### Dados e regras

- **O nome do arquivo do cartaz é o id do PUCmon** em todo lugar: no `Catalogo`, no Supabase (`capturas.criatura`) e
  nos dados de rastreio. Renomear um cartaz exige migrar o Supabase (exemplo: `3-nomes.sql`).
- **`Catalogo.asset`** (`Catalog`): para cada PUCmon,
  - nome, `represents` (o curso ou a coisa da PUC) e descrição;
  - **4 ações, nos níveis 1 a 4, e 1 passiva no nível 5** (o nível 0 não libera nada), por enquanto só com nome e
    descrição;
  - `knock`, `agility` e `shyness` do minijogo;
  - `Lab Intro`, o diálogo da primeira visita ao recinto;
  - o retrato.
  - O `PortraitMaker` põe no catálogo os PUCmons novos da cena e refaz os retratos quando a cena é salva.
- **Economia** (`Economy`, asset `Assets/Jogo/Resources/Economia`, lido com `Economy.Current`):
  - guarda a afinidade de cada nível, as horas para a fome e o humor zerarem, quanto cada cuidado rende e o
    intervalo do carinho;
  - os campos têm nome em português (`[Label]`) para quem balanceia;
  - o Inspector (`EconomyEditor`) mostra quanto tempo leva até cada nível;
  - o botão **Atualizar o servidor** gera o `Supabase/4-economia.sql` e o copia. **Cliente e servidor precisam bater:**
    depois de mudar o asset, rode o SQL no Supabase.
- **`Bond`** é uma linha de `capturas` (afinidade, fome, humor, `atualizado_em`, `carinho_em`).
  - Fome e humor caem com o tempo a partir de `atualizado_em`, calculados na leitura e sem cron, pela mesma regra
    no C# e no SQL.
  - A amizade nunca cai.
- **Conta** (`Account`, `Supabase`):
  - O jogador só tem um nome de usuário. O Auth do Supabase recebe `nome@pucmon.invalid`, então o “Confirm email”
    precisa ficar desligado.
  - A chave no código é a *publishable*, feita para ir no app. Quem protege os dados é a RLS.
  - **O cliente nunca grava afinidade:**
    - o insert em `capturas` só aceita a coluna `criatura`;
    - o resto passa pela função `cuidar(p_criatura, p_tipo, p_desempenho)` (`security definer`).
  - A sessão fica no `PlayerPrefs` (refresh token). Se ela for perdida de vez, o jogo volta para o `Login` com
    `Account.Notice`.
- **SQL**, na ordem: `schema.sql`, `2-colecao.sql` e `3-nomes.sql` rodam uma vez. O `4-economia.sql` roda de novo a
  cada mudança na economia. Quem roda é o Igor, em *SQL Editor > New query > Run*.

## Receitas

### PUCmon novo

Use o menu **PUCmon > Novo PUCmon...**, com nome, “representa”, descrição, o `.glb` e o cartaz. Ele:

- move os arquivos para `ImagesToTracker` e `Modelos/Resources` com o nome do PUCmon;
- cria a entrada no `Catalogo`;
- monta o `ImageTarget` na `SampleScene`, copiando o layout de um existente: modelo do mesmo tamanho, no mesmo
  lugar, e buraco na parede com outra semente;
- salva a cena, o que gera o retrato.

Depois preencha à mão no `Catalogo` as ações, a passiva, os atributos e o `Lab Intro`. O cartaz precisa ter no mínimo
480x640 px, e quanto mais bordas e detalhe, mais rápido ele é encontrado.

### Mudar a economia

1. Edite `Assets/Jogo/Resources/Economia` no Inspector.
2. Clique em **Atualizar o servidor**.
3. Avise o Igor para colar o SQL no Supabase.
4. Se os números padrão mudarem, atualize também o `Supabase/mock-supabase.mjs`: a regra está fixa lá, com fome e
   humor em 4 h, alimentar +30 abaixo de 80, brincar +20 a +40 e carinho +5 a cada 10 min.

### Build

- **No Unity:** *File > Build Settings > WebGL > Build*, para `Builds/FreeWebAR`. As cenas são `Login`, `Inicio` e
  `SampleScene`.
- **Em lote, sem fechar o Unity do Igor:**
  1. Espelhe o projeto numa cópia, sem `Library/` do projeto aberto e sem `Builds/`. O robocopy `/MIR` serve: saída
     menor que 8 é sucesso.
  2. Nessa cópia, ponha um script de editor:
     ```csharp
     static class Prep {
         static void Release() {
             var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
             var r = BuildPipeline.BuildPlayer(scenes, "Builds/FreeWebAR", BuildTarget.WebGL, BuildOptions.CleanBuildCache);
             EditorApplication.Exit(r.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
         }
     }
     ```
  3. Rode:
     ```bash
     "/c/Program Files/Unity/Hub/Editor/2022.3.8f1/Editor/Unity.exe" -batchmode -nographics -buildTarget WebGL -projectPath "<cópia>" -executeMethod Prep.Release -logFile "<log>"
     ```
  4. No log, procure `error CS` e a linha `LazyModel: 28 model(s) of SampleScene load when their poster is seen.`
  5. Copie o `Builds/FreeWebAR` da cópia de volta.
- **Use `BuildOptions.CleanBuildCache`** (ou *Clean Build*). O Unity pode pular o `OnProcessScene` de cenas que não
  mudaram, e aí o `LazyModelBuild` e o `ImageTargetBuild` não rodam.
- O build tem ~30 MB.

### Testar no PC (sem celular e sem o Supabase de verdade)

1. **Supabase falso:**
   ```bash
   node Supabase/mock-supabase.mjs 8768
   ```
   Qualquer login entra e os dados ficam na memória. `POST /__seed` muda o estado de uma captura.
2. **Numa cópia do projeto, nunca no original**, aplique os remendos de teste:
   - `Supabase.Url = "http://127.0.0.1:8768"`;
   - se quiser pular telas, login automático no `HomeScreen` (`SignIn("teste","teste123")`) e `CaptureScreen` sem
     redirecionar para o `Login`.

   Depois gere o build, por exemplo em `Builds/Teste`, com `Inicio` e `SampleScene`.
3. **Servidor com câmera simulada**, com a raiz no projeto:
   ```bash
   node Docs/artigo/reproducao/devserver.mjs . 8769
   ```
4. Abra `http://localhost:8769/Builds/Teste/index.html?fake3d&img=/Assets/ImagesToTracker/<cartaz>.png&debug`.
   - O `?fake3d` injeta o `fakecam3d.js`: um cartão 3D com ruído e tremor que substitui o `getUserMedia`.
   - Também aceita `noise`, `camfov` e `land`.
5. **Testes do rastreador e da ponte:**
   ```bash
   node "Assets/FreeWebAR/Tests~/tracker.test.mjs"
   ```
   ```bash
   node "Assets/FreeWebAR/Tests~/freewebar.test.mjs"
   ```

**No painel do navegador do Claude Code** (emulação de celular):

- os cliques do `computer` às vezes não chegam ao canvas da Unity: dispare `pointerdown`, `pointerup`, `mousedown`,
  `mouseup` e `click` por JS nas coordenadas CSS;
- screenshots podem vir com um quadro velho: confira o DOM antes de concluir.

### Testar no celular

- **Caminho normal:**
  1. Gere o build.
  2. Copie `Builds/FreeWebAR` para o `pucmon.dev`.
  3. O Igor faz commit e push, e o GitHub Pages publica.
- **Alternativas sem publicar** (README do FreeWebAR):
  - `cloudflared tunnel` (precisa instalar, então peça antes);
  - Android no cabo, com `chrome://inspect` e port forwarding.
- **Chaves do link**, que se juntam com `&`:
  - `?debug`: câmeras, estado do rastreador, FPS da câmera, do motor e da Unity, e ms por quadro;
  - `?cam=N`: força a câmera N;
  - `?nopix`: sem cópia do quadro para o rastreador denso;
  - `?dpr=N`: limita a densidade de pixels (o padrão é até 2).

## Armadilhas conhecidas

- **`PrefabUtility` não funciona no `OnProcessScene` durante o build:** os vínculos de prefab já sumiram da cópia da
  cena. O `LazyModelBuild` reconhece os modelos pela malha (sub-asset de um arquivo em `/Resources/`) e por
  comparação estrutural (`SameAs`).
  - Um modelo **mexido na cena** além de posição, rotação e escala continua carregando com a cena, com um aviso no
    log.
- **O glTFast embute as texturas do `.glb` sem compressão** (RGBA32): ~190 MB com os 28. A compressão do
  `TextureImporter` não vale para elas. Comprimir muda a qualidade, então só com aprovação.
- **CRLF:**
  - O git avisa “LF will be replaced by CRLF” em vários arquivos, e isso é normal.
  - Nos `xr/*.js` não pode acontecer (`-text` no `.gitattributes`).
  - No `pucmon.dev`, os `xr/*.js` e a fonte às vezes aparecem como modificados só por fim de linha.
- **Ferramenta Bash do Claude Code:** barras invertidas em heredoc de Python (`\\n`) chegam corrompidas. Escreva o
  script num arquivo e rode o arquivo.
- **YAML de cena e asset:** editar à mão funciona, mas preserve `fileID` e GUIDs e confira no Unity depois. Prefira
  scripts de editor.
- **Suporte da câmera:** só abre em HTTPS ou `localhost`.
- **Limites dos serviços grátis:**
  - Netlify: 300 créditos por mês, 15 por publicação de produção e 20 por GB de banda.
  - Supabase:
    - **pausa após 1 semana sem uso**;
    - não faz backup;
    - limita o Auth **por IP** (cadastro 30 a cada 5 min, login 150 a cada 5 min). O Wi-Fi da PUC junta muita gente
      num IP: suba os limites antes de qualquer evento.
  - Cloudflare Pages é a saída se a banda apertar: banda estática sem cota e limite de 25 MiB por arquivo.

## Estado atual (30/10/2026)

- **Fases 1 (Registro) e 2 (Laboratório) feitas.** Os ajustes do protótipo também: ações por nível, Dr. Beltran O. e
  economia de ~4 h.
- **Em andamento:** FPS baixo (5–10) na tela da câmera no celular. O diagnóstico com `?debug` está pronto e falta a
  medição no aparelho. Veja `Docs/onde-paramos.md`.
- **Próximo:** fase 3, a raid solo offline contra a G-HYDRA (plano, seções 3 e 5).
