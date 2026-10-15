using System.Text.RegularExpressions;
using BrasfootAPI.Models;
using MiniBrasfoot.Application.DTOs;
using MiniBrasfoot.Application.Interfaces;
using MiniBrasfoot.Application.Validators;
using MiniBrasfoot.Domain.Rules;

namespace MiniBrasfoot.Application.Services;

public sealed partial class CustomTeamService(IGameStore store, PlayerGenerator generator)
{
    public async Task<CustomTeamDto> Create(CustomTeamInput input, CancellationToken ct)
    {
        Validate(input);
        var players = generator.Generate();
        var team = new Team { Name = input.Name.Trim(), Country = input.Country.Trim(),
            Strength = (int)players.Average(player => player.Skill), IsCustom = true,
            BadgeTemplate = input.BadgeTemplate, PrimaryColor = input.PrimaryColor.ToUpperInvariant(),
            SecondaryColor = input.SecondaryColor.ToUpperInvariant(), ModeloDoUniforme = input.ModeloDoUniforme };
        await store.CreateTeamWithPlayers(team, players, ct);
        return new(TeamService.Map(team), players.Select(player => new PlayerDto(player.Id, player.Name,
            player.Age, player.Skill, player.TeamId, player.Position)).ToList());
    }

    private static void Validate(CustomTeamInput input)
    {
        if (!ModelosDeUniforme.EhValido(input.ModeloDoUniforme))
            Fail("Modelo de uniforme inválido.");
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Trim().Length > 100)
            Fail("Informe um nome de clube com até 100 caracteres.");
        if (string.IsNullOrWhiteSpace(input.Country) || input.Country.Trim().Length > 100)
            Fail("Informe um país com até 100 caracteres.");
        if (input.BadgeTemplate is not ("shield-hexagonal" or "shield-round" or "shield-triangular"))
            Fail("Modelo de escudo inválido.");
        if (input.PrimaryColor is null || !HexColor().IsMatch(input.PrimaryColor) ||
            input.SecondaryColor is null || !HexColor().IsMatch(input.SecondaryColor))
            Fail("Informe cores no formato hexadecimal #RRGGBB.");
    }

    [GeneratedRegex(@"\A#[0-9a-fA-F]{6}\z", RegexOptions.CultureInvariant)]
    private static partial Regex HexColor();
    private static void Fail(string message) => throw new UseCaseException(FailureKind.Validation, message);
}
