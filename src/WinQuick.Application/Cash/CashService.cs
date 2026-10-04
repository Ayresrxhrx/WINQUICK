using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Cash;

public sealed class CashService(
    IRepository<CashSession> sessions,
    IRepository<CashMovement> movements,
    IUnitOfWork unitOfWork) : ICashService
{
    public async Task RegisterPaymentAsync(
        Guid companyId,
        Guid terminalId,
        Guid userId,
        Guid cashSessionId,
        Guid paymentId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0m)
            throw new CashValidationException("O valor do movimento deve ser maior que zero.");

        var session = await sessions.GetByIdAsync(cashSessionId, cancellationToken);
        if (session is null || session.CompanyId != companyId || session.TerminalId != terminalId || !session.IsOpen)
            throw new CashValidationException("A sessão de caixa não existe, não pertence ao terminal ou está fechada.");

        var movement = new CashMovement
        {
            CashSessionId = cashSessionId,
            CompanyId = companyId,
            TerminalId = terminalId,
            UserId = userId,
            Amount = amount,
            Type = "SALE_PAYMENT",
            Reason = "Pagamento de venda",
            PaymentId = paymentId,
            CreatedAtUtc = DateTime.UtcNow
        };

        await movements.AddAsync(movement, cancellationToken);
        session.ExpectedAmount += amount;
        sessions.Update(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
