using GateKeep.Api.Application.Countries;
using GateKeep.Api.Domain;

namespace GateKeep.Api.Infrastructure;

/// <summary>
/// ** REPOSITORY PATTERN ** (Implementación en memoria)
/// Encapsula la lógica de acceso a datos para la entidad Country.
/// Permite cambiar fácilmente entre implementaciones (memoria, SQL, NoSQL, etc.)
/// sin afectar la lógica de negocio.
/// </summary>
public sealed class CountryMemoryStore : ICountryStore
{
    /// <summary>
    /// ** IN-MEMORY DATA STORE **
    /// Dictionary con comparación case-insensitive para IDs de países
    /// En producción sería reemplazado por Entity Framework, Dapper, etc.
    /// </summary>
    private readonly Dictionary<string, Country> _data = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// ** UPSERT OPERATION **
    /// Insert si no existe, Update si ya existe
    /// Operación idempotente común en APIs REST
    /// </summary>
    public bool Upsert(Country c)
    {
        _data[c.Id] = c;
        return true; // En implementación real podría fallar
    }

    /// <summary>
    /// ** TRY PATTERN **
    /// Evita excepciones costosas para casos de "no encontrado"
    /// </summary>
    public bool TryGet(string id, out Country? c) => _data.TryGetValue(id, out c);

    /// <summary>
    /// ** DEFENSIVE COPY **
    /// Retorna colección inmutable para prevenir modificaciones externas
    /// </summary>
    public IReadOnlyCollection<Country> All() => _data.Values.ToList().AsReadOnly();

    /// <summary>
    /// ** UTILITY METHOD ** para testing y demos
    /// </summary>
    public void Clear() => _data.Clear();
}
