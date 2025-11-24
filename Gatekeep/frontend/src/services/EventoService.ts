import apiClient from '@/lib/axios-offline-interceptor';
import { URLService } from "./urlService";

const API_URL = URLService.getLink() + "eventos";

export class EventoService {
  static getEventos() {
    return apiClient.get(API_URL);
  }

  static getEvento(id: number) {
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