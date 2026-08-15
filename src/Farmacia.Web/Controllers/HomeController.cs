using Farmacia.Web.Data;
using Farmacia.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Categories"] = await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        var featured = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderBy(p => p.Name)
            .Take(8)
            .ToListAsync();

        return View(featured);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
