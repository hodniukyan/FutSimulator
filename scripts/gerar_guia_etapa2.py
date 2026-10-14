"""Gera o guia didático da Etapa 2 a partir do código auditado do projeto."""

from pathlib import Path
from html import escape
import json

from reportlab.pdfgen import canvas
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, PageBreak, Table, TableStyle,
    Preformatted, Flowable, Image, KeepTogether,
)
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.colors import HexColor, white
from reportlab.lib.pagesizes import A4
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont


RAIZ = Path(__file__).resolve().parents[1]
PASTA_SAIDA = RAIZ / "output" / "pdf"
PASTA_REVISAO = RAIZ / "tmp" / "pdfs" / "guia_etapa2"
ARQUIVO_PDF = PASTA_SAIDA / "FutSimulator-Etapa2-Guia-Para-Quem-Vem-do-WebForms.pdf"
LARGURA_PAGINA, ALTURA_PAGINA = A4
MARGEM = 44
LARGURA_UTIL = LARGURA_PAGINA - MARGEM * 2
AZUL = HexColor("#152C45")
LARANJA = HexColor("#D96316")
CINZA = HexColor("#526170")
CLARO = HexColor("#F1F5F9")
VERDE = HexColor("#157A70")
BORDA = HexColor("#DBE3EA")

pdfmetrics.registerFont(TTFont("Texto", "C:/Windows/Fonts/segoeui.ttf"))
pdfmetrics.registerFont(TTFont("TextoNegrito", "C:/Windows/Fonts/segoeuib.ttf"))
pdfmetrics.registerFont(TTFont("Codigo", "C:/Windows/Fonts/consola.ttf"))
pdfmetrics.registerFontFamily("Texto", normal="Texto", bold="TextoNegrito")

ESTILOS = {
    "titulo": ParagraphStyle("titulo", fontName="TextoNegrito", fontSize=24,
        leading=29, textColor=AZUL, spaceAfter=14),
    "subtitulo": ParagraphStyle("subtitulo", fontName="Texto", fontSize=12,
        leading=17, textColor=CINZA, spaceAfter=14),
    "corpo": ParagraphStyle("corpo", fontName="Texto", fontSize=10.5,
        leading=15, textColor=AZUL, spaceAfter=9),
    "secao": ParagraphStyle("secao", fontName="TextoNegrito", fontSize=13,
        leading=17, textColor=AZUL, spaceBefore=11, spaceAfter=7),
    "tabela": ParagraphStyle("tabela", fontName="Texto", fontSize=9.3,
        leading=13, textColor=AZUL, spaceAfter=0, splitLongWords=True),
    "cabecalho": ParagraphStyle("cabecalho", fontName="TextoNegrito", fontSize=9.3,
        leading=13, textColor=white, spaceAfter=0),
    "nota": ParagraphStyle("nota", fontName="Texto", fontSize=9.1,
        leading=13, textColor=CINZA, spaceAfter=8),
    "codigo": ParagraphStyle("codigo", fontName="Codigo", fontSize=8.5,
        leading=12, textColor=AZUL, backColor=CLARO, borderPadding=10,
        spaceBefore=6, spaceAfter=13),
}


def paragrafo(texto, estilo="corpo"):
    return Paragraph(texto, ESTILOS[estilo])


def secao(titulo):
    return paragrafo(titulo, "secao")


def codigo(texto):
    for linha in texto.splitlines():
        assert pdfmetrics.stringWidth(linha, "Codigo", 8.5) < LARGURA_UTIL - 18, linha
    return Preformatted(texto, ESTILOS["codigo"])


def imagem_proporcional(caminho, largura):
    imagem = Image(str(caminho))
    imagem.drawWidth = largura
    imagem.drawHeight = largura * imagem.imageHeight / imagem.imageWidth
    imagem.hAlign = "CENTER"
    return imagem


def tabela(cabecalhos, linhas, proporcoes):
    dados = [[paragrafo(escape(celula), "cabecalho") for celula in cabecalhos]]
    dados += [[paragrafo(celula, "tabela") for celula in linha] for linha in linhas]
    larguras = [LARGURA_UTIL * proporcao / sum(proporcoes) for proporcao in proporcoes]
    resultado = Table(dados, colWidths=larguras, repeatRows=1, hAlign="LEFT")
    resultado.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), AZUL),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [white, CLARO]),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("LEFTPADDING", (0, 0), (-1, -1), 9),
        ("RIGHTPADDING", (0, 0), (-1, -1), 9),
        ("TOPPADDING", (0, 0), (-1, -1), 8),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 8),
        ("LINEBELOW", (0, -1), (-1, -1), 0.5, BORDA),
    ]))
    return resultado


def destaque(titulo, texto):
    resultado = Table([[paragrafo(f"<b>{titulo}</b><br/>{texto}")]], colWidths=[LARGURA_UTIL])
    resultado.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, -1), HexColor("#FFF4E9")),
        ("LINEBEFORE", (0, 0), (0, -1), 3, LARANJA),
        ("LEFTPADDING", (0, 0), (-1, -1), 12),
        ("RIGHTPADDING", (0, 0), (-1, -1), 12),
        ("TOPPADDING", (0, 0), (-1, -1), 10),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 8),
    ]))
    return resultado


