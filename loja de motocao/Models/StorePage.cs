using System.ComponentModel.DataAnnotations;

namespace EssenzStore.Models;

public class StorePage
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required, StringLength(220)]
    public string Slug { get; set; } = string.Empty;

    public string? ConteudoHtml { get; set; }

    [StringLength(200)]
    public string? SeoTitle { get; set; }

    [StringLength(300)]
    public string? SeoDescription { get; set; }

    public bool Publicado { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
