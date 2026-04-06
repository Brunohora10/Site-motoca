using EssenzStore.Models;
using EssenzStore.Models.ViewModels;

namespace EssenzStore.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(CheckoutViewModel model, Cart cart, string? userId);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<Order?> GetOrderByNumberAsync(string numero);
    Task<List<Order>> GetUserOrdersAsync(string userId);
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status, string? transactionId = null);
    Task<string> GenerateOrderNumberAsync();
}
