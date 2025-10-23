import { fileURLToPath } from 'url'
import { dirname } from 'path'

const __filename = fileURLToPath(import.meta.url)
const __dirname = dirname(__filename)

/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    domains: [],
  },
  // Servir la aplicación bajo /Gatekeep
  basePath: '/Gatekeep',
  assetPrefix: '/Gatekeep',
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

export default nextConfig
