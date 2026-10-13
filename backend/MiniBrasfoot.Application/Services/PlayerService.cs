using BrasfootAPI.Models;
using MiniBrasfoot.Application.DTOs;
using MiniBrasfoot.Application.Interfaces;
using MiniBrasfoot.Application.Validators;

namespace MiniBrasfoot.Application.Services;

public sealed class PlayerService(IGameStore store)
{
    public async Task<IReadOnlyList<PlayerDto>> List(int teamId, CancellationToken ct)
    {
        if (await store.FindTeam(teamId, ct) is null)
            throw new UseCaseException(FailureKind.NotFound, "Time não encontrado");
        return (await store.ListPlayers(teamId, ct)).Select(Map).ToList();
    }
    public async Task<PlayerDto> Get(int id, CancellationToken ct) => Map(await Find(id, ct));
    public async Task<PlayerDto> Create(PlayerInput input, CancellationToken ct)
    {
        InputRules.Validate(input);
        if (await store.FindTeam(input.TeamId, ct) is null)
            throw new UseCaseException(FailureKind.Validation, "Time não existe");
        var player = new Player { Name = input.Name, Age = input.Age, Skill = input.Skill,
            TeamId = input.TeamId, Position = input.Position };
        store.AddPlayer(player);
        await store.Save(ct);
        return Map(player);
    }
    public async Task<PlayerDto> Update(int id, PlayerInput input, CancellationToken ct)
    {
        var player = await Find(id, ct);
        InputRules.Validate(input);
        player.Name = input.Name;
        player.Age = input.Age;
        player.Skill = input.Skill;
        player.Position = input.Position;
        // Preserve the current behavior: updating does not transfer a player.
        await store.Save(ct);
        return Map(player);
    }
    public async Task Delete(int id, CancellationToken ct)
    {
        store.RemovePlayer(await Find(id, ct));
        await store.Save(ct);
    }
    private async Task<Player> Find(int id, CancellationToken ct) => await store.FindPlayer(id, ct)
        ?? throw new UseCaseException(FailureKind.NotFound, "Jogador não encontrado");
    private static PlayerDto Map(Player p) => new(p.Id, p.Name, p.Age, p.Skill, p.TeamId, p.Position);
}
