using System.Collections.Concurrent;
using GateKeep.Api.Application.Queues;

namespace GateKeep.Api.Infrastructure.Queues;

/// <summary>
/// Implementación de cola de sincronizaciones pendientes usando ConcurrentQueue
/// </summary>
public class SincronizacionQueue : ISincronizacionQueue
{
    private readonly ConcurrentQueue<(string tipo, long usuarioId)> _queue = new();
    private readonly ConcurrentDictionary<string, int> _countByTipo = new();

    public void Enqueue(string tipo, long usuarioId)
    {
        _queue.Enqueue((tipo, usuarioId));
        _countByTipo.AddOrUpdate(tipo, 1, (key, value) => value + 1);
    }

    public bool TryDequeue(out (string tipo, long usuarioId) item)
    {
        if (_queue.TryDequeue(out item))
        {
            _countByTipo.AddOrUpdate(item.tipo, 0, (key, value) => Math.Max(0, value - 1));
            return true;
        }
        return false;
    }

    public int Count => _queue.Count;

    public int CountByTipo(string tipo)
    {
        return _countByTipo.TryGetValue(tipo, out var count) ? count : 0;
    }
}

