using System.ComponentModel.DataAnnotations;

namespace Farmacia.Web.Models;

public class ContactMessage
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    [Required, StringLength(140)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; }
}
