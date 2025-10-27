using GateKeep.Api.Contracts.Security;
using GateKeep.Api.Application.Security;
using System.Security.Claims;

namespace GateKeep.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Authentication");

        // Login endpoint - PÚBLICO
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

        // Register endpoint - PÚBLICO
        group.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) ||
                string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.Apellido))
            {
                return Results.BadRequest(new AuthResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Todos los campos requeridos deben ser completados"
                });
            }

            if (request.Password != request.ConfirmPassword)
            {
                return Results.BadRequest(new AuthResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Las contraseñas no coinciden"
                });
            }

            var result = await authService.RegisterAsync(
                request.Email, 
                request.Password, 
                request.Nombre, 
                request.Apellido, 
                request.Telefono, 
                request.TipoUsuario
            );
            
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new AuthResponse
                {
                    IsSuccess = false,
                    ErrorMessage = result.ErrorMessage
                });
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
        .WithName("Register")
        .WithSummary("Registrar nuevo usuario")
        .WithDescription("Registra un nuevo usuario en el sistema")
        .Produces<AuthResponse>(200)
        .Produces(400);

        // Logout endpoint - REQUIERE AUTENTICACIÓN
        group.MapPost("/logout", (ClaimsPrincipal user, IAuthService authService) =>
        {
            // En un sistema real, aquí invalidarías el token en una blacklist
            // Por ahora, simplemente confirmamos que el usuario está autenticado
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = user.FindFirst(ClaimTypes.Email)?.Value;
            
            return Results.Ok(new { 
                message = "Sesión cerrada exitosamente",
                userId = userId,
                email = userEmail,
                timestamp = DateTime.UtcNow
            });
        })
        .RequireAuthorization("AllUsers")
        .WithName("Logout")
        .WithSummary("Cerrar sesión")
        .WithDescription("Cierra la sesión del usuario actual")
        .Produces<object>(200)
        .Produces(401)
        .Produces(403);

        // Refresh token endpoint - REQUIERE AUTENTICACIÓN
        group.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
        {
            var result = await authService.RefreshTokenAsync(request.Token);
            
            if (string.IsNullOrEmpty(result))
            {
                return Results.BadRequest(new AuthResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Token de renovación inválido"
                });
            }

            return Results.Ok(new AuthResponse
            {
                IsSuccess = true,
                Token = result,
                RefreshToken = request.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(8) // Configurar según tu configuración
            });
        })
        .RequireAuthorization("AllUsers")
        .WithName("RefreshToken")
        .WithSummary("Renovar token")
        .WithDescription("Renueva el token JWT usando el refresh token")
        .Produces<AuthResponse>(200)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // Debug endpoint - SOLO PARA DESARROLLO
        group.MapGet("/debug-token", (ClaimsPrincipal user) =>
        {
            if (user.Identity?.IsAuthenticated == true)
            {
                var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
                var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                
                return Results.Ok(new
                {
                    IsAuthenticated = true,
                    Name = user.Identity.Name,
                    AuthenticationType = user.Identity.AuthenticationType,
                    Claims = claims,
                    Roles = roles,
                    UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    Email = user.FindFirst(ClaimTypes.Email)?.Value,
                    Timestamp = DateTime.UtcNow
                });
            }
            return Results.Ok(new { 
                IsAuthenticated = false,
                Message = "No hay token válido o el usuario no está autenticado",
                Timestamp = DateTime.UtcNow
            });
        })
        .RequireAuthorization("AllUsers")
        .WithName("DebugToken")
        .WithSummary("Debug token info")
        .WithDescription("Información de debug del token JWT - Solo para desarrollo")
        .Produces<object>(200)
        .Produces(401)
        .Produces(403);

        return app;
    }
}
