namespace GateKeep.Api.Endpoints.Espacios;

public static class EspacioEndpoints
{
    public static IEndpointRouteBuilder MapEspacioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/espacios").WithTags("Espacios");
        return app;
    }
}


