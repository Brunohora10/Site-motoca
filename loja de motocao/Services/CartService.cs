using EssenzStore.Data;
using EssenzStore.Models;
using EssenzStore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EssenzStore.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public CartService(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    private IQueryable<Cart> CartQuery() =>
        _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images)
            .Include(c => c.Items)
                .ThenInclude(i => i.Variant)
            .Where(c => c.Status == CartStatus.Active);

    public async Task<Cart?> GetCartAsync(string sessionId, string? userId = null)
    {
        if (!string.IsNullOrEmpty(userId))
            return await CartQuery().FirstOrDefaultAsync(c => c.UserId == userId);
        return await CartQuery().FirstOrDefaultAsync(c => c.SessionId == sessionId);
    }

    public async Task<Cart> GetOrCreateCartAsync(string sessionId, string? userId = null)
    {
        var cart = await GetCartAsync(sessionId, userId);
        if (cart != null) return cart;

        cart = new Cart
        {
            SessionId = sessionId,
            UserId = userId,
            Status = CartStatus.Active
        };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return cart;
    }

    public async Task<CartViewModel> GetCartViewModelAsync(string sessionId, string? userId = null)
    {
        var cart = await GetCartAsync(sessionId, userId);
        var settings = await _db.StoreSettings.FirstOrDefaultAsync();
        var freteGratis = settings?.FreteGratisValor ?? 399m;

        if (cart == null)
            return new CartViewModel { Settings = settings, ValorParaFreteGratis = freteGratis };

        var items = cart.Items.Select(i => new CartItemViewModel
        {
            CartItemId = i.Id,
            ProductId = i.ProductId,
            VariantId = i.VariantId,
            NomeProduto = i.Product?.Nome ?? "",
            Tamanho = i.Variant?.Tamanho,
            Cor = i.Variant?.Cor,
            ImageUrl = i.Product?.Images.OrderBy(img => img.Ordem).FirstOrDefault()?.Url,
            Slug = i.Product?.Slug,
            PrecoUnitario = i.PrecoUnitario,
            Quantidade = i.Quantidade,
            Subtotal = i.Subtotal,
            EstoqueDisponivel = i.Variant?.Estoque ?? 0
        }).ToList();

        var subtotal = items.Sum(i => i.Subtotal);
        var freteGratisAtingido = subtotal >= freteGratis;

        return new CartViewModel
        {
            Cart = cart,
            Items = items,
            Subtotal = subtotal,
            Desconto = cart.Desconto,
            Frete = freteGratisAtingido ? 0 : cart.Frete,
            Total = subtotal - cart.Desconto + (freteGratisAtingido ? 0 : cart.Frete),
            CupomCodigo = cart.CupomCodigo,
            FreteGratisAtingido = freteGratisAtingido,
            ValorParaFreteGratis = Math.Max(0, freteGratis - subtotal),
            Settings = settings
        };
    }

    public async Task<(bool Success, string Message)> AddItemAsync(string sessionId, int productId, int variantId, int qty, string? userId = null)
    {
        var variant = await _db.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId && v.Ativo);

        if (variant == null) return (false, "Variante não encontrada");
        if (variant.Estoque < qty) return (false, "Estoque insuficiente");

        var cart = await GetOrCreateCartAsync(sessionId, userId);
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);

        var price = variant.Preco ?? variant.Product?.Preco ?? 0;
        if (variant.PrecoPromocional.HasValue && variant.PrecoPromocional < price)
            price = variant.PrecoPromocional.Value;

        if (existingItem != null)
        {
            var newQty = existingItem.Quantidade + qty;
            if (variant.Estoque < newQty) return (false, "Estoque insuficiente");
            existingItem.Quantidade = newQty;
            existingItem.Subtotal = existingItem.PrecoUnitario * newQty;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                VariantId = variantId,
                Quantidade = qty,
                PrecoUnitario = price,
                Subtotal = price * qty
            });
        }

        await RecalcCartAsync(cart);
        return (true, "Produto adicionado ao carrinho");
    }

    public async Task<(bool Success, string Message)> UpdateItemAsync(int cartItemId, int qty, string sessionId)
    {
        var item = await _db.CartItems
            .Include(i => i.Variant)
            .FirstOrDefaultAsync(i => i.Id == cartItemId);

        if (item == null) return (false, "Item não encontrado");
        if (qty <= 0)
        {
            await RemoveItemAsync(cartItemId, sessionId);
            return (true, "Item removido");
        }
        if (item.Variant?.Estoque < qty) return (false, "Estoque insuficiente");

        item.Quantidade = qty;
        item.Subtotal = item.PrecoUnitario * qty;

        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == item.CartId);
        if (cart != null) await RecalcCartAsync(cart);

        return (true, "Carrinho atualizado");
    }

    public async Task RemoveItemAsync(int cartItemId, string sessionId)
    {
        var item = await _db.CartItems.FindAsync(cartItemId);
        if (item == null) return;
        var cartId = item.CartId;
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();

        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == cartId);
        if (cart != null) await RecalcCartAsync(cart);
    }

    public async Task<(bool Success, string Message, decimal Desconto)> ApplyCouponAsync(string sessionId, string couponCode, string? userId = null)
    {
        var coupon = await _db.Coupons
            .Include(c => c.Usages)
            .FirstOrDefaultAsync(c => c.Codigo == couponCode.ToUpper() && c.Ativo);

        if (coupon == null) return (false, "Cupom inválido ou expirado", 0);

        var now = DateTime.UtcNow;
        if (coupon.DataInicio.HasValue && now < coupon.DataInicio) return (false, "Cupom ainda não está ativo", 0);
        if (coupon.DataFim.HasValue && now > coupon.DataFim) return (false, "Cupom expirado", 0);
        if (coupon.UsoMaximo.HasValue && coupon.UsoAtual >= coupon.UsoMaximo) return (false, "Limite de uso atingido", 0);

        if (!string.IsNullOrEmpty(userId) && coupon.UsoPorCliente.HasValue)
        {
            var usedByCustomer = coupon.Usages.Count(u => u.UserId == userId);
            if (usedByCustomer >= coupon.UsoPorCliente) return (false, "Você já utilizou este cupom", 0);
        }

        var cart = await GetOrCreateCartAsync(sessionId, userId);
        var subtotal = cart.Items.Sum(i => i.Subtotal);

        if (subtotal < coupon.ValorMinimo) return (false, $"Pedido mínimo de R$ {coupon.ValorMinimo:N2} para usar este cupom", 0);

        decimal desconto = coupon.Tipo switch
        {
            CouponType.Percentual => subtotal * (coupon.Valor / 100),
            CouponType.Fixo => Math.Min(coupon.Valor, subtotal),
            CouponType.FreteGratis => 0,
            _ => 0
        };

        cart.CupomCodigo = coupon.Codigo;
        cart.Desconto = desconto;
        if (coupon.Tipo == CouponType.FreteGratis) cart.Frete = 0;

        await RecalcCartAsync(cart);
        return (true, $"Cupom aplicado! Desconto de R$ {desconto:N2}", desconto);
    }

    public async Task RemoveCouponAsync(string sessionId)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.SessionId == sessionId);
        if (cart == null) return;
        cart.CupomCodigo = null;
        cart.Desconto = 0;
        await RecalcCartAsync(cart);
    }

    public async Task UpdateShippingAsync(string sessionId, decimal frete)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.SessionId == sessionId);
        if (cart == null) return;
        cart.Frete = frete;
        await RecalcCartAsync(cart);
    }

    public async Task ClearCartAsync(string sessionId)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.SessionId == sessionId);
        if (cart == null) return;
        cart.Items.Clear();
        cart.Status = CartStatus.Converted;
        cart.Desconto = 0;
        cart.Frete = 0;
        cart.Subtotal = 0;
        cart.Total = 0;
        cart.CupomCodigo = null;
        await _db.SaveChangesAsync();
    }

    public async Task MergeGuestCartAsync(string guestSessionId, string userId)
    {
        var guestCart = await CartQuery().FirstOrDefaultAsync(c => c.SessionId == guestSessionId && c.UserId == null);
        if (guestCart == null || !guestCart.Items.Any()) return;

        var userCart = await GetOrCreateCartAsync(userId, userId);
        foreach (var item in guestCart.Items)
            await AddItemAsync(userId, item.ProductId, item.VariantId, item.Quantidade, userId);

        guestCart.Status = CartStatus.Converted;
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetItemCountAsync(string sessionId, string? userId = null)
    {
        var cart = await GetCartAsync(sessionId, userId);
        return cart?.Items.Sum(i => i.Quantidade) ?? 0;
    }

    private async Task RecalcCartAsync(Cart cart)
    {
        cart.Subtotal = cart.Items.Sum(i => i.Subtotal);
        cart.Total = cart.Subtotal - cart.Desconto + cart.Frete;
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
