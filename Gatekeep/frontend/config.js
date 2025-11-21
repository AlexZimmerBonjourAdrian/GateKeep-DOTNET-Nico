// Archivo de configuración
// Copiado desde config.example.js y personalizado

// Función para obtener la URL del backend
const getApiUrl = () => {
  // Prioridad 1: Variable de entorno
  if (process.env.NEXT_PUBLIC_API_URL) {
    return process.env.NEXT_PUBLIC_API_URL;
  }
  
  // Prioridad 2: En cliente, detectar producción AWS
  if (typeof window !== 'undefined') {
    const origin = window.location.origin;
    if (origin.startsWith('https://') && !origin.includes('localhost')) {
      const domain = origin.replace(/^https?:\/\/(www\.)?/, '');
      return `https://api.${domain}`;
    }
    return origin;
  }
  
  // Prioridad 3: Fallback según NODE_ENV
  return typeof process !== 'undefined' && process.env.NODE_ENV === 'production' 
    ? 'https://api.zimmzimmgames.com'
    : 'http://localhost:5011';
};

export const config = {
  // URL del backend API
  apiUrl: getApiUrl(),

  // Configuración de la aplicación
  appName: 'React Template',
  appVersion: '1.0.0',

  // Configuración de autenticación (si se implementa)
  jwtSecret: 'your-secret-key-here',

  // Configuración de servicios externos (ejemplos)
  googleAnalyticsId: 'GA_MEASUREMENT_ID',
  stripePublicKey: 'pk_test_your_stripe_key_here',

  // Configuración de PrimeReact
  primeReact: {
    theme: 'lara-light-cyan',
    inputStyle: 'outlined',
    ripple: true
  }
};