# Criaturas da PUC Rio — GDD e direção técnica

**Versão:** 0.2, conceito para validação · **Data:** 20 de setembro de 2026 · **Plataforma alvo:** navegador móvel · **Projeto Unity:** 6000.5.1f1

## 1. Visão e decisões iniciais

Jogo de coleção e batalhas entre criaturas inspiradas nos cursos da PUC Rio. O jogador abre o jogo pelo site, aponta a câmera para uma ilustração impressa na parede do campus, vê a criatura se desprender da imagem em realidade aumentada, conclui um desafio curto e a adiciona à coleção. Fora da câmera, treina criaturas, monta uma equipe e disputa partidas online por CR, uma pontuação fictícia separada do coeficiente acadêmico real.

**Hipóteses de design, ainda não aprovadas:** batalhas por turnos 1 contra 1 com equipe de três; uma espécie por curso, com exemplares próprios por jogador; três rankings (CR competitivo, coleção e contribuição por curso). Essas propostas tornam o protótipo especificável; o balanceamento e o tom das piadas dependem de testes com alunos.

**Direção para o protótipo:** aproveitar o projeto Universal 3D já criado no Unity 6000.5.1f1 e testar um pacote de rastreamento de imagem para Unity Web. O candidato Imagine WebAR Image Tracker Free declara suporte a Unity 6000.x e WebGL, porém a edição gratuita não aceita imagens alvo personalizadas; por isso ela não resolve sozinha a visão de cartazes próprios de cada curso. Avaliar um rastreador que aceite arte própria ou a versão paga somente após comprovar rastreamento e desempenho com aparelhos reais. Como alternativa, uma página WebAR separada pode realizar a captura e compartilhar o backend com o jogo em Unity.

## 2. Pilares e público

1. **Descoberta presencial:** cartazes físicos estimulam a exploração do campus sem pedir geolocalização.
2. **Identidade acadêmica bem-humorada:** cada criatura traduz a cultura de um curso com humor afetuoso e espaço para estudantes daquele curso revisarem a representação.
3. **Competição compreensível:** poucos atributos, partidas curtas e classificação explicável.

Público inicial: estudantes da PUC Rio com celular e navegador atual, inclusive visitantes em um modo de demonstração se a equipe decidir permitir acesso fora do login institucional. Uma meta de acessibilidade é que a coleção e as batalhas funcionem sem câmera; a captura alternativa depende de uma decisão de produto para não anular a descoberta física.

## 3. Jornada e ciclo de jogo

**Primeiro acesso:** abre o endereço do jogo por link, favorito ou comunicação do campus → cria conta ou entra → escolhe apelido → recebe tutorial curto → inicia o modo câmera → concede permissão → enquadra uma ilustração impressa na parede → vê a criatura ancorada à imagem → joga o desafio de captura → recebe o resultado confirmado pelo servidor → pode consultar coleção, treino e batalhas.

**Descoberta sem QR:** a página já aberta reconhece as ilustrações registradas e identifica a criatura correspondente. O cartaz deve ter textura e pontos visuais distintos, escala conhecida e impressão estável na parede. O efeito de sair da parede usa a pose estimada da imagem para animar o personagem do plano do cartaz para a frente da câmera. Sem rastreamento espacial prolongado, ele volta a depender da imagem visível; ao perdê-la, pausar ou concluir a animação em uma transição de interface. Fotografia do cartaz também pode ser reconhecida: sem geolocalização ou outro mecanismo não há prova forte de presença física.

**Requisito para o catálogo:** rastrear imagens próprias e associar cada alvo a uma espécie. Reconhecimento de várias imagens no mesmo fluxo deve ser comprovado no pacote selecionado; se a solução só aceitar um alvo por sessão, será necessário um modo de seleção ou trocar de tecnologia. Na edição gratuita do Imagine WebAR, o fornecedor informa que não há personalização das imagens alvo nem rastreamento simultâneo, então usá-la apenas como teste técnico com os modelos oferecidos.

**Ciclo recorrente:** descobrir ponto → capturar → cumprir atividades curtas para obter recursos → escolher melhorias → disputar batalha → acompanhar evolução e rankings → buscar outro ponto.

## 4. Criaturas e progressão

