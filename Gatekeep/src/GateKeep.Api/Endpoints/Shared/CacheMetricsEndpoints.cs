using GateKeep.Api.Infrastructure.Caching;
using Microsoft.AspNetCore.Authorization;

namespace GateKeep.Api.Endpoints.Shared;

/// <summary>
/// Endpoints para métricas del cache
/// </summary>
public static class CacheMetricsEndpoints
{
    public static void MapCacheMetricsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cache-metrics")
            .WithTags("Cache Metrics")
            .WithOpenApi();

        // GET /api/cache-metrics
        group.MapGet("/", [Authorize(Policy = "AdminOnly")] (ICacheMetricsService metricsService) =>
        {
            var metrics = metricsService.GetMetrics();
            return Results.Ok(new
            {
                totalHits = metrics.TotalHits,
                totalMisses = metrics.TotalMisses,
                totalInvalidations = metrics.TotalInvalidations,
                totalRequests = metrics.TotalRequests,
                hitRate = Math.Round(metrics.HitRate, 2),
                lastResetTime = metrics.LastResetTime,
                hitsByKey = metrics.HitsByKey,
                missesByKey = metrics.MissesByKey
            });
        })
        .WithName("GetCacheMetrics")
        .WithDescription("Obtiene las métricas actuales del cache Redis (solo administradores)")
        .Produces<object>(StatusCodes.Status200OK);

        // POST /api/cache-metrics/reset
        group.MapPost("/reset", [Authorize(Policy = "AdminOnly")] (ICacheMetricsService metricsService) =>
        {
            metricsService.ResetMetrics();
            return Results.Ok(new { message = "Métricas de cache reiniciadas exitosamente" });
        })
        .WithName("ResetCacheMetrics")
        .WithDescription("Reinicia las métricas del cache (solo administradores)")
        .Produces<object>(StatusCodes.Status200OK);

        // GET /api/cache-metrics/health
        group.MapGet("/health", [AllowAnonymous] (ICacheMetricsService metricsService) =>
        {
            var metrics = metricsService.GetMetrics();
            return Results.Ok(new
            {
                status = "healthy",
                hitRate = Math.Round(metrics.HitRate, 2),
                totalRequests = metrics.TotalRequests
            });
        })
        .WithName("GetCacheHealth")
        .WithDescription("Verifica el estado del sistema de cache")
        .Produces<object>(StatusCodes.Status200OK);
    }
}

