namespace WinQuick.Application.Cash;

public sealed record OpenCashSessionCommand(
    Guid CompanyId,
    Guid TerminalId,
    Guid UserId,
    decimal OpeningAmount);
