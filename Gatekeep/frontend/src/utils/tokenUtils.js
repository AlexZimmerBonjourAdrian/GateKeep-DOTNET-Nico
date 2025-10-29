/**
 * Utilidades para el manejo de tokens JWT y localStorage
 */

// Claves para localStorage
const STORAGE_KEYS = {
    TOKEN: 'token',
    REFRESH_TOKEN: 'refreshToken',
    USER: 'user',
    TOKEN_EXPIRY: 'tokenExpiry',
};

/**
 * Clase para manejar tokens JWT y localStorage
 */
class TokenUtils {
    /**
     * Guarda un token en localStorage
     * @param {string} token - Token JWT
     */
    static setToken(token) {
        if (token) {
            localStorage.setItem(STORAGE_KEYS.TOKEN, token);
        }
    }

    /**
     * Obtiene el token del localStorage
     * @returns {string|null} - Token o null si no existe
     */
    static getToken() {
        return localStorage.getItem(STORAGE_KEYS.TOKEN);
    }

    /**
     * Elimina el token del localStorage
     */
    static removeToken() {
        localStorage.removeItem(STORAGE_KEYS.TOKEN);
    }

    /**
     * Guarda un refresh token en localStorage
     * @param {string} refreshToken - Refresh token
     */
    static setRefreshToken(refreshToken) {
        if (refreshToken) {
            localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, refreshToken);
        }
    }

    /**
     * Obtiene el refresh token del localStorage
     * @returns {string|null} - Refresh token o null si no existe
     */
    static getRefreshToken() {
        return localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
    }

    /**
     * Elimina el refresh token del localStorage
     */
    static removeRefreshToken() {
        localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
    }

    /**
     * Guarda datos del usuario en localStorage
     * @param {Object} user - Datos del usuario
     */
    static setUser(user) {
        if (user) {
            localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user));
        }
    }

    /**
     * Obtiene los datos del usuario del localStorage
     * @returns {Object|null} - Datos del usuario o null si no existe
     */
    static getUser() {
        const user = localStorage.getItem(STORAGE_KEYS.USER);
        return user ? JSON.parse(user) : null;
    }

    /**
     * Elimina los datos del usuario del localStorage
     */
    static removeUser() {
        localStorage.removeItem(STORAGE_KEYS.USER);
    }

    /**
     * Guarda la fecha de expiración del token
     * @param {string|Date} expiry - Fecha de expiración
     */
    static setTokenExpiry(expiry) {
        if (expiry) {
            const expiryDate = expiry instanceof Date ? expiry.toISOString() : expiry;
            localStorage.setItem(STORAGE_KEYS.TOKEN_EXPIRY, expiryDate);
        }
    }

    /**
     * Obtiene la fecha de expiración del token
     * @returns {Date|null} - Fecha de expiración o null si no existe
     */
    static getTokenExpiry() {
        const expiry = localStorage.getItem(STORAGE_KEYS.TOKEN_EXPIRY);
        return expiry ? new Date(expiry) : null;
    }

    /**
     * Elimina la fecha de expiración del token
     */
    static removeTokenExpiry() {
        localStorage.removeItem(STORAGE_KEYS.TOKEN_EXPIRY);
    }

    /**
     * Verifica si el token está expirado
     * @returns {boolean} - true si está expirado, false en caso contrario
     */
    static isTokenExpired() {
        const expiry = this.getTokenExpiry();
        if (!expiry) return true;

        const now = new Date();
        return now >= expiry;
    }

    /**
     * Verifica si hay un token válido (existe y no está expirado)
     * @returns {boolean} - true si el token es válido, false en caso contrario
     */
    static hasValidToken() {
        const token = this.getToken();
        return !!(token && !this.isTokenExpired());
    }

    /**
     * Limpia todos los datos de autenticación del localStorage
     */
    static clearAuthData() {
        this.removeToken();
        this.removeRefreshToken();
        this.removeUser();
        this.removeTokenExpiry();
    }

    /**
     * Decodifica un token JWT (sin verificar la firma)
     * @param {string} token - Token JWT
     * @returns {Object|null} - Payload decodificado o null si hay error
     */
    static decodeToken(token) {
        try {
            if (!token) return null;

            const parts = token.split('.');
            if (parts.length !== 3) return null;

            const payload = parts[1];
            const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
            return JSON.parse(decoded);
        } catch (error) {
            console.error('Error al decodificar token:', error);
            return null;
        }
    }

    /**
     * Obtiene información del token decodificado
     * @param {string} token - Token JWT (opcional, usa el token actual si no se proporciona)
     * @returns {Object|null} - Información del token o null si hay error
     */
    static getTokenInfo(token = null) {
        const tokenToUse = token || this.getToken();
        if (!tokenToUse) return null;

        const decoded = this.decodeToken(tokenToUse);
        if (!decoded) return null;

        return {
            payload: decoded,
            issuedAt: decoded.iat ? new Date(decoded.iat * 1000) : null,
            expiresAt: decoded.exp ? new Date(decoded.exp * 1000) : null,
            isExpired: decoded.exp ? new Date() >= new Date(decoded.exp * 1000) : true,
            userId: decoded.sub || decoded.userId,
            email: decoded.email,
            roles: decoded.roles || [],
        };
    }

    /**
     * Verifica si el usuario tiene un rol específico
     * @param {string} role - Rol a verificar
     * @returns {boolean} - true si tiene el rol, false en caso contrario
     */
    static hasRole(role) {
        const tokenInfo = this.getTokenInfo();
        if (!tokenInfo) return false;

        return tokenInfo.roles.includes(role);
    }

    /**
     * Verifica si el usuario tiene alguno de los roles especificados
     * @param {string[]} roles - Array de roles a verificar
     * @returns {boolean} - true si tiene alguno de los roles, false en caso contrario
     */
    static hasAnyRole(roles) {
        const tokenInfo = this.getTokenInfo();
        if (!tokenInfo) return false;

        return roles.some(role => tokenInfo.roles.includes(role));
    }

    /**
     * Obtiene el tiempo restante hasta la expiración del token
     * @returns {number|null} - Tiempo en milisegundos o null si no hay token
     */
    static getTimeUntilExpiry() {
        const tokenInfo = this.getTokenInfo();
        if (!tokenInfo || !tokenInfo.expiresAt) return null;

        const now = new Date();
        const expiry = tokenInfo.expiresAt;
        const timeLeft = expiry.getTime() - now.getTime();

        return timeLeft > 0 ? timeLeft : 0;
    }

    /**
     * Verifica si el token expirará pronto (en los próximos 5 minutos)
     * @returns {boolean} - true si expirará pronto, false en caso contrario
     */
    static isTokenExpiringSoon() {
        const timeLeft = this.getTimeUntilExpiry();
        if (timeLeft === null) return true;

        const fiveMinutes = 5 * 60 * 1000; // 5 minutos en milisegundos
        return timeLeft <= fiveMinutes;
    }
}

export default TokenUtils;