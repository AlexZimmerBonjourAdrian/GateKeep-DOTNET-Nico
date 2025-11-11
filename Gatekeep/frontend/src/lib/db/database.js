import Dexie from 'dexie';

/**
 * Base de datos local usando IndexedDB (Dexie)
 * Almacena datos para modo offline
 */
class GateKeepDatabase extends Dexie {
    constructor() {
        super('GateKeepDB');
        
        // Definir esquema de la base de datos
        this.version(1).stores({
            // Tablas principales
            usuarios: 'id, email, nombre, apellido, fechaAlta',
            eventos: 'id, nombre, fecha, resultado, puntoControl',
            eventosAcceso: 'id, nombre, fecha, resultado, puntoControl, usuarioId, espacioId',
            espacios: 'id, nombre, ubicacion, capacidad, activo',
            edificios: 'id, numeroPisos, codigoEdificio',
            laboratorios: 'id, edificioId, numeroLaboratorio',
            salones: 'id, edificioId, numeroSalon',
            reglasAcceso: 'id, espacioId, horarioApertura, horarioCierre',
            beneficios: 'id, tipo, vigencia, fechaDeVencimiento',
            beneficiosUsuarios: '[usuarioId+beneficioId], usuarioId, beneficioId',
            anuncios: 'id, nombre, fecha, activo',
            usuariosEspacios: '[usuarioId+espacioId], usuarioId, espacioId',
            
            // Cola de sincronización para operaciones pendientes
            syncQueue: '++id, operationType, entityType, entityId, jsonData, createdAt, isSynced'
        });
    }
}

// Crear instancia única de la base de datos
const db = new GateKeepDatabase();

export default db;

