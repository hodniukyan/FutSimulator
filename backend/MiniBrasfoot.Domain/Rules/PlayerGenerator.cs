using BrasfootAPI.Models;

namespace MiniBrasfoot.Domain.Rules;

public sealed class PlayerGenerator
{
    private static readonly string[] FirstNames = ["Lucas", "Rafael", "Gabriel", "Pedro", "André", "Diego", "Bruno", "Matheus", "Felipe", "Daniel", "Caio", "Victor"];
    private static readonly string[] LastNames = ["Silva", "Santos", "Costa", "Lima", "Almeida", "Rocha", "Pereira", "Souza", "Ferreira", "Oliveira", "Martins", "Barbosa"];

    public IReadOnlyList<Player> Generate(Random? random = null)
    {
        random ??= Random.Shared;

        string[] positions = ["GK", "DEF", "DEF", "DEF", "DEF", "MEI", "MEI", "MEI", "MEI", "ATT", "ATT"];
        var names = FirstNames.SelectMany(first => LastNames.Select(last => $"{first} {last}")).ToList();
        var players = new List<Player>(11);
        foreach (var position in positions)
        {
            int index = random.Next(names.Count);
            players.Add(new Player { Name = names[index], Position = position,
                Age = random.Next(18, 33), Skill = random.Next(50, 81) });
            names.RemoveAt(index);
        }
        return players;
    }
}
