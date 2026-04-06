using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }

    [Required, StringLength(500)]
    public string Url { get; set; } = string.Empty;

    public int Ordem { get; set; }
    public bool Destaque { get; set; }

    [StringLength(200)]
    public string? AltText { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }
}
