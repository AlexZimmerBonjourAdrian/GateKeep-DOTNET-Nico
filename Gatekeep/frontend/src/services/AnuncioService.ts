import apiClient from '@/lib/axios-offline-interceptor';
import { URLService } from "./urlService";

const API_URL = URLService.getLink() + "anuncios";

export class AnuncioService {
  static getAnuncios() {
    return apiClient.get(API_URL);
  }

  static getAnuncio(id: number) {
    return apiClient.get(`${API_URL}/${id}`);
  }

  static createAnuncio(data: { nombre: string; fecha: string; descripcion?: string; puntoControl?: string }) {
    return apiClient.post(API_URL, data);
  }

  static updateAnuncio(id: number, data: { nombre: string; fecha: string; descripcion?: string; puntoControl?: string }) {
    return apiClient.put(`${API_URL}/${id}`, data);
  }

  static deleteAnuncio(id: number) {
    return apiClient.delete(`${API_URL}/${id}`);
  }
}