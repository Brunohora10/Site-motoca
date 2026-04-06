using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class Cart
{
    public int Id { get; set; }
    public string? UserId { get; set; }

    [Required, StringLength(100)]
    public string SessionId { get; set; } = string.Empty;

    public CartStatus Status { get; set; } = CartStatus.Active;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Desconto { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Frete { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    [StringLength(50)]
    public string? CupomCodigo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }

    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public enum CartStatus { Active, Checkout, Converted, Abandoned }
