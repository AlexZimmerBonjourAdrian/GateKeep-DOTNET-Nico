import axios from "axios";

/**
 * Obtiene la URL base del backend API
 * En AWS: https://api.zimmzimmgames.com
 * En local: http://localhost:5011 (o http://localhost si usa nginx)
 */
const getApiBaseUrl = () => {
  // Prioridad 1: Variable de entorno (configurada en build/deployment)
  if (process.env.NEXT_PUBLIC_API_URL) {
    return process.env.NEXT_PUBLIC_API_URL.replace(/\/$/, '');
  }
  
  // Prioridad 2: En cliente, detectar si estamos en producción AWS
  if (typeof window !== 'undefined') {
    const origin = window.location.origin;
    // Si estamos en HTTPS y no es localhost, construir api.*
    if (origin.startsWith('https://') && !origin.includes('localhost')) {
      // Extraer dominio base (sin www)
      const domain = origin.replace(/^https?:\/\/(www\.)?/, '');
      return `https://api.${domain}`;
    }
    // En desarrollo local, usar el mismo origin (nginx enruta /api/)
    return origin;
  }
  
  // Prioridad 3: Fallback según NODE_ENV (solo en servidor)
  return process.env.NODE_ENV === 'production' 
    ? "https://api.zimmzimmgames.com"
    : "http://localhost:5011";
};

/**
 * Obtiene la URL completa de la API con el prefijo /api/
 */
const getApiUrl = () => {
  const baseUrl = getApiBaseUrl();
  // Si la URL base ya incluye /api, no agregar otro
  if (baseUrl.endsWith('/api') || baseUrl.endsWith('/api/')) {
    return baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`;
  }
  return `${baseUrl}/api/`;
};

const API_URL = getApiUrl();

export class URLService {
  /**
   * Obtiene la URL completa de la API (con /api/)
   * Ejemplo: https://api.zimmzimmgames.com/api/
   */
  static getLink() {
    return API_URL;
  }
  
  /**
   * Obtiene la URL base del backend (sin /api/)
   * Ejemplo: https://api.zimmzimmgames.com
   */
  static getBaseUrl() {
    return getApiBaseUrl();
  }
  
  /**
   * Alias para getLink() - mantiene compatibilidad
   */
  static getApiUrl() {
    return API_URL;
  }
}