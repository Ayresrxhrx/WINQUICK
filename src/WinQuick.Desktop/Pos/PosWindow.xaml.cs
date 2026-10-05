using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using WinQuick.Application.Sales;
using WinQuick.Infrastructure.Persistence;
using WinQuick.Core.Entities;

namespace WinQuick.Desktop.Pos;

public partial class PosWindow : Window
{
    private readonly WinQuickDbContext _db;
    private readonly ISaleService _sales;
    private readonly Guid _companyId;
    private readonly Guid _userId;
    private readonly PosViewModel _viewModel = new();
    private readonly ObservableCollection<Product> _products = new();
    private bool _loading;

    public PosWindow(WinQuickDbContext db, ISaleService sales, Guid companyId, Guid userId)
    {
        InitializeComponent();
        _db = db;
        _sales = sales;
        _companyId = companyId;
        _userId = userId;
        CartList.ItemsSource = _viewModel.Cart;
        Loaded += async (_, _) => await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        _loading = true;
        try
        {
            var products = await _db.Products.AsNoTracking()
                .Where(x => x.CompanyId == _companyId && x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();
            _products.Clear();
            foreach (var product in products) _products.Add(product);
            ApplyProductFilter();
        }
        finally { _loading = false; }
    }

    private void ApplyProductFilter()
    {
        var text = SearchBox.Text.Trim();
        var filtered = string.IsNullOrWhiteSpace(text)
            ? _products
            : new ObservableCollection<Product>(_products.Where(x =>
                x.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                x.Sku.Contains(text, StringComparison.OrdinalIgnoreCase)));
        ProductsList.ItemsSource = filtered;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_loading) ApplyProductFilter();
    }

    private void Product_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: Product product })
        {
            _viewModel.AddProduct(product);
            RefreshTotals();
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Clear();
        RefreshTotals();
    }

    private async void PayButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.HasItems)
        {
            MessageBox.Show("Adicione pelo menos um produto à venda.", "Venda", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var terminal = await _db.Terminals.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CompanyId == _companyId && x.IsActive);
            if (terminal is null)
            {
                MessageBox.Show("Não existe um terminal activo configurado para esta empresa.", "Ponto de Venda", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var paymentMethod = await _db.PaymentMethods.AsNoTracking()
                .Where(x => x.CompanyId == _companyId && x.IsActive)
                .OrderByDescending(x => x.IsCash)
                .ThenBy(x => x.Name)
                .FirstOrDefaultAsync();
            if (paymentMethod is null)
            {
                MessageBox.Show("Não existe nenhum método de pagamento activo.", "Ponto de Venda", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var command = new CreateSaleCommand(
                _companyId,
                terminal.Id,
                _userId,
                null,
                Guid.NewGuid().ToString("N"),
                _viewModel.Cart.Select(x => new CreateSaleItem(x.ProductId, x.Quantity, x.UnitPrice, 0m)).ToArray(),
                new[] { new CreateSalePayment(paymentMethod.Id, _viewModel.Total, _viewModel.Total, null) });

            var result = await _sales.CreateAsync(command);
            MessageBox.Show($"Venda registada com sucesso.\n\nN.º: {result.Number}\nTotal: MT {result.Total:N2}", "Venda concluída", MessageBoxButton.OK, MessageBoxImage.Information);
            _viewModel.Clear();
            RefreshTotals();
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Não foi possível concluir a venda", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RefreshTotals()
    {
        SubtotalText.Text = $"MT {_viewModel.Subtotal:N2}";
        DiscountText.Text = $"MT {_viewModel.Discount:N2}";
        TotalText.Text = $"MT {_viewModel.Total:N2}";
    }
}
