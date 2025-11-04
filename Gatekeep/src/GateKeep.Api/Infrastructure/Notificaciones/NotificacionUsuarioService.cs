using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Application.Usuarios;
using GateKeep.Api.Contracts.Notificaciones;
using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Infrastructure.Notificaciones;

public sealed class NotificacionUsuarioService : INotificacionUsuarioService
{
    private readonly INotificacionUsuarioRepository _notificacionUsuarioRepository;
    private readonly INotificacionRepository _notificacionRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public NotificacionUsuarioService(
        INotificacionUsuarioRepository notificacionUsuarioRepository,
        INotificacionRepository notificacionRepository,
        IUsuarioRepository usuarioRepository)
    {
        _notificacionUsuarioRepository = notificacionUsuarioRepository;
        _notificacionRepository = notificacionRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<IEnumerable<NotificacionCompletaDto>> ObtenerNotificacionesPorUsuarioAsync(long usuarioId)
    {
        // 1. Validar que el usuario existe en PostgreSQL
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario is null)
        {
            throw new InvalidOperationException($"El usuario con ID {usuarioId} no existe");
        }

        // 2. Consultar relaciones usuario-notificación en MongoDB
        var notificacionesUsuario = await _notificacionUsuarioRepository.ObtenerPorUsuarioAsync(usuarioId);

        // 3. Agrupar IDs de notificaciones para evitar N+1
        var notificacionIds = notificacionesUsuario
            .Select(nu => nu.NotificacionId)
            .Distinct()
            .ToList();

        // 4. Consultar todas las notificaciones en una sola consulta optimizada
        var notificacionesDict = new Dictionary<string, Notificacion>();
        foreach (var id in notificacionIds)
        {
            var notificacion = await _notificacionRepository.ObtenerPorIdAsync(id);
            if (notificacion is not null)
            {
                notificacionesDict[id] = notificacion;
            }
        }

        // 5. Combinar datos de ambas bases en memoria
        var resultados = notificacionesUsuario
            .Where(nu => notificacionesDict.ContainsKey(nu.NotificacionId))
            .Select(nu =>
            {
                var notificacion = notificacionesDict[nu.NotificacionId];
                return new NotificacionCompletaDto
                {
                    UsuarioId = usuario.Id,
                    UsuarioNombre = $"{usuario.Nombre} {usuario.Apellido}",
                    UsuarioEmail = usuario.Email,
                    NotificacionId = notificacion.Id,
                    Mensaje = notificacion.Mensaje,
                    Tipo = notificacion.Tipo,
                    FechaEnvio = notificacion.FechaEnvio,
                    Leido = nu.Leido,
                    FechaLectura = nu.FechaLectura,
                    CreatedAt = nu.CreatedAt
                };
            })
            .OrderByDescending(n => n.FechaEnvio)
            .ToList();

        return resultados;
    }

    public async Task<NotificacionCompletaDto?> ObtenerNotificacionCompletaAsync(long usuarioId, string notificacionId)
    {
        // 1. Validar que el usuario existe en PostgreSQL
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario is null)
        {
            return null;
        }

        // 2. Consultar relación usuario-notificación en MongoDB
        var notificacionUsuario = await _notificacionUsuarioRepository
            .ObtenerPorUsuarioYNotificacionAsync(usuarioId, notificacionId);
        
        if (notificacionUsuario is null)
        {
            return null;
        }

        // 3. Consultar notificación completa en MongoDB
        var notificacion = await _notificacionRepository.ObtenerPorIdAsync(notificacionId);
        if (notificacion is null)
        {
            return null;
        }

        // 4. Combinar datos
        return new NotificacionCompletaDto
        {
            UsuarioId = usuario.Id,
            UsuarioNombre = $"{usuario.Nombre} {usuario.Apellido}",
            UsuarioEmail = usuario.Email,
            NotificacionId = notificacion.Id,
            Mensaje = notificacion.Mensaje,
            Tipo = notificacion.Tipo,
            FechaEnvio = notificacion.FechaEnvio,
            Leido = notificacionUsuario.Leido,
            FechaLectura = notificacionUsuario.FechaLectura,
            CreatedAt = notificacionUsuario.CreatedAt
        };
    }

    public async Task<bool> MarcarComoLeidaAsync(long usuarioId, string notificacionId)
    {
        return await _notificacionUsuarioRepository.MarcarComoLeidaAsync(usuarioId, notificacionId);
    }

    public async Task<int> ContarNoLeidasAsync(long usuarioId)
    {
        return await _notificacionUsuarioRepository.ContarNoLeidasAsync(usuarioId);
    }
}

