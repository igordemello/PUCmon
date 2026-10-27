# PUCmon pós-registro: o Projeto PUCmon contra a G-HYDRA

> **Status: proposta para discussão.** Já implementados: os campos `course` e `description` de cada criatura no
> `Catalogo`. Números (atributos, pontos, tempos) são pontos de partida para balancear, não valores finais.

## 0. Resumo

1. **O cartaz serve só para registrar.** Tudo o que vem depois acontece no app, em qualquer lugar.
2. **A lore vira o jogo.** O jogador é bolsista de iniciação científica (PIBIC) do Projeto PUCmon do Prof. Dr. Beltran
   O. Registrar é a pesquisa de campo. O pós-registro é o projeto partindo para a ofensiva contra a **G-HYDRA**, a
   origem da maldição.
3. **A batalha é o Modo Raid:**
   - **solo**, com até 4 PUCmons registrados;
   - **em grupo**, com até 6 pessoas numa sala criada ou acessada por código, cada uma com 1 PUCmon.
4. **Pontos do ranking = dano causado ao chefão.**
   - Inclui o dano que você *possibilitou* (buffs e debuffs), para quem joga de suporte também pontuar.
   - No solo, conta o dano médio por PUCmon, para não valer 4× mais que jogar em grupo.
5. **Os chefões são as cabeças da G-HYDRA** e seguem o calendário real da PUC: a cabeça **G1** aparece na semana de
   G1, depois a **G2** e a **G3**. No fim do semestre vem a **G-HYDRA completa**, com uma barra de vida comunitária que
   o campus inteiro derruba junto.
6. **A raid tem “tempo de prova”:** 8 rodadas para derrubar o chefão. Quem derruba é **APROVADO** (bônus); quem não
   derruba ainda ganha os pontos do dano causado.
7. **PUCmons evoluem lutando:** nível = período (1º ao 10º, com formatura no fim), créditos para distribuir, uma
   eletiva aprendida com outro PUCmon registrado e um teto de crescimento.
8. **Estresse:** lutar contra a G-HYDRA estressa o PUCmon, que precisa descansar no laboratório. É o limite natural de
   partidas por dia, justificado pela própria lore, e premia quem registrou mais PUCmons.
9. **Sem PvP, sem combinação de resultado.** Como todos lutam contra o chefão, o maior vetor de trapaça de ranking
   (perder de propósito para um amigo) deixa de existir.
10. **Ordem:** primeiro a lore no app (pedra, “registrar”, apresentação do Beltran) e a Ficha; depois a raid solo
    offline; por fim as salas online e o ranking.

---

## 1. A lore e como cada parte vira mecânica

**O que entendi:**
- Existe um mundo de fauna mística, e cada criatura representa um curso da PUC-Rio.
- A **G-HYDRA**, uma cobra de três cabeças (as provas G1, G2 e G3), amaldiçoa e estressa essas criaturas. Para fugir
  da influência dela, elas atravessam para o nosso mundo.
- Ao chegar, a única forma de acalmá-las é registrá-las. Como a verba do projeto é curta e não dá para comprar
  tranquilizante, o método oficial é **tacar uma pedra** para desmaiá-las.
- A universidade, com o CNPCNI e a CAPIRAS, abriu o **Projeto PUCmon** (PIBIC), coordenado pelo **Prof. Dr. Beltran O.,
  Doutor em Criaturas Crípticas**, e a notícia saiu no **Z1**.

| Elemento da lore | Vira no jogo |
|---|---|
| O buraco na parede (`WallHole`, já existe) | A **fenda** por onde o PUCmon atravessa de mundo |
| A pedra | O arremesso do minijogo de registro (substitui a carteirinha; ver seção 10) |
| Registrar | O verbo do jogo: “PUCmons registrados”, “Fichário”, “Ficha de registro” |
| Prof. Beltran | Narrador, tutorial e “cara” do hub, que passa a ser o **Laboratório** |
| G-HYDRA e as cabeças G1/G2/G3 | Os chefões das raids, no ritmo do calendário de provas |
| A influência que faz mal | O **Estresse** de cada PUCmon (limite de raids, descanso no laboratório) |
| CNPCNI e CAPIRAS | Avisos e **editais** de eventos (“Edital CAPIRAS 03/2026: G2 avistada, pontos ×2”) |
| Z1 | Notícias da temporada (manchetes na abertura do app e para as redes sociais) |
| Verba curta | Humor e cosméticos: pedras especiais como recompensa de temporada |
| PIBIC, carreira acadêmica | Ligas: Bolsista IC → Mestrando → Doutorando → Pós-doc → Doutor em Criaturas Crípticas |

