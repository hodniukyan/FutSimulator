using MiniBrasfoot.Application.DTOs;
using MiniBrasfoot.Application.Interfaces;
using MiniBrasfoot.Application.Validators;
using MiniBrasfoot.Domain.Rules;

namespace MiniBrasfoot.Application.Services;

public sealed class MatchService(IGameStore store, MatchSimulator simulator)
{
    public async Task<MatchDto> Simulate(int team1Id, int team2Id, CancellationToken ct)
    {
        if (team1Id == team2Id)
            throw new UseCaseException(FailureKind.Validation, "Selecione dois times diferentes.");
        var team1 = await store.FindTeam(team1Id, ct);
        var team2 = await store.FindTeam(team2Id, ct);
        if (team1 is null || team2 is null)
            throw new UseCaseException(FailureKind.Validation, "Times inválidos");
        var players1 = await store.ListPlayers(team1Id, ct);
        var players2 = await store.ListPlayers(team2Id, ct);
        if (!players1.Any(p => p.Position != "GK") || !players2.Any(p => p.Position != "GK"))
            throw new UseCaseException(FailureKind.Validation, "Times precisam ter jogadores de linha");
        var result = simulator.Simulate(team1, team2, players1, players2);
        return new(result.Team1, result.Team2, result.Nivel, $"{result.Team1Goals} x {result.Team2Goals}",
            result.Goals.Select(g => new GoalDto(g.Minute, g.Team, g.Player)).ToList());
    }
}
