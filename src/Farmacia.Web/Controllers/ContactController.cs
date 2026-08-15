using Farmacia.Web.Data;
using Farmacia.Web.Models;
using Farmacia.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Farmacia.Web.Controllers;

public class ContactController : Controller
{
    private readonly ApplicationDbContext _context;

    public ContactController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View(new ContactViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.ContactMessages.Add(new ContactMessage
        {
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            Subject = model.Subject,
            Message = model.Message
        });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Gracias por escribirnos. Te responderemos a la brevedad.";
        return RedirectToAction(nameof(Index));
    }
}
