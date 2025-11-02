using GateKeep.Api.Application.Auditoria;
using GateKeep.Api.Domain.Entities;
using MongoDB.Bson;

namespace GateKeep.Api.Application.Auditoria;

public class EventoHistoricoService : IEventoHistoricoService
{
    private readonly IEventoHistoricoRepository _repository;
    private readonly TimeSpan _retentionDays;

    public EventoHistoricoService(
        IEventoHistoricoRepository repository,
        IConfiguration configuration)
    {
        _repository = repository;
        var retentionDays = configuration.GetValue<int>("auditoria:retentionDays", 365);
        _retentionDays = TimeSpan.FromDays(retentionDays);
    }

    public async Task RegistrarAccesoAsync(
        long usuarioId,
        long espacioId,
        string resultado,
        string puntoControl,
        Dictionary<string, object>? datosAdicionales = null)
    {
        var evento = CrearEvento(
            "Acceso",
            usuarioId,
            espacioId,
            resultado,
            puntoControl,
            datosAdicionales);

        await _repository.CrearAsync(evento);
    }

    public async Task RegistrarRechazoAsync(
        long usuarioId,
        long? espacioId,
        string razon,
        string puntoControl,
        Dictionary<string, object>? datosAdicionales = null)
    {
        var datos = datosAdicionales ?? new Dictionary<string, object>();
        datos["razon"] = razon;

        var evento = CrearEvento(
            "Rechazo",
            usuarioId,
            espacioId,
            "Rechazado",
            puntoControl,
            datos);

        await _repository.CrearAsync(evento);
    }

    public async Task RegistrarNotificacionAsync(
        long usuarioId,
        string tipoNotificacion,
        string resultado,
        Dictionary<string, object>? datosAdicionales = null)
    {
        var datos = datosAdicionales ?? new Dictionary<string, object>();
        datos["tipoNotificacion"] = tipoNotificacion;

        var evento = CrearEvento(
            "Notificacion",
            usuarioId,
            null,
            resultado,
            null,
            datos);

        await _repository.CrearAsync(evento);
    }

    public async Task RegistrarCambioRolAsync(
        long usuarioId,
        string rolAnterior,
        string rolNuevo,
        long? modificadoPor,
        Dictionary<string, object>? datosAdicionales = null)
    {
        var datos = datosAdicionales ?? new Dictionary<string, object>();
        datos["rolAnterior"] = rolAnterior;
        datos["rolNuevo"] = rolNuevo;
        if (modificadoPor.HasValue)
            datos["modificadoPor"] = modificadoPor.Value;

        var evento = CrearEvento(
            "CambioRol",
            usuarioId,
            null,
            "Modificado",
            null,
            datos);

        await _repository.CrearAsync(evento);
    }

    private EventoHistorico CrearEvento(
        string tipoEvento,
        long usuarioId,
        long? espacioId,
        string resultado,
        string? puntoControl,
        Dictionary<string, object>? datos)
    {
        var evento = new EventoHistorico
        {
            TipoEvento = tipoEvento,
            Fecha = DateTime.UtcNow,
            UsuarioId = usuarioId,
            EspacioId = espacioId,
            Resultado = resultado,
            PuntoControl = puntoControl,
            CreatedAt = DateTime.UtcNow,
            ExpireAt = DateTime.UtcNow.Add(_retentionDays)
        };

        if (datos != null && datos.Any())
        {
            evento.Datos = new BsonDocument(
                datos.Select(d => new BsonElement(d.Key, BsonValue.Create(d.Value)))
            );
        }

        return evento;
    }
}

