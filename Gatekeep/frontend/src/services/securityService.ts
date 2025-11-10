
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
    
    // Eliminar cookie del token
    this.deleteCookie('token');
    
    window.location.href = '/login';
  }

  /**
   * Guarda los datos de autenticación en localStorage y cookies
   * @param userId - ID del usuario
   * @param tipoUsuario - Tipo de usuario
   * @param token - Token de autenticación
   * @param refreshToken - Token de refresco (opcional)
   * @param user - Objeto completo del usuario (opcional)
   */
  static saveAuthData(userId: number, tipoUsuario: string, token: string, refreshToken?: string, user?: any): void {
    if (typeof window === 'undefined') return;
    
    console.log('💾 SecurityService - Saving auth data');
    console.log('💾 UserId:', userId, 'TipoUsuario:', tipoUsuario);
    console.log('💾 Token exists:', !!token);
    
    localStorage.setItem('userId', userId.toString());
    localStorage.setItem('tipoUsuario', tipoUsuario);
    localStorage.setItem('token', token);
    
    if (refreshToken) {
      localStorage.setItem('refreshToken', refreshToken);
    }

    if (user) {
      localStorage.setItem('user', JSON.stringify(user));
    }

    // Guardar token en cookie para el middleware (7 días de expiración)
    console.log('💾 Setting cookie for middleware...');
    this.setCookie('token', token, 7);
    console.log('✅ Auth data saved successfully');
  }

  /**
   * Establece una cookie
   * @param name - Nombre de la cookie
   * @param value - Valor de la cookie
   * @param days - Días hasta la expiración
   */
  static setCookie(name: string, value: string, days: number = 7): void {
    if (typeof window === 'undefined') return;
    
    let expires = '';
    if (days) {
      const date = new Date();
      date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
      expires = '; expires=' + date.toUTCString();
    }
    
    // Establecer cookie con configuración más explícita
    const cookieString = `${name}=${value || ''}${expires}; path=/; SameSite=Lax`;
    document.cookie = cookieString;
    
    // Debug: Verificar que la cookie se estableció
    console.log('🍪 Cookie set:', name, '- Value exists:', !!value);
    console.log('🍪 Cookie string:', cookieString);
  }

  /**
   * Elimina una cookie
   * @param name - Nombre de la cookie
   */
  private static deleteCookie(name: string): void {
    if (typeof window === 'undefined') return;
    document.cookie = name + '=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
  }

}

