# Arquitetura — Fase 1

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
