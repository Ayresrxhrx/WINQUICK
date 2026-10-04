namespace WinQuick.Core.Security;

public static class Permission
{
    public const string SaleCreate = "sales.create";
    public const string SaleCancel = "sales.cancel";
    public const string SaleReturn = "sales.return";
    public const string SaleManualPrice = "sales.manual_price";
    public const string SaleDiscount = "sales.discount";
    public const string CashOpen = "cash.open";
    public const string CashClose = "cash.close";
    public const string CashMovement = "cash.movement";
    public const string InvoiceIssue = "invoices.issue";
    public const string InvoiceCancel = "invoices.cancel";
    public const string StockAdjust = "stock.adjust";
    public const string PurchaseCreate = "purchases.create";
    public const string PurchaseReceive = "purchases.receive";
    public const string CustomerCredit = "customers.credit";
    public const string SettingsManage = "settings.manage";
    public const string UsersManage = "users.manage";
    public const string ReportsView = "reports.view";
}
