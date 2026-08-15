using System.ComponentModel.DataAnnotations;

namespace Farmacia.Web.Models;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(90)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(240)]
    public string? Description { get; set; }

    [StringLength(60)]
    public string? Icon { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
