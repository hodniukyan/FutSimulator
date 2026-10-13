using MiniBrasfoot.Application.DTOs;
using MiniBrasfoot.Application.Services;

namespace MiniBrasfoot.Api.Endpoints;

public static class PlayerEndpoints
{
    public static void MapPlayerEndpoints(this WebApplication app)
    {
        app.MapPost("/players", async (PlayerInput input, PlayerService service, CancellationToken ct) =>
        {
            var player = await service.Create(input, ct);
            return Results.Created($"/players/{player.Id}", player);
        }).WithTags("Jogadores");
        app.MapGet("/players/{id:int}", async (int id, PlayerService service, CancellationToken ct) =>
            Results.Ok(await service.Get(id, ct))).WithTags("Jogadores");
        app.MapGet("/teams/{id:int}/players", async (int id, PlayerService service, CancellationToken ct) =>
            Results.Ok(await service.List(id, ct))).WithTags("Jogadores");
        app.MapPut("/players/{id:int}", Update).WithTags("Jogadores");
        app.MapDelete("/players/{id:int}", Delete).WithTags("Jogadores");
        // Compatibility aliases: id is a PLAYER id, as in the original implementation.
        app.MapPut("/teams/{id:int}/players", Update).WithTags("Jogadores")
            .WithSummary("Compatibilidade: id identifica o jogador. Prefira PUT /players/{id}.");
        app.MapDelete("/teams/{id:int}/players", Delete).WithTags("Jogadores")
            .WithSummary("Compatibilidade: id identifica o jogador. Prefira DELETE /players/{id}.");
    }
    private static async Task<IResult> Update(int id, PlayerInput input, PlayerService service, CancellationToken ct) =>
        Results.Ok(await service.Update(id, input, ct));
    private static async Task<IResult> Delete(int id, PlayerService service, CancellationToken ct)
    {
        await service.Delete(id, ct);
        return Results.Ok("Jogador removido");
    }
}
