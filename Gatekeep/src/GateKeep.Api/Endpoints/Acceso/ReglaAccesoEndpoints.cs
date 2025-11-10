using GateKeep.Api.Application.Acceso;
using GateKeep.Api.Contracts.Acceso;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Acceso;

public static class ReglaAccesoEndpoints
{
    public static IEndpointRouteBuilder MapReglaAccesoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reglas-acceso")
            .WithTags("ReglasAcceso")
            .WithOpenApi();

        // GET /api/reglas-acceso - Solo funcionarios y administradores pueden ver reglas
        group.MapGet("/", async (IReglaAccesoService service) =>
        {
            var reglas = await service.ObtenerTodasAsync();
            return Results.Ok(reglas);
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("GetReglasAcceso")
        .WithSummary("Obtener todas las reglas de acceso")
        .Produces<IEnumerable<ReglaAccesoDto>>(200)
        .Produces(401)
        .Produces(403);

        // GET /api/reglas-acceso/{id} - Solo funcionarios y administradores pueden ver una regla específica
        group.MapGet("/{id:long}", async (long id, IReglaAccesoService service) =>
        {
            var regla = await service.ObtenerPorIdAsync(id);
            return regla is not null ? Results.Ok(regla) : Results.NotFound();
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("GetReglaAccesoById")
        .WithSummary("Obtener regla de acceso por ID")
        .Produces<ReglaAccesoDto>(200)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        // GET /api/reglas-acceso/espacio/{espacioId} - Solo funcionarios y administradores pueden ver regla por espacio
        group.MapGet("/espacio/{espacioId:long}", async (long espacioId, IReglaAccesoService service) =>
        {
            var regla = await service.ObtenerPorEspacioIdAsync(espacioId);
            return regla is not null ? Results.Ok(regla) : Results.NotFound();
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("GetReglaAccesoPorEspacioId")
        .WithSummary("Obtener regla de acceso por espacio ID")
        .Produces<ReglaAccesoDto>(200)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        // POST /api/reglas-acceso - Solo administradores pueden crear reglas
        group.MapPost("/", async (
            [FromBody] CrearReglaAccesoRequest request,
            ClaimsPrincipal user,
            IReglaAccesoService service) =>
        {
            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            try
            {
                var regla = await service.CrearAsync(request);
                return Results.CreatedAtRoute("GetReglaAccesoById", new { id = regla.Id }, regla);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Error al crear la regla de acceso: {ex.Message}" });
            }
        })
        .RequireAuthorization("AdminOnly")
        .WithName("CreateReglaAcceso")
        .WithSummary("Crear nueva regla de acceso")
        .Produces<ReglaAccesoDto>(201)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // PUT /api/reglas-acceso/{id} - Solo administradores pueden actualizar reglas
        group.MapPut("/{id:long}", async (
            long id,
            [FromBody] ActualizarReglaAccesoRequest request,
            ClaimsPrincipal user,
            IReglaAccesoService service) =>
        {
            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            
            var regla = await service.ObtenerPorIdAsync(id);
            if (regla == null)
                return Results.NotFound();
            
            try
            {
                var reglaActualizada = await service.ActualizarAsync(id, request);
                return Results.Ok(reglaActualizada);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Error al actualizar la regla de acceso: {ex.Message}" });
            }
        })
        .RequireAuthorization("AdminOnly")
        .WithName("UpdateReglaAcceso")
        .WithSummary("Actualizar regla de acceso")
        .Produces<ReglaAccesoDto>(200)
        .Produces(404)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // DELETE /api/reglas-acceso/{id} - Solo administradores pueden eliminar reglas
        group.MapDelete("/{id:long}", async (long id, IReglaAccesoService service) =>
        {
            try
            {
                await service.EliminarAsync(id);
                return Results.Ok(new { message = $"ReglaAcceso {id} eliminada correctamente" });
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Error al eliminar la regla de acceso: {ex.Message}" });
            }
        })
        .RequireAuthorization("AdminOnly")
        .WithName("DeleteReglaAcceso")
        .WithSummary("Eliminar regla de acceso")
        .Produces<string>(200)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        return app;
    }
}

