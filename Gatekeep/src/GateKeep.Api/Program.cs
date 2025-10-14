using System.Text.Json.Serialization;

using GateKeep.Api.Application.Countries;
using GateKeep.Api.Domain.Taxes;
using GateKeep.Api.Application.Pricing;
using GateKeep.Api.Application.Receipts;
using GateKeep.Api.Endpoints;
using GateKeep.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Swagger (exploración y documentación)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI de servicios/patrones

// Store en memoria (Singleton)
builder.Services.AddSingleton<ICountryStore, CountryMemoryStore>();

// Strategy overrides (opcionales). Dejamos el diccionario vacío para usar la estrategia configurable por defecto.
builder.Services.AddSingleton<IReadOnlyDictionary<string, ITaxStrategy>>(_ =>
  new Dictionary<string, ITaxStrategy>(StringComparer.OrdinalIgnoreCase));

// Cache en memoria para las estrategias configurables
builder.Services.AddMemoryCache();

// Factory Method: crea componentes de precio combinando Strategy + Decorator
builder.Services.AddSingleton<PriceCalculatorFactory>();

// Template Method: generador de recibos concreto
builder.Services.AddSingleton<ReceiptGenerator, RetailReceiptGenerator>();

// JSON
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Minimal API
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapPricingEndpoints();   // Pricing (Strategy + Decorator + Factory + Cache)
app.MapCountryEndpoints();   // Countries (Repository + Minimal API)

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
  .WithTags("System");

app.Run();

