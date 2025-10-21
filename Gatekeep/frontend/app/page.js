import React from 'react'

export default function Home() {
  return (
    <div style={{ 
      minHeight: '100vh', 
      backgroundColor: '#000000',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      fontFamily: 'Roboto, sans-serif'
    }}>
      <h1 style={{ 
        fontSize: '4rem', 
        fontWeight: 'bold', 
        color: '#ffffff',
        textAlign: 'center',
        margin: 0
      }}>
        Bienvenido a GateKeep
      </h1>
    </div>
  )
}
