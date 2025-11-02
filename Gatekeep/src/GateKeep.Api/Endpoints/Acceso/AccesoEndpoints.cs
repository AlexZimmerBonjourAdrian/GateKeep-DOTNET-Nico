using GateKeep.Api.Application.Acceso;
using GateKeep.Api.Contracts.Acceso;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Acceso;

public static class AccesoEndpoints
{
    public static IEndpointRouteBuilder MapAccesoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/acceso")
            .WithTags("Acceso")
            .WithOpenApi();

        group.MapPost("/validar", async (
            [FromBody] ValidarAccesoRequest request,
            ClaimsPrincipal user,
            [FromServices] IAccesoService accesoService) =>
        {
            var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (userId != request.UsuarioId && userRole != "Funcionario" && userRole != "Admin")
                return Results.Forbid();

            if (request.UsuarioId <= 0)
            {
                var errorResponse = new ErrorAccesoResponse
                {
                    TipoError = "VALIDACION",
                    Mensaje = "El ID de usuario debe ser mayor a 0",
                    CodigoError = "USUARIO_ID_INVALIDO",
                    UsuarioId = request.UsuarioId,
                    EspacioId = request.EspacioId,
                    PuntoControl = request.PuntoControl
                };
                return Results.BadRequest(errorResponse);
            }

            if (request.EspacioId <= 0)
            {
                var errorResponse = new ErrorAccesoResponse
                {
                    TipoError = "VALIDACION",
                    Mensaje = "El ID de espacio debe ser mayor a 0",
                    CodigoError = "ESPACIO_ID_INVALIDO",
                    UsuarioId = request.UsuarioId,
                    EspacioId = request.EspacioId,
                    PuntoControl = request.PuntoControl
                };
                return Results.BadRequest(errorResponse);
            }

            if (string.IsNullOrWhiteSpace(request.PuntoControl))
            {
                var errorResponse = new ErrorAccesoResponse
                {
                    TipoError = "VALIDACION",
                    Mensaje = "El punto de control es requerido y no puede estar vacío",
                    CodigoError = "PUNTO_CONTROL_REQUERIDO",
                    UsuarioId = request.UsuarioId,
                    EspacioId = request.EspacioId,
                    PuntoControl = request.PuntoControl
                };
                return Results.BadRequest(errorResponse);
            }

            try
            {
                var resultado = await accesoService.ValidarAccesoAsync(
                    request.UsuarioId,
                    request.EspacioId,
                    request.PuntoControl);

                if (resultado.Permitido)
                {
                    var response = new ValidarAccesoResponse
                    {
                        Permitido = true,
                        Razon = null,
                        UsuarioId = request.UsuarioId,
                        EspacioId = request.EspacioId,
                        PuntoControl = request.PuntoControl,
                        Fecha = DateTime.UtcNow
                    };
                    return Results.Ok(response);
                }

                var codigoError = resultado.TipoError switch
                {
                    TipoErrorAcceso.UsuarioNoExiste => "USUARIO_NO_EXISTE",
                    TipoErrorAcceso.UsuarioInvalido => "USUARIO_INVALIDO",
                    TipoErrorAcceso.EspacioNoExiste => "ESPACIO_NO_EXISTE",
                    TipoErrorAcceso.EspacioInactivo => "ESPACIO_INACTIVO",
                    TipoErrorAcceso.ReglasNoConfiguradas => "REGLAS_NO_CONFIGURADAS",
                    TipoErrorAcceso.FueraDeHorario => "FUERA_DE_HORARIO",
                    TipoErrorAcceso.FueraDeVigencia => "FUERA_DE_VIGENCIA",
                    TipoErrorAcceso.RolNoPermitido => "ROL_NO_PERMITIDO",
                    _ => "ACCESO_DENEGADO"
                };

                var statusCode = resultado.TipoError switch
                {
                    TipoErrorAcceso.UsuarioNoExiste => 404,
                    TipoErrorAcceso.EspacioNoExiste => 404,
                    TipoErrorAcceso.EspacioInactivo => 403,
                    TipoErrorAcceso.UsuarioInvalido => 403,
                    TipoErrorAcceso.ReglasNoConfiguradas => 412,
                    TipoErrorAcceso.FueraDeHorario => 403,
                    TipoErrorAcceso.FueraDeVigencia => 403,
                    TipoErrorAcceso.RolNoPermitido => 403,
                    _ => 400
                };

                var tipoErrorString = resultado.TipoError.ToString();

                var errorResponseDetail = new ErrorAccesoResponse
                {
                    TipoError = tipoErrorString,
                    Mensaje = resultado.Razon ?? "Acceso denegado",
                    CodigoError = codigoError,
                    UsuarioId = request.UsuarioId,
                    EspacioId = request.EspacioId,
                    PuntoControl = request.PuntoControl,
                    DetallesAdicionales = resultado.DetallesAdicionales
                };

                return statusCode switch
                {
                    404 => Results.NotFound(errorResponseDetail),
                    403 => Results.Json(errorResponseDetail, statusCode: 403),
                    412 => Results.Problem(
                        detail: errorResponseDetail.Mensaje,
                        statusCode: 412,
                        title: errorResponseDetail.TipoError,
                        type: errorResponseDetail.CodigoError,
                        extensions: new Dictionary<string, object?>
                        {
                            { "errorResponse", errorResponseDetail }
                        }),
                    _ => Results.BadRequest(errorResponseDetail)
                };
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorAccesoResponse
                {
                    TipoError = "ERROR_INTERNO",
                    Mensaje = $"Error inesperado al validar el acceso: {ex.Message}",
                    CodigoError = "ERROR_SERVIDOR",
                    UsuarioId = request.UsuarioId,
                    EspacioId = request.EspacioId,
                    PuntoControl = request.PuntoControl
                };
                return Results.Problem(
                    detail: errorResponse.Mensaje,
                    statusCode: 500,
                    title: errorResponse.TipoError,
                    type: errorResponse.CodigoError);
            }
        })
        .RequireAuthorization("AllUsers")
        .WithName("ValidarAcceso")
        .WithSummary("Validar acceso de un usuario a un espacio")
        .WithDescription("Valida si un usuario tiene permisos para acceder a un espacio en un punto de control específico. Devuelve respuestas estructuradas con códigos de error claros para cada tipo de rechazo.")
        .Produces<ValidarAccesoResponse>(200, "application/json")
        .Produces<ErrorAccesoResponse>(400, "application/json")
        .Produces<ErrorAccesoResponse>(403, "application/json")
        .Produces<ErrorAccesoResponse>(404, "application/json")
        .Produces<ErrorAccesoResponse>(412, "application/json")
        .Produces(401)
        .Produces(500);

        return app;
    }
}

