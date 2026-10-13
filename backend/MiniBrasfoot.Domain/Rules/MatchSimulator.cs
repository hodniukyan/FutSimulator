using BrasfootAPI.Models;

namespace MiniBrasfoot.Domain.Rules;

public sealed record SimulationResult(string Team1, string Team2, string Nivel,
    int Team1Goals, int Team2Goals, IReadOnlyList<Goal> Goals);

public sealed class MatchSimulator
{
    public SimulationResult Simulate(Team team1, Team team2,
        IReadOnlyList<Player> team1Players, IReadOnlyList<Player> team2Players, Random? random = null)
    {
        if (team1.Id == team2.Id)
            throw new ArgumentException("Selecione dois times diferentes.");
        var scorersTeam1 = team1Players.Where(p => p.Position != "GK").ToList();
        var scorersTeam2 = team2Players.Where(p => p.Position != "GK").ToList();
        if (scorersTeam1.Count == 0 || scorersTeam2.Count == 0)
            throw new ArgumentException("Times precisam ter jogadores de linha.");

        // Preserve the original average, thresholds, random call order and goal limits.
        int team1Strength = team1.Strength + (int)team1Players.Average(p => p.Skill);
        int team2Strength = team2.Strength + (int)team2Players.Average(p => p.Skill);
        random ??= Random.Shared;
        int diff = team1Strength - team2Strength;
        string nivel = Math.Abs(diff) > 30 ? "massacre" : Math.Abs(diff) > 10 ? "favorito" : "equilibrado";
        bool team1IsStronger = diff >= 0;
        int forteGoals;
        int fracoGoals;
        if (nivel == "massacre")
        {
            forteGoals = random.Next(3, 6);
            fracoGoals = random.Next(0, 2);
        }
        else if (nivel == "favorito")
        {
            forteGoals = random.Next(2, 5);
            fracoGoals = random.Next(0, 2);
        }
        else
        {
            forteGoals = random.Next(0, 3);
            fracoGoals = random.Next(0, 3);
        }
        if (random.Next(0, 100) < 20)
        {
            int zebraGoals = random.Next(1, 3);
            int favoritoGoals = random.Next(0, 2);
            forteGoals = favoritoGoals;
            fracoGoals = zebraGoals;
        }
        int team1Goals = team1IsStronger ? forteGoals : fracoGoals;
        int team2Goals = team1IsStronger ? fracoGoals : forteGoals;
        if (!team1IsStronger && team1Goals > 2) team1Goals = 2;
        if (team1IsStronger && team2Goals > 2) team2Goals = 2;
        team1Goals = Math.Min(team1Goals, 5);
        team2Goals = Math.Min(team2Goals, 5);
        var goals = new List<Goal>();
        AddGoals(team1Goals, team1, scorersTeam1);
        AddGoals(team2Goals, team2, scorersTeam2);
        return new(team1.Name, team2.Name, nivel, team1Goals, team2Goals,
            goals.OrderBy(g => g.Minute).ToList());

        void AddGoals(int count, Team team, List<Player> scorers)
        {
            for (int i = 0; i < count; i++)
            {
                var player = scorers[random.Next(scorers.Count)];
                goals.Add(new Goal { Minute = random.Next(1, 91), Team = team.Name, Player = player.Name });
            }
        }
    }
}
