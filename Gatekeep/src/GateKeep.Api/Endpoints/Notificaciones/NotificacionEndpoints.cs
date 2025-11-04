using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Contracts.Notificaciones;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Notificaciones;

public static class NotificacionEndpoints
{
    public static void MapNotificacionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notificaciones")
            .WithTags("Notificaciones")
            .WithOpenApi();

        // POST - Solo funcionarios y administradores pueden crear notificaciones
        group.MapPost("/", async (
            [FromBody] CrearNotificacionRequest request,
            ClaimsPrincipal user,
            INotificacionService service) =>
        {
            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var notificacion = await service.CrearNotificacionAsync(request.Mensaje, request.Tipo);
            return Results.Created($"/api/notificaciones/{notificacion.Id}", notificacion);
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("CrearNotificacion")
        .WithSummary("Crear nueva notificación")
        .Produces<NotificacionDto>(201)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // GET - Todos los usuarios autenticados pueden ver notificaciones
        group.MapGet("/", async (INotificacionService service) =>
        {
            var notificaciones = await service.ObtenerTodasAsync();
            return Results.Ok(notificaciones);
        })
        .RequireAuthorization("AllUsers")
        .WithName("ObtenerTodasNotificaciones")
        .WithSummary("Obtener todas las notificaciones")
        .Produces<IEnumerable<NotificacionDto>>(200)
        .Produces(401)
        .Produces(403);

        // GET - Todos los usuarios autenticados pueden ver una notificación específica
        group.MapGet("/{id}", async (string id, INotificacionService service) =>
        {
            var notificacion = await service.ObtenerPorIdAsync(id);
            return notificacion != null ? Results.Ok(notificacion) : Results.NotFound();
        })
        .RequireAuthorization("AllUsers")
        .WithName("ObtenerNotificacionPorId")
        .WithSummary("Obtener notificación por ID")
        .Produces<NotificacionDto>(200)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        // PUT - Solo funcionarios y administradores pueden actualizar notificaciones
        group.MapPut("/{id}", async (
            string id,
            [FromBody] CrearNotificacionRequest request,
            ClaimsPrincipal user,
            INotificacionService service) =>
        {
            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
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
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("ActualizarNotificacion")
        .WithSummary("Actualizar notificación")
        .Produces<NotificacionDto>(200)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        // DELETE - Solo administradores pueden eliminar notificaciones
        group.MapDelete("/{id}", async (string id, INotificacionService service) =>
        {
            var resultado = await service.EliminarNotificacionAsync(id);
            return resultado ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization("AdminOnly")
        .WithName("EliminarNotificacion")
        .WithSummary("Eliminar notificación")
        .Produces(204)
        .Produces(404)
        .Produces(401)
        .Produces(403);
    }
}
