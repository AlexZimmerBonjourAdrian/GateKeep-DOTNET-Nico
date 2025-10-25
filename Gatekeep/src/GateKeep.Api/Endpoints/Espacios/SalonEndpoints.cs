namespace GateKeep.Api.Endpoints.Espacios;

public static class SalonEndpoints
{
    public static IEndpointRouteBuilder MapSalonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/espacios/salones").WithTags("Salones");
        return app;
    }
}


