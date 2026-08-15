using Farmacia.Web.Data;
using Farmacia.Web.Models;
using Farmacia.Web.Models.ViewModels;
using Farmacia.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Farmacia.Web.Controllers;

public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CartService _cart;

    public CheckoutController(ApplicationDbContext context, CartService cart)
    {
        _context = context;
        _cart = cart;
    }

    public IActionResult Index()
    {
        var items = _cart.GetItems();
        if (items.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        return View(new CheckoutViewModel { Items = items });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        model.Items = _cart.GetItems();

        if (model.Items.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var order = new Order
        {
            CustomerName = model.CustomerName,
            CustomerEmail = model.CustomerEmail,
            CustomerPhone = model.CustomerPhone,
            Address = model.Address,
            Notes = model.Notes,
            Total = model.Total,
            Items = model.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _cart.Clear();

        return RedirectToAction(nameof(Confirmation), new { id = order.Id });
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }
}
