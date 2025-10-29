/**
 * Tipos e interfaces para los contratos de la API de autenticación
 * Este archivo sirve como documentación y puede ser migrado a TypeScript en el futuro
 */

/**
 * @typedef {Object} LoginRequest
 * @property {string} email - Email del usuario
 * @property {string} password - Contraseña del usuario
 */

/**
 * @typedef {Object} UserInfoResponse
 * @property {number} id - ID del usuario
 * @property {string} email - Email del usuario
 * @property {string} nombre - Nombre del usuario
 * @property {string} apellido - Apellido del usuario
 * @property {string} tipoUsuario - Tipo de usuario (Admin, Estudiante, Funcionario)
 * @property {string} telefono - Teléfono del usuario
 * @property {string} fechaAlta - Fecha de alta del usuario
 */

/**
 * @typedef {Object} AuthResponse
 * @property {boolean} isSuccess - Indica si la operación fue exitosa
 * @property {string} [token] - Token JWT
 * @property {string} [refreshToken] - Token de refresco
 * @property {string} [expiresAt] - Fecha de expiración del token
 * @property {UserInfoResponse} [user] - Información del usuario
 * @property {string} [errorMessage] - Mensaje de error si la operación falló
 */

/**
 * @typedef {Object} UsuarioDto
 * @property {string} email - Email del usuario
 * @property {string} nombre - Nombre del usuario
 * @property {string} apellido - Apellido del usuario
 * @property {string} contrasenia - Contraseña hasheada
 * @property {string} telefono - Teléfono del usuario
 */

/**
 * @typedef {Object} TestUser
 * @property {string} email - Email del usuario de prueba
 * @property {string} nombre - Nombre del usuario
 * @property {string} apellido - Apellido del usuario
 * @property {string} telefono - Teléfono del usuario
 * @property {string} password - Contraseña en texto plano
 * @property {string} tipo - Tipo de usuario (Admin, Estudiante, Funcionario)
 */

/**
 * @typedef {Object} CreateTestUsersResponse
 * @property {boolean} isSuccess - Indica si la operación fue exitosa
 * @property {string} message - Mensaje descriptivo
 * @property {Array<TestUser>} usuariosCreados - Usuarios creados
 * @property {Array<TestUser>} usuariosExistentes - Usuarios que ya existían
 * @property {Object} resumen - Resumen de la operación
 * @property {number} resumen.totalCreados - Total de usuarios creados
 * @property {number} resumen.totalExistentes - Total de usuarios existentes
 * @property {number} resumen.totalProcesados - Total de usuarios procesados
 */

/**
 * @typedef {Object} ListUsersResponse
 * @property {boolean} isSuccess - Indica si la operación fue exitosa
 * @property {string} message - Mensaje descriptivo
 * @property {number} totalUsuarios - Total de usuarios encontrados
 * @property {Array<Object>} usuarios - Lista de usuarios con contraseñas en texto plano
 */

/**
 * @typedef {Object} ApiResponse
 * @property {boolean} success - Indica si la operación fue exitosa
 * @property {*} [data] - Datos de respuesta
 * @property {string} [error] - Mensaje de error
 */

/**
 * @typedef {Object} AuthContextType
 * @property {UserInfoResponse|null} user - Usuario actual
 * @property {boolean} isAuthenticated - Indica si el usuario está autenticado
 * @property {boolean} isLoading - Indica si hay una operación en curso
 * @property {string|null} error - Mensaje de error actual
 * @property {function(string, string): Promise<ApiResponse>} login - Función de login
 * @property {function(): void} logout - Función de logout
 * @property {function(): void} clearError - Función para limpiar errores
 * @property {function(Object): void} updateUser - Función para actualizar datos del usuario
 * @property {function(): Promise<ApiResponse>} refreshUser - Función para refrescar datos del usuario
 * @property {function(): Promise<ApiResponse>} createTestUsers - Función para crear usuarios de prueba
 * @property {function(): Promise<ApiResponse>} listUsers - Función para listar usuarios
 * @property {function(): Promise<void>} checkAuthStatus - Función para verificar estado de autenticación
 */

/**
 * @typedef {Object} TokenInfo
 * @property {Object} payload - Payload del token JWT
 * @property {Date|null} issuedAt - Fecha de emisión del token
 * @property {Date|null} expiresAt - Fecha de expiración del token
 * @property {boolean} isExpired - Indica si el token está expirado
 * @property {string} userId - ID del usuario
 * @property {string} email - Email del usuario
 * @property {string[]} roles - Roles del usuario
 */

/**
 * @typedef {Object} AxiosError
 * @property {Object} response - Respuesta de error
 * @property {number} response.status - Código de estado HTTP
 * @property {Object} response.data - Datos de la respuesta de error
 * @property {string} response.data.message - Mensaje de error
 */

// Constantes para tipos de usuario
export const USER_TYPES = {
    ADMIN: 'Admin',
    ESTUDIANTE: 'Estudiante',
    FUNCIONARIO: 'Funcionario',
};

// Constantes para códigos de estado HTTP
export const HTTP_STATUS = {
    OK: 200,
    BAD_REQUEST: 400,
    UNAUTHORIZED: 401,
    FORBIDDEN: 403,
    NOT_FOUND: 404,
    INTERNAL_SERVER_ERROR: 500,
};

// Constantes para mensajes de error
export const ERROR_MESSAGES = {
    INVALID_CREDENTIALS: 'Credenciales inválidas',
    NETWORK_ERROR: 'Error de conexión',
    TOKEN_EXPIRED: 'Token expirado',
    UNAUTHORIZED: 'No autorizado',
    FORBIDDEN: 'Acceso denegado',
    NOT_FOUND: 'Recurso no encontrado',
    SERVER_ERROR: 'Error del servidor',
    VALIDATION_ERROR: 'Error de validación',
};

// Constantes para claves de localStorage
export const STORAGE_KEYS = {
    TOKEN: 'token',
    REFRESH_TOKEN: 'refreshToken',
    USER: 'user',
    TOKEN_EXPIRY: 'tokenExpiry',
};

// Constantes para configuración de tokens
export const TOKEN_CONFIG = {
    REFRESH_THRESHOLD: 5 * 60 * 1000, // 5 minutos en milisegundos
    MAX_RETRY_ATTEMPTS: 3,
};

export default {
    USER_TYPES,
    HTTP_STATUS,
    ERROR_MESSAGES,
    STORAGE_KEYS,
    TOKEN_CONFIG,
};