using System.Text.Json.Serialization;
using GateKeep.Api.Application.Espacios;
using GateKeep.Api.Endpoints.Espacios;
using GateKeep.Api.Infrastructure.Espacios;
using GateKeep.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Cargar config.json
builder.Configuration.AddJsonFile("config.json", optional: false, reloadOnChange: true);

// Swagger (exploración y documentación)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JSON
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// EF Core - PostgreSQL
builder.Services.AddDbContext<GateKeepDbContext>(options =>
{
    // Leer configuración desde config.json
    var config = builder.Configuration.GetSection("database");
    var host = config["host"] ?? "localhost";
    var port = config["port"] ?? "5432";
    var database = config["name"] ?? "GateKeep_Dev";
    var username = config["user"] ?? "postgres";
    var password = config["password"] ?? "dev_password";
    
    var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};";
    
    options.UseNpgsql(connectionString, npgsql =>
    {
        // Usar un esquema interno para el historial de migraciones
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema: "infra");
    });
});

// Factory Pattern para Espacios
builder.Services.AddScoped<IEspacioRepository, EspacioRepository>();
builder.Services.AddScoped<IEspacioFactory, EspacioFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Minimal API
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
  .WithTags("System");

// Endpoints
app.MapEspacioEndpoints();

// (MongoDB eliminado en favor de PostgreSQL)

// Auto-aplicar migraciones al iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GateKeepDbContext>();
    // Mover historial de migraciones a esquema 'infra' si existe en 'public'
    try
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE SCHEMA IF NOT EXISTS infra;
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM information_schema.tables
                    WHERE table_schema = 'public' AND table_name = '__EFMigrationsHistory'
                ) THEN
                    EXECUTE 'ALTER TABLE public.""__EFMigrationsHistory"" SET SCHEMA infra';
                END IF;
            END
            $$;
        ");
    }
    catch
    {
        // Si falla, continuar; la migración seguirá funcionando
    }
    db.Database.Migrate();
}

app.Run();

