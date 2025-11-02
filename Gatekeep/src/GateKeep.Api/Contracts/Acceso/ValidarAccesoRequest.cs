namespace GateKeep.Api.Contracts.Acceso;

public record ValidarAccesoRequest
{
    public long UsuarioId { get; init; }
    public long EspacioId { get; init; }
    public required string PuntoControl { get; init; }
}

