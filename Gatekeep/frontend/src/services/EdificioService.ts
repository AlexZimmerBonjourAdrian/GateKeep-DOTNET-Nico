import axios from "axios";
import { URLService } from "./urlService";
import { SecurityService } from "./securityService";

const API_URL = URLService.getLink() + "espacios/edificios";

export class EdificioService {
  static getAuthHeaders() {
    const token = SecurityService.getToken();
    return {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    };
  }

  static async getEdificios() {
    return axios.get(API_URL, this.getAuthHeaders());
  }

  static async getEdificioById(id: number) {
    return axios.get(`${API_URL}/${id}` , this.getAuthHeaders());
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
    return axios.post(API_URL, payload, this.getAuthHeaders());
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
    return axios.put(`${API_URL}/${id}`, payload, this.getAuthHeaders());
  }

  static async deleteEdificio(id: number) {
    return axios.delete(`${API_URL}/${id}`, this.getAuthHeaders());
  }
}
