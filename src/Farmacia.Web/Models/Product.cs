using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Farmacia.Web.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(140)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(600)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 100000)]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public bool RequiresPrescription { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [NotMapped]
    public bool InStock => Stock > 0;
}
