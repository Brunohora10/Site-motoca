using EssenzStore.Data;
using EssenzStore.Models;
using EssenzStore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EssenzStore.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;

    public OrderService(ApplicationDbContext db) => _db = db;

    public async Task<Order> CreateOrderAsync(CheckoutViewModel model, Cart cart, string? userId)
    {
        var numero = await GenerateOrderNumberAsync();

        var addressJson = JsonSerializer.Serialize(new
        {
            model.Cep, model.Rua, model.Numero, model.Complemento,
            model.Bairro, model.Cidade, model.Estado, model.Referencia
        });

        var order = new Order
        {
            NumeroPedido = numero,
            UserId = userId,
            NomeCliente = $"{model.Nome} {model.Sobrenome}",
            EmailCliente = model.Email,
            TelefoneCliente = model.Telefone,
            CpfCliente = model.Cpf,
            EnderecoEntregaJson = addressJson,
            Subtotal = cart.Items.Sum(i => i.Subtotal),
            Desconto = cart.Desconto,
            Frete = model.ValorFrete,
            Total = cart.Items.Sum(i => i.Subtotal) - cart.Desconto + model.ValorFrete,
            CupomCodigo = cart.CupomCodigo,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            ShippingStatus = ShippingStatus.Pending
        };

        foreach (var item in cart.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                NomeProdutoSnapshot = item.Product?.Nome ?? "",
                SkuSnapshot = item.Variant?.Sku,
                MarcaSnapshot = item.Product?.Brand?.Nome,
                CorSnapshot = item.Variant?.Cor,
                TamanhoSnapshot = item.Variant?.Tamanho,
                PrecoUnitarioSnapshot = item.PrecoUnitario,
                Quantidade = item.Quantidade,
                Subtotal = item.Subtotal
            });

            // Baixa estoque
            if (item.Variant != null)
            {
                item.Variant.Estoque = Math.Max(0, item.Variant.Estoque - item.Quantidade);
                _db.InventoryMovements.Add(new InventoryMovement
                {
                    VariantId = item.VariantId,
                    Tipo = InventoryMovementType.Saida,
                    Quantidade = item.Quantidade,
                    Origem = "pedido",
                    Observacao = $"Pedido {numero}"
                });
            }
        }

        // Atualiza uso do cupom
        if (!string.IsNullOrEmpty(cart.CupomCodigo))
        {
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Codigo == cart.CupomCodigo);
            if (coupon != null)
            {
                coupon.UsoAtual++;
                _db.CouponUsages.Add(new CouponUsage
                {
                    CouponId = coupon.Id,
                    UserId = userId,
                    UsedAt = DateTime.UtcNow
                });
            }
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(int id) =>
        await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Images)
            .Include(o => o.Payments)
            .Include(o => o.Shipments).ThenInclude(s => s.TrackingEvents)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order?> GetOrderByNumberAsync(string numero) =>
        await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Images)
            .Include(o => o.Payments)
            .Include(o => o.Shipments).ThenInclude(s => s.TrackingEvents)
            .FirstOrDefaultAsync(o => o.NumeroPedido == numero);

    public async Task<List<Order>> GetUserOrdersAsync(string userId) =>
        await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return;
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status, string? transactionId = null)
    {
        var order = await _db.Orders.Include(o => o.Payments).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return;
        order.PaymentStatus = status;
        if (status == PaymentStatus.Approved)
            order.Status = OrderStatus.Paid;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"ESS{today:yyMMdd}";
        var count = await _db.Orders.CountAsync(o => o.NumeroPedido.StartsWith(prefix));
        return $"{prefix}{(count + 1):D4}";
    }
}
