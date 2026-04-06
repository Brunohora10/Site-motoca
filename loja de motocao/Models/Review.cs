using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class Review
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int ProductId { get; set; }

    [Range(1, 5)]
    public int Nota { get; set; }

    [StringLength(200)]
    public string? Titulo { get; set; }

    [StringLength(1000)]
    public string? Comentario { get; set; }

    public bool Aprovado { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }
}
