using Microsoft.Extensions.Configuration;
using BCrypt.Net;

namespace GateKeep.Api.Application.Security;

public class PasswordService : IPasswordService
{
    private readonly IConfiguration _configuration;

    public PasswordService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

        // Usar BCrypt para hash seguro
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            return false;

        try
        {
            // Verificar contraseña usando BCrypt
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            // En caso de error en la verificación, retornar false
            return false;
        }
    }

    public bool ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        // Obtener configuración de seguridad desde config.json
        var minLength = _configuration.GetValue<int>("security:passwordMinLength", 8);
        
        // Validaciones básicas de fortaleza de contraseña
        var hasMinLength = password.Length >= minLength;
        var hasDigit = password.Any(char.IsDigit);
        var hasLetter = password.Any(char.IsLetter);
        var hasUpperOrLower = password.Any(char.IsUpper) || password.Any(char.IsLower);
        
        // La contraseña debe cumplir todos los criterios
        return hasMinLength && hasDigit && hasLetter && hasUpperOrLower;
    }
}
