using EssenzStore.Data;
using EssenzStore.Models.ViewModels;
using EssenzStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EssenzStore.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IProductService _products;

    public HomeController(ApplicationDbContext db, IProductService products)
    {
        _db = db;
        _products = products;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeViewModel
        {
            Banners = await _db.Banners
                .Where(b => b.Ativo && b.Posicao == "hero" &&
                    (b.DataInicio == null || b.DataInicio <= DateTime.UtcNow) &&
                    (b.DataFim == null || b.DataFim >= DateTime.UtcNow))
                .OrderBy(b => b.Ordem).ToListAsync(),
            CategoriesDestaque = await _db.Categories
                .Where(c => c.Ativo && c.ParentId == null)
                .OrderBy(c => c.Ordem).Take(8).ToListAsync(),
            Lancamentos = await _products.GetLancamentosAsync(8),
            MaisVendidos = await _products.GetMaisVendidosAsync(8),
            ProdutoMomento = await _products.GetProdutoMomentoAsync(),
            MarcasDestaque = await _db.Brands
                .Where(b => b.Ativo && b.Destaque)
                .OrderBy(b => b.Ordem).Take(8).ToListAsync(),
            Settings = await _db.StoreSettings.FirstOrDefaultAsync()
        };
        return View(vm);
    }

    public IActionResult Error() => View();
}
