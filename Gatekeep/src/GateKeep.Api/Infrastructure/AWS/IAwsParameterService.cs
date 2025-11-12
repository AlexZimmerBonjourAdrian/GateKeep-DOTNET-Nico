namespace GateKeep.Api.Infrastructure.AWS;

public interface IAwsParameterService
{
    Task<string> GetParameterAsync(string parameterName, bool withDecryption = false);
    Task PutParameterAsync(string parameterName, string value, bool overwrite = true);
    Task<List<string>> ListParametersAsync(string path);
}

