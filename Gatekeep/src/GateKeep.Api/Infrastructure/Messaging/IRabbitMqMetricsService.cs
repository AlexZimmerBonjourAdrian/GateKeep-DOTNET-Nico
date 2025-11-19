namespace GateKeep.Api.Infrastructure.Messaging;

/// <summary>
/// Servicio para obtener métricas de RabbitMQ desde la API de Management
/// </summary>
public interface IRabbitMqMetricsService
{
    /// <summary>
    /// Obtiene las métricas actuales de todas las colas de RabbitMQ
    /// </summary>
    Task<RabbitMqMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Modelo de métricas de RabbitMQ
/// </summary>
public class RabbitMqMetrics
{
    public int TotalMessagesPublished { get; set; }
    public int TotalMessagesConsumed { get; set; }
    public int TotalQueueDepth { get; set; }
    public int TotalDLQMessages { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public int ConsumerErrors { get; set; }
    public Dictionary<string, QueueMetrics> QueueMetrics { get; set; } = new();
}

/// <summary>
/// Métricas por cola individual
/// </summary>
public class QueueMetrics
{
    public string QueueName { get; set; } = string.Empty;
    public int Messages { get; set; }
    public int MessagesReady { get; set; }
    public int MessagesUnacknowledged { get; set; }
    public int Consumers { get; set; }
    public long MessagesPublished { get; set; }
    public long MessagesConsumed { get; set; }
    public bool IsDLQ { get; set; }
}

