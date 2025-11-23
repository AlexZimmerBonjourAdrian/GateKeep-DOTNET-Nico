using GateKeep.Api.Application.Espacios;
using GateKeep.Api.Contracts.Espacios;
using GateKeep.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Espacios;

public static class EdificioEndpoints
{
    public static IEndpointRouteBuilder MapEdificioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/espacios/edificios").WithTags("Edificios");

        // GET /api/espacios/edificios - Todos los usuarios autenticados pueden ver edificios
        group.MapGet("/", async (IEspacioRepository espacioRepository) =>
        {
            var espacios = await espacioRepository.ObtenerTodosAsync();
            var edificios = espacios.OfType<Edificio>().Select(e => new
            {
                e.Id,
                e.Nombre,
                e.Capacidad,
                e.NumeroPisos,
                e.CodigoEdificio,
                e.Ubicacion,
                e.Activo
            });
            return Results.Ok(edificios);
        })
            .RequireAuthorization("AllUsers")
            .WithName("GetEdificios")
            .WithSummary("Obtener todos los edificios")
            .Produces<IEnumerable<object>>(200)
            .Produces(401)
            .Produces(403);

        // GET /api/espacios/edificios/{id} - Todos los usuarios autenticados pueden ver un edificio específico
        group.MapGet("/{id:long}", async (long id, IEspacioRepository espacioRepository) =>
        {
            var espacio = await espacioRepository.ObtenerPorIdAsync(id);
            if (espacio is not Edificio edificio)
                return Results.NotFound(new { message = $"Edificio con ID {id} no encontrado" });

            return Results.Ok(new
            {
                edificio.Id,
                edificio.Nombre,
                edificio.Capacidad,
                edificio.NumeroPisos,
                edificio.CodigoEdificio,
                edificio.Ubicacion,
                edificio.Descripcion,
                edificio.Activo
            });
        })
            .RequireAuthorization("AllUsers")
            .WithName("GetEdificioById")
            .WithSummary("Obtener edificio por ID")
            .Produces<object>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        // POST /api/espacios/edificios - Solo funcionarios y administradores pueden crear edificios
        group.MapPost("/", async (CrearEdificioRequest request, IEspacioFactory espacioFactory, IEspacioRepository espacioRepository) =>
        {
            try
            {
                var edificio = await espacioFactory.CrearEdificioAsync(request);
                var edificioCreado = await espacioRepository.CrearAsync(edificio);
                
                return Results.Created($"/api/espacios/edificios/{edificioCreado.Id}", new
                {
                    message = "Edificio creado exitosamente",
                    id = edificioCreado.Id,
                    nombre = edificioCreado.Nombre,
                    codigoEdificio = ((Edificio)edificioCreado).CodigoEdificio
                });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique") == true || 
                                                ex.InnerException?.Message.Contains("duplicate") == true ||
                                                ex.InnerException?.Message.Contains("23505") == true)
            {
                return Results.BadRequest(new { error = "Ya existe un edificio con ese código" });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Error al crear el edificio: {ex.Message}",
                    statusCode: 500);
            }
        })
            .RequireAuthorization("FuncionarioOrAdmin")
            .WithName("CreateEdificio")
            .WithSummary("Crear nuevo edificio")
            .Produces<object>(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /api/espacios/edificios/{id} - Solo funcionarios y administradores pueden actualizar edificios
        group.MapPut("/{id:long}", async (long id, CrearEdificioRequest request, IEspacioRepository espacioRepository) =>
        {
            var espacio = await espacioRepository.ObtenerPorIdAsync(id);
            if (espacio is not Edificio edificio)
                return Results.NotFound(new { message = $"Edificio con ID {id} no encontrado" });

            // Crear una nueva instancia con los valores actualizados (records son inmutables)
            var edificioActualizado = edificio with
            {
                Nombre = request.Nombre ?? edificio.Nombre,
                Capacidad = request.Capacidad,
                NumeroPisos = request.NumeroPisos,
                CodigoEdificio = request.CodigoEdificio ?? edificio.CodigoEdificio,
                Descripcion = request.Descripcion ?? edificio.Descripcion,
                Ubicacion = request.Ubicacion ?? edificio.Ubicacion,
                Activo = true
            };
            
            await espacioRepository.ActualizarAsync(edificioActualizado);
            
            return Results.Ok(new
            {
                message = $"Edificio {id} actualizado exitosamente",
                id = edificioActualizado.Id,
                nombre = edificioActualizado.Nombre
            });
        })
            .RequireAuthorization("FuncionarioOrAdmin")
            .WithName("UpdateEdificio")
            .WithSummary("Actualizar edificio")
            .Produces<object>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        // DELETE /api/espacios/edificios/{id} - Solo administradores pueden eliminar edificios
        group.MapDelete("/{id:long}", async (long id, IEspacioRepository espacioRepository) =>
        {
            var espacio = await espacioRepository.ObtenerPorIdAsync(id);
            if (espacio is not Edificio edificio)
                return Results.NotFound(new { message = $"Edificio con ID {id} no encontrado" });

            await espacioRepository.EliminarAsync(id);
            
            return Results.Ok(new
            {
                message = $"Edificio {id} desactivado correctamente",
                id = edificio.Id,
                nombre = edificio.Nombre
            });
        })
            .RequireAuthorization("AdminOnly")
            .WithName("DeleteEdificio")
            .WithSummary("Eliminar edificio (borrado lógico)")
            .Produces<object>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        return app;
    }
}


