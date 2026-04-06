using System.ComponentModel.DataAnnotations;

namespace EssenzStore.Models.ViewModels;

public class AdminDashboardViewModel
{
    public decimal FaturamentoHoje { get; set; }
    public decimal FaturamentoMes { get; set; }
    public int PedidosPendentes { get; set; }
    public int PedidosPagos { get; set; }
    public int PedidosEnviados { get; set; }
    public decimal TicketMedio { get; set; }
    public int TotalClientes { get; set; }
    public int TotalProdutos { get; set; }
    public List<Order> UltimosPedidos { get; set; } = new();
    public List<(string Nome, int Vendas)> ProdutosMaisVendidos { get; set; } = new();
    public List<(string Nome, int Estoque)> EstoqueBaixo { get; set; } = new();
}

public class AdminProductViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome obrigatório")]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(220)]
    public string? Slug { get; set; }

    [Required]
    public int BrandId { get; set; }

    [Required]
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

    [Required, Range(0.01, 999999)]
    public decimal Preco { get; set; }

    public decimal? PrecoPromocional { get; set; }

    public bool Ativo { get; set; } = true;
    public bool Destaque { get; set; }
    public bool Lancamento { get; set; }
    public bool MaisVendido { get; set; }
    public bool ProdutoMomento { get; set; }

    [StringLength(200)]
    public string? SeoTitle { get; set; }

    [StringLength(300)]
    public string? SeoDescription { get; set; }

    public List<Brand> Brands { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<ProductVariant> Variants { get; set; } = new();
    public List<ProductImage> Images { get; set; } = new();
}

public class AdminOrderViewModel
{
    public List<Order> Orders { get; set; } = new();
    public string? StatusFilter { get; set; }
    public string? SearchQuery { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}

public class AdminCouponViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    public CouponType Tipo { get; set; }

    [Required, Range(0.01, 999999)]
    public decimal Valor { get; set; }

    public decimal ValorMinimo { get; set; }
    public int? UsoMaximo { get; set; }
    public int? UsoPorCliente { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool Ativo { get; set; } = true;

    [StringLength(200)]
    public string? Descricao { get; set; }
}

public class AdminFinanceiroViewModel
{
    public decimal Faturamento7Dias { get; set; }
    public decimal Faturamento30Dias { get; set; }
    public decimal FaturamentoMesAtual { get; set; }
    public decimal TotalReembolsosMes { get; set; }
    public int PedidosPagosMes { get; set; }
    public int PedidosAguardandoEnvio { get; set; }
    public int DevolucoesPendentes { get; set; }
    public List<AdminMetodoPagamentoViewModel> VendasPorMetodo { get; set; } = new();
    public List<AdminEstoqueCriticoViewModel> EstoqueCritico { get; set; } = new();
    public List<AdminProdutoSemVendaViewModel> ProdutosSemVenda { get; set; } = new();
}

public class AdminMetodoPagamentoViewModel
{
    public string Metodo { get; set; } = string.Empty;
    public int Pedidos { get; set; }
    public decimal Valor { get; set; }
}

public class AdminEstoqueCriticoViewModel
{
    public string Sku { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
    public int Estoque { get; set; }
}

public class AdminProdutoSemVendaViewModel
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int EstoqueTotal { get; set; }
    public int DiasSemVenda { get; set; }
}

public class AdminTeamMemberViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool Ativo { get; set; }
    public string Permissao { get; set; } = "Manager";
    public DateTime CreatedAt { get; set; }
}
