using System.ComponentModel.DataAnnotations;

namespace EssenzStore.Models;

public class Brand
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }

    [StringLength(500)]
    public string? LogoUrl { get; set; }

    [StringLength(500)]
    public string? BannerUrl { get; set; }

    public bool Ativo { get; set; } = true;
    public bool Destaque { get; set; }
    public int Ordem { get; set; }

    [StringLength(200)]
    public string? SeoTitle { get; set; }

    [StringLength(300)]
    public string? SeoDescription { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