class DiagramaVertical(Flowable):
    """Desenha etapas e setas sem depender de imagens externas."""
    def __init__(self, etapas, altura_caixa=47):
        super().__init__()
        self.etapas = etapas
        self.altura_caixa = altura_caixa
        self.width = LARGURA_UTIL
        self.height = len(etapas) * (altura_caixa + 13) - 13

    def draw(self):  # Nome exigido pela API do ReportLab.
        desenho = self.canv
        posicao_y = self.height
        for indice, (titulo, descricao, cor) in enumerate(self.etapas):
            posicao_y -= self.altura_caixa
            desenho.setFillColor(CLARO)
            desenho.roundRect(0, posicao_y, self.width, self.altura_caixa, 6, fill=1, stroke=0)
            desenho.setFillColor(cor)
            desenho.roundRect(0, posicao_y, 35, self.altura_caixa, 6, fill=1, stroke=0)
            desenho.setFillColor(white)
            desenho.setFont("TextoNegrito", 11)
            desenho.drawCentredString(17.5, posicao_y + self.altura_caixa / 2 - 4, str(indice + 1))
            desenho.setFillColor(AZUL)
            desenho.setFont("TextoNegrito", 10)
            desenho.drawString(47, posicao_y + self.altura_caixa - 17, titulo)
            desenho.setFillColor(CINZA)
            desenho.setFont("Texto", 9)
            desenho.drawString(47, posicao_y + 12, descricao)
            if indice < len(self.etapas) - 1:
                meio = self.width / 2
                desenho.setStrokeColor(LARANJA)
                desenho.setLineWidth(1.1)
                desenho.line(meio, posicao_y - 1, meio, posicao_y - 11)
                desenho.line(meio, posicao_y - 11, meio - 3, posicao_y - 7)
                desenho.line(meio, posicao_y - 11, meio + 3, posicao_y - 7)
            posicao_y -= 13


def desenhar_rodape(desenho, documento):
    desenho.saveState()
    desenho.setStrokeColor(BORDA)
    desenho.line(MARGEM, 40, LARGURA_PAGINA - MARGEM, 40)
    desenho.setFont("Texto", 8)
    desenho.setFillColor(CINZA)
    desenho.drawString(MARGEM, 26, "FUTSIMULATOR  /  ETAPA 2  /  GUIA DE ESTUDO")
    desenho.drawRightString(LARGURA_PAGINA - MARGEM, 26, f"{documento.page:02d} / 18")
    if documento.page > 1:
        desenho.setFont("TextoNegrito", 8)
        desenho.drawString(MARGEM, ALTURA_PAGINA - 31, "DO WEBFORMS AO C# ORGANIZADO")
    desenho.restoreState()


PAGINAS = []


def adicionar_pagina(titulo, subtitulo, conteudo):
    numero = len(PAGINAS) + 1
    PAGINAS.append((titulo, [
        paragrafo(f'<a name="pagina_{numero}"/>{titulo}', "titulo"),
        paragrafo(subtitulo, "subtitulo"),
        *conteudo,
    ]))


adicionar_pagina("Do WebForms ao<br/>C# organizado", "FutSimulator | Etapa 2 | Guia prático de leitura do seu projeto", [
    Spacer(1, 15),
    destaque("Você já conhece a lógica. Agora vamos localizar cada responsabilidade.",
        "Validar campos, cadastrar um clube e mostrar o resultado continuam sendo tarefas familiares. "
        "A diferença é que a tela, o fluxo do cadastro, as regras e o banco estão em lugares diferentes."),
    Spacer(1, 18),
    secao("O que você vai conseguir acompanhar"),
    paragrafo("<b>1.</b> Quem executa o React, o C# e o SQL.<br/>"
        "<b>2.</b> O que fazem API, Application, Domain e Infrastructure.<br/>"
        "<b>3.</b> A viagem completa do clique em Criar clube até os 11 jogadores salvos.<br/>"
        "<b>4.</b> Onde alterar uma regra e onde colocar um breakpoint.<br/>"
        "<b>5.</b> Como separar problemas de tela, HTTP, configuração e banco."),
    secao("Como estudar"),
    paragrafo("Deixe o VS Code aberto ao lado do PDF. Leia uma seção e encontre os arquivos citados. "
        "Os caminhos são relativos à pasta <b>BrasfootAPI</b>. Um método só fica claro quando você vê "
        "quem o chama, o que ele recebe e o que devolve."),
    paragrafo("O texto explica o código auditado desta Etapa 2. Os nomes atuais em inglês são mantidos "
        "nas citações para você encontrá-los no editor. Exemplos novos usam português brasileiro; "
        "não foram feitas renomeações nem mudanças no jogo para produzir este material.", "nota"),
    Spacer(1, 18),
    paragrafo("Edição de estudo: 13/10/2026, America/Cuiaba.<br/>"
        "Base: código local, documentação do projeto e referências oficiais ao final.", "nota"),
])

adicionar_pagina("Seu roteiro de leitura", "Você pode ler em ordem ou voltar diretamente a uma dúvida.", [
    tabela(["Página", "Assunto", "Pergunta que responde"], [
        (f'<link href="#pagina_{numero}" color="#D96316">{numero:02d}</link>', titulo, pergunta)
        for numero, titulo, pergunta in [
            (3, "WebForms x este projeto", "Onde foi parar o code-behind?"),
            (4, "Onde tudo executa", "As camadas são servidores diferentes?"),
            (5, "Mapa das pastas", "O que eu abro primeiro?"),
            (6, "Inicialização e DI", "Quem cria as classes e chama os métodos?"),
            (7, "Fluxo de criação", "O que acontece depois do clique?"),
            (8, "Caso de uso e DTOs", "Quem valida e decide o cadastro?"),
            (9, "Entidades e gerador", "Como surgem os 11 jogadores?"),
            (10, "EF Core e transação", "Quando o banco realmente recebe os dados?"),
            (11, "React e formulário", "Como a tela muda sem postback?"),
            (12, "Escudos e cores", "Por que a cor muda sem salvar?"),
            (13, "Listagem e amistoso", "Como os clubes entram na partida?"),
            (14, "Migrations e comandos", "Atualizar o schema é iniciar a API?"),
            (15, "Erros e depuração", "Onde eu procuro quando quebra?"),
            (16, "Exercícios guiados", "Como eu aprendo mexendo com segurança?"),
            (17, "Vocabulário do código", "O que significam async, record e interface?"),
            (18, "Testes e referências", "Como verificar e continuar estudando?"),
        ]
    ], [0.12, 0.40, 0.48]),
    Spacer(1, 14),
    destaque("Pergunta para levar a todas as páginas", "Este trecho apresenta dados, decide uma regra, "
        "transporta uma mensagem ou acessa o banco? A resposta ajuda a escolher a camada correta."),
])

