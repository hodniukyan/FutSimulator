using BrasfootAPI.Data;
using BrasfootAPI.Models;
using Microsoft.EntityFrameworkCore;
using MiniBrasfoot.Application.Interfaces;

namespace MiniBrasfoot.Infrastructure.Persistence;

public sealed class EfGameStore(AppDbContext db) : IGameStore
{
    public Task<List<Team>> ListTeams(CancellationToken ct) => db.Teams.AsNoTracking().OrderBy(t => t.Id).ToListAsync(ct);
    public async Task<Team?> FindTeam(int id, CancellationToken ct) => await db.Teams.FindAsync([id], ct);
    public async Task<Player?> FindPlayer(int id, CancellationToken ct) => await db.Players.FindAsync([id], ct);
    public Task<List<Player>> ListPlayers(int teamId, CancellationToken ct) =>
        db.Players.AsNoTracking().Where(p => p.TeamId == teamId).OrderBy(p => p.Id).ToListAsync(ct);
    public Task<bool> HasPlayers(int teamId, CancellationToken ct) => db.Players.AnyAsync(p => p.TeamId == teamId, ct);
    public void AddTeam(Team team) => db.Teams.Add(team);
    public void AddPlayer(Player player) => db.Players.Add(player);
    public void RemoveTeam(Team team) => db.Teams.Remove(team);
    public void RemovePlayer(Player player) => db.Players.Remove(player);
    public async Task Save(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
