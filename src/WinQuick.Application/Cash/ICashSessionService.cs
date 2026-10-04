namespace WinQuick.Application.Cash;

public interface ICashSessionService
{
    Task<Guid> OpenAsync(OpenCashSessionCommand command, CancellationToken cancellationToken = default);
}
