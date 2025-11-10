import axios from "axios";
import { URLService } from "./urlService";    

const API_URL = URLService.getLink() + "anuncios/";

export class AnuncioService {
  static getAnuncios() {
    return axios.get(API_URL);
  }
}