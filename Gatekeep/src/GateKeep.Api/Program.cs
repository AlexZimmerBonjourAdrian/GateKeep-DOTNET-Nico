using System.Text.Json.Serialization;
using GateKeep.Api.Application.Acceso;
using GateKeep.Api.Application.Anuncios;
using GateKeep.Api.Application.Auditoria;
using GateKeep.Api.Application.Beneficios;
using GateKeep.Api.Application.Espacios;
using GateKeep.Api.Application.Eventos;
using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Application.Security;
using GateKeep.Api.Application.Usuarios;
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
using GateKeep.Api.Infrastructure.Acceso;
using GateKeep.Api.Infrastructure.Anuncios;
using GateKeep.Api.Infrastructure.Auditoria;
using GateKeep.Api.Infrastructure.Beneficios;
using GateKeep.Api.Infrastructure.Espacios;
using GateKeep.Api.Infrastructure.Eventos;
using GateKeep.Api.Infrastructure.Notificaciones;
using GateKeep.Api.Infrastructure.Persistence;
using GateKeep.Api.Infrastructure.Security;
using GateKeep.Api.Infrastructure.Usuarios;
using GateKeep.Infrastructure.QrCodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Cargar config.json
builder.Configuration.AddJsonFile("config.json", optional: false, reloadOnChange: true);

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
        var jwtKey = jwtConfig["key"] ?? throw new InvalidOperationException("JWT Key no configurada");
        var jwtIssuer = jwtConfig["issuer"] ?? "GateKeep";
        var jwtAudience = jwtConfig["audience"] ?? "GateKeepUsers";
        
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

        // Configuración para Swagger con logging completo
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Authentication Failed: {context.Exception.Message}");
                Console.WriteLine($"Exception Type: {context.Exception.GetType().Name}");
                Console.WriteLine($"Request Path: {context.Request.Path}");
                
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    Console.WriteLine("Token has expired");
                    context.Response.Headers["Token-Expired"] = "true";
                }
                else if (context.Exception.GetType() == typeof(SecurityTokenInvalidSignatureException))
                {
                    Console.WriteLine("Token signature is invalid");
                }
                else if (context.Exception.GetType() == typeof(SecurityTokenInvalidIssuerException))
                {
                    Console.WriteLine("Token issuer is invalid");
                }
                else if (context.Exception.GetType() == typeof(SecurityTokenInvalidAudienceException))
                {
                    Console.WriteLine("Token audience is invalid");
                }
                
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"JWT Token Validated for user: {context.Principal?.Identity?.Name}");
                var roles = context.Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
                Console.WriteLine($"User Roles: {string.Join(", ", roles)}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"JWT Challenge: {context.Error} - {context.ErrorDescription}");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                Console.WriteLine($"JWT Message Received from: {context.Request.Path}");
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
    var host = config["host"] ?? "localhost";
    var port = config["port"] ?? "5432";
    var database = config["name"] ?? "GateKeep_Dev";
    var username = config["user"] ?? "postgres";
    var password = config["password"] ?? "dev_password";
    
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

// Servicios de Auditoria MongoDB
builder.Services.AddScoped<IEventoHistoricoRepository, EventoHistoricoRepository>();
builder.Services.AddScoped<IEventoHistoricoService, EventoHistoricoService>();

// MongoDB - Configuración con Atlas y API estable
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var mongoConfig = builder.Configuration.GetSection("mongodb");
    var connectionString = mongoConfig["connectionString"] ?? "mongodb://localhost:27017";
    var useStableApi = mongoConfig.GetValue<bool>("useStableApi", false);
    
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
    var databaseName = mongoConfig["databaseName"] ?? "GateKeepMongo";
    
    return client.GetDatabase(databaseName);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
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
}

// Middleware de CORS
app.UseCors("AllowFrontend");

// Middleware de Seguridad
app.UseAuthentication();
app.UseAuthorization();



// Minimal API
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
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

// Auto-aplicar migraciones al iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GateKeepDbContext>();
    
    if (app.Environment.IsDevelopment())
    {
        // En desarrollo: recrear BD automáticamente
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
        }
    }
    else
    {
        // En producción: solo migraciones
        db.Database.Migrate();
    }
}


app.Run();
