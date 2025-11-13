namespace GateKeep.Api.Infrastructure.Observability;

/// <summary>
/// Implementación del proveedor de CorrelationId usando AsyncLocal para mantener el contexto
/// </summary>
public class CorrelationIdProvider : ICorrelationIdProvider
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    public string? CorrelationId
    {
        get => _correlationId.Value;
        set => _correlationId.Value = value;
    }
}

