# Roadmap incremental

## Fase 1 — fundação

Separação de responsabilidades, preservação do MySQL, contratos DTO, tratamento HTTP, testes, frontend React inicial e documentação. Validar o banco real e revisar/aplicar a FK somente após backup/preflight.

## Próximas etapas propostas

### Uniformes e decisões de competição (14/10/2026)

Uniformes 8-bit no cadastro: quatro modelos SVG, prévia nas cores do clube, validação/persistência do identificador e fallback para clubes anteriores. Migration aguardando aplicação autorizada.

Competições continuam sem implementação por decisão do usuário. Versão inicial com exatamente 16 clubes, clube acompanhado, uma partida por vez e navegação Avançar/Fechar conforme vitória/derrota. Arquitetura, propostas e pendências: [PLANEJAMENTO-COMPETICOES.md](PLANEJAMENTO-COMPETICOES.md). Mais participantes, ligas e pontos corridos ficam para evolução posterior.

### Etapa 2 — implementada, schema aguardando aplicação autorizada

Criação de clube personalizado via POST /teams/custom, elenco automático 4-4-2, força inicial no backend, gravação atômica, SVGs originais com cores por instância, listagem/formulário responsivos e escudos no placar. Migration AddCustomTeamBadges preparada sem aplicação. Testes cobrem contratos antigos, validação, geração, compatibilidade com simulação e rollback em banco de teste.

Próximo passo operacional: backup/preflight/revisão das migrations pendentes e autorização antes de atualizar o MySQL. Próxima etapa funcional: competições mata-mata com 4/8/16/32 clubes, sorteio controlável, pênaltis e avanço automático, conforme a proposta em ARCHITECTURE.md. Reprodução visual de partidas em 30 segundos fica nessa etapa futura; o simulador atual permanece intacto.

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
