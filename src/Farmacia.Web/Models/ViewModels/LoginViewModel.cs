using System.ComponentModel.DataAnnotations;

namespace Farmacia.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu correo."), EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu contraseña."), DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordarme")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
