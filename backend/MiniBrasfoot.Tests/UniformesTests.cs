using System.Net;
using System.Net.Http.Json;
using BrasfootAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniBrasfoot.Application.DTOs;
using MiniBrasfoot.Application.Interfaces;
using MiniBrasfoot.Application.Services;
using MiniBrasfoot.Application.Validators;
using MiniBrasfoot.Domain.Rules;
using MiniBrasfoot.Infrastructure.Persistence;
using Xunit;

namespace MiniBrasfoot.Tests;

public sealed class UniformesTests
{
    [Theory]
    [InlineData(ModelosDeUniforme.ListradoHorizontal)]
    [InlineData(ModelosDeUniforme.ListradoVertical)]
    [InlineData(ModelosDeUniforme.Liso)]
    [InlineData(ModelosDeUniforme.MeioAMeio)]
    public async Task CriaClubeComModeloDeUniformeValido(string modeloDoUniforme)
    {
        var armazenamento = new FakeGameStore();
        var servico = new CustomTeamService(armazenamento, new PlayerGenerator());
        var entrada = new CustomTeamInput("Aurora", "Brasil", "shield-round", "#123456", "#ABCDEF", modeloDoUniforme);
        var resultado = await servico.Create(entrada, default);
        Assert.Equal(modeloDoUniforme, resultado.Team.ModeloDoUniforme);
        Assert.Equal(modeloDoUniforme, Assert.Single(armazenamento.Teams).ModeloDoUniforme);
        Assert.Equal(11, resultado.Players.Count);
        Assert.Equal((int)resultado.Players.Average(jogador => jogador.Skill), resultado.Team.Strength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("uniforme-desconhecido")]
    [InlineData("uniforme-liso\n")]
    public async Task RejeitaModeloInvalidoSemCadastrarClubeOuJogadores(string? modeloDoUniforme)
    {
        var armazenamento = new FakeGameStore();
        var servico = new CustomTeamService(armazenamento, new PlayerGenerator());
        var entrada = new CustomTeamInput("Aurora", "Brasil", "shield-round", "#123456", "#ABCDEF", modeloDoUniforme!);
        var erro = await Assert.ThrowsAsync<UseCaseException>(() => servico.Create(entrada, default));
        Assert.Equal(FailureKind.Validation, erro.Kind);
        Assert.Empty(armazenamento.Teams);
        Assert.Empty(armazenamento.Players);
    }

    [Fact]
    public async Task RequisicaoAnteriorSemUniformeContinuaValida()
    {
        await using var fabrica = new FabricaDaApi();
        using var cliente = fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/teams/custom", new {
            name = "Legado personalizado", country = "Brasil", badgeTemplate = "shield-round",
            primaryColor = "#123456", secondaryColor = "#ABCDEF" });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var resultado = (await resposta.Content.ReadFromJsonAsync<CustomTeamDto>())!;
        Assert.Equal(ModelosDeUniforme.Liso, resultado.Team.ModeloDoUniforme);
        Assert.Equal(11, resultado.Players.Count);
    }

    [Fact]
    public async Task ApiRetornaUniformeEmCriacaoConsultaListaEPreservaNoPutLegado()
    {
        await using var fabrica = new FabricaDaApi();
        using var cliente = fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/teams/custom", new CustomTeamInput(
            "Aurora", "Brasil", "shield-triangular", "#123456", "#ABCDEF", ModelosDeUniforme.MeioAMeio));
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var resultado = (await resposta.Content.ReadFromJsonAsync<CustomTeamDto>())!;
        Assert.Equal(ModelosDeUniforme.MeioAMeio, resultado.Team.ModeloDoUniforme);
        var conteudo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("\"modeloDoUniforme\":\"uniforme-meio-a-meio\"", conteudo);
        Assert.Equal(resultado.Team, await cliente.GetFromJsonAsync<TeamDto>($"/teams/{resultado.Team.Id}"));
        Assert.Contains((await cliente.GetFromJsonAsync<TeamDto[]>("/teams"))!, clube => clube.ModeloDoUniforme == ModelosDeUniforme.MeioAMeio);
        var alteracao = await cliente.PutAsJsonAsync($"/teams/{resultado.Team.Id}", new TeamInput("Novo nome", "Brasil", 65));
        Assert.Equal(ModelosDeUniforme.MeioAMeio, (await alteracao.Content.ReadFromJsonAsync<TeamDto>())!.ModeloDoUniforme);

        var invalida = await cliente.PostAsJsonAsync("/teams/custom", new CustomTeamInput(
            "Inválido", "Brasil", "shield-round", "#123456", "#ABCDEF", "uniforme-invalido"));
        Assert.Equal(HttpStatusCode.BadRequest, invalida.StatusCode);
        Assert.Equal("application/problem+json", invalida.Content.Headers.ContentType!.MediaType);
        Assert.Single((await cliente.GetFromJsonAsync<TeamDto[]>("/teams"))!);
    }

    [Fact]
    public async Task PersisteModeloNoBancoDeTesteSemAlterarClubeAnterior()
    {
        await using var conexao = new SqliteConnection("Data Source=:memory:");
        await conexao.OpenAsync();
        await using var contexto = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conexao).Options);
        await contexto.Database.EnsureCreatedAsync(); // Apenas o banco isolado em memória deste teste.
        var armazenamento = new EfGameStore(contexto);
        var legado = await new TeamService(armazenamento).Create(new TeamInput("Anterior", "Brasil", 70), default);
        var novo = await new CustomTeamService(armazenamento, new PlayerGenerator()).Create(new CustomTeamInput(
            "Aurora", "Brasil", "shield-round", "#123456", "#ABCDEF", ModelosDeUniforme.ListradoHorizontal), default);
        contexto.ChangeTracker.Clear();
        Assert.Equal(ModelosDeUniforme.ListradoHorizontal,
            (await contexto.Teams.AsNoTracking().SingleAsync(clube => clube.Id == novo.Team.Id)).ModeloDoUniforme);
        var anterior = await contexto.Teams.AsNoTracking().SingleAsync(clube => clube.Id == legado.Id);
        Assert.Null(anterior.ModeloDoUniforme);
        Assert.Equal(70, anterior.Strength);
        Assert.Equal(11, await contexto.Players.CountAsync());
    }

    private sealed class FabricaDaApi : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder construtor)
        {
            construtor.UseEnvironment("Development");
            construtor.UseSetting("Database:ServerVersion", "8.0.45");
            construtor.ConfigureTestServices(servicos => {
                servicos.RemoveAll<IGameStore>();
                servicos.AddSingleton<IGameStore, FakeGameStore>();
            });
        }
    }
}
