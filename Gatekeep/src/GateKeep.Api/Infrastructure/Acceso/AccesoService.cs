using GateKeep.Api.Application.Acceso;
using GateKeep.Api.Application.Auditoria;
using GateKeep.Api.Application.Usuarios;
using GateKeep.Api.Application.Espacios;
using GateKeep.Api.Application.Events;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Domain.Enums;
using GateKeep.Api.Infrastructure.Persistence;
using GateKeep.Api.Infrastructure.Observability;
using GateKeep.Api.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace GateKeep.Api.Infrastructure.Acceso;

public class AccesoService : IAccesoService
{
    private readonly IReglaAccesoRepository _reglaAccesoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEspacioRepository _espacioRepository;
    private readonly GateKeepDbContext _context;
    private readonly IEventoHistoricoService? _eventoHistoricoService;
    private readonly IEventPublisher? _eventPublisher;
    private readonly IEventBusPublisher? _eventBusPublisher;
    private readonly IObservabilityService _observabilityService;
    private readonly ILogger<AccesoService> _logger;

    public AccesoService(
        IReglaAccesoRepository reglaAccesoRepository,
        IUsuarioRepository usuarioRepository,
        IEspacioRepository espacioRepository,
        GateKeepDbContext context,
        IObservabilityService observabilityService,
        ILogger<AccesoService> logger,
        IEventoHistoricoService? eventoHistoricoService = null,
        IEventPublisher? eventPublisher = null,
        IEventBusPublisher? eventBusPublisher = null)
    {
        _reglaAccesoRepository = reglaAccesoRepository;
        _usuarioRepository = usuarioRepository;
        _espacioRepository = espacioRepository;
        _context = context;
        _observabilityService = observabilityService;
        _logger = logger;
        _eventoHistoricoService = eventoHistoricoService;
        _eventPublisher = eventPublisher;
        _eventBusPublisher = eventBusPublisher;
    }

