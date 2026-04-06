using EssenzStore.Models;
using EssenzStore.Models.ViewModels;

namespace EssenzStore.Services;

public interface IProductService
{
    Task<(List<Product> Products, int Total)> GetProductsAsync(ProductListViewModel filter);
    Task<Product?> GetBySlugAsync(string slug);
    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> GetRelatedAsync(int productId, int categoryId, int count = 6);
    Task<List<Product>> GetFeaturedAsync(int count = 8);
    Task<List<Product>> GetLancamentosAsync(int count = 8);
    Task<List<Product>> GetMaisVendidosAsync(int count = 8);
    Task<Product?> GetProdutoMomentoAsync();
    Task<List<string>> GetAllTamanhosAsync();
    Task<List<string>> GetAllCoresAsync();
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
}
