using GateKeep.Api.Contracts.Espacios;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Espacios;

public static class LaboratorioEndpoints
{
    public static IEndpointRouteBuilder MapLaboratorioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/espacios/laboratorios").WithTags("Laboratorios");

        // GET /espacios/laboratorios - Todos los usuarios autenticados pueden ver laboratorios
        group.MapGet("/", () => Results.Ok("Lista de laboratorios"))
            .RequireAuthorization("AllUsers")
            .WithName("GetLaboratorios")
            .WithSummary("Obtener todos los laboratorios")
            .Produces<string>(200)
            .Produces(401)
            .Produces(403);

        // GET /espacios/laboratorios/{id} - Todos los usuarios autenticados pueden ver un laboratorio específico
        group.MapGet("/{id:long}", (long id) => Results.Ok($"Laboratorio {id}"))
            .RequireAuthorization("AllUsers")
            .WithName("GetLaboratorioById")
            .WithSummary("Obtener laboratorio por ID")
            .Produces<string>(200)
            .Produces(401)
            .Produces(403);

        // POST /espacios/laboratorios - Solo funcionarios y administradores pueden crear laboratorios
        group.MapPost("/", (CrearLaboratorioRequest request) => Results.Created($"/espacios/laboratorios/1", "Laboratorio creado"))
            .RequireAuthorization("FuncionarioOrAdmin")
            .WithName("CreateLaboratorio")
            .WithSummary("Crear nuevo laboratorio")
            .Produces<string>(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /espacios/laboratorios/{id} - Solo funcionarios y administradores pueden actualizar laboratorios
        group.MapPut("/{id:long}", (long id, CrearLaboratorioRequest request) => Results.Ok($"Laboratorio {id} actualizado"))
            .RequireAuthorization("FuncionarioOrAdmin")
            .WithName("UpdateLaboratorio")
            .WithSummary("Actualizar laboratorio")
            .Produces<string>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        // DELETE /espacios/laboratorios/{id} - Solo administradores pueden eliminar laboratorios
        group.MapDelete("/{id:long}", (long id) => Results.Ok($"Laboratorio {id} marcado como eliminado (borrado lógico)"))
            .RequireAuthorization("AdminOnly")
            .WithName("DeleteLaboratorio")
            .WithSummary("Eliminar laboratorio (borrado lógico)")
            .Produces<string>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        return app;
    }
}


