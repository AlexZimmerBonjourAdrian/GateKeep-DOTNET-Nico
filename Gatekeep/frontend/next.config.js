import { fileURLToPath } from 'url'
import { dirname } from 'path'
import withPWA from 'next-pwa'

const __filename = fileURLToPath(
    import.meta.url)
const __dirname = dirname(__filename)

const isDev = process.env.NODE_ENV !== 'production'

/** @type {import('next').NextConfig} */
const nextConfig = {
    images: {
        domains: [],
    },
    // Aplicar basePath/assetPrefix solo en producción
    ...(isDev ? {} : { basePath: '/Gatekeep', assetPrefix: '/Gatekeep' }),
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

// Configuración PWA
const pwaConfig = withPWA({
    dest: 'public',
    register: true,
    skipWaiting: true,
    disable: isDev, // Deshabilitar en desarrollo para evitar problemas
    runtimeCaching: [
        {
            urlPattern: /^https?.*/,
            handler: 'NetworkFirst',
            options: {
                cacheName: 'offlineCache',
                expiration: {
                    maxEntries: 200,
                },
            },
        },
    ],
})

export default pwaConfig(nextConfig)