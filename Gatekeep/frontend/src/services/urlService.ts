import axios from "axios";

// Usar variable de entorno si está disponible, sino usar localhost para desarrollo
const getBaseUrl = () => {
  if (typeof window !== 'undefined') {
    // Cliente: usar variable de entorno o window.location
    const apiUrl = process.env.NEXT_PUBLIC_API_URL || window.location.origin;
    return `${apiUrl}/api/`;
  }
  // Servidor: usar variable de entorno o localhost
  return process.env.NEXT_PUBLIC_API_URL 
    ? `${process.env.NEXT_PUBLIC_API_URL}/api/`
    : "http://localhost:5011/api/";
};

const API_URL = getBaseUrl();

export class URLService {
  static getLink() {
    return API_URL;
  }
  
  static getBaseUrl() {
    if (typeof window !== 'undefined') {
      return process.env.NEXT_PUBLIC_API_URL || window.location.origin;
    }
    return process.env.NEXT_PUBLIC_API_URL || "http://localhost:5011";
  }
}