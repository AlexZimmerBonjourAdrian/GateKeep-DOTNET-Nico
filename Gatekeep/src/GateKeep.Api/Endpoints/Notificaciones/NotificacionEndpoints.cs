using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Contracts.Notificaciones;
using Microsoft.AspNetCore.Mvc;

namespace GateKeep.Api.Endpoints.Notificaciones;

public static class NotificacionEndpoints
{
    public static void MapNotificacionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notificaciones")
            .WithTags("Notificaciones")
            .WithOpenApi();

        // POST - Crear notificación
        group.MapPost("/", async (
            [FromBody] CrearNotificacionRequest request,
            INotificacionService service) =>
        {
            var notificacion = await service.CrearNotificacionAsync(request.Mensaje, request.Tipo);
            return Results.Created($"/api/notificaciones/{notificacion.Id}", notificacion);
        })
        .WithName("CrearNotificacion")
        .WithSummary("Crear nueva notificación");

        // GET - Obtener todas las notificaciones
        group.MapGet("/", async (INotificacionService service) =>
        {
            var notificaciones = await service.ObtenerTodasAsync();
            return Results.Ok(notificaciones);
        })
        .WithName("ObtenerTodasNotificaciones")
        .WithSummary("Obtener todas las notificaciones");

        // GET - Obtener notificación por ID
        group.MapGet("/{id}", async (string id, INotificacionService service) =>
        {
            var notificacion = await service.ObtenerPorIdAsync(id);
            return notificacion != null ? Results.Ok(notificacion) : Results.NotFound();
        })
        .WithName("ObtenerNotificacionPorId")
        .WithSummary("Obtener notificación por ID");

        // PUT - Actualizar notificación
        group.MapPut("/{id}", async (
            string id,
            [FromBody] CrearNotificacionRequest request,
            INotificacionService service) =>
        {
            var notificacionExistente = await service.ObtenerPorIdAsync(id);
            if (notificacionExistente == null)
                return Results.NotFound();

            // Crear nueva notificación con los datos actualizados
            var notificacionActualizada = new NotificacionDto
            {
                Id = id,
                Mensaje = request.Mensaje,
                Tipo = request.Tipo,
                FechaEnvio = notificacionExistente.FechaEnvio,
                Activa = notificacionExistente.Activa
            };

            return Results.Ok(notificacionActualizada);
        })
        .WithName("ActualizarNotificacion")
        .WithSummary("Actualizar notificación");

        // DELETE - Eliminar notificación
        group.MapDelete("/{id}", async (string id, INotificacionService service) =>
        {
            var resultado = await service.EliminarNotificacionAsync(id);
            return resultado ? Results.NoContent() : Results.NotFound();
        })
        .WithName("EliminarNotificacion")
        .WithSummary("Eliminar notificación");
    }
}
