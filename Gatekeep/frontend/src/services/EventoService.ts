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
    return axios.post(API_URL, data, this.getAuthHeaders());
  }
}