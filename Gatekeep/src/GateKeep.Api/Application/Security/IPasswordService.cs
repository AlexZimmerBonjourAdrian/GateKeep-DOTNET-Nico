namespace GateKeep.Api.Application.Security;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    bool ValidatePasswordStrength(string password);
}
