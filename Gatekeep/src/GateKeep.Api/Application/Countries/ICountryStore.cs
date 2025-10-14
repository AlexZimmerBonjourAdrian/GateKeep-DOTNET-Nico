using GateKeep.Api.Domain;

namespace GateKeep.Api.Application.Countries;

/// <summary>
/// ** REPOSITORY PATTERN ** 
/// Abstrae el acceso a datos de países, permitiendo diferentes implementaciones
/// (memoria, base de datos, caché distribuido, etc.) sin cambiar el código cliente
/// </summary>
public interface ICountryStore
{
  /// <summary>
  /// ** UPSERT OPERATION ** - Insert si no existe, Update si ya existe
  /// Patrón común en APIs REST para POST/PUT idempotentes
  /// </summary>
  bool Upsert(Country c);

  /// <summary>
  /// ** TRY PATTERN ** - Evita excepciones en operaciones que pueden fallar
  /// Más eficiente que lanzar/capturar excepciones para flujo normal
  /// </summary>
  bool TryGet(string id, out Country? c);

  /// <summary>
  /// ** READ-ONLY COLLECTION ** - Expone datos sin permitir modificación externa
  /// Principio de inmutabilidad en interfaces públicas
  /// </summary>
  IReadOnlyCollection<Country> All();

  /// <summary>
  /// ** UTILITY METHOD ** - Limpia el store (útil para testing y demos)
  /// </summary>
  void Clear();
}
