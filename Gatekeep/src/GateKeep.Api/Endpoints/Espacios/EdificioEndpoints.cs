using GateKeep.Api.Contracts.Espacios;
using Microsoft.AspNetCore.Mvc;

namespace GateKeep.Api.Endpoints.Espacios;

public static class EdificioEndpoints
{
    public static IEndpointRouteBuilder MapEdificioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/espacios/edificios").WithTags("Edificios");

        // GET /espacios/edificios - Obtener todos los edificios
        group.MapGet("/", () => Results.Ok("Lista de edificios"))
            .WithName("GetEdificios")
            .WithSummary("Obtener todos los edificios")
            .Produces<string>(200);

        // GET /espacios/edificios/{id} - Obtener edificio por ID
        group.MapGet("/{id:long}", (long id) => Results.Ok($"Edificio {id}"))
            .WithName("GetEdificioById")
            .WithSummary("Obtener edificio por ID")
            .Produces<string>(200);

        // POST /espacios/edificios - Crear nuevo edificio
        group.MapPost("/", (CrearEdificioRequest request) => Results.Created($"/espacios/edificios/1", "Edificio creado"))
            .WithName("CreateEdificio")
            .WithSummary("Crear nuevo edificio")
            .Produces<string>(201)
            .Produces(400);

        // PUT /espacios/edificios/{id} - Actualizar edificio
        group.MapPut("/{id:long}", (long id, CrearEdificioRequest request) => Results.Ok($"Edificio {id} actualizado"))
            .WithName("UpdateEdificio")
            .WithSummary("Actualizar edificio")
            .Produces<string>(200)
            .Produces(404);

        // DELETE /espacios/edificios/{id} - Eliminar edificio (borrado lógico)
        group.MapDelete("/{id:long}", (long id) => Results.Ok($"Edificio {id} marcado como eliminado (borrado lógico)"))
            .WithName("DeleteEdificio")
            .WithSummary("Eliminar edificio (borrado lógico)")
            .Produces<string>(200)
            .Produces(404);

        return app;
    }
}


