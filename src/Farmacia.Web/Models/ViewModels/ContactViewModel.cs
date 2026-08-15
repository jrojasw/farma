using System.ComponentModel.DataAnnotations;

namespace Farmacia.Web.Models.ViewModels;

public class ContactViewModel
{
    [Required(ErrorMessage = "Ingresa tu nombre."), StringLength(120)]
    [Display(Name = "Nombre completo")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu correo."), EmailAddress(ErrorMessage = "Correo inválido."), StringLength(160)]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Teléfono inválido."), StringLength(30)]
    [Display(Name = "Teléfono (opcional)")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Ingresa un asunto."), StringLength(140)]
    [Display(Name = "Asunto")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe tu mensaje."), StringLength(1000)]
    [Display(Name = "Mensaje")]
    public string Message { get; set; } = string.Empty;
}
