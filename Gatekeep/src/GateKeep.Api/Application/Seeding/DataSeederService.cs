using GateKeep.Api.Application.Acceso;
using GateKeep.Api.Application.Anuncios;
using GateKeep.Api.Application.Beneficios;
using GateKeep.Api.Application.Espacios;
using GateKeep.Api.Application.Eventos;
using GateKeep.Api.Application.Security;
using GateKeep.Api.Application.Usuarios;
using GateKeep.Api.Contracts.Espacios;
using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Domain.Enums;
using GateKeep.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Application.Seeding;

/// <summary>
/// Servicio para crear datos iniciales (seed data) en la base de datos
/// </summary>
public sealed class DataSeederService : IDataSeederService
{
    private readonly GateKeepDbContext _dbContext;
    private readonly IUsuarioFactory _usuarioFactory;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordService _passwordService;
    private readonly IEspacioFactory _espacioFactory;
    private readonly IEspacioRepository _espacioRepository;
    private readonly IBeneficioRepository _beneficioRepository;
    private readonly IBeneficioUsuarioRepository _beneficioUsuarioRepository;
    private readonly IReglaAccesoRepository _reglaAccesoRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly IAnuncioRepository _anuncioRepository;
    private readonly ILogger<DataSeederService> _logger;

    public DataSeederService(
        GateKeepDbContext dbContext,
        IUsuarioFactory usuarioFactory,
        IUsuarioRepository usuarioRepository,
        IPasswordService passwordService,
        IEspacioFactory espacioFactory,
        IEspacioRepository espacioRepository,
        IBeneficioRepository beneficioRepository,
        IBeneficioUsuarioRepository beneficioUsuarioRepository,
        IReglaAccesoRepository reglaAccesoRepository,
        IEventoRepository eventoRepository,
        IAnuncioRepository anuncioRepository,
        ILogger<DataSeederService> logger)
    {
        _dbContext = dbContext;
        _usuarioFactory = usuarioFactory;
        _usuarioRepository = usuarioRepository;
        _passwordService = passwordService;
        _espacioFactory = espacioFactory;
        _espacioRepository = espacioRepository;
        _beneficioRepository = beneficioRepository;
        _beneficioUsuarioRepository = beneficioUsuarioRepository;
        _reglaAccesoRepository = reglaAccesoRepository;
        _eventoRepository = eventoRepository;
        _anuncioRepository = anuncioRepository;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Iniciando seeding de datos iniciales...");

        try
        {
            // Verificar si ya hay datos
            if (await _dbContext.Usuarios.AnyAsync())
            {
                _logger.LogInformation("La base de datos ya contiene datos. Saltando seeding.");
                return;
            }

            // 1. Crear usuarios
            var usuarios = await SeedUsuariosAsync();

            // 2. Crear espacios (edificios, salones, laboratorios)
            var espacios = await SeedEspaciosAsync();

            // 3. Crear beneficios
            var beneficios = await SeedBeneficiosAsync();

            // 4. Crear relaciones Usuario-Espacio
            await SeedUsuarioEspaciosAsync(usuarios, espacios);

            // 5. Crear relaciones Beneficio-Usuario
            await SeedBeneficioUsuariosAsync(usuarios, beneficios);

            // 6. Crear reglas de acceso
            await SeedReglasAccesoAsync(espacios);

            // 7. Crear eventos
            await SeedEventosAsync();

            // 8. Crear eventos de acceso
            await SeedEventosAccesoAsync(usuarios, espacios);

            // 9. Crear anuncios
            await SeedAnunciosAsync();

            _logger.LogInformation("Seeding completado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el seeding de datos");
            throw;
        }
    }

