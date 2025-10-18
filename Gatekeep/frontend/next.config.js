import { fileURLToPath } from 'url'
import { dirname } from 'path'

const __filename = fileURLToPath(import.meta.url)
const __dirname = dirname(__filename)

/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    domains: [],
  },
  // Configuración para PrimeReact
  transpilePackages: ['primereact', 'primeicons', 'primeflex'],
  // Configuración para evitar conflictos con múltiples lockfiles
  outputFileTracingRoot: __dirname,
}

export default nextConfig
