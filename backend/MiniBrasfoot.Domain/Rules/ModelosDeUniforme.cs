namespace MiniBrasfoot.Domain.Rules;

public static class ModelosDeUniforme
{
    public const string Liso = "uniforme-liso";
    public const string ListradoHorizontal = "uniforme-listrado-horizontal";
    public const string ListradoVertical = "uniforme-listrado-vertical";
    public const string MeioAMeio = "uniforme-meio-a-meio";

    public static bool EhValido(string? modeloDoUniforme) => modeloDoUniforme is
        Liso or ListradoHorizontal or ListradoVertical or MeioAMeio;
}
