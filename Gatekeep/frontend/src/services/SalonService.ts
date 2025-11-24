import apiClient from '@/lib/axios-offline-interceptor';
import { URLService } from "./urlService";    
import { isOnline } from '@/lib/sync';
import { obtenerEspaciosLocales, obtenerEspacioPorId } from '@/lib/sqlite-db';

const API_URL = URLService.getLink() + "espacios/salones";

export class SalonService {
  static async getSalones() {
    if (!isOnline()) {
      // Filtrar solo salones del cache local
      const espaciosLocales = obtenerEspaciosLocales();
      const salones = espaciosLocales.filter((e: any) => e.tipo === 'Salon' || e.tipo === 'salon');
      return Promise.resolve({ data: salones, fromCache: true });
    }
    return apiClient.get(API_URL);
  }

  static async getSalon(id: number) {
    if (!isOnline()) {
      const espacioLocal = obtenerEspacioPorId(id);
      if (espacioLocal && (espacioLocal.tipo === 'Salon' || espacioLocal.tipo === 'salon')) {
        return Promise.resolve({ data: espacioLocal, fromCache: true });
      }
      return Promise.reject({ 
        isOffline: true, 
        message: 'Sin conexión y salón no encontrado en cache local' 
      });
    }
    return apiClient.get(`${API_URL}/${id}`);
  }

  static async getSalonById(id: number) {
    return this.getSalon(id);
  }

  static async createSalon(data: { 
    nombre: string; 
    capacidad: number; 
    numeroSalon: number; 
    edificioId: number;
    ubicacion: string;
    descripcion?: string;
    activo?: boolean;
  }) {
    return apiClient.post(API_URL, data);
  }

  static async updateSalon(id: number, data: { 
    nombre: string; 
    capacidad: number; 
    numeroSalon: number; 
    edificioId: number;
    ubicacion: string;
    descripcion?: string;
    activo?: boolean;
  }) {
    return apiClient.put(`${API_URL}/${id}`, data);
  }

  static async deleteSalon(id: number) {
    return apiClient.delete(`${API_URL}/${id}`);
  }
}
