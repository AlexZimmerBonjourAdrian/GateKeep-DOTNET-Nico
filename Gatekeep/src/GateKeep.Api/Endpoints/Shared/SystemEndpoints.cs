namespace GateKeep.Api.Endpoints.Shared;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/system").WithTags("System");
        return app;
    }
}


