namespace MiniBrasfoot.Application.DTOs;

public sealed record TeamInput(string Name, string Country, int Strength);
public sealed record TeamDto(int Id, string Name, string Country, int Strength);
public sealed record PlayerInput(string Name, int Age, int Skill, int TeamId, string Position);
public sealed record PlayerDto(int Id, string Name, int Age, int Skill, int TeamId, string Position);
public sealed record GoalDto(int Minute, string Team, string Player);
public sealed record MatchDto(string Team1, string Team2, string Nivel, string Score, IReadOnlyList<GoalDto> Goals);
