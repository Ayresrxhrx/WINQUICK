using Microsoft.Extensions.DependencyInjection;
using WinQuick.Application.Auditing;
using WinQuick.Application.Cash;
using WinQuick.Application.Customers;
using WinQuick.Application.Ingredients;
using WinQuick.Application.Invoicing;
using WinQuick.Application.Purchases;
using WinQuick.Application.Sales;
using WinQuick.Application.Security;
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
        services.AddScoped<IIngredientService, IngredientService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ISaleReturnService, SaleReturnService>();
        services.AddScoped<ISaleReturnRefundService, SaleReturnRefundService>();
        services.AddScoped<SaleReturnCreditNoteService>();
        return services;
    }
}
