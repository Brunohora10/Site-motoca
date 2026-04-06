using System.ComponentModel.DataAnnotations;

namespace EssenzStore.Models;

public class FaqItem
{
    public int Id { get; set; }

    [Required, StringLength(300)]
    public string Pergunta { get; set; } = string.Empty;

    [Required]
    public string Resposta { get; set; } = string.Empty;

    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
