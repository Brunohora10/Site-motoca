using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EssenzStore.Models;

public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(100)]
    public string Sobrenome { get; set; } = string.Empty;

    [StringLength(14)]
    public string? Cpf { get; set; }

    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string NomeCompleto => $"{Nome} {Sobrenome}".Trim();

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual Cart? Cart { get; set; }
}
