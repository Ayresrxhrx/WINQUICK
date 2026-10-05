using System.Windows;
using System.Windows.Controls;
using WinQuick.Application.Products;
using WinQuick.Core.Entities;

namespace WinQuick.Desktop.Products;

public partial class ProductsWindow : Window
{
    private readonly IProductService _products;
    private readonly Guid _companyId;
    public ProductsWindow(IProductService products, Guid companyId)
    {
        InitializeComponent(); _products = products; _companyId = companyId; Loaded += async (_, _) => await LoadAsync();
    }
    private async Task LoadAsync()
    {
        try { ProductsGrid.ItemsSource = await _products.SearchAsync(_companyId, SearchBox.Text, StatusFilter.SelectedIndex == 1); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "WINQUICK — Produtos", MessageBoxButton.OK, MessageBoxImage.Error); }
    }
    private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { if (IsLoaded) await LoadAsync(); }
    private async void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (IsLoaded) await LoadAsync(); }
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();
    private async void NewProduct_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProductEditorWindow(_products, _companyId) { Owner = this };
        if (dialog.ShowDialog() == true) await LoadAsync();
    }
    private async void ProductsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (ProductsGrid.SelectedItem is Product product)
        {
            var dialog = new ProductEditorWindow(_products, _companyId, product) { Owner = this };
            if (dialog.ShowDialog() == true) await LoadAsync();
        }
    }
}

public sealed class ProductEditorWindow : Window
{
    private readonly IProductService _service; private readonly Guid _companyId; private readonly Product? _editing;
    private readonly TextBox _name = new(); private readonly TextBox _sku = new(); private readonly TextBox _barcode = new();
    private readonly TextBox _cost = new(); private readonly TextBox _price = new(); private readonly TextBox _min = new(); private readonly TextBox _max = new();
    private readonly CheckBox _track = new() { Content = "Controlar stock", IsChecked = true, Margin = new Thickness(0, 8, 0, 14) };
    public ProductEditorWindow(IProductService service, Guid companyId, Product? product = null)
    {
        _service = service; _companyId = companyId; _editing = product; Title = product is null ? "Novo produto" : "Editar produto"; Width = 520; Height = 650; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var panel = new StackPanel { Margin = new Thickness(28) }; panel.Children.Add(new TextBlock { Text = Title, FontSize = 24, FontWeight = FontWeights.Bold, Margin = new Thickness(0,0,0,18) });
        Add(panel,"Nome",_name); Add(panel,"SKU",_sku); Add(panel,"Código de barras",_barcode); Add(panel,"Preço de custo",_cost); Add(panel,"Preço de venda",_price); Add(panel,"Stock mínimo",_min); Add(panel,"Stock máximo",_max); panel.Children.Add(_track);
        var save = new Button { Content="Guardar produto", Height=42, Background=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(22,163,74)), Foreground=System.Windows.Media.Brushes.White, FontWeight=FontWeights.SemiBold }; save.Click += Save_Click; panel.Children.Add(save); Content=panel;
        if (product is not null) { _name.Text=product.Name; _sku.Text=product.Sku; _cost.Text=product.CostPrice.ToString("0.##"); _price.Text=product.SalePrice.ToString("0.##"); _min.Text=product.MinimumStock.ToString("0.##"); _max.Text=product.MaximumStock.ToString("0.##"); _track.IsChecked=product.TrackStock; }
    }
    private static void Add(StackPanel p,string label,TextBox box) { p.Children.Add(new TextBlock { Text=label, FontSize=11, FontWeight=FontWeights.SemiBold }); box.Height=34; box.Margin=new Thickness(0,0,0,8); box.Padding=new Thickness(9,5,9,5); p.Children.Add(box); }
    private async void Save_Click(object sender,RoutedEventArgs e)
    {
        try
        {
            if (!decimal.TryParse(_cost.Text,out var cost)||!decimal.TryParse(_price.Text,out var price)||!decimal.TryParse(_min.Text,out var min)||!decimal.TryParse(_max.Text,out var max)) throw new ArgumentException("Preencha os valores numéricos correctamente.");
            if (_editing is null) await _service.CreateAsync(new CreateProductCommand(_companyId,_name.Text,_sku.Text,string.IsNullOrWhiteSpace(_barcode.Text)?null:_barcode.Text,null,cost,price,min,max,null,_track.IsChecked==true));
            else await _service.UpdateAsync(new UpdateProductCommand(_companyId,_editing.Id,_name.Text,_sku.Text,string.IsNullOrWhiteSpace(_barcode.Text)?null:_barcode.Text,_editing.CategoryId,cost,price,min,max,_editing.TaxRateId,_track.IsChecked==true,_editing.IsActive));
            DialogResult=true;
        }
        catch(Exception ex){MessageBox.Show(this,ex.Message,"Não foi possível guardar",MessageBoxButton.OK,MessageBoxImage.Warning);}
    }
}
