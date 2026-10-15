# ⚽ Mini Brasfoot

Jogo de gerenciamento de futebol de **Yan Hodni**, desenvolvido como projeto de aprendizado de C#, .NET e integração frontend/backend. A fase 1 organiza a aplicação existente e prepara sua expansão incremental.

## Stack e estrutura

- ASP.NET Core/.NET 10, Minimal APIs e ILogger.
- **MySQL existente**, EF Core 9.0.15 e Pomelo 9.0.0.
- React 19, TypeScript, Vite, Tailwind, shadcn/ui, React Router, TanStack Query, Axios, React Hook Form/Zod e Lucide.
- xUnit para regras e integração HTTP.

```text
backend/
  MiniBrasfoot.Api/             HTTP, configuração, middleware
  MiniBrasfoot.Application/     casos de uso, contratos, validações
  MiniBrasfoot.Domain/          entidades e simulação independente do banco
  MiniBrasfoot.Infrastructure/  EF Core, MySQL, migrations
  MiniBrasfoot.Tests/           testes automatizados
frontend/                      React por funcionalidades
legacy/frontend/               HTML/CSS/JS original preservado
tools/DatabaseProbe/            preflight MySQL somente leitura
docs/                          arquitetura, banco, roadmap e validação
```

O README anterior foi preservado integralmente em [docs/README-LEGACY.md](docs/README-LEGACY.md), incluindo contexto, autoria e evolução do estudo. Ele contém instruções antigas (SQLite/Live Server) que não descrevem a configuração atual.

## Requisitos

- SDK .NET 10 (ambiente validado com 10.0.201).
- Node 22.12+ (validado com 22.20.0) e npm.
- Seu servidor MySQL em execução. O ambiente existente usa MySQL 8.0.45 em localhost:3306 e banco brasfootdb.
- Docker é opcional e não é necessário para usar o banco existente.

## Executar localmente

Guia rápido: [como iniciar o projeto](docs/INICIAR-PROJETO.md). No uso diário, mantenha o MySQL ligado e abra dois terminais na raiz `BrasfootAPI`.

Terminal 1 - backend:

```powershell
dotnet run --project backend/MiniBrasfoot.Api --launch-profile http
```

API: http://localhost:5063. Swagger em desenvolvimento: http://localhost:5063/swagger.

A conexão local original está preservada em `backend/MiniBrasfoot.Api/appsettings.Development.json`. Caso seu MySQL exija senha, use configuração externa, sem editar arquivos versionados:

```powershell
# Execute no seu terminal local, substituindo o exemplo; não compartilhe senhas no chat.
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;database=brasfootdb;user=SEU_USUARIO;password=SUA_SENHA;" --project backend/MiniBrasfoot.Api
```

Alternativamente defina `ConnectionStrings__DefaultConnection` no ambiente. A API detecta a versão do servidor; `Database__ServerVersion` permite informar a versão real explicitamente. Para produção, a conexão deve ser fornecida externamente.

Terminal 2 - frontend:

```powershell
npm.cmd --prefix frontend run dev
```

Frontend: http://localhost:5173. Configure `VITE_API_URL` em `frontend/.env.local` se mudar host/porta. Nunca coloque senhas em variáveis VITE_ — elas são públicas no navegador.

Na primeira instalação ou quando as dependências mudarem, execute antes:

```powershell
dotnet restore BrasfootAPI.sln
npm.cmd --prefix frontend ci
```

Use Ctrl + C para parar cada processo. `npm.cmd` evita o bloqueio de npm.ps1 no PowerShell. Migrations são aplicadas somente quando houver schema pendente e após backup/revisão/autorização; não fazem parte da inicialização diária.

O `.env.example` da raiz documenta variáveis do backend e Compose. `dotnet run` não carrega `.env` automaticamente. Não há migrations automáticas na inicialização.

## Funcionalidades e contratos atuais

