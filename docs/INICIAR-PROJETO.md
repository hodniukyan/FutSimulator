# Como iniciar o projeto

Execute na pasta `BrasfootAPI`, com o serviço MySQL ligado e o schema atualizado para a versão do código.

## Toda vez que for usar

Abra dois terminais e deixe ambos abertos.

Terminal 1 - backend:

```powershell
dotnet run --project backend/MiniBrasfoot.Api --launch-profile http
```

Terminal 2 - frontend:

```powershell
npm.cmd --prefix frontend run dev
```

- Sistema: http://localhost:5173
- Clubes: http://localhost:5173/clubs
- Criar clube: http://localhost:5173/clubs/new
- Swagger: http://localhost:5063/swagger

Para encerrar cada processo, use Ctrl + C no respectivo terminal. O Workbench é um cliente; o serviço MySQL precisa estar ativo independentemente de sua janela estar aberta.

## Primeira instalação ou mudança de dependências

Antes de iniciar os dois processos:

```powershell
dotnet restore BrasfootAPI.sln
npm.cmd --prefix frontend ci
```

`npm.cmd` evita o bloqueio de execução de `npm.ps1` no PowerShell, sem alterar a política de segurança do Windows.

## Configuração opcional do frontend

O cliente usa http://localhost:5063 por padrão. Se mudar a porta da API, crie `frontend/.env.local` com `VITE_API_URL` apontando para a nova URL e reinicie o Vite. O arquivo `.env.example` serve como referência; não sobrescreva um `.env.local` que já tenha sua configuração. Variáveis VITE são públicas no navegador e não devem conter senhas.

## Quando atualizar o banco

Migration não é um comando de inicialização diária. Aplique somente quando houver alteração pendente de schema, após backup, conferência do histórico, revisão do script e autorização. A migration de uniforme adiciona `Teams.ModeloDoUniforme`; veja [DATABASE.md](DATABASE.md) e [mysql-modelo-de-uniforme.sql](mysql-modelo-de-uniforme.sql).

Não execute scripts repetidamente caso haja erro ou a coluna já exista. Conferir estrutura e `__EFMigrationsHistory` antes de retomar evita aplicar a mesma alteração duas vezes. A aplicação não executa migrations automaticamente.

## Diagnóstico rápido

- Tela não abre: conferir o terminal do Vite e a porta anunciada.
- Frontend não conecta à API: conferir o terminal do backend e `VITE_API_URL`.
- Banco indisponível: conferir serviço MySQL, credenciais e migrations pendentes; a mensagem não identifica sozinha a causa.
- Porta em uso: verificar se já existe outra instância do projeto antes de iniciar uma nova.
