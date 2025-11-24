"use client"

import React, { useEffect, useState, useRef } from 'react'
import { useParams, usePathname, useRouter } from 'next/navigation'
import Header from '../../../components/Header'
import { BeneficioService } from '../../../services/BeneficioService'
import { SecurityService } from '../../../services/securityService'
import TokenUtils from '../../../utils/tokenUtils'

export default function BeneficioDetalle() {
  const params = useParams()
  const router = useRouter()
  const pathname = usePathname()
  const rawId = params?.id
  const id = Number(rawId)

  const [beneficio, setBeneficio] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [showScanner, setShowScanner] = useState(false)
  const [isScanning, setIsScanning] = useState(false)
  const [scanResult, setScanResult] = useState(null)
  const [canjeError, setCanjeError] = useState(null)
  const [canjeSuccess, setCanjeSuccess] = useState(false)
  const html5QrCodeRef = useRef(null)
  const [usuarioId, setUsuarioId] = useState(null)

  useEffect(() => {
    SecurityService.checkAuthAndRedirect(pathname)
    // Obtener ID del usuario actual (mantenerlo como string para comparación)
    try {
      const id = SecurityService.getUserId()
      if (id) {
        setUsuarioId(id) // Guardar como string
      }
    } catch (error) {
      console.error('Error obteniendo usuario:', error)
    }
  }, [pathname])

  useEffect(() => {
    if (!id || Number.isNaN(id)) {
      setError('ID inválido')
      setLoading(false)
      return
    }
    const fetchBeneficio = async () => {
      try {
        const resp = await BeneficioService.getBeneficioById(id)
        setBeneficio(resp.data)
      } catch (e) {
        console.error('Error cargando beneficio', e)
        setError('No se encontró el beneficio')
      } finally {
        setLoading(false)
      }
    }
    fetchBeneficio()
  }, [id])

  const getTipoTexto = (tipo) => {
    if (tipo == 0 || tipo === 'Canje' || (typeof tipo === 'string' && tipo.toLowerCase() === 'canje')) return 'Canje';
    if (tipo == 1 || tipo === 'Consumo' || (typeof tipo === 'string' && tipo.toLowerCase() === 'consumo')) return 'Consumo';
    return 'Desconocido';
  }

  const iniciarEscaneo = async () => {
    setShowScanner(true)
    setIsScanning(true)
    setCanjeError(null)
    setScanResult(null)
    setCanjeSuccess(false)

    try {
      const { Html5Qrcode } = await import('html5-qrcode')
      const html5QrCode = new Html5Qrcode("qr-reader-canje")
      html5QrCodeRef.current = html5QrCode

      const config = {
        fps: 10,
        qrbox: function(viewfinderWidth, viewfinderHeight) {
          let minEdge = Math.min(viewfinderWidth, viewfinderHeight);
          let qrboxSize = Math.floor(minEdge * 0.7);
          return { width: qrboxSize, height: qrboxSize };
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
          await validarYCanjear(decodedText)
        },
        () => { /* Ignorar errores de "no se encontró QR" */ }
      )
    } catch (err) {
      console.error('Error inicializando escáner:', err)
      setCanjeError('Error al inicializar la cámara')
      setIsScanning(false)
    }
  }

  const validarYCanjear = async (token) => {
    try {
      setCanjeError(null)

      // 1. Decodificar el QR y validar que el usuario es el logueado
      const decoded = TokenUtils.decodeToken(token)
      const qrUsuarioId =
        decoded?.sub ||
        decoded?.userId ||
        decoded?.nameid ||
        decoded?.id ||
        decoded?.Id ||
        decoded?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]

      if (!qrUsuarioId) {
        setCanjeError('No se pudo obtener el ID de usuario del QR')
        return
      }
      if (String(qrUsuarioId) !== String(usuarioId)) {
        setCanjeError('Este QR no pertenece a tu cuenta')
        return
      }

      // 2. Canjear el beneficio usando el endpoint PATCH
      const canjearResponse = await BeneficioService.canjearBeneficio(
        parseInt(usuarioId, 10),
        id,
        'Punto de Control - App Web'
      )

      if (canjearResponse.status === 200) {
        setCanjeSuccess(true)
        setTimeout(() => {
          router.push('/historialBeneficios')
        }, 2000)
      }
    } catch (err) {
      console.error('Error canjeando beneficio:', err)
      if (err.response?.data?.error) {
        setCanjeError(err.response.data.error)
      } else if (err.response?.status === 404) {
        setCanjeError('Endpoint no encontrado - Verificá que el backend esté corriendo')
      } else {
        setCanjeError('Error al canjear el beneficio')
      }
    }
  }

  const cerrarEscaner = async () => {
    if (html5QrCodeRef.current && isScanning) {
      await html5QrCodeRef.current.stop()
    }
    setShowScanner(false)
    setIsScanning(false)
    setScanResult(null)
    setCanjeError(null)
  }

  // Limpiar al desmontar
  useEffect(() => {
    return () => {
      if (html5QrCodeRef.current) {
        html5QrCodeRef.current.stop().catch(() => {})
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
            ) : beneficio ? (
              <article className="card">
                <header className="card-header">
                  <h1 className="title">Beneficio #{beneficio.Id || beneficio.id}</h1>
                  {(() => {
                    const hoy = new Date();
                    const fechaVenc = new Date(beneficio.FechaDeVencimiento || beneficio.fechaDeVencimiento);
                    const vigente = fechaVenc >= hoy;
                    return (
                      <span className={`badge ${vigente ? 'ok' : 'off'}`}>
                        {vigente ? 'Vigente' : 'No vigente'}
                      </span>
                    );
                  })()}
                </header>

                <div className="meta">
                  <div className="meta-item">
                    <span className="meta-label">Tipo</span>
                    <span className="meta-value">{getTipoTexto(beneficio.Tipo ?? beneficio.tipo)}</span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Fecha de vencimiento</span>
                    <span className="meta-value">
                      {new Date(beneficio.FechaDeVencimiento || beneficio.fechaDeVencimiento).toLocaleDateString('es-ES', { 
                        year:'numeric', 
                        month:'long', 
                        day:'numeric'
                      })}
                    </span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Cupos disponibles</span>
                    <span className="meta-value">{beneficio.Cupos ?? beneficio.cupos}</span>
                  </div>
                </div>

                <div className="body">
                  <p className="hint">Beneficio disponible para canjear según disponibilidad de cupos.</p>
                  <button className="canjear-btn" onClick={iniciarEscaneo}>
                    Canjear Beneficio con QR
                  </button>
                </div>
              </article>
            ) : null}
          </div>
        </div>
      </section>

      {/* Modal del escáner QR */}
      {showScanner && (
        <div className="modal-overlay" onClick={cerrarEscaner}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <button className="close-btn" onClick={cerrarEscaner}>✕</button>
            
            <h2 className="scanner-title">Escanea tu QR de Credenciales</h2>
            <p className="scanner-hint">Escanea el QR que aparece en tu perfil</p>

            {canjeSuccess ? (
              <div className="success-message">
                <div className="success-icon">✓</div>
                <p>¡Beneficio canjeado exitosamente!</p>
                <p className="redirect-text">Redirigiendo al historial...</p>
              </div>
            ) : canjeError ? (
              <div className="error-message">
                <p>{canjeError}</p>
                <button className="retry-btn" onClick={iniciarEscaneo}>Reintentar</button>
              </div>
            ) : isScanning ? (
              <div id="qr-reader-canje" className="qr-reader"></div>
            ) : null}
          </div>
        </div>
      )}

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
        .hint { margin: 0 0 16px 0; font-size: 0.9rem; opacity: 0.85; }

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

        /* Modal del escáner */
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

        .redirect-text {
          font-size: 0.9rem;
          color: rgba(255,255,255,0.6);
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

        /* Skeleton */
        .skeleton { position: relative; overflow: hidden; }
        .skeleton::after { content: ''; position: absolute; inset: 0; background: linear-gradient(90deg, rgba(255,255,255,0) 0%, rgba(255,255,255,0.25) 50%, rgba(255,255,255,0) 100%); animation: shimmer 1.4s infinite; }
        .skeleton-line { height: 16px; border-radius: 8px; background: rgba(255,255,255,0.5); margin: 10px 0; }
        @keyframes shimmer { 0% { transform: translateX(-100%); } 100% { transform: translateX(100%); } }

        /* Error state */
        .card.error { background: #fee2e2; color: #991b1b; border: 2px solid rgba(153,27,27,0.3); }

        @media (max-width: 768px) {
          .hero { padding: 16px; min-height: 60vh; }
          .card { padding: 16px; border-radius: 16px; }
        }
      `}</style>
    </div>
  )
}
