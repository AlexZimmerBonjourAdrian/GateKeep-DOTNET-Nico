using GateKeep.Api.Application.Auditoria;
using GateKeep.Api.Application.Events;
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

        // POST /usuarios - Crear usuario con rol (Admin, Estudiante, Funcionario)
        group.MapPost("/", async (
            UsuarioDto dto,
            ClaimsPrincipal user,
            [FromServices] IUsuarioFactory factory,
            [FromServices] IUsuarioRepository repo,
            [FromServices] IPasswordService passwordService,
            [FromServices] IEventoHistoricoService? eventoHistoricoService,
            [FromServices] IEventPublisher? eventPublisher) =>
        {
            var dtoConPasswordHasheada = dto with { Contrasenia = passwordService.HashPassword(dto.Contrasenia) };
            var usuario = factory.CrearUsuario(dtoConPasswordHasheada);
            await repo.AddAsync(usuario);
            
            var fecha = DateTime.UtcNow;
            
            if (eventoHistoricoService != null)
            {
                try
                {
                    var modificadoPor = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    await eventoHistoricoService.RegistrarCambioRolAsync(
                        usuario.Id,
                        "Nuevo",
                        dto.Rol.ToString(),
                        modificadoPor,
                        new Dictionary<string, object> { { "accion", "CreacionUsuario" } });
                }
                catch
                {
                }
            }

            // Notificar a observers (Observer Pattern)
            if (eventPublisher != null)
            {
                try
                {
                    await eventPublisher.NotifyUsuarioCreadoAsync(
                        usuario.Id,
                        usuario.Email,
                        usuario.Nombre,
                        usuario.Apellido,
                        usuario.Rol.ToString(),
                        fecha);
                }
                catch
                {
                    // Log error pero no romper el flujo principal
                }
            }
            
            return Results.Created($"/usuarios/{usuario.Id}", usuario);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("CreateUsuario")
        .WithSummary("Crear nuevo usuario con rol")
        .Produces<Usuario>(201)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // PUT /usuarios/{id}/rol - Solo administradores pueden actualizar el rol de un usuario
        group.MapPut("/{id:long}/rol", async (
            long id,
            [FromBody] ActualizarRolRequest request,
            ClaimsPrincipal user,
            [FromServices] IUsuarioRepository repo,
            [FromServices] IEventoHistoricoService? eventoHistoricoService,
            [FromServices] IEventPublisher? eventPublisher) =>
        {
            var usuarioActual = await repo.GetByIdAsync(id);
            if (usuarioActual is null)
                return Results.NotFound();

            var rolAnterior = usuarioActual.Rol;
            if (rolAnterior == request.Rol)
                return Results.Ok(usuarioActual);

            var usuarioActualizado = usuarioActual with { Rol = request.Rol };
            await repo.UpdateAsync(usuarioActualizado);

            var fecha = DateTime.UtcNow;
            var modificadoPor = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (eventoHistoricoService != null)
            {
                try
                {
                    await eventoHistoricoService.RegistrarCambioRolAsync(
                        id,
                        rolAnterior.ToString(),
                        request.Rol.ToString(),
                        modificadoPor,
                        new Dictionary<string, object> { { "accion", "ActualizacionRol" } });
                }
                catch
                {
                }
            }

            // Notificar a observers (Observer Pattern)
            if (eventPublisher != null)
            {
                try
                {
                    await eventPublisher.NotifyCambioRolAsync(
                        id,
                        rolAnterior.ToString(),
                        request.Rol.ToString(),
                        modificadoPor,
                        fecha);
                }
                catch
                {
                    // Log error pero no romper el flujo principal
                }
            }

            return Results.Ok(usuarioActualizado);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("ActualizarRolUsuario")
        .WithSummary("Actualizar el rol de un usuario")
        .Produces<Usuario>(200)
        .Produces(404)
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
