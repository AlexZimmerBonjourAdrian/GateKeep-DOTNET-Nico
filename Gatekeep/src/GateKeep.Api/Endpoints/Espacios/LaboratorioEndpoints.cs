using GateKeep.Api.Contracts.Espacios;
using Microsoft.AspNetCore.Mvc;

namespace GateKeep.Api.Endpoints.Espacios;

public static class LaboratorioEndpoints
{
    public static IEndpointRouteBuilder MapLaboratorioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/espacios/laboratorios").WithTags("Laboratorios");

        // GET /espacios/laboratorios - Obtener todos los laboratorios
        group.MapGet("/", () => Results.Ok("Lista de laboratorios"))
            .WithName("GetLaboratorios")
            .WithSummary("Obtener todos los laboratorios")
            .Produces<string>(200);

        // GET /espacios/laboratorios/{id} - Obtener laboratorio por ID
        group.MapGet("/{id:long}", (long id) => Results.Ok($"Laboratorio {id}"))
            .WithName("GetLaboratorioById")
            .WithSummary("Obtener laboratorio por ID")
            .Produces<string>(200);

        // POST /espacios/laboratorios - Crear nuevo laboratorio
        group.MapPost("/", (CrearLaboratorioRequest request) => Results.Created($"/espacios/laboratorios/1", "Laboratorio creado"))
            .WithName("CreateLaboratorio")
            .WithSummary("Crear nuevo laboratorio")
            .Produces<string>(201)
            .Produces(400);

        // PUT /espacios/laboratorios/{id} - Actualizar laboratorio
        group.MapPut("/{id:long}", (long id, CrearLaboratorioRequest request) => Results.Ok($"Laboratorio {id} actualizado"))
            .WithName("UpdateLaboratorio")
            .WithSummary("Actualizar laboratorio")
            .Produces<string>(200)
            .Produces(404);

        // DELETE /espacios/laboratorios/{id} - Eliminar laboratorio (borrado lógico)
        group.MapDelete("/{id:long}", (long id) => Results.Ok($"Laboratorio {id} marcado como eliminado (borrado lógico)"))
            .WithName("DeleteLaboratorio")
            .WithSummary("Eliminar laboratorio (borrado lógico)")
            .Produces<string>(200)
            .Produces(404);

        return app;
    }
}


