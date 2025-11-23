"use client"

import React, { useEffect, useState, useRef } from 'react'
import { useParams, usePathname, useRouter } from 'next/navigation'
import Header from '../../../components/Header'
import { EdificioService } from '../../../services/EdificioService'
import { ReglaAccesoService } from '../../../services/ReglaAccesoService'
import { SecurityService } from '../../../services/securityService'
import { AccesoService } from '../../../services/AccesoService'
import TokenUtils from '../../../utils/tokenUtils'

export default function EdificioDetalle() {
  const params = useParams()
  const router = useRouter()
  const pathname = usePathname()
  const rawId = params?.id
  const id = Number(rawId)

  const [edificio, setEdificio] = useState(null)
  const [reglaAcceso, setReglaAcceso] = useState(null)
  const [loading, setLoading] = useState(true)
  const [loadingRegla, setLoadingRegla] = useState(false)
  const [error, setError] = useState(null)

  // Estados para el escáner QR
  const [showScanner, setShowScanner] = useState(false)
  const [isScanning, setIsScanning] = useState(false)
  const [scanResult, setScanResult] = useState(null)
  const [validationResult, setValidationResult] = useState(null)
  const [validationError, setValidationError] = useState(null)
  const [cameraError, setCameraError] = useState(null)
  const html5QrCodeRef = useRef(null)

  useEffect(() => {
    SecurityService.checkAuthAndRedirect(pathname)
  }, [pathname])

  useEffect(() => {
    if (!id || Number.isNaN(id)) {
      setError('ID inválido')
      setLoading(false)
      return
    }
    const fetchEdificio = async () => {
      try {
        const resp = await EdificioService.getEdificioById(id)
        setEdificio(resp.data)
        
        // Cargar regla de acceso si existe
        setLoadingRegla(true)
        try {
          const reglaResp = await ReglaAccesoService.getReglaAccesoPorEspacioId(id)
          if (reglaResp.data) {
            setReglaAcceso(reglaResp.data)
          }
        } catch (reglaError) {
          console.log('No hay regla de acceso para este edificio')
          setReglaAcceso(null)
        } finally {
          setLoadingRegla(false)
        }
      } catch (e) {
        console.error('Error cargando edificio', e)
        setError('No se encontró el edificio')
      } finally {
        setLoading(false)
      }
    }
    fetchEdificio()
  }, [id])

  // Función para iniciar el escáner
  const startScanner = async () => {
    if (typeof window === 'undefined') return
    
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
          let minEdge = Math.min(viewfinderWidth, viewfinderHeight);
          let qrboxSize = Math.floor(minEdge * 0.7);
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
          
          setIsScanning(false)
          setScanResult(decodedText)
          await validateAccess(decodedText)
        },
        () => {}
      )
      
      setIsScanning(true)
      setCameraError(null)
      setValidationResult(null)
      setValidationError(null)
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

  // Función para validar el acceso del usuario
  const validateAccess = async (token) => {
    try {
      setValidationError(null)
      console.log('[QR] validateAccess: token recibido', token)
      // Extraer el userId del token usando TokenUtils
      const decoded = TokenUtils.decodeToken(token)
      console.log('[QR] validateAccess: token decodificado', decoded)
      const usuarioId =
        decoded?.sub ||
        decoded?.userId ||
        decoded?.nameid ||
        decoded?.id ||
        decoded?.Id ||
        decoded?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]
      console.log('[QR] validateAccess: usuarioId extraído', usuarioId)
      if (!usuarioId) {
        setValidationError('No se pudo obtener el ID de usuario del QR')
        console.warn('[QR] validateAccess: No se pudo extraer el ID de usuario del token')
        return
      }
      // Obtener el punto de control del edificio (usamos el código o nombre del edificio)
      const puntoControl = edificio.CodigoEdificio || edificio.codigoEdificio || edificio.Nombre || edificio.nombre || `Edificio-${id}`
      console.log('[QR] validateAccess: puntoControl', puntoControl)
      // Validar acceso
      console.log('[QR] validateAccess: Enviando POST a AccesoService.validarAcceso', {
        usuarioId,
        espacioId: id,
        puntoControl
      })
      let response = null
      try {
        response = await AccesoService.validarAcceso({
          usuarioId: usuarioId,
          espacioId: id,
          puntoControl: puntoControl
        })
        console.log('[QR] validateAccess: respuesta del backend', response)
      } catch (err) {
        console.error('[QR] validateAccess: error en AccesoService.validarAcceso', err)
        setValidationError('Error de red o CORS al validar acceso')
        return
      }
      if (response?.data?.permitido) {
        setValidationResult({
          permitido: true,
          mensaje: 'Acceso Permitido',
          usuarioId: usuarioId,
          fecha: response.data.fecha
        })
        console.info('[QR] validateAccess: Acceso permitido', response.data)
      } else {
        setValidationError(response?.data?.razon || 'Acceso denegado')
        console.warn('[QR] validateAccess: Acceso denegado', response?.data)
      }
    } catch (err) {
      console.error('[QR] Error validando acceso:', err)
      const errorMsg = err.response?.data?.mensaje || err.response?.data?.Mensaje || 'Error al validar el acceso'
      setValidationError(errorMsg)
    }
  }

  // Función para reiniciar el escáner
  const resetScanner = () => {
    setScanResult(null)
    setValidationResult(null)
    setValidationError(null)
    setCameraError(null)
    startScanner()
  }

  // Limpiar escáner al desmontar
  useEffect(() => {
    return () => {
      if (html5QrCodeRef.current) {
        html5QrCodeRef.current.stop().catch(err => {
          console.error('Error al detener escáner:', err)
        })
      }
    }
  }, [])

  return (
    <div className="page-root">
      <Header />

      <section className="hero">
        <div className="hero-overlay" />
        <div className="hero-inner">
          <button className="back-btn" onClick={() => router.back()} aria-label="Volver">
            <span className="arrow">←</span> Volver
          </button>

          <div className="content">
            {loading ? (
              <div className="card skeleton">
                <div className="skeleton-line" style={{width:'60%'}} />
                <div className="skeleton-line" style={{width:'40%'}} />
                <div className="skeleton-line" style={{width:'30%'}} />
              </div>
            ) : error ? (
              <div className="card error"><h3>{error}</h3></div>
            ) : edificio ? (
              <article className="card">
                <header className="card-header">
                  <h1 className="title">{edificio.Nombre || edificio.nombre || 'Edificio'}</h1>
                  <span className={`badge ${((edificio.Activo ?? edificio.activo) ? 'ok' : 'off')}`}>
                    {(edificio.Activo ?? edificio.activo) ? 'Activo' : 'Inactivo'}
                  </span>
                </header>

                <div className="meta">
                  <div className="meta-item">
                    <span className="meta-label">Ubicación</span>
                    <span className="meta-value">{edificio.Ubicacion || edificio.ubicacion || 'No especificada'}</span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Capacidad</span>
                    <span className="meta-value">{edificio.Capacidad ?? edificio.capacidad ?? 'N/A'} personas</span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Pisos</span>
                    <span className="meta-value">{edificio.Pisos ?? edificio.pisos ?? 'N/A'}</span>
                  </div>
                </div>

                <div className="body">
                  {edificio.Descripcion || edificio.descripcion ? (
                    <p>{edificio.Descripcion || edificio.descripcion}</p>
                  ) : (
                    <p className="hint">No hay descripción adicional para este edificio.</p>
                  )}
                </div>
              </article>
            ) : null}

            {/* Regla de Acceso */}
            {!loading && edificio && (
              <article className="card regla-card">
                <header className="card-header">
                  <h2 className="subtitle">
                    <i className="pi pi-shield" style={{marginRight: '8px'}}></i>
                    Regla de Acceso
                  </h2>
                </header>

                {loadingRegla ? (
                  <div className="skeleton-regla">
                    <div className="skeleton-line" style={{width:'50%'}} />
                    <div className="skeleton-line" style={{width:'70%'}} />
                  </div>
                ) : reglaAcceso ? (
                  <div className="regla-content">
                    <div className="regla-info-grid">
                      <div className="regla-item">
                        <i className="pi pi-clock" style={{color: '#231F20', marginRight: '6px'}}></i>
                        <span className="regla-label">Horario:</span>
                        <span className="regla-value">
                          {new Date(reglaAcceso.HorarioApertura || reglaAcceso.horarioApertura).toISOString().slice(11,16)}
                          {' - '}
                          {new Date(reglaAcceso.HorarioCierre || reglaAcceso.horarioCierre).toISOString().slice(11,16)}
                        </span>
                      </div>
                      
                      <div className="regla-item">
                        <i className="pi pi-calendar" style={{color: '#231F20', marginRight: '6px'}}></i>
                        <span className="regla-label">Vigencia:</span>
                        <span className="regla-value">
                          {new Date(reglaAcceso.VigenciaApertura || reglaAcceso.vigenciaApertura).toLocaleDateString('es-ES')}
                          {' - '}
                          {new Date(reglaAcceso.VigenciaCierre || reglaAcceso.vigenciaCierre).toLocaleDateString('es-ES')}
                        </span>
                      </div>

                      <div className="regla-item">
                        <i className="pi pi-users" style={{color: '#231F20', marginRight: '6px'}}></i>
                        <span className="regla-label">Roles permitidos:</span>
                        <div className="roles-list">
                          {(reglaAcceso.RolesPermitidos || reglaAcceso.rolesPermitidos || []).map((rol, idx) => (
                            <span key={idx} className="rol-badge">{rol}</span>
                          ))}
                        </div>
                      </div>
                    </div>
                  </div>
                ) : (
                  <p className="hint">
                    <i className="pi pi-info-circle" style={{marginRight: '6px'}}></i>
                    No hay regla de acceso configurada para este edificio.
                  </p>
                )}
              </article>
            )}

            {/* Escáner QR para validar acceso */}
            {!loading && edificio && reglaAcceso && (
              <article className="card scanner-card">
                <header className="card-header">
                  <h2 className="subtitle">
                    <i className="pi pi-qrcode" style={{marginRight: '8px'}}></i>
                    Validar Acceso
                  </h2>
                </header>
                <div className="scanner-content">
                  <button className="btn-start-scan" onClick={() => setShowScanner(true)}>
                    <i className="pi pi-camera" style={{marginRight: '8px'}}></i>
                    Escanear QR
                  </button>
                </div>
                {showScanner && (
                  <div className="modal-overlay">
                    <div className="modal-content">
                      <button className="modal-close" onClick={async () => { setShowScanner(false); setIsScanning(false); setScanResult(null); setValidationResult(null); setValidationError(null); setCameraError(null); if (html5QrCodeRef.current) { await html5QrCodeRef.current.stop(); } }}>×</button>
                      <h3>Escanear QR de Usuario</h3>
                      <div id="qr-reader" style={{ width: '100%', minHeight: '260px', border: 'none', marginBottom: 12 }}></div>
                      {!isScanning && !scanResult && !cameraError && (
                        <button className="btn-start-scan" onClick={startScanner} style={{marginTop:8}}>Iniciar Escáner</button>
                      )}
                      {isScanning && (
                        <p className="scan-instruction">📷 Apunta la cámara al código QR del usuario</p>
                      )}
                      {cameraError && (
                        <div className="result-box error-box">
                          <div className="result-icon error-icon">✗</div>
                          <h3>Error de Cámara</h3>
                          <p>{cameraError}</p>
                          <button className="btn-retry" onClick={startScanner}>Reintentar</button>
                        </div>
                      )}
                      {validationResult && validationResult.permitido && (
                        <div className="result-box success-box">
                          <div className="result-icon success-icon">✓</div>
                          <h3>Acceso Permitido</h3>
                          <p className="result-message">El usuario tiene acceso autorizado a este edificio.</p>
                          <div className="result-details">
                            <div className="detail-item">
                              <span className="detail-label">Usuario ID:</span>
                              <span className="detail-value">{validationResult.usuarioId}</span>
                            </div>
                            <div className="detail-item">
                              <span className="detail-label">Fecha:</span>
                              <span className="detail-value">{new Date(validationResult.fecha).toLocaleString('es-ES')}</span>
                            </div>
                          </div>
                          <button className="btn-scan-again" onClick={resetScanner}>Escanear Otro QR</button>
                        </div>
                      )}
                      {validationError && (
                        <div className="result-box error-box">
                          <div className="result-icon error-icon">✗</div>
                          <h3>Acceso Denegado</h3>
                          <p className="result-message">{validationError}</p>
                          <button className="btn-scan-again" onClick={resetScanner}>Escanear Otro QR</button>
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </article>
            )}
          </div>
        </div>
      </section>

      <style jsx>{`
        .page-root {
          width: 100%;
          min-height: 100vh;
          background: #0f0f10;
          color: #231F20;
        }

        .hero {
          position: relative;
          width: 100%;
          min-height: 70vh;
          display: flex;
          align-items: flex-start;
          justify-content: center;
          padding: 24px;
          box-sizing: border-box;
          background: radial-gradient(1000px 500px at 10% -10%, rgba(243,116,38,0.3), transparent),
                      radial-gradient(800px 500px at 90% 0%, rgba(243,116,38,0.18), transparent),
                      linear-gradient(180deg, #1c1a1b 0%, #231F20 100%);
        }

        .hero-overlay {
          position: absolute;
          inset: 0;
          pointer-events: none;
          box-shadow: inset 0 80px 120px rgba(0,0,0,0.45), inset 0 -80px 120px rgba(0,0,0,0.45);
        }

        .hero-inner {
          position: relative;
          width: 100%;
          max-width: 980px;
          display: flex;
          flex-direction: column;
          gap: 18px;
        }

        .back-btn {
          align-self: flex-start;
          background: transparent;
          color: #F37426;
          border: 1px solid rgba(243,116,38,0.35);
          border-radius: 9999px;
          padding: 8px 12px;
          font-weight: 700;
          cursor: pointer;
          display: inline-flex;
          align-items: center;
          gap: 8px;
          transition: background 0.2s ease, transform 0.1s ease;
        }
        .back-btn:hover { background: rgba(243,116,38,0.08); }
        .back-btn:active { transform: translateY(1px); }
        .arrow { font-size: 1.1rem; line-height: 1; }

        .content { width: 100%; }

        .card {
          background: #F37426;
          color: #231F20;
          border-radius: 20px;
          padding: 20px;
          box-shadow: 0 10px 30px rgba(0,0,0,0.25);
        }

        .card-header {
          display: flex;
          align-items: center;
          justify-content: space-between;
          gap: 12px;
          margin-bottom: 12px;
        }

        .title {
          margin: 0;
          font-size: clamp(1.2rem, 2.4vw, 1.8rem);
          line-height: 1.2;
          letter-spacing: 0.2px;
        }

        .badge {
          display: inline-flex;
          align-items: center;
          height: 28px;
          padding: 0 10px;
          font-size: 0.85rem;
          font-weight: 800;
          border-radius: 9999px;
          border: 2px solid rgba(0,0,0,0.12);
          background: #fff6ee;
        }
        .badge.ok { background: #d1fae5; color: #065f46; border-color: rgba(6,95,70,0.2); }
        .badge.off { background: #fee2e2; color: #991b1b; border-color: rgba(153,27,27,0.2); }

        .meta { display:flex; gap: 18px; flex-wrap: wrap; margin-bottom: 6px; }
        .meta-item { display:flex; flex-direction:column; }
        .meta-label { font-size: 0.78rem; font-weight: 700; color: rgba(35,31,32,0.8); }
        .meta-value { font-size: 0.95rem; font-weight: 700; }

        .body { margin-top: 8px; }
        .body p { margin: 0; font-size: 0.9rem; line-height: 1.5; }
        .hint { opacity: 0.85; }

        /* Regla de Acceso */
        .regla-card {
          margin-top: 16px;
          background: linear-gradient(135deg, rgba(243,116,38,0.95) 0%, rgba(243,116,38,0.85) 100%);
        }

        .subtitle {
          margin: 0;
          font-size: 1.1rem;
          display: flex;
          align-items: center;
          font-weight: 700;
        }

        .regla-content {
          margin-top: 12px;
        }

        .regla-info-grid {
          display: flex;
          flex-direction: column;
          gap: 12px;
        }

        .regla-item {
          display: flex;
          align-items: flex-start;
          gap: 8px;
          flex-wrap: wrap;
        }

        .regla-label {
          font-weight: 700;
          font-size: 0.85rem;
          color: rgba(35,31,32,0.8);
        }

        .regla-value {
          font-weight: 600;
          font-size: 0.9rem;
          color: #231F20;
        }

        .roles-list {
          display: flex;
          gap: 6px;
          flex-wrap: wrap;
        }

        .rol-badge {
          background: rgba(255,255,255,0.9);
          color: #231F20;
          padding: 4px 10px;
          border-radius: 12px;
          font-size: 0.8rem;
          font-weight: 700;
          border: 1px solid rgba(35,31,32,0.15);
        }

        .skeleton-regla {
          margin-top: 12px;
        }

        /* Skeleton */
        .skeleton { position: relative; overflow: hidden; }
        .skeleton::after { content: ''; position: absolute; inset: 0; background: linear-gradient(90deg, rgba(255,255,255,0) 0%, rgba(255,255,255,0.25) 50%, rgba(255,255,255,0) 100%); animation: shimmer 1.4s infinite; }
        .skeleton-line { height: 16px; border-radius: 8px; background: rgba(255,255,255,0.5); margin: 10px 0; }
        @keyframes shimmer { 0% { transform: translateX(-100%); } 100% { transform: translateX(100%); } }

        /* Error state */
        .card.error { background: #fee2e2; color: #991b1b; border: 2px solid rgba(153,27,27,0.3); }

        /* Escáner QR */
        .scanner-card {
          margin-top: 16px;
          background: linear-gradient(135deg, rgba(243,116,38,0.95) 0%, rgba(243,116,38,0.85) 100%);
        }

        .scanner-content {
          margin-top: 12px;
        }

        .scanner-idle {
          display: flex;
          flex-direction: column;
          align-items: center;
          gap: 12px;
        }

        .btn-start-scan {
          background: rgba(255,255,255,0.9);
          color: #F37426;
          border: 2px solid rgba(35,31,32,0.15);
          border-radius: 12px;
          padding: 12px 24px;
          font-size: 1rem;
          font-weight: 700;
          cursor: pointer;
          display: inline-flex;
          align-items: center;
          transition: all 0.2s ease;
        }

        .btn-start-scan:hover {
          background: white;
          transform: translateY(-2px);
          box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        }

        .btn-start-scan:active {
          transform: translateY(0);
        }

        .scanner-active {
          display: flex;
          flex-direction: column;
          gap: 12px;
        }

        .scan-instruction {
          text-align: center;
          font-size: 0.9rem;
          font-weight: 600;
          margin: 0;
          padding: 8px;
          background: rgba(255,255,255,0.9);
          border-radius: 8px;
          color: #231F20;
        }

        .result-box {
          display: flex;
          flex-direction: column;
          align-items: center;
          gap: 12px;
          padding: 16px;
          border-radius: 12px;
          text-align: center;
        }

        .success-box {
          background: rgba(209,250,229,0.95);
          border: 2px solid rgba(6,95,70,0.3);
        }

        .error-box {
          background: rgba(254,226,226,0.95);
          border: 2px solid rgba(153,27,27,0.3);
        }

        .result-icon {
          width: 60px;
          height: 60px;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 2rem;
          font-weight: 800;
        }

        .success-icon {
          background: #065f46;
          color: white;
        }

        .error-icon {
          background: #991b1b;
          color: white;
        }

        .result-box h3 {
          margin: 0;
          font-size: 1.2rem;
          color: #231F20;
        }

        .result-message {
          margin: 0;
          font-size: 0.9rem;
          color: rgba(35,31,32,0.9);
        }

        .result-details {
          display: flex;
          flex-direction: column;
          gap: 8px;
          width: 100%;
          padding: 12px;
          background: rgba(255,255,255,0.5);
          border-radius: 8px;
        }

        .detail-item {
          display: flex;
          justify-content: space-between;
          align-items: center;
        }

        .detail-label {
          font-weight: 700;
          font-size: 0.85rem;
          color: rgba(35,31,32,0.8);
        }

        .detail-value {
          font-weight: 600;
          font-size: 0.9rem;
          color: #231F20;
        }

        .btn-scan-again,
        .btn-retry {
          background: #F37426;
          color: white;
          border: 2px solid rgba(35,31,32,0.15);
          border-radius: 12px;
          padding: 10px 20px;
          font-size: 0.95rem;
          font-weight: 700;
          cursor: pointer;
          transition: all 0.2s ease;
        }

        .btn-scan-again:hover,
        .btn-retry:hover {
          background: #ff8d45;
          transform: translateY(-2px);
          box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        }

        .btn-scan-again:active,
        .btn-retry:active {
          transform: translateY(0);
        }

        @media (max-width: 768px) {
          .hero { padding: 16px; min-height: 60vh; }
          .card { padding: 16px; border-radius: 16px; }
        }
      `}</style>
    </div>
  )
}
