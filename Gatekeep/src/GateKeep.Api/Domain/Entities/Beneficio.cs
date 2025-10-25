using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Domain.Entities;

public sealed record Beneficio
(
    long Id,
    TipoBeneficio Tipo,
    bool Vigencia,
    DateTime FechaDeVencimiento,
    int Cupos
);


