# Entrega — Etapa 2, 13/10/2026 (America/Cuiaba)

Clubes personalizados, geração automática de elenco e escudos SVG com cores dinâmicas implementados. A migration ao MySQL existente aguarda backup, revisão e autorização. Nenhum banco/dado real foi alterado nesta entrega.

## Resultado

### Melhoria da Etapa 2 - 14/10/2026

- Quatro templates de uniformes 8-bit em SVG: horizontal, vertical, liso e meio a meio; seletor e prévia nas cores do clube, camisa nos cards e no retorno do cadastro.
- Identificador `ModeloDoUniforme` validado no backend e persistido na tabela Teams. Migration aditiva `AdicionarModeloDeUniforme` e SQL revisado em `mysql-modelo-de-uniforme.sql`, sem aplicação ao MySQL nesta entrega.
- Código novo em português e compatibilidade com requisições anteriores sem uniforme (modelo liso).
- **69 testes .NET e 26 testes frontend aprovados**, builds e lint concluídos. Evidências e limites em [VALIDATION.md](VALIDATION.md).
- Arquitetura e decisões futuras da competição guardadas em [PLANEJAMENTO-COMPETICOES.md](PLANEJAMENTO-COMPETICOES.md), sem implementar a rotina agora.
- Comandos de uso diário, instalação inicial e distinção de migrations documentados em [INICIAR-PROJETO.md](INICIAR-PROJETO.md).

- POST /teams/custom valida identidade, gera 11 jogadores, calcula força e persiste o conjunto atomicamente. Retorna 201/Location com `team` e `players`.
- Formação 4-4-2: 1 GK, 4 DEF (2 zagueiros + 2 laterais), 4 MEI (2 volantes + 2 meias), 2 ATT. O modelo existente agrupa as subfunções. Nomes fictícios únicos por elenco; idades 18–32 e habilidades 50–80; força = média truncada.
- /clubs e /clubs/new incluem seleção de modelo/cores, preview, loading/erro e elenco criado. TeamBadge é reutilizado na seleção e no placar de amistosos.
- SVGs originais movidos de frontend/assets/shields para frontend/src/assets/shields com hashes iguais antes/depois. SVGR sem otimização preserva branco, bolas, coroa e atributos vetoriais.
- Competições apenas documentadas em ARCHITECTURE.md. MatchSimulator não foi modificado.

## Arquivos criados

| Área | Arquivos (relativos à raiz) |
| --- | --- |
| Domain | backend/MiniBrasfoot.Domain/Rules/PlayerGenerator.cs |
| Application | backend/MiniBrasfoot.Application/Services/CustomTeamService.cs |
| Infrastructure | backend/MiniBrasfoot.Infrastructure/Configurations/TeamConfiguration.cs; Migrations/20261014003412_AddCustomTeamBadges.cs e .Designer.cs |
| Testes .NET | backend/MiniBrasfoot.Tests/PlayerGeneratorTests.cs; CustomTeamTests.cs; CustomTeamPersistenceTests.cs |
| Componentes | frontend/src/features/teams/components/TeamBadge.tsx; BadgeSelector.tsx; BadgePreview.tsx; ColorPicker.tsx; TeamBadge.test.tsx |
| Feature | frontend/src/features/teams/schema.ts; schema.test.ts; use-create-team.ts |
| Rotas | frontend/src/routes/TeamsPage.tsx; CreateTeamPage.tsx |
| Configuração | .gitattributes; frontend/src/vite-env.d.ts; frontend/vitest.config.ts |
| Documentação | docs/ETAPA2.md; docs/mysql-custom-team-badges.sql |
| Material de estudo | output/pdf/FutSimulator-Etapa2-Guia-Para-Quem-Vem-do-WebForms.pdf; scripts/gerar_guia_etapa2.py |
| Evidências | docs/images/etapa2-desktop.jpg; etapa2-mobile.jpg; etapa2-roster.jpg |

SVGs existentes relocalizados: shield-hexagonal.svg, shield-round.svg, shield-triangular.svg. Nenhum novo desenho criado. Fixture temporária de QA em artifacts/etapa2/mock-api.mjs (ignorada pelo Git) não faz parte do produto e não conecta ao banco.

## Arquivos modificados

- backend/MiniBrasfoot.Domain/Entities/Team.cs: quatro campos de identidade.
- backend/MiniBrasfoot.Application/DTOs/Contracts.cs, Interfaces/IGameStore.cs e Services/TeamService.cs: contratos e operação atômica; propriedades anteriores preservadas.
- backend/MiniBrasfoot.Infrastructure/Persistence/EfGameStore.cs e Migrations/AppDbContextModelSnapshot.cs: transação e snapshot atualizado.
- backend/MiniBrasfoot.Api/Program.cs e Endpoints/TeamEndpoints.cs: DI e novo endpoint.
- backend/MiniBrasfoot.Tests/ApiTests.cs, FakeGameStore.cs e MiniBrasfoot.Tests.csproj: versão explícita no host fake, contrato de store e dependências apenas de teste.
- frontend/package.json, package-lock.json e vite.config.ts: SVGR/Vitest e comando de testes.
- frontend/src/app/App.tsx, components/layout/AppLayout.tsx e routes/HomePage.tsx: rotas, navegação e escudos no amistoso.
- frontend/src/features/teams/api.ts e types.ts: cliente HTTP e tipos.
- AGENTS.md, README.md, docs/ARCHITECTURE.md, DATABASE.md, ROADMAP.md e VALIDATION.md: regras, contratos, instruções e evidências.