**Por que os PUCmons lutam se a G-HYDRA lhes faz mal:** depois de registrados e calmos, eles ficam protegidos no
laboratório e voltam mais fortes. Enfraquecer a G-HYDRA alivia a maldição sobre os que ainda estão do outro lado. É um
motivo nobre e cabe numa fala do Beltran.

## 2. O cartaz é só para registrar

- AR continua exatamente como hoje: achar o cartaz, ver o PUCmon saindo da fenda, tacar a pedra e registrar.
- Depois de registrado, o cartaz mostra o que já mostra (“Você já registrou X”). Não há estudo, guardião nem nada que
  obrigue a voltar ao cartaz.
- O único efeito do registro no pós-game é o **perfil** do PUCmon, dado pela qualidade do arremesso (seção 5.4).

## 3. O jogador é bolsista, e tem curso

- No cadastro: usuário, senha e **curso** (lista, com “Outro / visitante”). Dá para trocar uma vez por temporada.
- O curso entra na **Copa dos Cursos**: a média dos pontos dos 10 melhores bolsistas de cada curso na temporada (só
  aparece com pelo menos 5 jogadores). A média dos melhores é mais justa que a soma, que favoreceria só cursos grandes.
- Se o curso do jogador tem um PUCmon, o Beltran sugere ir atrás dele primeiro.

## 4. PUCmons: atributos

### 4.1 Os cinco atributos

| Atributo | Na raid | Tema |
|---|---|---|
| **Fôlego** | vida (PV = 2 × Fôlego + 50) | aguentar a semana de provas |
| **Lógica** | força dos golpes de Lógica | raciocínio, cálculo, argumentação |
| **Criatividade** | força dos golpes de Criatividade | expressão, intuição, arte |
| **Resiliência** | reduz o dano recebido e o estresse ganho | calma sob a influência da G-HYDRA |
| **Iniciativa** | ordem de ação na rodada | proatividade |

- Todos os PUCmons somam **300 pontos** de atributos-base (de 40 a 85 cada). O curso define o formato, não o tamanho:
  ninguém deve sentir que “o curso dele é fraco”.
- Cada PUCmon tem uma **especialidade** (passiva) e um **golpe assinatura** ligados ao curso. Como agora o curso está
  no campo `course`, a tabela abaixo é só um exemplo de perfis. Os de verdade saem do que você preencher.

| Papel | Fôl | Lóg | Cri | Res | Ini | Exemplo de especialidade |
|---|---|---|---|---|---|---|
| Tanque | 80 | 50 | 55 | 75 | 40 | *Fé inabalável*: uma vez por raid, sobrevive com 1 PV |
| Velocista | 50 | 55 | 70 | 45 | 80 | *Furo de reportagem*: sempre age primeiro na 1ª rodada |
| Especialista | 55 | 40 | 85 | 55 | 65 | *Tinta*: golpes de Criatividade reduzem a precisão do chefão |
| Controle | 55 | 45 | 75 | 60 | 65 | *Insight*: imune ao primeiro efeito de status |
| Estrategista | 60 | 70 | 55 | 65 | 50 | *Citação*: golpes de Lógica ignoram 20% da Resiliência |
| Equilibrado | 60 | 60 | 60 | 60 | 60 | *Espírito PUC*: +10% de dano aos aliados de cursos diferentes |

Banco de especialidades por curso, para quando entrarem PUCmons novos:

| Curso | Especialidade ou golpe |
|---|---|
| Engenharia | *Cálculo estrutural*: protege um aliado do próximo golpe |
| Ciência da Computação | *Debug*: remove status do time · *Loop*: repete o último golpe com +30% |
| Direito | *Recurso*: uma vez por raid, anula o dano de um golpe do chefão |
| Administração | *Delegar*: passa os próprios bônus para um aliado |
| Economia | *Juros compostos*: +10% de dano a cada uso seguido do mesmo golpe |
| Medicina | *Plantão*: cura um pouco do time no fim de cada rodada |
| Psicologia | *Sessão*: reduz o estresse ganho pelo time nesta raid |
| Design | *Rascunho*: copia o último golpe de um aliado |
| Teologia | *Bênção*: cura 30% de um aliado |
| Filosofia | *Dúvida metódica*: anula os bônus do chefão |
| Matemática | *Prova por absurdo*: dano dobrado se o chefão tiver mais de 50% da vida |
| História | *Lição do passado*: repete o golpe que mais causou dano na raid |
| Artes Cênicas | *Improviso*: um golpe aleatório forte |
| Relações Internacionais | *Diplomacia*: o próximo ataque do chefão mira em quem tem mais vida |

