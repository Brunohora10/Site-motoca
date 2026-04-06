using System.ComponentModel.DataAnnotations;

namespace EssenzStore.Models.ViewModels;

public class CartViewModel
{
    public Cart? Cart { get; set; }
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal Frete { get; set; }
    public decimal Total { get; set; }
    public string? CupomCodigo { get; set; }
    public string? CupomMensagem { get; set; }
    public bool FreteGratisAtingido { get; set; }
    public decimal ValorParaFreteGratis { get; set; }
    public StoreSettings? Settings { get; set; }
}

public class CartItemViewModel
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public int VariantId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    public string? ImageUrl { get; set; }
    public string? Slug { get; set; }
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; }
    public decimal Subtotal { get; set; }
    public int EstoqueDisponivel { get; set; }
}

public class AddToCartViewModel
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public int VariantId { get; set; }

    [Range(1, 10)]
    public int Quantidade { get; set; } = 1;
}

public class ApplyCouponViewModel
{
    [Required]
    public string Codigo { get; set; } = string.Empty;
}
