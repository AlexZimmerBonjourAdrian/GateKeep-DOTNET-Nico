
using GateKeep.Api.Application.Countries;   
using GateKeep.Api.Application.Pricing;     
using GateKeep.Api.Application.Receipts;    
using GateKeep.Api.Contracts;               

namespace GateKeep.Api.Endpoints;

public static class PricingEndpoints
{
  public static IEndpointRouteBuilder MapPricingEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/pricing").WithTags("Pricing");

    // GET /pricing/{country}/{amount}
    group.MapGet("/{country}/{amount:decimal}",
        (string country, decimal amount,
          ICountryStore store,
          PriceCalculatorFactory factory,
          ReceiptGenerator receipt) =>
        {
          // 404 si el país no está registrado (POST /countries)
          if (!store.TryGet(country, out var _))
            return Results.NotFound(new { error = "Country not registered", country });

          // Invariante: si el país está registrado, la fábrica debería poder crear un componente.
          // Si no puede, es un error del servidor (configuración inconsistente).
          if (!factory.TryCreate(country, out var calc) || calc is null)
            return Results.Problem(
              title: "Pricing configuration error",
              detail: $"No se pudo crear el calculador para '{country}' pese a estar registrado.",
              statusCode: 500);

          var final = calc.Compute(amount);
          var text  = receipt.Generate(countryId: calc.CountryId, baseAmount: amount, finalAmount: final);

          return Results.Ok(new PricingResponse(country, amount, final, text));
        })
      .WithName("GetPrice")
      .WithOpenApi();

    return app;
  }
}
