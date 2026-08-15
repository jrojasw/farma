using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Farmacia.Web.Models;

public enum OrderStatus
{
    Pendiente,
    Confirmado,
    Entregado,
    Cancelado
}

public class Order
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required, StringLength(240)]
    public string Address { get; set; } = string.Empty;

    [StringLength(400)]
    public string? Notes { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pendiente;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
