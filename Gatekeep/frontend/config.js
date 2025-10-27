// Archivo de configuración
// Copiado desde config.example.js y personalizado

export const config = {
  // URL del backend API
  apiUrl: 'http://localhost:5000',

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