using Farmacia.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SubscribersController : Controller
{
    private readonly ApplicationDbContext _context;

    public SubscribersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.NewsletterSubscribers.OrderByDescending(s => s.CreatedAt).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var subscriber = await _context.NewsletterSubscribers.FindAsync(id);
        if (subscriber is null)
        {
            return NotFound();
        }

        _context.NewsletterSubscribers.Remove(subscriber);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Suscriptor eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
