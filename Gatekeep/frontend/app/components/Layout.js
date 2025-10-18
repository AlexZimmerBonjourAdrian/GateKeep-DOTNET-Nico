'use client'

import React from 'react'
import { Menubar } from 'primereact/menubar'
import Link from 'next/link'

const Layout = ({ children }) => {
  const items = [
    {
      label: 'Inicio',
      icon: 'pi pi-home',
      url: '/'
    },
    {
      label: 'Acerca de',
      icon: 'pi pi-info-circle',
      command: () => {
        console.log('Acerca de')
      }
    },
    {
      label: 'Contacto',
      icon: 'pi pi-envelope',
      command: () => {
        console.log('Contacto')
      }
    }
  ]

  const start = (
    <div style={{ display: 'flex', alignItems: 'center' }}>
      <i className="pi pi-code" style={{ fontSize: '1.5rem', color: 'var(--primary-color)', marginRight: '0.5rem' }}></i>
      <span style={{ fontSize: '1.25rem', fontWeight: 'bold', color: 'var(--primary-color)' }}>React Template</span>
    </div>
  )

  const end = (
    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
      <button className="p-button p-button-outlined p-button-sm">
        <i className="pi pi-user" style={{ marginRight: '0.5rem' }}></i>
        Login
      </button>
    </div>
  )

  return (
    <div style={{ minHeight: '100vh', backgroundColor: 'var(--surface-ground)' }}>
      <Menubar 
        model={items} 
        start={start} 
        end={end}
        style={{ boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)', border: 'none', backgroundColor: 'var(--surface-card)' }}
      />
      <main>
        {children}
      </main>
      <footer style={{ backgroundColor: '#1f2937', color: 'white', padding: '2rem 0', marginTop: '3rem' }}>
        <div className="container" style={{ textAlign: 'center' }}>
          <p>&copy; 2024 React Template. Todos los derechos reservados.</p>
          <p style={{ fontSize: '0.875rem', color: '#9ca3af', marginTop: '0.5rem' }}>
            Desarrollado con Next.js, React y PrimeReact
          </p>
        </div>
      </footer>
    </div>
  )
}

export default Layout
