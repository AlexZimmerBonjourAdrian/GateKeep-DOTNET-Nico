using GateKeep.Api.Infrastructure.AWS;
using Microsoft.AspNetCore.Mvc;

namespace GateKeep.Api.Endpoints.AWS;

public static class AwsTestEndpoint
{
    public static void MapAwsTestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aws")
            .WithTags("AWS")
            .WithOpenApi();

        // Endpoint para probar Secrets Manager
        group.MapGet("/secrets/{secretName}", GetSecret)
            .WithName("GetSecret")
            .WithSummary("Obtener un secret de AWS Secrets Manager")
            .Produces<string>(200)
            .Produces(404)
            .Produces(500);

        // Endpoint para probar Parameter Store
        group.MapGet("/parameters/{parameterName}", GetParameter)
            .WithName("GetParameter")
            .WithSummary("Obtener un parámetro de AWS Parameter Store")
            .Produces<string>(200)
            .Produces(404)
            .Produces(500);

        // Endpoint para listar parámetros
        group.MapGet("/parameters", ListParameters)
            .WithName("ListParameters")
            .WithSummary("Listar parámetros de AWS Parameter Store")
            .Produces<List<string>>(200)
            .Produces(500);
    }

    private static async Task<IResult> GetSecret(
        string secretName,
        IAwsSecretsService secretsService)
    {
        try
        {
            var secret = await secretsService.GetSecretAsync(secretName);
            return Results.Ok(new { secretName, secret });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetParameter(
        string parameterName,
        IAwsParameterService parameterService)
    {
        try
        {
            // Asegurar que el parámetro tenga el prefijo /
            if (!parameterName.StartsWith("/"))
            {
                parameterName = "/" + parameterName;
            }

            var parameter = await parameterService.GetParameterAsync(parameterName);
            return Results.Ok(new { parameterName, value = parameter });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> ListParameters(
        [FromQuery] string? path,
        IAwsParameterService parameterService)
    {
        try
        {
            var searchPath = path ?? "/gatekeep";
            var parameters = await parameterService.ListParametersAsync(searchPath);
            return Results.Ok(new { path = searchPath, parameters });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500);
        }
    }
}

