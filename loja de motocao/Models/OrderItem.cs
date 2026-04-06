using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int VariantId { get; set; }

    [Required, StringLength(200)]
    public string NomeProdutoSnapshot { get; set; } = string.Empty;

    [StringLength(50)]
    public string? SkuSnapshot { get; set; }

    [StringLength(100)]
    public string? MarcaSnapshot { get; set; }

    [StringLength(50)]
    public string? CorSnapshot { get; set; }

    [StringLength(20)]
    public string? TamanhoSnapshot { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecoUnitarioSnapshot { get; set; }

    public int Quantidade { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Subtotal { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }

    [ForeignKey(nameof(VariantId))]
    public virtual ProductVariant? Variant { get; set; }
}
