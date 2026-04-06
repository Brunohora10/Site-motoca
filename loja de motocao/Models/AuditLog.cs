using System.ComponentModel.DataAnnotations;

namespace EssenzStore.Models;

public class AuditLog
{
    public int Id { get; set; }
    public string? AdminId { get; set; }

    [Required, StringLength(100)]
    public string Acao { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Entidade { get; set; } = string.Empty;

    public int? EntidadeId { get; set; }
    public string? DetalhesJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
