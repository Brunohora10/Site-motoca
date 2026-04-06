using EssenzStore.Data;
using EssenzStore.Models;
using EssenzStore.Models.ViewModels;
using EssenzStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EssenzStore.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _products;
    private readonly ApplicationDbContext _db;

    public ProductsController(IProductService products, ApplicationDbContext db)
    {
        _products = products;
        _db = db;
    }

    public async Task<IActionResult> Index(ProductListViewModel filter)
    {
        filter.PageSize = 24;
        filter.Categories = await _db.Categories.Where(c => c.Ativo && c.ParentId == null).OrderBy(c => c.Ordem).ToListAsync();
        filter.Brands = await _db.Brands.Where(b => b.Ativo).OrderBy(b => b.Nome).ToListAsync();
        filter.Tamanhos = await _products.GetAllTamanhosAsync();
        filter.Cores = await _products.GetAllCoresAsync();

        var (products, total) = await _products.GetProductsAsync(filter);
        filter.Products = products;
        filter.TotalCount = total;

        return View(filter);
    }

    [Route("categoria/{slug}")]
    public async Task<IActionResult> ByCategory(string slug, ProductListViewModel filter)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Slug == slug && c.Ativo);
        if (category == null) return NotFound();

        filter.CategorySlug = slug;
        filter.CurrentCategory = category;
        filter.PageTitle = category.Nome;
        filter.PageSize = 24;
        filter.Categories = await _db.Categories.Where(c => c.Ativo && c.ParentId == null).OrderBy(c => c.Ordem).ToListAsync();
        filter.Brands = await _db.Brands.Where(b => b.Ativo).OrderBy(b => b.Nome).ToListAsync();
        filter.Tamanhos = await _products.GetAllTamanhosAsync();
        filter.Cores = await _products.GetAllCoresAsync();

        var (products, total) = await _products.GetProductsAsync(filter);
        filter.Products = products;
        filter.TotalCount = total;

        return View("Index", filter);
    }

    [Route("marca/{slug}")]
    public async Task<IActionResult> ByBrand(string slug, ProductListViewModel filter)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Slug == slug && b.Ativo);
        if (brand == null) return NotFound();

        filter.BrandSlug = slug;
        filter.CurrentBrand = brand;
        filter.PageTitle = brand.Nome;
        filter.PageSize = 24;
        filter.Categories = await _db.Categories.Where(c => c.Ativo && c.ParentId == null).OrderBy(c => c.Ordem).ToListAsync();
        filter.Brands = await _db.Brands.Where(b => b.Ativo).OrderBy(b => b.Nome).ToListAsync();
        filter.Tamanhos = await _products.GetAllTamanhosAsync();
        filter.Cores = await _products.GetAllCoresAsync();

        var (products, total) = await _products.GetProductsAsync(filter);
        filter.Products = products;
        filter.TotalCount = total;

        return View("Index", filter);
    }

    [Route("produto/{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var product = await _products.GetBySlugAsync(slug);
        if (product == null) return NotFound();

        var related = await _products.GetRelatedAsync(product.Id, product.CategoryId, 6);
        var isInWishlist = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            isInWishlist = userId != null && await _db.WishlistItems.AnyAsync(w => w.UserId == userId && w.ProductId == product.Id);
        }

        var vm = new ProductDetailViewModel
        {
            Product = product,
            Related = related,
            Reviews = product.Reviews.Where(r => r.Aprovado).OrderByDescending(r => r.CreatedAt).ToList(),
            IsInWishlist = isInWishlist,
            Settings = await _db.StoreSettings.FirstOrDefaultAsync()
        };
        return View(vm);
    }

    [Authorize, HttpPost("produto/{slug}/avaliar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(string slug, int nota, string? titulo, string? comentario)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Slug == slug);
        if (product == null) return NotFound();

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return RedirectToAction("Login", "Account");

        // Verificar se já avaliou
        var jaAvaliou = await _db.Reviews.AnyAsync(r => r.ProductId == product.Id && r.UserId == userId);
        if (jaAvaliou)
        {
            TempData["Error"] = "Você já avaliou este produto.";
            return RedirectToAction("Detail", new { slug });
        }

        _db.Reviews.Add(new Review
        {
            ProductId = product.Id,
            UserId = userId,
            Nota = Math.Clamp(nota, 1, 5),
            Titulo = titulo,
            Comentario = comentario,
            Aprovado = false // Admin aprova antes de publicar
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Avaliação enviada! Será publicada após moderação.";
        return RedirectToAction("Detail", new { slug });
    }
}
