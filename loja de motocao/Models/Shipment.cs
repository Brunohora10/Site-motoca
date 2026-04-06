using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class Shipment
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    [StringLength(100)]
    public string? MetodoEntrega { get; set; }

    [StringLength(100)]
    public string? Transportadora { get; set; }

    [StringLength(100)]
    public string? Servico { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal ValorFrete { get; set; }

    public int PrazoDias { get; set; }

    [StringLength(50)]
    public string? CodigoRastreio { get; set; }

    [StringLength(500)]
    public string? UrlRastreio { get; set; }

    [StringLength(500)]
    public string? EtiquetaUrl { get; set; }

    public ShippingStatus Status { get; set; } = ShippingStatus.Pending;
    public DateTime? PostedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    public virtual ICollection<TrackingEvent> TrackingEvents { get; set; } = new List<TrackingEvent>();
}
