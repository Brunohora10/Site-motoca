using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Required, StringLength(50)]
    public string Sku { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Cor { get; set; }

    [StringLength(20)]
    public string? Tamanho { get; set; }

    public int Estoque { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Preco { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecoPromocional { get; set; }

    public decimal Peso { get; set; } = 0.3m;
    public bool Ativo { get; set; } = true;

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
}
