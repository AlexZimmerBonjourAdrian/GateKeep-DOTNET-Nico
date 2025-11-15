import React, { createContext, useContext, useState, useEffect } from 'react';
import authService from '../services/authService.js';

// Crear el contexto
const AuthContext = createContext();

// Hook personalizado para usar el contexto
export const useAuth = () => {
  const context = useContext(AuthContext);
  // Durante el build/pre-renderizado, retornar valores por defecto en lugar de lanzar error
  if (!context) {
    if (typeof window === 'undefined') {
      // Server-side rendering o build time
      return {
        user: null,
        isAuthenticated: false,
        isLoading: false,
        error: null,
        login: async () => ({ success: false, error: 'Not available during build' }),
        register: async () => ({ success: false, error: 'Not available during build' }),
        logout: () => {},
        clearError: () => {},
        updateUser: () => {},
        refreshUser: async () => ({ success: false, error: 'Not available during build' }),
        createTestUsers: async () => ({ success: false, error: 'Not available during build' }),
        listUsers: async () => ({ success: false, error: 'Not available during build' }),
        checkAuthStatus: async () => {},
      };
    }
    throw new Error('useAuth debe ser usado dentro de un AuthProvider');
  }
  return context;
};

// Componente proveedor del contexto
export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  // Verificar autenticación al cargar la aplicación
  useEffect(() => {
    checkAuthStatus();
  }, []);

  // Verificar el estado de autenticación
  const checkAuthStatus = async () => {
    try {
      setIsLoading(true);
      setError(null);

      if (authService.isAuthenticated() && !authService.isTokenExpired()) {
        const currentUser = authService.getCurrentUser();
        setUser(currentUser);
        setIsAuthenticated(true);
      } else if (authService.isTokenExpired()) {
        // Intentar refrescar el token
        const refreshed = await authService.refreshToken();
        if (refreshed) {
          const currentUser = authService.getCurrentUser();
          setUser(currentUser);
          setIsAuthenticated(true);
        } else {
          // Token no se pudo refrescar, cerrar sesión
          logout();
        }
      } else {
        // No hay token o usuario
        logout();
      }
    } catch (error) {
      console.error('Error al verificar autenticación:', error);
      setError('Error al verificar el estado de autenticación');
      logout();
    } finally {
      setIsLoading(false);
    }
  };

  // Función de login
  const login = async (email, password) => {
    try {
      setIsLoading(true);
      setError(null);

      const result = await authService.login(email, password);
      
      if (result.success) {
        setUser(result.data.user);
        setIsAuthenticated(true);
        return { success: true, data: result.data };
      } else {
        setError(result.error);
        return { success: false, error: result.error };
      }
    } catch (error) {
      console.error('Error en login:', error);
      const errorMessage = 'Error inesperado durante el login';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    } finally {
      setIsLoading(false);
    }
  };

  // Función de registro
  const register = async ({ nombre, apellido, email, password, telefono }) => {
    try {
      setIsLoading(true);
      setError(null);

      const result = await authService.register({ nombre, apellido, email, password, telefono });

      if (result.success) {
        setUser(result.data.user);
        setIsAuthenticated(true);
        return { success: true, data: result.data };
      }

      setError(result.error);
      return { success: false, error: result.error };
    } catch (error) {
      const errorMessage = 'Error inesperado durante el registro';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    } finally {
      setIsLoading(false);
    }
  };

  // Función de logout
  const logout = () => {
    authService.logout();
    setUser(null);
    setIsAuthenticated(false);
    setError(null);
  };

  // Función para limpiar errores
  const clearError = () => {
    setError(null);
  };

  // Función para actualizar datos del usuario
  const updateUser = (userData) => {
    setUser(userData);
    localStorage.setItem('user', JSON.stringify(userData));
  };

  // Función para refrescar datos del usuario desde el servidor
  const refreshUser = async () => {
    try {
      const result = await authService.getCurrentUserFromServer();
      if (result.success) {
        updateUser(result.data);
        return { success: true, data: result.data };
      } else {
        setError(result.error);
        return { success: false, error: result.error };
      }
    } catch (error) {
      console.error('Error al refrescar usuario:', error);
      const errorMessage = 'Error al actualizar datos del usuario';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  // Función para crear usuarios de prueba (solo desarrollo)
  const createTestUsers = async () => {
    try {
      setIsLoading(true);
      setError(null);

      const result = await authService.createTestUsers();
      
      if (result.success) {
        return { success: true, data: result.data };
      } else {
        setError(result.error);
        return { success: false, error: result.error };
      }
    } catch (error) {
      console.error('Error al crear usuarios de prueba:', error);
      const errorMessage = 'Error al crear usuarios de prueba';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    } finally {
      setIsLoading(false);
    }
  };

  // Función para listar usuarios (solo desarrollo)
  const listUsers = async () => {
    try {
      setIsLoading(true);
      setError(null);

      const result = await authService.listUsers();
      
      if (result.success) {
        return { success: true, data: result.data };
      } else {
        setError(result.error);
        return { success: false, error: result.error };
      }
    } catch (error) {
      console.error('Error al listar usuarios:', error);
      const errorMessage = 'Error al listar usuarios';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    } finally {
      setIsLoading(false);
    }
  };

  // Valores del contexto
  const value = {
    // Estado
    user,
    isAuthenticated,
    isLoading,
    error,
    
    // Funciones
    login,
    register,
    logout,
    clearError,
    updateUser,
    refreshUser,
    createTestUsers,
    listUsers,
    checkAuthStatus,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export default AuthContext;
