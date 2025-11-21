using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Contracts.Beneficios;

public record BeneficioUsuarioDto
{
    public long UsuarioId { get; init; }
    public long BeneficioId { get; init; }
    public bool EstadoCanje { get; init; }
    public DateTime? FechaCanje { get; init; }
    public TipoBeneficio? Tipo { get; init; }
    public DateTime? FechaDeVencimiento { get; init; }
    public bool? Vigencia { get; init; }
}