### 4.2 Energias (o “tipo”)

Um triângulo neutro, da vida de estudante, sem hierarquizar cursos:

```
        CAFÉ ──vence──▶ SONO        Café acorda quem dorme
          ▲               │
        vence           vence       Quem dorme nem liga para o prazo
          │               ▼
          └──────────── PRAZO       Nenhum café salva prazo estourado
```

Na raid, isso dá estratégia de time: cada cabeça da G-HYDRA tem uma energia (e muda de fase), então vale levar PUCmons
variados. Vantagem: ×1,5; desvantagem: ×0,67. Golpe da mesma energia do PUCmon: ×1,2.

### 4.3 O que entra no catálogo (Unity)

- **Já existem:** `course` e `description`.
- **Para a raid:** `energy`, os 5 atributos-base, a especialidade e os golpes. Você continua balanceando pelo
  Inspector.
- Como o servidor precisa dos mesmos números, um menu **“PUCmon > Publicar balanceamento”** envia o catálogo para uma
  tabela `especies` no Supabase (seção 7.4).

## 5. Evolução sem cartaz

### 5.1 Níveis = períodos

- Todo PUCmon registrado começa no **1º período** e vai até o **10º**. XP por período: 200, 300, 400...
- Cada período aumenta os atributos-base em 4% e dá **3 créditos** para distribuir, com no máximo +15 por atributo.
- **10º período: formatura.** Capelo e beca por cima do mesmo modelo, e o `PortraitMaker` refaz o retrato “formado”.
- **Teto:** um formado tem no máximo cerca de 45% a mais que um calouro, para a escolha de time e de golpes pesar mais
  que o grind.

### 5.2 De onde vem o XP

| Fonte | XP |
|---|---|
| Participar de uma raid | +40 para cada PUCmon que lutou |
| Aprovado (chefão derrubado) | +40 extra |
| Dano causado | +1 por 100 de dano |
| Missões do Beltran (3 por dia) | +150 cada (ex.: “lute com um PUCmon de Prazo”, “aprove na G1 em grupo”) |

### 5.3 Eletiva por monitoria

A partir do 4º período, cada PUCmon tem um slot de **eletiva**: pode aprender o golpe assinatura de **outro PUCmon que
você registrou**, desde que esse esteja no 5º período ou mais (ele é o “monitor”). Isso:

- dá valor a registrar todos os PUCmons, mantendo o cartaz só para registro;
- faz dois PUCmons iguais de jogadores diferentes jogarem diferente, o que é essencial com poucos PUCmons;
- combina com o tema: interdisciplinaridade.

### 5.4 Perfil, dado pela pedrada

A qualidade do arremesso no registro (o tamanho do anel) define o perfil do PUCmon:

| Arremesso | Perfil | Efeito |
|---|---|---|
| anel no mínimo | **Nota 10** | +5% em dois atributos |
| anel pequeno | **Dedicado** | +5% em um atributo |
| normal | **Regular** | nada |

Os PUCmons registrados antes disso recebem um perfil sorteado.

### 5.5 Estresse

- Cada raid soma **35% de estresse** a cada PUCmon que lutou (menos, com Resiliência alta ou a especialidade de
  Psicologia). Com 100%, o PUCmon fica **esgotado** e não pode lutar até descansar.
- Descanso no laboratório: −10% por hora (de esgotado a pronto em cerca de 10 h, ou seja, uma noite).
- **Efeito:** cerca de 3 raids por PUCmon por dia. Quem registrou 6 PUCmons joga muito mais que quem registrou 1, o que
  dá mais um motivo para ir atrás de todos os cartazes. A tela mostra “volta em 2 h” para dar motivo de retorno.
- **Ficou para depois, se for necessário:** consumíveis que reduzem estresse (o “café” da verba curta).

## 6. Modo Raid

### 6.1 Formato

| | Solo | Grupo |
|---|---|---|
| Jogadores | 1 | até 6 por sala |
| PUCmons | até 4 seus, controlados por você | 1 por jogador |
| Como entra | “Raid solo” | **Criar sala** (gera código de 4 letras) ou **Entrar com código** |
| Tempo por rodada | sem limite | 20 s |
| Vida do chefão | escala com o número de PUCmons em campo | idem |

### 6.2 A luta: uma prova de 8 questões

- A raid tem **8 rodadas (“questões”)**. Derrubou o chefão antes do fim: **APROVADO**. O tempo acabou: **REPROVADO**,
  mas o dano causado vale do mesmo jeito.