    private async Task<List<Usuario>> SeedUsuariosAsync()
    {
        _logger.LogInformation("Creando usuarios iniciales...");

        var usuarios = new List<Usuario>();

        // Admin
        var adminDto = new UsuarioDto
        {
            Id = 0,
            Email = "admin@gatekeep.com",
            Nombre = "Administrador",
            Apellido = "Sistema",
            Contrasenia = _passwordService.HashPassword("admin123"),
            Telefono = "+1234567890",
            FechaAlta = DateTime.UtcNow,
            Credencial = TipoCredencial.Vigente,
            Rol = Rol.Admin
        };
        var admin = _usuarioFactory.CrearUsuario(adminDto);
        await _usuarioRepository.AddAsync(admin);
        usuarios.Add(admin);

        // Estudiante
        var estudianteDto = new UsuarioDto
        {
            Id = 0,
            Email = "estudiante@gatekeep.com",
            Nombre = "Juan",
            Apellido = "Pérez",
            Contrasenia = _passwordService.HashPassword("estudiante123"),
            Telefono = "+1234567891",
            FechaAlta = DateTime.UtcNow,
            Credencial = TipoCredencial.Vigente,
            Rol = Rol.Estudiante
        };
        var estudiante = _usuarioFactory.CrearUsuario(estudianteDto);
        await _usuarioRepository.AddAsync(estudiante);
        usuarios.Add(estudiante);

        // Funcionario
        var funcionarioDto = new UsuarioDto
        {
            Id = 0,
            Email = "funcionario@gatekeep.com",
            Nombre = "María",
            Apellido = "García",
            Contrasenia = _passwordService.HashPassword("funcionario123"),
            Telefono = "+1234567892",
            FechaAlta = DateTime.UtcNow,
            Credencial = TipoCredencial.Vigente,
            Rol = Rol.Funcionario
        };
        var funcionario = _usuarioFactory.CrearUsuario(funcionarioDto);
        await _usuarioRepository.AddAsync(funcionario);
        usuarios.Add(funcionario);

        // Estudiante adicional
        var estudiante2Dto = new UsuarioDto
        {
            Id = 0,
            Email = "estudiante2@gatekeep.com",
            Nombre = "Carlos",
            Apellido = "López",
            Contrasenia = _passwordService.HashPassword("estudiante123"),
            Telefono = "+1234567893",
            FechaAlta = DateTime.UtcNow,
            Credencial = TipoCredencial.Vigente,
            Rol = Rol.Estudiante
        };
        var estudiante2 = _usuarioFactory.CrearUsuario(estudiante2Dto);
        await _usuarioRepository.AddAsync(estudiante2);
        usuarios.Add(estudiante2);

        _logger.LogInformation("Usuarios creados: {Count}", usuarios.Count);
        return usuarios;
    }

    private async Task<List<Espacio>> SeedEspaciosAsync()
    {
        _logger.LogInformation("Creando espacios iniciales...");

        var espacios = new List<Espacio>();

        // Crear edificio
        var edificioRequest = new CrearEdificioRequest
        {
            Nombre = "Edificio Central",
            Descripcion = "Edificio principal del campus",
            Ubicacion = "Campus Principal, Avenida Principal 123",
            Capacidad = 500,
            NumeroPisos = 5,
            CodigoEdificio = "ED-001"
        };
        var edificio = await _espacioFactory.CrearEdificioAsync(edificioRequest);
        edificio = await _espacioRepository.GuardarEdificioAsync(edificio);
        espacios.Add(edificio);

        // Crear salones en el edificio
        var salon1Request = new CrearSalonRequest
        {
            Nombre = "Salón 101",
            Descripcion = "Salón de clases estándar",
            Ubicacion = "Edificio Central, Piso 1",
            Capacidad = 40,
            EdificioId = edificio.Id,
            NumeroSalon = 101,
            TipoSalon = "Aula"
        };
        var salon1 = await _espacioFactory.CrearSalonAsync(salon1Request);
        salon1 = await _espacioRepository.GuardarSalonAsync(salon1);
        espacios.Add(salon1);

        var salon2Request = new CrearSalonRequest
        {
            Nombre = "Salón 201",
            Descripcion = "Salón de clases con proyector",
            Ubicacion = "Edificio Central, Piso 2",
            Capacidad = 50,
            EdificioId = edificio.Id,
            NumeroSalon = 201,
            TipoSalon = "Aula Multimedia"
        };
        var salon2 = await _espacioFactory.CrearSalonAsync(salon2Request);
        salon2 = await _espacioRepository.GuardarSalonAsync(salon2);
        espacios.Add(salon2);

        // Crear laboratorio
        var laboratorioRequest = new CrearLaboratorioRequest
        {
            Nombre = "Laboratorio de Informática",
            Descripcion = "Laboratorio con equipos de cómputo",
            Ubicacion = "Edificio Central, Piso 3",
            Capacidad = 30,
            EdificioId = edificio.Id,
            NumeroLaboratorio = 301,
            TipoLaboratorio = "Informática",
            EquipamientoEspecial = true
        };
        var laboratorio = await _espacioFactory.CrearLaboratorioAsync(laboratorioRequest);
        laboratorio = await _espacioRepository.GuardarLaboratorioAsync(laboratorio);
        espacios.Add(laboratorio);

        _logger.LogInformation("Espacios creados: {Count}", espacios.Count);
        return espacios;
    }

