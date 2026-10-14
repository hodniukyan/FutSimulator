# Mini Brasfoot — instruções permanentes

## Idioma do código — preferência do usuário

- A partir desta orientação, escrever novos métodos, funções, variáveis, parâmetros, classes, interfaces, propriedades, componentes, arquivos próprios, testes e comentários em português brasileiro.
- Usar nomes claros, como `CriarClubePersonalizado`, `GeradorDeJogadores`, `nomeDoClube` e `corPrimaria`, mantendo as convenções de capitalização da linguagem. Identificadores e arquivos devem ficar sem acentos; comentários e textos devem usar a grafia correta.
- Preservar identificadores exigidos por bibliotecas, frameworks e ferramentas, assim como contratos HTTP/JSON, nomes de tabelas/colunas e migrations existentes, para manter compatibilidade.
- A preferência vale para o trabalho novo. Não renomear o código existente em massa sem uma solicitação específica do usuário.

## Escopo e arquitetura

- Trabalhe nesta pasta. API, Application, Domain e Infrastructure estão em `backend/`; React em `frontend/`.
- Banco oficial: **MySQL existente**, usando Pomelo 9 e EF Core 9. Não substituir por PostgreSQL/SQLite.
- Domain não referencia EF/HTTP. Application contém DTOs, validações e casos de uso.
- Infrastructure implementa `IGameStore`, DbContext e migrations. API só adapta HTTP e configura DI.
- Namespaces `BrasfootAPI.Models`/`BrasfootAPI.Data` foram preservados para compatibilidade das migrations.
- Não adicionar Repository genérico, Unit of Work, MediatR ou autenticação sem necessidade explícita.
- Preserve médias, limites, ordem dos sorteios e probabilidades da simulação. `Random` opcional permite testes determinísticos.
- Não implementar campeonatos, transferências, finanças, temporadas ou save games nesta fase.

## Dados e contratos

- Nunca apagar bancos, arquivos SQLite, migrations ou dados sem autorização explícita.
- Não aplicar migrations automaticamente ao iniciar a API. Fazer backup e verificar órfãos antes da FK pendente.
- Nunca usar `EnsureCreated` no banco existente. Não reaplicar `InitialMySql` se tabelas já existem sem verificar `__EFMigrationsHistory`.
- Não incluir senhas em arquivos versionados, logs, respostas ou variáveis `VITE_*`.
- Conexão local original permanece somente em appsettings.Development.json; produção exige configuração externa.
- HTTP usa DTOs. Erros usam ProblemDetails; respostas de sucesso mantêm nomes/formato anteriores.
- PUT/DELETE `/teams/{id}/players` são aliases históricos: `id` é do jogador. Prefira `/players/{id}`.
- Excluir time com jogadores retorna 409. PUT aplica as mesmas validações de criação.
- `Historico` permanece modelo não persistido. Não criar tabelas de módulos futuros por antecipação.

## Comandos (a partir da raiz)

```powershell
dotnet restore BrasfootAPI.sln
dotnet build BrasfootAPI.sln -m:1 /nr:false
dotnet test backend/MiniBrasfoot.Tests/MiniBrasfoot.Tests.csproj --no-build -m:1 /nr:false
dotnet tool restore
dotnet ef migrations has-pending-model-changes --project backend/MiniBrasfoot.Infrastructure --startup-project backend/MiniBrasfoot.Api --no-build
dotnet run --project backend/MiniBrasfoot.Api --launch-profile http
npm --prefix frontend ci
npm --prefix frontend run lint
npm --prefix frontend run build
npm --prefix frontend run dev
```

- Usar `-m:1 /nr:false` se o ambiente Windows restringir processos MSBuild.
- VSTest pode precisar executar fora do sandbox para comunicação entre processos.
- Testes HTTP substituem persistência por fake; não comprovam operação no MySQL real.
- Faça alterações pequenas e verificáveis; explique mudanças de comportamento em português.
- Frontend por funcionalidades, URLs no cliente HTTP, estados de carregamento/erro/vazio e controles acessíveis.
- Confira `docs/VALIDATION.md` para evidências e limitações, atualizando-o ao validar novas mudanças.

## Etapa 2 — clubes personalizados

- SVGs originais em `frontend/src/assets/shields/`: nunca redesenhar, alterar geometria ou substituir por ícones. Importar com `?react` usando SVGR com `svgo: false`.
- `TeamBadge` é a única implementação visual dos templates. Aplicar `--primary-color` e `--secondary-color` por instância; preservar branco e `viewBox`. Clubes sem template usam iniciais neutras.
- `POST /teams/custom` recebe somente nome, país, template e duas cores #RRGGBB. `IsCustom` e força inicial são definidos no backend.
- `PlayerGenerator` permanece no Domain, sem EF. São 11 nomes únicos, idades 18–32 e habilidades 50–80 inclusivas. Posições: 1 GK, 4 DEF, 4 MEI, 2 ATT, com slots internos do 4-4-2.
- Força inicial = média truncada dos 11 jogadores. Não modificar a soma da força com média de habilidade no simulador.
- `IGameStore.CreateTeamWithPlayers` deve persistir clube e elenco na mesma transação. Testar rollback real, não apenas fake.
- SQLite existe somente em testes relacionais em memória. Produção continua MySQL/Pomelo; nunca usar EnsureCreated no MySQL existente.
- `AddCustomTeamBadges` foi gerada sem aplicação. Antes de aplicar: backup, preflight, revisão da FK anterior pendente e autorização do usuário.
- Competições estão apenas documentadas em `docs/ARCHITECTURE.md`; nenhuma entidade/tabela/endpoint de campeonato deve ser criada nesta etapa.
- Testes de SVG e formulário: `npm --prefix frontend run test`. Testes usam os desenhos reais, incluindo verificação de hashes.
