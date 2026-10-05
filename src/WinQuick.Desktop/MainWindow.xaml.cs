using WinQuick.Application.Sales;
using WinQuick.Application.Security;
using WinQuick.Infrastructure.Persistence;

namespace WinQuick.Desktop;

public partial class MainWindow : System.Windows.Window
{
    private readonly UserManagementService _userManagement;
    private readonly WinQuickDbContext _db;
    private readonly ISaleService _sales;
    private readonly Guid _companyId;
    private readonly Guid _userId;

    public MainWindow(UserManagementService userManagement, WinQuickDbContext db, ISaleService sales, Guid companyId, Guid userId)
    {
        InitializeComponent();
        _userManagement = userManagement;
        _db = db;
        _sales = sales;
        _companyId = companyId;
        _userId = userId;
    }

    private void SetPage(string title, string subtitle) { PageTitle.Text = title; PageSubtitle.Text = subtitle; }
    private void Dashboard_Click(object sender, System.Windows.RoutedEventArgs e) => SetPage("Dashboard", "Visão geral da operação");
    private void Pos_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var window = new Pos.PosWindow(_db, _sales, _companyId, _userId) { Owner = this };
        window.ShowDialog();
    }
    private void Products_Click(object sender, System.Windows.RoutedEventArgs e) => SetPage("Produtos", "Produtos, preços, categorias e códigos de barras");
    private void Stock_Click(object sender, System.Windows.RoutedEventArgs e) => SetPage("Stock", "Existências e movimentos de stock");
    private void Customers_Click(object sender, System.Windows.RoutedEventArgs e) => SetPage("Clientes", "Clientes, crédito e histórico");
    private void Purchases_Click(object sender, System.Windows.RoutedEventArgs e) => SetPage("Compras", "Fornecedores, compras e recepção");
    private void Invoices_Click(object sender, System.Windows.RoutedEventArgs e) => SetPage("Facturação", "Facturas, recibos e notas de crédito");
    private void Cash_Click(object sender, System.Windows.RoutedEventArgs e) => SetPage("Caixa", "Abertura, movimentos e fecho de caixa");
    private void Reports_Click(object sender, System.Windows.RoutedEventArgs e) => SetPage("Relatórios", "Vendas, facturação, stock e caixa");
    private void Users_Click(object sender, System.Windows.RoutedEventArgs e) { var window = new UsersWindow(_userManagement, _companyId) { Owner = this }; window.ShowDialog(); }
}
