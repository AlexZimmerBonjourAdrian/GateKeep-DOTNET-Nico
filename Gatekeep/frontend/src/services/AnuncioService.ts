import axios from "axios";
import { URLService } from "./urlService";    
import { SecurityService } from "./securityService";

const API_URL = URLService.getLink() + "anuncios";

export class AnuncioService {
  static getAuthHeaders() {
    const token = SecurityService.getToken();
    return {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    };
  }

  static getAnuncios() {
    return axios.get(API_URL);
  }

  static getAnuncio(id: number) {
    return axios.get(`${API_URL}/${id}`);
  }

  static createAnuncio(data: { nombre: string; fecha: string; descripcion?: string; puntoControl?: string }) {
    return axios.post(API_URL, data, this.getAuthHeaders());
  }
}