using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class InventoryMovement
{
    public int Id { get; set; }
    public int VariantId { get; set; }

    public InventoryMovementType Tipo { get; set; }
    public int Quantidade { get; set; }

    [StringLength(300)]
    public string? Observacao { get; set; }

    [StringLength(100)]
    public string? Origem { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? AdminId { get; set; }

    [ForeignKey(nameof(VariantId))]
    public virtual ProductVariant? Variant { get; set; }
}

public enum InventoryMovementType { Entrada, Saida, Ajuste, Reserva, CancelamentoReserva }
