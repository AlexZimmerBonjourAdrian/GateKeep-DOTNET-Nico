namespace GateKeep.Api.Infrastructure.AWS;

/// <summary>
/// Interfaz para exportar métricas de RabbitMQ a CloudWatch
/// </summary>
public interface IRabbitMqCloudWatchExporter
{
    /// <summary>
    /// Inicia el exportador de métricas
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Detiene el exportador de métricas
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Exporta las métricas de RabbitMQ actuales a CloudWatch
    /// </summary>
    Task ExportRabbitMqMetricsAsync();

    /// <summary>
    /// Obtiene el estado del exportador
    /// </summary>
    bool IsRunning { get; }
}