    private async Task<List<Beneficio>> SeedBeneficiosAsync()
    {
        _logger.LogInformation("Creando beneficios iniciales...");

        var beneficios = new List<Beneficio>();

        var beneficio1 = new Beneficio
        {
            Id = 0,
            Tipo = TipoBeneficio.Canje,
            Vigencia = true,
            FechaDeVencimiento = DateTime.UtcNow.AddMonths(6),
            Cupos = 100
        };
        beneficio1 = await _beneficioRepository.CrearAsync(beneficio1);
        beneficios.Add(beneficio1);

        var beneficio2 = new Beneficio
        {
            Id = 0,
            Tipo = TipoBeneficio.Consumo,
            Vigencia = true,
            FechaDeVencimiento = DateTime.UtcNow.AddMonths(3),
            Cupos = 50
        };
        beneficio2 = await _beneficioRepository.CrearAsync(beneficio2);
        beneficios.Add(beneficio2);

        var beneficio3 = new Beneficio
        {
            Id = 0,
            Tipo = TipoBeneficio.Canje,
            Vigencia = true,
            FechaDeVencimiento = DateTime.UtcNow.AddMonths(12),
            Cupos = 200
        };
        beneficio3 = await _beneficioRepository.CrearAsync(beneficio3);
        beneficios.Add(beneficio3);

        _logger.LogInformation("Beneficios creados: {Count}", beneficios.Count);
        return beneficios;
    }

