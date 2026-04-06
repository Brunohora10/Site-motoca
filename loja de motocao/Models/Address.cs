using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class Address
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Apelido { get; set; }

    [Required, StringLength(9)]
    public string Cep { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Rua { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Numero { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Complemento { get; set; }

    [Required, StringLength(100)]
    public string Bairro { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string Estado { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Referencia { get; set; }

    public bool Principal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }
}
