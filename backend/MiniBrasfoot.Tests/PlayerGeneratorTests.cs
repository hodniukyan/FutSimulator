using MiniBrasfoot.Domain.Rules;
using BrasfootAPI.Models;
using Xunit;

namespace MiniBrasfoot.Tests;

public sealed class PlayerGeneratorTests
{
    [Fact]
    public void GeneratesUniqueBalanced442SquadsAcrossSeeds()
    {
        var generator = new PlayerGenerator();
        for (int seed = 0; seed < 200; seed++)
        {
            var players = generator.Generate(new Random(seed));
            Assert.Equal(11, players.Count);
            Assert.Equal(11, players.Select(player => player.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
            Assert.Equal(new[] { "GK", "DEF", "DEF", "DEF", "DEF", "MEI", "MEI", "MEI", "MEI", "ATT", "ATT" }, players.Select(player => player.Position));
            Assert.All(players, player => { Assert.InRange(player.Age, 18, 32); Assert.InRange(player.Skill, 50, 80); Assert.False(string.IsNullOrWhiteSpace(player.Name)); });
        }
    }

    [Fact]
    public void SameSeedReproducesNamesAgesSkillsAndPositions()
    {
        var generator = new PlayerGenerator();
        var first = generator.Generate(new Random(42));
        var second = generator.Generate(new Random(42));
        Assert.Equal(first.Select(player => (player.Name, player.Age, player.Skill, player.Position)),
            second.Select(player => (player.Name, player.Age, player.Skill, player.Position)));
    }

    [Theory]
    [InlineData(false, 18, 50)]
    [InlineData(true, 32, 80)]
    public void IncludesBothRangeBoundaries(bool maximum, int age, int skill)
    {
        var players = new PlayerGenerator().Generate(new BoundaryRandom(maximum));
        Assert.All(players, player => { Assert.Equal(age, player.Age); Assert.Equal(skill, player.Skill); });
        Assert.Equal(11, players.Select(player => player.Name).Distinct().Count());
    }

    [Fact]
    public void GeneratedPlayersWorkWithUnchangedSimulatorAndGoalkeepersDoNotScore()
    {
        var generator = new PlayerGenerator();
        var first = generator.Generate(new Random(1));
        var second = generator.Generate(new Random(2));
        var team1 = new Team { Id = 1, Name = "Casa", Strength = (int)first.Average(player => player.Skill) };
        var team2 = new Team { Id = 2, Name = "Fora", Strength = (int)second.Average(player => player.Skill) };
        foreach (var player in first) player.TeamId = 1;
        foreach (var player in second) player.TeamId = 2;
        for (int seed = 0; seed < 100; seed++)
        {
            var result = new MatchSimulator().Simulate(team1, team2, first, second, new Random(seed));
            Assert.Equal(result.Team1Goals + result.Team2Goals, result.Goals.Count);
            Assert.All(result.Goals, goal => Assert.Contains(
                goal.Team == "Casa" ? first : second, player => player.Position != "GK" && player.Name == goal.Player));
        }
    }

    private sealed class BoundaryRandom(bool maximum) : Random
    {
        public override int Next(int maxValue) => maximum ? maxValue - 1 : 0;
        public override int Next(int minValue, int maxValue) => maximum ? maxValue - 1 : minValue;
    }
}
