using GateKeep.Api.Application.Beneficios;
using GateKeep.Api.Contracts.Beneficios;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Beneficios;

public static class BeneficioEndpoints
{
    public static IEndpointRouteBuilder MapBeneficioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/beneficios").WithTags("Beneficios");

        // GET /beneficios - Todos los usuarios autenticados pueden ver beneficios
        group.MapGet("/", async (ICachedBeneficioService service) =>
        {
            var beneficios = await service.ObtenerTodosAsync();
            return Results.Ok(beneficios);
        })
        .RequireAuthorization("AllUsers")
        .WithName("GetBeneficios")
        .WithSummary("Obtener todos los beneficios")
        .Produces<IEnumerable<BeneficioDto>>(200)
        .Produces(401)
        .Produces(403);

        // GET /beneficios/{id} - Todos los usuarios autenticados pueden ver un beneficio específico
        group.MapGet("/{id:long}", async (long id, ICachedBeneficioService service) =>
        {
            var beneficio = await service.ObtenerPorIdAsync(id);
            return beneficio is not null ? Results.Ok(beneficio) : Results.NotFound();
        })
        .RequireAuthorization("AllUsers")
        .WithName("GetBeneficioById")
        .WithSummary("Obtener beneficio por ID")
        .Produces<BeneficioDto>(200)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        // GET /beneficios/vigentes - Todos los usuarios autenticados pueden ver beneficios vigentes
        group.MapGet("/vigentes", async (ICachedBeneficioService service) =>
        {
            var beneficios = await service.ObtenerBeneficiosVigentesAsync();
            return Results.Ok(beneficios);
        })
        .RequireAuthorization("AllUsers")
        .WithName("GetBeneficiosVigentes")
        .WithSummary("Obtener beneficios vigentes (con cache)")
        .Produces<IEnumerable<BeneficioDto>>(200)
        .Produces(401)
        .Produces(403);

