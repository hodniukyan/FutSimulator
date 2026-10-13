# Auditoria inicial

Projeto originalmente concentrado em Program.cs: CRUD de times, cadastro/listagem/atualização/exclusão de jogadores, consultas EF e simulação. .NET alvo 10, SDK 10.0.201. EF/Pomelo 9.0.0, Swashbuckle 10.1.7.

Código usava MySQL e migration InitialMySql; README dizia SQLite. Arquivos SQLite também estavam presentes. Player.TeamId não tinha FK/índice. Historico era criado em memória e descartado.

Frontend em wwwroot usava HTML/CSS/JS, URL fixa localhost e fetch. `teams` estava fora do escopo no carregamento; `container` não era declarado na simulação. A API não servia os arquivos estáticos. Não havia testes automatizados.

Outros problemas: CORS aberto, Swagger em produção, exposição de entidades EF, validação ausente no PUT, ambiguidade de IDs nas rotas de jogador, Dockerfile .NET 8 para projeto net10.0 e falta de tratamento centralizado de exceções.

Build inicial passou com aviso de vulnerabilidade alta em Microsoft.OpenApi 2.4.1. dotnet test não executava testes. MySQL em localhost:3306 não respondeu à sondagem inicial. Docker não disponível; Node 22.20.0/npm 10.9.3 presentes.

O programa original está preservado em `Program-LEGACY.cs.txt`; README original em `README-LEGACY.md`; frontend original em `legacy/frontend`. Dados não foram apagados. Arquivos locais apareciam como não rastreados no Git; não foram realizados commits/push.
