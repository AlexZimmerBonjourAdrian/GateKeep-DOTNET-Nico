import axios from "axios";
import { URLService } from "./urlService";
import { SecurityService } from "./securityService";

const API_URL = URLService.getLink() + "reglas-acceso";

export class ReglaAccesoService {
  static getAuthHeaders() {
    const token = SecurityService.getToken();
    return {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    };
  }

  static async getReglasAcceso() {
    return axios.get(API_URL, this.getAuthHeaders());
  }

  static async getReglaAccesoById(id: number) {
    return axios.get(`${API_URL}/${id}`, this.getAuthHeaders());
  }

  static async getReglaAccesoPorEspacioId(espacioId: number) {
    return axios.get(`${API_URL}/espacio/${espacioId}`, this.getAuthHeaders());
  }

  static async crearReglaAcceso(data: {
    horarioApertura: string;
    horarioCierre: string;
    vigenciaApertura: string;
    vigenciaCierre: string;
    rolesPermitidos: string[];
    espacioId: number;
  }) {
    return axios.post(API_URL, data, this.getAuthHeaders());
  }

  static async actualizarReglaAcceso(
    id: number,
    data: {
      horarioApertura: string;
      horarioCierre: string;
      vigenciaApertura: string;
      vigenciaCierre: string;
      rolesPermitidos: string[];
      espacioId: number;
    }
  ) {
    return axios.put(`${API_URL}/${id}`, data, this.getAuthHeaders());
  }

  static async eliminarReglaAcceso(id: number) {
    return axios.delete(`${API_URL}/${id}`, this.getAuthHeaders());
  }
}
