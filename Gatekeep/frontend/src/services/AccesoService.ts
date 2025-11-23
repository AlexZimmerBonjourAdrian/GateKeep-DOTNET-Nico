import axios, { AxiosInstance } from "axios";
import type { InternalAxiosRequestConfig } from "axios";
import { URLService } from "./urlService";

const API_URL = URLService.getLink(); // Incluye /api/
const ACCESO_URL = `${API_URL}acceso/`; // → /api/acceso/

// Instancia Axios con autenticación
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

export class AccesoService {
  /**
   * Valida si un usuario tiene acceso a un espacio en un punto de control específico
   */
  static validarAcceso(payload: {
    usuarioId: number;
    espacioId: number;
    puntoControl: string;
  }) {
    return api.post(ACCESO_URL + "validar", {
      UsuarioId: payload.usuarioId,
      EspacioId: payload.espacioId,
      PuntoControl: payload.puntoControl
    });
  }
}

export default AccesoService;
