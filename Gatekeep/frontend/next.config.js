import { fileURLToPath } from 'url'
import { dirname } from 'path'
import withPWAInit from 'next-pwa'

const __filename = fileURLToPath(
    import.meta.url)
const __dirname = dirname(__filename)

const isDev = process.env.NODE_ENV !== 'production'

const withPWA = withPWAInit({
    dest: 'public',
    disable: isDev,
    register: false, // Registro manual en providers.jsx
    skipWaiting: true,
    sw: 'sw.js', // Usar nuestro Service Worker personalizado
    buildExcludes: [/sw\.js$/, /workbox-.*\.js$/], // Excluir generación automática
})

/** @type {import('next').NextConfig} */
const nextConfig = {
    images: {
        domains: [],
    },
    // Deshabilitar static export para evitar problemas con pre-renderizado
    // output: 'standalone', // Usar standalone para mejor compatibilidad con Docker
    // Eliminar basePath/assetPrefix - el frontend está en la raíz del dominio
    // No usar basePath ya que el ALB enruta directamente a /
    // Configuración para PrimeReact
    transpilePackages: ['primereact', 'primeicons', 'primeflex'],
    // Configuración para evitar conflictos con múltiples lockfiles
    outputFileTracingRoot: __dirname,
    webpack(config, { dir, isServer }) {
        // Configurar alias @/ para apuntar a src/
        config.resolve.alias = {
            ...config.resolve.alias,
            '@': dir + '/src',
        };
        
        // Configurar sql.js para el cliente
        if (!isServer) {
            config.resolve.fallback = {
                ...config.resolve.fallback,
                fs: false,
                path: false,
                crypto: false,
            };
        }
        
        config.module.rules.push({
            test: /\.svg$/,
            use: ['@svgr/webpack'],
        });
        return config;
    },
}

export default withPWA(nextConfig)