- **Cada rodada:**
  1. Todos escolhem ao mesmo tempo: um dos 3 golpes do PUCmon ou **Proteger** (reduz o dano recebido e pode cobrir um
     aliado).
  2. Os PUCmons agem por Iniciativa.
  3. Depois, o chefão age.
- **O chefão avisa o próximo golpe** uma rodada antes (“A G2 prepara **Questão discursiva** contra @ana”). É isso que
  dá decisão às rodadas: proteger, curar ou arriscar o dano.
- **Esgotado:** um PUCmon com 0 PV sai da raid. Um aliado pode trazê-lo de volta com o golpe de suporte *Cafezinho*.
- **Dano** = poder do golpe × (Lógica ou Criatividade ÷ Resiliência do chefão) × afinidade × vantagem × sorteio de 0,9
  a 1,1. Crítico: 1 em 16, ×1,5.

### 6.3 Os chefões

| Chefão | Quando aparece | Mecânica |
|---|---|---|
| **Simulado** (cópia de treino da G-HYDRA) | sempre, sem pontos | tutorial, golpes lentos e avisados |
| **G1** | semana de G1 da PUC | Matéria curta: golpes simples, uma fraqueza clara. A raid de entrada |
| **G2** | semana de G2 | *Questão pegadinha*: reflete o próximo golpe de quem ela marcar. *Lista extra*: invoca lacaios (“Exercícios”) que protegem a cabeça |
| **G3** | semana de G3 | *Acumulativa*: usa os golpes da G1 e da G2, e o dano cresce 10% a cada rodada |
| **G-HYDRA completa** | fim do semestre | Três cabeças com vidas e energias próprias; derrubar uma muda o padrão das outras. O grupo precisa se dividir |
| **Prova surpresa** (evento) | sem aviso, por 1 hora | Um alerta CAPIRAS no app: pontos dobrados enquanto durar |

Ideias de lacaios ou chefes menores: *Trabalho em Grupo* (o dano que ele recebe é dividido entre todos, uma ironia),
*Lista de Exercícios* (vários lacaios fracos) e *Chamada* (ataca quem não agiu na rodada).

### 6.4 Pontuação: dano vira ponto

```
pontos = dano efetivo × bônus de grupo × aprovado × dificuldade
```

- **Dano efetivo** = dano direto do seu PUCmon + o dano extra que você possibilitou. Se você dá +30% de ataque a
  @ana, os 30% a mais que ela causou contam para você. Se você reduz a Resiliência do chefão, uma parte do dano extra
  de todos conta para você. Sem isso, ninguém escolhe suporte, porque suporte não pontuaria.
- **Solo:** o dano efetivo é a **média por PUCmon** (dano total ÷ número de PUCmons). Seu ponto é sempre o dano de
  *um* PUCmon: no grupo, o seu; no solo, a média dos seus. Sem essa regra, o solo com 4 valeria 4× uma partida em
  grupo, e ninguém jogaria com amigos.
- **Bônus de grupo:** +10% por jogador além do primeiro (até +50% com 6).
- **Aprovado:** ×1,2 para todos da sala.
- **Dificuldade:**

  | Chefão | Multiplicador |
  |---|---|
  | G1 | ×1 |
  | G2 | ×1,5 |
  | G3 | ×2 |
  | G-HYDRA | ×3 |

- **Rodada automática não pontua:** se o jogador não escolhe a tempo, o PUCmon dele faz o golpe básico, mas esse dano
  não vira ponto. Isso evita quem entra na sala e fica parado esperando os outros.

### 6.5 Salas e pareamento

- **Criar sala** gera um código de 4 letras, sem letras ambíguas (sem I, O, 0, 1), e um link `…?sala=KPZT`. O botão
  **Compartilhar** abre o WhatsApp. Por ser web app, o link já abre o jogo direto na sala, sem instalar nada. É a
  maior vantagem do formato web, e vale explorar.
- **Lobby:**
  - mostra os membros, o PUCmon escolhido e o estresse de cada um, e quem está “pronto”;
  - o dono inicia, e pode começar com quem estiver presente;
  - a sala expira após 10 minutos sem começar.
- **Procurar grupo** (para quem não tem amigos online agora): entra numa sala pública do chefão ativo que tenha vaga.
  Se não houver nenhuma em 30 s, cria uma sala pública e espera; quem estiver nela pode iniciar a qualquer momento.
- **Queda de conexão:** o PUCmon passa a fazer o golpe básico (sem pontuar) e o jogador pode voltar pelo mesmo link.

### 6.6 Rankings

