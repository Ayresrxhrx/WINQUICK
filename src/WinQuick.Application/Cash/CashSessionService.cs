using WinQuick.Application.Abstractions;
using WinQuick.Application.Security;
using WinQuick.Core.Entities;
using WinQuick.Core.Security;

namespace WinQuick.Application.Cash;

public sealed class CashSessionService(
    IRepository<CashSession> sessions,
    IPermissionService permissions,
    IUnitOfWork unitOfWork) : ICashSessionService
{
    public async Task<CashSession> OpenAsync(
        OpenCashSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        await permissions.EnsurePermissionAsync(command.UserId, command.CompanyId, Permission.CashOpen, cancellationToken);

        if (command.OpeningAmount < 0m)
            throw new CashValidationException("O fundo inicial não pode ser negativo.");

        var existing = sessions.Query()
            .FirstOrDefault(x => x.CompanyId == command.CompanyId &&
                                 x.TerminalId == command.TerminalId &&
                                 x.IsOpen);

        if (existing is not null)
            throw new CashValidationException("Já existe uma sessão de caixa aberta neste terminal.");

        var session = new CashSession
        {
            CompanyId = command.CompanyId,
            TerminalId = command.TerminalId,
            UserId = command.UserId,
            OpeningAmount = command.OpeningAmount,
            ExpectedAmount = command.OpeningAmount,
            CountedAmount = 0m,
            DifferenceAmount = 0m,
            OpenedAtUtc = DateTime.UtcNow,
            IsOpen = true
        };

        await sessions.AddAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return session;
    }
}
