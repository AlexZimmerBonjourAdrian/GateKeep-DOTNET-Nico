namespace GateKeep.Api.Endpoints.Usuarios;

public static class UsuarioAuthEndpoints
{
    public static IEndpointRouteBuilder MapUsuarioAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");
        return app;
    }
}


