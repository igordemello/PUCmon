# PUCmon: os 3 modos de jogo

## Ajustes no protótipo (antes de tudo)

O jogo deve seguir o documento do PUCmon nestes pontos:

| Ajuste | Hoje no jogo | Como deve ficar | Estado |
|---|---|---|---|
| Ações por nível | Cada PUCmon tinha 5 ações: a 1ª já vinha no nível 0, as outras quatro nos níveis 1 a 4, e a passiva no nível 5 | Como na tabela do documento: **o nível 0 não libera nada**, os níveis 1 a 4 liberam uma ação cada e o 5, a passiva | **Feito** |
| Nome do professor | “PROF. BELTRAN” | “**DR. BELTRAN O.**”, como no documento | **Feito** |
| Economia | Nível máximo em ~2 dias cuidando sem parar (~2 semanas no ritmo normal) | Nível máximo em ~4 h, com os números num asset fácil de ajustar | **Feito** |

- **Ações por nível** (feito):
  - Cada PUCmon tem agora 4 ações. Saiu a 1ª de cada um (a básica), e as outras desceram um nível.
  - Na raid, um PUCmon no nível 0 não tem ação e só pode **Proteger**.
  - Mudaram o `Catalogo` (28 PUCmons), a Ficha e o aviso de nível novo (`CareScreen`).
  - Com a economia nova, o nível 1 chega já no primeiro cuidado, então a 1ª ação continua vindo logo.
- **Nome do professor** (feito):
  - “DR. BELTRAN O.” fala em todos os diálogos: a primeira captura (`CaptureScreen`, na `SampleScene`), o `Lab Intro`
    dos 28 PUCmons no `Catalogo` e o texto padrão do diálogo nas cenas.
  - Quando o jogador o chama de “professor” numa fala, ficou assim: é como aluno chama.
- **Economia** (feito):
  - Fica no asset **`Assets/Jogo/Resources/Economia`**, com campos de nome em português e dica ao passar o mouse:
    - afinidade de cada nível;
    - horas para a fome e o humor zerarem;
    - quanto cada cuidado rende e enche;
    - intervalo do carinho.
  - O Inspector mostra quanto tempo um PUCmon leva até cada nível com os números de agora.
  - Depois de mudar, clique em **Atualizar o servidor** e cole o SQL no Supabase (SQL Editor > New query > Run).
    O botão também grava o `Supabase/4-economia.sql`.
  - Com os números padrão, cuidando sempre que rende:
    - nível 1 no primeiro cuidado;
    - nível 2 em 20 min;
    - nível 3 em 1h14;
    - nível 4 em 2h52;
    - nível 5 em 3h50.
  - PUCmons já registrados mantêm a afinidade e, com os níveis mais baixos, sobem de nível na hora.

> **Status:** plano alinhado à especificação de gameplay. Só o essencial para ela funcionar está nas seções 1 a 6;
> o resto ficou no **Plano futuro** (seção 8). Números são pontos de partida para balancear.

## 0. Especificação e decisões

A gameplay tem **3 modos**: **Registro** (AR), **Laboratório** (tamagochi) e **Raid** (contra a G-HYDRA). No app, "capturar" virou **registrar** e a coleção virou o **Laboratório** (com busca).

| Especificação | Onde | Estado |
|---|---|---|
| Captura pela câmera, jogando pedras até nocautear o PUCmon | Seção 2 | **Implementado** (fase 1) |
| Coleção: interagir e cuidar dos PUCmons capturados | Seção 4 | **Implementado** (fase 2) |
| Afinidade (XP) sobe o nível de amizade, de 0 a 5; não aumenta stats | Seção 4 | **Implementado** (fase 2) |
| Nível 0 não destrava nada; os níveis 1 a 4 destravam uma ação cada; o 5 destrava a passiva | Seções 3 e 4 | **Implementado** na Coleção (fase 2); o uso na raid vem na fase 3 |
| Começa no nível 0, porque você o apagou na pedrada | Seção 4 | **Implementado** (fase 2) |
| Diálogo com o Dr. Beltran O. na primeira captura | Seção 2 | **Implementado** |
| Raid solo com até 4 PUCmons, ou sala com código, 1 PUCmon por jogador | Seção 5 | A fazer (fases 3 e 4) |
| Chefão: G-HYDRA | Seção 5 | A fazer (fase 3) |
| O dano dado ao chefão é a pontuação da raid, num placar global das melhores raids | Seção 5 | A fazer (fase 4) |

