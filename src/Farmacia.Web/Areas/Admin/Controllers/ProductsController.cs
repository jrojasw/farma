using Farmacia.Web.Data;
using Farmacia.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(p => p.Name.Contains(q));
        }

        ViewData["SearchTerm"] = q;
        return View(await query.OrderBy(p => p.Name).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new Product { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(product.CategoryId);
            return View(product);
        }

        product.Slug = await GenerateUniqueSlugAsync(product.Name);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Producto creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        await PopulateCategoriesAsync(product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(product.CategoryId);
            return View(product);
        }

        var existing = await _context.Products.FindAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.ImageUrl = product.ImageUrl;
        existing.CategoryId = product.CategoryId;
        existing.RequiresPrescription = product.RequiresPrescription;
        existing.IsFeatured = product.IsFeatured;
        existing.IsActive = product.IsActive;

        if (!string.Equals(existing.Name, product.Name, StringComparison.Ordinal))
        {
            existing.Slug = await GenerateUniqueSlugAsync(product.Name, product.Id);
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Producto eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync(int? selectedId = null)
    {
        ViewData["Categories"] = new SelectList(
            await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", selectedId);
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeId = null)
    {
        var baseSlug = Slugify(name);
        var slug = baseSlug;
        var counter = 2;

        while (await _context.Products.AnyAsync(p => p.Slug == slug && p.Id != excludeId))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private static string Slugify(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var withoutDiacritics = new string(normalized
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray());

        var slug = System.Text.RegularExpressions.Regex.Replace(withoutDiacritics, @"[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? Guid.NewGuid().ToString("N")[..8] : slug;
    }
}
