namespace WinQuick.Core.Common;

public static class Guard
{
    public static void AgainstNull(object? value, string parameterName)
    {
        if (value is null) throw new ArgumentNullException(parameterName);
    }

    public static void AgainstEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("O valor é obrigatório.", parameterName);
    }

    public static void AgainstNonPositive(decimal value, string parameterName)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(parameterName, "O valor deve ser superior a zero.");
    }
}
