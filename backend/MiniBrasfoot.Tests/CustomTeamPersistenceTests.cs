using BrasfootAPI.Data;
using BrasfootAPI.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniBrasfoot.Application.DTOs;
using MiniBrasfoot.Application.Services;
using MiniBrasfoot.Domain.Rules;
using MiniBrasfoot.Infrastructure.Persistence;
using Xunit;

namespace MiniBrasfoot.Tests;

// Isolated relational tests. SQLite exists only in Tests, never in the API configuration.
public sealed class CustomTeamPersistenceTests
{
    [Fact]
    public async Task PersistsGeneratedClubAndRosterTogether()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync(); // This new in-memory connection has no existing user data.
        var result = await new CustomTeamService(new EfGameStore(db), new PlayerGenerator()).Create(
            new CustomTeamInput("Aurora FC", "Brasil", "shield-triangular", "#FF6B00", "#111827"), default);
        db.ChangeTracker.Clear();
        var team = await db.Teams.SingleAsync();
        Assert.Equal(result.Team.Id, team.Id);
        Assert.True(team.IsCustom);
        Assert.Equal("shield-triangular", team.BadgeTemplate);
        Assert.Equal("#FF6B00", team.PrimaryColor);
        Assert.Equal("#111827", team.SecondaryColor);
        Assert.Equal(11, await db.Players.CountAsync(player => player.TeamId == team.Id));
        Assert.Equal((int)(await db.Players.ToListAsync()).Average(player => player.Skill), team.Strength);
    }

    [Fact]
    public async Task PlayerFailureRollsBackNewClubAndPreservesExistingData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var existing = new Team { Name = "Existente", Country = "Brasil", Strength = 70 };
        db.Teams.Add(existing);
        await db.SaveChangesAsync();
        var existingPlayer = new Player { Name = "Original", Position = "ATT", Skill = 70, Age = 23, TeamId = existing.Id };
        db.Players.Add(existingPlayer);
        await db.SaveChangesAsync();
        var players = new PlayerGenerator().Generate(new Random(42));
        players[5].Name = null!; // NOT NULL fails after the new team was inserted inside the transaction.
        await Assert.ThrowsAsync<DbUpdateException>(() => new EfGameStore(db).CreateTeamWithPlayers(
            new Team { Name = "Falha", Country = "Brasil", Strength = 60, IsCustom = true }, players, default));
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.Equal(existing.Id, (await db.Teams.AsNoTracking().SingleAsync()).Id);
        Assert.Equal(existingPlayer.Id, (await db.Players.AsNoTracking().SingleAsync()).Id);
    }
}
