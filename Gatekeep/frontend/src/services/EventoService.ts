import axios from "axios";
import { URLService } from "./urlService";    

const API_URL = URLService.getLink() + "eventos/";

export class EventoService {
  static getEventos() {
    return axios.get(API_URL);
  }

  static getEvento(id: number) {
    return axios.get(API_URL + id);
  }
}