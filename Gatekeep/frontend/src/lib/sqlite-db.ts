/**
 * Gestor de SQLite local para modo offline en PWA
 * Usa sql.js (SQLite compilado a WebAssembly)
 * Persistencia: IndexedDB o localStorage
 */

import initSqlJs, { Database } from 'sql.js';

let SQL: any = null;
let db: Database | null = null;
const DB_STORE_NAME = 'gatekeep_offline_db';
const DB_KEY = 'sqlite_db';

/**
 * Inicializa la instancia de SQLite
 */
export async function initializeDatabase() {
  if (!SQL) {
    // Inicializar sql.js con configuración para el navegador
    SQL = await initSqlJs({
      locateFile: (file: string) => {
        // Intentar usar archivo local desde /public primero
        // Si no está disponible, usar CDN como fallback
        if (typeof window !== 'undefined') {
          // En el navegador, intentar ruta local
          const localPath = `/${file}`;
          return localPath;
        }
        // Fallback a CDN (útil en desarrollo o si el archivo no está en public)
        return `https://sql.js.org/dist/${file}`;
      }
    });
  }

  // Intentar cargar BD existente de IndexedDB
  const savedDb = await loadDatabaseFromStorage();
  
  if (savedDb) {
    const dbArray = Array.isArray(savedDb) ? new Uint8Array(savedDb) : savedDb;
    db = new SQL.Database(dbArray);
    console.log('✅ SQLite DB cargada desde IndexedDB');
  } else {
    // Crear nueva BD
    db = new SQL.Database();
    console.log('✅ SQLite DB nueva creada');
  }

  // Siempre asegurar que las tablas existan (CREATE TABLE IF NOT EXISTS es seguro)
  await createTables();

  return db;
}

/**
 * Crea las tablas espejo de la BD principal
 */
async function createTables() {
  const statements = [
    // Cache de usuarios
    `CREATE TABLE IF NOT EXISTS usuarios (
      id INTEGER PRIMARY KEY,
      rut TEXT,
      email TEXT UNIQUE,
      nombre TEXT,
      apellido TEXT,
      rol TEXT,
      credentialActiva BOOLEAN,
      ultimaActualizacion TEXT
    )`,

    // Cache de espacios
    `CREATE TABLE IF NOT EXISTS espacios (
      id INTEGER PRIMARY KEY,
      nombre TEXT,
      tipo TEXT,
      ubicacion TEXT,
      edificioId INTEGER,
      edificioNombre TEXT,
      ultimaActualizacion TEXT
    )`,

    // Cache de reglas de acceso
    `CREATE TABLE IF NOT EXISTS reglas_acceso (
      id INTEGER PRIMARY KEY,
      espacioId INTEGER,
      perfil TEXT,
      horaInicio TEXT,
      horaFin TEXT,
      activa BOOLEAN,
      ultimaActualizacion TEXT
    )`,

    // Cache de beneficios
    `CREATE TABLE IF NOT EXISTS beneficios (
      id INTEGER PRIMARY KEY,
      nombre TEXT,
      tipo TEXT,
      fechaVigenciaInicio TEXT,
      fechaVigenciaFin TEXT,
      cuposDisponibles INTEGER,
      activo BOOLEAN,
      ultimaActualizacion TEXT
    )`,

    // Cache de notificaciones
    `CREATE TABLE IF NOT EXISTS notificaciones (
      id TEXT PRIMARY KEY,
      tipo TEXT,
      titulo TEXT,
      mensaje TEXT,
      leido BOOLEAN,
      fechaCreacion TEXT
    )`,

    // Eventos offline pendientes de sincronización
    `CREATE TABLE IF NOT EXISTS eventos_offline (
      idTemporal TEXT PRIMARY KEY,
      tipoEvento TEXT,
      datosEvento TEXT,
      fechaCreacion TEXT,
      intentos INTEGER DEFAULT 0,
      estado TEXT DEFAULT 'Pendiente',
      idServidor TEXT
    )`,

    // Metadatos de sincronización
    `CREATE TABLE IF NOT EXISTS sync_metadata (
      clave TEXT PRIMARY KEY,
      valor TEXT,
      fechaActualizacion TEXT
    )`,
  ];

  for (const stmt of statements) {
    if (db) {
      db.run(stmt);
    }
  }

  // Migrar tablas existentes agregando columnas si no existen
  await migrateTables();

  console.log('✅ Tablas creadas en SQLite');
}

/**
 * Migra tablas existentes agregando columnas nuevas si no existen
 */
