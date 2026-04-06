using System.ComponentModel.DataAnnotations;

namespace EssenzStore.Models;

public class Banner
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Subtitulo { get; set; }

    [Required, StringLength(500)]
    public string ImagemDesktop { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ImagemMobile { get; set; }

    [StringLength(500)]
    public string? Link { get; set; }

    [StringLength(50)]
    public string Posicao { get; set; } = "hero";

    public bool Ativo { get; set; } = true;
    public int Ordem { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }

    [StringLength(50)]
    public string? TextoBotao { get; set; }
}
