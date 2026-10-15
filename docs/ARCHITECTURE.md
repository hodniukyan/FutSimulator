# Arquitetura — fundação e clubes personalizados

Monólito modular: um processo ASP.NET Core e um frontend React. Os projetos separados impõem dependências verificáveis pelo compilador:

```text
Api -> Application -> Domain
Api -> Infrastructure -> Application -> Domain
frontend -> contratos HTTP da Api
```

## Responsabilidades

- **Domain**: entidades existentes e `MatchSimulator`, sem bibliotecas externas ou acesso a banco. Mantém os namespaces das entidades usados pelas migrations.
- **Application**: `TeamService`, `PlayerService`, `MatchService`, DTOs, regras de entrada e `IGameStore`. A interface descreve apenas operações realmente necessárias. Não expõe IQueryable/DbContext.
- **Infrastructure**: `AppDbContext`, configurações EF, `EfGameStore`, migrations MySQL e factory de design time.
- **Api**: Minimal APIs agrupadas por funcionalidade, DI, CORS restrito, Swagger em desenvolvimento, logging estruturado e ProblemDetails.
- **Tests**: xUnit, testes determinísticos do algoritmo e integração HTTP com persistência fake.
- **frontend**: React/TypeScript/Vite, Tailwind, componentes locais baseados em shadcn/ui, React Router, TanStack Query, Axios e RHF/Zod. Página inicial conserva apenas a funcionalidade existente de amistoso.

Button e Card foram gerados pelo registro oficial shadcn/ui e têm imports adaptados ao utilitário local e à dependência Radix Slot já instalada. A CLI não é necessária para executar/compilar; foi removida após geração porque sua cadeia fast-glob/braces apresentou vulnerabilidade sem correção compatível. O aplicativo conserva somente dependências usadas e o arquivo components.json.

## Fluxo de uma partida

POST /match -> MatchService -> consulta times/elencos em IGameStore -> MatchSimulator -> MatchDto -> HTTP 200.

A simulação ocorre uma única vez. Não houve persistência de histórico no código original; `Historico` foi preservado como modelo sem DbSet/tabela. Sua implementação fica para uma etapa posterior.

## Regras preservadas e correções explícitas

- Força efetiva = força do time + média truncada da habilidade de todos os jogadores, incluindo GK.
- Diferença absoluta >30: massacre; >10: favorito; demais: equilibrado.
- Ramificação de zebra de 20% preservada, inclusive possibilidade de empate.
- Artilheiros sorteados uniformemente entre jogadores com Position != "GK"; comparação continua sensível a maiúsculas.
- Minutos 1–90, gols ordenados; limites de 5 para favorito e 2 para mais fraco.
- Random.Shared em produção e Random fornecido nos testes. Não existe vantagem de mando adicionada.
- POST ignora IDs enviados pelo cliente. PUT passa a validar nome/país/força e nome/posição/habilidade.
- Partidas contra o próprio time passam a retornar 400.
- Exclusão de time com jogadores passa a retornar 409, prevenindo órfãos.
- PUT de jogador continua sem transferir TeamId; transferências ficam fora do escopo.
- Rotas antigas permanecem como aliases, acrescentando GET/PUT/DELETE /players/{id}.
- Erros antes retornados como strings passam a ProblemDetails; clientes devem ler `title`.

## Configuração e evolução

Runtime mantém AutoDetect da versão MySQL existente. `Database__ServerVersion` permite especificar a versão real e iniciar sem consultar o banco; isso não elimina a necessidade de conexão nas operações.

A API não chama Migrate/EnsureCreated. `/health` verifica somente o processo. CORS permite localhost:5173/4173 por padrão e aceita configuração externa.

Segredos podem entrar via ambiente ou user-secrets. `.env` não é carregado pelo ASP.NET automaticamente. Nenhum segredo pode usar prefixo VITE_.

Módulos futuros serão introduzidos conforme casos de uso e requisitos; não há tabelas vazias, filas, serviços distribuídos ou abstrações genéricas.

## Etapa 2 — criação de clube

`POST /teams/custom -> CustomTeamService -> PlayerGenerator -> IGameStore.CreateTeamWithPlayers -> EfGameStore -> MySQL`.

Application valida nome/país (trim e máximo 100), template permitido e cores #RRGGBB; define IsCustom=true e força como média truncada das habilidades. O endpoint apenas adapta HTTP e retorna 201/Location com TeamDto e 11 PlayerDto. TeamDto ganha campos aditivos; rotas, propriedades anteriores e contratos de partida continuam disponíveis. PUT histórico de clube preserva os metadados do escudo.

PlayerGenerator é domínio puro. Sorteia nomes sem reposição de um conjunto de 144 combinações fictícias, sem necessidade de serviço externo e sem duplicatas no elenco. Recebe Random opcional (Random.Shared na produção, semente/implementação controlada nos testes). Cada slot recebe idade 18–32 e habilidade 50–80 inclusivas, sem ponderação por posição. Ordem: GK, dois zagueiros, dois laterais, dois volantes, dois meias e dois atacantes. Persistência usa códigos atuais: GK/DEF/MEI/ATT. O modelo atual não distingue subfunções de defensores e meio-campistas.

EfGameStore inicia transação, salva Team para obter seu ID, preenche TeamId dos jogadores, salva os 11 e confirma. Uma falha reverte ambas as gravações e limpa o ChangeTracker. Application não conhece DbContext/transação. Testes relacionais com SQLite somente em memória verificam sucesso e rollback após falha de jogador, preservando dados anteriores; isso não substitui validação no MySQL real.