async function migrateTables() {
  if (!db) return;

  try {
    // Agregar campo 'rut' a usuarios si no existe
    try {
      db.run('ALTER TABLE usuarios ADD COLUMN rut TEXT');
      console.log('✅ Columna rut agregada a usuarios');
    } catch (e: any) {
      // La columna ya existe, ignorar error
      if (!e.message?.includes('duplicate column')) {
        console.warn('⚠️ Error agregando columna rut:', e.message);
      }
    }

    // Agregar campos 'edificioId' y 'edificioNombre' a espacios si no existen
    try {
      db.run('ALTER TABLE espacios ADD COLUMN edificioId INTEGER');
      console.log('✅ Columna edificioId agregada a espacios');
    } catch (e: any) {
      if (!e.message?.includes('duplicate column')) {
        console.warn('⚠️ Error agregando columna edificioId:', e.message);
      }
    }

    try {
      db.run('ALTER TABLE espacios ADD COLUMN edificioNombre TEXT');
      console.log('✅ Columna edificioNombre agregada a espacios');
    } catch (e: any) {
      if (!e.message?.includes('duplicate column')) {
        console.warn('⚠️ Error agregando columna edificioNombre:', e.message);
      }
    }

    // Agregar campo 'idServidor' a eventos_offline si no existe
    try {
      db.run('ALTER TABLE eventos_offline ADD COLUMN idServidor TEXT');
      console.log('✅ Columna idServidor agregada a eventos_offline');
    } catch (e: any) {
      if (!e.message?.includes('duplicate column')) {
        console.warn('⚠️ Error agregando columna idServidor:', e.message);
      }
    }
  } catch (error) {
    console.error('❌ Error en migración de tablas:', error);
  }
}

/**
 * Guarda la BD actual en IndexedDB para persistencia
 */
export async function saveDatabaseToStorage() {
  if (!db) return;

  const data = db.export();
  const arr = Array.from(data);

  try {
    // Guardar en IndexedDB
    const request = indexedDB.open(DB_STORE_NAME, 1);
    
    request.onerror = () => console.error('❌ Error abriendo IndexedDB');
    request.onupgradeneeded = (e: IDBVersionChangeEvent) => {
      const target = (e.target as IDBOpenDBRequest).result;
      if (!target.objectStoreNames.contains('db')) {
        target.createObjectStore('db');
      }
    };

    request.onsuccess = () => {
      const database = request.result;
      const transaction = database.transaction(['db'], 'readwrite');
      const store = transaction.objectStore('db');
      store.put(arr, DB_KEY);
      console.log('✅ BD guardada en IndexedDB');
    };
  } catch (error) {
    console.error('❌ Error guardando BD:', error);
  }
}

/**
 * Carga la BD desde IndexedDB
 */
async function loadDatabaseFromStorage() {
  return new Promise((resolve) => {
    try {
      const request = indexedDB.open(DB_STORE_NAME, 1);

      request.onerror = () => resolve(null);
      request.onupgradeneeded = (e: IDBVersionChangeEvent) => {
        const target = e.target as IDBOpenDBRequest;
        const database = target.result;
        if (!database.objectStoreNames.contains('db')) {
          database.createObjectStore('db');
        }
      };

      request.onsuccess = () => {
        const database = request.result;
        const transaction = database.transaction(['db'], 'readonly');
        const store = transaction.objectStore('db');
        const getRequest = store.get(DB_KEY);

        getRequest.onsuccess = () => {
          resolve(getRequest.result || null);
        };
        getRequest.onerror = () => resolve(null);
      };
    } catch (error) {
      console.error('❌ Error cargando BD:', error);
      resolve(null);
    }
  });
}

/**
 * Inserta o actualiza un usuario en cache
 */
export function cacheUsuario(usuario: any): void {
  if (!db) return;

  const stmt = db.prepare(`
    INSERT OR REPLACE INTO usuarios 
    (id, rut, email, nombre, apellido, rol, credentialActiva, ultimaActualizacion)
    VALUES (?, ?, ?, ?, ?, ?, ?, ?)
  `);

  stmt.bind([
    usuario.id,
    usuario.rut || null,
    usuario.email,
    usuario.nombre,
    usuario.apellido,
    usuario.rol,
    usuario.credentialActiva,
    usuario.ultimaActualizacion || new Date().toISOString(),
  ]);

  stmt.step();
  stmt.free();
  saveDatabaseToStorage();
}

/**
 * Obtiene usuario del cache local
 */
export function getUsuarioLocal(id: number): any {
  if (!db) return null;

  const stmt = db.prepare('SELECT * FROM usuarios WHERE id = ?');
  stmt.bind([id]);

  let usuario = null;
  if (stmt.step()) {
    usuario = stmt.getAsObject();
  }
  stmt.free();

  return usuario;
}

