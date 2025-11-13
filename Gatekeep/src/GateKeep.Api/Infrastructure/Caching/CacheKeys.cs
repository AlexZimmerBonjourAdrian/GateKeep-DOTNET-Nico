namespace GateKeep.Api.Infrastructure.Caching;

/// <summary>
/// Constantes para las claves de cache y TTL
/// </summary>
public static class CacheKeys
{
    // Prefijos para las claves
    private const string BeneficiosPrefix = "beneficios";
    private const string ReglasAccesoPrefix = "reglas-acceso";
    private const string UsuariosPrefix = "usuarios";
    
    // Claves específicas para Beneficios
    public static string AllBeneficios => $"{BeneficiosPrefix}:all";
    public static string BeneficioById(long id) => $"{BeneficiosPrefix}:{id}";
    public static string BeneficiosVigentes => $"{BeneficiosPrefix}:vigentes";
    public static string BeneficiosPattern => $"{BeneficiosPrefix}:*";
    
    // Claves específicas para Reglas de Acceso
    public static string AllReglasAcceso => $"{ReglasAccesoPrefix}:all";
    public static string ReglaAccesoById(long id) => $"{ReglasAccesoPrefix}:{id}";
    public static string ReglasAccesoActivas => $"{ReglasAccesoPrefix}:activas";
    public static string ReglasAccesoPattern => $"{ReglasAccesoPrefix}:*";
    
    // Claves para Usuarios
    public static string UsuarioById(long id) => $"{UsuariosPrefix}:{id}";
    public static string UsuarioByEmail(string email) => $"{UsuariosPrefix}:email:{email}";
    
    // TTL (Time To Live) por tipo de dato
    public static class TTL
    {
        /// <summary>
        /// Beneficios vigentes: 5 minutos (datos que cambian con frecuencia media)
        /// </summary>
        public static readonly TimeSpan Beneficios = TimeSpan.FromMinutes(5);
        
        /// <summary>
        /// Reglas de acceso: 10 minutos (datos más estables)
        /// </summary>
        public static readonly TimeSpan ReglasAcceso = TimeSpan.FromMinutes(10);
        
        /// <summary>
        /// Usuarios: 15 minutos (datos relativamente estables)
        /// </summary>
        public static readonly TimeSpan Usuarios = TimeSpan.FromMinutes(15);
        
        /// <summary>
        /// Cache de corta duración: 1 minuto
        /// </summary>
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(1);
        
        /// <summary>
        /// Cache de larga duración: 30 minutos
        /// </summary>
        public static readonly TimeSpan Long = TimeSpan.FromMinutes(30);
    }
}

