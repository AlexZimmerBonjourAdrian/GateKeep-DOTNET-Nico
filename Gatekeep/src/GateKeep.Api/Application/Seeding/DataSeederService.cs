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
        _logger.LogInformation("Iniciando seeding de datos iniciales para AWS...");

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

            // 2. Crear edificios con sus espacios (salones y laboratorios)
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
            var eventos = await SeedEventosAsync();

            // 8. Crear eventos de acceso
            await SeedEventosAccesoAsync(usuarios, espacios);

            // 9. Crear anuncios
            await SeedAnunciosAsync();

            _logger.LogInformation("✓ Seeding completado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el seeding de datos");
            throw;
        }
    }

    private async Task<List<Usuario>> SeedUsuariosAsync()
    {
        _logger.LogInformation("✓ Creando 15+ usuarios de prueba...");

        var usuarios = new List<Usuario>();
        var roles = new[] { Rol.Admin, Rol.Estudiante, Rol.Funcionario };
        var nombres = new[] { "Juan", "María", "Carlos", "Ana", "Luis", "Rosa", "Pedro", "Sofia", "Miguel", "Elena", "Diego", "Isabela", "Ricardo", "Valentina", "Fernando" };
        var apellidos = new[] { "García", "López", "Pérez", "Rodríguez", "Martínez", "Fernández", "González", "Sánchez", "Ramírez", "Flores", "Reyes", "Rojas", "Silva", "Vargas", "Ortiz" };

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
        _logger.LogInformation("  → Admin: admin@gatekeep.com");

        // Funcionarios
        for (int i = 0; i < 3; i++)
        {
            var funcionarioDto = new UsuarioDto
            {
                Id = 0,
                Email = $"funcionario{i + 1}@gatekeep.com",
                Nombre = nombres[i],
                Apellido = apellidos[i],
                Contrasenia = _passwordService.HashPassword("funcionario123"),
                Telefono = $"+123456789{i}",
                FechaAlta = DateTime.UtcNow.AddDays(-i * 10),
                Credencial = TipoCredencial.Vigente,
                Rol = Rol.Funcionario
            };
            var funcionario = _usuarioFactory.CrearUsuario(funcionarioDto);
            await _usuarioRepository.AddAsync(funcionario);
            usuarios.Add(funcionario);
        }
        _logger.LogInformation("  → 3 Funcionarios creados");

        // Estudiantes
        for (int i = 0; i < 12; i++)
        {
            var estudianteDto = new UsuarioDto
            {
                Id = 0,
                Email = $"estudiante{i + 1}@gatekeep.com",
                Nombre = nombres[(i + 3) % nombres.Length],
                Apellido = apellidos[(i + 3) % apellidos.Length],
                Contrasenia = _passwordService.HashPassword("estudiante123"),
                Telefono = $"+134567890{i:00}",
                FechaAlta = DateTime.UtcNow.AddDays(-(i + 3) * 5),
                Credencial = TipoCredencial.Vigente,
                Rol = Rol.Estudiante
            };
            var estudiante = _usuarioFactory.CrearUsuario(estudianteDto);
            await _usuarioRepository.AddAsync(estudiante);
            usuarios.Add(estudiante);
        }
        _logger.LogInformation("  → 12 Estudiantes creados");

        _logger.LogInformation("Total usuarios creados: {Count}", usuarios.Count);
        return usuarios;
    }

    private async Task<List<Espacio>> SeedEspaciosAsync()
    {
        _logger.LogInformation("✓ Creando 3 edificios con espacios...");

        var espacios = new List<Espacio>();

        // ===== EDIFICIO 1: CENTRAL =====
        var edificio1Request = new CrearEdificioRequest
        {
            Nombre = "Edificio Central",
            Descripcion = "Edificio principal con aulas y laboratorios",
            Ubicacion = "Campus Principal, Avenida Central 123",
            Capacidad = 500,
            NumeroPisos = 5,
            CodigoEdificio = "ED-001"
        };
        var edificio1 = await _espacioFactory.CrearEdificioAsync(edificio1Request);
        edificio1 = await _espacioRepository.GuardarEdificioAsync(edificio1);
        espacios.Add(edificio1);
        _logger.LogInformation("  → Edificio Central (ED-001)");

        // Salones en Edificio Central
        for (int piso = 1; piso <= 5; piso++)
        {
            for (int num = 1; num <= 3; num++)
            {
                var numeroSalon = piso * 100 + num;
                var salonRequest = new CrearSalonRequest
                {
                    Nombre = $"Salón {numeroSalon}",
                    Descripcion = $"Aula de clase piso {piso}",
                    Ubicacion = $"Edificio Central, Piso {piso}",
                    Capacidad = 30 + (num * 5),
                    EdificioId = edificio1.Id,
                    NumeroSalon = numeroSalon,
                    TipoSalon = num == 3 ? "Aula Multimedia" : "Aula Estándar"
                };
                var salon = await _espacioFactory.CrearSalonAsync(salonRequest);
                salon = await _espacioRepository.GuardarSalonAsync(salon);
                espacios.Add(salon);
            }
        }
        _logger.LogInformation("  → 15 Salones creados en Edificio Central");

        // Laboratorios en Edificio Central
        var laboratorios = new[]
        {
            ("Laboratorio de Informática", "Computadoras y software de desarrollo", 301, "Informática", true),
            ("Laboratorio de Electrónica", "Equipos de medición y prototipado", 302, "Electrónica", true),
            ("Laboratorio de Química", "Reactivos y equipamiento de laboratorio", 303, "Química", true),
            ("Laboratorio de Biología", "Microscopios y material de investigación", 304, "Biología", true)
        };

        foreach (var (nombre, desc, numero, tipo, equipamiento) in laboratorios)
        {
            var laboratorioRequest = new CrearLaboratorioRequest
            {
                Nombre = nombre,
                Descripcion = desc,
                Ubicacion = "Edificio Central, Piso 3",
                Capacidad = 25,
                EdificioId = edificio1.Id,
                NumeroLaboratorio = numero,
                TipoLaboratorio = tipo,
                EquipamientoEspecial = equipamiento
            };
            var laboratorio = await _espacioFactory.CrearLaboratorioAsync(laboratorioRequest);
            laboratorio = await _espacioRepository.GuardarLaboratorioAsync(laboratorio);
            espacios.Add(laboratorio);
        }
        _logger.LogInformation("  → 4 Laboratorios creados en Edificio Central");

        // ===== EDIFICIO 2: ANEXO =====
        var edificio2Request = new CrearEdificioRequest
        {
            Nombre = "Edificio Anexo",
            Descripcion = "Edificio complementario con aulas y talleres",
            Ubicacion = "Campus Principal, Avenida Anexo 456",
            Capacidad = 300,
            NumeroPisos = 3,
            CodigoEdificio = "ED-002"
        };
        var edificio2 = await _espacioFactory.CrearEdificioAsync(edificio2Request);
        edificio2 = await _espacioRepository.GuardarEdificioAsync(edificio2);
        espacios.Add(edificio2);
        _logger.LogInformation("  → Edificio Anexo (ED-002)");

        // Salones en Edificio Anexo
        for (int piso = 1; piso <= 3; piso++)
        {
            for (int num = 1; num <= 2; num++)
            {
                var numeroSalon = 2000 + piso * 100 + num;
                var salonRequest = new CrearSalonRequest
                {
                    Nombre = $"Salón {numeroSalon}",
                    Descripcion = $"Aula anexa piso {piso}",
                    Ubicacion = $"Edificio Anexo, Piso {piso}",
                    Capacidad = 35 + (num * 5),
                    EdificioId = edificio2.Id,
                    NumeroSalon = numeroSalon,
                    TipoSalon = "Aula de Seminarios"
                };
                var salon = await _espacioFactory.CrearSalonAsync(salonRequest);
                salon = await _espacioRepository.GuardarSalonAsync(salon);
                espacios.Add(salon);
            }
        }
        _logger.LogInformation("  → 6 Salones creados en Edificio Anexo");

        // Laboratorios en Edificio Anexo
        var labAnexo = new[]
        {
            ("Taller de Mecánica", "Herramientas y equipos mecánicos", 201, "Mecánica", true),
            ("Laboratorio de Física", "Equipamiento de física experimental", 202, "Física", true)
        };

        foreach (var (nombre, desc, numero, tipo, equipamiento) in labAnexo)
        {
            var laboratorioRequest = new CrearLaboratorioRequest
            {
                Nombre = nombre,
                Descripcion = desc,
                Ubicacion = "Edificio Anexo, Piso 2",
                Capacidad = 20,
                EdificioId = edificio2.Id,
                NumeroLaboratorio = numero,
                TipoLaboratorio = tipo,
                EquipamientoEspecial = equipamiento
            };
            var laboratorio = await _espacioFactory.CrearLaboratorioAsync(laboratorioRequest);
            laboratorio = await _espacioRepository.GuardarLaboratorioAsync(laboratorio);
            espacios.Add(laboratorio);
        }
        _logger.LogInformation("  → 2 Laboratorios creados en Edificio Anexo");

        // ===== EDIFICIO 3: ADMINISTRATIVO =====
        var edificio3Request = new CrearEdificioRequest
        {
            Nombre = "Edificio Administrativo",
            Descripcion = "Oficinas administrativas y sala de reuniones",
            Ubicacion = "Campus Principal, Avenida Administrativa 789",
            Capacidad = 200,
            NumeroPisos = 2,
            CodigoEdificio = "ED-003"
        };
        var edificio3 = await _espacioFactory.CrearEdificioAsync(edificio3Request);
        edificio3 = await _espacioRepository.GuardarEdificioAsync(edificio3);
        espacios.Add(edificio3);
        _logger.LogInformation("  → Edificio Administrativo (ED-003)");

        // Salones en Edificio Administrativo (Salas de reunión)
        var salasReunion = new[]
        {
            ("Sala de Reuniones A", "Equipada con video conferencia", 101, 20),
            ("Sala de Reuniones B", "Sala de capacitación", 102, 25),
            ("Auditorio Principal", "Para presentaciones y eventos", 201, 100)
        };

        foreach (var (nombre, desc, numero, capacidad) in salasReunion)
        {
            var salonRequest = new CrearSalonRequest
            {
                Nombre = nombre,
                Descripcion = desc,
                Ubicacion = $"Edificio Administrativo, Piso {numero / 100}",
                Capacidad = capacidad,
                EdificioId = edificio3.Id,
                NumeroSalon = numero,
                TipoSalon = "Sala de Reuniones"
            };
            var salon = await _espacioFactory.CrearSalonAsync(salonRequest);
            salon = await _espacioRepository.GuardarSalonAsync(salon);
            espacios.Add(salon);
        }
        _logger.LogInformation("  → 3 Salas de Reuniones creadas en Edificio Administrativo");

        _logger.LogInformation("Total espacios creados: {Count}", espacios.Count);
        return espacios;
    }

    private async Task<List<Beneficio>> SeedBeneficiosAsync()
    {
        _logger.LogInformation("✓ Creando 10+ beneficios...");

        var beneficios = new List<Beneficio>();

        var beneficiosTipos = new[]
        {
            (TipoBeneficio.Canje, "Canje - Comedor", 6, 150),
            (TipoBeneficio.Consumo, "Consumo - Biblioteca", 3, 200),
            (TipoBeneficio.Canje, "Canje - Tienda de Libros", 12, 75),
            (TipoBeneficio.Consumo, "Consumo - Parqueadero", 6, 300),
            (TipoBeneficio.Canje, "Canje - Fotocopia", 3, 500),
            (TipoBeneficio.Consumo, "Consumo - Laboratorios", 6, 100),
            (TipoBeneficio.Canje, "Canje - Impresión 3D", 12, 50),
            (TipoBeneficio.Consumo, "Consumo - Asesorías", 6, 80),
            (TipoBeneficio.Canje, "Canje - Actividades Deportivas", 12, 120),
            (TipoBeneficio.Consumo, "Consumo - Transporte", 6, 250)
        };

        foreach (var (tipo, descripcion, meses, cupos) in beneficiosTipos)
        {
            var beneficio = new Beneficio
            {
                Id = 0,
                Tipo = tipo,
                Vigencia = true,
                FechaDeVencimiento = DateTime.UtcNow.AddMonths(meses),
                Cupos = cupos
            };
            beneficio = await _beneficioRepository.CrearAsync(beneficio);
            beneficios.Add(beneficio);
        }

        _logger.LogInformation("Total beneficios creados: {Count}", beneficios.Count);
        return beneficios;
    }

    private async Task SeedUsuarioEspaciosAsync(List<Usuario> usuarios, List<Espacio> espacios)
    {
        _logger.LogInformation("✓ Creando relaciones Usuario-Espacio...");

        var estudiantes = usuarios.Where(u => u.Rol == Rol.Estudiante).ToList();
        var funcionarios = usuarios.Where(u => u.Rol == Rol.Funcionario).ToList();
        var admin = usuarios.FirstOrDefault(u => u.Rol == Rol.Admin);

        // Admin puede acceder a todos los edificios
        if (admin != null)
        {
            var edificios = espacios.OfType<Edificio>().Take(3).ToList();
            foreach (var edificio in edificios)
            {
                var usuarioEspacio = new UsuarioEspacio(admin.Id, edificio.Id);
                _dbContext.UsuariosEspacios.Add(usuarioEspacio);
            }
        }

        // Distribuir estudiantes entre espacios
        int espacioIndex = 0;
        foreach (var estudiante in estudiantes)
        {
            // Cada estudiante puede acceder a 2-3 espacios
            var espaciosAsignados = 2 + (espacioIndex % 2);
            for (int i = 0; i < espaciosAsignados && espacioIndex < espacios.Count; i++)
            {
                var espacio = espacios[(espacioIndex + i) % espacios.Count];
                var usuarioEspacio = new UsuarioEspacio(estudiante.Id, espacio.Id);
                _dbContext.UsuariosEspacios.Add(usuarioEspacio);
            }
            espacioIndex += espaciosAsignados;
        }

        // Funcionarios pueden acceder a múltiples espacios
        foreach (var funcionario in funcionarios)
        {
            foreach (var espacio in espacios.Take(Math.Min(10, espacios.Count)))
            {
                var usuarioEspacio = new UsuarioEspacio(funcionario.Id, espacio.Id);
                _dbContext.UsuariosEspacios.Add(usuarioEspacio);
            }
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("  → Relaciones Usuario-Espacio creadas");
    }

    private async Task SeedBeneficioUsuariosAsync(List<Usuario> usuarios, List<Beneficio> beneficios)
    {
        _logger.LogInformation("✓ Creando relaciones Beneficio-Usuario...");

        var estudiantes = usuarios.Where(u => u.Rol == Rol.Estudiante).ToList();

        if (!beneficios.Any())
            return;

        // Distribuir beneficios entre estudiantes
        for (int i = 0; i < estudiantes.Count; i++)
        {
            var estudiante = estudiantes[i];
            
            // Cada estudiante obtiene 2-3 beneficios aleatorios
            var beneficiosAsignados = 2 + (i % 2);
            for (int j = 0; j < beneficiosAsignados && j < beneficios.Count; j++)
            {
                var beneficio = beneficios[(i + j) % beneficios.Count];
                var beneficioUsuario = new BeneficioUsuario(estudiante.Id, beneficio.Id, false);
                await _beneficioUsuarioRepository.CrearAsync(beneficioUsuario);
            }
        }

        _logger.LogInformation("  → Relaciones Beneficio-Usuario creadas");
    }

    private async Task SeedReglasAccesoAsync(List<Espacio> espacios)
    {
        _logger.LogInformation("✓ Creando 20+ reglas de acceso...");

        if (!espacios.Any())
            return;

        var horarios = new[]
        {
            (8, 20),    // 8am - 8pm
            (7, 18),    // 7am - 6pm
            (9, 17),    // 9am - 5pm
            (7, 22),    // 7am - 10pm
            (24, 24)    // 24 horas (laboratorios)
        };

        int reglasCreadas = 0;

        foreach (var espacio in espacios)
        {
            // Crear 2-3 reglas por espacio con diferentes horarios
            var horarioIdx = reglasCreadas % horarios.Length;
            var (horaApertura, horaCierre) = horarios[horarioIdx];

            var horarioApertura = DateTime.UtcNow.Date.AddHours(horaApertura);
            var horarioCierre = DateTime.UtcNow.Date.AddHours(horaCierre);

            var reglaAcceso = new ReglaAcceso(
                Id: 0,
                HorarioApertura: horarioApertura,
                HorarioCierre: horarioCierre,
                VigenciaApertura: DateTime.UtcNow.Date,
                VigenciaCierre: DateTime.UtcNow.Date.AddMonths(6),
                RolesPermitidos: new List<Rol> { Rol.Estudiante, Rol.Funcionario, Rol.Admin },
                EspacioId: espacio.Id
            );

            await _reglaAccesoRepository.CrearAsync(reglaAcceso);
            reglasCreadas++;

            // Agregar regla adicional para funcionarios en algunos espacios
            if (reglasCreadas % 3 == 0)
            {
                var reglaFuncionarios = new ReglaAcceso(
                    Id: 0,
                    HorarioApertura: DateTime.UtcNow.Date.AddHours(6),
                    HorarioCierre: DateTime.UtcNow.Date.AddHours(22),
                    VigenciaApertura: DateTime.UtcNow.Date,
                    VigenciaCierre: DateTime.UtcNow.Date.AddMonths(6),
                    RolesPermitidos: new List<Rol> { Rol.Funcionario, Rol.Admin },
                    EspacioId: espacio.Id
                );
                await _reglaAccesoRepository.CrearAsync(reglaFuncionarios);
                reglasCreadas++;
            }
        }

        _logger.LogInformation($"  → {reglasCreadas} Reglas de acceso creadas");
    }

    private async Task<List<Evento>> SeedEventosAsync()
    {
        _logger.LogInformation("✓ Creando 15+ eventos...");

        var eventos = new List<Evento>();

        var eventosData = new[]
        {
            ("Conferencia de Tecnología", DateTime.UtcNow.AddDays(7), "Entrada Principal"),
            ("Taller de Desarrollo Web", DateTime.UtcNow.AddDays(14), "Salón 201"),
            ("Seminario de Inteligencia Artificial", DateTime.UtcNow.AddDays(21), "Auditorio Principal"),
            ("Workshop de Python", DateTime.UtcNow.AddDays(2), "Laboratorio de Informática"),
            ("Charla de Seguridad Informática", DateTime.UtcNow.AddDays(10), "Salón 101"),
            ("Feria de Proyectos", DateTime.UtcNow.AddDays(30), "Edificio Central"),
            ("Competencia de Programación", DateTime.UtcNow.AddDays(45), "Laboratorio 301"),
            ("Taller de Electrónica", DateTime.UtcNow.AddDays(5), "Laboratorio de Electrónica"),
            ("Jornada de Biología", DateTime.UtcNow.AddDays(12), "Laboratorio de Biología"),
            ("Sesión de Física Experimental", DateTime.UtcNow.AddDays(8), "Taller de Mecánica"),
            ("Reunión Académica", DateTime.UtcNow.AddDays(3), "Sala de Reuniones A"),
            ("Congreso Estudiantil", DateTime.UtcNow.AddDays(25), "Auditorio Principal"),
            ("Capacitación de Sistema GateKeep", DateTime.UtcNow.AddDays(4), "Sala de Reuniones B"),
            ("Conferencia de Sostenibilidad", DateTime.UtcNow.AddDays(18), "Edificio Anexo"),
            ("Taller de Emprendimiento", DateTime.UtcNow.AddDays(11), "Salón 2101")
        };

        foreach (var (nombre, fecha, ubicacion) in eventosData)
        {
            var evento = new Evento(
                Id: 0,
                Nombre: nombre,
                Fecha: fecha,
                Resultado: fecha > DateTime.UtcNow ? "Programado" : "Completado",
                PuntoControl: ubicacion,
                Activo: true
            );
            evento = await _eventoRepository.CrearAsync(evento);
            eventos.Add(evento);
        }

        _logger.LogInformation($"  → {eventos.Count} Eventos creados");
        return eventos;
    }

    private async Task SeedEventosAccesoAsync(List<Usuario> usuarios, List<Espacio> espacios)
    {
        _logger.LogInformation("✓ Creando 50+ eventos de acceso...");

        if (!usuarios.Any() || !espacios.Any())
            return;

        var resultados = new[] { "Permitido", "Denegado", "Con Alerta" };
        var eventosAcceso = new List<EventoAcceso>();

        // Crear registros de acceso para los últimos 7 días
        for (int dia = 0; dia < 7; dia++)
        {
            var fecha = DateTime.UtcNow.AddDays(-dia);
            
            // 5-10 eventos por día
            for (int evento = 0; evento < 8; evento++)
            {
                var usuario = usuarios[evento % usuarios.Count];
                var espacio = espacios[evento % espacios.Count];
                var resultado = resultados[(dia + evento) % resultados.Length];
                
                var eventoAcceso = new EventoAcceso(
                    Id: 0,
                    Nombre: $"Acceso a {espacio.Nombre}",
                    Fecha: fecha.AddHours(8 + evento).AddMinutes(evento * 15),
                    Resultado: resultado,
                    PuntoControl: espacio.Ubicacion ?? "Entrada Principal",
                    UsuarioId: usuario.Id,
                    EspacioId: espacio.Id
                );
                eventosAcceso.Add(eventoAcceso);
            }
        }

        // Agregar eventos en batch
        foreach (var eventoAcceso in eventosAcceso)
        {
            _dbContext.EventosAcceso.Add(eventoAcceso);
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation($"  → {eventosAcceso.Count} Eventos de acceso creados");
    }

    private async Task SeedAnunciosAsync()
    {
        _logger.LogInformation("✓ Creando 15+ anuncios...");

        var anunciosData = new[]
        {
            ("Bienvenida al nuevo semestre", true),
            ("Mantenimiento programado - Sábado 23 de noviembre", true),
            ("Cambio de horario en laboratorios", true),
            ("Nuevo sistema de beneficios disponible", true),
            ("Inscripciones abiertas para talleres", true),
            ("Feria de empresas - 15 de diciembre", true),
            ("Cierre de campus - Festivo nacional", false),
            ("Actualización del sistema GateKeep", true),
            ("Capacitación para nuevos usuarios", true),
            ("Cambios en políticas de acceso", true),
            ("Disponibilidad de becas 2024", true),
            ("Licitación de nuevos laboratorios", true),
            ("Programa de prácticas profesionales", true),
            ("Certificaciones disponibles", true),
            ("Encuesta de satisfacción - Participar", true)
        };

        foreach (var (nombre, activo) in anunciosData)
        {
            var anuncio = new Anuncio(
                Id: 0,
                Nombre: nombre,
                Fecha: DateTime.UtcNow.AddDays(Random.Shared.Next(-30, 30)),
                Activo: activo
            );
            await _anuncioRepository.CrearAsync(anuncio);
        }

        _logger.LogInformation($"  → {anunciosData.Length} Anuncios creados");
    }
}

