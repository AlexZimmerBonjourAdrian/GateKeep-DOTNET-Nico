namespace GateKeep.Api.Contracts.Anuncios;

public record AnuncioDto
{
    public long Id { get; init; }
    public required string Nombre { get; init; }
    public DateTime Fecha { get; init; }
    public bool Activo { get; init; }
}

