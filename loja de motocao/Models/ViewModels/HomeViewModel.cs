namespace EssenzStore.Models.ViewModels;

public class HomeViewModel
{
    public List<Banner> Banners { get; set; } = new();
    public List<Category> CategoriesDestaque { get; set; } = new();
    public List<Product> Lancamentos { get; set; } = new();
    public List<Product> Novidades { get; set; } = new();
    public List<Product> MaisVendidos { get; set; } = new();
    public Product? ProdutoMomento { get; set; }
    public List<Brand> MarcasDestaque { get; set; } = new();
    public StoreSettings? Settings { get; set; }
}
