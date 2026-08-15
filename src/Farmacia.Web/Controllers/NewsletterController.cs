using Farmacia.Web.Data;
using Farmacia.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Web.Controllers;

public class NewsletterController : Controller
{
    private readonly ApplicationDbContext _context;

    public NewsletterController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(string email, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            TempData["NewsletterError"] = "Ingresa un correo válido.";
            return RedirectBack(returnUrl);
        }

        var alreadySubscribed = await _context.NewsletterSubscribers.AnyAsync(s => s.Email == email);
        if (!alreadySubscribed)
        {
            _context.NewsletterSubscribers.Add(new NewsletterSubscriber { Email = email });
            await _context.SaveChangesAsync();
        }

        TempData["NewsletterSuccess"] = "¡Gracias por suscribirte! Te avisaremos de nuestras novedades.";
        return RedirectBack(returnUrl);
    }

    private IActionResult RedirectBack(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
