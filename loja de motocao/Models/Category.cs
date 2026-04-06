using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }

    [StringLength(500)]
    public string? ImagemUrl { get; set; }

    [StringLength(500)]
    public string? BannerUrl { get; set; }

    public int? ParentId { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;

    [StringLength(200)]
    public string? SeoTitle { get; set; }

    [StringLength(300)]
    public string? SeoDescription { get; set; }

    [ForeignKey(nameof(ParentId))]
    public virtual Category? Parent { get; set; }

    public virtual ICollection<Category> Children { get; set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
