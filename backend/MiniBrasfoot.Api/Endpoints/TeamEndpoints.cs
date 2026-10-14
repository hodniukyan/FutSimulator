using MiniBrasfoot.Application.DTOs;
using MiniBrasfoot.Application.Services;

namespace MiniBrasfoot.Api.Endpoints;

public static class TeamEndpoints
{
    public static void MapTeamEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/teams").WithTags("Times");
        group.MapPost("/custom", async (CustomTeamInput input, CustomTeamService service, CancellationToken ct) =>
        {
            var result = await service.Create(input, ct);
            return Results.Created($"/teams/{result.Team.Id}", result);
        }).WithSummary("Cria clube personalizado e elenco 4-4-2 atomicamente.");
        group.MapGet("", async (TeamService service, CancellationToken ct) => Results.Ok(await service.List(ct)));
        group.MapGet("/{id:int}", async (int id, TeamService service, CancellationToken ct) => Results.Ok(await service.Get(id, ct)));
        group.MapPost("", async (TeamInput input, TeamService service, ILogger<TeamService> logger, CancellationToken ct) =>
        {
            var team = await service.Create(input, ct);
            logger.LogInformation("Time {TeamId} criado", team.Id);
            return Results.Created($"/teams/{team.Id}", team);
        });
        group.MapPut("/{id:int}", async (int id, TeamInput input, TeamService service, CancellationToken ct) =>
            Results.Ok(await service.Update(id, input, ct)));
        group.MapDelete("/{id:int}", async (int id, TeamService service, CancellationToken ct) =>
        {
            await service.Delete(id, ct);
            return Results.Ok("Time removido");
        });
    }
}
