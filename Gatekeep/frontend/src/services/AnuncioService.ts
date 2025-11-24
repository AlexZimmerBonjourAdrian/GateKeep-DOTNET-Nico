import apiClient from '@/lib/axios-offline-interceptor';
import { URLService } from "./urlService";    
import { isOnline } from '@/lib/sync';
import { obtenerAnunciosLocales, obtenerAnuncioLocal } from '@/lib/sqlite-db';

const API_URL = URLService.getLink() + "anuncios";

export class AnuncioService {
  static async getAnuncios() {
    if (!isOnline()) {
      const anunciosLocales = obtenerAnunciosLocales();
      return Promise.resolve({ data: anunciosLocales, fromCache: true });
    }
    return apiClient.get(API_URL);
  }

  static async getAnuncio(id: number) {
    if (!isOnline()) {
      const anuncioLocal = obtenerAnuncioLocal(id);
      if (anuncioLocal) {
        return Promise.resolve({ data: anuncioLocal, fromCache: true });
  }
      return Promise.reject({ 
        isOffline: true, 
        message: 'Sin conexión y anuncio no encontrado en cache local' 
      });
    }
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