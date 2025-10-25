namespace GateKeep.Api.Endpoints.Usuarios;

public static class UsuarioProfileEndpoints
{
    public static IEndpointRouteBuilder MapUsuarioProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/perfil").WithTags("Perfil");
        return app;
    }
}


