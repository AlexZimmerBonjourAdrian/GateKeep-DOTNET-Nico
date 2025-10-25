namespace GateKeep.Api.Endpoints.Usuarios;

public static class UsuarioEndpoints
{
    public static IEndpointRouteBuilder MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/usuarios").WithTags("Usuarios");
        return app;
    }
}


