using GateKeep.Api.Application.Espacios;
using GateKeep.Api.Contracts.Espacios;
using GateKeep.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Espacios;

public static class SalonEndpoints
{
    public static IEndpointRouteBuilder MapSalonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/espacios/salones").WithTags("Salones");

        // GET /espacios/salones - Todos los usuarios autenticados pueden ver salones
        group.MapGet("/", async (IEspacioRepository espacioRepository) =>
        {
            var espacios = await espacioRepository.ObtenerTodosAsync();
            var salones = espacios.OfType<Salon>().Select(s => new
            {
                s.Id,
                s.Nombre,
                s.Capacidad,
                s.NumeroSalon,
                EdificioId = s.EdificioId,
            });
            return Results.Ok(salones);
        })
            .RequireAuthorization("AllUsers")
            .WithName("GetSalones")
            .WithSummary("Obtener todos los salones")
            .Produces<IEnumerable<object>>(200)
            .Produces(401)
            .Produces(403);

        // GET /espacios/salones/{id} - Todos los usuarios autenticados pueden ver un salón específico
        group.MapGet("/{id:long}", async (long id, IEspacioRepository espacioRepository) =>
        {
            var espacio = await espacioRepository.ObtenerPorIdAsync(id);
            if (espacio is not Salon salon)
                return Results.NotFound(new { message = $"Salón con ID {id} no encontrado" });

            return Results.Ok(new
            {
                salon.Id,
                salon.Nombre,
                salon.Capacidad,
                salon.NumeroSalon,
                EdificioId = salon.EdificioId,
                salon.Descripcion,
            });
        })
            .RequireAuthorization("AllUsers")
            .WithName("GetSalonById")
            .WithSummary("Obtener salón por ID")
            .Produces<object>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        // POST /espacios/salones - Solo funcionarios y administradores pueden crear salones
        group.MapPost("/", async (CrearSalonRequest request, IEspacioFactory espacioFactory, IEspacioRepository espacioRepository) =>
        {
            var salon = await espacioFactory.CrearSalonAsync(request);
            var salonCreado = await espacioRepository.CrearAsync(salon);
            
            return Results.Created($"/espacios/salones/{salonCreado.Id}", new
            {
                message = "Salón creado exitosamente",
                id = salonCreado.Id,
                nombre = salonCreado.Nombre,
                numeroSalon = ((Salon)salonCreado).NumeroSalon
            });
        })
            .RequireAuthorization("FuncionarioOrAdmin")
            .WithName("CreateSalon")
            .WithSummary("Crear nuevo salón")
            .Produces<object>(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /espacios/salones/{id} - Solo funcionarios y administradores pueden actualizar salones
        group.MapPut("/{id:long}", async (long id, CrearSalonRequest request, IEspacioRepository espacioRepository) =>
        {
            var espacio = await espacioRepository.ObtenerPorIdAsync(id);
            if (espacio is not Salon salon)
                return Results.NotFound(new { message = $"Salón con ID {id} no encontrado" });

            // Crear una nueva instancia con los valores actualizados (records son inmutables)
            var salonActualizado = salon with
            {
                Nombre = request.Nombre ?? salon.Nombre,
                Capacidad = request.Capacidad,
                NumeroSalon = request.NumeroSalon,
                Descripcion = request.Descripcion ?? salon.Descripcion
            };
            
            await espacioRepository.ActualizarAsync(salonActualizado);
            
            return Results.Ok(new
            {
                message = $"Salón {id} actualizado exitosamente",
                id = salonActualizado.Id,
                nombre = salonActualizado.Nombre
            });
        })
            .RequireAuthorization("FuncionarioOrAdmin")
            .WithName("UpdateSalon")
            .WithSummary("Actualizar salón")
            .Produces<object>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        // DELETE /espacios/salones/{id} - Solo administradores pueden eliminar salones
        group.MapDelete("/{id:long}", async (long id, IEspacioRepository espacioRepository) =>
        {
            var espacio = await espacioRepository.ObtenerPorIdAsync(id);
            if (espacio is not Salon salon)
                return Results.NotFound(new { message = $"Salón con ID {id} no encontrado" });

            await espacioRepository.EliminarAsync(id);
            
            return Results.Ok(new
            {
                message = $"Salón {id} marcado como eliminado (borrado lógico)",
                id = salon.Id,
                nombre = salon.Nombre
            });
        })
            .RequireAuthorization("AdminOnly")
            .WithName("DeleteSalon")
            .WithSummary("Eliminar salón (borrado lógico)")
            .Produces<object>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        return app;
    }
}


