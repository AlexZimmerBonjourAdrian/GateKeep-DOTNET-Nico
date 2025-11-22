using GateKeep.Api.Application.Espacios;
using GateKeep.Api.Contracts.Espacios;
using GateKeep.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Espacios;

public static class LaboratorioEndpoints
{
    public static IEndpointRouteBuilder MapLaboratorioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/espacios/laboratorios").WithTags("Laboratorios");

        // GET /api/espacios/laboratorios - Todos los usuarios autenticados pueden ver laboratorios
        group.MapGet("/", async (IEspacioRepository espacioRepository) =>
        {
            var espacios = await espacioRepository.ObtenerTodosAsync();
            var laboratorios = espacios.OfType<Laboratorio>().Select(l => new
            {
                l.Id,
                l.Nombre,
                l.Capacidad,
                l.NumeroLaboratorio,
                EdificioId = l.EdificioId,
                l.TipoLaboratorio,
                l.EquipamientoEspecial,
                l.Ubicacion,
                l.Activo
            });
            return Results.Ok(laboratorios);
        })
            .RequireAuthorization("AllUsers")
            .WithName("GetLaboratorios")
            .WithSummary("Obtener todos los laboratorios")
            .Produces<IEnumerable<object>>(200)
            .Produces(401)
            .Produces(403);

        // GET /api/espacios/laboratorios/{id} - Todos los usuarios autenticados pueden ver un laboratorio específico
        group.MapGet("/{id:long}", async (long id, IEspacioRepository espacioRepository) =>
        {
            var espacio = await espacioRepository.ObtenerPorIdAsync(id);
            if (espacio is not Laboratorio laboratorio)
                return Results.NotFound(new { message = $"Laboratorio con ID {id} no encontrado" });

            return Results.Ok(new
            {
                laboratorio.Id,
                laboratorio.Nombre,
                laboratorio.Capacidad,
                laboratorio.NumeroLaboratorio,
                EdificioId = laboratorio.EdificioId,
                laboratorio.TipoLaboratorio,
                laboratorio.EquipamientoEspecial,
                laboratorio.Ubicacion,
                laboratorio.Descripcion,
                laboratorio.Activo
            });
        })
            .RequireAuthorization("AllUsers")
            .WithName("GetLaboratorioById")
            .WithSummary("Obtener laboratorio por ID")
            .Produces<object>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        // POST /api/espacios/laboratorios - Solo funcionarios y administradores pueden crear laboratorios
        group.MapPost("/", async (CrearLaboratorioRequest request, IEspacioFactory espacioFactory, IEspacioRepository espacioRepository) =>
        {
            try
            {
                var laboratorio = await espacioFactory.CrearLaboratorioAsync(request);
                var laboratorioCreado = await espacioRepository.CrearAsync(laboratorio);
                
                return Results.Created($"/api/espacios/laboratorios/{laboratorioCreado.Id}", new
                {
                    message = "Laboratorio creado exitosamente",
                    id = laboratorioCreado.Id,
                    nombre = laboratorioCreado.Nombre,
                    numeroLaboratorio = ((Laboratorio)laboratorioCreado).NumeroLaboratorio
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
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("foreign key") == true || 
                                                ex.InnerException?.Message.Contains("violates foreign key") == true ||
                                                ex.InnerException?.Message.Contains("23503") == true)
            {
                return Results.BadRequest(new { error = "El edificio especificado no existe" });
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique") == true || 
                                                ex.InnerException?.Message.Contains("duplicate") == true ||
                                                ex.InnerException?.Message.Contains("23505") == true)
            {
                return Results.BadRequest(new { error = "Ya existe un laboratorio con ese número en el edificio especificado" });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Error al crear el laboratorio: {ex.Message}",
                    statusCode: 500);
            }
        })
            .RequireAuthorization("FuncionarioOrAdmin")
            .WithName("CreateLaboratorio")
            .WithSummary("Crear nuevo laboratorio")
            .Produces<object>(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /api/espacios/laboratorios/{id} - Solo funcionarios y administradores pueden actualizar laboratorios
        group.MapPut("/{id:long}", async (long id, CrearLaboratorioRequest request, IEspacioRepository espacioRepository) =>
        {
            var espacio = await espacioRepository.ObtenerPorIdAsync(id);
            if (espacio is not Laboratorio laboratorio)
                return Results.NotFound(new { message = $"Laboratorio con ID {id} no encontrado" });

            // Crear una nueva instancia con los valores actualizados (records son inmutables)
            var laboratorioActualizado = laboratorio with
            {
                Nombre = request.Nombre ?? laboratorio.Nombre,
                Capacidad = request.Capacidad,
                NumeroLaboratorio = request.NumeroLaboratorio,
                TipoLaboratorio = request.TipoLaboratorio ?? laboratorio.TipoLaboratorio,
                EquipamientoEspecial = request.EquipamientoEspecial,
                Descripcion = request.Descripcion ?? laboratorio.Descripcion,
                Ubicacion = request.Ubicacion ?? laboratorio.Ubicacion
            };
            
            await espacioRepository.ActualizarAsync(laboratorioActualizado);
            
            return Results.Ok(new
            {
                message = $"Laboratorio {id} actualizado exitosamente",
                id = laboratorioActualizado.Id,
                nombre = laboratorioActualizado.Nombre
            });
        })
            .RequireAuthorization("FuncionarioOrAdmin")
            .WithName("UpdateLaboratorio")
            .WithSummary("Actualizar laboratorio")
            .Produces<object>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        // DELETE /api/espacios/laboratorios/{id} - Solo administradores pueden eliminar laboratorios
        group.MapDelete("/{id:long}", async (long id, IEspacioRepository espacioRepository) =>
        {
            var espacio = await espacioRepository.ObtenerPorIdAsync(id);
            if (espacio is not Laboratorio laboratorio)
                return Results.NotFound(new { message = $"Laboratorio con ID {id} no encontrado" });

            await espacioRepository.EliminarAsync(id);
            
            return Results.Ok(new
            {
                message = $"Laboratorio {id} marcado como eliminado (borrado lógico)",
                id = laboratorio.Id,
                nombre = laboratorio.Nombre
            });
        })
            .RequireAuthorization("AdminOnly")
            .WithName("DeleteLaboratorio")
            .WithSummary("Eliminar laboratorio (borrado lógico)")
            .Produces<object>(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        return app;
    }
}


