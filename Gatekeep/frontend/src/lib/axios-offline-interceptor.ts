import axios, { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios';
import { recordEvent } from './sync';
import { isOnline } from './sync';
import { URLService } from '@/services/urlService';

// Crear una instancia base de axios con configuración global
const apiClient: AxiosInstance = axios.create({
  baseURL: URLService.getLink(), // Incluye /api/
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000, // 30 segundos
});

// INTERCEPTOR DE REQUEST: Agregar token y detectar offline
apiClient.interceptors.request.use(
  async (config: InternalAxiosRequestConfig) => {
    // Agregar token de autenticación
    const token = localStorage.getItem('token');
    if (token) {
      const h: any = config.headers ?? {};
      if (typeof h.set === 'function') {
        h.set('Authorization', `Bearer ${token}`);
      } else {
        h['Authorization'] = `Bearer ${token}`;
      }
      (config as any).headers = h;
    }

    // Si no hay conexión y es una petición que modifica datos (POST, PUT, DELETE, PATCH)
    if (!isOnline() && config.method && ['post', 'put', 'delete', 'patch'].includes(config.method.toLowerCase())) {
      const offlineData = {
        url: config.url,
        method: config.method.toUpperCase(),
        data: config.data,
        headers: config.headers,
        baseURL: config.baseURL,
      };

      // Guardar en SQLite para sincronizar después
      try {
        await recordEvent('api_request', offlineData);
        console.log('📝 Petición guardada offline:', config.method?.toUpperCase(), config.url);
      } catch (error) {
        console.error('❌ Error guardando petición offline:', error);
      }

      // Rechazar la petición pero devolver un error especial
      return Promise.reject({
        isOffline: true,
        message: 'Sin conexión. La petición se guardó para sincronizar después.',
        offlineData,
        config,
      } as AxiosError);
    }

    // Si hay conexión, continuar normalmente
    return config;
  },
  (error: AxiosError) => Promise.reject(error)
);

// INTERCEPTOR DE RESPONSE: Capturar errores de red y guardar offline, manejar 401
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    // Manejar error 401 (token expirado o inválido)
    if (error.response?.status === 401) {
      // Limpiar datos de autenticación
      if (typeof window !== 'undefined') {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        localStorage.removeItem('tokenExpiry');
        // Redirigir a login solo si no es una petición de login/register
        const url = error.config?.url || '';
        if (!url.includes('auth/login') && !url.includes('auth/register')) {
          window.location.href = '/login';
        }
      }
      return Promise.reject(error);
    }

    // Si es un error de red (sin conexión) y no es GET
    if (
      !error.response &&
      error.config &&
      error.config.method &&
      !['get', 'head', 'options'].includes(error.config.method.toLowerCase()) &&
      !(error as any).isOffline // Evitar duplicados
    ) {
      const offlineData = {
        url: error.config.url,
        method: error.config.method?.toUpperCase(),
        data: error.config.data,
        headers: error.config.headers,
        baseURL: error.config.baseURL,
      };

      try {
        await recordEvent('api_request', offlineData);
        console.log('📝 Petición guardada offline (error de red):', error.config.method?.toUpperCase(), error.config.url);
      } catch (saveError) {
        console.error('❌ Error guardando petición offline:', saveError);
      }

      return Promise.reject({
        ...error,
        isOffline: true,
        message: 'Error de red. La petición se guardó para sincronizar después.',
        offlineData,
      } as AxiosError);
    }

    return Promise.reject(error);
  }
);

export default apiClient;

