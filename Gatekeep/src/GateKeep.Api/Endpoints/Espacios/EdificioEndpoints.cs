namespace GateKeep.Api.Endpoints.Espacios;

public static class EdificioEndpoints
{
    public static IEndpointRouteBuilder MapEdificioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/espacios/edificios").WithTags("Edificios");
        return app;
    }
}


