using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class StoreSettings
{
    public int Id { get; set; } = 1;

    [Required, StringLength(200)]
    public string NomeLoja { get; set; } = "ESSENZ STORE";

    [StringLength(500)]
    public string? LogoUrl { get; set; }

    [StringLength(500)]
    public string? FaviconUrl { get; set; }

    [StringLength(150)]
    public string? EmailContato { get; set; }

    [StringLength(20)]
    public string? Whatsapp { get; set; }

    [StringLength(100)]
    public string? Instagram { get; set; }

    [StringLength(18)]
    public string? Cnpj { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal FreteGratisValor { get; set; } = 399m;

    [StringLength(50)]
    public string? CupomPrimeiraCompra { get; set; }

    public int PercentualPrimeiraCompra { get; set; } = 5;

    [StringLength(200)]
    public string? Slogan { get; set; }

    [StringLength(300)]
    public string? MetaDescription { get; set; }

    [StringLength(100)]
    public string? Facebook { get; set; }

    [StringLength(100)]
    public string? Tiktok { get; set; }

    [StringLength(100)]
    public string? GaTrackingId { get; set; }

    [StringLength(100)]
    public string? MetaPixelId { get; set; }
}
