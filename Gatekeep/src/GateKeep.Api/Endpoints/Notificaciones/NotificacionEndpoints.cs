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

        // Endpoints para notificaciones de usuario (combinando PostgreSQL y MongoDB)
        var usuarioGroup = app.MapGroup("/api/usuarios/{usuarioId}/notificaciones")
            .WithTags("Notificaciones")
            .WithOpenApi();

        // GET - Obtener todas las notificaciones de un usuario
        usuarioGroup.MapGet("/", async (
            long usuarioId,
            INotificacionUsuarioService notificacionUsuarioService) =>
        {
            try
            {
                var notificaciones = await notificacionUsuarioService.ObtenerNotificacionesPorUsuarioAsync(usuarioId);
                return Results.Ok(notificaciones);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        })
        .RequireAuthorization("AllUsers")
        .WithName("ObtenerNotificacionesPorUsuario")
        .WithSummary("Obtener todas las notificaciones de un usuario")
        .Produces<IEnumerable<NotificacionCompletaDto>>(200)
        .Produces(404)
        .Produces(401);

        // GET - Obtener una notificación específica de un usuario
        usuarioGroup.MapGet("/{notificacionId}", async (
            long usuarioId,
            string notificacionId,
            INotificacionUsuarioService notificacionUsuarioService) =>
        {
            try
            {
                var notificacion = await notificacionUsuarioService.ObtenerNotificacionCompletaAsync(usuarioId, notificacionId);
                return notificacion != null ? Results.Ok(notificacion) : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        })
        .RequireAuthorization("AllUsers")
        .WithName("ObtenerNotificacionPorUsuario")
        .WithSummary("Obtener una notificación específica de un usuario")
        .Produces<NotificacionCompletaDto>(200)
        .Produces(404)
        .Produces(401);

        // PUT - Marcar notificación como leída
        usuarioGroup.MapPut("/{notificacionId}/leer", async (
            long usuarioId,
            string notificacionId,
            INotificacionUsuarioService notificacionUsuarioService) =>
        {
            try
            {
                var resultado = await notificacionUsuarioService.MarcarComoLeidaAsync(usuarioId, notificacionId);
                return resultado ? Results.Ok(new { success = true }) : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization("AllUsers")
        .WithName("MarcarNotificacionComoLeida")
        .WithSummary("Marcar una notificación como leída")
        .Produces(200)
        .Produces(404)
        .Produces(400)
        .Produces(401);

        // GET - Contar notificaciones no leídas
        usuarioGroup.MapGet("/no-leidas/count", async (
            long usuarioId,
            INotificacionUsuarioService notificacionUsuarioService) =>
        {
            try
            {
                var count = await notificacionUsuarioService.ContarNoLeidasAsync(usuarioId);
                return Results.Ok(new { count });
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        })
        .RequireAuthorization("AllUsers")
        .WithName("ContarNotificacionesNoLeidas")
        .WithSummary("Contar notificaciones no leídas de un usuario")
        .Produces(200)
        .Produces(404)
        .Produces(401);
    }
}
