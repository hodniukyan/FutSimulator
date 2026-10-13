using MiniBrasfoot.Application.Services;

namespace MiniBrasfoot.Api.Endpoints;

public static class MatchEndpoints
{
    public static void MapMatchEndpoints(this WebApplication app) =>
        app.MapPost("/match", async (int team1Id, int team2Id, MatchService service,
            ILogger<MatchService> logger, CancellationToken ct) =>
        {
            var match = await service.Simulate(team1Id, team2Id, ct);
            logger.LogInformation("Partida simulada entre {Team1Id} e {Team2Id}: {Score}", team1Id, team2Id, match.Score);
            return Results.Ok(match);
        }).WithTags("Partidas");
}
