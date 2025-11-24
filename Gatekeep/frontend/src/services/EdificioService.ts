import apiClient from '@/lib/axios-offline-interceptor';
import { URLService } from "./urlService";
import { isOnline } from '@/lib/sync';
import { obtenerEspaciosLocales, obtenerEspacioPorId } from '@/lib/sqlite-db';

const API_URL = URLService.getLink() + "espacios/edificios";

export class EdificioService {
  static async getEdificios() {
    if (!isOnline()) {
      // Filtrar solo edificios del cache local
      const espaciosLocales = obtenerEspaciosLocales();
      const edificios = espaciosLocales.filter((e: any) => e.tipo === 'Edificio' || e.tipo === 'edificio');
      return Promise.resolve({ data: edificios, fromCache: true });
    }
    return apiClient.get(API_URL);
  }

  static async getEdificioById(id: number) {
    if (!isOnline()) {
      const espacioLocal = obtenerEspacioPorId(id);
      if (espacioLocal && (espacioLocal.tipo === 'Edificio' || espacioLocal.tipo === 'edificio')) {
        return Promise.resolve({ data: espacioLocal, fromCache: true });
      }
      return Promise.reject({ 
        isOffline: true, 
        message: 'Sin conexión y edificio no encontrado en cache local' 
      });
    }
    return apiClient.get(`${API_URL}/${id}`);
  }

  static async crearEdificio(data: {
    nombre: string;
    descripcion?: string;
    ubicacion: string;
    capacidad: number;
    activo: boolean;
    numeroPisos: number;
    codigoEdificio?: string;
  }) {
    // Backend espera propiedades con PascalCase segun records (Nombre, Descripcion, Ubicacion, Capacidad, Activo, NumeroPisos, CodigoEdificio)
    const payload = {
      Nombre: data.nombre,
      Descripcion: data.descripcion,
      Ubicacion: data.ubicacion,
      Capacidad: data.capacidad,
      Activo: data.activo,
      NumeroPisos: data.numeroPisos,
      CodigoEdificio: data.codigoEdificio || null,
    };
    return apiClient.post(API_URL, payload);
  }

  static async updateEdificio(id: number, data: {
    nombre: string;
    descripcion?: string;
    ubicacion: string;
    capacidad: number;
    activo: boolean;
    numeroPisos: number;
    codigoEdificio?: string;
  }) {
    const payload = {
      Nombre: data.nombre,
      Descripcion: data.descripcion,
      Ubicacion: data.ubicacion,
      Capacidad: data.capacidad,
      Activo: data.activo,
      NumeroPisos: data.numeroPisos,
      CodigoEdificio: data.codigoEdificio || null,
    };
    return apiClient.put(`${API_URL}/${id}`, payload);
  }

  static async deleteEdificio(id: number) {
    return apiClient.delete(`${API_URL}/${id}`);
  }
}
