using GateKeep.Api.Application.Countries;
using GateKeep.Api.Domain;

namespace GateKeep.Api.Endpoints;

public static class CountryEndpoints
{
  public static IEndpointRouteBuilder MapCountryEndpoints(this IEndpointRouteBuilder app)
  {
    var g = app.MapGroup("/countries").WithTags("Countries");

    // POST /countries
    // Este endpoint registra/actualiza países en el store, lo que habilita la Strategy configurable.
    // Ejemplo de body:
    // { "id": "UY", "name": "Uruguay", "currency": "UYU", "taxRate": 0.22 }
    g.MapPost("/", (Country c, ICountryStore store) =>
      {
        // Validación mínima del taxRate (0..1)
        // La Strategy configurable usa este valor como porcentaje: final = base + base*taxRate
        if (c.TaxRate < 0m || c.TaxRate > 1m)
          return Results.BadRequest(new
          {
            error = "Invalid taxRate",
            expectedRange = "0..1",
            received = c.TaxRate
          });

        store.Upsert(c);
        return Results.Created($"/countries/{c.Id}", c);
      })
      .WithOpenApi();

    // GET /countries/UY
    // Recupera un país por id (útil para verificar la configuración cargada)
    g.MapGet("/{id}", (string id, ICountryStore store) => store.TryGet(id, out var c) ? Results.Ok((object?)c) : Results.NotFound())
      .WithOpenApi();

    // GET /countries
    // Lista todos los países registrados (fuente para la Strategy configurable)
    g.MapGet("/", (ICountryStore store) => Results.Ok(store.All()))
      .WithOpenApi();

    // DELETE /countries (limpia el store - útil para demos/tests)
    g.MapDelete("/", (ICountryStore store) =>
      {
        store.Clear();
        return Results.NoContent();
      })
      .WithOpenApi();

    return app;
  }
}
