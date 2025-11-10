import axios from "axios";

const API_URL = "http://localhost:5011/api/";

export class URLService {
  static getLink() {
    return API_URL;
  }
}