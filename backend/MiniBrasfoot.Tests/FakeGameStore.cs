using BrasfootAPI.Models;
using MiniBrasfoot.Application.Interfaces;

namespace MiniBrasfoot.Tests;

internal sealed class FakeGameStore : IGameStore
{
    public List<Team> Teams { get; } = [];
    public List<Player> Players { get; } = [];
    public Task<List<Team>> ListTeams(CancellationToken ct) => Task.FromResult(Teams.ToList());
    public Task<Team?> FindTeam(int id, CancellationToken ct) => Task.FromResult(Teams.FirstOrDefault(t => t.Id == id));
    public Task<Player?> FindPlayer(int id, CancellationToken ct) => Task.FromResult(Players.FirstOrDefault(p => p.Id == id));
    public Task<List<Player>> ListPlayers(int teamId, CancellationToken ct) => Task.FromResult(Players.Where(p => p.TeamId == teamId).ToList());
    public Task<bool> HasPlayers(int teamId, CancellationToken ct) => Task.FromResult(Players.Any(p => p.TeamId == teamId));
    public void AddTeam(Team team) { team.Id = Teams.Select(t => t.Id).DefaultIfEmpty().Max() + 1; Teams.Add(team); }
    public void AddPlayer(Player player) { player.Id = Players.Select(p => p.Id).DefaultIfEmpty().Max() + 1; Players.Add(player); }
    public void RemoveTeam(Team team) => Teams.Remove(team);
    public void RemovePlayer(Player player) => Players.Remove(player);
    public Task Save(CancellationToken ct) => Task.CompletedTask;
}
