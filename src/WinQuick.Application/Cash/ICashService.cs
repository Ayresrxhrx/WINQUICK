namespace WinQuick.Application.Cash;

public interface ICashService
{
    Task RegisterPaymentAsync(
        Guid companyId,
        Guid terminalId,
        Guid userId,
        Guid cashSessionId,
        Guid paymentId,
        decimal amount,
        CancellationToken cancellationToken = default);
}
