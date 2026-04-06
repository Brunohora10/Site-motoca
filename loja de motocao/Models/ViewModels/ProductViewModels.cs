using System.ComponentModel.DataAnnotations;

namespace EssenzStore.Models.ViewModels;

public class ProductListViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Brand> Brands { get; set; } = new();
    public List<string> Tamanhos { get; set; } = new();
    public List<string> Cores { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 24;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public string? SearchQuery { get; set; }
    public int? CategoryId { get; set; }
    public string? CategorySlug { get; set; }
    public int? BrandId { get; set; }
    public string? BrandSlug { get; set; }
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    public decimal? PrecoMin { get; set; }
    public decimal? PrecoMax { get; set; }
    public string? Sort { get; set; }
    public bool? SomenteLancamentos { get; set; }
    public bool? SomentePromo { get; set; }

    public Category? CurrentCategory { get; set; }
    public Brand? CurrentBrand { get; set; }
    public string PageTitle { get; set; } = "Todos os Produtos";
}

public class ProductDetailViewModel
{
    public Product Product { get; set; } = null!;
    public List<Product> Related { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public bool IsInWishlist { get; set; }
    public StoreSettings? Settings { get; set; }
}