| Ranking | Como conta |
|---|---|
| **Temporada** (semestre) | soma dos pontos; é o ranking geral |
| **Semana** | pontos contra o chefão ativo da semana |
| **Copa dos Cursos** | média dos 10 melhores bolsistas de cada curso |
| **Melhor grupo da semana** | maior dano somado por uma sala numa raid |

- **Ligas da temporada por percentil**, porque com pontos somados não há limite fixo que faça sentido:

  | Liga | Faixa |
  |---|---|
  | Doutor em Criaturas Crípticas | top 1% |
  | Pós-doc | top 10% |
  | Doutorando | top 25% |
  | Mestrando | top 50% |
  | Bolsista IC | o resto |

- No fim da temporada, recompensas cosméticas: título no perfil, moldura e **pedras especiais** para o minijogo de
  registro (seção 9). A soma zera a cada semestre.

### 6.7 Regras contra abuso

- O servidor decide tudo, porque o cliente WebGL é fácil de adulterar. O cliente só envia escolhas e anima o resultado.
- O dano máximo por raid é limitado pela vida do chefão, o que impede pontos absurdos.
- O estresse limita quantas raids cada PUCmon faz por dia.
- Para pontuar é preciso ter pelo menos 2 PUCmons registrados. Isso força a passar pelos cartazes e dificulta contas
  falsas em massa para inflar a Copa dos Cursos.
- Nomes de usuário aparecem no ranking: é preciso um filtro de palavrões no cadastro e um botão “denunciar”.

## 7. Arquitetura no Supabase

### 7.1 Tabelas

```
perfis         (id = auth.uid, usuario, curso, temporada)
capturas       (+ nivel=1, xp=0, creditos jsonb, eletiva, perfil, estresse, estresse_em)  ← já existe, ganha colunas
chefes         (id 'g1'|'g2'|'g3'|'ghydra'|'simulado', dados jsonb, ativo_de, ativo_ate, multiplicador)
salas          (id, codigo, dono, chefe, publica, estado 'lobby'|'luta'|'fim', rodada, prazo, vida, log jsonb, criada_em)
sala_membros   (sala, jogador, pucmons text[], pronto, dano_efetivo)
escolhas       (sala, rodada, jogador, pucmon, acao)   chave única → uma escolha por rodada
pontuacoes     (jogador, temporada, semana, chefe, sala, pontos)   uma linha por raid, para auditar
temporadas     (id '2026.2', inicio, fim)
especies       (poster, dados jsonb)   ← balanceamento publicado pelo Unity
```

- O **estresse não precisa de cron**: guarda-se o valor e o horário (`estresse_em`), e o valor atual é calculado na
  leitura (−10% por hora desde então).
- **As datas de G1, G2 e G3** ficam em `chefes`, editáveis no painel do Supabase a cada semestre, sem novo build.

### 7.2 Segurança (RLS)

- O cliente **lê** perfis públicos, o ranking e as próprias salas.
- O cliente **nunca escreve** XP, nível, estresse, dano ou pontos; isso só acontece por funções do servidor.
- As capturas continuam sendo inseridas pelo cliente, como hoje (não há como provar presença física), com limite por
  dia e só de criaturas do catálogo publicado.

### 7.3 A raid no servidor

- **Regras num arquivo só:** `regras.js`, com a função pura `resolverRodada(estado, escolhas, semente) → { estado,
  eventos }`.
  - Na **Edge Function `raid`** (Deno/TypeScript), ela decide as raids em grupo e as que pontuam.
  - Na **página WebGL**, ela roda o Simulado e o treino offline, chamada do C# por um `.jslib`, como a ponte de pose do
    AR.
  - Uma regra só, sem duplicar em C#. Isso é possível porque o jogo roda num navegador.
- **Endpoints:** `criar`, `entrar(codigo)`, `escolher(pucmon, acao)`, `iniciar`, `estado`. A rodada se resolve quando
  todos escolhem ou quando o prazo passa: a próxima requisição de qualquer membro depois do prazo dispara a resolução,
  sem cron.
- **Sincronização:**
  - começar com **consulta periódica**: cada membro lê a sala a cada 1,5 s. Com no máximo 6 pessoas por sala, é leve,
    e usa o `Supabase.Send` que já existe;
  - trocar pelo **Realtime** só se o volume crescer.
- **Solo pontuado** também passa pelo servidor. O solo de treino (Simulado) pode rodar só na página.

Esboço da criação de sala (o código é tentado de novo em caso de colisão):

