using GateKeep.Api.Contracts.Security;
using GateKeep.Api.Application.Security;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Authentication");

        group.MapPost("/login", (LoginRequest request, IAuthService authService) =>
        {
            return Results.StatusCode(501);
        })
        .WithName("Login")
        .WithSummary("Iniciar sesión")
        .WithDescription("Autentica un usuario y retorna un token JWT")
        .Produces<AuthResponse>(200)
        .Produces(401)
        .Produces(400);

        return app;
    }
}
