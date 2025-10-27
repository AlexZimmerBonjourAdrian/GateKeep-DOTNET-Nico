using GateKeep.Api.Contracts.Security;
using GateKeep.Api.Application.Security;

namespace GateKeep.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Authentication");

        // Login endpoint
        group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return Results.BadRequest(new AuthResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Email y contraseña son requeridos"
                });
            }

            var result = await authService.LoginAsync(request.Email, request.Password);
            
            if (!result.IsSuccess)
            {
                return Results.Unauthorized();
            }

            var response = new AuthResponse
            {
                IsSuccess = true,
                Token = result.Token,
                RefreshToken = result.RefreshToken,
                ExpiresAt = result.ExpiresAt,
                User = new UserInfoResponse
                {
                    Id = result.User!.Id,
                    Email = result.User.Email,
                    Nombre = result.User.Nombre,
                    Apellido = result.User.Apellido,
                    TipoUsuario = result.User.TipoUsuario.ToString(),
                    Telefono = result.User.Telefono,
                    FechaAlta = result.User.FechaAlta
                }
            };

            return Results.Ok(response);
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
