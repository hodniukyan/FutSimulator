namespace MiniBrasfoot.Application.DTOs;

public sealed record TeamInput(string Name, string Country, int Strength);
public sealed record TeamDto(int Id, string Name, string Country, int Strength,
    bool IsCustom = false, string? BadgeTemplate = null, string? PrimaryColor = null, string? SecondaryColor = null,
    string? ModeloDoUniforme = null);
public sealed record CustomTeamInput(string Name, string Country, string BadgeTemplate, string PrimaryColor, string SecondaryColor,
    string ModeloDoUniforme = "uniforme-liso");
public sealed record CustomTeamDto(TeamDto Team, IReadOnlyList<PlayerDto> Players);
public sealed record PlayerInput(string Name, int Age, int Skill, int TeamId, string Position);
public sealed record PlayerDto(int Id, string Name, int Age, int Skill, int TeamId, string Position);
public sealed record GoalDto(int Minute, string Team, string Player);
public sealed record MatchDto(string Team1, string Team2, string Nivel, string Score, IReadOnlyList<GoalDto> Goals);
