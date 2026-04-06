using EssenzStore.Models;
using EssenzStore.Models.ViewModels;

namespace EssenzStore.Services;

public interface ICartService
{
    Task<Cart?> GetCartAsync(string sessionId, string? userId = null);
    Task<Cart> GetOrCreateCartAsync(string sessionId, string? userId = null);
    Task<CartViewModel> GetCartViewModelAsync(string sessionId, string? userId = null);
    Task<(bool Success, string Message)> AddItemAsync(string sessionId, int productId, int variantId, int qty, string? userId = null);
    Task<(bool Success, string Message)> UpdateItemAsync(int cartItemId, int qty, string sessionId);
    Task RemoveItemAsync(int cartItemId, string sessionId);
    Task<(bool Success, string Message, decimal Desconto)> ApplyCouponAsync(string sessionId, string couponCode, string? userId = null);
    Task RemoveCouponAsync(string sessionId);
    Task UpdateShippingAsync(string sessionId, decimal frete);
    Task ClearCartAsync(string sessionId);
    Task MergeGuestCartAsync(string guestSessionId, string userId);
    Task<int> GetItemCountAsync(string sessionId, string? userId = null);
}
