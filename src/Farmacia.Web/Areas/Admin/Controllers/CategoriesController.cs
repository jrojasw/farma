using Farmacia.Web.Data;
using Farmacia.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    public IActionResult Create() => View(new Category());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        category.Slug = await GenerateUniqueSlugAsync(category.Name);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Categoría creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        var existing = await _context.Categories.FindAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        if (!string.Equals(existing.Name, category.Name, StringComparison.Ordinal))
        {
            existing.Slug = await GenerateUniqueSlugAsync(category.Name, category.Id);
        }

        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.Icon = category.Icon;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Categoría actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            return NotFound();
        }

        if (category.Products.Any())
        {
            TempData["Error"] = "No puedes eliminar una categoría que tiene productos asociados.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Categoría eliminada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeId = null)
    {
        var normalized = name.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var withoutDiacritics = new string(normalized
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray());
        var baseSlug = System.Text.RegularExpressions.Regex.Replace(withoutDiacritics, @"[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = Guid.NewGuid().ToString("N")[..8];
        }

        var slug = baseSlug;
        var counter = 2;
        while (await _context.Categories.AnyAsync(c => c.Slug == slug && c.Id != excludeId))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}
