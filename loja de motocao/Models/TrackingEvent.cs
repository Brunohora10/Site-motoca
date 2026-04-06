using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class TrackingEvent
{
    public int Id { get; set; }
    public int ShipmentId { get; set; }

    [Required, StringLength(100)]
    public string Status { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descricao { get; set; }

    [StringLength(200)]
    public string? Local { get; set; }

    public DateTime EventDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ShipmentId))]
    public virtual Shipment? Shipment { get; set; }
}
