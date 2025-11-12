using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GateKeep.Api.Infrastructure.AWS;

public class AwsSecretsService : IAwsSecretsService
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly ILogger<AwsSecretsService> _logger;

    public AwsSecretsService(
        IAmazonSecretsManager secretsManager,
        ILogger<AwsSecretsService> logger)
    {
        _secretsManager = secretsManager;
        _logger = logger;
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            _logger.LogInformation("Obteniendo secret: {SecretName}", secretName);

            var request = new GetSecretValueRequest
            {
                SecretId = secretName
            };

            var response = await _secretsManager.GetSecretValueAsync(request);
            
            _logger.LogInformation("Secret obtenido exitosamente: {SecretName}", secretName);
            
            return response.SecretString;
        }
        catch (ResourceNotFoundException ex)
        {
            _logger.LogError(ex, "Secret no encontrado: {SecretName}", secretName);
            throw new InvalidOperationException($"Secret '{secretName}' no encontrado en AWS Secrets Manager", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo secret {SecretName}", secretName);
            throw;
        }
    }

    public async Task<string> GetSecretValueAsync(string secretName, string key)
    {
        try
        {
            var secretString = await GetSecretAsync(secretName);
            
            // Intentar parsear como JSON si es un objeto
            try
            {
                var jsonDoc = JsonDocument.Parse(secretString);
                if (jsonDoc.RootElement.TryGetProperty(key, out var property))
                {
                    return property.GetString() ?? string.Empty;
                }
            }
            catch (JsonException)
            {
                // Si no es JSON, retornar el string completo
                _logger.LogWarning("Secret {SecretName} no es JSON, retornando valor completo", secretName);
            }

            return secretString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo valor del secret {SecretName} con key {Key}", secretName, key);
            throw;
        }
    }
}

