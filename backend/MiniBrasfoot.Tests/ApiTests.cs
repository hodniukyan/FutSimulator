using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniBrasfoot.Application.DTOs;
using MiniBrasfoot.Application.Interfaces;
using Xunit;

namespace MiniBrasfoot.Tests;

public sealed class ApiTests
{
    private sealed class Factory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IGameStore>();
                services.AddSingleton<IGameStore, FakeGameStore>();
            });
        }
    }

    [Fact]
    public async Task CrudAndMatchKeepPublicContractsAndLegacyPlayerRoutes()
    {
        await using var factory = new Factory();
        using var client = factory.CreateClient();
        var created = await client.PostAsJsonAsync("/teams", new TeamInput("Casa", "Brasil", 50));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var first = (await created.Content.ReadFromJsonAsync<TeamDto>())!;
        Assert.Equal($"/teams/{first.Id}", created.Headers.Location!.OriginalString);
        var second = (await (await client.PostAsJsonAsync("/teams", new TeamInput("Fora", "Brasil", 40)))
            .Content.ReadFromJsonAsync<TeamDto>())!;
        var player = (await (await client.PostAsJsonAsync("/players", new PlayerInput("Atacante", 23, 20, first.Id, "ST")))
            .Content.ReadFromJsonAsync<PlayerDto>())!;
        await client.PostAsJsonAsync("/players", new PlayerInput("Visitante", 25, 20, second.Id, "ST"));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/players/{player.Id}")).StatusCode);
        var changed = await client.PutAsJsonAsync($"/teams/{player.Id}/players", new PlayerInput("Novo nome", 24, 30, second.Id, "ST"));
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        Assert.Equal(first.Id, (await changed.Content.ReadFromJsonAsync<PlayerDto>())!.TeamId);
        var match = await client.PostAsync($"/match?team1Id={first.Id}&team2Id={second.Id}", null);
        Assert.Equal(HttpStatusCode.OK, match.StatusCode);
        var result = (await match.Content.ReadFromJsonAsync<MatchDto>())!;
        Assert.Equal("Casa", result.Team1);
        Assert.Equal("Fora", result.Team2);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/teams/{first.Id}/players")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/teams/{first.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync($"/teams/{player.Id}/players")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync($"/teams/{first.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/teams/{first.Id}")).StatusCode);
    }

    [Fact]
    public async Task InvalidInputsReturnProblemDetailsAndUpdateCannotBypassValidation()
    {
        await using var factory = new Factory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/teams", new TeamInput("", "Brasil", 50));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var team = (await (await client.PostAsJsonAsync("/teams", new TeamInput("Casa", "Brasil", 50)))
            .Content.ReadFromJsonAsync<TeamDto>())!;
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync($"/teams/{team.Id}", new TeamInput("Casa", "Brasil", 0))).StatusCode);
        Assert.Equal(50, (await client.GetFromJsonAsync<TeamDto>($"/teams/{team.Id}"))!.Strength);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/players", new PlayerInput("Nome", 23, 20, 999, "ST"))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync($"/match?team1Id={team.Id}&team2Id={team.Id}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync($"/match?team1Id={team.Id}&team2Id=999", null)).StatusCode);
    }

    [Fact]
    public async Task CorsAllowsFrontendAndRejectsUnlistedOrigins()
    {
        await using var factory = new Factory();
        using var client = factory.CreateClient();
        foreach (var (origin, allowed) in new[] { ("http://localhost:5173", true), ("https://unlisted.example", false) })
        {
            using var request = new HttpRequestMessage(HttpMethod.Options, "/teams");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", "POST");
            var response = await client.SendAsync(request);
            Assert.Equal(allowed, response.Headers.Contains("Access-Control-Allow-Origin"));
        }
    }

    [Fact]
    public async Task SwaggerAndHealthAreAvailable()
    {
        await using var factory = new Factory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/swagger/v1/swagger.json")).StatusCode);
    }
}
