using GateKeep.Api.Contracts.Espacios;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Espacios;

public static class EdificioEndpoints
{
    public static IEndpointRouteBuilder MapEdificioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/espacios/edificios").WithTags("Edificios");

        // GET /espacios/edificios - Todos los usuarios autenticados pueden ver edificios
        group.MapGet("/", () => Results.Ok("Lista de edificios"))
            .RequireAuthorization("AllUsers")
            .WithName("GetEdificios")
            .WithSummary("Obtener todos los edificios")
            .Produces<string>(200)
            .Produces(401)
            .Produces(403);

        // GET /espacios/edificios/{id} - Todos los usuarios autenticados pueden ver un edificio específico
        group.MapGet("/{id:long}", (long id) => Results.Ok($"Edificio {id}"))
            .RequireAuthorization("AllUsers")
            .WithName("GetEdificioById")
            .WithSummary("Obtener edificio por ID")
            .Produces<string>(200)
            .Produces(401)
            .Produces(403);

        // POST /espacios/edificios - Solo funcionarios y administradores pueden crear edificios
        group.MapPost("/", (CrearEdificioRequest request) => Results.Created($"/espacios/edificios/1", "Edificio creado"))
            .RequireAuthorization("FuncionarioOrAdmin")
            .WithName("CreateEdificio")
            .WithSummary("Crear nuevo edificio")
            .Produces<string>(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /espacios/edificios/{id} - Solo funcionarios y administradores pueden actualizar edificios
        group.MapPut("/{id:long}", (long id, CrearEdificioRequest request) => Results.Ok($"Edificio {id} actualizado"))
            .RequireAuthorization("FuncionarioOrAdmin")
            .WithName("UpdateEdificio")
            .WithSummary("Actualizar edificio")
            .Produces<string>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        // DELETE /espacios/edificios/{id} - Solo administradores pueden eliminar edificios
        group.MapDelete("/{id:long}", (long id) => Results.Ok($"Edificio {id} marcado como eliminado (borrado lógico)"))
            .RequireAuthorization("AdminOnly")
            .WithName("DeleteEdificio")
            .WithSummary("Eliminar edificio (borrado lógico)")
            .Produces<string>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        return app;
    }
}


