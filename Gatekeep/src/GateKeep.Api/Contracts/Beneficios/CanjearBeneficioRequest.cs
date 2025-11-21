namespace GateKeep.Api.Contracts.Beneficios;

public record CanjearBeneficioRequest
{
    public string PuntoControl { get; init; } = string.Empty;
}
