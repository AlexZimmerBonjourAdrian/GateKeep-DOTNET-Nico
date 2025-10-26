using System.Text.Json.Serialization;
using GateKeep.Api.Application.Beneficios;
using GateKeep.Api.Application.Espacios;
using GateKeep.Api.Endpoints.Beneficios;
using GateKeep.Api.Endpoints.Espacios;
using GateKeep.Api.Infrastructure.Beneficios;
using GateKeep.Api.Infrastructure.Espacios;
using GateKeep.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Bson;

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

// Servicios de Beneficios
builder.Services.AddScoped<IBeneficioRepository, BeneficioRepository>();
builder.Services.AddScoped<IBeneficioService, BeneficioService>();
builder.Services.AddScoped<IBeneficioUsuarioRepository, BeneficioUsuarioRepository>();
builder.Services.AddScoped<IBeneficioUsuarioService, BeneficioUsuarioService>();

// MongoDB - Configuración con Atlas y API estable
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var mongoConfig = builder.Configuration.GetSection("mongodb");
    var connectionString = mongoConfig["connectionString"] ?? "mongodb://localhost:27017";
    var useStableApi = mongoConfig.GetValue<bool>("useStableApi", false);
    
    try
    {
        if (useStableApi)
        {
            // Configuración para MongoDB Atlas con API estable
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            return new MongoClient(settings);
        }
        else
        {
            // Configuración local simple
            return new MongoClient(connectionString);
        }
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Error conectando a MongoDB: {ex.Message}", ex);
    }
});

builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    var mongoConfig = builder.Configuration.GetSection("mongodb");
    var databaseName = mongoConfig["databaseName"] ?? "GateKeepMongo";
    
    return client.GetDatabase(databaseName);
});

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

// MongoDB Health Check con ping usando BsonDocument
app.MapGet("/health/mongodb", (IMongoClient mongoClient) =>
{
    try
    {
        // Ping usando BsonDocument como en el código de Atlas
        var result = mongoClient.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
        return Results.Ok(new { 
            status = "ok", 
            database = "MongoDB Atlas", 
            message = "Pinged your deployment. You successfully connected to MongoDB!",
            pingResult = result.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error conectando a MongoDB Atlas: {ex.Message}");
    }
})
.WithTags("System");

// MongoDB Clear Database - Eliminar todos los datos (Solo en desarrollo)
app.MapDelete("/system/mongodb/clear", (IMongoDatabase mongoDatabase, IWebHostEnvironment env) =>
{
    // Validación de seguridad - Solo permitir en desarrollo
    if (!env.IsDevelopment())
    {
        return Results.Problem(
            "Este endpoint solo está disponible en modo desarrollo",
            statusCode: 403
        );
    }

    try
    {
        // Obtener lista de todas las colecciones
        var collections = mongoDatabase.ListCollectionNames().ToList();
        var deletedCollections = new List<string>();
        var totalDocumentsDeleted = 0;

        foreach (var collectionName in collections)
        {
            var collection = mongoDatabase.GetCollection<BsonDocument>(collectionName);
            var count = collection.CountDocuments(FilterDefinition<BsonDocument>.Empty);
            
            // Eliminar todos los documentos de la colección
            var deleteResult = collection.DeleteMany(FilterDefinition<BsonDocument>.Empty);
            totalDocumentsDeleted += (int)deleteResult.DeletedCount;
            deletedCollections.Add(collectionName);
        }

        return Results.Ok(new
        {
            status = "success",
            message = "Base de datos MongoDB limpiada exitosamente",
            environment = "Development",
            deletedCollections = deletedCollections,
            totalDocumentsDeleted = totalDocumentsDeleted,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error limpiando la base de datos MongoDB: {ex.Message}");
    }
})
.WithTags("System")
.WithSummary("Limpiar todos los datos de MongoDB")
.WithDescription("Elimina todos los documentos de todas las colecciones en la base de datos MongoDB");

// Endpoints
app.MapEdificioEndpoints();
app.MapLaboratorioEndpoints();
app.MapSalonEndpoints();
app.MapBeneficioEndpoints();

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

