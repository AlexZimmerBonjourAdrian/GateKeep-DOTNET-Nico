namespace GateKeep.Api.Application.Seeding;

/// <summary>
/// Servicio para crear datos iniciales (seed data) en la base de datos
/// </summary>
public interface IDataSeederService
{
    /// <summary>
    /// Crea datos iniciales para todas las entidades de PostgreSQL
    /// </summary>
    Task SeedAsync();

    /// <summary>
    /// Crea solo los recursos que faltan (espacios, beneficios, eventos, etc.) sin verificar usuarios
    /// </summary>
    Task SeedResourcesAsync();
}

