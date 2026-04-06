using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(220)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(50)]
    public string? SkuPrincipal { get; set; }

    public int BrandId { get; set; }
    public int CategoryId { get; set; }

    [StringLength(500)]
    public string? DescricaoCurta { get; set; }

    public string? DescricaoCompleta { get; set; }

    [StringLength(200)]
    public string? Composicao { get; set; }

    [StringLength(100)]
    public string? Modelagem { get; set; }

    [StringLength(200)]
    public string? InstrucoesLavagem { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Preco { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecoPromocional { get; set; }

    public decimal PrecoAtual => PrecoPromocional.HasValue && PrecoPromocional < Preco
        ? PrecoPromocional.Value : Preco;

    public int? PercentualDesconto => PrecoPromocional.HasValue && PrecoPromocional < Preco
        ? (int)Math.Round((1 - PrecoPromocional.Value / Preco) * 100) : null;

    public bool Ativo { get; set; } = true;
    public bool Destaque { get; set; }
    public bool Lancamento { get; set; }
    public bool MaisVendido { get; set; }
    public bool ProdutoMomento { get; set; }

    public decimal Peso { get; set; } = 0.3m;
    public decimal Altura { get; set; } = 3m;
    public decimal Largura { get; set; } = 30m;
    public decimal Comprimento { get; set; } = 40m;

    [StringLength(200)]
    public string? SeoTitle { get; set; }

    [StringLength(300)]
    public string? SeoDescription { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(BrandId))]
    public virtual Brand? Brand { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }

    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

    public ProductImage? MainImage => Images.OrderBy(i => i.Ordem).FirstOrDefault(i => i.Destaque)
        ?? Images.OrderBy(i => i.Ordem).FirstOrDefault();

    public double AverageRating => Reviews.Where(r => r.Aprovado).Any()
        ? Reviews.Where(r => r.Aprovado).Average(r => r.Nota) : 0;

    public int TotalEstoque => Variants.Where(v => v.Ativo).Sum(v => v.Estoque);
}