adicionar_pagina("Você já conhece metade disso", "Use WebForms como referência, sem tentar reproduzir todo o ciclo de uma página.", [
    paragrafo("No WebForms, você reconhece a página .aspx, o code-behind, eventos como "
        "<b>Page_Load</b> e o clique de um botão. Neste projeto, o formulário é React, "
        "e o servidor recebe pedidos HTTP que devolvem dados JSON. A separação por camadas "
        "também pode existir em WebForms; aqui ela está explícita em projetos C# separados."),
    tabela(["Referência em WebForms", "Onde procurar aqui"], [
        (".aspx e controles visuais", "CreateTeamPage.tsx e componentes React no navegador."),
        ("btnSalvar_Click no servidor", "O onSubmit começa no navegador. A ação de servidor entra em POST /teams/custom."),
        ("Regras no code-behind ou em uma classe de negócio", "CustomTeamService coordena o cadastro; PlayerGenerator gera o elenco."),
        ("Classe de acesso a dados", "EfGameStore e AppDbContext usam EF Core para falar com MySQL."),
        ("GridView e DataBind", "A tela recebe players e usa map para montar as linhas da tabela."),
        ("MasterPage / controles .ascx", "AppLayout organiza a moldura; componentes como TeamBadge são reutilizados."),
        ("ViewState / IsPostBack", "Não há ViewState do WebForms neste app. Estado do formulário e cache da consulta ficam no cliente."),
    ], [0.40, 0.60]),
    secao("O clique não chama uma classe C# diretamente"),
    codigo("// Exemplo didático de um evento WebForms; não é arquivo do projeto.\nprotected void SalvarClubeAoClicar(object origem, EventArgs argumentos)\n{\n    string nomeDoClube = txtNome.Text.Trim();\n    // Validar, gravar e atualizar a tela poderiam ocorrer aqui.\n}"),
    paragrafo("Aqui essas tarefas continuam existindo, mas o JavaScript envia uma mensagem ao servidor. "
        "O endpoint escolhe o serviço C#; o resultado volta pela rede, e o React atualiza a apresentação. "
        "Mudar uma cor da prévia não precisa de uma viagem ao servidor."),
    paragrafo("Referências conceituais: ciclo de página WebForms [1] e renderização React [2], página 18.", "nota"),
])

adicionar_pagina("Onde cada coisa executa", "Processos em execução e dependências entre projetos são mapas diferentes.", [
    DiagramaVertical([
        ("Navegador", "Executa o JavaScript do React e mostra os controles e SVGs.", LARANJA),
        ("ASP.NET Core / API - porta 5063", "Executa API, Application, Domain e Infrastructure no mesmo processo.", AZUL),
        ("Servidor MySQL - normalmente porta 3306", "Executa o SQL, guarda as tabelas e confirma as transações.", VERDE),
    ], altura_caixa=56),
    Spacer(1, 12),
    paragrafo("Em desenvolvimento, há ainda o <b>Vite na porta 5173</b>: um processo Node que entrega "
        "os arquivos e recompila o frontend. A tela React deste projeto é executada no navegador, "
        "não no Vite nem no servidor C#. O Workbench é um cliente do MySQL; abrir o Workbench "
        "não substitui iniciar o serviço do banco."),
    secao("Como os projetos C# se conhecem"),
    codigo("Api -> Application -> Domain\nApi -> Infrastructure -> Application -> Domain\n\nNo cadastro, Application chama IGameStore.\nEm execução, a DI entrega EfGameStore para essa interface."),
    destaque("Uma camada não é um microsserviço", "Você inicia a API uma vez. Ela carrega as bibliotecas "
        "das outras camadas. Não precisa abrir quatro terminais para quatro projetos do backend."),
    secao("Dependência em código x chamada em execução"),
    paragrafo("Application conhece o contrato IGameStore, não a classe de EF Core. Infrastructure "
        "implementa esse contrato. Isso permite ao cadastro pedir uma gravação sem importar DbContext. "
        "O fluxo de execução cruza a interface, mas a referência entre projetos continua apontando "
        "da infraestrutura para a aplicação."),
])

adicionar_pagina("Mapa do projeto", "Abra a solução para ver responsabilidades; abra os arquivos para ver o fluxo.", [
    tabela(["Pasta / projeto", "O que faz", "Arquivos de entrada"], [
        ("backend/<br/>MiniBrasfoot.Api", "Recebe HTTP, registra dependências, configura MySQL, CORS e erros.", "Program.cs<br/>Endpoints/TeamEndpoints.cs<br/>Middleware/ApiExceptionHandler.cs"),
        ("backend/<br/>MiniBrasfoot.Application", "Coordena os casos de uso, valida dados de entrada e define contratos.", "Services/CustomTeamService.cs<br/>DTOs/Contracts.cs<br/>Interfaces/IGameStore.cs"),
        ("backend/<br/>MiniBrasfoot.Domain", "Representa clubes/jogadores e regras independentes de HTTP/EF.", "Entities/Team.cs<br/>Entities/Players.cs<br/>Rules/PlayerGenerator.cs<br/>Rules/MatchSimulator.cs"),
        ("backend/<br/>MiniBrasfoot.Infrastructure", "Traduz a gravação para EF/MySQL e descreve o schema.", "Persistence/EfGameStore.cs<br/>Persistence/AppDbContext.cs<br/>Configurations/<br/>Migrations/"),
        ("backend/<br/>MiniBrasfoot.Tests", "Executa verificações sem gravar no MySQL do usuário.", "CustomTeamTests.cs<br/>PlayerGeneratorTests.cs<br/>CustomTeamPersistenceTests.cs"),
        ("frontend/src", "Apresentação, formulário, HTTP e estado no navegador.", "main.tsx<br/>app/App.tsx<br/>routes/CreateTeamPage.tsx<br/>features/teams/"),
    ], [0.26, 0.37, 0.37]),
    secao("Arquivos que dão suporte"),
    paragrafo("<b>BrasfootAPI.sln</b> reúne os projetos; um <b>.csproj</b> declara bibliotecas e referências. "
        "<b>Directory.Build.props</b> define net10.0 em comum; <b>global.json</b> orienta a escolha do SDK. "
        "<b>frontend/package.json</b> descreve scripts e dependências do frontend."),
    paragrafo("<b>docs/</b> explica decisões. <b>tools/DatabaseProbe</b> faz consultas de diagnóstico. "
        "<b>legacy/frontend</b> guarda a versão antiga. <b>scripts/</b> contém utilitários: eles só rodam "
        "quando alguém os executa. <b>output/pdf/</b> guarda materiais como este guia.", "nota"),
])