```sql
create function criar_sala(p_chefe text, p_publica bool) returns text language plpgsql security definer as $$
declare letras text := 'ABCDEFGHJKLMNPQRSTUVWXYZ'; cod text; nova uuid;
begin
  loop
    cod := (select string_agg(substr(letras, 1 + floor(random() * length(letras))::int, 1), '') from generate_series(1, 4));
    exit when not exists (select 1 from salas where codigo = cod and estado <> 'fim');
  end loop;
  insert into salas (codigo, dono, chefe, publica, estado) values (cod, auth.uid(), p_chefe, p_publica, 'lobby') returning id into nova;
  insert into sala_membros (sala, jogador) values (nova, auth.uid());
  return cod;
end $$;
```

### 7.4 Balanceamento no Inspector

- O menu de editor “Publicar balanceamento” lê o `Catalogo` (e os chefões, se forem ScriptableObjects também) e grava
  as tabelas no Supabase.
- Ele usa uma chave de serviço guardada **só no seu editor**, **nunca no build**.
- Você muda um número no Inspector, publica, e vale para todo mundo sem novo build.

### 7.5 Limites do plano grátis (conferir os valores atuais)

- Cabe com folga num campus: banco de 500 MB, alguns GB de tráfego por mês, centenas de milhares de chamadas de Edge
  Function por mês.
- **O projeto pausa após uma semana sem uso**, o que pode acontecer nas férias. É preciso reativá-lo no painel antes
  da volta às aulas.

## 8. Fluxo e telas

### 8.1 Primeira vez

```
Notícia do Z1 (imagem 1)  →  "O professor Dr. Beltran O. se encarregou..." (imagem 2)
   →  Projeto PIBIC PUCmon, apoio CNPCNI / CAPIRAS (imagem 3)
   →  Cadastro (usuário, senha, curso): "Bem-vindo(a), bolsista!"
   →  Beltran: "Sem verba para tranquilizante. Aqui está sua pedra."
   →  Laboratório, com a primeira missão: "Registre seu primeiro PUCmon"
```

As três imagens já prontas viram a abertura. Basta passar por toque, com o botão “pular” para quem já viu.

### 8.2 Mapa de navegação

```
┌─────────────────────────── LABORATÓRIO (hub com abas) ───────────────────────────┐
│  CAMPO (AR)        FICHÁRIO          RAID                RANKING        PERFIL     │
│   │                 │                 │                    │              │        │
│   └ registrar:      └ Ficha           ├ Chefão ativo       ├ Temporada    ├ Curso   │
│     pedra →           ├ Créditos      ├ Raid solo (≤4)     ├ Semana       ├ Títulos │
│     registro          ├ Golpes        ├ Criar sala         ├ Cursos       ├ Pedras  │
│                       ├ Eletiva       ├ Entrar com código  └ Grupos       └ Sair    │
│                       └ Estresse      ├ Procurar grupo                              │
│                                       └ Simulado (treino)                           │
│                                         → Lobby → Raid → Relatório de campo         │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

O botão grande do Laboratório muda conforme o momento: **CAMPO** enquanto faltam PUCmons para registrar, e **RAID**
quando há um chefão ativo.

### 8.3 Telas (retrato, estilo pixel atual: `#EBE4DA` / `#101F3F`, Press Start 2P + Pixelify Sans)

**Laboratório (hub)**
```
┌──────────────────────────────┐
│ @igor · Bolsista IC  · Eng.  │
│ Temporada 2026.2 · #37       │
├──────────────────────────────┤
│ [Beltran] "A G2 foi avistada │
│  no campus! Precisamos de    │
│  dados. Monte um grupo."     │
│ ┌ ALERTA CAPIRAS ──────────┐ │
│ │ RAID: G2 · até domingo   │ │
│ │ vida comunitária ▓▓▓░ 64%│ │
│ └──────────────────────────┘ │
│ MISSÕES          ▓▓▓▓░░ 2/3  │
│ PUCMONS PRONTOS  4/6 (2 desc)│
│        [   RAID   ]          │
├──────────────────────────────┤
│ CAMPO FICHÁRIO RAID RANK EU  │
└──────────────────────────────┘
```

