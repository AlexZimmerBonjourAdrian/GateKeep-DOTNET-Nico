/**
 * Servicio para detectar el estado de la conexión a internet
 */
class NetworkService {
    constructor() {
        this.isOnlineValue = typeof navigator !== 'undefined' ? navigator.onLine : true;
        this.listeners = [];

        if (typeof window !== 'undefined') {
            // Escuchar cambios de conectividad
            window.addEventListener('online', () => {
                this.isOnlineValue = true;
                this.notifyListeners(true);
            });

            window.addEventListener('offline', () => {
                this.isOnlineValue = false;
                this.notifyListeners(false);
            });
        }
    }

    /**
     * Verifica si hay conexión a internet
     * @returns {boolean}
     */
    isOnline() {
        return this.isOnlineValue;
    }

    /**
     * Agrega un listener para cambios de conectividad
     * @param {Function} callback - Función que se ejecuta cuando cambia la conectividad
     */
    onConnectivityChange(callback) {
        this.listeners.push(callback);
    }

    /**
     * Notifica a los listeners sobre cambios de conectividad
     * @param {boolean} isOnline - Estado de la conexión
     */
    notifyListeners(isOnline) {
        this.listeners.forEach(callback => {
            try {
                callback(isOnline);
            } catch (error) {
                console.error('Error en listener de conectividad:', error);
            }
        });
    }
}

// Crear instancia única del servicio
export const networkService = new NetworkService();

