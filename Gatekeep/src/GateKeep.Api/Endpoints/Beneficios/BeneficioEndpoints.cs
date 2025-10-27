using GateKeep.Api.Application.Beneficios;
using GateKeep.Api.Contracts.Beneficios;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Beneficios;

public static class BeneficioEndpoints
{
    public static IEndpointRouteBuilder MapBeneficioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/beneficios").WithTags("Beneficios");

        // GET /beneficios - Todos los usuarios autenticados pueden ver beneficios
        group.MapGet("/", async (IBeneficioService service) =>
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
        group.MapGet("/{id:long}", async (long id, IBeneficioService service) =>
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

        // POST /beneficios - Solo funcionarios y administradores pueden crear beneficios
        group.MapPost("/", async (CrearBeneficioRequest request, ClaimsPrincipal user, IBeneficioService service) =>
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
        group.MapPut("/{id:long}", async (long id, ActualizarBeneficioRequest request, ClaimsPrincipal user, IBeneficioService service) =>
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
        group.MapDelete("/{id:long}", async (long id, IBeneficioService service) =>
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

        return app;
    }
}
