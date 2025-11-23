"use client"

import React, { useEffect, useState } from 'react'
import { useParams, usePathname, useRouter } from 'next/navigation'
import Header from '../../../components/Header'
import { UsuarioService } from '../../../services/UsuarioService'
import { SecurityService } from '../../../services/securityService'

export default function UsuarioDetalle() {
  const params = useParams()
  const router = useRouter()
  const pathname = usePathname()
  const rawId = params?.id
  const id = Number(rawId)

  const [usuario, setUsuario] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    SecurityService.checkAuthAndRedirect(pathname)
  }, [pathname])

  useEffect(() => {
    if (!id || Number.isNaN(id)) {
      setError('ID inválido')
      setLoading(false)
      return
    }
    const fetchUsuario = async () => {
      try {
        const resp = await UsuarioService.getUsuario(id)
        setUsuario(resp.data)
      } catch (e) {
        console.error('Error cargando usuario', e)
        setError('No se encontró el usuario')
      } finally {
        setLoading(false)
      }
    }
    fetchUsuario()
  }, [id])

  const formatearFecha = (fechaStr) => {
    if (!fechaStr) return 'N/A'
    try {
      const fecha = new Date(fechaStr)
      return fecha.toLocaleDateString('es-ES', { 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric' 
      })
    } catch {
      return 'N/A'
    }
  }

  const getRolTexto = (rol) => {
    if (!rol) return 'N/A'
    const rolStr = String(rol)
    return rolStr.charAt(0).toUpperCase() + rolStr.slice(1).toLowerCase()
  }

  const getCredencialTexto = (credencial) => {
    if (!credencial) return 'N/A'
    const credStr = String(credencial)
    return credStr.charAt(0).toUpperCase() + credStr.slice(1).toLowerCase()
  }

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
            ) : usuario ? (
              <article className="card">
                <header className="card-header">
                  <h1 className="title">
                    {`${usuario.Nombre || usuario.nombre || ''} ${usuario.Apellido || usuario.apellido || ''}`.trim() || 'Usuario'}
                  </h1>
                  <span className={`badge ${((usuario.Activo ?? usuario.activo ?? true) ? 'ok' : 'off')}`}>
                    {(usuario.Activo ?? usuario.activo ?? true) ? 'Activo' : 'Inactivo'}
                  </span>
                </header>

                <div className="meta">
                  <div className="meta-item">
                    <span className="meta-label">Email</span>
                    <span className="meta-value">{usuario.Email || usuario.email || 'N/A'}</span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Teléfono</span>
                    <span className="meta-value">{usuario.Telefono || usuario.telefono || 'No especificado'}</span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Rol</span>
                    <span className="meta-value">{getRolTexto(usuario.Rol || usuario.rol)}</span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Credencial</span>
                    <span className="meta-value">{getCredencialTexto(usuario.Credencial || usuario.credencial)}</span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Fecha de Alta</span>
                    <span className="meta-value">
                      {formatearFecha(usuario.FechaAlta || usuario.fechaAlta)}
                    </span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">ID</span>
                    <span className="meta-value">#{usuario.Id || usuario.id}</span>
                  </div>
                </div>

                <div className="body">
                  <p className="hint">Información del usuario registrado en el sistema.</p>
                </div>
              </article>
            ) : null}
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

        .meta { 
          display: flex; 
          gap: 18px; 
          flex-wrap: wrap; 
          margin-bottom: 6px; 
        }
        .meta-item { 
          display: flex; 
          flex-direction: column; 
        }
        .meta-label { 
          font-size: 0.78rem; 
          font-weight: 700; 
          color: rgba(35,31,32,0.8); 
        }
        .meta-value { 
          font-size: 0.95rem; 
          font-weight: 700; 
        }

        .body { margin-top: 8px; }
        .hint { margin: 0; font-size: 0.9rem; opacity: 0.85; }

        /* Skeleton */
        .skeleton { 
          position: relative; 
          overflow: hidden; 
        }
        .skeleton::after { 
          content: ''; 
          position: absolute; 
          inset: 0; 
          background: linear-gradient(90deg, rgba(255,255,255,0) 0%, rgba(255,255,255,0.25) 50%, rgba(255,255,255,0) 100%); 
          animation: shimmer 1.4s infinite; 
        }
        .skeleton-line { 
          height: 16px; 
          border-radius: 8px; 
          background: rgba(255,255,255,0.5); 
          margin: 10px 0; 
        }
        @keyframes shimmer { 
          0% { transform: translateX(-100%); } 
          100% { transform: translateX(100%); } 
        }

        /* Error state */
        .card.error { 
          background: #fee2e2; 
          color: #991b1b; 
          border: 2px solid rgba(153,27,27,0.3); 
        }

        @media (max-width: 768px) {
          .hero { padding: 16px; min-height: 60vh; }
          .card { padding: 16px; border-radius: 16px; }
        }
      `}</style>
    </div>
  )
}

