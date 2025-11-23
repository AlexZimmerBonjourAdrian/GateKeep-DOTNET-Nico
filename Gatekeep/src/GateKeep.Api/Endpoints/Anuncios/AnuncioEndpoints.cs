using GateKeep.Api.Application.Anuncios;
using GateKeep.Api.Contracts.Anuncios;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Anuncios;

public static class AnuncioEndpoints
{
    public static IEndpointRouteBuilder MapAnuncioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/anuncios")
            .WithTags("Anuncios")
            .WithOpenApi();

        // GET /api/anuncios - Público, cualquier usuario puede ver anuncios
        group.MapGet("/", async (IAnuncioService service) =>
        {
            var anuncios = await service.ObtenerTodosAsync();
            return Results.Ok(anuncios);
        })
        .WithName("GetAnuncios")
        .WithSummary("Obtener todos los anuncios")
        .Produces<IEnumerable<AnuncioDto>>(200);

        // GET /api/anuncios/{id} - Público, cualquier usuario puede ver un anuncio específico
        group.MapGet("/{id:long}", async (long id, IAnuncioService service) =>
        {
            var anuncio = await service.ObtenerPorIdAsync(id);
            return anuncio is not null ? Results.Ok(anuncio) : Results.NotFound();
        })
        .WithName("GetAnuncioById")
        .WithSummary("Obtener anuncio por ID")
        .Produces<AnuncioDto>(200)
        .Produces(404);

        // POST /api/anuncios - Solo funcionarios y administradores pueden crear anuncios
        group.MapPost("/", async (
            [FromBody] CrearAnuncioRequest request,
            ClaimsPrincipal user,
            IAnuncioService service) =>
        {
            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            try
            {
                var anuncio = await service.CrearAsync(request);
                return Results.CreatedAtRoute("GetAnuncioById", new { id = anuncio.Id }, anuncio);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Error al crear el anuncio: {ex.Message}" });
            }
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("CreateAnuncio")
        .WithSummary("Crear nuevo anuncio")
        .Produces<AnuncioDto>(201)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // PUT /api/anuncios/{id} - Solo funcionarios y administradores pueden actualizar anuncios
        group.MapPut("/{id:long}", async (
            long id,
            [FromBody] ActualizarAnuncioRequest request,
            ClaimsPrincipal user,
            IAnuncioService service) =>
        {
            try
            {
                var anuncioActualizado = await service.ActualizarAsync(id, request);
                return Results.Ok(anuncioActualizado);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Error al actualizar el anuncio: {ex.Message}" });
            }
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("UpdateAnuncio")
        .WithSummary("Actualizar anuncio")
        .Produces<AnuncioDto>(200)
        .Produces(404)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // DELETE /api/anuncios/{id} - Solo administradores pueden eliminar anuncios (soft delete)
        group.MapDelete("/{id:long}", async (long id, IAnuncioService service) =>
        {
            try
            {
                await service.EliminarAsync(id);
                return Results.Ok(new { message = $"Anuncio {id} desactivado correctamente" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Error al desactivar el anuncio: {ex.Message}" });
            }
        })
        .RequireAuthorization("AdminOnly")
        .WithName("DeleteAnuncio")
        .WithSummary("Desactivar anuncio (soft delete)")
        .Produces<string>(200)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        return app;
    }
}

