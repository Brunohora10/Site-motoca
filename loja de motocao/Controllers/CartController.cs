using EssenzStore.Models.ViewModels;
using EssenzStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace EssenzStore.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cart;
    private string SessionId => HttpContext.Session.GetString("CartId") ?? CreateSession();
    private string? UserId => User.Identity?.IsAuthenticated == true
        ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;

    public CartController(ICartService cart) => _cart = cart;

    private string CreateSession()
    {
        var id = Guid.NewGuid().ToString();
        HttpContext.Session.SetString("CartId", id);
        return id;
    }

    public async Task<IActionResult> Index()
    {
        var vm = await _cart.GetCartViewModelAsync(SessionId, UserId);
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddToCartViewModel model)
    {
        var (success, message) = await _cart.AddItemAsync(SessionId, model.ProductId, model.VariantId, model.Quantidade, UserId);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType?.Contains("json") == true)
        {
            var count = await _cart.GetItemCountAsync(SessionId, UserId);
            return Json(new { success, message, cartCount = count });
        }
        if (success) TempData["Success"] = "Produto adicionado ao carrinho!";
        else TempData["Error"] = message;
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Update(int cartItemId, int quantidade)
    {
        var (success, message) = await _cart.UpdateItemAsync(cartItemId, quantidade, SessionId);
        if (!success) TempData["Error"] = message;
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        await _cart.RemoveItemAsync(cartItemId, SessionId);
        TempData["Success"] = "Produto removido do carrinho.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(ApplyCouponViewModel model)
    {
        var (success, message, desconto) = await _cart.ApplyCouponAsync(SessionId, model.Codigo, UserId);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var cart = await _cart.GetCartViewModelAsync(SessionId, UserId);
            return Json(new { success, message, desconto = (double)cart.Desconto, total = (double)cart.Total });
        }
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveCoupon()
    {
        await _cart.RemoveCouponAsync(SessionId);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Count()
    {
        var count = await _cart.GetItemCountAsync(SessionId, UserId);
        return Json(new { count });
    }

    [HttpGet]
    public async Task<IActionResult> MiniCart()
    {
        var vm = await _cart.GetCartViewModelAsync(SessionId, UserId);
        var items = vm.Items.Select(i => new
        {
            cartItemId = i.CartItemId,
            nome = i.NomeProduto,
            variante = string.Join(" / ", new[] { i.Tamanho, i.Cor }.Where(x => !string.IsNullOrEmpty(x))),
            quantidade = i.Quantidade,
            precoUnit = (double)i.PrecoUnitario,
            imageUrl = i.ImageUrl
        });
        return Json(new
        {
            count = vm.Items.Sum(i => i.Quantidade),
            total = (double)vm.Total,
            items
        });
    }
}

public class UpdateCartItem { public int CartItemId { get; set; } public int Quantidade { get; set; } }
public class RemoveCartItem { public int CartItemId { get; set; } }
