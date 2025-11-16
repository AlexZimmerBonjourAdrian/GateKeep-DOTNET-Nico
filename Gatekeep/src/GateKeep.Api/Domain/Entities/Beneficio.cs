using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Domain.Entities;

public sealed record Beneficio
{
    public long Id { get; init; }
    public TipoBeneficio Tipo { get; init; }
    public bool Vigencia { get; init; }
    public DateTime FechaDeVencimiento { get; init; }
    public int Cupos { get; init; }
}