    public async Task<ResultadoValidacionAcceso> ValidarAccesoAsync(
        long usuarioId,
        long espacioId,
        string puntoControl)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null)
        {
            return new ResultadoValidacionAcceso
            {
                Permitido = false,
                Razon = $"El usuario con ID {usuarioId} no existe en el sistema",
                TipoError = TipoErrorAcceso.UsuarioNoExiste,
                DetallesAdicionales = new Dictionary<string, object>
                {
                    { "UsuarioId", usuarioId }
                }
            };
        }

        if (usuario.Credencial != TipoCredencial.Vigente)
        {
            await RegistrarRechazoAsync(usuarioId, espacioId, puntoControl, "Credencial no vigente");
            return new ResultadoValidacionAcceso
            {
                Permitido = false,
                Razon = $"La credencial del usuario {usuarioId} no está vigente. Estado actual: {usuario.Credencial}",
                TipoError = TipoErrorAcceso.UsuarioInvalido,
                DetallesAdicionales = new Dictionary<string, object>
                {
                    { "UsuarioId", usuarioId },
                    { "CredencialActual", usuario.Credencial.ToString() }
                }
            };
        }

        var espacio = await _espacioRepository.ObtenerPorIdAsync(espacioId);
        if (espacio == null)
        {
            return new ResultadoValidacionAcceso
            {
                Permitido = false,
                Razon = $"El espacio con ID {espacioId} no existe en el sistema",
                TipoError = TipoErrorAcceso.EspacioNoExiste,
                DetallesAdicionales = new Dictionary<string, object>
                {
                    { "EspacioId", espacioId }
                }
            };
        }

        if (!espacio.Activo)
        {
            await RegistrarRechazoAsync(usuarioId, espacioId, puntoControl, "Espacio inactivo");
            return new ResultadoValidacionAcceso
            {
                Permitido = false,
                Razon = $"El espacio {espacio.Nombre} (ID: {espacioId}) está inactivo",
                TipoError = TipoErrorAcceso.EspacioInactivo,
                DetallesAdicionales = new Dictionary<string, object>
                {
                    { "EspacioId", espacioId },
                    { "EspacioNombre", espacio.Nombre }
                }
            };
        }

        var regla = await _reglaAccesoRepository.ObtenerPorEspacioIdAsync(espacioId);
        if (regla == null)
        {
            await RegistrarRechazoAsync(usuarioId, espacioId, puntoControl, "No hay reglas de acceso configuradas");
            return new ResultadoValidacionAcceso
            {
                Permitido = false,
                Razon = $"No hay reglas de acceso configuradas para el espacio {espacio.Nombre} (ID: {espacioId})",
                TipoError = TipoErrorAcceso.ReglasNoConfiguradas,
                DetallesAdicionales = new Dictionary<string, object>
                {
                    { "EspacioId", espacioId },
                    { "EspacioNombre", espacio.Nombre }
                }
            };
        }

        var ahora = DateTime.UtcNow;
        var horaActual = ahora.TimeOfDay;
        var horarioApertura = regla.HorarioApertura.TimeOfDay;
        var horarioCierre = regla.HorarioCierre.TimeOfDay;

        if (ahora < regla.VigenciaApertura || ahora > regla.VigenciaCierre)
        {
            await RegistrarRechazoAsync(usuarioId, espacioId, puntoControl, $"Fuera del período de vigencia. Vigencia: {regla.VigenciaApertura:yyyy-MM-dd} a {regla.VigenciaCierre:yyyy-MM-dd}");
            return new ResultadoValidacionAcceso
            {
                Permitido = false,
                Razon = $"El acceso está fuera del período de vigencia. Vigencia permitida: del {regla.VigenciaApertura:yyyy-MM-dd} al {regla.VigenciaCierre:yyyy-MM-dd}. Fecha actual: {ahora:yyyy-MM-dd}",
                TipoError = TipoErrorAcceso.FueraDeVigencia,
                DetallesAdicionales = new Dictionary<string, object>
                {
                    { "EspacioId", espacioId },
                    { "VigenciaApertura", regla.VigenciaApertura },
                    { "VigenciaCierre", regla.VigenciaCierre },
                    { "FechaActual", ahora }
                }
            };
        }

        if (horaActual < horarioApertura || horaActual > horarioCierre)
        {
            await RegistrarRechazoAsync(usuarioId, espacioId, puntoControl, $"Fuera del horario permitido. Horario: {horarioApertura:hh\\:mm} - {horarioCierre:hh\\:mm}");
            return new ResultadoValidacionAcceso
            {
                Permitido = false,
                Razon = $"El acceso está fuera del horario permitido. Horario permitido: {horarioApertura:hh\\:mm} - {horarioCierre:hh\\:mm}. Hora actual: {horaActual:hh\\:mm}",
                TipoError = TipoErrorAcceso.FueraDeHorario,
                DetallesAdicionales = new Dictionary<string, object>
                {
                    { "EspacioId", espacioId },
                    { "HorarioApertura", horarioApertura.ToString(@"hh\:mm") },
                    { "HorarioCierre", horarioCierre.ToString(@"hh\:mm") },
                    { "HoraActual", horaActual.ToString(@"hh\:mm") }
                }
            };
        }

        if (!regla.RolesPermitidos.Contains(usuario.Rol))
        {
            await RegistrarRechazoAsync(usuarioId, espacioId, puntoControl, $"Rol no permitido. Rol actual: {usuario.Rol}. Roles permitidos: {string.Join(", ", regla.RolesPermitidos)}");
            return new ResultadoValidacionAcceso
            {
                Permitido = false,
                Razon = $"El rol del usuario ({usuario.Rol}) no tiene permiso para acceder a este espacio. Roles permitidos: {string.Join(", ", regla.RolesPermitidos)}",
                TipoError = TipoErrorAcceso.RolNoPermitido,
                DetallesAdicionales = new Dictionary<string, object>
                {
                    { "UsuarioId", usuarioId },
                    { "RolUsuario", usuario.Rol.ToString() },
                    { "EspacioId", espacioId },
                    { "RolesPermitidos", regla.RolesPermitidos.Select(r => r.ToString()).ToList() }
                }
            };
        }

        await RegistrarAccesoAsync(usuarioId, espacioId, puntoControl);
        return new ResultadoValidacionAcceso
        {
            Permitido = true,
            Razon = null,
            TipoError = TipoErrorAcceso.None
        };
    }

    private async Task RegistrarAccesoAsync(long usuarioId, long espacioId, string puntoControl)
    {
        var fecha = DateTime.UtcNow;
        var eventoAcceso = new EventoAcceso(
            Id: 0,
            Nombre: $"Acceso a espacio {espacioId}",
            Fecha: fecha,
            Resultado: "Permitido",
            PuntoControl: puntoControl,
            UsuarioId: usuarioId,
            EspacioId: espacioId
        );

        _context.EventosAcceso.Add(eventoAcceso);
        await _context.SaveChangesAsync();
        
        // Registrar métrica de acceso permitido
        _observabilityService.RecordAcceso("Permitido", true);
        _logger.LogInformation("Acceso permitido: Usuario={UsuarioId}, Espacio={EspacioId}, PuntoControl={PuntoControl}",
            usuarioId, espacioId, puntoControl);

        if (_eventoHistoricoService != null)
        {
            try
            {
                await _eventoHistoricoService.RegistrarAccesoAsync(
                    usuarioId,
                    espacioId,
                    "Permitido",
                    puntoControl);
            }
            catch
            {
            }
        }

        // Notificar a observers (Observer Pattern)
        if (_eventPublisher != null)
        {
            try
            {
                await _eventPublisher.NotifyAccesoPermitidoAsync(usuarioId, espacioId, puntoControl, fecha);
            }
            catch
            {
                // Log error pero no romper el flujo principal
            }
        }
    }

    private async Task RegistrarRechazoAsync(long usuarioId, long espacioId, string puntoControl, string razon)
    {
        var fecha = DateTime.UtcNow;
        var eventoAcceso = new EventoAcceso(
            Id: 0,
            Nombre: $"Intento de acceso a espacio {espacioId}",
            Fecha: fecha,
            Resultado: "Rechazado",
            PuntoControl: puntoControl,
            UsuarioId: usuarioId,
            EspacioId: espacioId
        );

        _context.EventosAcceso.Add(eventoAcceso);
        await _context.SaveChangesAsync();
        
        // Registrar métrica de acceso rechazado
        _observabilityService.RecordAcceso("Rechazado", false);
        _logger.LogWarning("Acceso rechazado: Usuario={UsuarioId}, Espacio={EspacioId}, PuntoControl={PuntoControl}, Razon={Razon}",
            usuarioId, espacioId, puntoControl, razon);

        if (_eventoHistoricoService != null)
        {
            try
            {
                await _eventoHistoricoService.RegistrarRechazoAsync(
                    usuarioId,
                    espacioId,
                    razon,
                    puntoControl);
            }
            catch
            {
            }
        }

        // Notificar a observers (Observer Pattern)
        if (_eventPublisher != null)
        {
            try
            {
                await _eventPublisher.NotifyAccesoRechazadoAsync(usuarioId, espacioId, razon, puntoControl, fecha);
            }
            catch
            {
                // Log error pero no romper el flujo principal
            }
        }

        // Publicar evento a RabbitMQ para procesamiento asíncrono
        if (_eventBusPublisher != null)
        {
            try
            {
                var detalles = new Dictionary<string, object>
                {
                    { "EspacioId", espacioId },
                    { "UsuarioId", usuarioId },
                    { "Timestamp", fecha }
                };

                await _eventBusPublisher.PublishAccesoRechazadoAsync(
                    usuarioId,
                    espacioId,
                    razon,
                    puntoControl,
                    "AccesoRechazado",
                    detalles);

                _logger.LogInformation(
                    "Evento AccesoRechazado publicado a RabbitMQ - Usuario: {UsuarioId}, Espacio: {EspacioId}",
                    usuarioId, espacioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error publicando evento AccesoRechazado a RabbitMQ - Usuario: {UsuarioId}, Espacio: {EspacioId}",
                    usuarioId, espacioId);
                // No re-lanzar, el rechazo ya fue registrado en BD
            }
        }
    }
}

