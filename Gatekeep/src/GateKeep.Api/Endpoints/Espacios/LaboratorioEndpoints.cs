namespace GateKeep.Api.Endpoints.Espacios;

public static class LaboratorioEndpoints
{
    public static IEndpointRouteBuilder MapLaboratorioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/espacios/laboratorios").WithTags("Laboratorios");
        return app;
    }
}


