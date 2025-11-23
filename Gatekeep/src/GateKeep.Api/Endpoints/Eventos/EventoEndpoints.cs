using GateKeep.Api.Application.Eventos;
using GateKeep.Api.Contracts.Eventos;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Eventos;

public static class EventoEndpoints
{
    public static IEndpointRouteBuilder MapEventoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/eventos")
            .WithTags("Eventos")
            .WithOpenApi();

        // GET /api/eventos - Público, cualquier usuario puede ver eventos
        group.MapGet("/", async (IEventoService service) =>
        {
            var eventos = await service.ObtenerTodosAsync();
            return Results.Ok(eventos);
        })
        .WithName("GetEventos")
        .WithSummary("Obtener todos los eventos")
        .Produces<IEnumerable<EventoDto>>(200);

        // GET /api/eventos/{id} - Público, cualquier usuario puede ver un evento específico
        group.MapGet("/{id:long}", async (long id, IEventoService service) =>
        {
            var evento = await service.ObtenerPorIdAsync(id);
            return evento is not null ? Results.Ok(evento) : Results.NotFound();
        })
        .WithName("GetEventoById")
        .WithSummary("Obtener evento por ID")
        .Produces<EventoDto>(200)
        .Produces(404);

        // POST /api/eventos - Solo funcionarios y administradores pueden crear eventos
        group.MapPost("/", async (
            [FromBody] CrearEventoRequest request,
            ClaimsPrincipal user,
            IEventoService service) =>
        {
            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            try
            {
                var evento = await service.CrearAsync(request);
                return Results.CreatedAtRoute("GetEventoById", new { id = evento.Id }, evento);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Error al crear el evento: {ex.Message}" });
            }
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("CreateEvento")
        .WithSummary("Crear nuevo evento")
        .Produces<EventoDto>(201)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // PUT /api/eventos/{id} - Solo funcionarios y administradores pueden actualizar eventos
        group.MapPut("/{id:long}", async (
            long id,
            [FromBody] ActualizarEventoRequest request,
            ClaimsPrincipal user,
            IEventoService service) =>
        {
            try
            {
                var eventoActualizado = await service.ActualizarAsync(id, request);
                return Results.Ok(eventoActualizado);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Error al actualizar el evento: {ex.Message}" });
            }
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("UpdateEvento")
        .WithSummary("Actualizar evento")
        .Produces<EventoDto>(200)
        .Produces(404)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // DELETE /api/eventos/{id} - Solo administradores pueden eliminar eventos (soft delete)
        group.MapDelete("/{id:long}", async (long id, IEventoService service) =>
        {
            try
            {
                await service.EliminarAsync(id);
                return Results.Ok(new { message = $"Evento {id} desactivado correctamente" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Error al desactivar el evento: {ex.Message}" });
            }
        })
        .RequireAuthorization("AdminOnly")
        .WithName("DeleteEvento")
        .WithSummary("Desactivar evento (soft delete)")
        .Produces<string>(200)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        return app;
    }
}

