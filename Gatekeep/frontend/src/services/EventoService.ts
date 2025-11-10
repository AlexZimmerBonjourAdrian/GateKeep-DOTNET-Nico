import axios from "axios";
import { URLService } from "./urlService";

const API_URL = URLService.getLink() + "evento"; // p.ej. http://localhost:5011/api/evento

export class EventoService {
  static getEventos() {
    return axios.get(API_URL);
  }
}