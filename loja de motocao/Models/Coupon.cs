using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EssenzStore.Models;

public class Coupon
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    public CouponType Tipo { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal ValorMinimo { get; set; }

    public int? UsoMaximo { get; set; }
    public int? UsoPorCliente { get; set; }
    public int UsoAtual { get; set; }

    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }

    public bool Ativo { get; set; } = true;

    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }

    [StringLength(200)]
    public string? Descricao { get; set; }

    public virtual ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
}

public enum CouponType { Percentual, Fixo, FreteGratis }
