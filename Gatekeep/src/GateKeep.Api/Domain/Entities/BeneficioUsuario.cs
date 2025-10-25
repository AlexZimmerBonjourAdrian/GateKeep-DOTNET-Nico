namespace GateKeep.Api.Domain.Entities;

public sealed record BeneficioUsuario
(
    long UsuarioId,
    long BeneficioId,
    bool EstadoCanje
);


