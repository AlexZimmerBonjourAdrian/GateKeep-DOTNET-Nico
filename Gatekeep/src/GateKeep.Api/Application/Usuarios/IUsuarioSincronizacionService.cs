namespace GateKeep.Api.Application.Usuarios;

public interface IUsuarioSincronizacionService
{
    Task EliminarUsuarioCompletoAsync(long usuarioId);
}

