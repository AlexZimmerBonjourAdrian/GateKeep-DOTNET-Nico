using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Infrastructure.Messaging;

/// <summary>
/// Servicio para obtener métricas de RabbitMQ desde la API de Management
/// </summary>
public class RabbitMqMetricsService : IRabbitMqMetricsService
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqMetricsService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public RabbitMqMetricsService(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqMetricsService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateHttpClient()
    {
        var client = _httpClientFactory.CreateClient("RabbitMQ");
        
        // Configurar autenticación básica para RabbitMQ Management API
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_settings.Username}:{_settings.Password}"));
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        
        return client;
    }

    public async Task<RabbitMqMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = new RabbitMqMetrics();
            var protocol = _settings.UseHttps ? "https" : "http";
            var managementPort = _settings.ManagementPort > 0 ? _settings.ManagementPort : (_settings.UseHttps ? 443 : 15672);
            var baseUrl = $"{protocol}://{_settings.Host}:{managementPort}/api";
            var vhost = Uri.EscapeDataString(_settings.VirtualHost);

            _logger.LogDebug(
                "Obteniendo métricas de RabbitMQ - URL: {BaseUrl}, Protocol: {Protocol}, Port: {Port}",
                baseUrl, protocol, managementPort);

            // Obtener información de todas las colas
            using var httpClient = CreateHttpClient();
            var queuesUrl = $"{baseUrl}/queues/{vhost}";
            var queuesResponse = await httpClient.GetAsync(queuesUrl, cancellationToken);

            if (!queuesResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "No se pudo obtener métricas de RabbitMQ. Status: {StatusCode}",
                    queuesResponse.StatusCode);
                return metrics;
            }

            var queuesJson = await queuesResponse.Content.ReadAsStringAsync(cancellationToken);
            var queues = JsonSerializer.Deserialize<List<RabbitMqQueueInfo>>(queuesJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<RabbitMqQueueInfo>();

            foreach (var queue in queues)
            {
                var isDLQ = queue.Name.Contains("-dlq", StringComparison.OrdinalIgnoreCase);
                
                var queueMetrics = new QueueMetrics
                {
                    QueueName = queue.Name,
                    Messages = queue.Messages ?? 0,
                    MessagesReady = queue.MessagesReady ?? 0,
                    MessagesUnacknowledged = queue.MessagesUnacknowledged ?? 0,
                    Consumers = queue.Consumers ?? 0,
                    MessagesPublished = queue.MessageStats?.Publish ?? 0,
                    MessagesConsumed = queue.MessageStats?.DeliverGet ?? 0,
                    IsDLQ = isDLQ
                };

                metrics.QueueMetrics[queue.Name] = queueMetrics;

                // Acumular totales
                metrics.TotalMessagesPublished += (int)queueMetrics.MessagesPublished;
                metrics.TotalMessagesConsumed += (int)queueMetrics.MessagesConsumed;
                metrics.TotalQueueDepth += queueMetrics.Messages;

                if (isDLQ)
                {
                    metrics.TotalDLQMessages += queueMetrics.Messages;
                }
            }

            _logger.LogDebug(
                "Métricas de RabbitMQ obtenidas - Colas: {QueueCount}, Mensajes: {TotalMessages}, DLQ: {DLQMessages}",
                metrics.QueueMetrics.Count,
                metrics.TotalQueueDepth,
                metrics.TotalDLQMessages);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo métricas de RabbitMQ");
            return new RabbitMqMetrics();
        }
    }
}

/// <summary>
/// Modelo para deserializar información de colas desde RabbitMQ Management API
/// </summary>
internal class RabbitMqQueueInfo
{
    public string Name { get; set; } = string.Empty;
    public int? Messages { get; set; }
    public int? MessagesReady { get; set; }
    public int? MessagesUnacknowledged { get; set; }
    public int? Consumers { get; set; }
    public RabbitMqMessageStats? MessageStats { get; set; }
}

internal class RabbitMqMessageStats
{
    public long Publish { get; set; }
    public long DeliverGet { get; set; }
}

