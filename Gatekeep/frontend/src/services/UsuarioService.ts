import axios from "axios";
import { URLService } from "./urlService";

const API_URL = URLService.getLink() + "usuarios/";

export class UsuarioService {

  static getUsuario(id : number) {
    return axios.get(API_URL + id);
  }
}