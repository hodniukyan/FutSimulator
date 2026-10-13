using BrasfootAPI.Models;
using MiniBrasfoot.Domain.Rules;
using Xunit;

namespace MiniBrasfoot.Tests;

public sealed class MatchSimulatorTests
{
    private readonly MatchSimulator simulator = new();
    private static Team Team(int id, int strength) => new() { Id = id, Name = $"Time {id}", Strength = strength };
    private static List<Player> Squad(int id) =>
        [new() { Name = "Goleiro", Position = "GK", TeamId = id, Skill = 20 },
         new() { Name = "Atacante", Position = "ST", TeamId = id, Skill = 20 }];

    [Theory]
    [InlineData(0, "equilibrado")]
    [InlineData(10, "equilibrado")]
    [InlineData(11, "favorito")]
    [InlineData(30, "favorito")]
    [InlineData(31, "massacre")]
    [InlineData(-31, "massacre")]
    public void KeepsOriginalStrengthThresholds(int difference, string expected)
    {
        var result = simulator.Simulate(Team(1, 100 + difference), Team(2, 100), Squad(1), Squad(2), new Random(42));
        Assert.Equal(expected, result.Nivel);
    }

    [Fact]
    public void UsesTruncatedAverageIncludingGoalkeeper()
    {
        var first = Squad(1);
        first[0].Skill = 1;
        first[1].Skill = 20;
        var result = simulator.Simulate(Team(1, 120), Team(2, 100), first, Squad(2), new Random(1));
        Assert.Equal("equilibrado", result.Nivel); // 120 + 10 versus 100 + 20.
    }

    [Fact]
    public void GoalsMatchScoreAndExcludeGoalkeepersAcrossSeeds()
    {
        for (int seed = 0; seed < 500; seed++)
        {
            var result = simulator.Simulate(Team(1, 90), Team(2, 30), Squad(1), Squad(2), new Random(seed));
            Assert.InRange(result.Team1Goals, 0, 5);
            Assert.InRange(result.Team2Goals, 0, 2);
            Assert.Equal(result.Team1Goals, result.Goals.Count(g => g.Team == "Time 1"));
            Assert.Equal(result.Team2Goals, result.Goals.Count(g => g.Team == "Time 2"));
            Assert.All(result.Goals, g => { Assert.Equal("Atacante", g.Player); Assert.InRange(g.Minute, 1, 90); });
            Assert.Equal(result.Goals.Select(g => g.Minute).Order(), result.Goals.Select(g => g.Minute));
        }
    }

    [Fact]
    public void DrawWithoutGoalsIsValid()
    {
        var result = simulator.Simulate(Team(1, 50), Team(2, 50), Squad(1), Squad(2),
            new SequenceRandom(0, 0, 99));
        Assert.Equal(0, result.Team1Goals);
        Assert.Equal(0, result.Team2Goals);
        Assert.Empty(result.Goals);
    }

    [Fact]
    public void UpsetBranchCanProduceLowScoreDraw()
    {
        var result = simulator.Simulate(Team(1, 90), Team(2, 30), Squad(1), Squad(2),
            new SequenceRandom(5, 0, 19, 1, 1, 0, 10, 0, 20));
        Assert.Equal(1, result.Team1Goals);
        Assert.Equal(1, result.Team2Goals);
    }

    [Fact]
    public void RejectsIdenticalTeamsAndSquadsWithoutOutfieldPlayers()
    {
        Assert.Throws<ArgumentException>(() => simulator.Simulate(Team(1, 50), Team(1, 50), Squad(1), Squad(1)));
        Assert.Throws<ArgumentException>(() => simulator.Simulate(Team(1, 50), Team(2, 50), [], Squad(2)));
        Assert.Throws<ArgumentException>(() => simulator.Simulate(Team(1, 50), Team(2, 50), [Squad(1)[0]], Squad(2)));
    }

    private sealed class SequenceRandom(params int[] values) : Random
    {
        private readonly Queue<int> queue = new(values);
        public override int Next(int maxValue) => Next(0, maxValue);
        public override int Next(int minValue, int maxValue)
        {
            int value = queue.Dequeue();
            Assert.InRange(value, minValue, maxValue - 1);
            return value;
        }
    }
}
