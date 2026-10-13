# Roadmap incremental

## Fase 1 — fundação

Separação de responsabilidades, preservação do MySQL, contratos DTO, tratamento HTTP, testes, frontend React inicial e documentação. Validar o banco real e revisar/aplicar a FK somente após backup/preflight.

## Próximas etapas propostas

1. CRUD visual de clubes e jogadores, reutilizando os endpoints atuais; preservar escopo antes de criar telas completas.
2. Histórico de partidas: persistir uma única vez o mesmo resultado retornado, com IDs de clubes, gols e data UTC; definir política de exclusão de clubes históricos.
3. Elencos: hoje Player.TeamId representa vínculo atual. Ao introduzir temporadas, avaliar inscrição por temporada, sem duplicar dados prematuramente.
4. Campeonatos/classificação: Competition, Season, CompetitionEntry e Match conforme regras definidas; classificação derivada dos resultados inicialmente.
5. Calendário: rodada/data/status sobre partidas agendadas, separando simulação de agendamento.
6. Transferências: Transfer e histórico de vínculos quando houver regras de negociação e janelas.
7. Finanças: lançamentos contábeis associados ao clube/temporada, evitando saldo calculado sem origem auditável.
8. Save games: definir isolamento por GameSave antes de implementar múltiplas carreiras; clubes/jogadores atuais não são templates ainda.
9. Autenticação: incorporar identidade/autorização quando saves multiusuário exigirem; JWT não faz parte da fase 1.

Cada etapa exige requisitos próprios, modelagem, migrations e testes. Nenhuma das tabelas acima foi criada nesta execução.
