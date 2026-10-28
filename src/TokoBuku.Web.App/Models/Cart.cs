namespace TokoBuku.Web.App.Models;

public class CartItem
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public int Quantity { get; set; } = 1;

    public decimal FinalPrice => Math.Round(UnitPrice - (UnitPrice * (DiscountPercent / 100m)), 2);
    public decimal Subtotal => FinalPrice * Quantity;
}

public class Cart
{
    public List<CartItem> Items { get; set; } = new();

    public int TotalItems => Items.Sum(i => i.Quantity);
    public decimal TotalOriginalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);
    public decimal TotalNetAmount => Items.Sum(i => i.Subtotal);
    public decimal TotalDiscountAmount => TotalOriginalAmount - TotalNetAmount;
}
