using EssenzStore.Data;
using EssenzStore.Models;
using EssenzStore.Models.ViewModels;
using EssenzStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EssenzStore.Controllers;

[Authorize, Route("favoritos")]
public class WishlistController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    public WishlistController(ApplicationDbContext db, UserManager<ApplicationUser> um) { _db = db; _userManager = um; }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        var items = await _db.WishlistItems
            .Include(w => w.Product).ThenInclude(p => p!.Images)
            .Include(w => w.Product).ThenInclude(p => p!.Brand)
            .Include(w => w.Product).ThenInclude(p => p!.Variants)
            .Where(w => w.UserId == user.Id)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
        return View(items);
    }

    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle([FromBody] int productId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var existing = await _db.WishlistItems.FirstOrDefaultAsync(w => w.UserId == user.Id && w.ProductId == productId);
        if (existing != null)
        {
            _db.WishlistItems.Remove(existing);
            await _db.SaveChangesAsync();
            return Json(new { added = false, message = "Removido dos favoritos" });
        }
        _db.WishlistItems.Add(new WishlistItem { UserId = user.Id, ProductId = productId });
        await _db.SaveChangesAsync();
        return Json(new { added = true, message = "Adicionado aos favoritos" });
    }
}

[Route("busca")]
public class SearchController : Controller
{
    private readonly IProductService _products;
    public SearchController(IProductService products) => _products = products;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q)
    {
        var filter = new ProductListViewModel { SearchQuery = q, PageSize = 24, Page = 1 };
        if (!string.IsNullOrWhiteSpace(q))
        {
            var (products, total) = await _products.GetProductsAsync(filter);
            filter.Products = products;
            filter.TotalCount = total;
        }
        return View(filter);
    }
}
