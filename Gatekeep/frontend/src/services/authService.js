import apiClient from '@/lib/axios-offline-interceptor';

// Nota: apiClient ya maneja el token automáticamente y los errores 401
// No es necesario agregar interceptores adicionales aquí

class AuthService {
    /**
     * Realiza el login del usuario
     * @param {string} email - Email del usuario
     * @param {string} password - Contraseña del usuario
     * @returns {Promise<Object>} - Respuesta con token y datos del usuario
     */
    async login(email, password) {
        try {
            const response = await apiClient.post('auth/login', {
                email,
                password,
            });

            if (response.data.isSuccess) {
                // Guardar tokens y datos del usuario en localStorage
                localStorage.setItem('token', response.data.token);
                localStorage.setItem('refreshToken', response.data.refreshToken);
                localStorage.setItem('user', JSON.stringify(response.data.user));
                localStorage.setItem('tokenExpiry', response.data.expiresAt);

                return {
                    success: true,
                    data: response.data,
                };
            } else {
                return {
                    success: false,
                    error: response.data.errorMessage || 'Error en el login',
                };
            }
        } catch (error) {
            console.error('Error en login:', error);
            return {
                success: false,
                error: error.response?.data?.message || 'Error de conexión',
            };
        }
    }

  /**
   * Registra un nuevo usuario
   * @param {Object} data - Datos del usuario
   * @param {string} data.nombre
   * @param {string} data.apellido
   * @param {string} data.email
   * @param {string} data.password
   * @param {string} [data.telefono]
   * @returns {Promise<Object>} - Respuesta con token y datos del usuario
   */
  async register({ nombre, apellido, email, password, telefono }) {
    try {
      const response = await apiClient.post('auth/register', {
        nombre,
        apellido,
        email,
        password,
        telefono,
      });

      if (response.data.isSuccess) {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('refreshToken', response.data.refreshToken);
        localStorage.setItem('user', JSON.stringify(response.data.user));
        localStorage.setItem('tokenExpiry', response.data.expiresAt);

        return {
          success: true,
          data: response.data,
        };
      }

      return {
        success: false,
        error: response.data.errorMessage || 'Error en el registro',
      };
    } catch (error) {
      return {
        success: false,
        error: error.response?.data?.message || 'Error de conexión',
      };
    }
  }

    /**
     * Cierra la sesión del usuario
     */
    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        localStorage.removeItem('tokenExpiry');
        window.location.href = '/auth/login';
    }

    /**
     * Obtiene el token actual
     * @returns {string|null} - Token actual o null si no existe
     */
    getToken() {
        return localStorage.getItem('token');
    }

    /**
     * Obtiene el usuario actual
     * @returns {Object|null} - Datos del usuario actual o null si no está autenticado
     */
    getCurrentUser() {
        const user = localStorage.getItem('user');
        return user ? JSON.parse(user) : null;
    }

    /**
     * Verifica si el usuario está autenticado
     * @returns {boolean} - true si está autenticado, false en caso contrario
     */
    isAuthenticated() {
        const token = this.getToken();
        const user = this.getCurrentUser();
        return !!(token && user);
    }

    /**
     * Verifica si el token está expirado
     * @returns {boolean} - true si está expirado, false en caso contrario
     */
    isTokenExpired() {
        const expiry = localStorage.getItem('tokenExpiry');
        if (!expiry) return true;

        const expiryDate = new Date(expiry);
        const now = new Date();
        return now >= expiryDate;
    }

    /**
     * Refresca el token usando el refresh token
     * @returns {Promise<boolean>} - true si se refrescó exitosamente, false en caso contrario
     */
    async refreshToken() {
        try {
            const refreshToken = localStorage.getItem('refreshToken');
            if (!refreshToken) return false;

            const response = await apiClient.post('auth/refresh', {
                refreshToken,
            });

            if (response.data.isSuccess) {
                localStorage.setItem('token', response.data.token);
                localStorage.setItem('refreshToken', response.data.refreshToken);
                localStorage.setItem('tokenExpiry', response.data.expiresAt);
                return true;
            }
            return false;
        } catch (error) {
            console.error('Error al refrescar token:', error);
            this.logout();
            return false;
        }
    }

    /**
     * Crea usuarios de prueba (solo para desarrollo)
     * @returns {Promise<Object>} - Respuesta con los usuarios creados
     */
    async createTestUsers() {
        try {
            const response = await apiClient.post('auth/create-test-users');
            return {
                success: true,
                data: response.data,
            };
        } catch (error) {
            console.error('Error al crear usuarios de prueba:', error);
            return {
                success: false,
                error: error.response?.data?.message || 'Error al crear usuarios de prueba',
            };
        }
    }

    /**
     * Lista todos los usuarios (solo para desarrollo)
     * @returns {Promise<Object>} - Respuesta con la lista de usuarios
     */
    async listUsers() {
        try {
            const response = await apiClient.get('auth/list-users');
            return {
                success: true,
                data: response.data,
            };
        } catch (error) {
            console.error('Error al listar usuarios:', error);
            return {
                success: false,
                error: error.response?.data?.message || 'Error al listar usuarios',
            };
        }
    }

    /**
     * Obtiene información del usuario actual desde el servidor
     * @returns {Promise<Object>} - Respuesta con los datos del usuario
     */
    async getCurrentUserFromServer() {
        try {
            const response = await apiClient.get('usuarios/me');
            return {
                success: true,
                data: response.data,
            };
        } catch (error) {
            console.error('Error al obtener usuario actual:', error);
            return {
                success: false,
                error: error.response?.data?.message || 'Error al obtener usuario actual',
            };
        }
    }
}

// Crear una instancia única del servicio
const authService = new AuthService();

export default authService;