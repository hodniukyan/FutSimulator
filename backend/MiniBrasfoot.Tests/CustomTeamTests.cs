using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniBrasfoot.Application.DTOs;
using MiniBrasfoot.Application.Interfaces;
using MiniBrasfoot.Application.Services;
using MiniBrasfoot.Application.Validators;
using MiniBrasfoot.Domain.Rules;
using Xunit;

namespace MiniBrasfoot.Tests;

public sealed class CustomTeamTests
{
    private static CustomTeamInput ValidInput => new(" Aurora FC ", " Brasil ", "shield-hexagonal", "#ff6b00", "#111827");

    [Theory]
    [InlineData("shield-hexagonal")]
    [InlineData("shield-round")]
    [InlineData("shield-triangular")]
    public async Task CreatesClubAndElevenPlayersAndComputesStrength(string template)
    {
        var store = new FakeGameStore();
        var result = await new CustomTeamService(store, new PlayerGenerator()).Create(ValidInput with { BadgeTemplate = template }, default);
        Assert.Single(store.Teams);
        Assert.Equal(11, store.Players.Count);
        Assert.Equal("Aurora FC", result.Team.Name);
        Assert.Equal("Brasil", result.Team.Country);
        Assert.True(result.Team.IsCustom);
        Assert.Equal(template, result.Team.BadgeTemplate);
        Assert.Equal("#FF6B00", result.Team.PrimaryColor);
        Assert.Equal((int)result.Players.Average(player => player.Skill), result.Team.Strength);
        Assert.All(result.Players, player => { Assert.Equal(result.Team.Id, player.TeamId); Assert.True(player.Id > 0); });
    }

    public static IEnumerable<object[]> InvalidInputs()
    {
        yield return [ValidInput with { Name = null! }];
        yield return [ValidInput with { Name = "  " }];
        yield return [ValidInput with { Name = new string('a', 101) }];
        yield return [ValidInput with { Country = null! }];
        yield return [ValidInput with { Country = "" }];
        yield return [ValidInput with { Country = new string('a', 101) }];
        yield return [ValidInput with { BadgeTemplate = null! }];
        yield return [ValidInput with { BadgeTemplate = "shield-unknown" }];
        yield return [ValidInput with { PrimaryColor = null! }];
        yield return [ValidInput with { PrimaryColor = "red" }];
        yield return [ValidInput with { PrimaryColor = "#FFF" }];
        yield return [ValidInput with { PrimaryColor = "#12345Z" }];
        yield return [ValidInput with { PrimaryColor = "#123456\n" }];
        yield return [ValidInput with { SecondaryColor = null! }];
        yield return [ValidInput with { SecondaryColor = "#12345678" }];
        yield return [ValidInput with { SecondaryColor = "" }];
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public async Task RejectsInvalidInputBeforeAnyPersistence(CustomTeamInput input)
    {
        var store = new FakeGameStore();
        var exception = await Assert.ThrowsAsync<UseCaseException>(() => new CustomTeamService(store, new PlayerGenerator()).Create(input, default));
        Assert.Equal(FailureKind.Validation, exception.Kind);
        Assert.Empty(store.Teams);
        Assert.Empty(store.Players);
    }

    [Fact]
    public async Task HttpCreationReturnsLocationAndRosterAndPreservesExistingContracts()
    {
        await using var factory = new Factory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/teams/custom", new {
            name = "Aurora FC", country = "Brasil", badgeTemplate = "shield-round",
            primaryColor = "#F97316", secondaryColor = "#17212B", strength = 999, isCustom = false });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<CustomTeamDto>())!;
        Assert.Equal($"/teams/{created.Team.Id}", response.Headers.Location!.OriginalString);
        Assert.True(created.Team.IsCustom);
        Assert.InRange(created.Team.Strength, 50, 80);
        Assert.Equal(11, created.Players.Count);
        Assert.Equal(created.Team, await client.GetFromJsonAsync<TeamDto>($"/teams/{created.Team.Id}"));
        Assert.Equal(11, (await client.GetFromJsonAsync<PlayerDto[]>($"/teams/{created.Team.Id}/players"))!.Length);
        Assert.Contains((await client.GetFromJsonAsync<TeamDto[]>("/teams"))!, team => team.Id == created.Team.Id && team.BadgeTemplate == "shield-round");
        var legacyResponse = await client.PostAsJsonAsync("/teams", new TeamInput("Legado", "Brasil", 70));
        Assert.Equal(HttpStatusCode.Created, legacyResponse.StatusCode);
        var legacy = (await legacyResponse.Content.ReadFromJsonAsync<TeamDto>())!;
        Assert.False(legacy.IsCustom);
        Assert.Null(legacy.BadgeTemplate);
        await client.PostAsJsonAsync("/players", new PlayerInput("Atacante", 22, 65, legacy.Id, "ATT"));
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/match?team1Id={created.Team.Id}&team2Id={legacy.Id}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/teams/{created.Team.Id}", new TeamInput("Aurora", "Brasil", created.Team.Strength))).StatusCode);
        Assert.Equal("shield-round", (await client.GetFromJsonAsync<TeamDto>($"/teams/{created.Team.Id}"))!.BadgeTemplate);
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public async Task HttpValidationUsesProblemDetails(CustomTeamInput input)
    {
        await using var factory = new Factory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/teams/custom", input);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        Assert.Empty((await client.GetFromJsonAsync<TeamDto[]>("/teams"))!);
    }

    private sealed class Factory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("Database:ServerVersion", "8.0.45");
            builder.ConfigureTestServices(services => {
                services.RemoveAll<IGameStore>();
                services.AddSingleton<IGameStore, FakeGameStore>();
            });
        }
    }
}
