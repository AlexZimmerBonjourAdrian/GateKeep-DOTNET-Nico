using System.Collections.Concurrent;
using GateKeep.Api.Application.Queues;

namespace GateKeep.Api.Infrastructure.Queues;

/// <summary>
/// Implementación de cola de eventos pendientes usando ConcurrentQueue
/// </summary>
public class EventoQueue : IEventoQueue
{
    private readonly ConcurrentQueue<(string tipoEvento, object eventoData)> _queue = new();
    private readonly ConcurrentDictionary<string, int> _countByTipo = new();

    public void Enqueue(string tipoEvento, object eventoData)
    {
        _queue.Enqueue((tipoEvento, eventoData));
        _countByTipo.AddOrUpdate(tipoEvento, 1, (key, value) => value + 1);
    }

    public bool TryDequeue(out (string tipoEvento, object eventoData) item)
    {
        if (_queue.TryDequeue(out item))
        {
            _countByTipo.AddOrUpdate(item.tipoEvento, 0, (key, value) => Math.Max(0, value - 1));
            return true;
        }
        return false;
    }

    public int Count => _queue.Count;

    public int CountByTipo(string tipoEvento)
    {
        return _countByTipo.TryGetValue(tipoEvento, out var count) ? count : 0;
    }
}