**Decisões assumidas para seguir** (mude qualquer uma que não concordar):

| Decisão | O que foi assumido |
|---|---|
| Tamanho da sala | “Até 6 amigos” = **6 jogadores no total** |
| Nível 0 | O PUCmon ainda não tem ação: na raid, ele só pode **Proteger** |
| Fim da raid | A G-HYDRA **não é derrotada**: a raid dura até 10 rodadas ou até o time cair, e vale o dano causado (senão a pontuação teria teto e os bons times empatariam) |
| Placar | Abas **Solo** e **Grupo** (um grupo de 6 sempre faria mais dano que um solo de 4); **uma linha por equipe**, guardando a melhor raid; **sem zerar** por enquanto |

## 1. Lore

- Cada PUCmon **representa algo da PUC-Rio**: um curso ou outra coisa da universidade.
- A **G-HYDRA**, cobra de três cabeças (G1, G2 e G3), amaldiçoa os PUCmons, que fogem para o nosso mundo pela fenda
  (o buraco na parede).
- A presença dela libera uma **miasma** que se prende nos PUCmons e os deixa violentos: tira o sono e aumenta o
  estresse. No AR, ela aparece como uma aura roxa saindo deles, que se dissipa no nocaute.
- Na primeira visita ao recinto de cada PUCmon, o Dr. Beltran O. conta a história dele (`Lab Intro`, no `Catalogo`).
- O projeto de pesquisa do **Dr. Beltran O.** não tem verba para tranquilizante: o jeito é tacar pedras até
  nocautear o PUCmon e capturá-lo.
- Capturado, ele acorda desconfiado de quem o apedrejou. Conquistar a confiança dele é o tamagochi.
- Um PUCmon que confia em você luta melhor ao seu lado contra a G-HYDRA, e é por isso que a amizade destrava ações.

## 2. Modo 1: Registro (implementado)

- Na primeira vez, o **Dr. Beltran O.** explica a missão (as falas ficam editáveis na `SampleScene`, no `CaptureScreen`).
- Achar o cartaz, ver o PUCmon saindo da fenda e **tacar pedras até nocauteá-lo**.
- Uma **barra de consciência** aparece sobre o PUCmon.
  - Cada pedrada certeira tira um pedaço da barra, e acertar com o anel pequeno tira mais.
  - Quando a barra zera, ele fica tonto, com estrelinhas girando, cai e é capturado.
- Se as pedras da tentativa acabam antes do nocaute, ele foge por alguns segundos e volta com a consciência cheia.
- **Balanceamento no Inspector:**
  - `Throws`, no `CaptureMinigame`: pedras por tentativa;
  - `Ring Bonus`: quanto o anel pequeno aumenta a pedrada;
  - `Knock`, em cada PUCmon do `Catalogo`: quanto de consciência uma pedrada tira. 0,5 = duas pedradas comuns, ou
    uma perfeita.

## 3. PUCmons: representação, atributos e ações

- **Representação** (`Represents` no Inspector): um curso (“Psicologia”) ou qualquer coisa da PUC (“Bandejão”,
  “Biblioteca”, “Pilotis”). As ações e a passiva saem daí.
- **Descrição** (`Description`): o texto da ficha do PUCmon na Coleção.
- **Atributos fixos** da espécie, que nunca crescem:

  | Atributo | Na raid |
  |---|---|
  | Fôlego | vida |
  | Lógica | força das ações de Lógica |
  | Criatividade | força das ações de Criatividade |
  | Resiliência | reduz o dano recebido |
  | Iniciativa | ordem de ação |

  Todos somam 300 pontos (de 40 a 85 cada): a representação muda o formato, não o tamanho.
