namespace GateKeep.Api.Contracts.Beneficios;

public record BeneficioUsuarioDto
{
    public long UsuarioId { get; init; }
    public long BeneficioId { get; init; }
    public bool EstadoCanje { get; init; }
}


