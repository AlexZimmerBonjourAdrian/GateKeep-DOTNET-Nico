using GateKeep.Api.Contracts.Security;
using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Application.Security;
using GateKeep.Api.Application.Usuarios;
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

        // Crear usuarios de prueba - PÚBLICO (para testing)
        group.MapPost("/create-test-users", async (
            IUsuarioFactory factory, 
            IUsuarioRepository repo,
            IPasswordService passwordService) =>
        {
            var usuariosCreados = new List<object>();
            var usuariosExistentes = new List<object>();
            var totalCreados = 0;
            var totalExistentes = 0;

            // Datos de usuarios de prueba con contraseñas en texto plano
            var usuariosTest = new[]
            {
                // Administradores
                new { Email = "admin1@gatekeep.com", Nombre = "Admin", Apellido = "Uno", Telefono = "+1234567891", Password = "admin123", Tipo = "Admin" },
                new { Email = "admin2@gatekeep.com", Nombre = "Admin", Apellido = "Dos", Telefono = "+1234567892", Password = "admin123", Tipo = "Admin" },
                new { Email = "admin3@gatekeep.com", Nombre = "Admin", Apellido = "Tres", Telefono = "+1234567893", Password = "admin123", Tipo = "Admin" },
                
                // Estudiantes
                new { Email = "estudiante1@gatekeep.com", Nombre = "Juan", Apellido = "Pérez", Telefono = "+1234567894", Password = "estudiante123", Tipo = "Estudiante" },
                new { Email = "estudiante2@gatekeep.com", Nombre = "María", Apellido = "García", Telefono = "+1234567895", Password = "estudiante123", Tipo = "Estudiante" },
                new { Email = "estudiante3@gatekeep.com", Nombre = "Carlos", Apellido = "López", Telefono = "+1234567896", Password = "estudiante123", Tipo = "Estudiante" },
                new { Email = "estudiante4@gatekeep.com", Nombre = "Ana", Apellido = "Martínez", Telefono = "+1234567897", Password = "estudiante123", Tipo = "Estudiante" },
                new { Email = "estudiante5@gatekeep.com", Nombre = "Luis", Apellido = "Rodríguez", Telefono = "+1234567898", Password = "estudiante123", Tipo = "Estudiante" },
                
                // Funcionarios
                new { Email = "funcionario1@gatekeep.com", Nombre = "Roberto", Apellido = "Silva", Telefono = "+1234567899", Password = "funcionario123", Tipo = "Funcionario" },
                new { Email = "funcionario2@gatekeep.com", Nombre = "Patricia", Apellido = "Morales", Telefono = "+1234567900", Password = "funcionario123", Tipo = "Funcionario" },
                new { Email = "funcionario3@gatekeep.com", Nombre = "Fernando", Apellido = "Castro", Telefono = "+1234567901", Password = "funcionario123", Tipo = "Funcionario" },
                new { Email = "funcionario4@gatekeep.com", Nombre = "Isabel", Apellido = "Vargas", Telefono = "+1234567902", Password = "funcionario123", Tipo = "Funcionario" }
            };

            foreach (var usuarioTest in usuariosTest)
            {
                var existing = await repo.GetByEmailAsync(usuarioTest.Email);
                if (existing == null)
                {
                    // Crear usuario nuevo
                    var usuarioDto = new UsuarioDto
                    {
                        Email = usuarioTest.Email,
                        Nombre = usuarioTest.Nombre,
                        Apellido = usuarioTest.Apellido,
                        Contrasenia = passwordService.HashPassword(usuarioTest.Password),
                        Telefono = usuarioTest.Telefono,
                        TipoUsuario = usuarioTest.Tipo switch
                        {
                            "Admin" => GateKeep.Api.Domain.Enums.TipoUsuario.Admin,
                            "Estudiante" => GateKeep.Api.Domain.Enums.TipoUsuario.Estudiante,
                            "Funcionario" => GateKeep.Api.Domain.Enums.TipoUsuario.Funcionario,
                            _ => GateKeep.Api.Domain.Enums.TipoUsuario.Estudiante
                        }
                    };

                    var usuario = factory.CrearUsuario(usuarioDto);

                    await repo.AddAsync(usuario);
                    usuariosCreados.Add(new { 
                        Tipo = usuarioTest.Tipo, 
                        Id = usuario.Id, 
                        Email = usuario.Email, 
                        Nombre = usuario.Nombre, 
                        Apellido = usuario.Apellido,
                        Password = usuarioTest.Password,
                        Telefono = usuario.Telefono
                    });
                    totalCreados++;
                }
                else
                {
                    // Usuario ya existe - agregar a lista de existentes
                    usuariosExistentes.Add(new { 
                        Tipo = usuarioTest.Tipo, 
                        Id = existing.Id, 
                        Email = existing.Email, 
                        Nombre = existing.Nombre, 
                        Apellido = existing.Apellido,
                        Password = usuarioTest.Password,
                        Telefono = existing.Telefono
                    });
                    totalExistentes++;
                }
            }

            return Results.Ok(new
            {
                IsSuccess = true,
                Message = $"Proceso completado. Creados: {totalCreados}, Existentes: {totalExistentes}",
                UsuariosCreados = usuariosCreados,
                UsuariosExistentes = usuariosExistentes,
                Resumen = new
                {
                    TotalCreados = totalCreados,
                    TotalExistentes = totalExistentes,
                    TotalProcesados = totalCreados + totalExistentes
                }
            });
        })
        .WithName("CreateTestUsers")
        .WithSummary("Crear usuarios de prueba")
        .WithDescription("Crea usuarios de prueba de todos los tipos para testing")
        .Produces<AuthResponse>(200)
        .Produces(400);

        // Endpoint para listar usuarios con contraseñas en texto plano
        group.MapGet("/list-users", async (IUsuarioRepository repo) =>
        {
            var usuarios = await repo.GetAllAsync();
            
            // Mapear usuarios con contraseñas en texto plano para testing
            var usuariosConPasswords = usuarios.Select(u => new
            {
                Id = u.Id,
                Email = u.Email,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Telefono = u.Telefono,
                TipoUsuario = u.TipoUsuario.ToString(),
                FechaAlta = u.FechaAlta,
                Credencial = u.Credencial,
                // Contraseñas en texto plano para testing (solo para desarrollo)
                Password = GetPasswordForTesting(u.Email)
            }).ToList();

            return Results.Ok(new
            {
                IsSuccess = true,
                Message = $"Se encontraron {usuariosConPasswords.Count} usuarios",
                TotalUsuarios = usuariosConPasswords.Count,
                Usuarios = usuariosConPasswords
            });
        })
        .WithName("ListUsers")
        .WithSummary("Listar usuarios")
        .WithDescription("Lista todos los usuarios con contraseñas en texto plano para testing")
        .Produces(200)
        .Produces(400);

        return app;
    }

    // Método auxiliar para obtener contraseñas de testing
    private static string GetPasswordForTesting(string email)
    {
        // Mapeo de contraseñas para testing basado en el email
        return email switch
        {
            var e when e.Contains("admin") => "admin123",
            var e when e.Contains("estudiante") => "estudiante123",
            var e when e.Contains("funcionario") => "funcionario123",
            _ => "password123" // Contraseña por defecto
        };
    }
}
