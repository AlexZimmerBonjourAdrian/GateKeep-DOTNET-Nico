"use client"

import React, { useState, useEffect, useRef } from 'react'
import { useRouter } from 'next/navigation'
import Header from '../../../components/Header'
import { URLService } from '../../../services/urlService'

export default function EscanerQR() {
  const router = useRouter()
  const [scanResult, setScanResult] = useState(null)
  const [isScanning, setIsScanning] = useState(true)
  const [userData, setUserData] = useState(null)
  const [error, setError] = useState(null)
  const [cameraError, setCameraError] = useState(null)
  const scannerRef = useRef(null)
  const html5QrCodeRef = useRef(null)

  useEffect(() => {
    let isMounted = true
    
    const initScanner = async () => {
      if (typeof window === 'undefined') return
      
      try {
        // Importar dinámicamente la librería
        const { Html5Qrcode } = await import('html5-qrcode')
        
        if (!isMounted) return
        
        const html5QrCode = new Html5Qrcode("qr-reader")
        html5QrCodeRef.current = html5QrCode
        
        const config = { 
          fps: 10,
          qrbox: function(viewfinderWidth, viewfinderHeight) {
            // Cuadro más grande y adaptativo
            let minEdge = Math.min(viewfinderWidth, viewfinderHeight);
            let qrboxSize = Math.floor(minEdge * 0.7); // 70% del área disponible
            return {
              width: qrboxSize,
              height: qrboxSize
            };
          },
          aspectRatio: 1.0
        }
        
        await html5QrCode.start(
          { facingMode: "environment" },
          config,
          async (decodedText, decodedResult) => {
            console.log('QR detectado:', decodedText)
            
            // Detener el escáner
            if (html5QrCodeRef.current) {
              await html5QrCodeRef.current.stop()
            }
            
            setScanResult(decodedText)
            setIsScanning(false)
            
            // Validar el token
            await validateToken(decodedText)
          },
          (errorMessage) => {
            // Ignorar errores de "no se encontró QR"
          }
        )
        
        console.log('Escáner iniciado correctamente')
      } catch (err) {
        console.error('Error inicializando escáner:', err)
        if (err.name === 'NotAllowedError') {
          setCameraError('Permisos de cámara denegados. Por favor, permite el acceso a la cámara.')
        } else if (err.name === 'NotFoundError') {
          setCameraError('No se encontró ninguna cámara en el dispositivo.')
        } else {
          setCameraError('Error al inicializar la cámara: ' + err.message)
        }
      }
    }
    
    initScanner()
    
    return () => {
      isMounted = false
      if (html5QrCodeRef.current) {
        html5QrCodeRef.current.stop().catch(err => {
          console.error('Error al detener escáner:', err)
        })
      }
    }
  }, [])

  const validateToken = async (token) => {
    try {
      setError(null)
      
      // Usar URLService para obtener la URL correcta (producción o desarrollo)
      const apiBase = URLService.getLink(); // Incluye /api/
      const response = await fetch(`${apiBase}auth/validate`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      })

      if (!response.ok) {
        if (response.status === 401) {
          setError('QR inválido o expirado')
        } else {
          setError('Error al validar las credenciales')
        }
        return
      }

      const data = await response.json()
      
      console.log('Datos recibidos del backend:', data)
      
      if (data.isValid) {
        setUserData(data.user)
      } else {
        setError('Credenciales inválidas')
      }
    } catch (err) {
      console.error('Error validando token:', err)
      setError('Error de conexión con el servidor')
    }
  }

  const resetScanner = async () => {
    setScanResult(null)
    setUserData(null)
    setError(null)
    setCameraError(null)
    setIsScanning(true)
    
    // Reiniciar escáner
    try {
      const { Html5Qrcode } = await import('html5-qrcode')
      
      if (html5QrCodeRef.current) {
        await html5QrCodeRef.current.stop()
      }
      
      const html5QrCode = new Html5Qrcode("qr-reader")
      html5QrCodeRef.current = html5QrCode
      
      const config = { 
        fps: 10,
        qrbox: function(viewfinderWidth, viewfinderHeight) {
          // Cuadro más grande y adaptativo
          let minEdge = Math.min(viewfinderWidth, viewfinderHeight);
          let qrboxSize = Math.floor(minEdge * 0.7); // 70% del área disponible
          return {
            width: qrboxSize,
            height: qrboxSize
          };
        },
        aspectRatio: 1.0
      }
      
      await html5QrCode.start(
        { facingMode: "environment" },
        config,
        async (decodedText) => {
          console.log('QR detectado:', decodedText)
          
          if (html5QrCodeRef.current) {
            await html5QrCodeRef.current.stop()
          }
          
          setScanResult(decodedText)
          setIsScanning(false)
          await validateToken(decodedText)
        },
        () => {}
      )
    } catch (err) {
      console.error('Error reiniciando escáner:', err)
      setCameraError('Error al reiniciar la cámara')
    }
  }

  return (
    <div className="container">
      <Header />
      
      <div className="scanner-container">
        <h1 style={{color: '#F37426'}}>Escáner de Credenciales</h1>
        
        <div className="scanner-wrapper">
          {isScanning && !cameraError && (
            <div className="camera-container">
              <div id="qr-reader" style={{ width: '100%' }}></div>
              <p className="scan-instruction">📷 Apunta la cámara al código QR</p>
              <p className="scan-tip">Aleja la cámara si el QR es muy grande. Mantén buena iluminación.</p>
            </div>
          )}
          
          {cameraError && (
            <div className="error-card">
              <div className="error-icon">📷</div>
              <h2>Error de Cámara</h2>
              <p className="error-message">{cameraError}</p>
              <button className="btn-scan-again" onClick={() => window.location.reload()}>
                Reintentar
              </button>
            </div>
          )}
          
          {scanResult && !isScanning && (
            <div className="result-container">
              {userData ? (
                <div className="success-card">
                  <div className="success-icon">✓</div>
                  <h2>Credenciales Válidas</h2>
                  <div className="user-info">
                    <div className="info-row">
                      <span className="label">Nombre:</span>
                      <span className="value">
                        {userData.nombre || userData.apellido 
                          ? `${userData.nombre || ''} ${userData.apellido || ''}`.trim() 
                          : 'No especificado'}
                      </span>
                    </div>
                    <div className="info-row">
                      <span className="label">Email:</span>
                      <span className="value">{userData.email || 'No especificado'}</span>
                    </div>
                    <div className="info-row">
                      <span className="label">Rol:</span>
                      <span className="value">{userData.rol || 'No especificado'}</span>
                    </div>
                    <div className="info-row">
                      <span className="label">ID:</span>
                      <span className="value">{userData.id || 'No especificado'}</span>
                    </div>
                  </div>
                  <button className="btn-scan-again" onClick={resetScanner}>
                    Escanear Otro QR
                  </button>
                </div>
              ) : error ? (
                <div className="error-card">
                  <div className="error-icon">✗</div>
                  <h2>Error de Validación</h2>
                  <p className="error-message">{error}</p>
                  <button className="btn-scan-again" onClick={resetScanner}>
                    Intentar de Nuevo
                  </button>
                </div>
              ) : (
                <div className="loading-card">
                  <div className="spinner"></div>
                  <p>Validando credenciales...</p>
                </div>
              )}
            </div>
          )}
        </div>
        
        <button className="btn-back" onClick={() => router.push('/perfil')}>
          Volver al Perfil
        </button>
      </div>

      <style jsx>{`
        .container {
          min-height: 100vh;
          background: #0f0f10;
          color: #231F20;
        }

        .scanner-container {
          max-width: 600px;
          margin: 0 auto;
          padding: 40px 20px;
        }

        h1 {
          color: #F37426;
          text-align: center;
          margin-bottom: 30px;
          font-size: 2rem;
        }

        .scanner-wrapper {
          background: #1c1a1b;
          border-radius: 20px;
          padding: 30px;
          box-shadow: 0 10px 30px rgba(0,0,0,0.25);
          min-height: 400px;
          display: flex;
          align-items: center;
          justify-content: center;
          border: 2px solid rgba(243,116,38,0.2);
        }

        .camera-container {
          width: 100%;
          max-width: 500px;
          position: relative;
        }

        .scan-instruction {
          text-align: center;
          color: rgba(255,255,255,0.9);
          margin-top: 15px;
          font-size: 0.9rem;
          font-weight: 600;
          padding: 8px;
          background: rgba(255,255,255,0.1);
          border-radius: 8px;
        }

        .scan-tip {
          text-align: center;
          color: rgba(255,255,255,0.7);
          margin-top: 8px;
          font-size: 0.85rem;
        }

        .result-container {
          width: 100%;
        }

        .success-card,
        .error-card,
        .loading-card {
          text-align: center;
          padding: 30px;
        }

        .success-icon {
          width: 60px;
          height: 60px;
          background: #065f46;
          color: white;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 2rem;
          font-weight: 800;
          margin: 0 auto 20px;
        }

        .error-icon {
          width: 60px;
          height: 60px;
          background: #991b1b;
          color: white;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 2rem;
          font-weight: 800;
          margin: 0 auto 20px;
        }

        .success-card h2 {
          color: #065f46;
          margin-bottom: 20px;
          font-size: 1.2rem;
        }

        .error-card h2 {
          color: #991b1b;
          margin-bottom: 20px;
          font-size: 1.2rem;
        }

        .user-info {
          margin: 20px 0;
          text-align: left;
          background: rgba(255,255,255,0.1);
          border-radius: 12px;
          padding: 16px;
        }

        .info-row {
          display: flex;
          justify-content: space-between;
          padding: 12px;
          border-bottom: 1px solid rgba(255,255,255,0.1);
        }

        .info-row:last-child {
          border-bottom: none;
        }

        .label {
          font-weight: 700;
          color: rgba(255,255,255,0.8);
          font-size: 0.85rem;
        }

        .value {
          color: #fff;
          font-weight: 600;
          font-size: 0.9rem;
        }

        .error-message {
          color: rgba(255,255,255,0.9);
          margin: 20px 0;
          font-size: 0.9rem;
        }

        .btn-scan-again {
          background: #F37426;
          color: white;
          border: 2px solid rgba(35,31,32,0.15);
          padding: 10px 20px;
          border-radius: 12px;
          font-size: 0.95rem;
          font-weight: 700;
          cursor: pointer;
          margin-top: 20px;
          transition: all 0.2s ease;
        }

        .btn-scan-again:hover {
          background: #ff8d45;
          transform: translateY(-2px);
          box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        }

        .btn-scan-again:active {
          transform: translateY(0);
        }

        .btn-back {
          width: 100%;
          background: transparent;
          color: #F37426;
          border: 1px solid rgba(243,116,38,0.35);
          padding: 12px 30px;
          border-radius: 9999px;
          font-size: 1rem;
          font-weight: 700;
          cursor: pointer;
          margin-top: 20px;
          transition: all 0.2s ease;
        }

        .btn-back:hover {
          background: rgba(243,116,38,0.08);
        }

        .btn-back:active {
          transform: translateY(1px);
        }

        .loading-card {
          display: flex;
          flex-direction: column;
          align-items: center;
          gap: 20px;
        }

        .spinner {
          width: 50px;
          height: 50px;
          border: 4px solid rgba(255,255,255,0.1);
          border-top: 4px solid #F37426;
          border-radius: 50%;
          animation: spin 1s linear infinite;
        }

        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }

        @media (max-width: 768px) {
          h1 {
            font-size: 1.5rem;
          }

          .scanner-wrapper {
            padding: 20px;
          }

          .scanner-container {
            padding: 20px 16px;
          }
        }
      `}</style>
    </div>
  )
}