        // POST /beneficios - Solo funcionarios y administradores pueden crear beneficios
        group.MapPost("/", async (CrearBeneficioRequest request, ClaimsPrincipal user, ICachedBeneficioService service) =>
        {
            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Agregar información del creador al request si es necesario
            // request.CreadoPor = userId; // Descomenta si el request tiene esta propiedad
            
            var beneficio = await service.CrearAsync(request);
            return Results.CreatedAtRoute("GetBeneficioById", new { id = beneficio.Id }, beneficio);
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("CreateBeneficio")
        .WithSummary("Crear nuevo beneficio")
        .Produces<BeneficioDto>(201)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // PUT /beneficios/{id} - Solo funcionarios y administradores pueden actualizar beneficios
        group.MapPut("/{id:long}", async (long id, ActualizarBeneficioRequest request, ClaimsPrincipal user, ICachedBeneficioService service) =>
        {
            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            
            // Verificar si el usuario puede editar este beneficio
            var beneficio = await service.ObtenerPorIdAsync(id);
            if (beneficio == null)
                return Results.NotFound();
                
            // Solo administradores pueden editar cualquier beneficio
            // Si quieres que el creador también pueda editar, descomenta la siguiente línea:
            // if (beneficio.CreadoPor != userId && userRole != "Admin")
            //     return Results.Forbid("No tienes permisos para editar este beneficio");
            
            try
            {
                var beneficioActualizado = await service.ActualizarAsync(id, request);
                return Results.Ok(beneficioActualizado);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("UpdateBeneficio")
        .WithSummary("Actualizar beneficio")
        .Produces<BeneficioDto>(200)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        // DELETE /beneficios/{id} - Solo administradores pueden eliminar beneficios
        group.MapDelete("/{id:long}", async (long id, ICachedBeneficioService service) =>
        {
            await service.EliminarAsync(id);
            return Results.Ok($"Beneficio {id} marcado como eliminado (borrado lógico)");
        })
        .RequireAuthorization("AdminOnly")
        .WithName("DeleteBeneficio")
        .WithSummary("Eliminar beneficio (borrado lógico)")
        .Produces<string>(200)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        // Endpoints para asignación de beneficios a usuarios
        var usuarioBeneficioGroup = app.MapGroup("/api/usuarios/{usuarioId:long}/beneficios")
            .WithTags("Beneficios")
            .WithOpenApi();

        // GET /api/usuarios/{usuarioId}/beneficios - Obtener beneficios de un usuario
        usuarioBeneficioGroup.MapGet("/", async (
            long usuarioId,
            [FromServices] IBeneficioUsuarioService beneficioUsuarioService) =>
        {
            var beneficios = await beneficioUsuarioService.ObtenerBeneficiosPorUsuarioAsync(usuarioId);
            return Results.Ok(beneficios);
        })
        .RequireAuthorization("AllUsers")
        .WithName("GetBeneficiosPorUsuario")
        .WithSummary("Obtener beneficios de un usuario")
        .Produces<IEnumerable<BeneficioUsuarioDto>>(200)
        .Produces(401)
        .Produces(403);

        // GET /api/usuarios/{usuarioId}/beneficios/canjeados - Obtener historial de beneficios canjeados
        usuarioBeneficioGroup.MapGet("/canjeados", async (
            long usuarioId,
            [FromServices] IBeneficioUsuarioService beneficioUsuarioService) =>
        {
            var beneficios = await beneficioUsuarioService.ObtenerBeneficiosCanjeadosPorUsuarioAsync(usuarioId);
            return Results.Ok(beneficios);
        })
        .RequireAuthorization("AllUsers")
        .WithName("GetBeneficiosCanjeados")
        .WithSummary("Obtener historial de beneficios canjeados por un usuario")
        .Produces<IEnumerable<BeneficioUsuarioDto>>(200)
        .Produces(401)
        .Produces(403);

        // POST /api/usuarios/{usuarioId}/beneficios/{beneficioId} - Asignar beneficio a usuario
        usuarioBeneficioGroup.MapPost("/{beneficioId:long}", async (
            long usuarioId,
            long beneficioId,
            [FromServices] IBeneficioUsuarioService beneficioUsuarioService) =>
        {
            try
            {
                var resultado = await beneficioUsuarioService.AsignarBeneficioAsync(usuarioId, beneficioId);
                return Results.Created($"/api/usuarios/{usuarioId}/beneficios/{beneficioId}", resultado);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("AsignarBeneficio")
        .WithSummary("Asignar un beneficio a un usuario (dispara evento BeneficioAsignado)")
        .Produces<BeneficioUsuarioDto>(201)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // DELETE /api/usuarios/{usuarioId}/beneficios/{beneficioId} - Desasignar beneficio de usuario
        usuarioBeneficioGroup.MapDelete("/{beneficioId:long}", async (
            long usuarioId,
            long beneficioId,
            [FromServices] IBeneficioUsuarioService beneficioUsuarioService) =>
        {
            try
            {
                await beneficioUsuarioService.DesasignarBeneficioAsync(usuarioId, beneficioId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("DesasignarBeneficio")
        .WithSummary("Desasignar un beneficio de un usuario (dispara evento BeneficioDesasignado)")
        .Produces(204)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // PATCH /api/usuarios/{usuarioId}/beneficios/{beneficioId}/canjear - Canjear beneficio
        usuarioBeneficioGroup.MapPatch("/{beneficioId:long}/canjear", async (
            long usuarioId,
            long beneficioId,
            [FromBody] CanjearBeneficioRequest request,
            [FromServices] IBeneficioUsuarioService beneficioUsuarioService) =>
        {
            try
            {
                var resultado = await beneficioUsuarioService.CanjearBeneficioAsync(usuarioId, beneficioId, request.PuntoControl);
                return Results.Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization("AllUsers")
        .WithName("CanjearBeneficio")
        .WithSummary("Canjear un beneficio (dispara evento BeneficioCanjeado con notificación)")
        .Produces<BeneficioUsuarioDto>(200)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        return app;
    }
}