| Método | Rota | Comportamento |
| --- | --- | --- |
| GET/POST | /teams | Listar/criar times |
| POST | /teams/custom | Criar clube personalizado e seus 11 jogadores atomicamente |
| GET/PUT/DELETE | /teams/{id} | Consultar/atualizar/excluir time |
| POST | /players | Criar jogador vinculado a um time existente |
| GET/PUT/DELETE | /players/{id} | Consultar/atualizar/excluir jogador |
| GET | /teams/{id}/players | Listar elenco do time |
| PUT/DELETE | /teams/{id}/players | Alias legado: **id é do jogador** |
| POST | /match?team1Id=X&team2Id=Y | Simular amistoso |
| GET | /health | Disponibilidade do processo, sem verificar banco |

O frontend permite simular amistosos, listar clubes em `/clubs` e criar clubes em `/clubs/new`, com prévia dos três escudos originais e cores dinâmicas. Após o cadastro, exibe o clube e seus 11 jogadores. Escudos são reutilizados nos cards, seleção de times e placar; clubes anteriores recebem iniciais neutras.

### Clubes personalizados — Etapa 2

```http
POST /teams/custom
Content-Type: application/json

{
  "name": "FutSimulator FC",
  "country": "Brasil",
  "badgeTemplate": "shield-hexagonal",
  "primaryColor": "#FF6B00",
  "secondaryColor": "#111827"
}
```

Retorna **201 Created**, `Location: /teams/{id}` e `{ "team": { ... }, "players": [ ... ] }`. Campos inválidos retornam **400** com ProblemDetails. Os três templates aceitos são `shield-hexagonal`, `shield-round` e `shield-triangular`. Nome/país são normalizados com trim, obrigatórios e limitados a 100 caracteres; cores exigem #RRGGBB e são persistidas em maiúsculas.

O backend gera 11 nomes fictícios únicos por elenco, idades de 18 a 32 e habilidades de 50 a 80, com todos os limites inclusivos. Formação 4-4-2: 1 GK, 4 DEF, 4 MEI, 2 ATT. Zagueiros/laterais e volantes/meias usam os códigos agrupados existentes. Força inicial é a média truncada das habilidades. Não envie `strength` ou `isCustom`: não fazem parte do contrato de criação personalizada.

SVGs são importados como componentes React via SVGR, sem otimização dos desenhos. `TeamBadge` aplica variáveis CSS locais; branco, bolas, coroa e geometria permanecem iguais. Veja [arquitetura](docs/ARCHITECTURE.md) para adicionar templates e [entrega da Etapa 2](docs/ETAPA2.md) para arquivos e pendências.

Nome/país/posição são obrigatórios; força e habilidade devem ser positivas. Essas validações também se aplicam ao PUT. Atualizar jogador não transfere TeamId. Não se pode jogar contra o mesmo clube ou excluir clube com jogadores (409).

As respostas de sucesso conservam os campos existentes. Os erros agora usam `application/problem+json`, com `status`, `title` e `traceId`; clientes devem ler `title`.

### Simulação preservada

Força efetiva = força do time + média truncada das habilidades do elenco (incluindo goleiros). Diferença >30 gera nível massacre, >10 favorito, demais equilibrado. A ramificação de zebra tem 20% de chance e pode empatar. Goleiros identificados por `GK` não marcam; autores são sorteados uniformemente e os gols aparecem ordenados por minuto (1–90). Não foi adicionada vantagem de mando.

Exemplo de formato de retorno:

```json
{
  "team1": "Casa",
  "team2": "Fora",
  "nivel": "equilibrado",
  "score": "1 x 0",
  "goals": [{ "minute": 42, "team": "Casa", "player": "Atacante" }]
}
```

Histórico ainda não é persistido; seu modelo existente foi preservado para uma próxima etapa.

## Uniformes personalizados

A criação de clubes oferece quatro camisas SVG em estilo 8-bit: listras horizontais, verticais, liso e meio a meio. Selecione o modelo em `/clubs/new`; as cores acompanham a identidade do clube. O liso usa somente a primária, com gola/contorno neutros. Cards de clubes e o resultado do cadastro também mostram a camisa.

Campo opcional adicional em `POST /teams/custom`:

```json
{ "modeloDoUniforme": "uniforme-meio-a-meio" }
```

