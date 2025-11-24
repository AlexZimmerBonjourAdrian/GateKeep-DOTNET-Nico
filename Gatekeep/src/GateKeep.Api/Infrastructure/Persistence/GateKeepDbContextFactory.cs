using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GateKeep.Api.Infrastructure.Persistence;

/// <summary>
/// Factory para crear el DbContext en tiempo de diseño (para migraciones)
/// </summary>
public sealed class GateKeepDbContextFactory : IDesignTimeDbContextFactory<GateKeepDbContext>
{
    public GateKeepDbContext CreateDbContext(string[] args)
    {
        // Leer configuración desde variables de entorno o usar valores por defecto
        var host = Environment.GetEnvironmentVariable("DATABASE__HOST")
            ?? Environment.GetEnvironmentVariable("DB_HOST")
            ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DATABASE__PORT")
            ?? Environment.GetEnvironmentVariable("DB_PORT")
            ?? "5432";
        var database = Environment.GetEnvironmentVariable("DATABASE__NAME")
            ?? Environment.GetEnvironmentVariable("DB_NAME")
            ?? "Gatekeep";
        var username = Environment.GetEnvironmentVariable("DATABASE__USER")
            ?? Environment.GetEnvironmentVariable("DB_USER")
            ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DATABASE__PASSWORD")
            ?? Environment.GetEnvironmentVariable("DB_PASSWORD")
            ?? "dev_password";

        var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};";

        var optionsBuilder = new DbContextOptionsBuilder<GateKeepDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema: "infra");
        });

        return new GateKeepDbContext(optionsBuilder.Options);
    }
}

