using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Application.Security;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string email, string password, string nombre, string apellido, string? telefono, Rol rol);
    Task<UserInfo?> ValidateTokenAsync(string token);
    Task<string?> RefreshTokenAsync(string token);
    Task<bool> LogoutAsync(string token);
}
