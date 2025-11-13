using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Infrastructure.AWS;

public class AwsParameterService : IAwsParameterService
{
    private readonly IAmazonSimpleSystemsManagement _ssm;
    private readonly ILogger<AwsParameterService> _logger;

    public AwsParameterService(
        IAmazonSimpleSystemsManagement ssm,
        ILogger<AwsParameterService> logger)
    {
        _ssm = ssm;
        _logger = logger;
    }

    public async Task<string> GetParameterAsync(string parameterName, bool withDecryption = false)
    {
        try
        {
            _logger.LogInformation("Obteniendo parámetro: {ParameterName}", parameterName);

            var request = new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = withDecryption
            };

            var response = await _ssm.GetParameterAsync(request);
            
            _logger.LogInformation("Parámetro obtenido exitosamente: {ParameterName}", parameterName);
            
            return response.Parameter.Value;
        }
        catch (ParameterNotFoundException ex)
        {
            _logger.LogError(ex, "Parámetro no encontrado: {ParameterName}", parameterName);
            throw new InvalidOperationException($"Parámetro '{parameterName}' no encontrado en AWS Parameter Store", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo parámetro {ParameterName}", parameterName);
            throw;
        }
    }

    public async Task PutParameterAsync(string parameterName, string value, bool overwrite = true)
    {
        try
        {
            _logger.LogInformation("Guardando parámetro: {ParameterName}", parameterName);

            var request = new PutParameterRequest
            {
                Name = parameterName,
                Value = value,
                Type = ParameterType.String,
                Overwrite = overwrite
            };

            await _ssm.PutParameterAsync(request);
            
            _logger.LogInformation("Parámetro guardado exitosamente: {ParameterName}", parameterName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando parámetro {ParameterName}", parameterName);
            throw;
        }
    }

    public async Task<List<string>> ListParametersAsync(string path)
    {
        try
        {
            _logger.LogInformation("Listando parámetros con path: {Path}", path);

            var request = new GetParametersByPathRequest
            {
                Path = path,
                Recursive = true
            };

            var response = await _ssm.GetParametersByPathAsync(request);
            
            var parameterNames = response.Parameters.Select(p => p.Name).ToList();
            
            _logger.LogInformation("Encontrados {Count} parámetros en path {Path}", parameterNames.Count, path);
            
            return parameterNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listando parámetros con path {Path}", path);
            throw;
        }
    }
}

