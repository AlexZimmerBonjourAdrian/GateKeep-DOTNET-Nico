namespace GateKeep.Api.Infrastructure.AWS;

public interface IAwsSecretsService
{
    Task<string> GetSecretAsync(string secretName);
    Task<string> GetSecretValueAsync(string secretName, string key);
}

