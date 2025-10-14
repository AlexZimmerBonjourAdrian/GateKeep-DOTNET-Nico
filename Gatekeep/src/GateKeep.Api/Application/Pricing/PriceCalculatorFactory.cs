using GateKeep.Api.Application.Countries;
using Microsoft.Extensions.Caching.Memory;

namespace GateKeep.Api.Application.Pricing;

/// <summary>
/// Factory Method: crea el componente de precio para un país dado,
/// combinando Strategy (impuestos), Decorator (descuento) y Cache.
/// - Strategy: usa la tasa de impuesto (taxRate) del país registrado.
/// - Decorator: aplica descuento si "Pricing:DiscountRate" > 0 en appsettings.
/// </summary>
public sealed class PriceCalculatorFactory(
  ICountryStore countries,
  IMemoryCache cache,
  IConfiguration config)
{
  /// <summary>
    /// Intenta crear el componente de precio para un país.
    /// Devuelve false si el país no está registrado.
    /// </summary>
    public bool TryCreate(string countryId, out IPriceComponent? component)
    {
        // 1) Validamos que el país exista en el store (POST /countries)
        if (!countries.TryGet(countryId, out var country) || country is null)
        {
            component = null;
            return false;
        }

        // 2) Obtenemos (y cacheamos) la Strategy configurada con el taxRate del país
        var strategy = cache.GetOrCreate($"tax:{country.Id}", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30);
            return new ConfigurableTaxStrategy(country.Id, country.TaxRate);
        })!;

        // 3) Leemos el descuento global desde configuración (0..1). Si no está, 0.
        var rate = config.GetValue<decimal?>("Pricing:DiscountRate") ?? 0m;
        if (rate < 0m) rate = 0m;     // saneo básico
        if (rate > 1m) rate = 1m;

        // 4) Componente base (precio + impuestos)
        IPriceComponent baseComponent = new BasePriceComponent(strategy);

        // 5) Decorator: si hay descuento, lo aplicamos; si no, devolvemos el base
        component = rate > 0m
            ? new DiscountDecorator(baseComponent, rate)
            : baseComponent;

        return true;
    }
}
