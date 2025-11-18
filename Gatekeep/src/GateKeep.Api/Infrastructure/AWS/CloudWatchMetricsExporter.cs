using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using GateKeep.Api.Infrastructure.Caching;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Infrastructure.AWS;

/// <summary>
/// Exportador de métricas de cache Redis a AWS CloudWatch
/// Se ejecuta como un HostedService en background enviando métricas cada N segundos
/// </summary>
public class CloudWatchMetricsExporter : BackgroundService, ICloudWatchMetricsExporter
{
    private readonly IAmazonCloudWatch _cloudWatchClient;
    private readonly ICacheMetricsService _cacheMetricsService;
    private readonly ILogger<CloudWatchMetricsExporter> _logger;
    private readonly string _namespace = "GateKeep/Redis";
    private readonly int _intervalSeconds = 30; // Intervalo de exportación en segundos
    private bool _isRunning = false;

    public bool IsRunning => _isRunning;

    public CloudWatchMetricsExporter(
        IAmazonCloudWatch cloudWatchClient,
        ICacheMetricsService cacheMetricsService,
        ILogger<CloudWatchMetricsExporter> logger)
    {
        _cloudWatchClient = cloudWatchClient;
        _cacheMetricsService = cacheMetricsService;
        _logger = logger;
    }

    /// <summary>
    /// Inicia el servicio de exportación de métricas
    /// </summary>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _isRunning = true;
        _logger.LogInformation("CloudWatch Metrics Exporter iniciado - intervalo: {IntervalSeconds} segundos", _intervalSeconds);
        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Detiene el servicio de exportación de métricas
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _isRunning = false;
        _logger.LogInformation("CloudWatch Metrics Exporter detenido");
        await base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Método ejecutado en background que exporta métricas periódicamente
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExportCacheMetricsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exportando métricas a CloudWatch");
            }

            // Espera el intervalo antes de exportar de nuevo
            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }
    }

    /// <summary>
    /// Exporta las métricas de cache actuales a CloudWatch
    /// </summary>
    public async Task ExportCacheMetricsAsync()
    {
        try
        {
            var metrics = _cacheMetricsService.GetMetrics();
            var timestamp = DateTime.UtcNow;

            // Crear lista de datos métricos para enviar a CloudWatch
            var metricDataList = new List<MetricDatum>
            {
                // Métrica: Total de hits de cache
                new MetricDatum
                {
                    MetricName = "CacheHitsTotal",
                    Value = metrics.TotalHits,
                    Unit = StandardUnit.Count,
                    Timestamp = timestamp,
                    Dimensions = new List<Dimension>
                    {
                        new Dimension { Name = "Environment", Value = GetEnvironment() },
                        new Dimension { Name = "Service", Value = "GateKeepAPI" }
                    }
                },

                // Métrica: Total de misses de cache
                new MetricDatum
                {
                    MetricName = "CacheMissesTotal",
                    Value = metrics.TotalMisses,
                    Unit = StandardUnit.Count,
                    Timestamp = timestamp,
                    Dimensions = new List<Dimension>
                    {
                        new Dimension { Name = "Environment", Value = GetEnvironment() },
                        new Dimension { Name = "Service", Value = "GateKeepAPI" }
                    }
                },

                // Métrica: Total de invalidaciones de cache
                new MetricDatum
                {
                    MetricName = "CacheInvalidationsTotal",
                    Value = metrics.TotalInvalidations,
                    Unit = StandardUnit.Count,
                    Timestamp = timestamp,
                    Dimensions = new List<Dimension>
                    {
                        new Dimension { Name = "Environment", Value = GetEnvironment() },
                        new Dimension { Name = "Service", Value = "GateKeepAPI" }
                    }
                },

                // Métrica: Hit Rate (porcentaje)
                new MetricDatum
                {
                    MetricName = "CacheHitRate",
                    Value = metrics.HitRate * 100, // Convertir a porcentaje
                    Unit = StandardUnit.Percent,
                    Timestamp = timestamp,
                    Dimensions = new List<Dimension>
                    {
                        new Dimension { Name = "Environment", Value = GetEnvironment() },
                        new Dimension { Name = "Service", Value = "GateKeepAPI" }
                    }
                }
            };

            // Agregar métricas específicas por clave si existen
            if (metrics.HitsByKey.Count > 0)
            {
                foreach (var (key, hits) in metrics.HitsByKey.Take(5)) // Limitar a 5 claves más activas
                {
                    metricDataList.Add(new MetricDatum
                    {
                        MetricName = "CacheHitsByKey",
                        Value = hits,
                        Unit = StandardUnit.Count,
                        Timestamp = timestamp,
                        Dimensions = new List<Dimension>
                        {
                            new Dimension { Name = "Environment", Value = GetEnvironment() },
                            new Dimension { Name = "Service", Value = "GateKeepAPI" },
                            new Dimension { Name = "CacheKey", Value = key }
                        }
                    });
                }
            }

            if (metrics.MissesByKey.Count > 0)
            {
                foreach (var (key, misses) in metrics.MissesByKey.Take(5)) // Limitar a 5 claves más activas
                {
                    metricDataList.Add(new MetricDatum
                    {
                        MetricName = "CacheMissesByKey",
                        Value = misses,
                        Unit = StandardUnit.Count,
                        Timestamp = timestamp,
                        Dimensions = new List<Dimension>
                        {
                            new Dimension { Name = "Environment", Value = GetEnvironment() },
                            new Dimension { Name = "Service", Value = "GateKeepAPI" },
                            new Dimension { Name = "CacheKey", Value = key }
                        }
                    });
                }
            }

            // Enviar métricas a CloudWatch en lotes (máximo 20 por solicitud)
            for (int i = 0; i < metricDataList.Count; i += 20)
            {
                var batch = metricDataList.Skip(i).Take(20).ToList();
                var request = new PutMetricDataRequest
                {
                    Namespace = _namespace,
                    MetricData = batch
                };

                await _cloudWatchClient.PutMetricDataAsync(request);

                _logger.LogDebug(
                    "Métricas exportadas a CloudWatch - Batch {BatchNumber}: {Count} métricas",
                    (i / 20) + 1,
                    batch.Count);
            }

            _logger.LogDebug(
                "Métricas de cache exportadas exitosamente a CloudWatch - Hits: {Hits}, Misses: {Misses}, HitRate: {HitRate:P}",
                metrics.TotalHits,
                metrics.TotalMisses,
                metrics.HitRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar métricas de cache a CloudWatch");
            throw;
        }
    }

    /// <summary>
    /// Obtiene el nombre del ambiente actual
    /// </summary>
    private string GetEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    }
}
