"use client"

import React, { useState, useEffect, useRef } from 'react'
import Image from 'next/image'
import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import logo from '/public/assets/LogoGateKeep.webp'
import harvard from '/public/assets/Harvard.webp'
import BasketballIcon from '/public/assets/basketball-icon.svg'
import UsuarioService from '../../services/UsuarioService'
import { SecurityService } from '../../services/securityService'
import { NotificacionService } from '../../services/NotificacionService'

export default function Perfil() {
  const pathname = usePathname();
  const router = useRouter();
  const adminMenuRef = useRef(null);
  
  // Verificación de autenticación en cliente
  useEffect(() => {
    SecurityService.checkAuthAndRedirect(pathname);
  }, [pathname]);

   
  const [name, setName] = useState('')
  const [phone, setPhone] = useState('')
  const [role, setRole] = useState('Usuario')
  const [userId, setUserId] = useState(null)
  const [editMode, setEditMode] = useState(false)
  const [notificaciones, setNotificaciones] = useState(0);
  const [qrUrl, setQrUrl] = useState(null)
  const [loading, setLoading] = useState(true)
  const [isAdmin, setIsAdmin] = useState(false)
  const [adminMenuOpen, setAdminMenuOpen] = useState(false)

  useEffect(() => {
      const fetchNotifications = async () => {
        SecurityService.checkAuthAndRedirect(pathname);
  
        // Si está autenticado, obtener datos del usuario y notificaciones
          const storedUserId = SecurityService.getUserId();
          const tipo = SecurityService.getTipoUsuario?.() || null;
          try {
            let admin = false;
            if (tipo) {
              admin = /admin|administrador/i.test(String(tipo));
            } else {
              // fallback: intentar con el objeto user del localStorage
              const rawUser = typeof window !== 'undefined' ? localStorage.getItem('user') : null;
              if (rawUser) {
                const user = JSON.parse(rawUser);
                const role = user?.TipoUsuario || user?.tipoUsuario || user?.Rol || user?.rol;
                if (role) admin = /admin|administrador/i.test(String(role));
              }
            }
            setIsAdmin(admin);
          } catch {
            setIsAdmin(false);
          }
          
          if (storedUserId) {
            const userId = parseInt(storedUserId, 10);
            if (!isNaN(userId)) {
              // Función para obtener notificaciones con reintentos
              const fetchNotifications = async (retryCount = 0) => {
                try {
                  const count = await NotificacionService.getNoLeidasCount(userId);
                  setNotificaciones(count || 0);
                } catch (error) {
                  console.error('Error al cargar notificaciones:', error);
                  if (retryCount < 2) {
                    // Reintentar después de 500ms
                    setTimeout(() => fetchNotifications(retryCount + 1), 500);
                  } else {
                    setNotificaciones(0);
                  }
                }
              };
              
              fetchNotifications();
            }
          }
        
      };
  
      fetchNotifications();
    }, [pathname, router]);

  // Cargar datos reales del usuario y QR desde el backend
  useEffect(() => {
    let revokedUrl = null
    const load = async () => {
      try {
        const usuario = await UsuarioService.getUsuarioActual({ refresh: true })
        if (usuario) {
         setUserId(usuario.id ?? null)
          setName(`${usuario.nombre ?? ''} ${usuario.apellido ?? ''}`.trim())
          // rol viene como string por JsonStringEnumConverter
          if (usuario.rol) {
            setRole(usuario.rol)
            // Verificar si es admin
            const admin = /admin|administrador/i.test(String(usuario.rol));
            setIsAdmin(admin);
          }
          if (usuario.telefono) setPhone(usuario.telefono)
        }

        // Obtener QR del token actual como blob url
        const url = await UsuarioService.getAuthQrUrl({ width: 400, height: 400 })
        setQrUrl(url)
        revokedUrl = url
      } catch (e) {
        console.error('Error cargando perfil/QR:', e)
      } finally {
        setLoading(false)
      }
    }
    load()
    return () => {
      if (revokedUrl) URL.revokeObjectURL(revokedUrl)
    }
  }, [])

  // Cierre por click fuera del menú admin
  useEffect(() => {
    const handleClickOutside = (e) => {
      if (adminMenuRef.current && !adminMenuRef.current.contains(e.target)) {
        setAdminMenuOpen(false);
      }
    };
    if (adminMenuOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [adminMenuOpen]);

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!editMode) return

    // Dividir el campo nombre en Nombre y Apellido (heurística simple)
    const parts = (name || '').trim().split(/\s+/)
    const first = parts.shift() || ''
    const last = parts.join(' ') || ''

    try {
      const updated = await UsuarioService.updateUsuarioActual({
        nombre: first,
        apellido: last,
        telefono: phone || null,
      })
      // Refrescar UI
      setName(`${updated.nombre ?? ''} ${updated.apellido ?? ''}`.trim())
      setPhone(updated.telefono ?? '')
      setEditMode(false)
    } catch (err) {
      console.error('Error actualizando perfil', err)
      alert('No se pudo actualizar el perfil')
    }
  }

  // El QR ahora proviene del backend (/auth/qr) a través de un blob URL en qrUrl

  return (
    <div className="header-root">
      <div className="header-hero">
        <Image src={harvard} alt="Harvard" fill className="harvard-image" priority />
        <div className="header-overlay" />
        <div className="harvard-placeholder" />

      <div className="header-topbar">
        <div className="icon-group">
          <Link href="/">
            <Image src={logo} alt="Logo GateKeep" width={160} priority className="logo-image" />
          </Link>
          
          <Link href="/notificaciones" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Notificaciones" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
            <div className="item-card notification-card">
              <i className="pi pi-bell item-icon" aria-hidden={true}></i>
                {notificaciones > 0 && (
                  <div className="notification-badge">
                    {notificaciones}
                  </div>
                )}
            </div>
          </Link>
        </div>
        
        <Link href="/" className="btn-volver">
          ← Volver al Inicio
        </Link>
      </div>
            
      <div className="header-middle-bar">
        <form className="text-card" onSubmit={handleSubmit}>
          <div style={{alignItems: 'center', width: '100%'}}>
            <h1 className="text-3xl font-bold text-white">Mi Perfil</h1>
            <hr />
          </div>

          <div className="profile-form">
            <div className="image-section">
              <div className="image-preview">
                <Image src={logo} alt="Avatar" width={120} height={120} className="preview-img" />
              </div>
            </div>

            <div className="fields-section">
              <label className="field">
                <span>Nombre</span>
             <input type="text" value={name} onChange={(e) => setName(e.target.value)} readOnly={!editMode} placeholder="Tu nombre" />
              </label>

              <label className="field">
                <span>Rol</span>
                <input type="text" value={role} readOnly />
              </label>

              <label className="field">
                <span>Teléfono</span>
                  <input type="text" value={phone} onChange={(e) => setPhone(e.target.value)} readOnly={!editMode} placeholder="Tu teléfono" />
              </label>
            <div style={{ display: 'flex', gap: 12, padding: '0 18px 12px', alignItems: 'center' }}>
              {!editMode ? (
                <button type="button" className="save-btn" onClick={() => setEditMode(true)}>Editar</button>
              ) : (
                <>
                  <button type="button" className="save-btn" style={{ background: '#666' }} onClick={() => { setEditMode(false); }}>Cancelar</button>
                  <button type="submit" className="save-btn">Guardar</button>
                </>
              )}
            </div>
            </div>
          </div>
          <div className="qr-section">
            <h3 className="qr-title">Mi QR de acceso</h3>
            <div className="qr-card">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              {qrUrl ? (
                <img src={qrUrl} alt="QR token" className="qr-img" />
              ) : (
                <span className="qr-caption">{loading ? 'Generando QR…' : 'No se pudo generar el QR'}</span>
              )}
            </div>
            <Link href="/perfil/escaner" className="scanner-btn">
              📷 Escanear QR
            </Link>
          </div>
        </form>
      </div>
    </div>

      {/* Barra de navegación inferior solo en móvil */}
      <div className="header-bottom-bar">
        <Link href="/" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Home" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
          <div className="item-card">
            <i className="pi pi-home item-icon" aria-hidden={true}></i>
            <p className="item-text">Home</p>
          </div>
        </Link>

        <Link href="/evento/listadoEventos" style={{ textDecoration: 'none', outline: 'none'}} aria-label="Eventos" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
          <div className="item-card">
            <BasketballIcon style={{ color: '#231F20', width: 30, height: 30 }} />
            <p className="item-text">Eventos</p>
          </div>
        </Link>

        <Link href="/anuncio/listadoAnuncios" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Anuncios" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
          <div className="item-card">
            <i className="pi pi-megaphone item-icon" aria-hidden={true}></i>
            <p className="item-text">Anuncios</p>
          </div>
        </Link>

        {isAdmin && (
          <div className="admin-menu-wrapper" ref={adminMenuRef}>
            <button
              type="button"
              className="item-card admin-trigger"
              aria-haspopup="true"
              aria-expanded={adminMenuOpen}
              aria-label="Recursos administrativos"
              onClick={() => setAdminMenuOpen(o => !o)}
            >
              <i className="pi pi-sliders-h item-icon" aria-hidden={true}></i>
              <p className="item-text">Admin</p>
            </button>
            {adminMenuOpen && (
              <div className="admin-dropdown-mobile" role="menu">
                <button
                  type="button"
                  role="menuitem"
                  tabIndex={0}
                  className="admin-dropdown-item"
                  onClick={() => {
                    router.push('/reglas-acceso/listadoReglasAcceso');
                    setAdminMenuOpen(false);
                  }}
                >
                  <i className="pi pi-sliders-h" aria-hidden={true}></i>
                  <span>Reglas</span>
                </button>
                <button
                  type="button"
                  role="menuitem"
                  tabIndex={0}
                  className="admin-dropdown-item"
                  onClick={() => {
                    router.push('/edificios/listadoEdificios');
                    setAdminMenuOpen(false);
                  }}
                >
                  <i className="pi pi-building" aria-hidden={true}></i>
                  <span>Edificios</span>
                </button>
                <button
                  type="button"
                  role="menuitem"
                  tabIndex={0}
                  className="admin-dropdown-item"
                  onClick={() => {
                    router.push('/salones/listadoSalones');
                    setAdminMenuOpen(false);
                  }}
                >
                  <i className="pi pi-th-large" aria-hidden={true}></i>
                  <span>Salones</span>
                </button>
                <button
                  type="button"
                  role="menuitem"
                  tabIndex={0}
                  className="admin-dropdown-item"
                  onClick={() => {
                    router.push('/usuarios/listadoUsuarios');
                    setAdminMenuOpen(false);
                  }}
                >
                  <i className="pi pi-users" aria-hidden={true}></i>
                  <span>Usuarios</span>
                </button>
              </div>
            )}
          </div>
        )}

        <Link href="/perfil" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Perfil" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
          <div className="item-card">
            <i className="pi pi-user item-icon" aria-hidden={true}></i>
            <p className="item-text">Perfil</p>
          </div>
        </Link>
      </div>

      <style jsx>{`
        .header-root {
          width: 100%;
          display: block;
        }

        .header-hero {
          width: 100%;
          height: 768px;
          position: relative;
          display: flex;
          flex-direction: column;
          gap: 5px;
          padding: 24px;
          box-sizing: border-box;
        }

        @media (max-width: 768px) {
          .header-hero {
            padding: 16px;
            height: 600px;
          }
        }

        @media (max-width: 430px) {
          .header-root {
            padding-bottom: 90px;
          }

          .header-hero {
            padding: 12px;
            height: auto;
            min-height: 200px;
            gap: 8px;
            padding-top: 8px;
            padding-bottom: 8px;
          }
        }

        :global(.harvard-image) {
          object-fit: cover;
          position: absolute;
          inset: 0;
          z-index: 0;
        }

        .harvard-placeholder {
          display: none;
          box-shadow: none;
        }

        @media (max-width: 768px) {
          :global(.harvard-image) {
            display: none !important;
            visibility: hidden !important;
            opacity: 0 !important;
          }
        }

        @media (max-width: 430px) {
          :global(.harvard-image) {
            display: none !important;
            visibility: hidden !important;
            opacity: 0 !important;
            box-shadow: none !important;
          }

          .harvard-placeholder {
            display: block;
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 160px;
            max-height: 45vh;
            width: 100%;
            background-color: #f37426;
            opacity: 0.4;
            z-index: 0;
          }
        }

        .header-overlay {
          position: absolute;
          inset: 0;
          z-index: 1;
          pointer-events: none;
          box-shadow: inset 0 80px 120px rgba(0, 0, 0, 0.6), inset 0 -80px 120px rgba(0, 0, 0, 0.6);
        }

        @media (max-width: 430px) {
          .header-overlay {
            box-shadow: none;
          }
        }

        .header-topbar {
          position: relative;
          z-index: 2;
          display: flex;
          align-items: center;
          justify-content: space-between;
          gap: 10px;
          min-height: 72px;
        }

        .logo-image {
          width: 160px;
          height: auto;
          cursor: pointer;
          opacity: 0.9;
        }

        .item-card {
          height: 56px;
          min-width: 56px; /* ensure tappable size */
          padding: 0 12px; /* allow some horizontal breathing room for icons/text */
          background-color: #F37426;
          border-radius: 20px;
          display: flex;
          align-items: center;
          justify-content: center;
          cursor: pointer;
          opacity: 0.9;
          transition: transform 150ms ease, box-shadow 150ms ease, opacity 150ms ease;
          z-index: 2; /* Ensure item cards are above the bottom bar */
          box-sizing: border-box;
        }
        .item-card:hover {
          transform: translateY(-4px);
          box-shadow: 0 8px 20px rgba(0,0,0,0.12);
        }
        
        .item-card:active {
          transform: translateY(-1px);
        }
        .item-card:focus-visible {
          outline: none;
          box-shadow: 0 0 0 4px rgba(243,116,38,0.16);
        }

        .item-icon {
          display: block;
          font-size: 1.875rem; /* default 30px */
          color: #231F20;
          line-height: 1;
        }

        .notification-card {
          position: relative;
        }

        .notification-badge {
          position: absolute;
          top: -8px;
          right: -8px;
          background-color: #F62D2D;
          color: white;
          border-radius: 50%;
          width: 28px;
          height: 28px;
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 0.875rem; /* 14px */
          font-weight: bold;
          box-sizing: border-box;
        }

        @media (max-width: 430px) {
          .logo-image {
            width: 120px; /* Ajusta el tamaño del logo en pantallas pequeñas */
          }
        }

        .icon-group {
          display: inline-flex;
          align-items: center;
          gap: 10px;
        }

        .btn-volver {
          background: rgba(255, 255, 255, 0.1);
          color: white;
          padding: 10px 20px;
          border-radius: 8px;
          text-decoration: none;
          font-weight: 600;
          transition: all 0.3s;
          border: 2px solid rgba(255, 255, 255, 0.2);
          backdrop-filter: blur(10px);
        }

        .btn-volver:hover {
          background: rgba(255, 255, 255, 0.2);
          border-color: rgba(255, 255, 255, 0.4);
          transform: translateY(-2px);
        }

        @media (max-width: 768px) {
          .btn-volver {
            padding: 8px 16px;
            font-size: 0.9rem;
          }
        }

        span {
          font-size: 0.8rem;
          margin-left: 1vw;
          margin-right: 1vw;
          margin-bottom: 0;
        }

        h1 {
          color: #F37426;
          margin-left: 1vw;
          margin-right: 1vw;
          text-align: center; /* Centra el texto "Log in" */
        }


        hr {
          width: 100%;
          border: 1.5px solid #F37426;
        }

        .container-Subtext {
          display: flex;
          justify-content: center;
          align-items: center;
          width: 100%;
          gap: 14px;
          margin-top: 7px;
          margin-bottom: 10px;
        }

        .text-card {
          display: flex;
          flex-direction: column;
          align-items: flex-start;
          width: 42.97vw;
          height: auto;
          background-color: #231F20;
          opacity: 0.75;
          padding: 0vw;
          border-radius: 20px;
          border: 3px solid #F37426;
        }

        @media (max-width: 768px) {
          .text-card {
            width: 90%; /* Ajusta el ancho en pantallas medianas */
          }
        }

        @media (max-width: 430px) {
          .text-card {
            width: 100%; /* Ocupa todo el ancho en pantallas pequeñas */
          }
        }

        .header-middle-bar {
          position: relative;
          z-index: 2;
          display: flex;
          justify-content: center;
          width: 100%;
        }

        @media (max-width: 430px) {
          .header-middle-bar {
            display: flex;
            align-items: center;
            justify-content: center;
            height: 100%;
          }

          .header-bottom-bar {
            width: 100%;
            height: 80px;
            background-color: #7e4928;
            display: flex;
            justify-content: space-evenly;
            align-items: center;
            position: fixed;
            bottom: 0;
            z-index: 4;
            padding: 7px;
          }

          .header-bottom-bar .item-icon {
            font-size: 2.1rem;
          }

          .header-bottom-bar .item-text {
            font-size: 0.7rem;
            font-weight: 250;
          }

          .header-bottom-bar .item-card {
            width: 18vw;
            height: 70px;
            background-color: #F37426;
            border-radius: 20px;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            cursor: pointer;
            opacity: 0.9;
            transition: transform 150ms ease, box-shadow 150ms ease, opacity 150ms ease;
          }

          .header-bottom-bar .item-card:hover {
            transform: none;
            box-shadow: none;
          }
        }

        @media (min-width: 426px) {
          .header-bottom-bar {
            display: none;
          }
        }

        /* Perfil form styles */
        .profile-form {
          display: flex;
          gap: 20px;
          padding: 18px;
          width: 100%;
          box-sizing: border-box;
        }

        .image-section {
          display: flex;
          flex-direction: column;
          align-items: center;
          gap: 8px;
          padding: 8px;
          min-width: 140px;
        }

        .image-preview {
          width: 120px;
          height: 120px;
          border-radius: 9999px;
          overflow: hidden;
          background: rgba(255,255,255,0.06);
          display: flex;
          align-items: center;
          justify-content: center;
          border: 2px solid rgba(255,255,255,0.06);
        }

        .preview-img {
          width: 100%;
          height: 100%;
          object-fit: cover;
          display: block;
        }

        .file-label input[type="file"] {
          display: none;
        }

        .file-label span {
          cursor: pointer;
          background: #F37426;
          color: #fff;
          padding: 8px 12px;
          border-radius: 8px;
          font-weight: 600;
          display: inline-block;
          margin-top: 8px;
        }

        .fields-section {
          flex: 1;
          display: flex;
          flex-direction: column;
          gap: 12px;
          padding: 0 12px;
        }

        .field {
          display: flex;
          flex-direction: column;
          gap: 5px;
          color: #fff;
        }

        .field input[type="text"], .field input[type="date"] {
          padding: 8px 10px;
          border-radius: 8px;
          border: 1px solid #ddd;
          background: #fff;
          color: #000;
        }

        /* estilos para inputs de solo lectura */
        .field input[readonly] {
          background: #f3f3f3;
          color: #333;
          cursor: default;
        }

        .save-btn {
          background: #F37426;
          color: #fff;
          padding: 10px 14px;
          border-radius: 10px;
          border: none;
          cursor: pointer;
          font-weight: 600;
        }

        @media (max-width: 768px) {
          .profile-form {
            flex-direction: column;
            align-items: center;
          }

          .fields-section {
            width: 100%;
          }
        }

        .qr-section {
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          gap: 8px;
          padding: 12px;
          border-top: 1px solid rgba(255,255,255,0.06);
          margin-top: 10px;
          width: 100%;
          box-sizing: border-box;
          text-align: center;
        }

        .qr-title {
          color: #F37426;
          margin: 0;
        }

        .qr-card {
          width: 230px;
          height: 230px;
          background: #F37426;
          padding: 6px;
          border-radius: 12px;
          display: flex;
          align-items: center;
          justify-content: center;
          margin: 0 auto;
          margin-bottom: 8px;
        }

        .qr-img {
          width: 100%;
          height: 100%;
          object-fit: contain;
          display: block;
        }

        .qr-caption {
          color: #fff;
          font-size: 0.9rem;
          opacity: 0.9;
          text-align: center;
          word-break: break-word;
        }

        .scanner-btn {
          background: #2196f3;
          color: white;
          border: none;
          padding: 12px 24px;
          border-radius: 8px;
          font-size: 1rem;
          font-weight: 600;
          cursor: pointer;
          text-decoration: none;
          display: inline-block;
          transition: background 0.3s;
          margin-top: 10px;
        }

        .scanner-btn:hover {
          background: #1976d2;
        }

        /* Barra de navegación inferior solo en móvil */
        .header-bottom-bar {
          display: none;
        }

        @media (max-width: 430px) {
          .header-bottom-bar {
            width: 100%;
            height: 80px;
            background-color: #7e4928;
            display: flex;
            justify-content: space-evenly;
            align-items: center;
            position: fixed;
            bottom: 0;
            z-index: 4;
            padding: 7px;
          }

          .header-bottom-bar .item-icon {
            font-size: 1.8rem;
            margin: 0;
            color: #231F20;
          }

          .header-bottom-bar .item-text {
            font-size: 0.7rem;
            font-weight: 250;
            margin: 0;
            color: #231F20;
          }

          .header-bottom-bar .item-card {
            width: 18vw;
            height: 70px;
            background-color: #F37426;
            border-radius: 20px;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            cursor: pointer;
            opacity: 0.9;
            transition: transform 150ms ease, box-shadow 150ms ease, opacity 150ms ease;
          }

          .header-bottom-bar .item-card:hover {
            transform: none;
            box-shadow: none;
          }

          .admin-menu-wrapper {
            position: relative;
          }

          .admin-trigger {
            border: none;
            background-color: #F37426;
          }

          .admin-dropdown-mobile {
            position: fixed;
            bottom: 90px;
            left: 50%;
            transform: translateX(-50%);
            background: rgba(255, 255, 255, 0.98);
            padding: 8px;
            border-radius: 16px;
            display: flex;
            flex-direction: column;
            gap: 6px;
            min-width: 200px;
            max-width: 280px;
            box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15), 0 2px 8px rgba(0, 0, 0, 0.1);
            z-index: 9999;
            border: 1px solid rgba(243, 116, 38, 0.2);
            animation: adminMenuFade 200ms ease;
            backdrop-filter: blur(10px);
          }

          .admin-dropdown-item {
            background: #FFFFFF;
            color: #231F20;
            text-decoration: none;
            padding: 12px 16px;
            font-size: 0.95rem;
            outline: none;
            display: flex;
            align-items: center;
            gap: 12px;
            font-weight: 600;
            letter-spacing: 0.3px;
            position: relative;
            border-radius: 12px;
            transition: all 180ms ease;
            border: 1.5px solid transparent;
            box-sizing: border-box;
            cursor: pointer;
            width: 100%;
            text-align: left;
          }

          .admin-dropdown-item i {
            font-size: 1.15rem;
            color: #F37426;
            transition: transform 180ms ease;
          }

          .admin-dropdown-item:hover,
          .admin-dropdown-item:focus-visible {
            background: #F37426;
            color: #FFFFFF;
            border-color: #F37426;
            transform: translateX(4px);
            box-shadow: 0 4px 12px rgba(243, 116, 38, 0.25);
          }

          .admin-dropdown-item:hover i,
          .admin-dropdown-item:focus-visible i {
            color: #FFFFFF;
            transform: scale(1.1);
          }

          .admin-dropdown-item:active {
            transform: translateX(2px) scale(0.98);
            box-shadow: 0 2px 6px rgba(243, 116, 38, 0.2);
          }

          .admin-dropdown-item span {
            flex: 1;
            text-align: left;
            font-weight: 600;
          }

          @keyframes adminMenuFade {
            from {
              opacity: 0;
              transform: translateX(-50%) translateY(-10px);
            }
            to {
              opacity: 1;
              transform: translateX(-50%) translateY(0);
            }
          }
        }
      `}</style>
    </div>
  )
}
