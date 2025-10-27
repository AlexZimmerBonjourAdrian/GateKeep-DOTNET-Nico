using GateKeep.Api.Application.Security;
using GateKeep.Api.Application.Usuarios;
using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace GateKeep.Api.Infrastructure.Security;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUsuarioFactory _usuarioFactory;
    private readonly IPasswordService _passwordService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IUsuarioFactory usuarioFactory,
        IPasswordService passwordService,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _usuarioFactory = usuarioFactory;
        _passwordService = passwordService;
        _configuration = configuration;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            // Validar entrada
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return AuthResult.Failed("Email y contraseña son requeridos");

            // Buscar usuario por email
            var usuario = await _usuarioRepository.GetByEmailAsync(email);
            if (usuario == null)
                return AuthResult.Failed("Credenciales inválidas");

            // Verificar contraseña
            if (!_passwordService.VerifyPassword(password, usuario.Contrasenia))
                return AuthResult.Failed("Credenciales inválidas");

            // Verificar si el usuario está activo
            if (usuario.Credencial != TipoCredencial.Vigente)
                return AuthResult.Failed("Usuario inactivo");

            // Generar token JWT
            var token = GenerateJwtToken(usuario);
            var expiresAt = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("jwt:expirationHours", 8));

            // Crear información del usuario
            var userInfo = new UserInfo
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                TipoUsuario = Enum.Parse<TipoUsuario>(usuario.TipoUsuario),
                Telefono = usuario.Telefono,
                FechaAlta = usuario.FechaAlta
            };

            var refreshToken = GenerateRefreshToken();
            return AuthResult.Success(token, userInfo, refreshToken: refreshToken, expiresAt: expiresAt);
        }
        catch (Exception ex)
        {
            return AuthResult.Failed($"Error durante el login: {ex.Message}");
        }
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string nombre, string apellido, string? telefono, TipoUsuario tipoUsuario)
    {
        try
        {
            // Validar entrada
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || 
                string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido))
                return AuthResult.Failed("Todos los campos requeridos deben ser completados");

            // Validar fortaleza de contraseña
            if (!_passwordService.ValidatePasswordStrength(password))
                return AuthResult.Failed("La contraseña no cumple con los requisitos de seguridad");

            // Verificar si el email ya existe
            var existingUser = await _usuarioRepository.GetByEmailAsync(email);
            if (existingUser != null)
                return AuthResult.Failed("El email ya está registrado");

            // Crear usuario usando factory
            var usuarioDto = new UsuarioDto
            {
                Id = 0,
                Email = email,
                Nombre = nombre,
                Apellido = apellido,
                Contrasenia = _passwordService.HashPassword(password),
                Telefono = telefono,
                FechaAlta = DateTime.UtcNow,
                Credencial = TipoCredencial.Vigente
            };

            Usuario nuevoUsuario = tipoUsuario switch
            {
                TipoUsuario.Admin => _usuarioFactory.CrearAdmin(usuarioDto),
                TipoUsuario.Estudiante => _usuarioFactory.CrearEstudiante(usuarioDto),
                TipoUsuario.Funcionario => _usuarioFactory.CrearFuncionario(usuarioDto),
                _ => throw new ArgumentException("Tipo de usuario no válido")
            };

            // Guardar usuario
            await _usuarioRepository.AddAsync(nuevoUsuario);

            // Generar token JWT
            var token = GenerateJwtToken(nuevoUsuario);
            var expiresAt = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("jwt:expirationHours", 8));

            // Crear información del usuario
            var userInfo = new UserInfo
            {
                Id = nuevoUsuario.Id,
                Email = nuevoUsuario.Email,
                Nombre = nuevoUsuario.Nombre,
                Apellido = nuevoUsuario.Apellido,
                TipoUsuario = Enum.Parse<TipoUsuario>(nuevoUsuario.TipoUsuario),
                Telefono = nuevoUsuario.Telefono,
                FechaAlta = nuevoUsuario.FechaAlta
            };

            var refreshToken = GenerateRefreshToken();
            return AuthResult.Success(token, userInfo, refreshToken: refreshToken, expiresAt: expiresAt);
        }
        catch (Exception ex)
        {
            return AuthResult.Failed($"Error durante el registro: {ex.Message}");
        }
    }

    public async Task<UserInfo?> ValidateTokenAsync(string token)
    {
        try
        {
            var principal = ValidateJwtToken(token);
            if (principal == null)
                return null;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                return null;

            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario == null || usuario.Credencial != TipoCredencial.Vigente)
                return null;

            return new UserInfo
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                TipoUsuario = Enum.Parse<TipoUsuario>(usuario.TipoUsuario),
                Telefono = usuario.Telefono,
                FechaAlta = usuario.FechaAlta
            };
        }
        catch
        {
            return null;
        }
    }

    public Task<string?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Validar que el refresh token no esté vacío
            if (string.IsNullOrEmpty(refreshToken))
                return Task.FromResult<string?>(null);

            // En una implementación real, aquí validarías el refresh token contra una base de datos
            // y verificarías que no haya expirado. Por ahora, implementación básica.
            
            // Generar un nuevo JWT token
            // Nota: En producción, deberías extraer el userId del refresh token almacenado
            // Por ahora, generamos un token genérico
            var jwtConfig = _configuration.GetSection("jwt");
            var jwtKey = jwtConfig["key"] ?? throw new InvalidOperationException("JWT Key no configurada");
            var jwtIssuer = jwtConfig["issuer"] ?? "GateKeep";
            var jwtAudience = jwtConfig["audience"] ?? "GateKeepUsers";
            var expirationHours = jwtConfig.GetValue<int>("expirationHours", 8);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims básicos para el nuevo token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"), // En producción, usar el userId real
                new Claim(ClaimTypes.Email, "user@example.com"), // En producción, usar el email real
                new Claim(ClaimTypes.Name, "Usuario"),
                new Claim(ClaimTypes.Role, "Estudiante"),
                new Claim("TipoUsuario", "Estudiante"),
                new Claim("Telefono", ""),
                new Claim("FechaAlta", DateTime.UtcNow.ToString("O"))
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials
            );

            return Task.FromResult<string?>(new JwtSecurityTokenHandler().WriteToken(token));
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }

    public Task<bool> LogoutAsync(string token)
    {
        // Implementación básica - en producción invalidar token en blacklist
        return Task.FromResult(true);
    }

    private string GenerateJwtToken(Usuario usuario)
    {
        var jwtConfig = _configuration.GetSection("jwt");
        var jwtKey = jwtConfig["key"] ?? throw new InvalidOperationException("JWT Key no configurada");
        var jwtIssuer = jwtConfig["issuer"] ?? "GateKeep";
        var jwtAudience = jwtConfig["audience"] ?? "GateKeepUsers";
        var expirationHours = jwtConfig.GetValue<int>("expirationHours", 8);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
            new Claim(ClaimTypes.Role, usuario.TipoUsuario.ToString()),
            new Claim("TipoUsuario", usuario.TipoUsuario.ToString()),
            new Claim("Telefono", usuario.Telefono ?? ""),
            new Claim("FechaAlta", usuario.FechaAlta.ToString("O"))
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidateJwtToken(string token)
    {
        try
        {
            var jwtConfig = _configuration.GetSection("jwt");
            var jwtKey = jwtConfig["key"] ?? throw new InvalidOperationException("JWT Key no configurada");
            var jwtIssuer = jwtConfig["issuer"] ?? "GateKeep";
            var jwtAudience = jwtConfig["audience"] ?? "GateKeepUsers";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateRefreshToken()
    {
        // Generar un refresh token único usando GUID y timestamp
        return Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks.ToString("X");
    }
}
