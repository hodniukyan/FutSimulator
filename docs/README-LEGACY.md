# ⚽ Mini Brasfoot API + Frontend

Projeto desenvolvido com foco em aprendizado de Back-end (C# / .NET), Front-end (HTML, CSS, JS) e integração completa entre API e interface, simulando partidas de futebol no clássico estilo Brasfoot. A aplicação foi construída do zero, priorizando a lógica, o fluxo de dados e a comunicação entre as camadas.

---

## 🚀 Sobre o Projeto

O **Mini Brasfoot** é uma aplicação interativa que permite:

- Criar e gerenciar times.
- Criar jogadores e vinculá-los aos clubes.
- Simular partidas entre dois times selecionados.
- Visualizar o placar final e os autores dos gols.
- Interagir com uma interface web temática com design imersivo dos anos 90.

---

## 🏗️ Tecnologias Utilizadas

**Back-end**

- **C# & .NET:** Minimal API
- **Entity Framework Core:** ORM para mapeamento de dados
- **SQLite:** Banco de dados relacional leve

**Front-end**

- **HTML5 & CSS3:** Tematização retrô (estilo interface de sistemas dos anos 90)
- **JavaScript (Vanilla JS):** Consumo da API via `fetch` e manipulação do DOM

---

## 🧠 Conceitos Aplicados

Este projeto foi um laboratório prático para consolidar diversos fundamentos do desenvolvimento:

- Estruturação de API RESTful (CRUD).
- Integração e modelagem de banco de dados, incluindo a relação entre entidades (Time → Jogadores).
- Lógica de programação e probabilidade aplicadas à simulação de eventos.
- Tratamento de erros HTTP (400, 404, 500) e validações consistentes.
- Separação de responsabilidades e debug de problemas comuns (CORS, portas, rotas).

---

## ⚙️ Funcionalidades e Rotas da API

### 🟢 Times

- `GET /teams` → Lista todos os times cadastrados.
- `GET /teams/{id}` → Busca os detalhes de um time específico.
- `POST /teams` → Cria um novo time.
- `PUT /teams/{id}` → Atualiza os dados de um time.
- `DELETE /teams/{id}` → Remove um time do sistema.

### 🔵 Jogadores

- `POST /players` → Cria um novo jogador.
- `GET /teams/{id}/players` → Lista todos os jogadores pertencentes a um time específico.
- `PUT /teams/{id}/players` → Atualiza os dados de um jogador

### 🔴 Partidas e Simulação

- `POST /match?team1Id=X&team2Id=Y` → Inicia a simulação entre duas equipes.

A simulação de partidas **não é determinística**, buscando o realismo do esporte:

1.  A força base do time e a soma das habilidades dos jogadores compõem o peso da equipe.
2.  A partida é dividida em várias "jogadas".
3.  Em cada jogada, um número aleatório é gerado com base na força de cada time. O maior valor vence a disputa, podendo resultar em gol.
4.  Times mais fortes têm maior probabilidade de vitória, mas a aleatoriedade controlada permite "zebras".

**Exemplo de Retorno (JSON):**

````json
{
  "Team1": "Barcelona",
  "Team2": "Real Madrid",
  "Score": "2 x 1",
  "Goals": [
    { "team": "Barcelona", "player": "Messi" },
    { "team": "Real Madrid", "player": "Benzema" }
  ]
}
````
## 🎨 Integração Front-end e UI

O front-end foi desenhado para invocar a nostalgia dos primeiros managers de futebol.

* **Design Retrô:** Fontes monospace, layout funcional e fundo com efeito de *pixel grid/CRT*.
* **Fluxo de Interação:** O JavaScript carrega dinamicamente os times da API para preencher os *dropdowns*. Após a seleção, o usuário clica em "Jogar", o front-end aciona a rota `/match` e renderiza instantaneamente o placar e os artilheiros na tela.

---

## ⚠️ Validações e Testes Implementados

Para garantir a estabilidade do sistema, as seguintes regras de negócio foram aplicadas:

* Nome do time e país de origem são obrigatórios.
* A força do time e a habilidade (*skill*) do jogador devem ser maiores que zero.
* Um jogador só pode ser cadastrado se vinculado a um time existente na base.

O projeto passou por testes abrangentes, incluindo criação de dados via Swagger, simulação direta de partidas, envio de dados inválidos para validar o tratamento de erros e chamadas via Postman.

---

## 💡 Melhorias Futuras

O projeto continua em evolução. As próximas atualizações previstas incluem:

- [ ] Renderização de escudos dos times na interface web.
- [ ] Registro e histórico das partidas passadas.
- [ ] Sistema de pontuação e ranking de times (Tabela do Campeonato).
- [ ] Implementação de autenticação de segurança (JWT).
- [ ] Deploy da aplicação em nuvem.

---

## 📎 Como Rodar o Projeto

### Back-end

1. Navegue até a pasta do projeto `.NET` no terminal.
2. Execute o comando:
   ```bash
   dotnet run
3. Acesse a documentação interativa em: `http://localhost:5063/swagger`

### Front-end

1. Abra a pasta do front-end no VS Code.
2. Inicie o arquivo `index.html` utilizando a extensão **Live Server**.

---

## 👨‍💻 Autor

Desenvolvido por **Yan Hodniuk** 🚀

> *Projeto criado com foco em aprendizado prático, evoluindo de conceitos básicos à integração end-to-end.*
````


