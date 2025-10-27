using GateKeep.Api.Application.Security;
using GateKeep.Api.Application.Usuarios;
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

    public Task<AuthResult> LoginAsync(string email, string password)
    {
        return Task.FromException<AuthResult>(new NotImplementedException());
    }

    public Task<AuthResult> RegisterAsync(string email, string password, string nombre, string apellido, string? telefono, TipoUsuario tipoUsuario)
    {
        return Task.FromException<AuthResult>(new NotImplementedException());
    }

    public Task<UserInfo?> ValidateTokenAsync(string token)
    {
        return Task.FromException<UserInfo?>(new NotImplementedException());
    }

    public Task<string?> RefreshTokenAsync(string token)
    {
        return Task.FromException<string?>(new NotImplementedException());
    }

    public Task<bool> LogoutAsync(string token)
    {
        return Task.FromException<bool>(new NotImplementedException());
    }

    private string GenerateJwtToken(Usuario usuario)
    {
        throw new NotImplementedException();
    }

    private ClaimsPrincipal? ValidateJwtToken(string token)
    {
        throw new NotImplementedException();
    }
}
