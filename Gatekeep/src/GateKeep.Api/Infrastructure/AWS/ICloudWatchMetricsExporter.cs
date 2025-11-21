namespace GateKeep.Api.Infrastructure.AWS;

/// <summary>
/// Interfaz para exportar métricas de cache a CloudWatch
/// </summary>
public interface ICloudWatchMetricsExporter
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
    /// Exporta las métricas de cache actuales a CloudWatch
    /// </summary>
    Task ExportCacheMetricsAsync();

    /// <summary>
    /// Obtiene el estado del exportador
    /// </summary>
    bool IsRunning { get; }
}
