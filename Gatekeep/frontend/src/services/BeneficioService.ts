import apiClient from '@/lib/axios-offline-interceptor';
import { URLService } from "./urlService";
import { isOnline } from '@/lib/sync';
import { obtenerBeneficiosLocales, obtenerBeneficioLocal } from '@/lib/sqlite-db';

const API_URL = URLService.getLink() + "beneficios";

export class BeneficioService {
  static async getBeneficios() {
    if (!isOnline()) {
      const beneficiosLocales = obtenerBeneficiosLocales();
      return Promise.resolve({ data: beneficiosLocales, fromCache: true });
    }
    return apiClient.get(API_URL);
  }

  static async getBeneficioById(id: number) {
    if (!isOnline()) {
      const beneficioLocal = obtenerBeneficioLocal(id);
      if (beneficioLocal) {
        return Promise.resolve({ data: beneficioLocal, fromCache: true });
      }
      return Promise.reject({ 
        isOffline: true, 
        message: 'Sin conexión y beneficio no encontrado en cache local' 
      });
    }
    return apiClient.get(`${API_URL}/${id}`);
  }

  static async crearBeneficio(data: {
    tipo: number;
    vigencia: boolean;
    fechaDeVencimiento: string;
    cupos: number;
  }) {
    const payload = {
      Tipo: data.tipo,
      Vigencia: data.vigencia,
      FechaDeVencimiento: data.fechaDeVencimiento,
      Cupos: data.cupos,
    };
    return apiClient.post(API_URL, payload);
  }

  static async actualizarBeneficio(
    id: number,
    data: {
      tipo: number;
      vigencia: boolean;
      fechaDeVencimiento: string;
      cupos: number;
    }
  ) {
    const payload = {
      Tipo: data.tipo,
      Vigencia: data.vigencia,
      FechaDeVencimiento: data.fechaDeVencimiento,
      Cupos: data.cupos,
    };
    return apiClient.put(`${API_URL}/${id}`, payload);
  }

  static async eliminarBeneficio(id: number) {
    return apiClient.delete(`${API_URL}/${id}`);
  }

  // PATCH /api/usuarios/{usuarioId}/beneficios/{beneficioId}/canjear
  static async canjearBeneficio(usuarioId: number, beneficioId: number, puntoControl: string) {
    const url = `${URLService.getLink()}usuarios/${usuarioId}/beneficios/${beneficioId}/canjear`;
    return apiClient.patch(url, { PuntoControl: puntoControl });
  }

  // GET /api/usuarios/{usuarioId}/beneficios/canjeados
  static async getBeneficiosCanjeados(usuarioId: number) {
    const url = `${URLService.getLink()}usuarios/${usuarioId}/beneficios/canjeados`;
    return apiClient.get(url);
  }
}
