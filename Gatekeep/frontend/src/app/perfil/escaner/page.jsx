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
      const apiBase = URLService.getBaseUrl();
      const response = await fetch(`${apiBase}/auth/validate`, {
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
        <h1>Escáner de Credenciales</h1>
        
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
          background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
        }

        .scanner-container {
          max-width: 600px;
          margin: 0 auto;
          padding: 40px 20px;
        }

        h1 {
          color: white;
          text-align: center;
          margin-bottom: 30px;
          font-size: 2rem;
        }

        .scanner-wrapper {
          background: white;
          border-radius: 12px;
          padding: 20px;
          box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
          min-height: 400px;
          display: flex;
          align-items: center;
          justify-content: center;
        }

        .camera-container {
          width: 100%;
          max-width: 500px;
          position: relative;
        }

        .scan-instruction {
          text-align: center;
          color: #666;
          margin-top: 15px;
          font-size: 1rem;
          font-weight: 600;
        }

        .scan-tip {
          text-align: center;
          color: #999;
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
          width: 80px;
          height: 80px;
          background: #4caf50;
          color: white;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 3rem;
          margin: 0 auto 20px;
        }

        .error-icon {
          width: 80px;
          height: 80px;
          background: #f44336;
          color: white;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 3rem;
          margin: 0 auto 20px;
        }

        .success-card h2 {
          color: #4caf50;
          margin-bottom: 20px;
        }

        .error-card h2 {
          color: #f44336;
          margin-bottom: 20px;
        }

        .user-info {
          margin: 20px 0;
          text-align: left;
        }

        .info-row {
          display: flex;
          justify-content: space-between;
          padding: 12px;
          border-bottom: 1px solid #eee;
        }

        .info-row:last-child {
          border-bottom: none;
        }

        .label {
          font-weight: 600;
          color: #555;
        }

        .value {
          color: #333;
        }

        .error-message {
          color: #666;
          margin: 20px 0;
          font-size: 1.1rem;
        }

        .btn-scan-again {
          background: #2196f3;
          color: white;
          border: none;
          padding: 12px 30px;
          border-radius: 6px;
          font-size: 1rem;
          cursor: pointer;
          margin-top: 20px;
          transition: background 0.3s;
        }

        .btn-scan-again:hover {
          background: #1976d2;
        }

        .btn-back {
          width: 100%;
          background: white;
          color: #2196f3;
          border: 2px solid white;
          padding: 12px 30px;
          border-radius: 6px;
          font-size: 1rem;
          cursor: pointer;
          margin-top: 20px;
          transition: all 0.3s;
        }

        .btn-back:hover {
          background: transparent;
          color: white;
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
          border: 4px solid #f3f3f3;
          border-top: 4px solid #2196f3;
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
            padding: 10px;
          }
        }
      `}</style>
    </div>
  )
}
