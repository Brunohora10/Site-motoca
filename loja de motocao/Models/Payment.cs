using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    [StringLength(50)]
    public string Gateway { get; set; } = string.Empty;

    public PaymentMethod Metodo { get; set; }

    [StringLength(200)]
    public string? TransactionId { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; }

    public string? PayloadGatewayJson { get; set; }

    [StringLength(1000)]
    public string? QrCodePix { get; set; }

    [StringLength(200)]
    public string? QrCodeImageUrl { get; set; }

    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }
}

public enum PaymentMethod { Pix, CreditCard, Boleto, Debit }
