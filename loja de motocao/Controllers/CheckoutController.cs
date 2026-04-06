using EssenzStore.Models;
using EssenzStore.Models.ViewModels;
using EssenzStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace EssenzStore.Controllers;

public class CheckoutController : Controller
{
    private readonly ICartService _cart;
    private readonly IOrderService _orders;
    private readonly IShippingService _shipping;
    private readonly IEmailService _email;
    private readonly IPaymentService _payment;

    private string SessionId => HttpContext.Session.GetString("CartId") ?? "";
    private string? UserId => User.Identity?.IsAuthenticated == true
        ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;

    public CheckoutController(ICartService cart, IOrderService orders, IShippingService shipping,
        IEmailService email, IPaymentService payment)
    {
        _cart = cart; _orders = orders; _shipping = shipping; _email = email; _payment = payment;
    }

    public async Task<IActionResult> Index()
    {
        var cartVm = await _cart.GetCartViewModelAsync(SessionId, UserId);
        if (!cartVm.Items.Any()) return RedirectToAction("Index", "Cart");

        var vm = new CheckoutViewModel
        {
            Items = cartVm.Items,
            Subtotal = cartVm.Subtotal,
            Desconto = cartVm.Desconto,
            Total = cartVm.Total,
            CupomCodigo = cartVm.CupomCodigo
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> QuoteFrete(string cep)
    {
        var cartVm = await _cart.GetCartViewModelAsync(SessionId, UserId);
        var peso = cartVm.Items.Sum(i => 0.3m * i.Quantidade);
        var options = await _shipping.QuoteAsync(cep, peso, cartVm.Subtotal);
        return Json(options);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var cartVm = await _cart.GetCartViewModelAsync(SessionId, UserId);
            model.Items = cartVm.Items;
            model.Subtotal = cartVm.Subtotal;
            return View("Index", model);
        }

        var cart = await _cart.GetCartViewModelAsync(SessionId, UserId);
        if (!cart.Items.Any()) return RedirectToAction("Index", "Cart");

        var cartEntity = cart.Cart;
        if (cartEntity == null) return RedirectToAction("Index", "Cart");

        var order = await _orders.CreateOrderAsync(model, cartEntity, UserId);

        // Processar pagamento
        PaymentResult? paymentResult = null;
        paymentResult = model.MetodoPagamento switch
        {
            PaymentMethod.Pix => await _payment.CreatePixAsync(order),
            PaymentMethod.CreditCard => await _payment.CreateCreditCardAsync(order, model.CardToken ?? "", model.CartaoParcelas),
            PaymentMethod.Boleto => await _payment.CreateBoletoAsync(order),
            _ => await _payment.CreatePixAsync(order)
        };

        if (paymentResult.Success)
        {
            var payment = new Payment
            {
                OrderId = order.Id,
                Gateway = "MercadoPago",
                Metodo = model.MetodoPagamento,
                TransactionId = paymentResult.TransactionId,
                Valor = order.Total,
                QrCodePix = paymentResult.QrCodePix,
                QrCodeImageUrl = paymentResult.QrCodeImageUrl,
                Status = model.MetodoPagamento == PaymentMethod.CreditCard
                    ? PaymentStatus.Approved
                    : PaymentStatus.Pending,
                PayloadGatewayJson = paymentResult.RawResponse
            };

            if (payment.Status == PaymentStatus.Approved)
            {
                payment.PaidAt = DateTime.UtcNow;
                await _orders.UpdatePaymentStatusAsync(order.Id, PaymentStatus.Approved, paymentResult.TransactionId);
                await _email.SendPaymentApprovedAsync(model.Email, model.Nome, order.NumeroPedido);
            }
            else
            {
                await _orders.UpdatePaymentStatusAsync(order.Id, PaymentStatus.Pending, paymentResult.TransactionId);
            }
        }

        await _cart.ClearCartAsync(SessionId);
        await _email.SendOrderConfirmationAsync(model.Email, model.Nome, order.NumeroPedido);

        return RedirectToAction("Sucesso", new { numero = order.NumeroPedido });
    }

    public async Task<IActionResult> Sucesso(string numero)
    {
        var order = await _orders.GetOrderByNumberAsync(numero);
        if (order == null) return RedirectToAction("Index", "Home");

        var vm = new OrderSuccessViewModel
        {
            Order = order,
            Payment = order.Payments.FirstOrDefault(),
            QrCodePix = order.Payments.FirstOrDefault(p => p.Metodo == PaymentMethod.Pix)?.QrCodePix
        };
        return View(vm);
    }
}
