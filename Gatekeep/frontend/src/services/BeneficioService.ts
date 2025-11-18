import axios from "axios";
import { URLService } from "./urlService";
import { SecurityService } from "./securityService";

const API_URL = URLService.getLink() + "beneficios";

export class BeneficioService {
  static getAuthHeaders() {
    const token = SecurityService.getToken();
    return {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    };
  }

  static async getBeneficios() {
    return axios.get(API_URL, this.getAuthHeaders());
  }

  static async getBeneficioById(id: number) {
    return axios.get(`${API_URL}/${id}`, this.getAuthHeaders());
  }

  static async crearBeneficio(data: {
    tipo: number;
    vigencia: boolean;
    fechaDeVencimiento: string;
    cupos: number;
  }) {
    const payload = {
      Tipo: data.tipo,
      Vigencia: data.vigencia,
      FechaDeVencimiento: data.fechaDeVencimiento,
      Cupos: data.cupos,
    };
    return axios.post(API_URL, payload, this.getAuthHeaders());
  }

  static async actualizarBeneficio(
    id: number,
    data: {
      tipo: number;
      vigencia: boolean;
      fechaDeVencimiento: string;
      cupos: number;
    }
  ) {
    const payload = {
      Tipo: data.tipo,
      Vigencia: data.vigencia,
      FechaDeVencimiento: data.fechaDeVencimiento,
      Cupos: data.cupos,
    };
    return axios.put(`${API_URL}/${id}`, payload, this.getAuthHeaders());
  }

  static async eliminarBeneficio(id: number) {
    return axios.delete(`${API_URL}/${id}`, this.getAuthHeaders());
  }
}