- **Repertório:** 4 ações (uma em cada nível de 1 a 4) e 1 passiva (nível 5), todas preenchidas no catálogo. O nível
  0 não destrava nada. Ordem sugerida:

  | Nível | Ação |
  |---|---|
  | 1 | de dano |
  | 2 | suporte ou status |
  | 3 | forte com recarga |
  | 4 | assinatura |
  | 5 | passiva |

  Ideias por representação:

  | Representação | Ação ou passiva |
  |---|---|
  | Direito | *Recurso*: anula o dano de um golpe, uma vez por raid |
  | Design | *Rascunho*: copia a última ação de um aliado |
  | Psicologia | *Escuta ativa*: o time ignora o próximo efeito de status |
  | Teologia | *Bênção*: cura 30% de um aliado |
  | Computação | *Debug*: remove status do time |
  | Bandejão | *Prato feito*: cura todo o time um pouco |
  | Biblioteca | *Silêncio!*: o próximo golpe da G-HYDRA causa metade do dano |
  | Pilotis | *Ponto de encontro*: +10% de dano por aliado em campo |

## 4. Modo 2: Laboratório (tamagochi)

### 4.1 Níveis de amizade

| Nível | Nome | Afinidade acumulada | Destrava |
|---|---|---|---|
| 0 | Desconfiado | 0 | nada |
| 1 | Curioso | 50 | 1ª ação |
| 2 | Conhecido | 120 | 2ª ação |
| 3 | Colega | 220 | 3ª ação |
| 4 | Amigo | 340 | 4ª ação |
| 5 | Inseparável | 460 | passiva |

A afinidade de cada nível fica no asset `Economia` (veja o topo).

**A amizade nunca cai.** Tamagochi que pune vira ansiedade.

### 4.2 Necessidades e interações

Duas necessidades dão sentido ao cuidado. Elas mudam com o tempo real, mesmo com o app fechado.

| Necessidade | Muda como | Cuidar |
|---|---|---|
| **Fome** | zera em cerca de 4 h | **Alimentar**: arrastar um lanche até ele |
| **Humor** | zera em cerca de 4 h | **Brincar**: minijogo curto, reaproveitando o arremesso, com uma bolinha de papel. **Carinho**: passar o dedo nele |

Afinidade por interação:

| Interação | Afinidade | Só conta se... |
|---|---|---|
| Alimentar | +30 | Fome abaixo de 80 |
| Brincar | +20 a +40 | Humor abaixo de 80 |
| Carinho | +5 | Intervalo de 10 min |
| Lutar junto numa raid | +10 | Sempre |

- As condições impedem o spam de toques: alimentar um PUCmon cheio não dá nada.
- Cuidando sempre que rende, do nível 0 ao 5 leva cerca de 4 h.
- Todos esses números, menos o da raid, ficam no asset `Economia` (veja o topo).

### 4.3 Telas

- O Laboratório mostra a grade dos PUCmons, com um aviso de quem está com fome ou triste.
- Tocar num PUCmon abre o **Recinto**: o modelo 3D reagindo ao toque (com o `Squish` que já existe), as barras de fome
  e humor, o nível de amizade e os botões das interações.
- A **Ficha** mostra a representação, a descrição, os atributos e as ações, com as trancadas mostrando o nível que
  falta.

## 5. Modo 3: Raid

- **Solo:** até 4 PUCmons seus. **Grupo:** sala de até 6 jogadores, 1 PUCmon cada.
  - Criar sala gera um código de 4 letras e um link para compartilhar.
  - Entrar com código abre a sala.
- **Cada rodada:**
  1. Todos escolhem ao mesmo tempo uma ação destravada ou **Proteger** (no nível 0, só Proteger).
  2. Os PUCmons agem por Iniciativa.
  3. A G-HYDRA ataca.
- **A G-HYDRA:**
  - As três cabeças (G1, G2, G3) se revezam à frente, cada uma com golpes próprios.
  - Ela avisa o próximo golpe uma rodada antes.
  - Fica 10% mais forte a cada rodada.
  - A raid dura até 10 rodadas ou até o time inteiro cair.
- **Pontuação** = dano total causado. O relatório mostra quanto cada um causou.
- **Placar global das melhores raids:** abas Solo e Grupo, uma linha por equipe com a melhor raid dela, e a sua
  melhor raid fixa no rodapé.
- **Contra trapaça:**
  - o servidor (Supabase) decide a raid; o cliente só envia as escolhas;
  - o servidor confere se a ação já foi destravada pela amizade;
  - entrar no placar exige ter capturado pelo menos 2 PUCmons.

