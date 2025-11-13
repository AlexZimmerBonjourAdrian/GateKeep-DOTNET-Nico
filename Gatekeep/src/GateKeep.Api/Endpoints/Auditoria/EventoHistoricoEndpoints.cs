using GateKeep.Api.Application.Auditoria;
using GateKeep.Api.Contracts.Auditoria;
using GateKeep.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Auditoria;

public static class EventoHistoricoEndpoints
{
    public static IEndpointRouteBuilder MapEventoHistoricoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auditoria/eventos")
            .WithTags("Auditoria")
            .WithOpenApi();

        group.MapGet("/", async (
            [FromServices] IEventoHistoricoRepository repository,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] long? usuarioId = null,
            [FromQuery] string? tipoEvento = null,
            [FromQuery] string? resultado = null) =>
        {
            var filtroBuilder = Builders<EventoHistorico>.Filter.Empty;

            if (fechaDesde.HasValue)
                filtroBuilder &= Builders<EventoHistorico>.Filter.Gte(e => e.Fecha, fechaDesde.Value);

            if (fechaHasta.HasValue)
                filtroBuilder &= Builders<EventoHistorico>.Filter.Lte(e => e.Fecha, fechaHasta.Value);

            if (usuarioId.HasValue)
                filtroBuilder &= Builders<EventoHistorico>.Filter.Eq(e => e.UsuarioId, usuarioId.Value);

            if (!string.IsNullOrEmpty(tipoEvento))
                filtroBuilder &= Builders<EventoHistorico>.Filter.Eq(e => e.TipoEvento, tipoEvento);

            if (!string.IsNullOrEmpty(resultado))
                filtroBuilder &= Builders<EventoHistorico>.Filter.Eq(e => e.Resultado, resultado);

            var (eventos, totalCount) = await repository.ObtenerFiltradosAsync(
                filtroBuilder,
                page,
                pageSize);

            var eventosDto = eventos.Select(e => new EventoHistoricoDto
            {
                Id = e.Id,
                TipoEvento = e.TipoEvento,
                Fecha = e.Fecha,
                UsuarioId = e.UsuarioId,
                EspacioId = e.EspacioId,
                Resultado = e.Resultado,
                PuntoControl = e.PuntoControl,
                Datos = e.Datos != null ? ConvertirBsonADict(e.Datos) : null
            });

            return Results.Ok(new EventoHistoricoPaginadoDto
            {
                Eventos = eventosDto,
                Paginacion = new PaginacionDto
                {
                    Pagina = page,
                    TamanoPagina = pageSize,
                    TotalCount = totalCount,
                    TotalPaginas = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("GetEventosHistoricos")
        .WithSummary("Obtener eventos históricos con paginación y filtros")
        .Produces<EventoHistoricoPaginadoDto>(200)
        .Produces(401)
        .Produces(403);

        group.MapGet("/usuario/{usuarioId}", async (
            long usuarioId,
            [FromServices] IEventoHistoricoRepository repository,
            ClaimsPrincipal user,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string? tipoEvento = null) =>
        {
            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (userId != usuarioId && userRole != "Funcionario" && userRole != "Admin")
                return Results.Forbid();

            var (eventos, totalCount) = await repository.ObtenerPorUsuarioAsync(
                usuarioId,
                fechaDesde,
                fechaHasta,
                tipoEvento,
                page,
                pageSize);

            var eventosDto = eventos.Select(e => new EventoHistoricoDto
            {
                Id = e.Id,
                TipoEvento = e.TipoEvento,
                Fecha = e.Fecha,
                UsuarioId = e.UsuarioId,
                EspacioId = e.EspacioId,
                Resultado = e.Resultado,
                PuntoControl = e.PuntoControl,
                Datos = e.Datos != null ? ConvertirBsonADict(e.Datos) : null
            });

            return Results.Ok(new EventoHistoricoPaginadoDto
            {
                Eventos = eventosDto,
                Paginacion = new PaginacionDto
                {
                    Pagina = page,
                    TamanoPagina = pageSize,
                    TotalCount = totalCount,
                    TotalPaginas = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        })
        .RequireAuthorization("AllUsers")
        .WithName("GetEventosHistoricosPorUsuario")
        .WithSummary("Obtener eventos históricos de un usuario")
        .Produces<EventoHistoricoPaginadoDto>(200)
        .Produces(401)
        .Produces(403);

        group.MapGet("/estadisticas", async (
            [FromServices] IEventoHistoricoRepository repository,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null) =>
        {
            var fechaDesdeFinal = fechaDesde ?? DateTime.UtcNow.AddDays(-30);
            var fechaHastaFinal = fechaHasta ?? DateTime.UtcNow;

            if (fechaDesdeFinal > fechaHastaFinal)
            {
                return Results.BadRequest(new { error = "La fecha desde no puede ser mayor que la fecha hasta" });
            }

            var estadisticas = await repository.ObtenerEstadisticasPorTipoAsync(fechaDesdeFinal, fechaHastaFinal);

            return Results.Ok(new ReporteEstadisticasDto
            {
                FechaDesde = fechaDesdeFinal,
                FechaHasta = fechaHastaFinal,
                EstadisticasPorTipo = estadisticas
            });
        })
        .RequireAuthorization("FuncionarioOrAdmin")
        .WithName("GetEstadisticasEventos")
        .WithSummary("Obtener estadísticas agregadas de eventos")
        .Produces<ReporteEstadisticasDto>(200)
        .Produces(401)
        .Produces(403);

        return app;
    }

    private static Dictionary<string, object> ConvertirBsonADict(MongoDB.Bson.BsonDocument bsonDoc)
    {
        var dict = new Dictionary<string, object>();
        foreach (var element in bsonDoc)
        {
            var value = ConvertirBsonValue(element.Value);
            if (value != null)
            {
                dict[element.Name] = value;
            }
        }
        return dict;
    }

    private static object? ConvertirBsonValue(MongoDB.Bson.BsonValue value)
    {
        return value.BsonType switch
        {
            MongoDB.Bson.BsonType.String => value.AsString,
            MongoDB.Bson.BsonType.Int32 => value.AsInt32,
            MongoDB.Bson.BsonType.Int64 => value.AsInt64,
            MongoDB.Bson.BsonType.Double => value.AsDouble,
            MongoDB.Bson.BsonType.Boolean => value.AsBoolean,
            MongoDB.Bson.BsonType.DateTime => value.ToUniversalTime(),
            MongoDB.Bson.BsonType.Document => ConvertirBsonADict(value.AsBsonDocument),
            MongoDB.Bson.BsonType.Array => value.AsBsonArray.Select(ConvertirBsonValue).ToList(),
            _ => value.ToString()
        };
    }
}

