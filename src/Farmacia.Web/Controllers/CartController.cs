using Farmacia.Web.Data;
using Farmacia.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Web.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CartService _cart;

    public CartController(ApplicationDbContext context, CartService cart)
    {
        _context = context;
        _cart = cart;
    }

    public IActionResult Index()
    {
        return View(_cart.GetItems());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product is null || !product.IsActive)
        {
            return NotFound();
        }

        if (quantity < 1)
        {
            quantity = 1;
        }

        _cart.AddItem(product, quantity);
        TempData["Success"] = $"\"{product.Name}\" se agregó al carrito.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(int productId, int quantity)
    {
        _cart.UpdateQuantity(productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        _cart.RemoveItem(productId);
        return RedirectToAction(nameof(Index));
    }
}