## 6. Supabase

```
capturas      (+ afinidade, fome, humor, atualizado_em)   ← já existe, ganha colunas
interacoes    (jogador, pucmon, tipo, afinidade, em)      ← intervalos e condições do tamagochi
salas         (id, codigo, dono, estado, rodada, prazo, estado_luta jsonb)
sala_membros  (sala, jogador, pucmons, pronto)
escolhas      (sala, rodada, jogador, pucmon, acao)
raids         (id, modo, jogadores, equipe, dano_total, membros jsonb, criada_em)   + visão "placar"
especies      (poster, dados jsonb)   ← atributos e ações publicados pelo Unity
```

- **Sem cron:** fome e humor são calculados na leitura, a partir do horário da última atualização.
- **O cliente nunca escreve** afinidade nem resultados de raid. Ele chama funções do servidor (`cuidar`, `raid`).
- **As regras da raid ficam num arquivo JavaScript só,** usado pela Edge Function e pela página WebGL.

## 7. Fases

| Fase | Entrega | Estado |
|---|---|---|
| 1 | **Captura:** pedras até o nocaute, barra de consciência; campos `represents` e `description` | **feito** |
| 2 | **Coleção:** recinto, fome e humor, interações, afinidade e níveis; ações e passiva no catálogo; função `cuidar` no Supabase; Ficha; diálogo do Dr. Beltran O. | **feito** |
| 3 | **Raid solo offline:** regras, cena da raid, G-HYDRA | próxima |
| 4 | **Raid online e placar:** servidor, salas com código, placar Solo/Grupo | |

## 8. Plano futuro

Ideias que não estão na especificação, guardadas para depois:

| Ideia | O que é |
|---|---|
| Estresse | Sobe nas raids e cai com carinho e descanso; dá mais peso ao cuidado |
| Dormir e Conversar | Mais duas interações: descansar e ouvir falas que contam a história do PUCmon por nível |
| Comida favorita e lanches especiais | Cada PUCmon tem uma comida preferida (afinidade em dobro); as raids dão lanches |
| Perfil pela pedrada | Capturar com pedradas perfeitas dá um bônus pequeno (“Nota 10”) |
| Energias (Café / Sono / Prazo) | Triângulo de vantagens contra as cabeças da G-HYDRA |
| Simulado | Raid de treino sem pontuar, como tutorial |
| Procurar grupo | Salas públicas para quem não tem amigos online |
| Abertura com a lore | As imagens do Z1, do Dr. Beltran O. e do projeto PIBIC antes do cadastro |
| Calendário de provas | Na semana de G1 da PUC, a cabeça G1 fica mais forte e ganha placar próprio |
| Temporadas | Zerar o placar por semestre, com hall da fama |
| Eventos | Editais do CNPCNI e da CAPIRAS, manchetes do Z1, pedras cosméticas como recompensa |
| Curso do jogador | Copa dos Cursos: a melhor raid de cada curso |
| Visitas | Ver o recinto do PUCmon de um amigo |
| Lembretes | “Seu PUCmon está com fome” (exige instalar como PWA) |

## 9. Riscos

| Risco | Como tratar |
|---|---|
| Placar tomado por pontuações falsas | Servidor decide a raid |
| Spam de toques no tamagochi | Afinidade só com necessidade real e intervalos, no servidor |
| Online caro e talvez pouco divertido | Raid solo offline primeiro (fase 3) |
| Nomes ofensivos no placar | Filtro no cadastro e botão de denúncia |
| Projeto do Supabase pausa após uma semana sem uso | Reativar antes da volta às aulas |
| Netlify gratuito (por créditos): ~15 GB/mês, e cada publicação gasta o equivalente a 0,75 GB; o build pesa ~28 MB no primeiro acesso; acabou, o site sai do ar até o mês seguinte | Hospedar o build no Cloudflare Pages (sem cota de banda) ou no GitHub Pages; publicar menos vezes |
| Limite de cadastro do Supabase: 30 por 5 min **por IP**, e o Wi-Fi da PUC junta muita gente num IP só | Subir os limites em Authentication > Rate Limits antes de qualquer evento |
| Supabase gratuito não faz backup | Exportar o banco de vez em quando, ou o plano Pro (US$ 25/mês) |
