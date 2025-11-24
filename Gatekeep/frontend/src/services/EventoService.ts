import apiClient from '@/lib/axios-offline-interceptor';
import { URLService } from "./urlService";    
import { isOnline } from '@/lib/sync';
import { obtenerEventosLocales, obtenerEventoLocal } from '@/lib/sqlite-db';

const API_URL = URLService.getLink() + "eventos";

export class EventoService {
  static async getEventos() {
    if (!isOnline()) {
      const eventosLocales = obtenerEventosLocales();
      return Promise.resolve({ data: eventosLocales, fromCache: true });
    }
    return apiClient.get(API_URL);
  }

  static async getEvento(id: number) {
    if (!isOnline()) {
      const eventoLocal = obtenerEventoLocal(id);
      if (eventoLocal) {
        return Promise.resolve({ data: eventoLocal, fromCache: true });
  }
      return Promise.reject({ 
        isOffline: true, 
        message: 'Sin conexión y evento no encontrado en cache local' 
      });
    }
    return apiClient.get(`${API_URL}/${id}`);
  }

  static createEvento(data: { nombre: string; fecha: string; resultado?: string; puntoControl?: string }) {
    const payload = {
      Nombre: data.nombre,
      Fecha: data.fecha,
      Resultado: data.resultado || '',
      PuntoControl: data.puntoControl || '',
      Activo: true
    };
    return apiClient.post(API_URL, payload);
  }

  static updateEvento(id: number, data: { nombre: string; fecha: string; resultado?: string; puntoControl?: string }) {
    const payload = {
      Nombre: data.nombre,
      Fecha: data.fecha,
      Resultado: data.resultado || '',
      PuntoControl: data.puntoControl || '',
      Activo: true
    };
    return apiClient.put(`${API_URL}/${id}`, payload);
  }

  static deleteEvento(id: number) {
    return apiClient.delete(`${API_URL}/${id}`);
  }
}