namespace WinQuick.Application.Security;

public sealed class PermissionDeniedException(string permission) : Exception($"O utilizador não possui a permissão '{permission}'.")
{
    public string Permission { get; } = permission;
}
