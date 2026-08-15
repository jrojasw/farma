using System.Text.Json;
using Farmacia.Web.Models;
using Farmacia.Web.Models.Cart;
using Microsoft.AspNetCore.Http;

namespace Farmacia.Web.Services;

public class CartService
{
    private const string SessionKey = "Cart";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session =>
        _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("No hay una sesión HTTP disponible.");

    public List<CartItem> GetItems()
    {
        var json = Session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json))
        {
            return new List<CartItem>();
        }

        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
    }

    public void AddItem(Product product, int quantity)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(i => i.ProductId == product.Id);

        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            items.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                ImageUrl = product.ImageUrl,
                Quantity = quantity
            });
        }

        Save(items);
    }

    public void UpdateQuantity(int productId, int quantity)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is null)
        {
            return;
        }

        if (quantity <= 0)
        {
            items.Remove(existing);
        }
        else
        {
            existing.Quantity = quantity;
        }

        Save(items);
    }

    public void RemoveItem(int productId)
    {
        var items = GetItems();
        items.RemoveAll(i => i.ProductId == productId);
        Save(items);
    }

    public void Clear()
    {
        Session.Remove(SessionKey);
    }

    public int GetItemCount() => GetItems().Sum(i => i.Quantity);

    public decimal GetTotal() => GetItems().Sum(i => i.Subtotal);

    private void Save(List<CartItem> items)
    {
        Session.SetString(SessionKey, JsonSerializer.Serialize(items));
    }
}
