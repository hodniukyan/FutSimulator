# Competições - arquitetura e decisões

Atualizado em 14/10/2026 (America/Cuiaba). **Planejamento aprovado parcialmente; implementação adiada pelo usuário.** Este documento não autoriza criar endpoints, tabelas, migrations ou telas de competição agora. A melhoria de uniformes tem prioridade.

## Decisões confirmadas pelo usuário

- Botão **Criar torneio** ao lado de **Clubes**, quando esta feature for implementada.
- Primeira versão: mata-mata de jogo único com **exatamente 16 clubes**, começando nas oitavas.
- Usuário seleciona os 16 participantes e um **clube acompanhado**, obrigatoriamente inscrito.
- Sorteio aleatório dos confrontos no backend, seguindo a convenção Random.Shared em produção e Random controlável em testes.
- **Uma partida por vez**, sem botão de simular rodada nesta primeira versão.
- Se o clube acompanhado perder: exibir **Fechar** e navegar para a página inicial.
- Se vencer e houver outra fase: exibir **Avançar** e seguir para o próximo jogo do acompanhamento.
- Preservar agora o algoritmo de MatchSimulator. Registrar sua evolução como melhoria futura em um card.
- Métodos, classes, tabelas, colunas, DTOs e componentes novos em português brasileiro, com identificadores sem acentos. Preservar contratos/nomes existentes necessários à compatibilidade.
- Regras em C#; frontend apresenta/interage. Banco apenas persiste dados e aplica integridade, sem procedures, functions ou triggers de negócio.
- Projetar a base para mais clubes e futuros formatos de liga/pontos corridos, sem implementar esses formatos agora.

## Base existente e extensão proposta

A Etapa 2 implementou clubes, escudos, elenco e amistosos; não implementou entidades/tabelas de competição. Reutilizar Team, Player, TeamBadge e MatchSimulator. API, Application, Domain e Infrastructure continuam no mesmo processo ASP.NET Core.

| Camada | Componentes previstos | Responsabilidade |
| --- | --- | --- |
| Domain | Competicao, ParticipanteDaCompeticao, Partida, EventoDaPartida | Estado e resultado da competição |
| Domain | GeradorDeChaveMataMata | Embaralhamento Fisher-Yates e montagem da chave |
| Domain | ResolvedorDePenaltis | Desempate separado do motor de amistosos |
| Application | ServicoDeCompeticoes | Orquestrar cadastro, validação, sorteio, acompanhamento, simulação e avanço |
| Application | DTOs e IArmazenamentoDeCompeticoes | Contratos HTTP e operações de persistência realmente necessárias |
| Infrastructure | Armazenamento EF, configurações e migration futura | MySQL, índices, FKs e transações |
| API | EndpointsDeCompeticoes | Adaptar HTTP sem colocar regras nos endpoints |
| React | PaginaCriarTorneio, PaginaTorneio, ChaveMataMata | Seleção, chave, acompanhamento e reprodução visual |

## Modelo de dados proposto

Quatro tabelas novas, com nomes em português. ClubeId referencia **Teams.Id**, preservando a tabela existente.

| Tabela | Colunas propostas |
| --- | --- |
| Competicoes | Id, Nome, Formato, Situacao, CriadaEmUtc, ClubeAcompanhadoId, ClubeCampeaoId, Versao |
| ParticipantesDaCompeticao | Id, CompeticaoId, ClubeId, PosicaoNoSorteio |
| Partidas | Id, CompeticaoId, Rodada, PosicaoNaChave, ClubeMandanteId, ClubeVisitanteId, Situacao, GolsMandante, GolsVisitante, PenaltisMandante, PenaltisVisitante, ClubeVencedorId, ProximaPartidaId, PosicaoNaProximaPartida, Versao |
| EventosDaPartida | Id, PartidaId, Ordem, Minuto, Tipo, ClubeId, JogadorId, NomeDoJogadorNoMomento |

Formato inicialmente aceita somente MataMata. Participantes, placares e vencedor de confrontos ainda indefinidos são nulos. Índices únicos: participante por competição/clube e confronto por competição/rodada/posição. Exclusão restrita de clubes participantes. Definir também a política de exclusão de jogador com eventos históricos antes de implementar a FK de eventos.

Gerar a estrutura de 15 partidas: 8 oitavas, 4 quartas, 2 semifinais e 1 final. Salvar o sorteio uma vez; recarregar a página nunca sorteia novamente. Não fixar esses números dentro da entidade geral: a validação da primeira versão exige 16; o gerador pode receber uma quantidade suportada quando o produto evoluir.

