using System.Diagnostics;

namespace GateKeep.Api.Infrastructure.Observability;

/// <summary>
/// Middleware que captura o genera un CorrelationId para cada request
/// y lo propaga a través del sistema usando Activity y headers HTTP
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdProvider correlationIdProvider)
    {
        // Intentar obtener el CorrelationId del header de la request
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();

        // Si no existe, generar uno nuevo
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Almacenar en el provider para acceso desde servicios
        if (correlationIdProvider is CorrelationIdProvider provider)
        {
            provider.CorrelationId = correlationId;
        }

        // Agregar al Activity actual de OpenTelemetry
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("correlation_id", correlationId);
        }

        // Agregar a los items del HttpContext para acceso rápido
        context.Items["CorrelationId"] = correlationId;

        // Agregar al response header para que el cliente pueda rastrear
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        _logger.LogDebug("Request iniciada con CorrelationId: {CorrelationId}", correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            _logger.LogDebug("Request finalizada con CorrelationId: {CorrelationId}", correlationId);
        }
    }
}

/// <summary>
/// Extensiones para registrar el middleware de CorrelationId
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}

