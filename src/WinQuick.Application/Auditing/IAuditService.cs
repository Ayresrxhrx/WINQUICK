using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Auditing;

public interface IAuditService
{
    Task RecordAsync(Guid companyId, Guid? userId, Guid? terminalId, string action, string entityName, Guid? entityId, string? detailsJson, CancellationToken cancellationToken = default);
}

public sealed class AuditService(IRepository<AuditLog> logs, IUnitOfWork unitOfWork) : IAuditService
{
    public async Task RecordAsync(Guid companyId, Guid? userId, Guid? terminalId, string action, string entityName, Guid? entityId, string? detailsJson, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("A acção de auditoria é obrigatória.");
        if (string.IsNullOrWhiteSpace(entityName)) throw new ArgumentException("A entidade de auditoria é obrigatória.");

        await logs.AddAsync(new AuditLog
        {
            CompanyId = companyId,
            UserId = userId,
            TerminalId = terminalId,
            Action = action.Trim(),
            EntityName = entityName.Trim(),
            EntityId = entityId,
            DetailsJson = detailsJson,
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
