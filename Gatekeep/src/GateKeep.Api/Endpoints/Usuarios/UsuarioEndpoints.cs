using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Application.Usuarios;
using GateKeep.Api.Application.Security;
using GateKeep.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Usuarios;

public static class UsuarioEndpoints
{
    public static IEndpointRouteBuilder MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/usuarios").WithTags("Usuarios");

        // GET /usuarios - Solo administradores pueden ver todos los usuarios
        group.MapGet("/", async ([FromServices] IUsuarioRepository repo) =>
        {
            var usuarios = await repo.GetAllAsync();
            return Results.Ok(usuarios);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("GetUsuarios")
        .WithSummary("Obtener todos los usuarios")
        .Produces<List<Usuario>>(200)
        .Produces(401)
        .Produces(403);

        // GET /usuarios/{id} - Solo el propio usuario o administradores pueden ver el perfil
        group.MapGet("/{id:long}", async (long id, ClaimsPrincipal user, [FromServices] IUsuarioRepository repo) =>
        {
            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            
            // Solo puede ver su propio perfil o ser admin
            if (userId != id && userRole != "Admin")
                return Results.Forbid();
                
            var usuario = await repo.GetByIdAsync(id);
            return usuario is not null ? Results.Ok(usuario) : Results.NotFound();
        })
        .RequireAuthorization("AllUsers")
        .WithName("GetUsuarioById")
        .WithSummary("Obtener usuario por ID")
        .Produces<Usuario>(200)
        .Produces(404)
        .Produces(401)
        .Produces(403);

        // POST /usuarios/estudiante - Solo administradores pueden crear estudiantes
        group.MapPost("/estudiante", async (UsuarioDto dto, [FromServices] IUsuarioFactory factory, [FromServices] IUsuarioRepository repo, [FromServices] IPasswordService passwordService) =>
        {
            // Hashear la contraseña antes de crear el usuario
            var dtoConPasswordHasheada = dto with { Contrasenia = passwordService.HashPassword(dto.Contrasenia) };
            var usuario = factory.CrearEstudiante(dtoConPasswordHasheada);
            await repo.AddAsync(usuario);
            return Results.Created($"/usuarios/{usuario.Id}", usuario);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("CreateEstudiante")
        .WithSummary("Crear nuevo estudiante")
        .Produces<Usuario>(201)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // POST /usuarios/funcionario - Solo administradores pueden crear funcionarios
        group.MapPost("/funcionario", async (UsuarioDto dto, [FromServices] IUsuarioFactory factory, [FromServices] IUsuarioRepository repo, [FromServices] IPasswordService passwordService) =>
        {
            // Hashear la contraseña antes de crear el usuario
            var dtoConPasswordHasheada = dto with { Contrasenia = passwordService.HashPassword(dto.Contrasenia) };
            var usuario = factory.CrearFuncionario(dtoConPasswordHasheada);
            await repo.AddAsync(usuario);
            return Results.Created($"/usuarios/{usuario.Id}", usuario);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("CreateFuncionario")
        .WithSummary("Crear nuevo funcionario")
        .Produces<Usuario>(201)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // POST /usuarios/admin - Solo administradores pueden crear otros administradores
        group.MapPost("/admin", async (UsuarioDto dto, [FromServices] IUsuarioFactory factory, [FromServices] IUsuarioRepository repo, [FromServices] IPasswordService passwordService) =>
        {
            // Hashear la contraseña antes de crear el usuario
            var dtoConPasswordHasheada = dto with { Contrasenia = passwordService.HashPassword(dto.Contrasenia) };
            var admin = factory.CrearAdmin(dtoConPasswordHasheada);
            await repo.AddAsync(admin);
            return Results.Created($"/usuarios/{admin.Id}", admin);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("CreateAdmin")
        .WithSummary("Crear nuevo administrador")
        .Produces<Usuario>(201)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // DELETE /usuarios/{id} - Solo administradores pueden eliminar usuarios
        group.MapDelete("/{id:long}", async (long id, ClaimsPrincipal user, [FromServices] IUsuarioRepository repo) =>
        {
            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // No puede eliminarse a sí mismo
            if (userId == id)
                return Results.BadRequest("No puedes eliminarte a ti mismo");
                
            var usuario = await repo.GetByIdAsync(id);
            if (usuario is null) return Results.NotFound();

            await repo.DeleteAsync(id);
            return Results.Ok($"Usuario {id} marcado como eliminado");
        })
        .RequireAuthorization("AdminOnly")
        .WithName("DeleteUsuario")
        .WithSummary("Eliminar usuario (borrado lógico)")
        .Produces<string>(200)
        .Produces(404)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        return app;
    }
}
