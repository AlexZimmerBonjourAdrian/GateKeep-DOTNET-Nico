"use client"

import { useEffect, useState } from 'react'
import { SecurityService } from '../../services/securityService.ts'

export default function DebugAuth() {
  const [info, setInfo] = useState({})

  useEffect(() => {
    // Obtener información de localStorage
    const localStorage = {
      token: SecurityService.getToken(),
      userId: SecurityService.getUserId(),
      tipoUsuario: SecurityService.getTipoUsuario(),
      isLogged: SecurityService.isLogged()
    }

    // Obtener cookies del navegador
    const cookies = document.cookie
      .split(';')
      .reduce((acc, cookie) => {
        const [key, value] = cookie.trim().split('=')
        acc[key] = value
        return acc
      }, {})

    setInfo({
      localStorage,
      cookies,
      allCookies: document.cookie
    })
  }, [])

  return (
    <div style={{ padding: '20px', fontFamily: 'monospace' }}>
      <h1>🔍 Debug de Autenticación</h1>
      
      <h2>📦 LocalStorage:</h2>
      <pre style={{ background: '#f5f5f5', padding: '10px', borderRadius: '5px' }}>
        {JSON.stringify(info.localStorage, null, 2)}
      </pre>

      <h2>🍪 Cookies:</h2>
      <pre style={{ background: '#f5f5f5', padding: '10px', borderRadius: '5px' }}>
        {JSON.stringify(info.cookies, null, 2)}
      </pre>

      <h2>🍪 Raw Cookies:</h2>
      <pre style={{ background: '#f5f5f5', padding: '10px', borderRadius: '5px', wordWrap: 'break-word' }}>
        {info.allCookies || 'No hay cookies'}
      </pre>

      <h2>🧪 Acciones de Prueba:</h2>
      <button 
        onClick={() => {
          SecurityService.setCookie('test-cookie', 'test-value', 7)
          alert('Cookie de prueba establecida. Recarga la página para verla.')
        }}
        style={{ padding: '10px', margin: '5px' }}
      >
        Establecer Cookie de Prueba
      </button>

      <button 
        onClick={() => {
          window.location.reload()
        }}
        style={{ padding: '10px', margin: '5px' }}
      >
        Recargar Página
      </button>

      <button 
        onClick={() => {
          SecurityService.logout()
        }}
        style={{ padding: '10px', margin: '5px' }}
      >
        Cerrar Sesión
      </button>
    </div>
  )
}
