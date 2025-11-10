import axios from "axios";
import { URLService } from "./urlService";

const API_URL = URLService.getLink() + "anuncio"; // p.ej. http://localhost:5011/api/evento

export class AnuncioService {
  static getAnuncios() {
    return axios.get(API_URL);
  }
}