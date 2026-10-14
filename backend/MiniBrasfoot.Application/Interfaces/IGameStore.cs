using BrasfootAPI.Models;

namespace MiniBrasfoot.Application.Interfaces;

// Operations needed by the current use cases; no generic repository or EF types exposed.
public interface IGameStore
{
    Task<List<Team>> ListTeams(CancellationToken ct);
    Task<Team?> FindTeam(int id, CancellationToken ct);
    Task<Player?> FindPlayer(int id, CancellationToken ct);
    Task<List<Player>> ListPlayers(int teamId, CancellationToken ct);
    Task<bool> HasPlayers(int teamId, CancellationToken ct);
    void AddTeam(Team team);
    void AddPlayer(Player player);
    void RemoveTeam(Team team);
    void RemovePlayer(Player player);
    Task Save(CancellationToken ct);
    Task CreateTeamWithPlayers(Team team, IReadOnlyList<Player> players, CancellationToken ct);
}
