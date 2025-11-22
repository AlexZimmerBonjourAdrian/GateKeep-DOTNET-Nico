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
            if (string.IsNullOrWhiteSpace(request.Mensaje))
            {
                return Results.BadRequest(new { error = "El mensaje de la notificación es requerido y no puede estar vacío" });
            }

            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            try
            {
                var notificacion = await service.CrearNotificacionAsync(request.Mensaje, request.Tipo, userId);
                return Results.Created($"/api/notificaciones/{notificacion.Id}", notificacion);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Error al crear la notificación: {ex.Message}" });
            }
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("CrearNotificacion")
        .WithSummary("Crear nueva notificación")
        .Produces<NotificacionDto>(201)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // GET - Todos los usuarios autenticados pueden ver notificaciones
        group.MapGet("/", async (INotificacionService service, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("NotificacionEndpoints");
            try
            {
                var notificaciones = await service.ObtenerTodasAsync();
                return Results.Ok(notificaciones);
            }
            catch (MongoDB.Driver.MongoConnectionException ex)
            {
                logger.LogError(ex, "Error de conexión a MongoDB al obtener notificaciones");
                return Results.Problem(
                    detail: "Error de conexión a la base de datos de notificaciones. Por favor, intente más tarde.",
                    statusCode: 503);
            }
            catch (MongoDB.Driver.MongoException ex)
            {
                logger.LogError(ex, "Error de MongoDB al obtener notificaciones");
                return Results.Problem(
                    detail: $"Error al acceder a las notificaciones: {ex.Message}",
                    statusCode: 500);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado al obtener notificaciones");
                return Results.Problem(
                    detail: $"Error inesperado: {ex.Message}",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("AllUsers")
        .WithName("ObtenerTodasNotificaciones")
        .WithSummary("Obtener todas las notificaciones")
        .Produces<IEnumerable<NotificacionDto>>(200)
        .Produces(401)
        .Produces(403)
        .Produces(500)
        .Produces(503);

        // GET - Todos los usuarios autenticados pueden ver una notificación específica
        group.MapGet("/{id}", async (string id, INotificacionService service) =>
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Results.BadRequest(new { error = "El ID de notificación es requerido y no puede estar vacío" });
            }

            var notificacion = await service.ObtenerPorIdAsync(id);
            if (notificacion == null)
            {
                return Results.NotFound(new { error = $"La notificación con ID {id} no existe en el sistema" });
            }
            
            return Results.Ok(notificacion);
        })
        .RequireAuthorization("AllUsers")
        .WithName("ObtenerNotificacionPorId")
        .WithSummary("Obtener notificación por ID")
        .Produces<NotificacionDto>(200)
        .Produces(400)
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
            if (string.IsNullOrWhiteSpace(id))
            {
                return Results.BadRequest(new { error = "El ID de notificación es requerido y no puede estar vacío" });
            }

            if (string.IsNullOrWhiteSpace(request.Mensaje))
            {
                return Results.BadRequest(new { error = "El mensaje de la notificación es requerido y no puede estar vacío" });
            }

            try
            {
                var notificacionActualizada = await service.ActualizarNotificacionAsync(id, request.Mensaje, request.Tipo);
                if (notificacionActualizada == null)
                {
                    return Results.NotFound(new { error = $"La notificación con ID {id} no existe en el sistema" });
                }

                return Results.Ok(notificacionActualizada);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Error al actualizar la notificación: {ex.Message}" });
            }
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("ActualizarNotificacion")
        .WithSummary("Actualizar notificación")
        .Produces<NotificacionDto>(200)
        .Produces(400)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        // DELETE - Solo administradores pueden eliminar notificaciones
        group.MapDelete("/{id}", async (string id, INotificacionService service) =>
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Results.BadRequest(new { error = "El ID de notificación es requerido y no puede estar vacío" });
            }

            var resultado = await service.EliminarNotificacionAsync(id);
            if (!resultado)
            {
                return Results.NotFound(new { error = $"La notificación con ID {id} no existe en el sistema" });
            }
            
            return Results.NoContent();
        })
        .RequireAuthorization("AdminOnly")
        .WithName("EliminarNotificacion")
        .WithSummary("Eliminar notificación")
        .Produces(204)
        .Produces(400)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        // Endpoints para notificaciones de usuario (combinando PostgreSQL y MongoDB)
        var usuarioGroup = app.MapGroup("/api/usuarios/{usuarioId}/notificaciones")
            .WithTags("Notificaciones")
            .WithOpenApi();

        // GET - Obtener todas las notificaciones de un usuario
        usuarioGroup.MapGet("/", async (
            [FromServices] INotificacionUsuarioService notificacionUsuarioService,
            [FromRoute] long usuarioId) =>
        {
            if (usuarioId <= 0)
            {
                return Results.BadRequest(new { error = "El ID de usuario debe ser mayor a 0" });
            }

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
        .Produces(400)
        .Produces(404)
        .Produces(401);

        // GET - Obtener una notificación específica de un usuario
        usuarioGroup.MapGet("/{notificacionId}", async (
            [FromServices] INotificacionUsuarioService notificacionUsuarioService,
            [FromRoute] long usuarioId,
            string notificacionId) =>
        {
            if (usuarioId <= 0)
            {
                return Results.BadRequest(new { error = "El ID de usuario debe ser mayor a 0" });
            }

            if (string.IsNullOrWhiteSpace(notificacionId))
            {
                return Results.BadRequest(new { error = "El ID de notificación es requerido y no puede estar vacío" });
            }

            try
            {
                var notificacion = await notificacionUsuarioService.ObtenerNotificacionCompletaAsync(usuarioId, notificacionId);
                if (notificacion == null)
                {
                    return Results.NotFound(new 
                    { 
                        error = $"La notificación con ID {notificacionId} no existe o no está asignada al usuario {usuarioId}" 
                    });
                }
                
                return Results.Ok(notificacion);
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
        .Produces(400)
        .Produces(404)
        .Produces(401);

        // PUT - Marcar notificación como leída
        usuarioGroup.MapPut("/{notificacionId}/leer", async (
            [FromServices] INotificacionUsuarioService notificacionUsuarioService,
            [FromRoute] long usuarioId,
            string notificacionId) =>
        {
            if (usuarioId <= 0)
            {
                return Results.BadRequest(new { error = "El ID de usuario debe ser mayor a 0" });
            }

            if (string.IsNullOrWhiteSpace(notificacionId))
            {
                return Results.BadRequest(new { error = "El ID de notificación es requerido y no puede estar vacío" });
            }

            try
            {
                var resultado = await notificacionUsuarioService.MarcarComoLeidaAsync(usuarioId, notificacionId);
                if (!resultado)
                {
                    return Results.NotFound(new 
                    { 
                        error = $"La notificación con ID {notificacionId} no existe o no está asignada al usuario {usuarioId}" 
                    });
                }
                
                return Results.Ok(new { success = true, message = "Notificación marcada como leída exitosamente" });
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
            [FromServices] INotificacionUsuarioService notificacionUsuarioService,
            [FromRoute] long usuarioId) =>
        {
            if (usuarioId <= 0)
            {
                return Results.BadRequest(new { error = "El ID de usuario debe ser mayor a 0" });
            }

            try
            {
                var count = await notificacionUsuarioService.ContarNoLeidasAsync(usuarioId);
                return Results.Ok(new { count, usuarioId });
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
        .Produces(400)
        .Produces(404)
        .Produces(401);
    }
}
