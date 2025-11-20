using System.Text.Json.Serialization;
using GateKeep.Api.Application.Acceso;
using GateKeep.Api.Application.Anuncios;
using GateKeep.Api.Application.Auditoria;
using GateKeep.Api.Application.Beneficios;
using GateKeep.Api.Application.Espacios;
using GateKeep.Api.Application.Eventos;
using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Application.Security;
using GateKeep.Api.Application.Events;
using GateKeep.Api.Application.Usuarios;
using GateKeep.Api.Application.Sync;
using MassTransit;
using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Domain.Enums;
using GateKeep.Api.Endpoints.Acceso;
using GateKeep.Api.Endpoints.Anuncios;
using GateKeep.Api.Endpoints.Auditoria;
using GateKeep.Api.Endpoints.Auth;
using GateKeep.Api.Endpoints.Beneficios;
using GateKeep.Api.Endpoints.Espacios;
using GateKeep.Api.Endpoints.Eventos;
using GateKeep.Api.Endpoints.Notificaciones;
using GateKeep.Api.Endpoints.Usuarios;
using GateKeep.Api.Endpoints.Shared;
using GateKeep.Api.Infrastructure.Acceso;
using GateKeep.Api.Infrastructure.Anuncios;
using GateKeep.Api.Infrastructure.Auditoria;
using GateKeep.Api.Infrastructure.Beneficios;
using GateKeep.Api.Infrastructure.Caching;
using GateKeep.Api.Infrastructure.Espacios;
using GateKeep.Api.Infrastructure.Eventos;
using GateKeep.Api.Infrastructure.Events;
using GateKeep.Api.Infrastructure.Notificaciones;
using GateKeep.Api.Infrastructure.Queues;
using GateKeep.Api.Infrastructure.Sync;
using GateKeep.Api.Application.Queues;
using GateKeep.Api.Infrastructure.Persistence;
using GateKeep.Api.Infrastructure.Security;
using GateKeep.Api.Infrastructure.Usuarios;
using GateKeep.Api.Infrastructure.Observability;
using GateKeep.Infrastructure.QrCodes;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using System.Reflection;
using StackExchange.Redis;
using Serilog;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SimpleSystemsManagement;
using Amazon.CloudWatch;
using GateKeep.Api.Infrastructure.AWS;
using GateKeep.Api.Endpoints.AWS;
using Serilog.Events;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

Log.Information("Iniciando GateKeep.Api");

try
{
    // Cargar variables de entorno desde archivo .env SOLO si estamos en desarrollo local
    // En Docker, las variables ya están en el entorno del contenedor (pasadas por docker-compose)
    // Docker Compose lee automáticamente el .env del host y lo pasa al contenedor
    if (!Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
    {
        // Estamos ejecutando localmente, intentar cargar .env
        var envPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));
        
        if (File.Exists(envPath))
        {
            DotNetEnv.Env.Load(envPath);
            Log.Information("Variables de entorno cargadas desde: {EnvPath}", envPath);
        }
        else
        {
            Log.Warning("Archivo .env no encontrado en: {EnvPath}", envPath);
            Log.Warning("Usando variables de entorno del sistema o valores por defecto.");
        }
    }
    else
    {
        Log.Information("Ejecutando en contenedor Docker - usando variables de entorno del contenedor");
    }
    
    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog desde appsettings.json
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "GateKeep.Api"));

// Cargar config.json: hacerlo opcional para entornos Docker donde montamos config.Production.json
builder.Configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true);

// Cargar configuración específica para producción SOLO en producción
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddJsonFile("config.Production.json", optional: true, reloadOnChange: true);
}

// Permitir sobreescritura por variables de entorno
builder.Configuration.AddEnvironmentVariables();

// Configurar puerto desde variable de entorno o config
var port = Environment.GetEnvironmentVariable("GATEKEEP_PORT") 
    ?? builder.Configuration.GetSection("application")["urls"]?.Split(':').LastOrDefault()
    ?? "5011";

// Si el puerto viene como URL completa (ej: http://localhost:5011), extraer solo el número
if (port.Contains("://"))
{
    port = port.Split(':').LastOrDefault() ?? "5011";
}

// En Docker, ASPNETCORE_URLS contiene "+" (ej: "http://+:5011"), usar 0.0.0.0 para escuchar en todas las interfaces
// En local, usar localhost
var listenAddress = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Contains("+") == true 
    ? "0.0.0.0" 
    : "localhost";
