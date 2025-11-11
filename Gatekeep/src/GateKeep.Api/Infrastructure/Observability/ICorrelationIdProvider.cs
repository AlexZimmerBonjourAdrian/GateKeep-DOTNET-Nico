namespace GateKeep.Api.Infrastructure.Observability;

/// <summary>
/// Proveedor de CorrelationId para rastreo de requests a través del sistema
/// </summary>
public interface ICorrelationIdProvider
{
    /// <summary>
    /// Obtiene el CorrelationId de la request actual
    /// </summary>
    string? CorrelationId { get; }
}

