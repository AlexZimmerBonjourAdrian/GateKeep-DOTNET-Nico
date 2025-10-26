using GateKeep.Api.Application.Beneficios;
using GateKeep.Api.Contracts.Beneficios;

namespace GateKeep.Api.Endpoints.Beneficios;

public static class BeneficioEndpoints
{
    public static IEndpointRouteBuilder MapBeneficioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/beneficios").WithTags("Beneficios");

        // GET /beneficios - Obtener todos los beneficios
        group.MapGet("/", async (IBeneficioService service) =>
        {
            var beneficios = await service.ObtenerTodosAsync();
            return Results.Ok(beneficios);
        })
        .WithName("GetBeneficios")
        .WithSummary("Obtener todos los beneficios")
        .Produces<IEnumerable<BeneficioDto>>(200);

        // GET /beneficios/{id} - Obtener beneficio por ID
        group.MapGet("/{id:long}", async (long id, IBeneficioService service) =>
        {
            var beneficio = await service.ObtenerPorIdAsync(id);
            return beneficio is not null ? Results.Ok(beneficio) : Results.NotFound();
        })
        .WithName("GetBeneficioById")
        .WithSummary("Obtener beneficio por ID")
        .Produces<BeneficioDto>(200)
        .Produces(404);

        // POST /beneficios - Crear nuevo beneficio
        group.MapPost("/", async (CrearBeneficioRequest request, IBeneficioService service) =>
        {
            var beneficio = await service.CrearAsync(request);
            return Results.CreatedAtRoute("GetBeneficioById", new { id = beneficio.Id }, beneficio);
        })
        .WithName("CreateBeneficio")
        .WithSummary("Crear nuevo beneficio")
        .Produces<BeneficioDto>(201)
        .Produces(400);

        // PUT /beneficios/{id} - Actualizar beneficio
        group.MapPut("/{id:long}", async (long id, ActualizarBeneficioRequest request, IBeneficioService service) =>
        {
            try
            {
                var beneficio = await service.ActualizarAsync(id, request);
                return Results.Ok(beneficio);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        })
        .WithName("UpdateBeneficio")
        .WithSummary("Actualizar beneficio")
        .Produces<BeneficioDto>(200)
        .Produces(404);

        // DELETE /beneficios/{id} - Eliminar beneficio (borrado lógico)
        group.MapDelete("/{id:long}", async (long id, IBeneficioService service) =>
        {
            await service.EliminarAsync(id);
            return Results.Ok($"Beneficio {id} marcado como eliminado (borrado lógico)");
        })
        .WithName("DeleteBeneficio")
        .WithSummary("Eliminar beneficio (borrado lógico)")
        .Produces<string>(200)
        .Produces(404);

        return app;
    }
}