builder.WebHost.UseUrls($"http://{listenAddress}:{port}");
Log.Information("GateKeep.Api configurado para ejecutarse en puerto: {Port}", port);

// DEBUG: Verificar configuración de la base de datos
var currentDir = Directory.GetCurrentDirectory();
var configJsonPath = Path.Combine(currentDir, "config.json");
Log.Information("Directorio actual: {CurrentDir}", currentDir);
Log.Information("Entorno: {Environment}", builder.Environment.EnvironmentName);
Log.Information("config.json existe: {Exists}", File.Exists(configJsonPath));
var dbHost = builder.Configuration.GetSection("database")["host"];
var dbPort = builder.Configuration.GetSection("database")["port"];
var dbName = builder.Configuration.GetSection("database")["name"];
Log.Information("Configuración DB - Host: {Host}, Port: {Port}, DB: {Database}", dbHost ?? "NULL", dbPort ?? "NULL", dbName ?? "NULL");

// Swagger (exploración y documentación)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "GateKeep API", 
        Version = "v1",
        Description = "API para el sistema de gestión de acceso GateKeep",
        Contact = new OpenApiContact
        {
            Name = "GateKeep Team",
            Email = "support@gatekeep.com"
        }
    });

    // Configuración para JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Incluir comentarios XML si los tienes
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// JSON
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configuración de Seguridad JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtConfig = builder.Configuration.GetSection("jwt");
        
        // Permitir override con variables de entorno
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
            ?? jwtConfig["key"] 
            ?? throw new InvalidOperationException("JWT Key no configurada");
        
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
            ?? jwtConfig["issuer"] 
            ?? "GateKeep";
        
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
            ?? jwtConfig["audience"] 
            ?? "GateKeepUsers";
        
        Log.Information("Configurando JWT - Issuer: {Issuer}, Audience: {Audience}", jwtIssuer, jwtAudience);
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(5) // Permitir 5 minutos de diferencia
        };

        // Configuración para Swagger con logging de autenticación
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT Authentication Failed: {Message}, ExceptionType: {ExceptionType}, RequestPath: {Path}",
                    context.Exception.Message,
                    context.Exception.GetType().Name,
                    context.Request.Path);
                
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    Log.Warning("Token has expired for request: {Path}", context.Request.Path);
                    context.Response.Headers["Token-Expired"] = "true";
                }
                else if (context.Exception.GetType() == typeof(SecurityTokenInvalidSignatureException))
                {
                    Log.Warning("Token signature is invalid for request: {Path}", context.Request.Path);
                }
                else if (context.Exception.GetType() == typeof(SecurityTokenInvalidIssuerException))
                {
                    Log.Warning("Token issuer is invalid for request: {Path}", context.Request.Path);
                }
                else if (context.Exception.GetType() == typeof(SecurityTokenInvalidAudienceException))
                {
                    Log.Warning("Token audience is invalid for request: {Path}", context.Request.Path);
                }
                
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userName = context.Principal?.Identity?.Name ?? "Unknown";
                var roles = context.Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
                Log.Information("JWT Token validated for user: {UserName}, Roles: {Roles}", userName, string.Join(", ", roles));
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Log.Warning("JWT Challenge: Error={Error}, Description={ErrorDescription}, Path={Path}",
                    context.Error,
                    context.ErrorDescription,
                    context.Request.Path);
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                Log.Debug("JWT Message received from: {Path}", context.Request.Path);
                return Task.CompletedTask;
            }
        };
    });

