
import axios from "axios";
import { URLService } from "./urlService";


export class SecurityService {

  /**
   * Verifica si el usuario está autenticado
   * @returns true si está logueado, false si no
   */
  static isLogged(): boolean {
    if (typeof window === 'undefined') return false; // SSR check
    
    const storedUserId = localStorage.getItem('userId');
    const token = localStorage.getItem('token');
    
    return !!(storedUserId && token);
  }

  /**
   * Verifica la autenticación y redirige al login si no está autenticado
   * @param currentPath - La ruta actual
   * @returns true si está autenticado, false si debe redirigir
   */
  static checkAuthAndRedirect(currentPath: string): boolean {
    if (typeof window === 'undefined') return true; // SSR check
    
    // Rutas públicas que no requieren autenticación
    const publicPaths = ['/login', '/register'];
    
    // Si está en una ruta pública, no verificar autenticación
    if (publicPaths.includes(currentPath)) {
      return true;
    }
    
    // Verificar si está logueado
    if (!this.isLogged()) {
      window.location.href = '/login';
      return false;
    }
    
    return true;
  }

  /**
   * Obtiene el userId del localStorage
   * @returns El userId o null si no existe
   */
  static getUserId(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('userId');
  }

  /**
   * Obtiene el tipo de usuario del localStorage
   * @returns El tipo de usuario o null si no existe
   */
  static getTipoUsuario(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('tipoUsuario');
  }

  /**
   * Obtiene el token del localStorage
   * @returns El token o null si no existe
   */
  static getToken(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('token');
  }

  /**
   * Cierra la sesión del usuario
   */
  static logout(): void {
    if (typeof window === 'undefined') return;
    
    localStorage.removeItem('userId');
    localStorage.removeItem('tipoUsuario');
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    
    window.location.href = '/login';
  }

  /**
   * Guarda los datos de autenticación en localStorage
   * @param userId - ID del usuario
   * @param tipoUsuario - Tipo de usuario
   * @param token - Token de autenticación
   * @param refreshToken - Token de refresco (opcional)
   */
  static saveAuthData(userId: number, tipoUsuario: string, token: string, refreshToken?: string): void {
    if (typeof window === 'undefined') return;
    
    localStorage.setItem('userId', userId.toString());
    localStorage.setItem('tipoUsuario', tipoUsuario);
    localStorage.setItem('token', token);
    
    if (refreshToken) {
      localStorage.setItem('refreshToken', refreshToken);
    }
  }

}

