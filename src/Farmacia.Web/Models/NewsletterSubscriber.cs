using System.ComponentModel.DataAnnotations;

namespace Farmacia.Web.Models;

public class NewsletterSubscriber
{
    public int Id { get; set; }

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
