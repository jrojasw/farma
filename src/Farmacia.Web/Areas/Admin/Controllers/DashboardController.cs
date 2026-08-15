using Farmacia.Web.Data;
using Farmacia.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ProductCount"] = await _context.Products.CountAsync();
        ViewData["OrderCount"] = await _context.Orders.CountAsync();
        ViewData["PendingOrderCount"] = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pendiente);
        ViewData["UnreadMessageCount"] = await _context.ContactMessages.CountAsync(m => !m.IsRead);
        var revenueOrders = await _context.Orders
            .Where(o => o.Status != OrderStatus.Cancelado)
            .Select(o => o.Total)
            .ToListAsync();
        ViewData["Revenue"] = revenueOrders.Sum();

        var recentOrders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View(recentOrders);
    }
}
