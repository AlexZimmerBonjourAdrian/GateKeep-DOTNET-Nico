import apiClient from '@/lib/axios-offline-interceptor';
import { URLService } from "./urlService";

const API_URL = URLService.getLink() + "notificaciones";
const USUARIOS_URL = URLService.getLink() + "usuarios";

export class NotificacionService {
  // Obtener todas las notificaciones (admin/funcionario)
  static async getNotificaciones() {
    return apiClient.get(API_URL);
  }

  // Obtener notificación por ID
  static async getNotificacionById(id: string) {
    return apiClient.get(`${API_URL}/${id}`);
  }

  // Obtener notificaciones de un usuario específico
  static async getNotificacionesPorUsuario(usuarioId: number) {
    try {
      const response = await apiClient.get(`${USUARIOS_URL}/${usuarioId}/notificaciones`);
      return response.data;
    } catch (error: any) {
      console.error('Error al obtener notificaciones del usuario:', error.message);
      throw error;
    }
  }

  // Obtener una notificación específica de un usuario (DESHABILITADO - Error 500)
  // static async getNotificacionUsuario(usuarioId: number, notificacionId: string) {
  //   return apiClient.get(`${USUARIOS_URL}/${usuarioId}/notificaciones/${notificacionId}`);
  // }

  // Marcar notificación como leída
  static async marcarComoLeida(usuarioId: number, notificacionId: string) {
    try {
      const response = await apiClient.put(
        `${USUARIOS_URL}/${usuarioId}/notificaciones/${notificacionId}/leer`,
        {}
      );
      return response.data;
    } catch (error: any) {
      console.error('Error al marcar como leída:', error.message);
      throw error;
    }
  }

  // Marcar todas las notificaciones como leídas (TODO: verificar si existe endpoint)
  static async marcarTodasComoLeidas(usuarioId: number) {
    // Por ahora no disponible
    console.warn('Endpoint marcar-todas-leidas no implementado aún');
    return { success: false };
  }

  // Obtener cantidad de notificaciones no leídas
  static async getNoLeidasCount(usuarioId: number): Promise<number> {
    try {
      const url = `${USUARIOS_URL}/${usuarioId}/notificaciones/no-leidas/count`;
      const response = await apiClient.get(url);
      return response.data?.count || 0;
    } catch (error: any) {
      console.error('Error al obtener conteo de notificaciones:', error.message);
      return 0;
    }
  }

  // Crear notificación (solo admin/funcionario)
  static async crearNotificacion(data: { mensaje: string; tipo: string }) {
    return apiClient.post(API_URL, data);
  }

  // Actualizar notificación (solo admin/funcionario)
  static async actualizarNotificacion(id: string, data: { mensaje: string; tipo: string }) {
    return apiClient.put(`${API_URL}/${id}`, data);
  }

  // Eliminar notificación (solo admin)
  static async eliminarNotificacion(id: string) {
    return apiClient.delete(`${API_URL}/${id}`);
  }
}
