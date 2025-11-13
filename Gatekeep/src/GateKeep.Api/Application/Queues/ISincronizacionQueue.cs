namespace GateKeep.Api.Application.Queues;

/// <summary>
/// Interfaz para la cola de sincronizaciones pendientes
/// </summary>
public interface ISincronizacionQueue
{
    /// <summary>
    /// Encola una sincronización pendiente
    /// </summary>
    void Enqueue(string tipo, long usuarioId);

    /// <summary>
    /// Intenta obtener el siguiente elemento de la cola
    /// </summary>
    bool TryDequeue(out (string tipo, long usuarioId) item);

    /// <summary>
    /// Obtiene el número de elementos pendientes en la cola
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Obtiene el número de elementos pendientes por tipo
    /// </summary>
    int CountByTipo(string tipo);
}