**Estrutura de dados:** `Course` (id estável, nome e unidade), `Species` (curso associado, visual, papel, habilidades, atributos base), `CreatureInstance` (dono, espécie, nível, pontos distribuídos, cosméticos), `EncounterPoint` (cartaz, alvo de imagem, espécie, janela de disponibilidade), `Player` e `Inventory`. Separar dados de espécie dos exemplares evita duplicação e permite ajustes de balanceamento.

**Quatro atributos compartilhados propostos:** Raciocínio (efeitos técnicos), Expressão (blefes e controle), Persistência (resistência e recuperação), Improviso (iniciativa e adaptação). São caricaturas mecânicas de estilos de jogo, não avaliações reais dos estudantes. Cada curso recebe distribuição inicial distinta com a **mesma soma de pontos**; fraquezas e vantagens surgem de combinações de habilidades, sem atribuir superioridade a um curso.

**Exemplos provisórios, não lista oficial de cursos:** Computação pode ter Raciocínio alto e habilidade de preparar uma jogada; Design pode ter Expressão alta e uma habilidade de alterar a própria estratégia; Direito pode usar uma reação para contestar um efeito. Nomes, roupas, símbolos e piadas devem passar por revisão de alunos e de arte antes de produção. A lista completa de cursos e as autorizações para uso de nome e marca da universidade devem ser confirmadas antes da expansão do catálogo.

**Progressão sugerida:** experiência por partidas e desafios, pontos de treinamento limitados por nível, teto de nível por temporada competitiva e redistribuição de pontos acessível. Capturas repetidas podem render cosméticos ou materiais, sem gerar vantagem ilimitada. A evolução visual da criatura é uma possibilidade futura; a melhoria de atributos já basta para a primeira versão.

## 5. Encontro e captura

**Estado do encontro:** indisponível → modo câmera aberto → autenticação se necessário → permissão de câmera → alvo reconhecido → animação da criatura se desprendendo da parede → desafio → confirmação do servidor → resultado. Ao perder o alvo, preservar o progresso por alguns segundos, pausar efeitos espaciais e pedir novo enquadramento. Oferecer mensagens claras para câmera negada, página recarregada e conexão perdida.

**Mecânica candidata para teste:** em 20 a 30 segundos, a criatura sinaliza três gestos ou símbolos; o jogador toca a sequência correta enquanto ela se move ao redor do cartaz. Acertos aumentam uma barra de vínculo; ao completá-la, solicita a captura. O teste deve verificar se o desafio continua jogável quando a imagem oscila, se a postura é segura ao caminhar e se o input não depende de reflexos excessivos. Não premiar recursos apenas com base em um resultado informado pelo cliente.

**Regras de captura propostas:** primeira captura da espécie garantida após um desafio concluído; capturas posteriores por ponto têm limite configurável por conta e por intervalo. Pontos administráveis no servidor permitem retirar cartazes danificados ou trocar eventos. A UI mostra quando o jogador está impedido de tentar e por quê.

## 6. Batalha online

**Formato inicial proposto:** duelo assíncrono em ritmo de turnos, com duas pessoas conectadas simultaneamente, equipe de até três criaturas e uma ativa por vez; selecionar ação dentro de um tempo limite, resolver o turno no servidor e enviar o estado aos dois clientes. Partida alvo: 3 a 5 minutos. Esse formato reduz exigências de sincronização de quadros e latência em comparação com combate de ação em tempo real.

Cada criatura tem vida, duas habilidades e uma ação defensiva comum. Atributos afetam valores com limites explícitos; habilidades têm custo ou recarga. Mostrar efeitos antes de confirmar a ação e registrar um resumo do turno. Vitória ao derrotar a equipe adversária; abandono, reconexão e empate precisam de regras fechadas antes do ranqueado.

**Servidor autoritativo:** cliente envia intenção (`matchId`, `turn`, `action`, `target`, `requestId`); servidor valida posse, cooldown, estado e prazo, calcula o resultado com versão fixa das regras, grava o turno e publica o novo estado. O cliente nunca envia dano final, vitória, CR ou conteúdo da coleção como fonte de verdade. WebSocket serve para eventos de turno; HTTPS atende login, perfil, inventário e rankings. Para protótipo, um único serviço de aplicação com banco relacional é suficiente.

**Emparelhamento:** primeiro criar sala privada por código e testar fluxo de ponta a ponta; depois fila ranqueada que considera faixa de CR e tempo de espera. Evitar partidas competitivas entre contas da mesma pessoa com limitações proporcionais e moderação posterior. Guardar histórico básico para investigar erros de pontuação.

