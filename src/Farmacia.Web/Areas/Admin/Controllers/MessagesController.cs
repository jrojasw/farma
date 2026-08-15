using Farmacia.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MessagesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MessagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.ContactMessages.OrderByDescending(m => m.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message is null)
        {
            return NotFound();
        }

        if (!message.IsRead)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }

        return View(message);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message is null)
        {
            return NotFound();
        }

        _context.ContactMessages.Remove(message);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Mensaje eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
