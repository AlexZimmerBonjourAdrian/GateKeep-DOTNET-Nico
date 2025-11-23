import axios from "axios";
import { URLService } from "./urlService";    
import { SecurityService } from "./securityService";

const API_URL = URLService.getLink() + "espacios/salones";

export class SalonService {
  static getAuthHeaders() {
    const token = SecurityService.getToken();
    return {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    };
  }

  static async getSalones() {
    return axios.get(API_URL, this.getAuthHeaders());
  }

  static async getSalon(id: number) {
    return axios.get(`${API_URL}/${id}`, this.getAuthHeaders());
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
    return axios.post(API_URL, data, this.getAuthHeaders());
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
    return axios.put(`${API_URL}/${id}`, data, this.getAuthHeaders());
  }

  static async deleteSalon(id: number) {
    return axios.delete(`${API_URL}/${id}`, this.getAuthHeaders());
  }
}