## 7. CR e três rankings

**CR competitivo:** classificação numérica fictícia da temporada, iniciada em 1000. Uma proposta simples é `novoCR = CR + K × (resultado − expectativa)`, com expectativa baseada na diferença entre CRs; `K` e penalidades de abandono serão calibrados após testes. Não confundir com o CR acadêmico da PUC Rio: nome e interface devem deixar explícito que é uma pontuação de jogo. O servidor registra alterações de modo idempotente, uma vez por partida encerrada.

| Ranking | Métrica proposta | Reinício | Proteção contra distorção |
|---|---|---|---|
| Competitivo | CR de partidas ranqueadas | Por temporada, com ajuste inicial | Apenas partidas válidas; regras de abandono |
| Colecionador | Espécies distintas obtidas | Permanente | Duplicatas não somam |
| Curso | Contribuição dos jogadores a um curso escolhido | Por temporada | Teto diário de pontos por pessoa |

O terceiro ranking precisa de uma escolha de identidade: curso em que o jogador estuda, curso declarado, ou equipe de afinidade escolhida livremente. Recomenda-se afinidade livre para não exigir nem exibir vínculo acadêmico real. Se a equipe desejar usar matrícula ou e-mail institucional, isso requer autorização e desenho de privacidade específicos.

## 8. Arquitetura e implantação

```text
Link de acesso → site móvel por HTTPS → modo câmera
                                      ├─ imagem impressa → rastreador → criatura em Unity Web
                                      └─ coleção e batalha em Unity Web
                                                 │ HTTPS / WebSocket
                                      API autoritativa + autenticação
                                                 │
                                      Banco de dados + registros de partida
```

**Frontend de RA:** a primeira experiência será um build Unity Web com um pacote WebAR de terceiros. Preparar uma cena de prova com uma imagem alvo, um cubo ou modelo provisório e animação de deslocamento para fora da parede. Se o rastreador Unity falhar nos aparelhos alvo ou exigir custos incompatíveis com o escopo, migrar apenas a captura para uma página WebAR com rastreamento de imagem e 3D leve. MindAR com Three.js é um candidato para essa alternativa. Em ambos os casos, usar HTTPS para câmera, medir tamanho do download e manter modelos leves.

**Unity no navegador:** a documentação da Unity lista suporte Web para Safari iOS e Chrome Android, mas isso não equivale a suporte de AR Foundation em WebGL. AR Foundation requer um plug-in provedor; a lista oficial inclui ARCore/ARKit para aplicativos móveis, não Web. O Imagine WebAR Image Tracker é uma implementação de terceiros específica para Unity WebGL. Sua versão gratuita declara suporte à família Unity 6000.x, mas foi lançada em 2023 na Asset Store e limita alvos personalizados e múltiplos alvos. Validar especificamente a versão 6000.5.1f1 e o template URP do projeto, além de iPhone/Safari e Android/Chrome, antes de adotá-la.

**Ferramenta de hospedagem local:** Web Build Host pode facilitar servir um build e ler logs durante o desenvolvimento. A página da Asset Store declara compatibilidade com 6000.5.0f1 e URP, mas a descrição pública não detalha garantias para câmera móvel ou a forma de HTTPS. Não é requisito do jogo nem substitui hospedagem permanente. Primeiro validar o build com uma origem HTTPS acessível pelo celular.

**Login:** para MVP, e-mail com verificação ou provedor de identidade configurado com segurança; conta de convidado somente se houver migração posterior de progresso. Login institucional via SSO da PUC Rio depende de acordo, acesso técnico e autorização da universidade, portanto não integra a premissa do MVP. Usar sessão segura no navegador e validar permissões em todas as rotas de API.

**Dados persistidos:** jogadores, espécies, instâncias, pontos, registros de captura, melhorias, partidas, turnos, saldos de CR e temporada. Índices únicos para captura/resultado que não pode ser repetido; transações para prêmio, consumo de recurso e pontuação. A câmera não precisa ser enviada ao servidor: processar os quadros no aparelho e armazenar somente eventos mínimos. Definir política de retenção e aviso de privacidade antes da abertura pública.

**Painel operacional mínimo:** cadastrar e desativar cartazes, associar espécies, ajustar limites e temporadas, consultar falhas de captura/partida, corrigir contas mediante registro de auditoria. Um serviço gerenciado de banco e hospedagem de API pode atender o piloto; estimar custos após medir acessos simultâneos, tráfego de modelos e duração das partidas.

