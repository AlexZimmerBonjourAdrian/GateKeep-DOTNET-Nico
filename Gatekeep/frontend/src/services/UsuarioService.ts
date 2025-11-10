import axios, { AxiosInstance } from "axios";
import type { InternalAxiosRequestConfig } from "axios";
import { URLService } from "./urlService";

// Base URL del backend y recursos específicos
const API_URL = URLService.getLink(); // p.ej. http://localhost:5011/api/
const USUARIOS_URL = API_URL + "usuarios/";
const AUTH_URL = API_URL + "auth/";

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
   * Obtiene un usuario por ID (valida que el backend autorice si no es el propio).
   */
  static getUsuario(id: number) {
    return api.get(USUARIOS_URL + id);
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
      if (!token) return null;
      try {
        const payloadPart = token.split(".")[1];
        const json = JSON.parse(atob(payloadPart));
        const idClaim = json["nameid"] || json["sub"]; // según cómo emitiste el claim NameIdentifier
        if (!idClaim) return null;
        const resp = await this.getUsuario(Number(idClaim));
        parsed = resp.data;
        localStorage.setItem("user", JSON.stringify(parsed));
        return parsed;
      } catch {
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