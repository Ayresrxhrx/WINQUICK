using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WinQuick.Core.Entities;

namespace WinQuick.Desktop.Pos;

public sealed class PosCartLine : INotifyPropertyChanged
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    private decimal _quantity = 1m;
    public decimal Quantity { get => _quantity; set { if (_quantity == value) return; _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(Total)); } }
    public decimal Total => UnitPrice * Quantity;
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class PosViewModel : INotifyPropertyChanged
{
    public ObservableCollection<PosCartLine> Cart { get; } = new();
    private string _searchText = string.Empty;
    private decimal _discount;
    public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }
    public decimal Discount { get => _discount; set { _discount = Math.Max(0, value); OnPropertyChanged(); OnPropertyChanged(nameof(Total)); } }
    public decimal Subtotal => Cart.Sum(x => x.Total);
    public decimal Tax => 0m;
    public decimal Total => Math.Max(0m, Subtotal + Tax - Discount);
    public bool HasItems => Cart.Count > 0;
    public void AddProduct(Product product) { if (!product.IsActive) return; var line = Cart.FirstOrDefault(x => x.ProductId == product.Id); if (line is null) Cart.Add(new PosCartLine { ProductId = product.Id, Name = product.Name, UnitPrice = product.SalePrice }); else line.Quantity++; RefreshTotals(); }
    public void RemoveLine(PosCartLine line) { Cart.Remove(line); RefreshTotals(); }
    public void Clear() { Cart.Clear(); Discount = 0; RefreshTotals(); }
    public void RefreshTotals() { OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(Tax)); OnPropertyChanged(nameof(Total)); OnPropertyChanged(nameof(HasItems)); }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