adicionar_pagina("Quem liga o backend?", "Program.cs monta a aplicação; a injeção de dependências monta os objetos.", [
    paragrafo("Ao executar <b>dotnet run</b> no projeto Api, Program.cs configura o host. Ele registra serviços, "
        "define como conectar ao MySQL, monta o tratamento HTTP, mapeia as rotas e inicia a escuta. "
        "Não existe um Page_Load de cada tela React no backend."),
    secao("Trecho real de Program.cs"),
    codigo("builder.Services.AddScoped<IGameStore, EfGameStore>();\nbuilder.Services.AddSingleton<PlayerGenerator>();\nbuilder.Services.AddScoped<CustomTeamService>();\n\nvar app = builder.Build();\napp.UseExceptionHandler();\napp.UseCors();\napp.MapTeamEndpoints();\napp.Run();"),
    tabela(["Registro", "Tradução para o dia a dia"], [
        ("AddScoped&lt;IGameStore, EfGameStore&gt;", "Quando alguém pedir IGameStore, entregue EfGameStore. No fluxo HTTP, a instância é compartilhada dentro do escopo daquela requisição."),
        ("AddScoped&lt;CustomTeamService&gt;", "Crie o coordenador do cadastro e forneça as dependências registradas que ele pede."),
        ("AddSingleton&lt;PlayerGenerator&gt;", "Compartilhe uma instância do gerador na aplicação. Ele não guarda um elenco mutável entre chamadas."),
    ], [0.46, 0.54]),
    secao("O construtor que parece uma declaração curta"),
    codigo("public sealed partial class CustomTeamService(\n    IGameStore store, PlayerGenerator generator)\n{\n    // A classe usa as dependências recebidas pelo construtor.\n}"),
    paragrafo("É um <b>construtor primário</b> de C#. store e generator são dependências recebidas. "
        "O endpoint pede CustomTeamService; a DI resolve a árvore necessária, inclusive AppDbContext. "
        "Esse DbContext é scoped por padrão no registro AddDbContext. Não é criado um por cada jogador."),
    paragrafo("Lifetimes: scoped é por escopo HTTP; singleton é compartilhado. Fonte [3], página 18.", "nota"),
])

adicionar_pagina("A viagem do clique ao banco", "Leia de cima para baixo. A resposta percorre o caminho de volta.", [
    DiagramaVertical([
        ("CreateTeamPage.tsx - clique em Criar clube", "handleSubmit valida o formulário com Zod, no navegador.", LARANJA),
        ("use-create-team.ts", "mutation.mutate inicia o envio e controla loading, sucesso e erro.", LARANJA),
        ("features/teams/api.ts + services/http.ts", "Axios envia POST /teams/custom com dados JSON para a API.", LARANJA),
        ("TeamEndpoints.cs - servidor C#", "Recebe CustomTeamInput e chama CustomTeamService.Create.", AZUL),
        ("CustomTeamService.cs", "Valida, pede o elenco, calcula a força e monta o clube.", VERDE),
        ("PlayerGenerator.cs", "Gera exatamente 11 jogadores em memória, sem banco.", VERDE),
        ("IGameStore -> EfGameStore", "A implementação inicia uma transação e faz duas gravações EF.", AZUL),
        ("MySQL -> DTO -> HTTP 201", "Commit confirma; o servidor devolve clube e jogadores com IDs.", AZUL),
        ("React exibe o clube e os 11 jogadores", "O cache de ['teams'] é invalidado para atualizar a listagem.", LARANJA),
    ], altura_caixa=46),
    Spacer(1, 13),
    destaque("Dois pedidos diferentes", "GET /teams apenas lista. POST /teams/custom cria clube e elenco. "
        "Abrir a página de criação ou trocar a cor da prévia não salva um clube."),
])

adicionar_pagina("Application: o cadastro", "CustomTeamService.Create é o coordenador do caso de uso.", [
    paragrafo("Um <b>caso de uso</b> é uma ação completa do sistema: neste caso, criar um clube com identidade "
        "e elenco. Ele não desenha controles nem escreve SQL. Ele organiza as decisões do cadastro."),
    secao("Sequência real, em trechos curtos"),
    codigo("Validate(input);\nvar players = generator.Generate();\n\n// Dentro da montagem de Team:\nStrength = (int)players.Average(player => player.Skill)\n\nawait store.CreateTeamWithPlayers(team, players, ct);\n// Depois: mapear o clube e cada jogador para DTOs de resposta."),
    tabela(["Decisão", "Onde está e por que fica no servidor"], [
        ("Nome e país", "Validate rejeita branco e mais de 100 caracteres após trim. A interface pode avisar antes, mas pedidos diretos à API também precisam ser validados."),
        ("Template e cores", "Só os três identificadores previstos e cores #RRGGBB. ToUpperInvariant normaliza as cores antes de persistir."),
        ("Força e IsCustom", "São definidos pelo caso de uso. O cliente não escolhe uma força arbitrária para o cadastro personalizado."),
    ], [0.26, 0.74]),
    secao("DTO não é a entidade do banco"),
    paragrafo("<b>CustomTeamInput</b> é o contrato de entrada: nome, país, template e duas cores. "
        "<b>CustomTeamDto</b> é a resposta: TeamDto + lista de PlayerDto. Estão em DTOs/Contracts.cs. "
        "São records para transportar dados; não fazem a gravação por conta própria."),
    codigo('{\n  "name": "Aurora FC", "country": "Brasil",\n  "badgeTemplate": "shield-round",\n  "primaryColor": "#165DFF", "secondaryColor": "#FFD700"\n}'),
    paragrafo("ASP.NET Core converte JSON em DTO de entrada e serializa DTO de resposta em JSON. "
        "Na rede, o contrato usa camelCase. Name é a propriedade C#; name é a chave JSON. "
        "A resposta 201 inclui Location: /teams/{id}.", "nota"),
])

