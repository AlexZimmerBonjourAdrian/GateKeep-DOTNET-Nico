import apiClient from '@/lib/axios-offline-interceptor';
import { URLService } from "./urlService";
import { isOnline } from '@/lib/sync';
import { getUsuarioLocal, obtenerUsuariosLocales } from '@/lib/sqlite-db';

// Base URL del backend y recursos específicos
const API_URL = URLService.getLink(); // URL dinámica según entorno (producción o desarrollo) - incluye /api/
// Todas las rutas del backend están bajo /api/
const USUARIOS_URL = `${API_URL}usuarios/`;  // → /api/usuarios/
const AUTH_URL = `${API_URL}auth/`;           // → /api/auth/

export class UsuarioService {
  /**
   * Registro de usuario nuevo.
   */
  static register(payload: { 
    email: string; 
    password: string; 
    confirmPassword: string; 
    nombre: string; 
    apellido: string; 
    telefono?: string | null; 
    rol: string; 
  }) {
    return apiClient.post(AUTH_URL + "register", payload);
  }
  /**
   * Login de usuario con email y contraseña.
   */
  static login(credentials: { email: string; password: string }) {
    return apiClient.post(AUTH_URL + "login", credentials);
  }

  /**
   * Obtiene un usuario por ID (valida que el backend autorice si no es el propio).
   * Si está offline, intenta obtener desde cache local.
   */
  static async getUsuario(id: number) {
    if (!isOnline()) {
      // Intentar obtener desde cache local
      const usuarioLocal = getUsuarioLocal(id);
      if (usuarioLocal) {
        return Promise.resolve({ data: usuarioLocal, fromCache: true });
      }
      // Si no está en cache, rechazar con error offline
      return Promise.reject({ 
        isOffline: true, 
        message: 'Sin conexión y usuario no encontrado en cache local' 
      });
    }
    return apiClient.get(USUARIOS_URL + id);
  }

  /**
   * Actualiza datos básicos del usuario (Nombre, Apellido, Telefono)
   */
  static updateUsuario(id: number, data: { nombre: string; apellido: string; telefono?: string | null }) {
    return apiClient.put(USUARIOS_URL + id, data);
  }

  /** Actualiza el usuario actual (resuelve id desde cache/token). */
  static async updateUsuarioActual(data: { nombre: string; apellido: string; telefono?: string | null }) {
    const current = await this.getUsuarioActual();
    if (!current?.id) throw new Error("No hay usuario autenticado");
    const resp = await this.updateUsuario(current.id, data);
    // refrescar cache local
    localStorage.setItem("user", JSON.stringify(resp.data));
    return resp.data;
  }

  /**
   * Devuelve el usuario actual leyendo primero de localStorage (cache) y opcionalmente refetch del backend.
   */
  static async getUsuarioActual(options: { refresh?: boolean } = {}) {
    const cached = localStorage.getItem("user");
    let parsed = cached ? JSON.parse(cached) : null;
    if (!parsed) {
      // Si no hay cache necesitamos el id. Lo extraemos del token decodificando (simple parse) o abortamos.
      const token = localStorage.getItem("token");
  // Fallback: si el login guardó directamente el id en localStorage bajo la clave "id" o "userId"
  const storedId = localStorage.getItem("id") || localStorage.getItem("userId");
      if (!token) return null;
      try {
        const payloadPart = token.split(".")[1];
        const json = JSON.parse(atob(payloadPart));
        // Intentar múltiples posibles nombres de claim para el identificador
        const idClaim = json["nameid"] || json["sub"] || json["nameidentifier"] || json["Id"];
        let resolvedId: number | null = null;
        if (idClaim) {
          resolvedId = Number(idClaim);
        } else if (storedId) {
          resolvedId = Number(storedId);
          console.debug("getUsuarioActual: usando storedId porque no se encontró claim en el token", storedId);
        }
        if (!resolvedId || Number.isNaN(resolvedId)) {
          console.warn("getUsuarioActual: no se pudo resolver el id del usuario desde token ni storedId");
          return null;
        }
        const resp = await this.getUsuario(resolvedId);
        parsed = resp.data;
        localStorage.setItem("user", JSON.stringify(parsed));
        return parsed;
      } catch {
        // Si falla la decodificación, intentar usar storedId directamente si existe
        if (storedId) {
          try {
            const resp = await this.getUsuario(Number(storedId));
            parsed = resp.data;
            localStorage.setItem("user", JSON.stringify(parsed));
            return parsed;
          } catch (innerErr) {
            console.error("getUsuarioActual: error usando storedId tras fallo de decodificación", innerErr);
          }
        }
        return null;
      }
    }
    if (options.refresh && parsed?.id) {
      try {
        const resp = await this.getUsuario(parsed.id);
        parsed = resp.data;
        localStorage.setItem("user", JSON.stringify(parsed));
      } catch {
        // Ignorar errores de refresh
      }
    }
    return parsed;
  }

  /**
   * Cantidad de notificaciones sin leer para un usuario.
   */
  static getNotificacionesSinLeer(id: number) {
    return apiClient.get(USUARIOS_URL + id + "/notificaciones/no-leidas/count");
  }

  /**
   * Obtiene el QR del JWT actual como Blob.
   * Opcionalmente se puede pasar el token manual (para casos específicos) y tamaño.
   */
  static async getAuthQrBlob(params?: { token?: string; width?: number; height?: number }) {
    const query: string[] = [];
    if (params?.token) query.push(`token=${encodeURIComponent(params.token)}`);
    if (params?.width) query.push(`w=${params.width}`);
    if (params?.height) query.push(`h=${params.height}`);
    const q = query.length ? `?${query.join("&")}` : "";

  const response = await apiClient.get(AUTH_URL + `qr${q}`, { responseType: "blob" });
    return response.data as Blob;
  }

  /**
   * Helper para generar una URL temporal (Object URL) del QR para usar directamente en <img />.
   * Recuerda revocar la URL luego de usarla.
   */
  static async getAuthQrUrl(params?: { token?: string; width?: number; height?: number }) {
    const blob = await this.getAuthQrBlob(params);
    return URL.createObjectURL(blob);
  }

  /**
   * Obtiene todos los usuarios (solo para administradores).
   * Si está offline, retorna usuarios del cache local.
   */
  static async getUsuarios() {
    if (!isOnline()) {
      const usuariosLocales = obtenerUsuariosLocales();
      return Promise.resolve({ data: usuariosLocales, fromCache: true });
    }
    return apiClient.get(USUARIOS_URL);
  }

  /**
   * Crea un nuevo usuario (solo para administradores).
   */
  static createUsuario(payload: {
    email: string;
    password: string;
    nombre: string;
    apellido: string;
    telefono?: string | null;
    rol: string;
  }) {
    // Transformar el payload al formato esperado por el backend
    const backendPayload = {
      email: payload.email,
      contrasenia: payload.password,
      nombre: payload.nombre,
      apellido: payload.apellido,
      telefono: payload.telefono,
      rol: payload.rol
    };
    return apiClient.post(USUARIOS_URL, backendPayload);
  }

  /**
   * Elimina un usuario (solo para administradores).
   */
  static deleteUsuario(id: number) {
    return apiClient.delete(USUARIOS_URL + id);
  }
}

export default UsuarioService;