/**
 * Registra un evento offline para sincronizar después
 */
export function recordOfflineEvent(tipoEvento: string, datosEvento: any): string {
  if (!db) return '';

  const idTemporal = `${tipoEvento}-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  const fechaCreacion = new Date().toISOString();

  const stmt = db.prepare(`
    INSERT INTO eventos_offline 
    (idTemporal, tipoEvento, datosEvento, fechaCreacion, intentos, estado)
    VALUES (?, ?, ?, ?, 0, 'Pendiente')
  `);

  stmt.bind([idTemporal, tipoEvento, JSON.stringify(datosEvento), fechaCreacion]);
  stmt.step();
  stmt.free();

  saveDatabaseToStorage();
  console.log(`📝 Evento offline registrado: ${idTemporal}`);

  return idTemporal;
}

/**
 * Obtiene todos los eventos offline pendientes
 */
export function getPendingOfflineEvents() {
  if (!db) return [];

  const stmt = db.prepare(
    'SELECT * FROM eventos_offline WHERE estado = ? ORDER BY fechaCreacion ASC'
  );
  stmt.bind(['Pendiente']);

  const eventos = [];
  while (stmt.step()) {
    const evento = stmt.getAsObject();
    const datosStr = String(evento.datosEvento || '{}');
    evento.datosEvento = JSON.parse(datosStr);
    eventos.push(evento);
  }
  stmt.free();

  return eventos;
}

/**
 * Marca un evento como sincronizado
 */
export function markEventoAsSynced(idTemporal: string, idServidor?: string): void {
  if (!db) return;

  if (idServidor) {
    const stmt = db.prepare(
      'UPDATE eventos_offline SET estado = ?, idServidor = ? WHERE idTemporal = ?'
    );
    stmt.bind(['Procesado', idServidor, idTemporal]);
    stmt.step();
    stmt.free();
  } else {
    const stmt = db.prepare(
      'UPDATE eventos_offline SET estado = ? WHERE idTemporal = ?'
    );
    stmt.bind(['Procesado', idTemporal]);
    stmt.step();
    stmt.free();
  }

  saveDatabaseToStorage();
}

/**
 * Guarda metadatos de sincronización
 */
export function setSyncMetadata(clave: string, valor: string): void {
  if (!db) return;

  const stmt = db.prepare(`
    INSERT OR REPLACE INTO sync_metadata 
    (clave, valor, fechaActualizacion)
    VALUES (?, ?, ?)
  `);

  stmt.bind([clave, valor, new Date().toISOString()]);
  stmt.step();
  stmt.free();

  saveDatabaseToStorage();
}

/**
 * Obtiene metadatos de sincronización
 */
export function getSyncMetadata(clave: string): string | null {
  if (!db) return null;

  const stmt = db.prepare('SELECT valor FROM sync_metadata WHERE clave = ?');
  stmt.bind([clave]);

  let valor: string | null = null;
  if (stmt.step()) {
    const row = stmt.getAsObject();
    valor = row.valor ? String(row.valor) : null;
  }
  stmt.free();

  return valor;
}

/**
 * Sincroniza datos descargados desde el servidor
 */
export function syncDataFromServer(syncPayload: any): void {
  if (!db) return;

  // Sincronizar usuarios
  if (syncPayload.usuarios) {
    for (const usuario of syncPayload.usuarios) {
      cacheUsuario(usuario);
    }
  }

  // Sincronizar espacios
  if (syncPayload.espacios) {
    const stmtEspacios = db.prepare(`
      INSERT OR REPLACE INTO espacios 
      (id, nombre, tipo, ubicacion, edificioId, edificioNombre, ultimaActualizacion)
      VALUES (?, ?, ?, ?, ?, ?, ?)
    `);

    for (const espacio of syncPayload.espacios) {
      stmtEspacios.bind([
        espacio.id,
        espacio.nombre,
        espacio.tipo,
        espacio.ubicacion,
        espacio.edificioId || null,
        espacio.edificioNombre || null,
        espacio.ultimaActualizacion,
      ]);
      stmtEspacios.step();
    }
    stmtEspacios.free();
  }

  // Sincronizar reglas de acceso
  if (syncPayload.reglasAcceso) {
    const stmtReglas = db.prepare(`
      INSERT OR REPLACE INTO reglas_acceso 
      (id, espacioId, perfil, horaInicio, horaFin, activa, ultimaActualizacion)
      VALUES (?, ?, ?, ?, ?, ?, ?)
    `);

    for (const regla of syncPayload.reglasAcceso) {
      stmtReglas.bind([
        regla.id,
        regla.espacioId,
        regla.perfil,
        regla.horaInicio,
        regla.horaFin,
        regla.activa,
        regla.ultimaActualizacion,
      ]);
      stmtReglas.step();
    }
    stmtReglas.free();
  }

  // Sincronizar beneficios
  if (syncPayload.beneficios) {
    const stmtBeneficios = db.prepare(`
      INSERT OR REPLACE INTO beneficios 
      (id, nombre, tipo, fechaVigenciaInicio, fechaVigenciaFin, cuposDisponibles, activo, ultimaActualizacion)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?)
    `);

    for (const beneficio of syncPayload.beneficios) {
      stmtBeneficios.bind([
        beneficio.id,
        beneficio.nombre,
        beneficio.tipo,
        beneficio.fechaVigenciaInicio,
        beneficio.fechaVigenciaFin,
        beneficio.cuposDisponibles,
        beneficio.activo,
        beneficio.ultimaActualizacion,
      ]);
      stmtBeneficios.step();
    }
    stmtBeneficios.free();
  }

  // Sincronizar notificaciones
  if (syncPayload.notificaciones) {
    const stmtNotif = db.prepare(`
      INSERT OR REPLACE INTO notificaciones 
      (id, tipo, titulo, mensaje, leido, fechaCreacion)
      VALUES (?, ?, ?, ?, ?, ?)
    `);

    for (const notif of syncPayload.notificaciones) {
      stmtNotif.bind([
        notif.id,
        notif.tipo,
        notif.titulo,
        notif.mensaje,
        notif.leido,
        notif.fechaCreacion,
      ]);
      stmtNotif.step();
    }
    stmtNotif.free();
  }

  saveDatabaseToStorage();
  console.log('✅ Datos sincronizados desde servidor');
}

/**
 * Limpia eventos offline ya procesados
 */
export function clearProcessedEvents() {
  if (!db) return;

  const stmt = db.prepare('DELETE FROM eventos_offline WHERE estado = ?');
  stmt.bind(['Procesado']);
  stmt.step();
  stmt.free();

  saveDatabaseToStorage();
  console.log('✅ Eventos procesados eliminados');
}

/**
 * Cuenta los eventos offline pendientes de sincronización
 */
export function contarEventosPendientes(): number {
  if (!db) return 0;

  const stmt = db.prepare(
    'SELECT COUNT(*) as count FROM eventos_offline WHERE estado = ?'
  );
  stmt.bind(['Pendiente']);
  stmt.step();
  const count = stmt.getAsObject().count;
  stmt.free();

  return count;
}

/**
 * Obtiene todos los espacios del cache local
 */
export function obtenerEspaciosLocales(): any[] {
  if (!db) return [];

  const stmt = db.prepare('SELECT * FROM espacios ORDER BY nombre ASC');
  const espacios = [];

  while (stmt.step()) {
    espacios.push(stmt.getAsObject());
  }
  stmt.free();

  return espacios;
}

/**
 * Obtiene un espacio específico del cache local por ID
 */
export function obtenerEspacioPorId(id: number): any {
  if (!db) return null;

  const stmt = db.prepare('SELECT * FROM espacios WHERE id = ?');
  stmt.bind([id]);

  let espacio = null;
  if (stmt.step()) {
    espacio = stmt.getAsObject();
  }
  stmt.free();

  return espacio;
}

/**
 * Obtiene resumen de estado de la BD local
 */
export function getOfflineStatus() {
  if (!db) {
    return {
      eventosOfflinePendientes: 0,
      usuariosEnCache: 0,
      totalEventosOffline: 0,
      ultimaSincronizacion: null,
    };
  }

  const pendingStmt = db.prepare(
    'SELECT COUNT(*) as count FROM eventos_offline WHERE estado = ?'
  );
  pendingStmt.bind(['Pendiente']);
  pendingStmt.step();
  const pending = pendingStmt.getAsObject().count;
  pendingStmt.free();

  const usuariosStmt = db.prepare('SELECT COUNT(*) as count FROM usuarios');
  usuariosStmt.step();
  const usuarios = usuariosStmt.getAsObject().count;
  usuariosStmt.free();

  const eventosStmt = db.prepare('SELECT COUNT(*) as count FROM eventos_offline');
  eventosStmt.step();
  const totalEventos = eventosStmt.getAsObject().count;
  eventosStmt.free();

  return {
    eventosOfflinePendientes: pending,
    usuariosEnCache: usuarios,
    totalEventosOffline: totalEventos,
    ultimaSincronizacion: getSyncMetadata('ultimaSincronizacion'),
  };
}