// Configuración de Autorización
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("FuncionarioOrAdmin", policy => policy.RequireRole("Funcionario", "Admin"))
    .AddPolicy("AllUsers", policy => policy.RequireRole("Estudiante", "Funcionario", "Admin"));

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// EF Core - PostgreSQL
builder.Services.AddDbContext<GateKeepDbContext>(options =>
{
    // Leer configuración desde config.json
    var config = builder.Configuration.GetSection("database");
    
    // Permitir override con variables de entorno (prioridad: ENV > config.json)
    // Docker Compose pasa DATABASE__HOST (formato .NET Configuration), también soportamos DB_HOST para compatibilidad
    // .NET Configuration mapea DATABASE__HOST a Configuration["DATABASE:HOST"]
    var host = builder.Configuration["DATABASE:HOST"]
        ?? Environment.GetEnvironmentVariable("DATABASE__HOST")
        ?? Environment.GetEnvironmentVariable("DB_HOST")
        ?? config["host"]
        ?? "localhost";
    var port = builder.Configuration["DATABASE:PORT"]
        ?? Environment.GetEnvironmentVariable("DATABASE__PORT")
        ?? Environment.GetEnvironmentVariable("DB_PORT")
        ?? config["port"]
        ?? "5432";
    var database = builder.Configuration["DATABASE:NAME"]
        ?? Environment.GetEnvironmentVariable("DATABASE__NAME")
        ?? Environment.GetEnvironmentVariable("DB_NAME")
        ?? config["name"]
        ?? "GateKeep_Dev";
    var username = builder.Configuration["DATABASE:USER"]
        ?? Environment.GetEnvironmentVariable("DATABASE__USER")
        ?? Environment.GetEnvironmentVariable("DB_USER")
        ?? config["user"]
        ?? "postgres";
    var password = builder.Configuration["DATABASE:PASSWORD"]
        ?? Environment.GetEnvironmentVariable("DATABASE__PASSWORD")
        ?? Environment.GetEnvironmentVariable("DB_PASSWORD")
        ?? config["password"]
        ?? "dev_password";
    
    Log.Information("Configurando PostgreSQL - Host: {Host}, Puerto: {Port}, Base de datos: {Database}, Usuario: {User}", 
        host, port, database, username);
    
    var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};";
    
    options.UseNpgsql(connectionString, npgsql =>
    {
        // Usar un esquema interno para el historial de migraciones
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema: "infra");
    });
});

// Factory Pattern para Espacios
builder.Services.AddScoped<IEspacioRepository, EspacioRepository>();
builder.Services.AddScoped<IEspacioFactory, EspacioFactory>();

// Servicios de Anuncios
builder.Services.AddScoped<IAnuncioRepository, AnuncioRepository>();
builder.Services.AddScoped<IAnuncioService, AnuncioService>();

// Servicios de Beneficios
builder.Services.AddScoped<IBeneficioRepository, BeneficioRepository>();
builder.Services.AddScoped<IBeneficioService, BeneficioService>();
builder.Services.AddScoped<IBeneficioUsuarioRepository, BeneficioUsuarioRepository>();
builder.Services.AddScoped<IBeneficioUsuarioService, BeneficioUsuarioService>();

// Servicios de Eventos
builder.Services.AddScoped<IEventoRepository, EventoRepository>();
builder.Services.AddScoped<IEventoService, EventoService>();

// Servicios de Usuarios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioFactory, UsuarioFactory>();

// Servicios de Acceso
builder.Services.AddScoped<IReglaAccesoRepository, ReglaAccesoRepository>();
builder.Services.AddScoped<IReglaAccesoService, ReglaAccesoService>();
builder.Services.AddScoped<IAccesoService, AccesoService>();

// Servicios de Seguridad
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Utilidades
builder.Services.AddSingleton<QrCodeGenerator>();

// Servicios de Notificaciones MongoDB
builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<INotificacionUsuarioRepository, NotificacionUsuarioRepository>();

// Servicios de Sincronización Offline
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<INotificacionUsuarioService, NotificacionUsuarioService>();
builder.Services.AddScoped<INotificacionUsuarioValidationService, NotificacionUsuarioValidationService>();
builder.Services.AddScoped<INotificacionSincronizacionService, NotificacionSincronizacionService>();
builder.Services.AddScoped<INotificacionTransactionService, NotificacionTransactionService>();

// Servicios de Auditoria MongoDB
builder.Services.AddScoped<IEventoHistoricoRepository, EventoHistoricoRepository>();
builder.Services.AddScoped<IEventoHistoricoService, EventoHistoricoService>();

// MongoDB - Configuración con Atlas y API estable
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var mongoConfig = builder.Configuration.GetSection("mongodb");
    
    // Permitir override con variables de entorno
    var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION") 
        ?? mongoConfig["connectionString"] 
        ?? "mongodb://localhost:27017";
    
    var useStableApi = Environment.GetEnvironmentVariable("MONGODB_USE_STABLE_API")?.ToLower() == "true"
        || mongoConfig.GetValue<bool>("useStableApi", false);
    
    Log.Information("Configurando MongoDB - UseStableApi: {UseStableApi}", useStableApi);
    
    try
    {
        if (useStableApi)
        {
            // Configuración para MongoDB Atlas con API estable
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            return new MongoClient(settings);
        }
        else
        {
            // Configuración local simple
            return new MongoClient(connectionString);
        }
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Error conectando a MongoDB: {ex.Message}", ex);
    }
});

builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    var mongoConfig = builder.Configuration.GetSection("mongodb");
    
    // Permitir override con variables de entorno
    var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE") 
        ?? mongoConfig["databaseName"] 
        ?? "GateKeepMongo";
    
    Log.Information("Configurando MongoDB Database: {DatabaseName}", databaseName);
    
    return client.GetDatabase(databaseName);
});

// Configuración de Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConfig = builder.Configuration.GetSection("redis");
    // Permitir override con variables de entorno
    var connectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? redisConfig["connectionString"] ?? "localhost:6379";
    var instanceName = Environment.GetEnvironmentVariable("REDIS_INSTANCE") ?? redisConfig["instanceName"] ?? "GateKeepRedis:";
    
    options.Configuration = connectionString;
    options.InstanceName = instanceName;
    
    Log.Information("Configurando Redis - Connection: {Connection}, Instance: {Instance}", connectionString, instanceName);
});

// Servicios de Redis y Caching
builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
{
    var redisConfig = builder.Configuration.GetSection("redis");
    var connectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? redisConfig["connectionString"] ?? "localhost:6379";
    Log.Information("Conectando a Redis: {Connection}", connectionString);
    return ConnectionMultiplexer.Connect(connectionString);
});

builder.Services.AddSingleton<ICacheMetricsService, CacheMetricsService>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Servicios de Beneficios con Caching
builder.Services.AddScoped<ICachedBeneficioService, CachedBeneficioService>();

// Configuración de AWS SDK
var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? "sa-east-1";
var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);

Log.Information("Configurando AWS SDK - Región: {Region}", awsRegion);

// AWS Secrets Manager
builder.Services.AddSingleton<IAmazonSecretsManager>(sp =>
{
    var config = new AmazonSecretsManagerConfig
    {
        RegionEndpoint = regionEndpoint
    };
    // Las credenciales se leen automáticamente de las variables de entorno:
    // AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_REGION
    return new AmazonSecretsManagerClient(config);
});

// AWS Parameter Store (Systems Manager)
builder.Services.AddSingleton<IAmazonSimpleSystemsManagement>(sp =>
{
    var config = new AmazonSimpleSystemsManagementConfig
    {
        RegionEndpoint = regionEndpoint
    };
    return new AmazonSimpleSystemsManagementClient(config);
});

// AWS CloudWatch (para exportar métricas de cache)
builder.Services.AddSingleton<IAmazonCloudWatch>(sp =>
{
    var config = new AmazonCloudWatchConfig
    {
        RegionEndpoint = regionEndpoint
    };
    return new AmazonCloudWatchClient(config);
});

// Servicios AWS
builder.Services.AddScoped<IAwsSecretsService, AwsSecretsService>();
builder.Services.AddScoped<IAwsParameterService, AwsParameterService>();
builder.Services.AddSingleton<ICloudWatchMetricsExporter, CloudWatchMetricsExporter>();

