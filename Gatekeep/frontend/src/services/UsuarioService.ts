import axios from "axios";
import { URLService } from "./urlService";

const API_URL = URLService.getLink() + "usuarios/";
const LOGIN_API_URL = "http://localhost:5011/auth/login";


export class UsuarioService {

  static getUsuario(id : number) {
    return axios.get(API_URL + id);
  }

  static getNotificacionesSinLeer(id : number) {
    return axios.get(API_URL + id + "/notificaciones/no-leidas/count");
  }

  static login(credentials: { email: string; password: string }) {
    return axios.post(LOGIN_API_URL, credentials);
  }
}