import { fileURLToPath } from 'url'
import { dirname } from 'path'

const __filename = fileURLToPath(
    import.meta.url)
const __dirname = dirname(__filename)

const isDev = process.env.NODE_ENV !== 'production'

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
    webpack(config) {
        config.module.rules.push({
            test: /\.svg$/,
            use: ['@svgr/webpack'],
        });
        return config;
    },
}

// Exportar configuración sin PWA para evitar dependencia faltante
export default nextConfig