// HttpClient para RabbitMQ Management API
builder.Services.AddHttpClient("RabbitMQ", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Servicios de métricas de RabbitMQ
builder.Services.AddSingleton<GateKeep.Api.Infrastructure.Messaging.IRabbitMqMetricsService, 
    GateKeep.Api.Infrastructure.Messaging.RabbitMqMetricsService>();
builder.Services.AddSingleton<IRabbitMqCloudWatchExporter, RabbitMqCloudWatchExporter>();

// Servicios de Observabilidad
builder.Services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();
builder.Services.AddSingleton<IObservabilityService, ObservabilityService>();

// Servicios de Eventos (Observer Pattern)
builder.Services.AddSingleton<IEventPublisher, EventPublisher>();

// Configuración de RabbitMQ Settings
builder.Services.Configure<GateKeep.Api.Infrastructure.Messaging.RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

// Servicios de Mensajería Asíncrona
builder.Services.AddSingleton<GateKeep.Api.Infrastructure.Messaging.IIdempotencyService, 
    GateKeep.Api.Infrastructure.Messaging.RedisIdempotencyService>();
builder.Services.AddScoped<GateKeep.Api.Infrastructure.Messaging.IEventBusPublisher, 
    GateKeep.Api.Infrastructure.Messaging.MassTransitEventBusPublisher>();

// Configuración de MassTransit con RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Registrar consumidores
    x.AddConsumer<GateKeep.Api.Infrastructure.Messaging.Consumers.AccesoRechazadoConsumer>();
    x.AddConsumer<GateKeep.Api.Infrastructure.Messaging.Consumers.BeneficioCanjeadoConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");
        
        // Leer configuración con fallback a variables de entorno
        var host = builder.Configuration["RABBITMQ:HOST"]
            ?? Environment.GetEnvironmentVariable("RABBITMQ__HOST")
            ?? rabbitMqConfig["Host"]
            ?? "localhost";
        var port = int.Parse(builder.Configuration["RABBITMQ:PORT"]
            ?? Environment.GetEnvironmentVariable("RABBITMQ__PORT")
            ?? rabbitMqConfig["Port"]
            ?? "5672");
        var username = builder.Configuration["RABBITMQ:USERNAME"]
            ?? Environment.GetEnvironmentVariable("RABBITMQ__USERNAME")
            ?? rabbitMqConfig["Username"]
            ?? "guest";
        var password = builder.Configuration["RABBITMQ:PASSWORD"]
            ?? Environment.GetEnvironmentVariable("RABBITMQ__PASSWORD")
            ?? rabbitMqConfig["Password"]
            ?? "guest";
        var virtualHost = builder.Configuration["RABBITMQ:VIRTUALHOST"]
            ?? Environment.GetEnvironmentVariable("RABBITMQ__VIRTUALHOST")
            ?? rabbitMqConfig["VirtualHost"]
            ?? "/";

        var retryCount = int.Parse(rabbitMqConfig["RetryCount"] ?? "3");
        var initialIntervalSeconds = int.Parse(rabbitMqConfig["InitialIntervalSeconds"] ?? "5");
        var intervalIncrementSeconds = int.Parse(rabbitMqConfig["IntervalIncrementSeconds"] ?? "10");

        // Verificar si se debe usar SSL (Amazon MQ usa SSL en puerto 5671)
        var useSsl = builder.Configuration["RABBITMQ:USE_SSL"]?.ToLower() == "true"
            || Environment.GetEnvironmentVariable("RABBITMQ__USE_SSL")?.ToLower() == "true"
            || port == 5671; // Puerto 5671 generalmente indica SSL

        Log.Information("Configurando RabbitMQ - Host: {Host}:{Port}, VHost: {VirtualHost}, Usuario: {Username}, SSL: {UseSsl}", 
            host, port, virtualHost, username, useSsl);

        cfg.Host(host, (ushort)port, virtualHost, h =>
        {
            h.Username(username);
            h.Password(password);
            
            // Configurar SSL si es necesario (Amazon MQ requiere SSL)
            if (useSsl)
            {
                h.UseSsl(s =>
                {
                    s.Protocol = System.Security.Authentication.SslProtocols.Tls12;
                });
            }
        });

        // Configurar reintentos con backoff exponencial
        cfg.UseMessageRetry(retry =>
        {
            retry.Exponential(retryCount, 
                TimeSpan.FromSeconds(initialIntervalSeconds), 
                TimeSpan.FromSeconds(intervalIncrementSeconds), 
                TimeSpan.FromMinutes(5));
            
            retry.Ignore<ArgumentNullException>();
            retry.Ignore<InvalidOperationException>();
        });

        // Configurar endpoints para los consumidores
        cfg.ReceiveEndpoint("acceso-rechazado-queue", e =>
        {
            e.ConfigureConsumer<GateKeep.Api.Infrastructure.Messaging.Consumers.AccesoRechazadoConsumer>(context);
            
            // Configurar Dead Letter Queue
            e.BindDeadLetterQueue("acceso-rechazado-queue-dlq", "acceso-rechazado-queue-dlq-exchange");
            
            // Configurar prefetch para limitar mensajes concurrentes
            e.PrefetchCount = 16;
            e.UseConcurrencyLimit(8);
        });

        cfg.ReceiveEndpoint("beneficio-canjeado-queue", e =>
        {
            e.ConfigureConsumer<GateKeep.Api.Infrastructure.Messaging.Consumers.BeneficioCanjeadoConsumer>(context);
            
            // Configurar Dead Letter Queue
            e.BindDeadLetterQueue("beneficio-canjeado-queue-dlq", "beneficio-canjeado-queue-dlq-exchange");
            
            e.PrefetchCount = 16;
            e.UseConcurrencyLimit(8);
        });

        // Configuración global de serialización
        cfg.ConfigureJsonSerializerOptions(options =>
        {
            options.WriteIndented = false;
            return options;
        });
    });
});

