using GateKeep.Api.Application.Espacios;
using GateKeep.Api.Contracts.Espacios;
using GateKeep.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GateKeep.Api.Endpoints.Espacios;

public static class EspacioEndpoints
{
    public static IEndpointRouteBuilder MapEspacioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/espacios").WithTags("Espacios");

        // GET /espacios - Obtener todos los espacios
        group.MapGet("/", async (IEspacioRepository repository) =>
        {
            var espacios = await repository.ObtenerTodosAsync();
            return Results.Ok(espacios);
        })
        .WithName("GetEspacios")
        .WithSummary("Obtener todos los espacios")
        .Produces<IEnumerable<Espacio>>(200);

        // GET /espacios/{id} - Obtener espacio por ID
        group.MapGet("/{id:long}", async (long id, IEspacioRepository repository) =>
        {
            var espacio = await repository.ObtenerPorIdAsync(id);
            return espacio is not null ? Results.Ok(espacio) : Results.NotFound();
        })
        .WithName("GetEspacioById")
        .WithSummary("Obtener espacio por ID")
        .Produces<Espacio>(200)
        .Produces(404);

        // POST /espacios/edificio - Crear nuevo edificio usando factory
        group.MapPost("/edificio", async (CrearEdificioRequest request, IEspacioFactory factory, IEspacioRepository repository) =>
        {
            try
            {
                var edificio = await factory.CrearEdificioAsync(request);
                var edificioGuardado = await repository.GuardarEdificioAsync(edificio);
                return Results.CreatedAtRoute("GetEspacioById", new { id = edificioGuardado.Id }, edificioGuardado);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .WithName("CreateEdificio")
        .WithSummary("Crear nuevo edificio")
        .Accepts<CrearEdificioRequest>("application/json")
        .Produces<Espacio>(201)
        .Produces(400)
        .Produces(409);

        // POST /espacios/salon - Crear nuevo salón usando factory
        group.MapPost("/salon", async (CrearSalonRequest request, IEspacioFactory factory, IEspacioRepository repository) =>
        {
            try
            {
                var salon = await factory.CrearSalonAsync(request);
                var salonGuardado = await repository.GuardarSalonAsync(salon);
                return Results.CreatedAtRoute("GetEspacioById", new { id = salonGuardado.Id }, salonGuardado);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .WithName("CreateSalon")
        .WithSummary("Crear nuevo salón")
        .Accepts<CrearSalonRequest>("application/json")
        .Produces<Espacio>(201)
        .Produces(400)
        .Produces(409);

        // POST /espacios/laboratorio - Crear nuevo laboratorio usando factory
        group.MapPost("/laboratorio", async (CrearLaboratorioRequest request, IEspacioFactory factory, IEspacioRepository repository) =>
        {
            try
            {
                var laboratorio = await factory.CrearLaboratorioAsync(request);
                var laboratorioGuardado = await repository.GuardarLaboratorioAsync(laboratorio);
                return Results.CreatedAtRoute("GetEspacioById", new { id = laboratorioGuardado.Id }, laboratorioGuardado);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .WithName("CreateLaboratorio")
        .WithSummary("Crear nuevo laboratorio")
        .Accepts<CrearLaboratorioRequest>("application/json")
        .Produces<Espacio>(201)
        .Produces(400)
        .Produces(409);

        // POST /espacios - Crear espacio genérico usando factory
        group.MapPost("/", async (CrearEspacioRequest request, IEspacioFactory factory, IEspacioRepository repository) =>
        {
            // Intentar determinar el tipo basado en las propiedades del request
            string tipoEspacio = DeterminarTipoEspacio(request);
            
            if (!factory.EsTipoValido(tipoEspacio))
            {
                return Results.BadRequest(new { error = $"Tipo de espacio no válido: {tipoEspacio}" });
            }

            if (factory.TryCrear(tipoEspacio, request, out var espacioTask))
            {
                try
                {
                    var espacio = await espacioTask!;
                    var espacioCreado = await repository.CrearAsync(espacio);
                    return Results.CreatedAtRoute("GetEspacioById", new { id = espacioCreado.Id }, espacioCreado);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { error = ex.Message });
                }
            }

            return Results.BadRequest(new { error = "No se pudo crear el espacio" });
        })
        .WithName("CreateEspacio")
        .WithSummary("Crear espacio genérico")
        .Accepts<CrearEspacioRequest>("application/json")
        .Produces<Espacio>(201)
        .Produces(400)
        .Produces(409);

        // DELETE /espacios/{id} - Eliminar espacio
        group.MapDelete("/{id:long}", async (long id, IEspacioRepository repository) =>
        {
            var eliminado = await repository.EliminarAsync(id);
            return eliminado ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteEspacio")
        .WithSummary("Eliminar espacio")
        .Produces(204)
        .Produces(404);

        return app;
    }

    private static string DeterminarTipoEspacio(CrearEspacioRequest request)
    {
        // Lógica simple para determinar el tipo basado en las propiedades
        // En un escenario real, esto podría ser más sofisticado
        if (request is CrearEdificioRequest)
            return "edificio";
        if (request is CrearSalonRequest)
            return "salon";
        if (request is CrearLaboratorioRequest)
            return "laboratorio";
        
        // Por defecto, asumir que es un edificio si no se puede determinar
        return "edificio";
    }
}


