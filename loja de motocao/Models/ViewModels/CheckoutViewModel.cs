using System.ComponentModel.DataAnnotations;

namespace EssenzStore.Models.ViewModels;

public class CheckoutViewModel
{
    // Identificação
    [Required(ErrorMessage = "Nome obrigatório")]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sobrenome obrigatório")]
    [StringLength(100)]
    public string Sobrenome { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefone obrigatório")]
    [StringLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "CPF obrigatório")]
    [StringLength(14)]
    public string Cpf { get; set; } = string.Empty;

    // Endereço
    [Required(ErrorMessage = "CEP obrigatório")]
    public string Cep { get; set; } = string.Empty;

    [Required(ErrorMessage = "Endereço obrigatório")]
    public string Rua { get; set; } = string.Empty;

    [Required(ErrorMessage = "Número obrigatório")]
    public string Numero { get; set; } = string.Empty;

    public string? Complemento { get; set; }

    [Required(ErrorMessage = "Bairro obrigatório")]
    public string Bairro { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cidade obrigatória")]
    public string Cidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "Estado obrigatório")]
    public string Estado { get; set; } = string.Empty;

    public string? Referencia { get; set; }

    // Frete
    public string? MetodoEntrega { get; set; }
    public decimal ValorFrete { get; set; }
    public int PrazoDias { get; set; }

    // Pagamento
    [Required]
    public PaymentMethod MetodoPagamento { get; set; } = PaymentMethod.Pix;

    // Cartão
    public string? CartaoNumero { get; set; }
    public string? CartaoNome { get; set; }
    public string? CartaoValidade { get; set; }
    public string? CartaoCvv { get; set; }
    public int CartaoParcelas { get; set; } = 1;
    public string? CardToken { get; set; }

    // Cupom
    public string? CupomCodigo { get; set; }

    // Resumo
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal Total { get; set; }

    // Opções de frete disponíveis
    public List<ShippingOption> OpcoesEntrega { get; set; } = new();
}

public class ShippingOption
{
    public string Servico { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Transportadora { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int PrazoDias { get; set; }
    public string Prazo => PrazoDias == 0 ? "Grátis" : $"{PrazoDias} dia(s) úteis";
}

public class OrderSuccessViewModel
{
    public Order Order { get; set; } = null!;
    public Payment? Payment { get; set; }
    public string? QrCodePix { get; set; }
}
