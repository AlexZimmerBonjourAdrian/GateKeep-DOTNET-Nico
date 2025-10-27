using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Application.Usuarios;
using GateKeep.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GateKeep.Api.Endpoints.Usuarios;

public static class UsuarioEndpoints
{
    public static IEndpointRouteBuilder MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/usuarios").WithTags("Usuarios");

        // GET /usuarios - Obtener todos los usuarios
        group.MapGet("/", async ([FromServices] IUsuarioRepository repo) =>
        {
            var usuarios = await repo.GetAllAsync();
            return Results.Ok(usuarios);
        })
        .WithName("GetUsuarios")
        .WithSummary("Obtener todos los usuarios")
        .Produces<List<Usuario>>(200);

        // GET /usuarios/{id} - Obtener usuario por ID
        group.MapGet("/{id:long}", async (long id, [FromServices] IUsuarioRepository repo) =>
        {
            var usuario = await repo.GetByIdAsync(id);
            return usuario is not null ? Results.Ok(usuario) : Results.NotFound();
        })
        .WithName("GetUsuarioById")
        .WithSummary("Obtener usuario por ID")
        .Produces<Usuario>(200)
        .Produces(404);

        // POST /usuarios - Crear nuevo usuario
        group.MapPost("/", async (UsuarioDto dto, [FromServices] IUsuarioFactory factory, [FromServices] IUsuarioRepository repo) =>
        {
            if (!factory.EsRolValido(dto.Rol))
                return Results.BadRequest($"Rol inválido: {dto.Rol}");

            var usuario = factory.CrearUsuario(dto);
            await repo.AddAsync(usuario);
            return Results.Created($"/usuarios/{usuario.Id}", usuario);
        })
        .WithName("CreateUsuario")
        .WithSummary("Crear nuevo usuario")
        .Produces<Usuario>(201)
        .Produces(400);

        // PUT /usuarios/{id} - Actualizar usuario
        group.MapPut("/{id:long}", async (long id, UsuarioDto dto, [FromServices] IUsuarioFactory factory, [FromServices] IUsuarioRepository repo) =>
        {
            var existente = await repo.GetByIdAsync(id);
            if (existente is null) return Results.NotFound();

            var actualizado = factory.CrearUsuario(dto) with { Id = id };
            await repo.UpdateAsync(actualizado);
            return Results.Ok(actualizado);
        })
        .WithName("UpdateUsuario")
        .WithSummary("Actualizar usuario")
        .Produces<Usuario>(200)
        .Produces(404);

        // DELETE /usuarios/{id} - Eliminar usuario (borrado lógico)
        group.MapDelete("/{id:long}", async (long id, [FromServices] IUsuarioRepository repo) =>
        {
            var usuario = await repo.GetByIdAsync(id);
            if (usuario is null) return Results.NotFound();

            await repo.DeleteAsync(id); // Asumiendo borrado lógico
            return Results.Ok($"Usuario {id} marcado como eliminado");
        })
        .WithName("DeleteUsuario")
        .WithSummary("Eliminar usuario (borrado lógico)")
        .Produces<string>(200)
        .Produces(404);

        return app;
    }
}
