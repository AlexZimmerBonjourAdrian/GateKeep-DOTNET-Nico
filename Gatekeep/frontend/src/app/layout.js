import React from 'react'
import './globals.css'
import Providers from './providers'
// PrimeReact and PrimeIcons global styles
import 'primeflex/primeflex.css'
import 'primereact/resources/themes/lara-light-cyan/theme.css'
import 'primereact/resources/primereact.min.css'
import 'primeicons/primeicons.css'

export const metadata = {
  title: 'GateKeep',
  description: 'Sistema de gestión de acceso y control para espacios universitarios',
  manifest: '/manifest.json',
  themeColor: '#0066cc',
  appleWebApp: {
    capable: true,
    statusBarStyle: 'default',
    title: 'GateKeep',
  },
  viewport: {
    width: 'device-width',
    initialScale: 1,
    maximumScale: 1,
    userScalable: false,
  },
}

export default function RootLayout({ children }) {
  return (
    <html lang="es">
      <head>
        <link rel="manifest" href="/manifest.json" />
        <link rel="apple-touch-icon" href="/assets/LogoGateKeep.webp" />
        <meta name="theme-color" content="#0066cc" />
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="true" />
        <link 
          href="https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&display=swap" 
          rel="stylesheet" 
        />
      </head>
      <body>
        <Providers>{children}</Providers>
      </body>
    </html>
  )
}
