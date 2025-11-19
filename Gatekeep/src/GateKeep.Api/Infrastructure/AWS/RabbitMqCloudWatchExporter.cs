using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using GateKeep.Api.Infrastructure.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Infrastructure.AWS;

/// <summary>
/// Exportador de métricas de RabbitMQ a AWS CloudWatch
/// Se ejecuta como un HostedService en background enviando métricas cada N segundos
/// </summary>
public class RabbitMqCloudWatchExporter : BackgroundService, IRabbitMqCloudWatchExporter
{
    private readonly IAmazonCloudWatch _cloudWatchClient;
    private readonly IRabbitMqMetricsService _rabbitMqMetricsService;
    private readonly ILogger<RabbitMqCloudWatchExporter> _logger;
    private readonly string _namespace = "GateKeep/RabbitMQ";
    private readonly int _intervalSeconds = 60; // Intervalo de exportación en segundos (más largo que Redis)
    private bool _isRunning = false;

    public bool IsRunning => _isRunning;

    public RabbitMqCloudWatchExporter(
        IAmazonCloudWatch cloudWatchClient,
        IRabbitMqMetricsService rabbitMqMetricsService,
        ILogger<RabbitMqCloudWatchExporter> logger)
    {
        _cloudWatchClient = cloudWatchClient;
        _rabbitMqMetricsService = rabbitMqMetricsService;
        _logger = logger;
    }

    /// <summary>
    /// Inicia el servicio de exportación de métricas
    /// </summary>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _isRunning = true;
        _logger.LogInformation("RabbitMQ CloudWatch Metrics Exporter iniciado - intervalo: {IntervalSeconds} segundos", _intervalSeconds);
        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Detiene el servicio de exportación de métricas
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _isRunning = false;
        _logger.LogInformation("RabbitMQ CloudWatch Metrics Exporter detenido");
        await base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Método ejecutado en background que exporta métricas periódicamente
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Esperar un poco al inicio para que RabbitMQ esté listo
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExportRabbitMqMetricsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exportando métricas de RabbitMQ a CloudWatch");
            }

            // Espera el intervalo antes de exportar de nuevo
            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }
    }

    /// <summary>
    /// Exporta las métricas de RabbitMQ actuales a CloudWatch
    /// </summary>
    public async Task ExportRabbitMqMetricsAsync()
    {
        try
        {
            var metrics = await _rabbitMqMetricsService.GetMetricsAsync();
            var timestamp = DateTime.UtcNow;
            var environment = GetEnvironment();

            // Crear lista de datos métricos para enviar a CloudWatch
            var metricDataList = new List<MetricDatum>();

            // Métricas globales
            metricDataList.Add(new MetricDatum
            {
                MetricName = "TotalMessagesPublished",
                Value = metrics.TotalMessagesPublished,
                Unit = StandardUnit.Count,
                Timestamp = timestamp,
                Dimensions = new List<Dimension>
                {
                    new Dimension { Name = "Environment", Value = environment },
                    new Dimension { Name = "Service", Value = "GateKeepAPI" }
                }
            });

            metricDataList.Add(new MetricDatum
            {
                MetricName = "TotalMessagesConsumed",
                Value = metrics.TotalMessagesConsumed,
                Unit = StandardUnit.Count,
                Timestamp = timestamp,
                Dimensions = new List<Dimension>
                {
                    new Dimension { Name = "Environment", Value = environment },
                    new Dimension { Name = "Service", Value = "GateKeepAPI" }
                }
            });

            metricDataList.Add(new MetricDatum
            {
                MetricName = "TotalQueueDepth",
                Value = metrics.TotalQueueDepth,
                Unit = StandardUnit.Count,
                Timestamp = timestamp,
                Dimensions = new List<Dimension>
                {
                    new Dimension { Name = "Environment", Value = environment },
                    new Dimension { Name = "Service", Value = "GateKeepAPI" }
                }
            });

            metricDataList.Add(new MetricDatum
            {
                MetricName = "TotalDLQMessages",
                Value = metrics.TotalDLQMessages,
                Unit = StandardUnit.Count,
                Timestamp = timestamp,
                Dimensions = new List<Dimension>
                {
                    new Dimension { Name = "Environment", Value = environment },
                    new Dimension { Name = "Service", Value = "GateKeepAPI" }
                }
            });

            // Métricas por cola individual
            foreach (var (queueName, queueMetrics) in metrics.QueueMetrics)
            {
                metricDataList.Add(new MetricDatum
                {
                    MetricName = "QueueDepth",
                    Value = queueMetrics.Messages,
                    Unit = StandardUnit.Count,
                    Timestamp = timestamp,
                    Dimensions = new List<Dimension>
                    {
                        new Dimension { Name = "Environment", Value = environment },
                        new Dimension { Name = "Service", Value = "GateKeepAPI" },
                        new Dimension { Name = "QueueName", Value = queueName }
                    }
                });

                metricDataList.Add(new MetricDatum
                {
                    MetricName = "QueueMessagesReady",
                    Value = queueMetrics.MessagesReady,
                    Unit = StandardUnit.Count,
                    Timestamp = timestamp,
                    Dimensions = new List<Dimension>
                    {
                        new Dimension { Name = "Environment", Value = environment },
                        new Dimension { Name = "Service", Value = "GateKeepAPI" },
                        new Dimension { Name = "QueueName", Value = queueName }
                    }
                });

                metricDataList.Add(new MetricDatum
                {
                    MetricName = "QueueConsumers",
                    Value = queueMetrics.Consumers,
                    Unit = StandardUnit.Count,
                    Timestamp = timestamp,
                    Dimensions = new List<Dimension>
                    {
                        new Dimension { Name = "Environment", Value = environment },
                        new Dimension { Name = "Service", Value = "GateKeepAPI" },
                        new Dimension { Name = "QueueName", Value = queueName }
                    }
                });

                if (queueMetrics.MessagesPublished > 0)
                {
                    metricDataList.Add(new MetricDatum
                    {
                        MetricName = "QueueMessagesPublished",
                        Value = queueMetrics.MessagesPublished,
                        Unit = StandardUnit.Count,
                        Timestamp = timestamp,
                        Dimensions = new List<Dimension>
                        {
                            new Dimension { Name = "Environment", Value = environment },
                            new Dimension { Name = "Service", Value = "GateKeepAPI" },
                            new Dimension { Name = "QueueName", Value = queueName }
                        }
                    });
                }

                if (queueMetrics.MessagesConsumed > 0)
                {
                    metricDataList.Add(new MetricDatum
                    {
                        MetricName = "QueueMessagesConsumed",
                        Value = queueMetrics.MessagesConsumed,
                        Unit = StandardUnit.Count,
                        Timestamp = timestamp,
                        Dimensions = new List<Dimension>
                        {
                            new Dimension { Name = "Environment", Value = environment },
                            new Dimension { Name = "Service", Value = "GateKeepAPI" },
                            new Dimension { Name = "QueueName", Value = queueName }
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
                    "Métricas de RabbitMQ exportadas a CloudWatch - Batch {BatchNumber}: {Count} métricas",
                    (i / 20) + 1,
                    batch.Count);
            }

            _logger.LogDebug(
                "Métricas de RabbitMQ exportadas exitosamente a CloudWatch - Colas: {QueueCount}, Mensajes: {TotalMessages}, DLQ: {DLQMessages}",
                metrics.QueueMetrics.Count,
                metrics.TotalQueueDepth,
                metrics.TotalDLQMessages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar métricas de RabbitMQ a CloudWatch");
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

