using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Application.Security;

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public UserInfo? User { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public static AuthResult Success(string token, UserInfo user, string? refreshToken = null, DateTime? expiresAt = null)
    {
        return new AuthResult
        {
            IsSuccess = true,
            Token = token,
            RefreshToken = refreshToken,
            User = user,
            ExpiresAt = expiresAt
        };
    }

    public static AuthResult Failed(string errorMessage)
    {
        return new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

public class UserInfo
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public Rol Rol { get; set; }
    public string? Telefono { get; set; }
    public DateTime FechaAlta { get; set; }
}