## 9. Escopo e marcos

| Marco | Entrega verificável | Critério de passagem |
|---|---|---|
| 0. Prova de RA | Abrir site; câmera reconhece imagem de teste; criatura sai visualmente da parede | Testar iPhone/Safari e Android/Chrome em aparelhos reais, luz e Wi-Fi do campus; verificar se é possível usar uma imagem própria |
| 1. Fatia jogável | Login, um cartaz, uma criatura, desafio, captura persistida, coleção | Reabrir e recuperar progresso; impedir captura duplicada indevida |
| 2. Batalha fechada | Duas contas, sala privada, turnos no servidor e resultado | Reconexão, turno expirado e resultado único após repetição de requisição |
| 3. Piloto | Três a cinco criaturas, treino, CR e três rankings, painel básico | Teste com alunos, revisão de humor e equilíbrio, métricas de abandono |
| 4. Expansão | Catálogo por curso e mais pontos físicos | Aprovação de conteúdo, produção de arte e manutenção de cartazes |

**Fora do MVP:** geolocalização, mundo aberto, AR multiplayer compartilhada, trocas entre jogadores, monetização, ranking acadêmico real, dezenas de criaturas antes de validar o loop. O número final de espécies depende da lista de cursos adotada e da capacidade de produção de arte.

## 10. Riscos, medições e decisões pendentes

| Risco | Experimento ou resposta |
|---|---|
| RA instável em aparelhos móveis | Medir tempo até aparecer, perda de alvo e taxa de conclusão em ambos os sistemas móveis |
| Foto do cartaz usada fora do campus | Aceitar o limite no piloto; não afirmar presença física; avaliar dinâmica de evento presencial |
| Edição gratuita só aceita alvos modelo | Usá-la para testar viabilidade; escolher outro rastreador ou licença antes de produzir cartazes definitivos |
| Custo de arte por curso | Criar uma criatura modelo com esqueleto, animações e variações reutilizáveis |
| Desequilíbrio competitivo | Começar com poucas espécies, telemetria por habilidade e ajustes por versão |
| Humor ofensivo ou pouco representativo | Co-criação com alunos, revisão de linguagem e canal de feedback |
| Marca universitária e dados pessoais | Pedir autorização para uso de identidade oficial; minimizar dados e explicar uso da câmera |

**Métricas do piloto:** proporção de sessões de câmera que reconhecem um cartaz, tempo de carregamento, taxa de captura concluída, falhas por navegador, retorno em sete dias, batalhas concluídas, abandono por turno, distribuição de vitórias por espécie e variação de CR por faixa.

**Decisões que a equipe precisa fechar:** Qual rastreador aceita os cartazes próprios no orçamento disponível? Unity será mantido na experiência de RA após o teste móvel? Como o jogador encontra o site sem QR? A batalha é por turnos ou ação em tempo real? Quem pode criar conta? Como será definida a afiliação do terceiro ranking? Qual o tom visual das criaturas? Existe autorização para usar nome, marca e espaços físicos da PUC Rio? O teste de RA determina a arquitetura final.

## 11. Referências técnicas consultadas

- [Unity — compatibilidade de navegadores Web](https://docs.unity3d.com/Manual/webgl-browsercompatibility.html).
- [Unity — AR Foundation e provedores por plataforma](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0/manual/index.html).
- [MDN — WebXR e disponibilidade entre navegadores](https://developer.mozilla.org/en-US/docs/Web/API/WebXR_Device_API).
- [MDN — acesso à câmera e contexto seguro](https://developer.mozilla.org/en-US/docs/Web/API/MediaDevices/getUserMedia).
- [MindAR — rastreamento de imagem na web](https://hiukim.github.io/mind-ar-js-doc/).
- [AR.js — rastreamento de imagem](https://ar-js-org.github.io/AR.js-Docs/image-tracking/).
- [Unity — panorama de serviços multiplayer](https://docs.unity.com/en-us/multiplayer).
- [Imagine WebAR Image Tracker Free — funcionalidades e restrições](https://imagine-webar.com/unity/image-tracker-free/).
- [Web Build Host — página do pacote na Asset Store](https://assetstore.unity.com/packages/tools/network/web-build-host-local-public-host-console-for-web-builds-384084).

As escolhas de mecânica, arquitetura de servidor e CR neste documento são propostas de design, não funcionalidades garantidas pelas referências.