**Ficha de registro** (aqui aparecem `course` e `description`)
```
┌──────────────────────────────┐
│ < VOLTAR   FICHA Nº 003      │
│      [retrato / 3D girando]  │
│ PSONECO · Psicologia         │
│ SONO · Nota 10 · 4º período  │
│ "Descrição que você escreve  │
│  no Catálogo..."             │
│ ESTRESSE ▓▓▓░░░ 35% (3h30)   │
│ FÔLEGO       ▓▓▓▓▓░░  62     │
│ LÓGICA       ▓▓▓▓░░░  50     │
│ CRIATIVIDADE ▓▓▓▓▓▓▓  81     │
│ RESILIÊNCIA  ▓▓▓▓▓░░  66     │
│ INICIATIVA   ▓▓▓▓▓▓░  70     │
│ 3 CRÉDITOS LIVRES     [ + ]  │
│ GOLPES: Análise · Insight ·  │
│         Sessão               │
│ ELETIVA: Rascunho (monitor:  │
│          Ventintoso)         │
└──────────────────────────────┘
```

**Raid (menu)**
```
┌──────────────────────────────┐
│ CHEFÃO ATIVO: G2  (×1,5)     │
│   [arte da cabeça G2]        │
│ Fraca contra: CAFÉ           │
│ [ RAID SOLO (até 4) ]        │
│ [ CRIAR SALA        ]        │
│ [ ENTRAR COM CÓDIGO ]        │
│ [ PROCURAR GRUPO    ]        │
│ [ SIMULADO (treino) ]        │
└──────────────────────────────┘
```

**Lobby**
```
┌──────────────────────────────┐
│ SALA  K P Z T   [COMPARTILHAR]│
│ Chefão: G2                   │
│ ★ @igor   Psoneco    PRONTO  │
│   @ana    Santídeo   PRONTO  │
│   @teo    escolhendo...      │
│   ·  ·  ·  (3 vagas)         │
│ Seu PUCmon: [Psoneco ▼]      │
│ [  PRONTO  ]  [ INICIAR ]    │
└──────────────────────────────┘
```

**Raid (luta)**
```
┌──────────────────────────────┐
│ QUESTÃO 3/8          ⏱ 14 s  │
│ G2 ▓▓▓▓▓▓▓▓░░░░░░ 58%        │
│      [cabeça G2, 3D]         │
│ ⚠ Próximo: QUESTÃO DISCURSIVA│
│   → @ana                     │
│ Psoneco 88% Santídeo 41% ... │
├──────────────────────────────┤
│ @teo usou Viralizar! +12%    │
├──────────────────────────────┤
│ [ ANÁLISE  ] [ INSIGHT     ] │
│ [ SESSÃO   ] [ PROTEGER    ] │
└──────────────────────────────┘
```

**Relatório de campo (resultado)**
```
┌──────────────────────────────┐
│        APROVADO!  (G2)       │
│ seu dano efetivo   3.420     │
│  × grupo (4)       1,3       │
│  × aprovado        1,2       │
│  × G2              1,5       │
│ = 8.003 pontos   (#37 → #29) │
│ Psoneco +94 XP · estresse 70%│
│ Missão "aprove em grupo" ✓   │
│ [ DE NOVO ]  [ LABORATÓRIO ] │
└──────────────────────────────┘
```

**Ranking**
```
┌──────────────────────────────┐
│ [TEMP.] [SEMANA] [CURSOS] [GR]│
│ Temporada 2026.2 · 41 dias   │
│  1 @lua   Direito  98.120 Dr.│
│  2 @teo   Design   91.004    │
│ ...                          │
│ 37 @igor  Eng.Comp 40.211 ◀  │
└──────────────────────────────┘
```

## 9. Ideias para deixar o jogo muito mais interessante

1. **Calendário real de provas.** A G1 aparece na semana de G1 da PUC, e assim por diante. O jogo conversa com a vida
   do aluno no momento em que todo mundo está falando das provas. Uma tabela no Supabase define as datas a cada
   semestre.
2. **G-HYDRA comunitária no fim do semestre.** Uma barra de vida gigante que **todo o campus** derruba junto durante a
   semana: cada raid de cada jogador tira um pouco. Se cair, todos ganham uma recompensa. Funciona muito bem num telão
   num evento ou no stand.
3. **Manchetes do Z1 como diário da temporada:**
   - “G2 avistada no Pilotis”;
   - “Bolsistas de Design lideram a Copa”;
   - “CAPIRAS confirma: Prova Surpresa hoje às 15h”.

   O app mostra a manchete na abertura, e as mesmas artes vão para o Instagram.
4. **Editais CNPCNI/CAPIRAS como eventos:** “Edital 04/2026: raids em grupo de 6 valem ×2 neste fim de semana”.
5. **Pedras como cosméticos** (a piada da verba curta): pedra comum, pedra de rio polida, paralelepípedo, “pedra
   dourada da reitoria”. São recompensas de temporada e conquistas, só visuais, aparecem no minijogo de registro e
   não dão vantagem.
