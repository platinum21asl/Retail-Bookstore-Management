using System.Text.Json;
using TokoBuku.Web.App.Models;

namespace TokoBuku.Web.App.Services;

public interface ICartService
{
    Cart GetCart();
    void AddItem(int bookId, string title, string author, string? coverImageUrl, decimal unitPrice, decimal discountPercent, int quantity = 1);
    void UpdateQuantity(int bookId, int quantity);
    void RemoveItem(int bookId);
    void ClearCart();
}

public class CartService : ICartService
{
    private const string CartSessionKey = "STOREFRONT_CART";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession? Session => _httpContextAccessor.HttpContext?.Session;

    public Cart GetCart()
    {
        if (Session == null) return new Cart();

        var json = Session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(json))
        {
            return new Cart();
        }

        try
        {
            return JsonSerializer.Deserialize<Cart>(json) ?? new Cart();
        }
        catch
        {
            return new Cart();
        }
    }

    public void AddItem(int bookId, string title, string author, string? coverImageUrl, decimal unitPrice, decimal discountPercent, int quantity = 1)
    {
        if (quantity <= 0) return;

        var cart = GetCart();
        var existingItem = cart.Items.FirstOrDefault(i => i.BookId == bookId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                BookId = bookId,
                Title = title,
                Author = author,
                CoverImageUrl = coverImageUrl,
                UnitPrice = unitPrice,
                DiscountPercent = discountPercent,
                Quantity = quantity
            });
        }

        SaveCart(cart);
    }

    public void UpdateQuantity(int bookId, int quantity)
    {
        var cart = GetCart();
        var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);

        if (item != null)
        {
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            SaveCart(cart);
        }
    }

    public void RemoveItem(int bookId)
    {
        var cart = GetCart();
        var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);
        if (item != null)
        {
            cart.Items.Remove(item);
            SaveCart(cart);
        }
    }

    public void ClearCart()
    {
        var cart = new Cart();
        SaveCart(cart);
    }

    private void SaveCart(Cart cart)
    {
        if (Session != null)
        {
            var json = JsonSerializer.Serialize(cart);
            Session.SetString(CartSessionKey, json);
        }
    }
}
