using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Sales;

public sealed record RefundSaleReturnCommand(Guid SaleReturnId, Guid CashSessionId, string PaymentMethod, string? Reference);

public interface ISaleReturnRefundService
{
    Task<CashMovement> RefundAsync(RefundSaleReturnCommand command, Guid companyId, Guid terminalId, Guid userId, CancellationToken cancellationToken = default);
}

public sealed class SaleReturnRefundService(
    IRepository<SaleReturn> returns,
    IRepository<CashSession> cashSessions,
    IRepository<CashMovement> cashMovements,
    IUnitOfWork unitOfWork) : ISaleReturnRefundService
{
    public async Task<CashMovement> RefundAsync(RefundSaleReturnCommand command, Guid companyId, Guid terminalId, Guid userId, CancellationToken cancellationToken = default)
    {
        var saleReturn = returns.Query().FirstOrDefault(x => x.Id == command.SaleReturnId && x.CompanyId == companyId && !x.IsCancelled)
            ?? throw new InvalidOperationException("Devolução não encontrada ou já cancelada.");

        if (saleReturn.TotalAmount <= 0m)
            throw new InvalidOperationException("A devolução não possui valor para reembolso.");

        var session = cashSessions.Query().FirstOrDefault(x => x.Id == command.CashSessionId && x.CompanyId == companyId && x.TerminalId == terminalId && x.IsOpen)
            ?? throw new InvalidOperationException("A sessão de caixa não está aberta neste terminal.");

        if (string.IsNullOrWhiteSpace(command.PaymentMethod))
            throw new ArgumentException("O método de reembolso é obrigatório.");

        var movement = new CashMovement
        {
            CashSessionId = session.Id,
            CompanyId = companyId,
            TerminalId = terminalId,
            UserId = userId,
            Amount = -saleReturn.TotalAmount,
            Type = "REFUND",
            Reason = $"Reembolso da devolução {saleReturn.Number}",
            PaymentId = null,
            CreatedAtUtc = DateTime.UtcNow
        };

        session.ExpectedAmount -= saleReturn.TotalAmount;
        if (session.ExpectedAmount < 0m)
            throw new InvalidOperationException("O reembolso excede o valor disponível no caixa.");

        cashSessions.Update(session);
        await cashMovements.AddAsync(movement, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return movement;
    }
}
