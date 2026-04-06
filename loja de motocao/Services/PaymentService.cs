using EssenzStore.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EssenzStore.Services;

public class PaymentService : IPaymentService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;
    private readonly ILogger<PaymentService> _logger;

    private const string BaseUrl = "https://api.mercadopago.com/v1/payments";

    public PaymentService(IConfiguration config, IHttpClientFactory httpFactory, ILogger<PaymentService> logger)
    {
        _config = config;
        _http = httpFactory.CreateClient("MercadoPago");
        _logger = logger;
    }

    private bool IsConfigured => !string.IsNullOrEmpty(_config["MercadoPago:AccessToken"]);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<PaymentResult> CreatePixAsync(Order order)
    {
        if (!IsConfigured) return PixSimulado(order);

        var body = new
        {
            transaction_amount = order.Total,
            description = $"Pedido #{order.NumeroPedido} - Essenz Store",
            payment_method_id = "pix",
            payer = new
            {
                email = order.EmailCliente,
                first_name = order.NomeCliente.Split(' ').First(),
                last_name = order.NomeCliente.Contains(' ') ? order.NomeCliente[(order.NomeCliente.IndexOf(' ') + 1)..] : "",
                identification = new { type = "CPF", number = order.CpfCliente?.Replace(".", "").Replace("-", "") ?? "" }
            }
        };

        return await PostPaymentAsync(body, "PIX");
    }

    public async Task<PaymentResult> CreateCreditCardAsync(Order order, string cardToken, int parcelas)
    {
        if (!IsConfigured) return new PaymentResult { Success = true, TransactionId = $"CARD-SIMUL-{order.NumeroPedido}" };

        var body = new
        {
            transaction_amount = order.Total,
            token = cardToken,
            description = $"Pedido #{order.NumeroPedido} - Essenz Store",
            installments = parcelas,
            payment_method_id = "visa",
            payer = new
            {
                email = order.EmailCliente,
                identification = new { type = "CPF", number = order.CpfCliente?.Replace(".", "").Replace("-", "") ?? "" }
            }
        };

        return await PostPaymentAsync(body, "Cartão");
    }

    public async Task<PaymentResult> CreateBoletoAsync(Order order)
    {
        if (!IsConfigured) return new PaymentResult { Success = true, TransactionId = $"BOL-SIMUL-{order.NumeroPedido}" };

        var body = new
        {
            transaction_amount = order.Total,
            description = $"Pedido #{order.NumeroPedido} - Essenz Store",
            payment_method_id = "bolbradesco",
            payer = new
            {
                email = order.EmailCliente,
                first_name = order.NomeCliente.Split(' ').First(),
                last_name = order.NomeCliente.Contains(' ') ? order.NomeCliente[(order.NomeCliente.IndexOf(' ') + 1)..] : "",
                identification = new { type = "CPF", number = order.CpfCliente?.Replace(".", "").Replace("-", "") ?? "" },
                address = new { zip_code = order.CepEntrega() }
            }
        };

        return await PostPaymentAsync(body, "Boleto");
    }

    private async Task<PaymentResult> PostPaymentAsync(object body, string tipo)
    {
        try
        {
            var token = _config["MercadoPago:AccessToken"]!;
            var json = JsonSerializer.Serialize(body, JsonOpts);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _http.PostAsync(BaseUrl, content);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("MercadoPago {Tipo} falhou: {Status} {Body}", tipo, response.StatusCode, raw);
                return new PaymentResult { ErrorMessage = "Pagamento recusado. Verifique os dados e tente novamente.", RawResponse = raw };
            }

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            var result = new PaymentResult
            {
                Success = true,
                TransactionId = root.GetProperty("id").GetRawText(),
                RawResponse = raw
            };

            if (tipo == "PIX" && root.TryGetProperty("point_of_interaction", out var poi))
            {
                var td = poi.GetProperty("transaction_data");
                result.QrCodePix = td.GetProperty("qr_code").GetString();
                result.QrCodeImageUrl = td.TryGetProperty("qr_code_base64", out var img) ? "data:image/png;base64," + img.GetString() : null;
            }

            if (tipo == "Boleto" && root.TryGetProperty("transaction_details", out var td2))
                result.BoletoUrl = td2.GetProperty("external_resource_url").GetString();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao chamar MercadoPago ({Tipo})", tipo);
            return new PaymentResult { ErrorMessage = "Erro interno ao processar pagamento." };
        }
    }

    // Simulação para ambiente sem AccessToken configurado
    private static PaymentResult PixSimulado(Order order) => new()
    {
        Success = true,
        TransactionId = $"PIX-SIMUL-{order.NumeroPedido}",
        QrCodePix = $"00020126580014br.gov.bcb.pix0136{Guid.NewGuid():N}5204000053039865802BR5925ESSENZ STORE6009SAO PAULO62070503***6304SIMUL",
        QrCodeImageUrl = null
    };
}

// Extension helper
public static class OrderExtensions
{
    public static string CepEntrega(this Order order)
    {
        if (string.IsNullOrEmpty(order.EnderecoEntregaJson)) return "";
        try
        {
            using var doc = JsonDocument.Parse(order.EnderecoEntregaJson);
            return doc.RootElement.TryGetProperty("Cep", out var cep) ? cep.GetString() ?? "" : "";
        }
        catch { return ""; }
    }
}
