import React from 'react'
import { Card } from 'primereact/card'
import { Button } from 'primereact/button'

export default function Home() {
  return (
    <div style={{ minHeight: '100vh', backgroundColor: 'var(--surface-ground)', padding: '2rem 0' }}>
      <div className="container">
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <h1 style={{ fontSize: '2.5rem', fontWeight: 'bold', color: 'var(--text-color)', marginBottom: '1rem' }}>
            Bienvenido al Template React con Next.js
          </h1>
          <p style={{ fontSize: '1.125rem', color: 'var(--text-color-secondary)', maxWidth: '32rem', margin: '0 auto' }}>
            Este es un template limpio y reutilizable con Next.js y PrimeReact.
            Perfecto para comenzar nuevos proyectos.
          </p>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '1.5rem', maxWidth: '72rem', margin: '0 auto' }}>
          <Card>
            <div style={{ textAlign: 'center', padding: '1rem' }}>
              <i className="pi pi-cog" style={{ fontSize: '2.5rem', color: 'var(--primary-color)', marginBottom: '1rem' }}></i>
              <h3 style={{ fontSize: '1.25rem', fontWeight: '600', marginBottom: '0.5rem' }}>Configuración</h3>
              <p style={{ color: 'var(--text-color-secondary)', marginBottom: '1rem' }}>
                Configuración lista para usar con Next.js y PrimeReact.
              </p>
              <Button label="Configurar" className="p-button-outlined" />
            </div>
          </Card>

          <Card>
            <div style={{ textAlign: 'center', padding: '1rem' }}>
              <i className="pi pi-palette" style={{ fontSize: '2.5rem', color: '#10b981', marginBottom: '1rem' }}></i>
              <h3 style={{ fontSize: '1.25rem', fontWeight: '600', marginBottom: '0.5rem' }}>Estilos</h3>
              <p style={{ color: 'var(--text-color-secondary)', marginBottom: '1rem' }}>
                PrimeReact configurado con estilos personalizados para un diseño moderno.
              </p>
              <Button label="Personalizar" className="p-button-outlined" />
            </div>
          </Card>

          <Card>
            <div style={{ textAlign: 'center', padding: '1rem' }}>
              <i className="pi pi-code" style={{ fontSize: '2.5rem', color: '#8b5cf6', marginBottom: '1rem' }}></i>
              <h3 style={{ fontSize: '1.25rem', fontWeight: '600', marginBottom: '0.5rem' }}>Desarrollo</h3>
              <p style={{ color: 'var(--text-color-secondary)', marginBottom: '1rem' }}>
                Estructura organizada y lista para desarrollar tu aplicación.
              </p>
              <Button label="Comenzar" className="p-button-outlined" />
            </div>
          </Card>
        </div>

        <div style={{ textAlign: 'center', marginTop: '3rem' }}>
          <Card style={{ maxWidth: '32rem', margin: '0 auto' }}>
            <div style={{ padding: '1.5rem' }}>
              <h2 style={{ fontSize: '1.5rem', fontWeight: 'bold', marginBottom: '1rem' }}>Características del Template</h2>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem', textAlign: 'left' }}>
                <div>
                  <h4 style={{ fontWeight: '600', marginBottom: '0.5rem' }}>Frontend</h4>
                  <ul style={{ fontSize: '0.875rem', color: 'var(--text-color-secondary)', listStyle: 'none', padding: 0 }}>
                    <li>• Next.js 15</li>
                    <li>• React 18</li>
                    <li>• PrimeReact</li>
                    <li>• PrimeFlex</li>
                    <li>• CSS Personalizado</li>
                  </ul>
                </div>
                <div>
                  <h4 style={{ fontWeight: '600', marginBottom: '0.5rem' }}>Herramientas</h4>
                  <ul style={{ fontSize: '0.875rem', color: 'var(--text-color-secondary)', listStyle: 'none', padding: 0 }}>
                    <li>• ESLint</li>
                    <li>• TypeScript Support</li>
                    <li>• Hot Reload</li>
                    <li>• Source Maps</li>
                    <li>• PrimeIcons</li>
                  </ul>
                </div>
              </div>
            </div>
          </Card>
        </div>
      </div>
    </div>
  )
}
