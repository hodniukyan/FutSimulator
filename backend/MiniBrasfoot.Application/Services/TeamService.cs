using BrasfootAPI.Models;
using MiniBrasfoot.Application.DTOs;
using MiniBrasfoot.Application.Interfaces;
using MiniBrasfoot.Application.Validators;

namespace MiniBrasfoot.Application.Services;

public sealed class TeamService(IGameStore store)
{
    public async Task<IReadOnlyList<TeamDto>> List(CancellationToken ct) =>
        (await store.ListTeams(ct)).Select(Map).ToList();
    public async Task<TeamDto> Get(int id, CancellationToken ct) => Map(await Find(id, ct));
    public async Task<TeamDto> Create(TeamInput input, CancellationToken ct)
    {
        InputRules.Validate(input);
        var team = new Team { Name = input.Name, Country = input.Country, Strength = input.Strength };
        store.AddTeam(team);
        await store.Save(ct);
        return Map(team);
    }
    public async Task<TeamDto> Update(int id, TeamInput input, CancellationToken ct)
    {
        var team = await Find(id, ct);
        InputRules.Validate(input);
        team.Name = input.Name;
        team.Country = input.Country;
        team.Strength = input.Strength;
        await store.Save(ct);
        return Map(team);
    }
    public async Task Delete(int id, CancellationToken ct)
    {
        var team = await Find(id, ct);
        if (await store.HasPlayers(id, ct))
            throw new UseCaseException(FailureKind.Conflict, "Remova os jogadores antes de excluir o time.");
        store.RemoveTeam(team);
        await store.Save(ct);
    }
    private async Task<Team> Find(int id, CancellationToken ct) => await store.FindTeam(id, ct)
        ?? throw new UseCaseException(FailureKind.NotFound, "Time não encontrado");
    private static TeamDto Map(Team team) => new(team.Id, team.Name, team.Country, team.Strength);
}
