using Microsoft.Extensions.DependencyInjection;
using WinQuick.Application.Cash;
using WinQuick.Application.Customers;
using WinQuick.Application.Invoicing;
using WinQuick.Application.Purchases;
using WinQuick.Application.Sales;
using WinQuick.Application.Suppliers;

namespace WinQuick.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddWinQuickApplication(this IServiceCollection services)
    {
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<ICashSessionService, CashSessionService>();
        services.AddScoped<CashOperationsService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<InvoiceService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        return services;
    }
}
