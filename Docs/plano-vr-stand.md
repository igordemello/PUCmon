# Plano hipotético: batalha em realidade mista no stand

> **Status: hipotético, ainda não confirmado.** Ideia registrada em outubro de 2026 para avaliar mais tarde. Nada disto
> foi implementado.

## A ideia

O jogo continua no celular como hoje (capturar PUCmons nos cartazes pelo campus). No evento, o stand do PUCmon tem
dois óculos de realidade mista. Dois jogadores colocam os óculos, veem o stand de verdade pela câmera do óculos e,
sobre a mesa real, uma arena virtual onde os PUCmons capturados por eles batalham. Os dois veem a mesma arena, no
mesmo lugar da mesa, e a mesma batalha.

É um **aplicativo à parte, instalado nos óculos** (não o site). Ele reaproveita os modelos, o visual e as contas do
Supabase do projeto atual.

## Equipamento

- **2× Meta Quest 3 (ou 3S)**: mostram a realidade em cores pela câmera ("passthrough"), com o conteúdo virtual por cima.
- **Roteador Wi-Fi próprio do stand**: Wi-Fi de evento costuma ser saturado, e os óculos conversam o tempo todo.

## Os dois problemas técnicos

### 1. A arena no mesmo lugar da mesa para os dois óculos

Recurso da Meta: **Colocation + Shared Spatial Anchors** (âncoras espaciais compartilhadas).

1. O óculos "anfitrião" marca a arena na mesa: o operador aponta para a mesa uma vez, ou o óculos reconhece a mesa
   pelo escaneamento da sala.
2. A âncora é compartilhada com o segundo óculos, que encontra o primeiro por Bluetooth.
3. Os dois passam a ver a arena no mesmo ponto físico: se um aponta para um bicho, o outro vê o mesmo bicho ali.

Não precisa de marcador impresso na mesa. Os óculos precisam do **"Enhanced Spatial Services"** ligado nas
configurações (sem isso a âncora não é compartilhada).

### 2. A mesma batalha nos dois

Multijogador em rede local: um óculos é o servidor, o outro se conecta. Uma **batalha por turnos** (estilo Pokémon:
cada um escolhe um golpe, depois as animações rodam) é a mais simples de sincronizar, porque só trafegam as escolhas.

## Ligação com o app do celular

- No app do celular, um botão **"Batalhar no stand"** mostra um código de 4 dígitos.
- No óculos, a pessoa digita o código num teclado virtual grande, usando as mãos (sem controle). O óculos carrega a
  coleção dela pelo Supabase. Isso precisa de uma tabela de códigos e de uma função segura no Supabase, porque hoje
  cada conta só lê a própria coleção.
- **Só batalha com os PUCmons capturados nos cartazes**: dá motivo para caçar pelo campus antes de ir ao stand.
- Opcional: vitórias gravadas na conta e ranking do evento.

## O que reaproveita do projeto atual

- Os modelos `.glb` e o catálogo (nomes, imagens 2D, balanceamento).
- O efeito do buraco na parede (`WallHole`) **vira buraco na mesa**: os PUCmons saem do tampo para a arena.
- O estilo pixel (paleta, fontes, caixas) nos menus flutuantes.
- O Supabase e as contas.

## Logística do stand

- **Luz**: o rastreamento e a imagem da câmera precisam de ambiente bem iluminado.
- **Fila andando**: partidas de 3 a 5 minutos, entrada sem controle (só as mãos). Espelhar um óculos numa TV para
  quem está na fila assistir.
- **Higiene e bateria**: capas descartáveis; um óculos carregando enquanto o outro é usado, ou cabos de energia.

## Como seria feito

- **Projeto Unity separado.** O SDK da Meta pede Unity mais nova (Unity 6) e build Android/Quest; atualizar este
  projeto arriscaria quebrar o app web. O projeto dos óculos copia os modelos e conversa com o mesmo Supabase.
- **Ponto de partida**: o bloco pronto de **Colocation** da Meta e os exemplos oficiais de alinhamento entre óculos.
- **Etapas**:
  1. Protótipo: dois óculos vendo a mesma arena na mesa, com um bicho de cada lado. É o maior risco técnico, então
     vem primeiro.
  2. Batalha por turnos.
  3. Login por código e coleção do jogador.
  4. Visual e teste no próprio stand.

## Alternativa mais barata

Dois tablets ou celulares em volta de um **tapete-arena impresso** na mesa. O tapete vira a âncora compartilhada,
usando o rastreamento de imagem que o projeto já tem (`FreeWebAR`); só falta a parte de rede. Impressiona menos que os
óculos, mas custa muito menos e reaproveita quase tudo.

## Referências

- [Shared Spatial Anchors (Meta, Unity)](https://developers.meta.com/horizon/documentation/unity/unity-shared-spatial-anchors/)
- [Exemplo de Shared Spatial Anchors (Meta)](https://developers.meta.com/horizon/documentation/unity/unity-sample-shared-spatial-anchors)
- [Exemplo Colocation com Photon Fusion (Meta)](https://developers.meta.com/vr/documentation/unity/unity-sample-colocation-fusion)
- [Meta Building Blocks na Unity (Brown VR Wiki)](https://www.vrwiki.cs.brown.edu/vr-development-software/unity/meta-building-blocks-in-unity)
