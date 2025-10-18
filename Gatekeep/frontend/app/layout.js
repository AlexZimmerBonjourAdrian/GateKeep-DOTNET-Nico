import { PrimeReactProvider } from 'primereact/api'
import { primeReactConfig } from '../src/utils/primeReactConfig'
import Layout from './components/Layout'

// Importar estilos de PrimeReact
import 'primereact/resources/themes/lara-light-cyan/theme.css'
import 'primereact/resources/primereact.min.css'
import 'primeicons/primeicons.css'
import 'primeflex/primeflex.css'

// Importar estilos globales personalizados
import '../src/styles/global.css'

export const metadata = {
  title: 'React Template',
  description: 'Template limpio y reutilizable con Next.js y PrimeReact',
}

export default function RootLayout({ children }) {
  return (
    <html lang="es">
      <head>
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="true" />
        <link 
          href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" 
          rel="stylesheet" 
        />
      </head>
      <body>
        <PrimeReactProvider value={primeReactConfig}>
          <Layout>
            {children}
          </Layout>
        </PrimeReactProvider>
      </body>
    </html>
  )
}
