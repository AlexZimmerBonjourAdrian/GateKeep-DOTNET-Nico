import axios, { AxiosInstance } from "axios";
import type { InternalAxiosRequestConfig } from "axios";
import { URLService } from "./urlService";

// Base URL del backend y recursos específicos
const API_URL = URLService.getLink(); // URL dinámica según entorno (producción o desarrollo) - incluye /api/
// Todas las rutas del backend están bajo /api/
const USUARIOS_URL = `${API_URL}usuarios/`;  // → /api/usuarios/
const AUTH_URL = `${API_URL}auth/`;           // → /api/auth/

// Instancia Axios para usuarios (reutiliza token si existe)
const api: AxiosInstance = axios.create({
  baseURL: API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem("token");
  if (token) {
    const h: any = config.headers ?? {};
    if (typeof h.set === "function") {
      h.set("Authorization", `Bearer ${token}`);
    } else {
      h["Authorization"] = `Bearer ${token}`;
    }
    (config as any).headers = h;
  }
  return config;
});

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
    return axios.post(AUTH_URL + "register", payload);
  }
  /**
   * Login de usuario con email y contraseña.
   */
  static login(credentials: { email: string; password: string }) {
    return axios.post(AUTH_URL + "login", credentials);
  }

  /**
   * Obtiene un usuario por ID (valida que el backend autorice si no es el propio).
   */
  static getUsuario(id: number) {
    return api.get(USUARIOS_URL + id);
  }

  /**
   * Actualiza datos básicos del usuario (Nombre, Apellido, Telefono)
   */
  static updateUsuario(id: number, data: { nombre: string; apellido: string; telefono?: string | null }) {
    return api.put(USUARIOS_URL + id, data);
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
    return api.get(USUARIOS_URL + id + "/notificaciones/no-leidas/count");
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

  const response = await api.get(AUTH_URL + `qr${q}`, { responseType: "blob" });
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
}

export default UsuarioService;