adicionar_pagina("Domain: clube e jogadores", "Objetos do jogo e regras que podem funcionar sem HTTP ou MySQL.", [
    paragrafo("<b>Team.cs</b> representa um clube: Id, Name, Country, Strength e os quatro campos de identidade. "
        "<b>Players.cs</b> contém a classe Player: Id, Name, Age, Skill, TeamId e Position. "
        "Uma entidade pode existir só na memória; instanciá-la não a grava no banco."),
    tabela(["Função na formação", "Quantidade", "Código persistido"], [
        ("Goleiro", "1", "GK"), ("Zagueiros", "2", "DEF"), ("Laterais", "2", "DEF"),
        ("Volantes", "2", "MEI"), ("Meias", "2", "MEI"), ("Atacantes", "2", "ATT"),
    ], [0.54, 0.19, 0.27]),
    paragrafo("O gerador usa 11 slots nessa ordem. O modelo atual agrupa zagueiros/laterais como DEF "
        "e volantes/meias como MEI; não existe um campo separado de subfunção. A regra de goleiro "
        "continua usando GK exatamente em maiúsculas."),
    secao("Por que os nomes não se repetem?"),
    paragrafo("PlayerGenerator combina 12 nomes com 12 sobrenomes, formando 144 opções. A cada jogador, "
        "sorteia um índice e remove o nome escolhido da lista de opções. Não tenta adivinhar um nome "
        "novo repetidamente; a escolha sem reposição garante unicidade dentro daquele elenco."),
    codigo("Age = random.Next(18, 33)   // Valores possíveis: 18 até 32.\nSkill = random.Next(50, 81) // Valores possíveis: 50 até 80."),
    paragrafo("O limite superior de Random.Next é exclusivo. Generate aceita Random opcional: "
        "Random.Shared em produção; new Random(42) num teste reproduz a sequência, "
        "no mesmo ambiente de execução. Os testes também podem fornecer outra implementação de Random."),
    destaque("Força inicial", "Habilidades 60, 65, 70, 75, 68, 66, 69, 72, 64, 67, 70 somam 746. "
        "746 / 11 = 67,818...; a conversão para int resulta em Strength = 67. Não é arredondamento para 68."),
])

adicionar_pagina("Infrastructure: gravar de verdade", "EfGameStore liga a intenção do cadastro ao EF Core e ao MySQL.", [
    paragrafo("<b>IGameStore</b> declara as operações de persistência. <b>EfGameStore</b> implementa essas "
        "operações. <b>AppDbContext</b> acompanha entidades e oferece DbSet&lt;Team&gt; Teams e "
        "DbSet&lt;Player&gt; Players. Pomelo é o provider que permite ao EF Core trabalhar com MySQL."),
    DiagramaVertical([
        ("BeginTransactionAsync", "Abre uma transação para abranger clube e elenco.", AZUL),
        ("db.Teams.Add(team)", "Coloca o clube no ChangeTracker. Add ainda não executa o INSERT.", AZUL),
        ("Primeiro SaveChangesAsync", "Executa a gravação; o MySQL gera o Id do clube.", VERDE),
        ("Preencher TeamId e AddRange(players)", "Os 11 jogadores recebem o Id e são acompanhados pelo EF.", AZUL),
        ("Segundo SaveChangesAsync + CommitAsync", "Grava o elenco e confirma o conjunto inteiro.", VERDE),
    ], altura_caixa=48),
    Spacer(1, 12),
    secao("Se falhar o sexto jogador?"),
    paragrafo("O catch faz RollbackAsync, limpa o ChangeTracker e relança a exceção. "
        "Mesmo que o clube já tenha sido inserido dentro da transação, a gravação não fica confirmada "
        "sem o elenco. Isso depende de tabelas transacionais, como InnoDB."),
    destaque("Duas gravações; uma confirmação", "Uma transação automática por SaveChanges não uniria "
        "sozinha as duas chamadas. A transação explícita envolve ambas. A documentação EF Core [4] "
        "explica esse comportamento."),
    paragrafo("Um rollback não promete devolver os números de auto_increment já utilizados. "
        "Um intervalo na sequência de IDs não significa que existe um clube parcialmente cadastrado.", "nota"),
])

adicionar_pagina("React: o lado da tela", "Os componentes organizam apresentação e eventos, não as regras do elenco.", [
    tabela(["Arquivo / ferramenta", "O que faz durante a criação"], [
        ("main.tsx + app/App.tsx", "main monta React no HTML. App instala QueryClient/rotas e seleciona CreateTeamPage em /clubs/new."),
        ("CreateTeamPage.tsx", "Monta campos, prévia e botão. Após sucesso, usa os dados retornados para mostrar clube e jogadores."),
        ("React Hook Form + schema.ts", "useForm registra campos; Zod valida formato para feedback. handleSubmit só chama a mutation se o formulário for válido."),
        ("Controller + useWatch", "Controller conecta componentes controlados ao formulário. useWatch acompanha valores para atualizar a prévia local."),
        ("use-create-team.ts", "TanStack Query usa useMutation para controlar o envio. No sucesso, invalida ['teams']; a próxima consulta pode buscar a lista atualizada."),
        ("features/teams/api.ts", "createCustomTeam usa o cliente Axios. A URL da operação fica aqui, não espalhada pelos controles."),
        ("services/http.ts", "Define baseURL/timeout. getHttpError converte falhas em texto para a tela."),
        ("Button, Card, AppLayout e globals.css", "Organizam aparência, layout e tema. Não geram jogadores nem calculam força."),
    ], [0.38, 0.62]),
    secao("Estados que substituem o 'recarregar tudo'"),
    paragrafo("<b>isPending</b>: botão e campos ficam desabilitados durante o envio. "
        "<b>isError</b>: a mensagem da operação aparece. <b>isSuccess</b>: mutation.data contém "
        "o resultado, e a função do componente descreve outra apresentação."),
    paragrafo("React pode executar a função do componente novamente para calcular a interface. "
        "Isso não significa novo cadastro: o POST está no evento de envio, não solto no corpo da função. "
        "Estado do cliente também não equivale a dados persistidos no MySQL. Referência [2]."),
])