Acrescente ao corpo completo do clube documentado acima. Aceitos: `uniforme-listrado-horizontal`, `uniforme-listrado-vertical`, `uniforme-liso` e `uniforme-meio-a-meio`. Omitir o campo preserva clientes anteriores e assume liso; valor null/desconhecido retorna 400. Clube anterior sem modelo recebe apresentação lisa com fallback seguro, sem atualização de seus dados.

Persistência: `Teams.ModeloDoUniforme`, opcional, até 32 caracteres. A migration `AdicionarModeloDeUniforme` precisa ser aplicada após backup/revisão/autorização antes de usar a API nova no MySQL, inclusive GET de clubes. SVGs somente em `frontend/src/assets/kits/`. Nenhuma tabela nova para uniformes ou competições.

O torneio continua apenas planejado em [PLANEJAMENTO-COMPETICOES.md](docs/PLANEJAMENTO-COMPETICOES.md), com 16 clubes, clube acompanhado e botões Avançar/Fechar conforme o resultado.

## Banco e migrations

Os arquivos SQLite antigos continuam intactos, mas a aplicação usa MySQL. A migration `InitialMySql` foi preservada. `AddPlayerTeamRelationship` prepara índice e FK com exclusão restrita; foi gerada **sem aplicação ao banco**.

Na Etapa 2, `AddCustomTeamBadges` acrescenta quatro colunas em Teams e também permanece **sem aplicação**. O código novo requer essas colunas para consultar/gravar times no MySQL. Antes de iniciar a API nova contra o banco existente, revise/aplique as migrations pendentes após backup e autorização. SQL aditivo de revisão: [docs/mysql-custom-team-badges.sql](docs/mysql-custom-team-badges.sql). As etapas estão em [docs/DATABASE.md](docs/DATABASE.md).

```powershell
# Apenas SELECT: versão, contagens, órfãos, histórico de migrations e FKs.
dotnet run --project tools/DatabaseProbe

# Verificar consistência entre modelo e snapshot sem acessar/aplicar ao banco.
dotnet ef migrations has-pending-model-changes --project backend/MiniBrasfoot.Infrastructure --startup-project backend/MiniBrasfoot.Api --no-build
```

Antes de aplicar qualquer migration, faça backup, revise o histórico/schema e confirme ausência de órfãos. Consulte [docs/DATABASE.md](docs/DATABASE.md) e o script incremental [docs/mysql-player-relationship.sql](docs/mysql-player-relationship.sql). Não use EnsureCreated ou apague migrations para contornar problemas.

## Verificação

```powershell
dotnet build BrasfootAPI.sln -m:1 /nr:false
dotnet test backend/MiniBrasfoot.Tests/MiniBrasfoot.Tests.csproj --no-build -m:1 /nr:false
npm --prefix frontend run lint
npm --prefix frontend run build
npm --prefix frontend run test
```

O parâmetro `-m:1 /nr:false` evita limitações de subprocessos MSBuild em ambientes Windows restritos. VSTest pode exigir execução fora do sandbox para conectar seus processos.

Os testes HTTP usam fake de persistência e não escrevem no seu banco. Veja [docs/VALIDATION.md](docs/VALIDATION.md) para verificações efetivamente executadas e limitações.

## Docker opcional

O Dockerfile foi alinhado ao .NET 10. O Compose inicia somente a API; seu MySQL continua sendo o existente. Para usar, copie `.env.example` para `.env`, configure sua conexão com host `host.docker.internal` (MySQL do Windows visto do container), credenciais e execute `docker compose up --build`. API no container: http://localhost:8080; frontend deve usar essa URL.

Docker não estava instalado/disponível durante a validação; esse fluxo ainda precisa de teste no ambiente com Docker.

## Documentação e evolução

- [Arquitetura](docs/ARCHITECTURE.md)
- [Banco de dados](docs/DATABASE.md)
- [Roadmap](docs/ROADMAP.md)
- [Auditoria inicial](docs/AUDIT.md)
- [Guia da Etapa 2 para quem vem do WebForms](output/pdf/FutSimulator-Etapa2-Guia-Para-Quem-Vem-do-WebForms.pdf)
- [Instruções para agentes](AGENTS.md)

Campeonatos, transferências, finanças, temporadas, saves e autenticação permanecem para etapas futuras.
