using GateKeep.Api.Contracts.Beneficios;
using GateKeep.Api.Infrastructure.Caching;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Application.Beneficios;

/// <summary>
/// Servicio de beneficios con caching de Redis
/// </summary>
public sealed class CachedBeneficioService : ICachedBeneficioService
{
    private readonly IBeneficioService _beneficioService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedBeneficioService> _logger;

    public CachedBeneficioService(
        IBeneficioService beneficioService,
        ICacheService cacheService,
        ILogger<CachedBeneficioService> logger)
    {
        _beneficioService = beneficioService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IEnumerable<BeneficioDto>> ObtenerTodosAsync()
    {
        try
        {
            var cacheKey = CacheKeys.AllBeneficios;
            
            // Intentar obtener del cache
            var cachedBeneficios = await _cacheService.GetAsync<List<BeneficioDto>>(cacheKey);
            if (cachedBeneficios is not null)
            {
                return cachedBeneficios;
            }

            // Si no está en cache, obtener de la BD
            var beneficios = (await _beneficioService.ObtenerTodosAsync()).ToList();
            
            // Guardar en cache (puede fallar, pero no es crítico)
            try
            {
                await _cacheService.SetAsync(cacheKey, beneficios, CacheKeys.TTL.Beneficios);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo guardar en cache, pero los datos se obtuvieron de la BD correctamente");
            }
            
            return beneficios;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener beneficios. Intentando obtener directamente de la BD sin cache...");
            // Fallback directo a la BD si todo falla
            return await _beneficioService.ObtenerTodosAsync();
        }
    }

    public async Task<BeneficioDto?> ObtenerPorIdAsync(long id)
    {
        var cacheKey = CacheKeys.BeneficioById(id);
        
        // Intentar obtener del cache
        var cachedBeneficio = await _cacheService.GetAsync<BeneficioDto>(cacheKey);
        if (cachedBeneficio is not null)
        {
            return cachedBeneficio;
        }

        // Si no está en cache, obtener de la BD
        var beneficio = await _beneficioService.ObtenerPorIdAsync(id);
        if (beneficio is null)
        {
            return null;
        }
        
        // Guardar en cache
        await _cacheService.SetAsync(cacheKey, beneficio, CacheKeys.TTL.Beneficios);
        
        return beneficio;
    }

    public async Task<IEnumerable<BeneficioDto>> ObtenerBeneficiosVigentesAsync()
    {
        var cacheKey = CacheKeys.BeneficiosVigentes;
        
        // Intentar obtener del cache
        var cachedBeneficios = await _cacheService.GetAsync<List<BeneficioDto>>(cacheKey);
        if (cachedBeneficios is not null)
        {
            return cachedBeneficios;
        }

        // Si no está en cache, obtener de la BD
        var todosBeneficios = await _beneficioService.ObtenerTodosAsync();
        var beneficiosVigentes = todosBeneficios
            .Where(b => b.FechaDeVencimiento > DateTime.UtcNow)
            .ToList();
        
        // Guardar en cache con TTL corto (datos más dinámicos)
        await _cacheService.SetAsync(cacheKey, beneficiosVigentes, CacheKeys.TTL.Beneficios);
        
        return beneficiosVigentes;
    }

    public async Task<BeneficioDto> CrearAsync(CrearBeneficioRequest request)
    {
        // Crear el beneficio
        var beneficio = await _beneficioService.CrearAsync(request);
        
        // Invalidar el cache de todos los beneficios y vigentes
        await InvalidarCacheBeneficios();
        
        return beneficio;
    }

    public async Task<BeneficioDto> ActualizarAsync(long id, ActualizarBeneficioRequest request)
    {
        // Actualizar el beneficio
        var beneficio = await _beneficioService.ActualizarAsync(id, request);
        
        // Invalidar el cache de todos los beneficios y el específico
        await InvalidarCacheBeneficios();
        await _cacheService.RemoveAsync(CacheKeys.BeneficioById(id));
        
        return beneficio;
    }

    public async Task EliminarAsync(long id)
    {
        // Eliminar el beneficio
        await _beneficioService.EliminarAsync(id);
        
        // Invalidar el cache de todos los beneficios y el específico
        await InvalidarCacheBeneficios();
        await _cacheService.RemoveAsync(CacheKeys.BeneficioById(id));
    }

    private async Task InvalidarCacheBeneficios()
    {
        await _cacheService.RemoveAsync(CacheKeys.AllBeneficios);
        await _cacheService.RemoveAsync(CacheKeys.BeneficiosVigentes);
    }
}