// Servicios de Colas
builder.Services.AddSingleton<ISincronizacionQueue, SincronizacionQueue>();
builder.Services.AddSingleton<IEventoQueue, EventoQueue>();

// Servicios de Background para procesar colas
builder.Services.AddHostedService<SincronizacionQueueProcessor>();
// EventoQueueProcessor deshabilitado - ahora se usa RabbitMQ para procesamiento asíncrono
// builder.Services.AddHostedService<EventoQueueProcessor>();
builder.Services.AddHostedService<BacklogMetricsUpdater>();
builder.Services.AddHostedService<CloudWatchMetricsExporter>(); // Exportador de métricas de Redis a CloudWatch
builder.Services.AddHostedService<RabbitMqCloudWatchExporter>(); // Exportador de métricas de RabbitMQ a CloudWatch

// Configuración de OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("GateKeep.Api")
        .AddAttributes(new Dictionary<string, object>
        {
            ["environment"] = builder.Environment.EnvironmentName,
            ["host.name"] = Environment.MachineName
        }))
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("GateKeep.Api")
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequest = (activity, request) =>
                {
                    if (request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
                    {
                        activity.SetTag("correlation_id", correlationId.ToString());
                    }
                };
                options.EnrichWithHttpResponse = (activity, response) =>
                {
                    activity.SetTag("http.response.content_type", response.ContentType);
                };
            })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddEntityFrameworkCoreInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
                options.EnrichWithIDbCommand = (activity, command) =>
                {
                    activity.SetTag("db.name", "GateKeepDb");
                };
            });
    })
    .WithMetrics(meterProviderBuilder =>
    {
        meterProviderBuilder
            .AddMeter("GateKeep.Api")
            .AddPrometheusExporter();
    });

var app = builder.Build();

// Swagger disponible en Development y Production (para demos)
// En un ambiente productivo real, esto debería estar protegido o deshabilitado
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GateKeep API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "GateKeep API Documentation";
    c.DefaultModelsExpandDepth(-1); // Ocultar modelos por defecto
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    // c.EnableFilter(); // ← Comentado para deshabilitar el filtro
    c.ShowExtensions();
    c.EnableValidator();
});

// Middleware de CORS
app.UseCors("AllowFrontend");

// Middleware de CorrelationId (antes de authentication para que esté disponible en logs)
app.UseCorrelationId();

// Middleware de Serilog para logging de requests
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        
        if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
        {
            diagnosticContext.Set("CorrelationId", correlationId);
        }
    };
});

// Middleware de Seguridad
app.UseAuthentication();
app.UseAuthorization();



// Minimal API
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
  .WithTags("System");

// Prometheus Metrics Endpoint
app.MapPrometheusScrapingEndpoint()
  .WithTags("System");