    private async Task SeedUsuarioEspaciosAsync(List<Usuario> usuarios, List<Espacio> espacios)
    {
        _logger.LogInformation("Creando relaciones Usuario-Espacio...");

        var estudiante = usuarios.FirstOrDefault(u => u.Rol == Rol.Estudiante);
        var funcionario = usuarios.FirstOrDefault(u => u.Rol == Rol.Funcionario);

        if (estudiante != null && espacios.Any())
        {
            // Asignar estudiante a los primeros dos espacios
            foreach (var espacio in espacios.Take(2))
            {
                var usuarioEspacio = new UsuarioEspacio(estudiante.Id, espacio.Id);
                _dbContext.UsuariosEspacios.Add(usuarioEspacio);
            }
        }

        if (funcionario != null && espacios.Any())
        {
            // Asignar funcionario a todos los espacios
            foreach (var espacio in espacios)
            {
                var usuarioEspacio = new UsuarioEspacio(funcionario.Id, espacio.Id);
                _dbContext.UsuariosEspacios.Add(usuarioEspacio);
            }
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Relaciones Usuario-Espacio creadas");
    }

    private async Task SeedBeneficioUsuariosAsync(List<Usuario> usuarios, List<Beneficio> beneficios)
    {
        _logger.LogInformation("Creando relaciones Beneficio-Usuario...");

        var estudiante = usuarios.FirstOrDefault(u => u.Rol == Rol.Estudiante);
        var estudiante2 = usuarios.Skip(1).FirstOrDefault(u => u.Rol == Rol.Estudiante);

        if (estudiante != null && beneficios.Any())
        {
            // Asignar primer beneficio al estudiante
            var beneficioUsuario1 = new BeneficioUsuario(estudiante.Id, beneficios[0].Id, false);
            await _beneficioUsuarioRepository.CrearAsync(beneficioUsuario1);

            // Asignar segundo beneficio al estudiante
            if (beneficios.Count > 1)
            {
                var beneficioUsuario2 = new BeneficioUsuario(estudiante.Id, beneficios[1].Id, false);
                await _beneficioUsuarioRepository.CrearAsync(beneficioUsuario2);
            }
        }

        if (estudiante2 != null && beneficios.Count > 2)
        {
            // Asignar tercer beneficio al segundo estudiante
            var beneficioUsuario3 = new BeneficioUsuario(estudiante2.Id, beneficios[2].Id, false);
            await _beneficioUsuarioRepository.CrearAsync(beneficioUsuario3);
        }

        _logger.LogInformation("Relaciones Beneficio-Usuario creadas");
    }

    private async Task SeedReglasAccesoAsync(List<Espacio> espacios)
    {
        _logger.LogInformation("Creando reglas de acceso...");

        if (!espacios.Any())
            return;

        var espacio = espacios.First();
        var horarioApertura = DateTime.UtcNow.Date.AddHours(8);
        var horarioCierre = DateTime.UtcNow.Date.AddHours(20);
        var vigenciaApertura = DateTime.UtcNow.Date;
        var vigenciaCierre = DateTime.UtcNow.Date.AddMonths(6);

        var reglaAcceso = new ReglaAcceso(
            Id: 0,
            HorarioApertura: horarioApertura,
            HorarioCierre: horarioCierre,
            VigenciaApertura: vigenciaApertura,
            VigenciaCierre: vigenciaCierre,
            RolesPermitidos: new List<Rol> { Rol.Estudiante, Rol.Funcionario, Rol.Admin },
            EspacioId: espacio.Id
        );

        await _reglaAccesoRepository.CrearAsync(reglaAcceso);

        _logger.LogInformation("Reglas de acceso creadas");
    }

    private async Task SeedEventosAsync()
    {
        _logger.LogInformation("Creando eventos iniciales...");

        var evento1 = new Evento(
            Id: 0,
            Nombre: "Conferencia de Tecnología",
            Fecha: DateTime.UtcNow.AddDays(7),
            Resultado: "Programado",
            PuntoControl: "Entrada Principal",
            Activo: true
        );
        await _eventoRepository.CrearAsync(evento1);

        var evento2 = new Evento(
            Id: 0,
            Nombre: "Taller de Desarrollo",
            Fecha: DateTime.UtcNow.AddDays(14),
            Resultado: "Programado",
            PuntoControl: "Salón 201",
            Activo: true
        );
        await _eventoRepository.CrearAsync(evento2);

        _logger.LogInformation("Eventos creados");
    }

    private async Task SeedEventosAccesoAsync(List<Usuario> usuarios, List<Espacio> espacios)
    {
        _logger.LogInformation("Creando eventos de acceso...");

        if (!usuarios.Any() || !espacios.Any())
            return;

        var usuario = usuarios.First();
        var espacio = espacios.First();

        var eventoAcceso1 = new EventoAcceso(
            Id: 0,
            Nombre: "Acceso al Edificio Central",
            Fecha: DateTime.UtcNow.AddHours(-2),
            Resultado: "Permitido",
            PuntoControl: "Entrada Principal",
            UsuarioId: usuario.Id,
            EspacioId: espacio.Id
        );
        _dbContext.EventosAcceso.Add(eventoAcceso1);

        var eventoAcceso2 = new EventoAcceso(
            Id: 0,
            Nombre: "Acceso al Salón 101",
            Fecha: DateTime.UtcNow.AddHours(-1),
            Resultado: "Permitido",
            PuntoControl: "Salón 101",
            UsuarioId: usuario.Id,
            EspacioId: espacios.Skip(1).First().Id
        );
        _dbContext.EventosAcceso.Add(eventoAcceso2);

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Eventos de acceso creados");
    }

    private async Task SeedAnunciosAsync()
    {
        _logger.LogInformation("Creando anuncios iniciales...");

        var anuncio1 = new Anuncio(
            Id: 0,
            Nombre: "Bienvenida al nuevo semestre",
            Fecha: DateTime.UtcNow,
            Activo: true
        );
        await _anuncioRepository.CrearAsync(anuncio1);

        var anuncio2 = new Anuncio(
            Id: 0,
            Nombre: "Mantenimiento programado",
            Fecha: DateTime.UtcNow.AddDays(1),
            Activo: true
        );
        await _anuncioRepository.CrearAsync(anuncio2);

        _logger.LogInformation("Anuncios creados");
    }
}

