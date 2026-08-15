using System.ComponentModel.DataAnnotations;
using Farmacia.Web.Models.Cart;

namespace Farmacia.Web.Models.ViewModels;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Ingresa tu nombre completo."), StringLength(120)]
    [Display(Name = "Nombre completo")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu correo."), EmailAddress(ErrorMessage = "Correo inválido."), StringLength(160)]
    [Display(Name = "Correo electrónico")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu teléfono."), Phone(ErrorMessage = "Teléfono inválido."), StringLength(30)]
    [Display(Name = "Teléfono")]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa la dirección de entrega."), StringLength(240)]
    [Display(Name = "Dirección de entrega")]
    public string Address { get; set; } = string.Empty;

    [StringLength(400)]
    [Display(Name = "Notas adicionales")]
    public string? Notes { get; set; }

    public List<CartItem> Items { get; set; } = new();

    public decimal Total => Items.Sum(i => i.Subtotal);
}