adicionar_pagina("Escudos: a prévia é local", "O banco guarda escolhas; os desenhos continuam sendo arquivos SVG do frontend.", [
    tabela(["Componente", "Responsabilidade"], [
        ("BadgeSelector.tsx", "Mostra três radios com os modelos e informa qual foi escolhido."),
        ("ColorPicker.tsx", "Oferece seletor visual e campo hexadecimal, com feedback de formato."),
        ("BadgePreview.tsx", "Apresenta o escudo maior e o nome digitado."),
        ("TeamBadge.tsx", "Escolhe o componente SVG e aplica duas variáveis CSS por instância."),
    ], [0.34, 0.66]),
    secao("O mesmo desenho; valores diferentes"),
    codigo("// Trecho real do estilo montado em TeamBadge:\n'--primary-color': validColor(primaryColor, '#F97316'),\n'--secondary-color': validColor(secondaryColor, '#17212B'),"),
    paragrafo("Os SVGs usam fill/stroke com var(--primary-color, ...) e var(--secondary-color, ...). "
        "SVGR transforma o import com <b>?react</b> em um componente inline. No Vite, svgo: false "
        "preserva os atributos do desenho. Um img apontando para um SVG não receberia essas "
        "variáveis locais da mesma maneira."),
    imagem_proporcional(RAIZ / "docs/images/etapa2-desktop.jpg", LARGURA_UTIL * 0.53),
    paragrafo("Evidência da interface na QA da Etapa 2, com API simulada. A coroa continua branca. "
        "A captura não representa dados atuais do seu banco.", "nota"),
    destaque("Alterar a prévia não altera o banco", "useWatch atualiza os valores locais. Só Criar clube "
        "envia template e cores. No MySQL não há markup SVG. Clube sem template usa iniciais neutras."),
])

adicionar_pagina("Listar e jogar: outros fluxos", "A mesma arquitetura atende ações diferentes, cada uma com sua própria rota.", [
    secao("Listagem de clubes"),
    codigo("TeamsPage.tsx -> useTeams() -> getTeams()\nGET /teams -> TeamEndpoints -> TeamService.List\nIGameStore.ListTeams -> EfGameStore -> MySQL\nTeamDto[] -> cards com TeamBadge"),
    paragrafo("use-teams.ts usa a chave de cache <b>['teams']</b>. Após criação, a mutation invalida essa "
        "chave. TeamService.Map inclui identidade e força no DTO. Em EfGameStore, AsNoTracking "
        "é usado na listagem: esses objetos lidos não entram no acompanhamento de alterações daquela consulta."),
    secao("Amistoso"),
    codigo("HomePage.tsx -> features/matches/api.ts\nPOST /match?team1Id=X&team2Id=Y\nMatchEndpoints -> MatchService.Simulate\nConsultar clubes/elencos -> MatchSimulator.Simulate\nMatchDto -> placar e autores dos gols"),
    paragrafo("MatchService valida os clubes e busca os elencos. MatchSimulator recebe objetos e listas "
        "para calcular o resultado sem acessar o banco. O motor já existia; sua matemática não foi "
        "alterada pela Etapa 2."),
    tabela(["Regra atual", "Como interpretar"], [
        ("Força efetiva", "Strength do clube + média truncada de Skill de todo o elenco, incluindo o goleiro."),
        ("Goleiros e gols", "Position == GK fica fora do sorteio de autores de gols."),
        ("Persistência de partida", "O resultado é devolvido ao cliente, mas histórico de partidas ainda não tem tabela neste projeto."),
        ("Escudos no placar", "O frontend recupera a identidade dos clubes selecionados e usa TeamBadge. MatchDto continua com o contrato anterior."),
    ], [0.30, 0.70]),
    destaque("Etapa futura", "Competition, chaves, pênaltis e reprodução de 30 segundos foram documentados, "
        "não implementados. Eles não estão escondidos em alguma classe da Etapa 2."),
])

adicionar_pagina("Schema, migrations e comandos", "A classe C# descreve o modelo; a migration altera o banco para acompanhar esse modelo.", [
    tabela(["Arquivo", "O que representa"], [
        ("TeamConfiguration.cs", "Mapeamento EF: IsCustom tem default false; template até 32, cores até 7; identidade pode faltar em clubes anteriores."),
        ("PlayerConfiguration.cs", "Relacionamento Players.TeamId -> Teams.Id, com exclusão restrita."),
        ("AddCustomTeamBadges.cs", "Up acrescenta quatro colunas. Down remove essas colunas; não é um botão seguro de desfazer dados personalizados."),
        (".Designer.cs e ModelSnapshot", "Metadados gerados e retrato do modelo usado pelo EF para comparar mudanças."),
        ("AppDbContextFactory.cs", "Cria o DbContext para comandos dotnet ef. Lê variáveis de ambiente ou fallback local; não é o caminho normal das requisições HTTP."),
    ], [0.37, 0.63]),
    secao("São operações diferentes"),
    codigo("# Iniciar a API:\ndotnet run --project backend/MiniBrasfoot.Api --launch-profile http\n\n# Instalar o frontend a partir do lockfile e iniciar o Vite:\nnpm.cmd --prefix frontend ci\nnpm.cmd --prefix frontend run dev"),
    paragrafo("npm.cmd evita o bloqueio de npm.ps1 no seu PowerShell. ci reinstala dependências "
        "conforme o lockfile; run dev inicia a interface. dotnet build compila, mas não aplica migration. "
        "dotnet ef migrations add gera arquivos; database update aplica as migrations pendentes."),
    codigo("# Comando de schema: somente após backup e revisão das pendentes.\ndotnet ef database update `\n  --project backend/MiniBrasfoot.Infrastructure `\n  --startup-project backend/MiniBrasfoot.Api"),
    destaque("O que sabemos nesta conversa", "Você informou que fez backup. Nas imagens anteriores "
        "só InitialMySql estava aplicada e faltavam as quatro colunas. A conclusão da atualização "
        "não foi confirmada. Confira __EFMigrationsHistory e SHOW COLUMNS FROM Teams no seu banco."),
    paragrafo("A API não aplica migrations automaticamente. DDL MySQL pode fazer commit implícito; "
        "a transação da página 10 protege os INSERTs do cadastro, não promete desfazer ALTER TABLE.", "nota"),
])