globals.css já tinha alterações locais e não foi editado nesta etapa. output/, scripts/ e tmp/ também já continham arquivos não rastreados e foram preservados.

O guia didático tem 18 páginas, com comparações ao WebForms, fluxos, explicação dos arquivos e exercícios. Sua fonte é scripts/gerar_guia_etapa2.py, que requer ReportLab e as fontes Segoe UI/Consolas do Windows. O material descreve a implementação sem alterar o código do jogo.

## Dependências adicionadas

- frontend/dev: vite-plugin-svgr 5.2.0 e vitest 5.0.4, versões fixadas no package.json/lockfile.
- Tests apenas: Microsoft.EntityFrameworkCore.Sqlite 9.0.15 e SQLitePCLRaw.bundle_e_sqlite3 3.0.5. O bundle explícito substitui a dependência nativa antiga reportada como vulnerável na primeira instalação.
- API/Infrastructure continuam MySQL/Pomelo/EF Core 9; SQLite não participa da DI de produção.

## Banco e endpoints

AddCustomTeamBadges adiciona IsCustom (false para anteriores), BadgeTemplate/PrimaryColor/SecondaryColor opcionais. Não altera colunas existentes. A criação usa transação de dados com rollback; DDL MySQL não é transacional da mesma maneira.

Novo: POST /teams/custom. Preservados: GET/POST /teams; GET/PUT/DELETE /teams/{id}; POST /players; GET/PUT/DELETE /players/{id}; GET /teams/{id}/players; aliases PUT/DELETE /teams/{id}/players; POST /match; GET /health e Swagger em desenvolvimento.

GET de clubes inclui campos novos de maneira aditiva. Clubes anteriores conservam IDs, força e elenco e usam fallback visual. As rotas históricas não editam o escudo; edição de identidade não foi solicitada nesta etapa.

## Verificação

- Build .NET com zero erros/avisos e 58 testes xUnit aprovados.
- Build React/TypeScript/Vite, ESLint e 16 testes Vitest aprovados.
- Testes cobrem templates, campos inválidos/null, cores, nomes únicos, posições, limites, semente determinística, força calculada e compatibilidade com simulação.
- Persistência relacional em memória verifica sucesso e rollback por falha de jogador, preservando registros anteriores. HTTP verifica 201/Location, ProblemDetails, GET/PUT, cadastro legado e amistoso.
- SVGs: hashes e todos os elementos/atributos originais, branco, viewBox e cores independentes.
- EF has-pending-model-changes sem diferenças; SQL Up revisado contém quatro ADD COLUMN e histórico, sem drops/updates de dados.
- Auditorias finais npm e NuGet (transitivas dos testes) sem vulnerabilidades reportadas.
- Browser com Vite de QA (5174) e API simulada (5064): preview azul/amarelo com coroa branca, bloqueio de cor inválida, 11 linhas de elenco, atualização da lista e escudos no placar. A 390px não houve overflow horizontal do documento. Nenhum clique gravou no MySQL. Sessões temporárias encerradas.

Detalhes e evidências: [VALIDATION.md](VALIDATION.md).

## Executar localmente e pendências

Antes de iniciar a API nova contra o MySQL: seguir DATABASE.md para backup, preflight e revisão em cópia; conferir a FK anterior pendente e obter autorização para aplicar migrations. O schema antigo não contém as colunas consultadas pela nova API.

Após atualização autorizada do schema, em terminais separados, na raiz:

```powershell
dotnet run --project backend/MiniBrasfoot.Api --launch-profile http
npm --prefix frontend ci
npm --prefix frontend run dev
```

API: http://localhost:5063. Interface: http://localhost:5173/clubs e http://localhost:5173/clubs/new. Configure frontend/.env.local com VITE_API_URL se mudar a porta. A conexão do banco permanece via configuração local/user-secrets/ambiente, sem segredos no frontend.

Repetir a validação:

```powershell
dotnet build BrasfootAPI.sln -m:1 /nr:false
dotnet test backend/MiniBrasfoot.Tests/MiniBrasfoot.Tests.csproj --no-build -m:1 /nr:false
npm --prefix frontend run lint
npm --prefix frontend run build
npm --prefix frontend run test
```

Pendências: aplicação autorizada das migrations e teste de escrita em cópia do MySQL real; competições/reprodução de 30 segundos em etapa futura. Docker não foi testado nesta etapa.
