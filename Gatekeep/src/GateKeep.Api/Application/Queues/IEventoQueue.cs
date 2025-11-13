namespace GateKeep.Api.Application.Queues;

/// <summary>
/// Interfaz para la cola de eventos pendientes
/// </summary>
public interface IEventoQueue
{
    /// <summary>
    /// Encola un evento pendiente
    /// </summary>
    void Enqueue(string tipoEvento, object eventoData);

    /// <summary>
    /// Intenta obtener el siguiente elemento de la cola
    /// </summary>
    bool TryDequeue(out (string tipoEvento, object eventoData) item);

    /// <summary>
    /// Obtiene el número de elementos pendientes en la cola
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Obtiene el número de elementos pendientes por tipo
    /// </summary>
    int CountByTipo(string tipoEvento);
}

