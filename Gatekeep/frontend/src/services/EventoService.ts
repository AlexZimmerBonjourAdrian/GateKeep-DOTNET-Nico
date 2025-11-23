import axios from "axios";
import { URLService } from "./urlService";    
import { SecurityService } from "./securityService";

const API_URL = URLService.getLink() + "eventos";

export class EventoService {
  static getAuthHeaders() {
    const token = SecurityService.getToken();
    return {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    };
  }

  static getEventos() {
    return axios.get(API_URL, this.getAuthHeaders());
  }

  static getEvento(id: number) {
    return axios.get(`${API_URL}/${id}`, this.getAuthHeaders());
  }

  static createEvento(data: { nombre: string; fecha: string; resultado?: string; puntoControl?: string }) {
    const payload = {
      Nombre: data.nombre,
      Fecha: data.fecha,
      Resultado: data.resultado || '',
      PuntoControl: data.puntoControl || '',
      Activo: true
    };
    return axios.post(API_URL, payload, this.getAuthHeaders());
  }

  static updateEvento(id: number, data: { nombre: string; fecha: string; resultado?: string; puntoControl?: string }) {
    const payload = {
      Nombre: data.nombre,
      Fecha: data.fecha,
      Resultado: data.resultado || '',
      PuntoControl: data.puntoControl || '',
      Activo: true
    };
    return axios.put(`${API_URL}/${id}`, payload, this.getAuthHeaders());
  }

  static deleteEvento(id: number) {
    return axios.delete(`${API_URL}/${id}`, this.getAuthHeaders());
  }
}