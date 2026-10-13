using MiniBrasfoot.Application.DTOs;

namespace MiniBrasfoot.Application.Validators;

public enum FailureKind { Validation, NotFound, Conflict }
public sealed class UseCaseException(FailureKind kind, string message) : Exception(message)
{
    public FailureKind Kind { get; } = kind;
}

public static class InputRules
{
    public static void Validate(TeamInput input)
    {
        if (input.Strength <= 0) Fail("Força inválida");
        if (string.IsNullOrWhiteSpace(input.Name)) Fail("Nome obrigatório");
        if (string.IsNullOrWhiteSpace(input.Country)) Fail("País obrigatório");
    }
    public static void Validate(PlayerInput input)
    {
        if (input.Skill <= 0) Fail("Skill inválida");
        if (string.IsNullOrWhiteSpace(input.Name)) Fail("Nome obrigatório");
        if (string.IsNullOrWhiteSpace(input.Position)) Fail("Posição é obrigatória");
    }
    private static void Fail(string message) => throw new UseCaseException(FailureKind.Validation, message);
}
