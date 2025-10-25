namespace GateKeep.Api.Endpoints.Shared;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");
        return app;
    }
}


