import axios from "axios";

const API_URL = "https://localhost:5001/api/";

export class URLService {
  static getLink() {
    return API_URL;
  }
}