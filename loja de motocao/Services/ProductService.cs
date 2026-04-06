using EssenzStore.Data;
using EssenzStore.Models;
using EssenzStore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EssenzStore.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;
    private static readonly MemoryCacheEntryOptions _cacheOpts = new MemoryCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

    public ProductService(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    private IQueryable<Product> BaseQuery() =>
        _db.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Where(p => p.Ativo);

    public async Task<(List<Product> Products, int Total)> GetProductsAsync(ProductListViewModel filter)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            query = query.Where(p =>
                p.Nome.Contains(filter.SearchQuery) ||
                p.DescricaoCurta!.Contains(filter.SearchQuery) ||
                p.Brand!.Nome.Contains(filter.SearchQuery));

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId);
        else if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
            query = query.Where(p => p.Category!.Slug == filter.CategorySlug);

        if (filter.BrandId.HasValue)
            query = query.Where(p => p.BrandId == filter.BrandId);
        else if (!string.IsNullOrWhiteSpace(filter.BrandSlug))
            query = query.Where(p => p.Brand!.Slug == filter.BrandSlug);

        if (!string.IsNullOrWhiteSpace(filter.Tamanho))
            query = query.Where(p => p.Variants.Any(v => v.Tamanho == filter.Tamanho && v.Ativo));

        if (!string.IsNullOrWhiteSpace(filter.Cor))
            query = query.Where(p => p.Variants.Any(v => v.Cor == filter.Cor && v.Ativo));

        if (filter.PrecoMin.HasValue)
            query = query.Where(p => p.Preco >= filter.PrecoMin);

        if (filter.PrecoMax.HasValue)
            query = query.Where(p => p.Preco <= filter.PrecoMax);

        if (filter.SomenteLancamentos == true)
            query = query.Where(p => p.Lancamento);

        if (filter.SomentePromo == true)
            query = query.Where(p => p.PrecoPromocional.HasValue && p.PrecoPromocional < p.Preco);

        query = filter.Sort switch
        {
            "preco-asc" => query.OrderBy(p => p.Preco),
            "preco-desc" => query.OrderByDescending(p => p.Preco),
            "lancamentos" => query.OrderByDescending(p => p.CreatedAt),
            "mais-vendidos" => query.OrderByDescending(p => p.MaisVendido).ThenByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.Destaque).ThenByDescending(p => p.CreatedAt)
        };

        var total = await query.CountAsync();
        var products = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (products, total);
    }

    public async Task<Product?> GetBySlugAsync(string slug) =>
        await BaseQuery()
            .Include(p => p.Reviews.Where(r => r.Aprovado))
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Slug == slug);

    public async Task<Product?> GetByIdAsync(int id) =>
        await _db.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Product>> GetRelatedAsync(int productId, int categoryId, int count = 6) =>
        await BaseQuery()
            .Where(p => p.CategoryId == categoryId && p.Id != productId)
            .OrderByDescending(p => p.Destaque)
            .Take(count)
            .ToListAsync();

    public async Task<List<Product>> GetFeaturedAsync(int count = 8)
    {
        var key = $"featured_{count}";
        if (_cache.TryGetValue(key, out List<Product>? cached) && cached != null) return cached;
        var result = await BaseQuery().Where(p => p.Destaque).OrderByDescending(p => p.CreatedAt).Take(count).ToListAsync();
        _cache.Set(key, result, _cacheOpts);
        return result;
    }

    public async Task<List<Product>> GetLancamentosAsync(int count = 8)
    {
        var key = $"lancamentos_{count}";
        if (_cache.TryGetValue(key, out List<Product>? cached) && cached != null) return cached;
        var result = await BaseQuery().Where(p => p.Lancamento).OrderByDescending(p => p.CreatedAt).Take(count).ToListAsync();
        _cache.Set(key, result, _cacheOpts);
        return result;
    }

    public async Task<List<Product>> GetMaisVendidosAsync(int count = 8)
    {
        var key = $"maisvendidos_{count}";
        if (_cache.TryGetValue(key, out List<Product>? cached) && cached != null) return cached;
        var result = await BaseQuery().Where(p => p.MaisVendido).OrderByDescending(p => p.CreatedAt).Take(count).ToListAsync();
        _cache.Set(key, result, _cacheOpts);
        return result;
    }

    public async Task<Product?> GetProdutoMomentoAsync() =>
        await BaseQuery()
            .Where(p => p.ProdutoMomento)
            .FirstOrDefaultAsync();

    public async Task<List<string>> GetAllTamanhosAsync() =>
        await _db.ProductVariants
            .Where(v => v.Ativo && v.Tamanho != null)
            .Select(v => v.Tamanho!)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

    public async Task<List<string>> GetAllCoresAsync() =>
        await _db.ProductVariants
            .Where(v => v.Ativo && v.Cor != null)
            .Select(v => v.Cor!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            product.Ativo = false;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null) =>
        await _db.Products.AnyAsync(p => p.Slug == slug && (!excludeId.HasValue || p.Id != excludeId));
}
