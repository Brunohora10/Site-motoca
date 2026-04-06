using EssenzStore.Models;

namespace EssenzStore.Services;

public interface IPaymentService
{
    Task<PaymentResult> CreatePixAsync(Order order);
    Task<PaymentResult> CreateCreditCardAsync(Order order, string cardToken, int parcelas);
    Task<PaymentResult> CreateBoletoAsync(Order order);
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? QrCodePix { get; set; }
    public string? QrCodeImageUrl { get; set; }
    public string? BoletoUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RawResponse { get; set; }
}
