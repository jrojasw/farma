using Farmacia.Web.Data;
using Farmacia.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(OrderStatus? status)
    {
        var query = _context.Orders.AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        ViewData["CurrentStatus"] = status;
        return View(await query.OrderByDescending(o => o.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        order.Status = status;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Estado del pedido actualizado.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