// MongoDB Health Check con ping usando BsonDocument
app.MapGet("/health/mongodb", (IMongoClient mongoClient) =>
{
    try
    {
        // Ping usando BsonDocument como en el código de Atlas
        var result = mongoClient.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
        return Results.Ok(new { 
            status = "ok", 
            database = "MongoDB Atlas", 
            message = "Pinged your deployment. You successfully connected to MongoDB!",
            pingResult = result.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error conectando a MongoDB Atlas: {ex.Message}");
    }
})
.WithTags("System");

// Redis Health Check
app.MapGet("/health/redis", (IConnectionMultiplexer redis) =>
{
    try
    {
        var isConnected = redis.IsConnected;
        
        return Results.Ok(new
        {
            status = isConnected ? "ok" : "disconnected",
            isConnected,
            endpoints = redis.GetEndPoints().Select(ep => ep.ToString()).ToArray(),
            message = "Redis is connected and operational"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error conectando a Redis: {ex.Message}");
    }
})
.WithTags("System");

// MongoDB Clear Database - Eliminar todos los datos (Solo en desarrollo)
app.MapDelete("/system/mongodb/clear", (IMongoDatabase mongoDatabase, IWebHostEnvironment env) =>
{
    // Validación de seguridad - Solo permitir en desarrollo
    if (!env.IsDevelopment())
    {
        return Results.Problem(
            "Este endpoint solo está disponible en modo desarrollo",
            statusCode: 403
        );
    }

    try
    {
        // Obtener lista de todas las colecciones
        var collections = mongoDatabase.ListCollectionNames().ToList();
        var deletedCollections = new List<string>();
        var totalDocumentsDeleted = 0;

        foreach (var collectionName in collections)
        {
            var collection = mongoDatabase.GetCollection<BsonDocument>(collectionName);
            var count = collection.CountDocuments(FilterDefinition<BsonDocument>.Empty);
            
            // Eliminar todos los documentos de la colección
            var deleteResult = collection.DeleteMany(FilterDefinition<BsonDocument>.Empty);
            totalDocumentsDeleted += (int)deleteResult.DeletedCount;
            deletedCollections.Add(collectionName);
        }

        return Results.Ok(new
        {
            status = "success",
            message = "Base de datos MongoDB limpiada exitosamente",
            environment = "Development",
            deletedCollections = deletedCollections,
            totalDocumentsDeleted = totalDocumentsDeleted,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error limpiando la base de datos MongoDB: {ex.Message}");
    }
})
.WithTags("System")
.WithSummary("Limpiar todos los datos de MongoDB")
.WithDescription("Elimina todos los documentos de todas las colecciones en la base de datos MongoDB");

// Endpoints
app.MapAccesoEndpoints();
app.MapAnuncioEndpoints();
app.MapAuthEndpoints();
app.MapEdificioEndpoints();
app.MapEventoEndpoints();
app.MapEventoHistoricoEndpoints();
app.MapLaboratorioEndpoints();
app.MapReglaAccesoEndpoints();
app.MapSalonEndpoints();
app.MapBeneficioEndpoints();
app.MapNotificacionEndpoints();
app.MapUsuarioEndpoints();
app.MapUsuarioProfileEndpoints();
app.MapCacheMetricsEndpoints(); // Endpoint de métricas de cache
app.MapAwsTestEndpoints(); // Endpoints de prueba AWS

// Auto-aplicar migraciones al iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GateKeepDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
    if (app.Environment.IsDevelopment())
    {
        // En desarrollo: recrear BD automáticamente
            logger.LogInformation("Modo Development: recreando base de datos...");
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        
        // Seed data inicial
        if (!db.Usuarios.Any())
        {
            var factory = scope.ServiceProvider.GetRequiredService<IUsuarioFactory>();
            var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
            
            // Crear usuario admin por defecto
            var adminDto = new UsuarioDto
            {
                Id = 0,
                Email = "admin@gatekeep.com",
                Nombre = "Administrador",
                Apellido = "Sistema",
                Contrasenia = passwordService.HashPassword("admin123"),
                Telefono = "+1234567890",
                FechaAlta = DateTime.UtcNow,
                Credencial = TipoCredencial.Vigente,
                Rol = Rol.Admin
            };
            
            var admin = factory.CrearUsuario(adminDto);
            db.Usuarios.Add(admin);
            
            // Crear estudiante de ejemplo
            var estudianteDto = new UsuarioDto
            {
                Id = 0,
                Email = "estudiante@gatekeep.com",
                Nombre = "Juan",
                Apellido = "Pérez",
                Contrasenia = passwordService.HashPassword("estudiante123"),
                Telefono = "+1234567891",
                FechaAlta = DateTime.UtcNow,
                Credencial = TipoCredencial.Vigente,
                Rol = Rol.Estudiante
            };
            
            var estudiante = factory.CrearUsuario(estudianteDto);
            db.Usuarios.Add(estudiante);
            
            // Crear funcionario de ejemplo
            var funcionarioDto = new UsuarioDto
            {
                Id = 0,
                Email = "funcionario@gatekeep.com",
                Nombre = "María",
                Apellido = "García",
                Contrasenia = passwordService.HashPassword("funcionario123"),
                Telefono = "+1234567892",
                FechaAlta = DateTime.UtcNow,
                Credencial = TipoCredencial.Vigente,
                Rol = Rol.Funcionario
            };
            
            var funcionario = factory.CrearUsuario(funcionarioDto);
            db.Usuarios.Add(funcionario);
            
            await db.SaveChangesAsync();
                logger.LogInformation("Datos iniciales creados exitosamente");
        }
    }
    else
    {
            // En producción: crear esquema infra y aplicar migraciones
            logger.LogInformation("Aplicando migraciones de base de datos...");
            
            // Crear el esquema 'infra' si no existe (requerido para el historial de migraciones)
            try
            {
                db.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS infra;");
            }
            catch (Exception schemaEx)
            {
                logger.LogWarning(schemaEx, "Advertencia al crear esquema 'infra' (puede que ya exista)");
            }
            
            try
            {
                // Verificar si hay migraciones pendientes antes de aplicar
                var pendingMigrations = db.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Aplicando {Count} migraciones pendientes...", pendingMigrations.Count);
                    db.Database.Migrate();
                    logger.LogInformation("Migraciones aplicadas exitosamente");
                }
                else
                {
                    logger.LogInformation("No hay migraciones pendientes. Base de datos está actualizada.");
                }
            }
            catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42P07" || pgEx.SqlState == "23505")
            {
                // Error de tabla/objeto ya existe o violación de clave única
                // Esto puede ocurrir si las tablas fueron creadas manualmente o hay inconsistencias
                logger.LogWarning(pgEx, "Advertencia: Algunas tablas ya existen. Verificando estado de migraciones...");
                
                // Intentar verificar el estado de las migraciones
                try
                {
                    var appliedMigrations = db.Database.GetAppliedMigrations().ToList();
                    var allMigrations = db.Database.GetMigrations().ToList();
                    
                    if (appliedMigrations.Count == allMigrations.Count)
                    {
                        logger.LogInformation("Todas las migraciones ya están aplicadas. Continuando...");
                    }
                    else
                    {
                        logger.LogWarning("Hay inconsistencias en las migraciones. La aplicación continuará pero puede haber problemas.");
                        logger.LogWarning("Migraciones aplicadas: {AppliedCount}/{TotalCount}", appliedMigrations.Count, allMigrations.Count);
                    }
                }
                catch (Exception checkEx)
                {
                    logger.LogError(checkEx, "No se pudo verificar el estado de las migraciones");
                    // Continuar de todas formas - la aplicación puede funcionar si las tablas existen
                }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al aplicar migraciones de base de datos");
        // En producción, no lanzar la excepción para evitar que la app se reinicie continuamente
        // Solo registrar el error y continuar
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
        else
        {
            logger.LogWarning("Continuando a pesar del error de migración (modo producción)");
        }
    }

    // Asegurar que el usuario admin de respaldo siempre exista (desarrollo y producción)
    try
    {
        var usuarioRepo = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
        var factory = scope.ServiceProvider.GetRequiredService<IUsuarioFactory>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        
        const string adminEmail = "admin1@gatekeep.com";
        const string adminPassword = "admin123";
        
        var adminExistente = await usuarioRepo.GetByEmailAsync(adminEmail);
        
        if (adminExistente == null)
        {
            logger.LogInformation("Creando usuario admin de respaldo: {Email}", adminEmail);
            
            var adminDto = new UsuarioDto
            {
                Id = 0,
                Email = adminEmail,
                Nombre = "Administrador",
                Apellido = "Respaldo",
                Contrasenia = passwordService.HashPassword(adminPassword),
                Telefono = "+1234567890",
                FechaAlta = DateTime.UtcNow,
                Credencial = TipoCredencial.Vigente,
                Rol = Rol.Admin
            };
            
            var admin = factory.CrearUsuario(adminDto);
            await usuarioRepo.AddAsync(admin);
            
            logger.LogInformation("✅ Usuario admin de respaldo creado exitosamente: {Email}", adminEmail);
        }
        else
        {
            // Verificar que el usuario tenga el rol Admin y actualizar si es necesario
            if (adminExistente.Rol != Rol.Admin)
            {
                logger.LogWarning("Usuario {Email} existe pero no tiene rol Admin. Actualizando rol...", adminEmail);
                
                // Actualizar el usuario con el rol Admin
                var usuarioActualizado = adminExistente with { Rol = Rol.Admin };
                await usuarioRepo.UpdateAsync(usuarioActualizado);
                
                logger.LogInformation("✅ Rol Admin actualizado para usuario: {Email}", adminEmail);
            }
            else
            {
                logger.LogInformation("✅ Usuario admin de respaldo ya existe: {Email}", adminEmail);
            }
        }
    }
    catch (Exception seedEx)
    {
        logger.LogError(seedEx, "Error al crear/verificar usuario admin de respaldo");
        // No lanzar excepción - el sistema puede funcionar sin este usuario
        // Solo registrar el error
    }
}

    Log.Information("GateKeep.Api iniciado correctamente");
    
    app.Run();
    
    Log.Information("GateKeep.Api detenido correctamente");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error fatal al iniciar la aplicación");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
