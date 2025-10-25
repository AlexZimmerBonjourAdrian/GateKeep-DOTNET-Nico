namespace GateKeep.Api.Endpoints.Shared;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/health").WithTags("System");
        return app;
    }
}


