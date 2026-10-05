using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using WinQuick.Application.Abstractions;
using WinQuick.Application.Stock;
using WinQuick.Core.Entities;
using WinQuick.Infrastructure.Persistence;

namespace WinQuick.Desktop.Stock;

public sealed class StockRow { public Guid ProductId { get; init; } public string Name { get; init; } = ""; public string Sku { get; init; } = ""; public decimal Quantity { get; init; } public decimal Available { get; init; } public decimal Minimum { get; init; } public string Status { get; init; } = ""; }

public partial class StockWindow : Window
{
    private readonly WinQuickDbContext _db; private readonly IStockService _stock; private readonly Guid _companyId; private readonly Guid _userId;
    public StockWindow(WinQuickDbContext db, IStockService stock, Guid companyId, Guid userId) { InitializeComponent(); _db=db; _stock=stock; _companyId=companyId; _userId=userId; Loaded+=async (_,_)=>await LoadAsync(); }
    private async Task LoadAsync()
    {
        var term=SearchBox.Text?.Trim() ?? "";
        var rows=await (from p in _db.Products.AsNoTracking() join b in _db.StockBalances.AsNoTracking() on p.Id equals b.ProductId into bs from b in bs.DefaultIfEmpty() where p.CompanyId==_companyId && p.IsActive && (term=="" || p.Name.Contains(term) || p.Sku.Contains(term)) select new StockRow { ProductId=p.Id, Name=p.Name, Sku=p.Sku, Quantity=b==null?0:b.Quantity, Available=b==null?0:b.Quantity-b.ReservedQuantity, Minimum=p.MinimumStock, Status=(b==null||b.Quantity<=p.MinimumStock)?"Stock baixo":"Normal" }).ToListAsync();
        StockGrid.ItemsSource=rows;
    }
    private async void SearchBox_TextChanged(object sender,TextChangedEventArgs e){if(IsLoaded)await LoadAsync();}
    private async void Adjust_Click(object sender,RoutedEventArgs e)
    {
        if(StockGrid.SelectedItem is not StockRow row){MessageBox.Show(this,"Seleccione um produto para ajustar o stock.","WINQUICK",MessageBoxButton.OK,MessageBoxImage.Information);return;}
        var dialog=new StockAdjustWindow(_stock,_companyId,_userId,row){Owner=this}; if(dialog.ShowDialog()==true)await LoadAsync();
    }
}

public sealed class StockAdjustWindow : Window
{
    private readonly IStockService _stock; private readonly Guid _companyId,_userId; private readonly StockRow _row; private readonly TextBox _qty=new(); private readonly TextBox _reason=new();
    public StockAdjustWindow(IStockService stock,Guid companyId,Guid userId,StockRow row){_stock=stock;_companyId=companyId;_userId=userId;_row=row;Title="Ajustar stock";Width=450;Height=360;WindowStartupLocation=WindowStartupLocation.CenterOwner;var p=new StackPanel{Margin=new Thickness(26)};p.Children.Add(new TextBlock{Text=row.Name,FontSize=21,FontWeight=FontWeights.Bold});p.Children.Add(new TextBlock{Text=$"Stock actual: {row.Quantity:N2}",Foreground=System.Windows.Media.Brushes.Gray,Margin=new Thickness(0,5,0,18)});p.Children.Add(new TextBlock{Text="Nova quantidade",FontWeight=FontWeights.SemiBold});p.Children.Add(_qty);p.Children.Add(new TextBlock{Text="Motivo",FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,12,0,4)});p.Children.Add(_reason);var b=new Button{Content="Guardar ajuste",Height=40,Margin=new Thickness(0,18,0,0),Background=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(22,163,74)),Foreground=System.Windows.Media.Brushes.White};b.Click+=Save_Click;p.Children.Add(b);Content=p;}
    private async void Save_Click(object sender,RoutedEventArgs e){try{if(!decimal.TryParse(_qty.Text,out var q)||q<0)throw new ArgumentException("Introduza uma quantidade válida.");await _stock.AdjustAsync(_companyId,_row.ProductId,q,_userId,null,_reason.Text);DialogResult=true;}catch(Exception ex){MessageBox.Show(this,ex.Message,"Não foi possível ajustar",MessageBoxButton.OK,MessageBoxImage.Warning);}}
}