## Fluxo de criação

1. Receber nome, IDs dos 16 clubes e ID do clube acompanhado.
2. Validar quantidade, existência, ausência de duplicatas, inscrição do acompanhado e elencos compatíveis.
3. Embaralhar uma cópia dos participantes e formar pares consecutivos.
4. Montar as vagas das próximas fases.
5. Salvar competição, inscrições e chave numa transação.
6. Exibir a chave e disponibilizar o acompanhamento do primeiro jogo.

## Fluxo de partida e persistência

Consultar clubes/elencos, chamar o simulador existente, resolver eventual empate, persistir resultado/eventos e alimentar a próxima vaga. Na final, registrar campeão e conclusão da competição. Tudo em C#; resultado e progressão confirmados atomicamente.

Atualização concorrente deve usar Versao/atualização condicional para impedir dois resultados confirmados ou avanço duplicado. Uma nova requisição para partida concluída devolve o resultado já persistido. GET nunca simula. Não recalcular durante animação, refresh ou retomada de navegação.

Os eventos atuais possuem nomes; acrescentar metadados internos ClubeId/JogadorId para persistência inequívoca, pois nomes podem se repetir. Isso pode preservar a ordem do Random, probabilidades e resposta do endpoint legado de amistoso.

## Acompanhamento confirmado

O resultado esportivo fica persistido. **Fechar é navegação para a página inicial**, não exclusão do torneio nem reversão da derrota. A possibilidade de retomar/consultar posteriormente deve respeitar que uma eliminação já aconteceu e não pode sortear outro resultado.

Vitória intermediária: Avançar leva ao próximo confronto do clube acompanhado quando o adversário estiver definido. Derrota, inclusive nos pênaltis: Fechar. Não exibir Avançar para um jogo inexistente depois da final.

## Pontos de produto ainda a confirmar antes de implementar

- Como simular os outros jogos necessários para determinar o adversário do acompanhado. Proposta: o backend resolve esses confrontos uma única vez na preparação do próximo jogo, sem pedir um botão de rodada ao usuário. Isso ainda precisa de confirmação.
- Final vencida: proposta de tela de campeão com Encerrar/Voltar ao início, sem novo jogo.
- Desempate: proposta de cinco cobranças por equipe, encerramento antecipado quando matematicamente definido e alternadas se necessário; sem prorrogação. A regra exata ainda precisa de confirmação.
- Reprodução de 30 segundos veio do planejamento anterior. Confirmar duração total e tratamento dos pênaltis nesta nova experiência de acompanhamento.
- Definir se a competição interrompida após eliminação continua pendente ou recebe resultados finais ao ser consultada; não simular automaticamente o restante apenas por fechar a tela.
- Definir comportamento caso o elenco de um clube inscrito seja alterado/excluído durante o torneio. Revalidar antes da partida é o mínimo; snapshots de elenco exigem decisão adicional.

## Endpoints propostos

- POST /competicoes: criar/sortear, recebendo também clubeAcompanhadoId.
- GET /competicoes e GET /competicoes/{id}: listagem e chave.
- GET /competicoes/{id}/partidas/{partidaId}: resultado/eventos.
- POST /competicoes/{id}/partidas/{partidaId}/simular: confirmar simulação permitida.
- A operação de preparar/avançar o acompanhamento deverá ter contrato próprio após decidir a resolução dos demais jogos; não executar efeitos em GET.

## Interface e evolução

Seleção por cards com escudos (e uniformes quando disponíveis), busca e contador de 16 vagas. Seleção explícita do clube acompanhado. Chave por fases; em mobile, abas por rodada. Avançar/Fechar guiados pelo resultado persistido, com o backend validando progressão independentemente da tela.

Card planejado: **Evoluir o motor de simulação**. Revisar equilíbrio da força, influência das posições e distribuição dos autores dos gols, mantendo testes determinísticos. Não alterar o algoritmo junto com a criação do módulo de competição.

Ligas futuras reutilizam Competicao, inscrições, partidas e eventos. Acrescentar gerador de calendário, ServicoDePontosCorridos e CalculadoraDeClassificacao quando solicitados. Não criar tabelas vazias nem implementar pontos, saldo de gols e desempates agora.

## Verificações previstas para a futura implementação

16 inscritos únicos; acompanhado inscrito; sorteio determinístico em testes; pares sem repetição; chave de 15 jogos; avanço correto; empate/desempate; campeão; derrota/Fechar; vitória/Avançar; final sem próximo jogo; refresh sem novo sorteio/simulação; concorrência e rollback; compatibilidade dos amistosos existentes; navegação responsiva.
