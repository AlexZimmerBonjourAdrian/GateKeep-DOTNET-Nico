using GateKeep.Api.Contracts.Espacios;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Espacios;

public static class SalonEndpoints
{
    public static IEndpointRouteBuilder MapSalonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/espacios/salones").WithTags("Salones");

        // GET /espacios/salones - Todos los usuarios autenticados pueden ver salones
        group.MapGet("/", () => Results.Ok("Lista de salones"))
            .RequireAuthorization("AllUsers")
            .WithName("GetSalones")
            .WithSummary("Obtener todos los salones")
            .Produces<string>(200)
            .Produces(401)
            .Produces(403);

        // GET /espacios/salones/{id} - Todos los usuarios autenticados pueden ver un salón específico
        group.MapGet("/{id:long}", (long id) => Results.Ok($"Salón {id}"))
            .RequireAuthorization("AllUsers")
            .WithName("GetSalonById")
            .WithSummary("Obtener salón por ID")
            .Produces<string>(200)
            .Produces(401)
            .Produces(403);

        // POST /espacios/salones - Solo funcionarios y administradores pueden crear salones
        group.MapPost("/", (CrearSalonRequest request) => Results.Created($"/espacios/salones/1", "Salón creado"))
            .RequireAuthorization("FuncionarioOrAdmin")
            .WithName("CreateSalon")
            .WithSummary("Crear nuevo salón")
            .Produces<string>(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /espacios/salones/{id} - Solo funcionarios y administradores pueden actualizar salones
        group.MapPut("/{id:long}", (long id, CrearSalonRequest request) => Results.Ok($"Salón {id} actualizado"))
            .RequireAuthorization("FuncionarioOrAdmin")
            .WithName("UpdateSalon")
            .WithSummary("Actualizar salón")
            .Produces<string>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        // DELETE /espacios/salones/{id} - Solo administradores pueden eliminar salones
        group.MapDelete("/{id:long}", (long id) => Results.Ok($"Salón {id} marcado como eliminado (borrado lógico)"))
            .RequireAuthorization("AdminOnly")
            .WithName("DeleteSalon")
            .WithSummary("Eliminar salón (borrado lógico)")
            .Produces<string>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        return app;
    }
}


