using EssenzStore.Models.ViewModels;

namespace EssenzStore.Services;

public interface IShippingService
{
    Task<List<ShippingOption>> QuoteAsync(string cep, decimal peso, decimal valorPedido);
}

public class ShippingService : IShippingService
{
    private readonly IConfiguration _config;
    public ShippingService(IConfiguration config) => _config = config;

    public async Task<List<ShippingOption>> QuoteAsync(string cep, decimal peso, decimal valorPedido)
    {
        await Task.CompletedTask;

        var freteGratis = _config.GetValue<decimal>("Store:FreteGratisValor", 299m);
        var gratis = valorPedido >= freteGratis;

        // Calcula frete baseado na UF do CEP (prefixo)
        var cepNum = int.TryParse(cep.Replace("-", "").Trim(), out var c) ? c : 0;

        decimal pac, sedex;

        // Regiões com frete mais caro (Norte/Nordeste)
        if (cepNum >= 69000000 && cepNum <= 69999999 ||  // AM
            cepNum >= 66000000 && cepNum <= 68899999 ||  // PA/AP/RR
            cepNum >= 57000000 && cepNum <= 57999999 ||  // AL
            cepNum >= 64000000 && cepNum <= 65999999)    // PI/MA
        {
            pac = 34.90m; sedex = 59.90m;
        }
        // Região Sudeste (frete mais barato)
        else if (cepNum >= 1000000 && cepNum <= 19999999 ||   // SP
                 cepNum >= 20000000 && cepNum <= 28999999 ||  // RJ
                 cepNum >= 30000000 && cepNum <= 39999999)    // MG
        {
            pac = 14.90m; sedex = 24.90m;
        }
        else
        {
            pac = 19.90m; sedex = 34.90m;
        }

        return new List<ShippingOption>
        {
            new() { Servico = "pac",   Nome = "PAC",   Transportadora = "Correios", Valor = gratis ? 0 : pac,   PrazoDias = 7 },
            new() { Servico = "sedex", Nome = "SEDEX", Transportadora = "Correios", Valor = gratis ? 0 : sedex, PrazoDias = 3 },
        };
    }
}
