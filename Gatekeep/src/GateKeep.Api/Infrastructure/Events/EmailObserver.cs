using GateKeep.Api.Application.Events;

namespace GateKeep.Api.Infrastructure.Events;

/// <summary>
/// Observer que envía notificaciones por email cuando ocurren eventos en el sistema
/// </summary>
public sealed class EmailObserver : IEventObserver
{
    private readonly ILogger<EmailObserver>? _logger;

    public EmailObserver(ILogger<EmailObserver>? logger = null)
    {
        _logger = logger;
    }

    public async Task OnAccesoPermitidoAsync(long usuarioId, long espacioId, string puntoControl, DateTime fecha)
    {
        try
        {
            // Simulación de envío de email - En producción usar servicio de email real
            var asunto = "Acceso Permitido";
            var cuerpo = $"Tu acceso al espacio {espacioId} en {puntoControl} ha sido permitido el {fecha:yyyy-MM-dd HH:mm:ss}";
            
            await EnviarEmailAsync(usuarioId, asunto, cuerpo);
            
            _logger?.LogInformation($"Email enviado a usuario {usuarioId}: {asunto}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error enviando email de acceso permitido a usuario {usuarioId}");
        }
    }

    public async Task OnAccesoRechazadoAsync(long usuarioId, long? espacioId, string razon, string puntoControl, DateTime fecha)
    {
        try
        {
            var asunto = "Acceso Rechazado";
            var cuerpo = $"Tu intento de acceso al espacio {espacioId} en {puntoControl} fue rechazado el {fecha:yyyy-MM-dd HH:mm:ss}. Razón: {razon}";
            
            await EnviarEmailAsync(usuarioId, asunto, cuerpo);
            
            _logger?.LogInformation($"Email enviado a usuario {usuarioId}: {asunto}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error enviando email de acceso rechazado a usuario {usuarioId}");
        }
    }

    public async Task OnCambioRolAsync(long usuarioId, string rolAnterior, string rolNuevo, long? modificadoPor, DateTime fecha)
    {
        try
        {
            var asunto = "Cambio de Rol";
            var cuerpo = $"Tu rol ha sido cambiado de {rolAnterior} a {rolNuevo} el {fecha:yyyy-MM-dd HH:mm:ss}";
            
            await EnviarEmailAsync(usuarioId, asunto, cuerpo);
            
            _logger?.LogInformation($"Email enviado a usuario {usuarioId}: {asunto}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error enviando email de cambio de rol a usuario {usuarioId}");
        }
    }

    public async Task OnUsuarioCreadoAsync(long usuarioId, string email, string nombre, string apellido, string rol, DateTime fecha)
    {
        try
        {
            var asunto = "Bienvenido a GateKeep";
            var cuerpo = $"Hola {nombre} {apellido},\n\nTu cuenta ha sido creada exitosamente con el rol: {rol}.\n\nFecha de creación: {fecha:yyyy-MM-dd HH:mm:ss}";
            
            await EnviarEmailAsync(usuarioId, asunto, cuerpo, email);
            
            _logger?.LogInformation($"Email de bienvenida enviado a {email}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error enviando email de bienvenida a usuario {usuarioId}");
        }
    }

    public async Task OnBeneficioAsignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha)
    {
        try
        {
            var asunto = "Nuevo Beneficio Asignado";
            var cuerpo = $"Se te ha asignado el beneficio: {beneficioNombre} el {fecha:yyyy-MM-dd HH:mm:ss}";
            
            await EnviarEmailAsync(usuarioId, asunto, cuerpo);
            
            _logger?.LogInformation($"Email enviado a usuario {usuarioId}: {asunto}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error enviando email de beneficio asignado a usuario {usuarioId}");
        }
    }

    public async Task OnBeneficioDesasignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha)
    {
        try
        {
            var asunto = "Beneficio Desasignado";
            var cuerpo = $"El beneficio {beneficioNombre} ha sido desasignado de tu cuenta el {fecha:yyyy-MM-dd HH:mm:ss}";
            
            await EnviarEmailAsync(usuarioId, asunto, cuerpo);
            
            _logger?.LogInformation($"Email enviado a usuario {usuarioId}: {asunto}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error enviando email de beneficio desasignado a usuario {usuarioId}");
        }
    }

    private async Task EnviarEmailAsync(long usuarioId, string asunto, string cuerpo, string? email = null)
    {
        // Simulación de envío de email - En producción implementar servicio real
        // Ejemplo: usar SendGrid, SMTP, AWS SES, etc.
        
        await Task.Delay(100); // Simula latencia de red
        
        // En producción, aquí iría el código real:
        // await _emailService.SendAsync(email ?? usuario.Email, asunto, cuerpo);
        
        Console.WriteLine($"[EMAIL] Para usuario {usuarioId}: {asunto}");
        Console.WriteLine($"[EMAIL] Cuerpo: {cuerpo}");
    }
}

