import apiClient from '@/lib/axios-offline-interceptor';
import { URLService } from "./urlService";
import { isOnline } from '@/lib/sync';
import { obtenerReglasAccesoLocales, obtenerReglaAccesoPorEspacioIdLocal } from '@/lib/sqlite-db';

const API_URL = URLService.getLink() + "reglas-acceso";

export class ReglaAccesoService {
  static async getReglasAcceso() {
    if (!isOnline()) {
      const reglasLocales = obtenerReglasAccesoLocales();
      return Promise.resolve({ data: reglasLocales, fromCache: true });
    }
    return apiClient.get(API_URL);
  }

  static async getReglaAccesoById(id: number) {
    if (!isOnline()) {
      const reglasLocales = obtenerReglasAccesoLocales();
      const regla = reglasLocales.find((r: any) => r.id === id);
      if (regla) {
        return Promise.resolve({ data: regla, fromCache: true });
      }
      return Promise.reject({ 
        isOffline: true, 
        message: 'Sin conexión y regla no encontrada en cache local' 
      });
    }
    return apiClient.get(`${API_URL}/${id}`);
  }

  static async getReglaAccesoPorEspacioId(espacioId: number) {
    if (!isOnline()) {
      const reglaLocal = obtenerReglaAccesoPorEspacioIdLocal(espacioId);
      if (reglaLocal) {
        return Promise.resolve({ data: reglaLocal, fromCache: true });
      }
      return Promise.reject({ 
        isOffline: true, 
        message: 'Sin conexión y regla no encontrada en cache local' 
      });
    }
    return apiClient.get(`${API_URL}/espacio/${espacioId}`);
  }

  static async crearReglaAcceso(data: {
    horarioApertura: string;
    horarioCierre: string;
    vigenciaApertura: string;
    vigenciaCierre: string;
    rolesPermitidos: string[];
    espacioId: number;
  }) {
    // Backend espera propiedades con PascalCase
    const payload = {
      HorarioApertura: data.horarioApertura,
      HorarioCierre: data.horarioCierre,
      VigenciaApertura: data.vigenciaApertura,
      VigenciaCierre: data.vigenciaCierre,
      RolesPermitidos: data.rolesPermitidos,
      EspacioId: data.espacioId,
    };
    return apiClient.post(API_URL, payload);
  }

  static async actualizarReglaAcceso(
    id: number,
    data: {
      horarioApertura: string;
      horarioCierre: string;
      vigenciaApertura: string;
      vigenciaCierre: string;
      rolesPermitidos: string[];
      espacioId: number;
    }
  ) {
    // Backend espera propiedades con PascalCase
    const payload = {
      HorarioApertura: data.horarioApertura,
      HorarioCierre: data.horarioCierre,
      VigenciaApertura: data.vigenciaApertura,
      VigenciaCierre: data.vigenciaCierre,
      RolesPermitidos: data.rolesPermitidos,
      EspacioId: data.espacioId,
    };
    return apiClient.put(`${API_URL}/${id}`, payload);
  }

  static async eliminarReglaAcceso(id: number) {
    return apiClient.delete(`${API_URL}/${id}`);
  }
}
