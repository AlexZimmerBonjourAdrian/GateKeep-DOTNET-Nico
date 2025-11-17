import axios from "axios";
import { URLService } from "./urlService";
import { SecurityService } from "./securityService";

const API_URL = URLService.getLink() + "notificaciones";
const USUARIOS_URL = URLService.getLink() + "usuarios";

export class NotificacionService {
  static getAuthHeaders() {
    const token = SecurityService.getToken();
    return {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    };
  }

  // Obtener todas las notificaciones (admin/funcionario)
  static async getNotificaciones() {
    return axios.get(API_URL, this.getAuthHeaders());
  }

  // Obtener notificación por ID
  static async getNotificacionById(id: string) {
    return axios.get(`${API_URL}/${id}`, this.getAuthHeaders());
  }

  // Obtener notificaciones de un usuario específico
  static async getNotificacionesPorUsuario(usuarioId: number) {
    try {
      const response = await axios.get(`${USUARIOS_URL}/${usuarioId}/notificaciones`, this.getAuthHeaders());
      return response.data;
    } catch (error: any) {
      console.error('Error al obtener notificaciones del usuario:', error.message);
      throw error;
    }
  }

  // Obtener una notificación específica de un usuario (DESHABILITADO - Error 500)
  // static async getNotificacionUsuario(usuarioId: number, notificacionId: string) {
  //   return axios.get(`${USUARIOS_URL}/${usuarioId}/notificaciones/${notificacionId}`, this.getAuthHeaders());
  // }

  // Marcar notificación como leída
  static async marcarComoLeida(usuarioId: number, notificacionId: string) {
    try {
      const response = await axios.put(
        `${USUARIOS_URL}/${usuarioId}/notificaciones/${notificacionId}/leer`,
        {},
        this.getAuthHeaders()
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
      const response = await axios.get(url, this.getAuthHeaders());
      return response.data?.count || 0;
    } catch (error: any) {
      console.error('Error al obtener conteo de notificaciones:', error.message);
      return 0;
    }
  }

  // Crear notificación (solo admin/funcionario)
  static async crearNotificacion(data: { mensaje: string; tipo: string }) {
    return axios.post(API_URL, data, this.getAuthHeaders());
  }

  // Actualizar notificación (solo admin/funcionario)
  static async actualizarNotificacion(id: string, data: { mensaje: string; tipo: string }) {
    return axios.put(`${API_URL}/${id}`, data, this.getAuthHeaders());
  }

  // Eliminar notificación (solo admin)
  static async eliminarNotificacion(id: string) {
    return axios.delete(`${API_URL}/${id}`, this.getAuthHeaders());
  }
}
