using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Cash;

public sealed record RegisterCashMovementCommand(
    Guid CompanyId,
    Guid TerminalId,
    Guid UserId,
    Guid CashSessionId,
    decimal Amount,
    string Type,
    string Reason);

public sealed record CloseCashSessionCommand(
    Guid CompanyId,
    Guid TerminalId,
    Guid UserId,
    Guid CashSessionId,
    decimal CountedAmount,
    string? Justification);

public sealed record CashCloseResult(
    Guid SessionId,
    decimal ExpectedAmount,
    decimal CountedAmount,
    decimal DifferenceAmount,
    DateTime ClosedAtUtc);

public sealed class CashOperationsService(
    IRepository<CashSession> sessions,
    IRepository<CashMovement> movements,
    IUnitOfWork unitOfWork)
{
    public async Task RegisterManualMovementAsync(
        RegisterCashMovementCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Amount <= 0m)
            throw new CashValidationException("O valor do movimento deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(command.Type))
            throw new CashValidationException("O tipo do movimento é obrigatório.");
        if (string.IsNullOrWhiteSpace(command.Reason))
            throw new CashValidationException("A justificação do movimento é obrigatória.");

        var session = sessions.Query().FirstOrDefault(x =>
            x.Id == command.CashSessionId &&
            x.CompanyId == command.CompanyId &&
            x.TerminalId == command.TerminalId &&
            x.IsOpen);

        if (session is null)
            throw new CashValidationException("A sessão de caixa não está aberta ou não pertence ao terminal.");

        var normalizedType = command.Type.Trim().ToUpperInvariant();
        if (normalizedType is not ("REINFORCEMENT" or "WITHDRAWAL"))
            throw new CashValidationException("Tipo de movimento inválido. Use REINFORCEMENT ou WITHDRAWAL.");

        var signedAmount = normalizedType == "REINFORCEMENT" ? command.Amount : -command.Amount;
        if (session.ExpectedAmount + signedAmount < 0m)
            throw new CashValidationException("O movimento não pode deixar o saldo esperado negativo.");

        await movements.AddAsync(new CashMovement
        {
            CashSessionId = session.Id,
            CompanyId = command.CompanyId,
            TerminalId = command.TerminalId,
            UserId = command.UserId,
            Amount = signedAmount,
            Type = normalizedType,
            Reason = command.Reason.Trim()
        }, cancellationToken);

        session.ExpectedAmount += signedAmount;
        sessions.Update(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CashCloseResult> CloseAsync(
        CloseCashSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.CountedAmount < 0m)
            throw new CashValidationException("O dinheiro contado não pode ser negativo.");

        var session = sessions.Query().FirstOrDefault(x =>
            x.Id == command.CashSessionId &&
            x.CompanyId == command.CompanyId &&
            x.TerminalId == command.TerminalId &&
            x.IsOpen);

        if (session is null)
            throw new CashValidationException("A sessão de caixa não está aberta ou não pertence ao terminal.");

        var difference = command.CountedAmount - session.ExpectedAmount;
        if (difference != 0m && string.IsNullOrWhiteSpace(command.Justification))
            throw new CashValidationException("É obrigatória uma justificação quando existe diferença no fecho.");

        session.CountedAmount = command.CountedAmount;
        session.DifferenceAmount = difference;
        session.ClosingJustification = string.IsNullOrWhiteSpace(command.Justification)
            ? null
            : command.Justification.Trim();
        session.ClosedAtUtc = DateTime.UtcNow;
        session.IsOpen = false;

        sessions.Update(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CashCloseResult(
            session.Id,
            session.ExpectedAmount,
            session.CountedAmount,
            session.DifferenceAmount,
            session.ClosedAtUtc.Value);
    }
}
