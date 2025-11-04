namespace GateKeep.Api.Application.Acceso;

public enum TipoErrorAcceso
{
    None,
    UsuarioNoExiste,
    UsuarioInvalido,
    EspacioNoExiste,
    EspacioInactivo,
    ReglasNoConfiguradas,
    FueraDeHorario,
    FueraDeVigencia,
    RolNoPermitido
}

public record ResultadoValidacionAcceso
{
    public bool Permitido { get; init; }
    public string? Razon { get; init; }
    public TipoErrorAcceso TipoError { get; init; }
    public Dictionary<string, object>? DetallesAdicionales { get; init; }
}

public interface IAccesoService
{
    Task<ResultadoValidacionAcceso> ValidarAccesoAsync(
        long usuarioId,
        long espacioId,
        string puntoControl);
}