## Escudos e frontend

### Extensão de uniformes

`ModelosDeUniforme` no Domain mantém os identificadores de quatro modelos. `CustomTeamService` valida o campo opcional, define `Team.ModeloDoUniforme` e usa a mesma operação atômica de clube/elenco. DTOs de consulta retornam `modeloDoUniforme`. O CRUD anterior de nome/país/força preserva esse metadado.

Infrastructure mapeia uma coluna opcional varchar(32). Migration aditiva `AdicionarModeloDeUniforme`, sem aplicação nesta entrega. Não há tabela de templates, cópia de cores ou SVG persistido. Cores continuam nas propriedades existentes do clube.

Frontend: `modelos-de-uniforme.ts` define união/legendas, `UniformeDoClube.tsx` importa os SVGs, `SeletorDeUniforme.tsx` oferece radios e `PreviaDoUniforme.tsx` apresenta a camisa ampliada. Criação mantém RHF/Zod/TanStack Query e o cliente HTTP; cards e sucesso do cadastro reutilizam a camisa. Identificadores novos em português; anteriores preservados para compatibilidade.

SVGs em grade 64x64, comandos ortogonais inteiros, contorno neutro e crispEdges. Horizontal, vertical e meio a meio consomem as duas cores; liso somente a primária. Sem IDs/clipPaths compartilhados, para que instâncias não colidam. Nenhuma alteração nos escudos fornecidos.

- `frontend/src/assets/shields/*.svg`: os três arquivos fornecidos, movidos sem modificar bytes.
- Vite/SVGR importa `*.svg?react`; `svgo: false` impede otimização/redesenho. Não usar img para passar variáveis CSS ao documento SVG.
- `TeamBadge` resolve template, aplica `--primary-color` e `--secondary-color` ao SVG inline, conserva proporção 120:140 e aceita tamanho/nome. Template ausente/desconhecido usa iniciais neutras. Cores inválidas usam defaults seguros.
- `BadgeSelector`, `BadgePreview` e `ColorPicker` reutilizam TeamBadge. Variáveis são locais por escudo, sem interferir nos tokens globais do tema.
- `/clubs` lista times e `/clubs/new` usa RHF/Zod para feedback de formulário; backend é a autoridade. País é string no contrato, com opções comuns na interface. API em `features/teams/api.ts`; mutation em `use-create-team.ts`, com invalidação de `['teams']` após sucesso.
- Mesmo TeamBadge é usado nos cards, formulário, seleção e placar. Futuras chaves/histórico deverão reutilizá-lo; essas telas ainda não existem.

### Adicionar um template SVG futuramente

1. Receber o SVG aprovado em assets/shields, mantendo paths, formas, viewBox e detalhes neutros. Parametrizar somente fill/stroke necessários com as duas variáveis CSS.
2. Importar com `?react` e registrar em TeamBadge e BadgeSelector.
3. Acrescentar identificador à união BadgeTemplate, schema Zod e lista permitida em CustomTeamService. Não persistir markup SVG, URLs ou arquivos no banco.
4. Testar todos os elementos do desenho, cores por instância e acessibilidade. Registrar hash do novo arquivo em TeamBadge.test.tsx; nunca atualizar hashes antigos para encobrir alterações de arte.

## Arquitetura futura de competições — somente proposta

Decisões mais recentes e limites de autorização: [PLANEJAMENTO-COMPETICOES.md](PLANEJAMENTO-COMPETICOES.md). Primeira versão confirmada com 16 clubes e uma partida por vez acompanhando um clube: derrota oferece Fechar para a página inicial; vitória intermediária oferece Avançar. Implementação expressamente adiada para priorizar uniformes.

| Componente | Responsabilidade proposta |
| --- | --- |
| Competition | Identidade, status, tamanho 4/8/16/32, rodada atual e clube acompanhado; modo espectador/manager não permite interferir no resultado |
| CompetitionTeam | Inscrição de Team na Competition, unicidade por competição e posição no sorteio |
| Match | Confronto persistido, clubes, rodada/slot, resultado, vencedor, eventual placar de pênaltis e vínculo ao próximo confronto |
| MatchEvent | Eventos ordenados por minuto/sequência, vinculados à partida e aos participantes; origem da reprodução visual |
| KnockoutBracketGenerator | Domínio puro: recebe IDs únicos e Random controlável, embaralha e gera os confrontos/slots sem EF |
| CompetitionService | Valida inscrições, cria chave, coordena simulação/pênaltis/persistência e avanço automático de vencedores |

Mata-mata será de jogo único, com N-1 partidas. Participantes devem existir e não se repetir. O sorteio ocorre uma vez e sua chave é persistida; leitura/atualização visual não sorteia novamente. CompetitionService chama o MatchSimulator existente uma única vez por partida; empate será resolvido por uma regra separada de pênaltis, sem alterar o algoritmo atual de amistosos. Persistir vencedor e progressão atomicamente, com controle de concorrência/idempotência para impedir dupla simulação e avanço duplicado.

O React reproduzirá em 30 segundos o resultado/eventos já calculados: minuto/90 * 30 para posicionamento inicial dos gols, acrescido do tratamento visual dos pênaltis definido na futura etapa. Tempo da animação e controles de acompanhamento não recalculam resultados. A API continuará contendo as decisões de negócio. Match atual não é persistido; o contrato de amistoso deverá permanecer compatível quando essa estrutura for introduzida.

Nenhuma classe, tabela, migration ou endpoint de competição foi implementado na Etapa 2.