6. **Falas do Prof. Beltran.** Comentários curtos no Laboratório, no relatório e quando um PUCmon se forma (“Seu
   Psoneco colou grau! Eu choraria, mas não tenho verba para lenço.”). Personagem com personalidade segura o jogo.
7. **Lore que se revela.** A `description` mostra mais conforme o período: uma frase no 1º, a história completa no 5º
   e uma curiosidade na formatura.
8. **Estatísticas por curso de PUCmon:** “Qual curso mais bateu na G2 esta semana?”. Gera manchete do Z1 e rivalidade
   saudável.
9. **Conquistas:** “Aprovado na G3 sem nenhum esgotado”, “Uma pedra só” (registrou na primeira pedrada), “Grupo
   completo” (raid com 6), “Formatura” (primeiro PUCmon no 10º período).
10. **Link de sala no WhatsApp** abrindo o jogo direto no lobby. É a forma mais natural de juntar a turma no intervalo.

## 10. Pendências no jogo atual (a partir da lore)

- **Trocar a carteirinha pela pedra:**
  - **Sprite:** `cardSprite` do `CaptureMinigame` (hoje `UI/Carteirinha.png`) e o objeto “Carteirinha na mão” da
    `SampleScene`.
  - **Animação do acerto:** hoje o PUCmon “entra” na carteirinha, que balança, como uma pokébola. Com a pedra, ele leva
    a pedrada, fica tonto (estrelinhas girando, balançando no lugar) durante os balanços, e então desmaia (registrado)
    ou acorda e foge.
  - **Arremesso:** a pedra gira de outro jeito, rolando em vez de girar como cartão, e cai no chão depois do acerto.
  - **Textos:** “carteirinha” some; “Capturando...” vira “Registrando...”, e “Você capturou X” vira “X registrado!”.
- **Verbo do jogo:** padronizar **registrar** em todas as telas (“PUCmons registrados”, “Fichário”).
- **Abertura com as 3 imagens** (Z1, Beltran, PIBIC) antes do cadastro, só na primeira vez.

## 11. Riscos

| Risco | Como tratar |
|---|---|
| Online é caro e pode não ser divertido | Validar a raid solo offline (Simulado) antes de qualquer servidor |
| Dano como ponto premia quem tem mais tempo | Estresse limita por dia; ranking semanal e por percentil dão chance a quem joga menos |
| Suporte não pontuar | Dano efetivo com assistência |
| Solo valer mais que grupo | Média por PUCmon no solo e bônus de grupo |
| Pouca gente online para grupo | Solo pontua; “Procurar grupo” com salas públicas |
| Trapaça pelo cliente WebGL | Servidor decide; cliente só envia escolhas |
| Contas falsas para inflar a Copa dos Cursos | Mínimo de 2 PUCmons registrados para pontuar |
| Projeto do Supabase pausado nas férias | Reativar antes das aulas |
| Nomes ofensivos no ranking | Filtro no cadastro e denúncia |
| Uso do nome e da marca da PUC, e paródia de CNPq/CAPES/G1 | Combinar com a universidade antes de divulgar em larga escala |

## 12. Ordem de implementação

| Fase | Entrega | Por quê |
|---|---|---|
| **0** (feito) | Campos `course` e `description` no catálogo | Você já pode preencher |
| **1** | Pedra no lugar da carteirinha; verbo “registrar”; abertura com as 3 imagens; curso no cadastro; **Ficha de registro**; hub Laboratório | A lore entra no jogo e o Fichário ganha conteúdo, sem servidor novo |
| **2** | Atributos, períodos e estresse; `regras.js`; cena de raid; **Simulado e raid solo offline** | Descobrir se a raid diverte, barato |
| **3** | Edge Function `raid`; **salas com código**; solo pontuado; pontos e ranking (temporada, semana, cursos) | O núcleo competitivo e social |
| **4** | Procurar grupo (salas públicas); G2, G3 e G-HYDRA comunitária; editais; pedras cosméticas; Z1 | Eventos e longevidade |

## 13. Decisões que preciso de você

1. **“Até 6 amigos”** quer dizer sala de 6 no total, ou você mais 6 (7 no total)? Usei 6 no total.
2. **O solo pontua no ranking?** Recomendo que sim, com a média por PUCmon.
3. **Os chefões são as cabeças da G-HYDRA** (G1, G2, G3 e a completa no fim)? Ou a G-HYDRA é só o vilão de fundo, e os
   chefões são outros?
4. **Datas de G1, G2 e G3** da PUC neste semestre, para o calendário de chefões.
5. **Ranking geral pela soma da temporada** (recomendado) ou pela melhor raid?