adicionar_pagina("Quando der erro, siga o pedido", "O local da mensagem na tela não é necessariamente o local da causa.", [
    tabela(["Sintoma", "Onde conferir primeiro"], [
        ("npm.ps1 bloqueado", "Terminal PowerShell: usar npm.cmd. Não é um erro de React nem de banco."),
        ("Tela não abre", "Terminal do Vite e URL localhost:5173. Verifique se o processo está ativo e qual porta anunciou."),
        ("Não conectou à API", "Backend na porta 5063 e VITE_API_URL em .env.local. Após mudar configuração VITE, reinicie o Vite."),
        ("HTTP 400", "Dados enviados e validação no serviço. Campos inválidos no formulário também podem bloquear o POST antes da rede."),
        ("HTTP 409", "Relacionamento/conflito: exclusão de clube com jogadores ou falha de FK. Não apagar dados para contornar."),
        ("HTTP 503", "Servidor/credenciais/schema do MySQL. 'Banco de dados indisponível' é genérico; não prova sozinho que o serviço está parado."),
        ("Prévia não muda", "useWatch, props de TeamBadge, variáveis CSS e SVG inline. Não começar olhando o SQL."),
    ], [0.34, 0.66]),
    secao("Quem transforma exceções em mensagens?"),
    paragrafo("ApiExceptionHandler.cs mapeia exceções para códigos HTTP e ProblemDetails. "
        "InputRules/CustomTeamService usam UseCaseException para erros esperados. "
        "No frontend, getHttpError lê title da resposta. Logs mostram tipo/traceId; "
        "não imprimem a senha de conexão."),
    secao("Breakpoints no VS Code"),
    paragrafo("Inicie o backend pelo depurador C# ou conecte o depurador ao processo já iniciado. "
        "Só desenhar um ponto vermelho e executar dotnet run num terminal não garante depuração. "
        "Coloque pontos no endpoint, em Create, em Generate e antes de cada SaveChangesAsync."),
    destaque("Ferramentas do navegador", "F12 > Network mostra método, URL, payload e status do POST. "
        "Se nenhum POST apareceu, comece pelo formulário. Se ele chegou e retornou erro, siga para o C#. "
        "Evite compartilhar senhas ou strings de conexão nas capturas."),
])

adicionar_pagina("Aprenda fazendo um percurso", "Cinco exercícios curtos para conhecer o sistema com o editor aberto.", [
    secao("1. Leia o cadastro sem executar"),
    paragrafo("Abra CreateTeamPage.tsx e encontre handleSubmit. Siga useCreateTeam até createCustomTeam. "
        "Depois abra TeamEndpoints.cs e localize /custom. Em cada ponto, escreva: 'recebe X, chama Y, devolve Z'. "
        "Você terá o mapa da página 7 feito por você."),
    secao("2. Observe a memória antes da gravação"),
    paragrafo("Em ambiente de desenvolvimento com o depurador, pare após generator.Generate(). "
        "Inspecione players.Count, Position, Age e Skill. Nesse momento não houve INSERT de clube/jogador "
        "pelo cadastro. Os IDs novos ainda serão obtidos na persistência. Se concluir o cadastro, "
        "será criado um clube real: use uma cópia do banco para experimentar."),
    secao("3. Acompanhe os IDs"),
    paragrafo("Pare em EfGameStore antes e depois do primeiro SaveChangesAsync. Compare team.Id. "
        "Depois acompanhe TeamId em cada jogador. Entenda por que o clube precisa do ID antes de "
        "o elenco ser salvo nesta implementação."),
    secao("4. Teste uma regra sem abrir o site"),
    codigo("// Exemplo novo em português, usando a classe existente.\nvar geradorDeJogadores = new PlayerGenerator();\nvar jogadores = geradorDeJogadores.Generate(new Random(42));\nint quantidadeDeJogadores = jogadores.Count; // Deve ser 11.\nint forcaInicial = (int)jogadores.Average(jogador => jogador.Skill);"),
    paragrafo("Esse trecho é um exemplo para um teste ou experimento separado, não uma alteração "
        "já aplicada. Para mudar os limites de habilidade no produto, a regra começa no gerador, "
        "mas testes e texto da tela também precisam acompanhar a mudança."),
    secao("5. Separe cor visual de dado persistido"),
    paragrafo("Abra /clubs/new, altere as cores e acompanhe Network. A prévia muda sem POST. "
        "Observe TeamBadge recebendo os valores. O salvamento só acontece quando o formulário é enviado."),
    destaque("Onde colocar uma mudança?", "Texto e layout: React. Regra do cadastro: Application. "
        "Geração e simulação: Domain. Mapeamento/SQL: Infrastructure. Contrato HTTP e DI: Api. "
        "Sempre acompanhe o resultado com um teste apropriado."),
])

