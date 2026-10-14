# Banco de dados — MySQL existente

Decisão do usuário: preservar MySQL, conexão local e dados. PostgreSQL não faz parte desta fase.

## Modelo atual

| Tabela | Campos |
| --- | --- |
| Teams | Id (PK identity), Name, Country, Strength |
| Players | Id (PK identity), Name, Age, Skill, TeamId, Position |

Strings continuam longtext NOT NULL para não reduzir colunas existentes. Validações de nome/país/posição e valores positivos ficam nos casos de uso. Age e Position não ganham restrições novas nesta fase.

## Etapa 2 — colunas aditivas de clubes

`AddCustomTeamBadges`, gerada sem aplicação, adiciona a Teams:

| Coluna | Tipo MySQL | Compatibilidade |
| --- | --- | --- |
| IsCustom | tinyint(1) NOT NULL DEFAULT 0 | Clubes anteriores continuam não personalizados |
| BadgeTemplate | varchar(32) NULL | Template ausente recebe fallback visual |
| PrimaryColor | varchar(7) NULL | Cor #RRGGBB, definida na criação personalizada |
| SecondaryColor | varchar(7) NULL | Cor #RRGGBB, definida na criação personalizada |

Não altera Name/Country/Strength, não recria tabelas e não modifica elencos existentes. Armazena apenas identificador do modelo e cores, nunca SVG. Novos clube/jogadores são inseridos em transação explícita; atomicidade requer tabelas transacionais InnoDB. Não alterar engines automaticamente.

O modelo novo consulta as colunas adicionais em GET /teams; portanto o MySQL existente precisa do schema atualizado antes de usar a API nova. Não foi executado database update, DDL ou cadastro no banco nesta entrega. O teste SQLite usa conexão :memory: exclusivamente no projeto Tests, não os arquivos legados nem a configuração de produção.

SQL específico de revisão: `mysql-custom-team-badges.sql`, gerado da migration anterior até AddCustomTeamBadges. Ele presume que AddPlayerTeamRelationship já consta no histórico. Se essa FK ainda estiver pendente, o update normal tentará aplicá-la antes das quatro colunas: conferir órfãos e histórico real novamente, sem confiar nas contagens antigas desta documentação.

DDL MySQL faz commit implícito: START TRANSACTION no script gerado não garante rollback dos ALTER TABLE. Isso é diferente da transação de INSERT do clube/elenco, que é verificada nos testes. Revisar uma migration parcialmente executada requer comparar colunas e histórico, sem rodar o script cegamente.

Após backup, revisão em cópia e autorização explícita, gerar o script COMPLETO das migrations realmente pendentes usando a última migration aplicada como origem:

```powershell
dotnet ef migrations list --project backend/MiniBrasfoot.Infrastructure --startup-project backend/MiniBrasfoot.Api
dotnet ef migrations script ULTIMA_MIGRATION_APLICADA AddCustomTeamBadges --project backend/MiniBrasfoot.Infrastructure --startup-project backend/MiniBrasfoot.Api --output artifacts/pending-migrations.sql
```

Substituir o marcador pelo ID conferido em __EFMigrationsHistory; não executar esse exemplo literalmente. A aplicação continua sem migrations automáticas. Evitar rollback Down de AddCustomTeamBadges em banco com clubes personalizados: ele remove as colunas e seus dados de identidade.

`Goal` é um resultado de simulação; `Historico` existe como classe, mas não tem tabela/DbSet. Nenhuma tabela futura foi criada.

## Provider e conexão

- .NET 10, EF Core/Relational/Design 9.0.15, Pomelo 9.0.0. O Pomelo 9 suporta a família EF Core 9; não atualizar EF para 10 isoladamente.
- A conexão original de desenvolvimento foi preservada em `backend/MiniBrasfoot.Api/appsettings.Development.json`.
- Produção exige `ConnectionStrings__DefaultConnection` ou user-secrets no desenvolvimento.
- A factory de design time usa variável de ambiente, fallback local sem senha e versão `8.0.0` para gerar scripts offline. Configure `Database__ServerVersion` com a versão REAL antes de avaliar SQL para seu servidor (MariaDB: sufixo `-mariadb`).
- Preflight real confirmou MySQL 8.0.45, brasfootdb, 6 times, 11 jogadores, zero órfãos e somente InitialMySql aplicada. Ainda não há FK Players -> Teams. A sondagem inicial falhou por restrição de acesso local do sandbox; a consulta fora dele funcionou.

## Migrations preservadas e pendentes

`20260406022610_InitialMySql` foi movida para Infrastructure sem alterar seu ID ou suas operações. O namespace e o nome do DbContext continuam iguais. A mudança de assembly não exige recriação do banco.

`AddPlayerTeamRelationship` acrescenta somente:

- índice IX_Players_TeamId;
- FK Players.TeamId -> Teams.Id, ON DELETE RESTRICT.

Essa migration foi GERADA, não aplicada. O script incremental está em `mysql-player-relationship.sql`. Não contém operações de remoção de tabelas ou dados. A criação da FK pode bloquear temporariamente a tabela e falhar se houver órfãos.

## Procedimento antes de aplicar

1. Faça backup do banco existente com MySQL Workbench/mysqldump e confirme recuperação em ambiente separado.
2. Confirme `SELECT VERSION()` e `SELECT DATABASE()` usando a conexão correta.
   A ferramenta `dotnet run --project tools/DatabaseProbe` executa apenas SELECT e informa versão, contagens, órfãos, migrations e quantidade de FKs, sem imprimir conexão ou dados dos jogadores.
3. Confira o histórico e esquema existentes:

```sql
SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId;
SHOW CREATE TABLE Teams;
SHOW CREATE TABLE Players;
SELECT p.Id, p.TeamId FROM Players p LEFT JOIN Teams t ON t.Id = p.TeamId WHERE t.Id IS NULL;
SELECT Id FROM Teams WHERE Strength <= 0 OR TRIM(Name) = '' OR TRIM(Country) = '';
SELECT Id FROM Players WHERE Skill <= 0 OR TRIM(Name) = '' OR TRIM(Position) = '';
```

4. Se `__EFMigrationsHistory` não existir ou InitialMySql não estiver registrada, não executar update cegamente: as tabelas podem existir manualmente. Compare o schema antes de definir o baseline.
5. Se houver órfãos, apresente os registros para decisão do usuário; não excluir ou reassociar automaticamente.
6. Gere/revise o script com a versão correta e teste primeiro em cópia do banco.
7. Só depois execute conscientemente:

```powershell
dotnet ef database update --project backend/MiniBrasfoot.Infrastructure --startup-project backend/MiniBrasfoot.Api
```

A API não aplica migrations ao iniciar. Operações CRUD utilizam as mesmas tabelas/colunas, e a aplicação já bloqueia exclusão de times com jogadores antes da FK.

## SQLite legado

`brasfoot.db`, `brasfoot.db-wal` e `brasfoot.db-shm` foram preservados e ignorados pelo Git. Não são utilizados pelo provider atual. Não apagar/copiar isoladamente enquanto algum processo estiver usando SQLite; uma eventual importação exige identificar a fonte correta, backup consistente e verificar IDs/conteúdo antes de escrever no MySQL.

## Docker

Docker não estava disponível na auditoria. O Compose é opcional e contém somente a API, usando seu MySQL existente. Dentro do container, `localhost` identifica o container; configure host `host.docker.internal` para o MySQL do Windows, credenciais e permissões adequadas. Nenhum novo servidor/banco é iniciado automaticamente.
