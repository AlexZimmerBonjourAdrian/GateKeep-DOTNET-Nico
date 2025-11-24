import apiClient from '@/lib/axios-offline-interceptor';
import { URLService } from "./urlService";

const API_URL = URLService.getLink(); // Incluye /api/
const ACCESO_URL = `${API_URL}acceso/`; // → /api/acceso/

export class AccesoService {
  /**
   * Valida si un usuario tiene acceso a un espacio en un punto de control específico
   */
  static validarAcceso(payload: {
    usuarioId: number;
    espacioId: number;
    puntoControl: string;
  }) {
    return apiClient.post(ACCESO_URL + "validar", {
      UsuarioId: payload.usuarioId,
      EspacioId: payload.espacioId,
      PuntoControl: payload.puntoControl
    });
  }
}

export default AccesoService;