adicionar_pagina("Vocabulário para ler o C#", "Traduções práticas dos termos que aparecem nos arquivos atuais.", [
    tabela(["Termo", "Como ler neste projeto"], [
        ("class / entidade", "Um tipo com propriedades/comportamento. Team representa um clube; não é a tabela em execução."),
        ("interface", "Um contrato de operações. IGameStore permite trocar a implementação sem mudar o caso de uso."),
        ("record / DTO", "Um tipo adequado para dados de entrada/saída. Não grava nada sozinho."),
        ("sealed / partial", "sealed impede herdar a classe. partial permite partes da mesma classe; GeneratedRegex gera outra parte durante a compilação."),
        ("async / await / Task&lt;T&gt;", "O método pode aguardar trabalho assíncrono e produzir T depois. await continua após o resultado; não cria automaticamente uma thread para cada chamada."),
        ("CancellationToken ct", "Sinaliza cancelamento e é repassado às operações. Não substitui Commit/Rollback."),
        ("IReadOnlyList&lt;Player&gt;", "Uma lista que esse contrato não oferece para adicionar/remover. Os objetos Player ainda podem ter propriedades alteradas, como TeamId."),
        ("LINQ: Select / Average", "Select transforma cada item; Average calcula a média. Usados para montar DTOs e calcular força."),
        ("DbSet / ChangeTracker", "DbSet é a entrada EF para aquele tipo; ChangeTracker acompanha estados antes de SaveChanges."),
        ("namespace / using", "namespace organiza nomes de tipos. using permite referenciar tipos ou recursos. Não significa executar aquele arquivo."),
        ("JSON / props / hook", "JSON transporta dados; props são entradas do componente; hook conecta comportamento/estado ao React."),
    ], [0.34, 0.66]),
    secao("Leia esta linha em voz alta"),
    codigo("await store.CreateTeamWithPlayers(team, players, ct);"),
    paragrafo("'Peça ao armazenamento para criar o clube com seus jogadores e aguarde a conclusão, "
        "propagando o cancelamento.' A interface descreve o pedido; a implementação EfGameStore "
        "faz a transação. A linha seguinte só monta a resposta depois que esse pedido termina com sucesso."),
    paragrafo("Conceito async/await: referência [5], página 18.", "nota"),
])

adicionar_pagina("Verifique e continue estudando", "Os testes permitem entender o comportamento sem depender de cliques manuais.", [
    tabela(["Teste", "O que ensina"], [
        ("PlayerGeneratorTests.cs", "Quantidade, códigos de posição, nomes únicos, limites, semente e compatibilidade com o simulador."),
        ("CustomTeamTests.cs", "Caso de uso e HTTP: validação, força, 201/Location, retorno do elenco e contratos antigos."),
        ("CustomTeamPersistenceTests.cs", "Transação real em SQLite em memória: sucesso e rollback preservando dados anteriores. Não comprova escrita no MySQL real."),
        ("FakeGameStore.cs + ApiTests.cs", "Fake atende o contrato sem banco. ApiTests cobre rotas históricas, CORS e erros."),
        ("TeamBadge.test.tsx + schema.test.ts", "SVGs originais/cores/fallback e validação do formulário. MatchSimulatorTests.cs protege as regras da simulação."),
    ], [0.43, 0.57]),
    codigo("# Compilar e testar backend; rodar os comandos na raiz.\ndotnet build BrasfootAPI.sln -m:1 /nr:false\ndotnet test backend/MiniBrasfoot.Tests --no-build\n\n# Frontend; não precisa do MySQL para estes testes unitários.\nnpm.cmd --prefix frontend run test\nnpm.cmd --prefix frontend run lint"),
    paragrafo("A entrega da Etapa 2 registrou 58 testes .NET e 16 testes frontend aprovados. "
        "Esses são resultados daquela execução, não uma nova execução feita para este PDF. "
        "Em docs/VALIDATION.md estão as limitações e evidências."),
    secao("Referências oficiais para aprofundar"),
    paragrafo('<b>[1]</b> <link href="https://learn.microsoft.com/en-us/previous-versions/aspnet/ms178472(v=vs.100)" color="#D96316">Microsoft: ciclo de vida de páginas WebForms</link><br/>'
        '<b>[2]</b> <link href="https://react.dev/learn/render-and-commit" color="#D96316">React: renderização e atualização do DOM</link><br/>'
        '<b>[3]</b> <link href="https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/service-lifetimes" color="#D96316">Microsoft: ciclos de vida dos serviços na DI</link><br/>'
        '<b>[4]</b> <link href="https://learn.microsoft.com/en-us/ef/core/saving/transactions" color="#D96316">Microsoft: transações no EF Core</link><br/>'
        '<b>[5]</b> <link href="https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios" color="#D96316">Microsoft: cenários assíncronos em C#</link>', "nota"),
    destaque("A base deste guia é o seu código", "Program.cs, serviços, entidades, persistência, componentes "
        "e testes foram lidos no projeto local. As referências complementam os conceitos; "
        "os fluxos, caminhos e regras descritos são os da implementação auditada."),
])


def gerar_documento():
    PASTA_SAIDA.mkdir(parents=True, exist_ok=True)
    PASTA_REVISAO.mkdir(parents=True, exist_ok=True)
    documento = SimpleDocTemplate(str(ARQUIVO_PDF), pagesize=A4, leftMargin=MARGEM,
        rightMargin=MARGEM, topMargin=55, bottomMargin=57, title="FutSimulator - Guia da Etapa 2 para quem vem do WebForms",
        author="FutSimulator", subject="Fluxo, camadas, React, EF Core e MySQL", pageCompression=1)
    elementos = []
    for indice, (titulo, conteudo) in enumerate(PAGINAS):
        if indice:
            elementos.append(PageBreak())
        elementos.extend(conteudo)
    documento.build(elementos, onFirstPage=desenhar_rodape, onLaterPages=desenhar_rodape)
    (PASTA_REVISAO / "roteiro_paginas.json").write_text(json.dumps(
        [{"pagina": indice + 1, "titulo": titulo.replace("<br/>", " ")} for indice, (titulo, _) in enumerate(PAGINAS)],
        ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"PDF criado: {ARQUIVO_PDF}")
    print(f"Seções previstas: {len(PAGINAS)}")


if __name__ == "__main__":
    gerar_documento()
