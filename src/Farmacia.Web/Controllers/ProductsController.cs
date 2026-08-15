using Farmacia.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Web.Controllers;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? category, string? q, int page = 1)
    {
        const int pageSize = 9;

        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category!.Slug == category);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(p => p.Name.Contains(q) || (p.Description != null && p.Description.Contains(q)));
        }

        var total = await query.CountAsync();
        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["Categories"] = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewData["CurrentCategory"] = category;
        ViewData["SearchTerm"] = q;
        ViewData["Page"] = page;
        ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)pageSize);

        return View(products);
    }

    public async Task<IActionResult> Details(string slug)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

        if (product is null)
        {
            return NotFound();
        }

        var related = await _context.Products
            .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
            .Take(4)
            .ToListAsync();

        ViewData["Related"] = related;

        return View(product);
    }
}
