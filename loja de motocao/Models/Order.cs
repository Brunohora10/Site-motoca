using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class Order
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string NumeroPedido { get; set; } = string.Empty;

    public string? UserId { get; set; }

    [Required, StringLength(100)]
    public string NomeCliente { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string EmailCliente { get; set; } = string.Empty;

    [StringLength(20)]
    public string? TelefoneCliente { get; set; }

    [StringLength(14)]
    public string? CpfCliente { get; set; }

    public string EnderecoEntregaJson { get; set; } = "{}";
    public string? EnderecoCobrancaJson { get; set; }

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

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public ShippingStatus ShippingStatus { get; set; } = ShippingStatus.Pending;

    [StringLength(50)]
    public string? Gateway { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }

    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}

public enum OrderStatus
{
    Pending, Paid, Separating, Packed, Shipped, Delivered, Cancelled, Returned, Refunded
}

public enum PaymentStatus { Pending, Approved, Refused, Cancelled, Refunded }

public enum ShippingStatus { Pending, LabelCreated, Posted, InTransit, OutForDelivery, Delivered, Failed }
