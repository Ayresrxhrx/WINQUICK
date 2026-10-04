using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Invoicing;

public sealed class InvoiceNumberService(IRepository<InvoiceSeries> series, IUnitOfWork unitOfWork)
{
    public async Task<string> NextAsync(Guid companyId, string seriesCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(seriesCode))
            throw new ArgumentException("A série da factura é obrigatória.");

        var value = seriesCode.Trim().ToUpperInvariant();
        var entity = await series.Query()
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Code == value && x.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Série de factura não encontrada ou inactiva.");

        entity.NextNumber++;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        series.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return $"{entity.Code}/{entity.NextNumber:D6}";
    }
}
