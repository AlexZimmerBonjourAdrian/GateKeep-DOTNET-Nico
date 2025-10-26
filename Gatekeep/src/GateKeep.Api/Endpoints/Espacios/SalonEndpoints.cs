using GateKeep.Api.Contracts.Espacios;
using Microsoft.AspNetCore.Mvc;

namespace GateKeep.Api.Endpoints.Espacios;

public static class SalonEndpoints
{
    public static IEndpointRouteBuilder MapSalonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/espacios/salones").WithTags("Salones");

        // GET /espacios/salones - Obtener todos los salones
        group.MapGet("/", () => Results.Ok("Lista de salones"))
            .WithName("GetSalones")
            .WithSummary("Obtener todos los salones")
            .Produces<string>(200);

        // GET /espacios/salones/{id} - Obtener salón por ID
        group.MapGet("/{id:long}", (long id) => Results.Ok($"Salón {id}"))
            .WithName("GetSalonById")
            .WithSummary("Obtener salón por ID")
            .Produces<string>(200);

        // POST /espacios/salones - Crear nuevo salón
        group.MapPost("/", (CrearSalonRequest request) => Results.Created($"/espacios/salones/1", "Salón creado"))
            .WithName("CreateSalon")
            .WithSummary("Crear nuevo salón")
            .Produces<string>(201)
            .Produces(400);

        // PUT /espacios/salones/{id} - Actualizar salón
        group.MapPut("/{id:long}", (long id, CrearSalonRequest request) => Results.Ok($"Salón {id} actualizado"))
            .WithName("UpdateSalon")
            .WithSummary("Actualizar salón")
            .Produces<string>(200)
            .Produces(404);

        // DELETE /espacios/salones/{id} - Eliminar salón (borrado lógico)
        group.MapDelete("/{id:long}", (long id) => Results.Ok($"Salón {id} marcado como eliminado (borrado lógico)"))
            .WithName("DeleteSalon")
            .WithSummary("Eliminar salón (borrado lógico)")
            .Produces<string>(200)
            .Produces(404);

        return app;
    }
}


