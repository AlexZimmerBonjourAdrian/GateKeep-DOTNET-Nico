namespace GateKeep.Api.Infrastructure.Messaging;

/// <summary>
/// Configuración para RabbitMQ
/// </summary>
public class RabbitMqSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    
    // Configuración de reintentos
    public int RetryCount { get; set; } = 3;
    public int InitialIntervalSeconds { get; set; } = 5;
    public int IntervalIncrementSeconds { get; set; } = 10;
    
    // Configuración de DLQ (Dead Letter Queue)
    public bool EnableDLQ { get; set; } = true;
    public int DLQMessageTTLDays { get; set; } = 7;
    
    public string GetConnectionString()
    {
        return $"amqp://{Username}:{Password}@{Host}:{Port}{VirtualHost}";
    }
}
