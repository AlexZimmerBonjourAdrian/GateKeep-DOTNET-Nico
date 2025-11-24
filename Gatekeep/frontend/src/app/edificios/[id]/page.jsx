"use client"

import React, { useEffect, useState, useRef } from 'react'
import { NotificacionService } from '../../../services/NotificacionService'
import { useParams, usePathname, useRouter } from 'next/navigation'
import Header from '../../../components/Header'
import { EdificioService } from '../../../services/EdificioService'
import { ReglaAccesoService } from '../../../services/ReglaAccesoService'
import { SecurityService } from '../../../services/securityService'
import { AccesoService } from '../../../services/AccesoService'
import TokenUtils from '../../../utils/tokenUtils'

export default function EdificioDetalle() {
    // Manejo de errores personalizados para validación QR
    const errorMessages = {
      FUERA_DE_HORARIO: (data) => `El acceso está fuera del horario permitido. Horario permitido: ${data?.detallesAdicionales?.HorarioApertura} - ${data?.detallesAdicionales?.HorarioCierre}. Hora actual: ${data?.detallesAdicionales?.HoraActual}`,
      FUERA_DE_VIGENCIA: () => 'El acceso está fuera de la vigencia permitida.',
      REGLAS_NO_CONFIGURADAS: () => 'No hay reglas de acceso configuradas para este espacio.',
      ROL_NO_PERMITIDO: () => 'Tu rol no tiene permiso para acceder a este espacio.',
      USUARIO_NO_EXISTE: () => 'El usuario no existe.',
      ESPACIO_NO_EXISTE: () => 'El espacio no existe.',
      ESPACIO_INACTIVO: () => 'El espacio está inactivo.',
      USUARIO_INVALIDO: () => 'El usuario no es válido.',
      PUNTO_CONTROL_REQUERIDO: () => 'El punto de control es requerido.',
      USUARIO_ID_INVALIDO: () => 'El ID de usuario es inválido.',
      ESPACIO_ID_INVALIDO: () => 'El ID de espacio es inválido.',
      ACCESO_DENEGADO: () => 'Acceso denegado.',
      ERROR_INTERNO: () => 'Error interno del servidor.',
    };
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
      // Extraer el userId del token usando TokenUtils
      const decoded = TokenUtils.decodeToken(token)
      const usuarioId =
        decoded?.sub ||
        decoded?.userId ||
        decoded?.nameid ||
        decoded?.id ||
        decoded?.Id ||
        decoded?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]
      if (!usuarioId) {
        setValidationError('No se pudo obtener el ID de usuario del QR')
        // Notificación de rechazo
        try {
          await NotificacionService.crearNotificacion({
            mensaje: `Intento de acceso fallido: QR inválido en edificio ${edificio?.Nombre || edificio?.nombre || id}`,
            tipo: 'AccesoRechazado'
          });
        } catch (e) { /* opcional: manejar error de notificación */ }
        return
      }
      // Obtener el punto de control del edificio (usamos el código o nombre)
      const puntoControl = edificio.CodigoEdificio || edificio.codigoEdificio || edificio.Nombre || edificio.nombre || `Edificio-${id}`
      // Validar acceso
      const response = await AccesoService.validarAcceso({
        usuarioId: usuarioId,
        espacioId: id,
        puntoControl: String(puntoControl)
      })
      if (response.data.permitido) {
        setValidationResult({
          permitido: true,
          mensaje: 'Acceso Permitido',
          usuarioId: usuarioId,
          fecha: response.data.fecha
        })
      } else {
        setValidationError(response.data.razon || 'Acceso denegado')
        // Notificación de rechazo
        try {
          await NotificacionService.crearNotificacion({
            mensaje: `Acceso rechazado para usuario ${usuarioId} en edificio ${edificio?.Nombre || edificio?.nombre || id}: ${response.data.razon || 'Acceso denegado'}`,
            tipo: 'AccesoRechazado'
          });
        } catch (e) { /* opcional: manejar error de notificación */ }
      }
    } catch (err) {
      console.error('Error validando acceso:', err)
      const codigoError = err.response?.data?.codigoError;
      const data = err.response?.data;
      if (codigoError && errorMessages[codigoError]) {
        setValidationError(errorMessages[codigoError](data));
        // Notificación de rechazo
        try {
          await NotificacionService.crearNotificacion({
            mensaje: `Acceso rechazado para usuario ${usuarioId} en edificio ${edificio?.Nombre || edificio?.nombre || id}: ${errorMessages[codigoError](data)}`,
            tipo: 'AccesoRechazado'
          });
        } catch (e) { /* opcional: manejar error de notificación */ }
      } else {
        const errorMsg = data?.mensaje || data?.Mensaje || 'Error al validar el acceso';
        setValidationError(errorMsg);
        // Notificación de rechazo
        try {
          await NotificacionService.crearNotificacion({
            mensaje: `Acceso rechazado para usuario ${usuarioId} en edificio ${edificio?.Nombre || edificio?.nombre || id}: ${errorMsg}`,
            tipo: 'AccesoRechazado'
          });
        } catch (e) { /* opcional: manejar error de notificación */ }
      }
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

  // Función para cerrar el escáner (modal)
  const cerrarEscaner = async () => {
    if (html5QrCodeRef.current && isScanning) {
      await html5QrCodeRef.current.stop()
    }
    setShowScanner(false)
    setIsScanning(false)
    setScanResult(null)
    setValidationError(null)
  }


  // Iniciar escáner automáticamente al abrir el modal
  useEffect(() => {
    if (showScanner && isScanning) {
      startScanner();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [showScanner, isScanning]);

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
                    <span className="meta-value">{edificio.NumeroPisos ?? edificio.numeroPisos ?? 'N/A'}</span>
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
                          {(reglaAcceso.HorarioApertura || reglaAcceso.horarioApertura)?.slice(0,5)}
                          {' - '}
                          {(reglaAcceso.HorarioCierre || reglaAcceso.horarioCierre)?.slice(0,5)}
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

            {/* Botón y modal de escáner QR idénticos a Beneficio */}
            {!loading && edificio && reglaAcceso && (
              <article className="card">
                <div className="body">
                  <button className="canjear-btn" onClick={() => { setShowScanner(true); setIsScanning(true); setValidationError(null); setScanResult(null); }}>
                    Validar acceso con QR
                  </button>
                </div>
              </article>
            )}
            {/* Modal del escáner QR */}
            {showScanner && (
              <div className="modal-overlay" onClick={cerrarEscaner}>
                <div className="modal-content" onClick={e => e.stopPropagation()}>
                  <button className="close-btn" onClick={cerrarEscaner}>✕</button>
                  <h2 className="scanner-title">Escanea tu QR de Credenciales</h2>
                  <p className="scanner-hint">Escanea el QR que aparece en tu perfil</p>
                  {validationResult && validationResult.permitido ? (
                    <div className="success-message">
                      <div className="success-icon">✓</div>
                      <p>¡Acceso concedido!</p>
                    </div>
                  ) : validationError ? (
                    <div className="error-message">
                      <p>{validationError}</p>
                      <button className="retry-btn" onClick={resetScanner}>Reintentar</button>
                    </div>
                  ) : isScanning ? (
                    <div id="qr-reader" className="qr-reader"></div>
                  ) : null}
                </div>
              </div>
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


        /* Botón y modal QR idénticos a Beneficio */
        .canjear-btn {
          width: 100%;
          background: #231F20;
          color: #F37426;
          border: none;
          border-radius: 12px;
          padding: 14px 20px;
          font-size: 1rem;
          font-weight: 700;
          cursor: pointer;
          transition: all 0.2s ease;
          display: flex;
          align-items: center;
          justify-content: center;
          gap: 8px;
        }
        .canjear-btn:hover {
          background: #1a1617;
          transform: translateY(-2px);
          box-shadow: 0 6px 20px rgba(0,0,0,0.3);
        }
        .canjear-btn:active {
          transform: translateY(0);
        }

        .modal-overlay {
          position: fixed;
          inset: 0;
          background: rgba(0,0,0,0.85);
          display: flex;
          align-items: center;
          justify-content: center;
          z-index: 1000;
          padding: 20px;
        }

        .modal-content {
          position: relative;
          background: #1c1a1b;
          border-radius: 20px;
          padding: 30px;
          max-width: 500px;
          width: 100%;
          color: #fff;
        }

        .close-btn {
          position: absolute;
          top: 15px;
          right: 15px;
          background: rgba(255,255,255,0.1);
          border: none;
          color: #fff;
          font-size: 24px;
          width: 40px;
          height: 40px;
          border-radius: 50%;
          cursor: pointer;
          display: flex;
          align-items: center;
          justify-content: center;
          transition: background 0.2s;
        }
        .close-btn:hover {
          background: rgba(255,255,255,0.2);
        }

        .scanner-title {
          margin: 0 0 8px 0;
          font-size: 1.5rem;
          color: #F37426;
        }

        .scanner-hint {
          margin: 0 0 20px 0;
          font-size: 0.9rem;
          color: rgba(255,255,255,0.7);
        }

        .qr-reader {
          width: 100%;
          border-radius: 12px;
          overflow: hidden;
        }

        .success-message {
          text-align: center;
          padding: 40px 20px;
        }

        .success-icon {
          width: 80px;
          height: 80px;
          background: #10b981;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 48px;
          color: white;
          margin: 0 auto 20px;
          animation: scaleIn 0.3s ease;
        }

        @keyframes scaleIn {
          from { transform: scale(0); }
          to { transform: scale(1); }
        }

        .success-message p {
          font-size: 1.1rem;
          margin: 10px 0;
        }

        .error-message {
          text-align: center;
          padding: 30px 20px;
          background: rgba(239, 68, 68, 0.1);
          border-radius: 12px;
          border: 2px solid rgba(239, 68, 68, 0.3);
        }

        .error-message p {
          color: #fca5a5;
          margin: 0 0 20px 0;
        }

        .retry-btn {
          background: #F37426;
          color: #231F20;
          border: none;
          border-radius: 8px;
          padding: 10px 20px;
          font-size: 0.95rem;
          font-weight: 700;
          cursor: pointer;
          transition: all 0.2s;
        }
        .retry-btn:hover {
          background: #e66815;
          transform: translateY(-2px);
        }

        @media (max-width: 768px) {
          .hero { padding: 16px; min-height: 60vh; }
          .card { padding: 16px; border-radius: 16px; }
        }
      `}</style>
    </div>
  )
}
