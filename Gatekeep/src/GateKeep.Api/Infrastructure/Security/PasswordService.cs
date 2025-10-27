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
        throw new NotImplementedException();
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        throw new NotImplementedException();
    }

    public bool ValidatePasswordStrength(string password)
    {
        throw new NotImplementedException();
    }
}
