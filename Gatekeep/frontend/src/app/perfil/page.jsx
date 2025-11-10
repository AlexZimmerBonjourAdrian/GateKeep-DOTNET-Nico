"use client"

import React, { useState, useEffect } from 'react'
import Image from 'next/image'
import Link from 'next/link'
import { usePathname } from 'next/navigation'
import logo from '/public/assets/LogoGateKeep.webp'
import harvard from '/public/assets/Harvard.webp'
import BasketballIcon from '/public/assets/basketball-icon.svg'
import UsuarioService from '../../services/UsuarioService'
import { SecurityService } from '../../services/securityService'

export default function Perfil() {
  const pathname = usePathname();
  SecurityService.checkAuthAndRedirect(pathname);

  const [name, setName] = useState('')
  const [dob, setDob] = useState('')
  const [role, setRole] = useState('Usuario')
  const [profileImage, setProfileImage] = useState(null)
  const [preview, setPreview] = useState(null)
  const [qrUrl, setQrUrl] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!profileImage) {
      setPreview(null)
      return
    }

    const objectUrl = URL.createObjectURL(profileImage)
    setPreview(objectUrl)

    return () => URL.revokeObjectURL(objectUrl)
  }, [profileImage])

  // Cargar datos reales del usuario y QR desde el backend
  useEffect(() => {
    let revokedUrl = null
    const load = async () => {
      try {
        const usuario = await UsuarioService.getUsuarioActual({ refresh: true })
        if (usuario) {
          setName(`${usuario.nombre ?? ''} ${usuario.apellido ?? ''}`.trim())
          // rol viene como string por JsonStringEnumConverter
          if (usuario.rol) setRole(usuario.rol)
        }

        // Obtener QR del token actual como blob url
        const url = await UsuarioService.getAuthQrUrl({ width: 220, height: 220 })
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

  const handleImageChange = (e) => {
    const file = e.target.files && e.target.files[0]
    if (file) setProfileImage(file)
  }

  const handleSubmit = (e) => {
    e.preventDefault()
    // Aquí puedes integrar el envío al servidor o contexto de auth
    console.log('Perfil guardado', { name, dob, profileImage })
    alert('Perfil guardado (demo)')
  }

  // El QR ahora proviene del backend (/auth/qr) a través de un blob URL en qrUrl

  return (
    <div className="header-root">
      <div className="header-hero">
        <Image src={harvard} alt="Harvard" fill className="harvard-image" priority />
        <div className="header-overlay" />

      <div className="header-topbar">
        <div className="icon-group">
          <Link href="/">
            <Image src={logo} alt="Logo GateKeep" width={160} priority className="logo-image" />
          </Link>
        </div>      
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
                {preview ? (
                  // Preview seleccionado
                  // eslint-disable-next-line @next/next/no-img-element
                  <img src={preview} alt="Preview" className="preview-img" />
                ) : (
                  <Image src={logo} alt="Avatar" width={120} height={120} className="preview-img" />
                )}
              </div>

              <label className="file-label">
                <input type="file" accept="image/*" onChange={handleImageChange} />
                <span>Seleccionar imagen</span>
              </label>
            </div>

            <div className="fields-section">
              <label className="field">
                <span>Nombre</span>
                <input type="text" value={name} onChange={(e) => setName(e.target.value)} readOnly placeholder="Tu nombre" />
              </label>

              <label className="field">
                <span>Rol</span>
                <input type="text" value={role} readOnly />
              </label>

              <label className="field">
                <span>Fecha de nacimiento</span>
                <input type="date" value={dob} onChange={(e) => setDob(e.target.value)}  readOnly/>
              </label>
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
          </div>
        </form>
      </div>
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
          padding: 24px; /* Valor por defecto para pantallas grandes */
          box-sizing: border-box;
        }

        @media (max-width: 768px) {
          .header-hero {
            padding: 16px; /* Reduce el padding en pantallas medianas */
            height: 600px; /* Ajusta la altura en pantallas medianas */
          }
        }

        @media (max-width: 425px) {
          .header-hero {
            padding: 12px; /* Reduce aún más el padding en pantallas pequeñas */
            height: auto; /* Permite que la altura sea dinámica */
          }
        }

        .harvard-image {
          object-fit: cover;
          position: absolute;
          inset: 0;
          z-index: 0;
        }

        @media (max-width: 425px) {
          .harvard-image {
            display: none; /* Oculta la imagen en pantallas pequeñas */
            box-shadow: none;
          }
        }

        .header-overlay {
          position: absolute;
          inset: 0;
          z-index: 1;
          pointer-events: none;
          box-shadow: inset 0 80px 120px rgba(0, 0, 0, 0.6), inset 0 -80px 120px rgba(0, 0, 0, 0.6);
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

        @media (max-width: 425px) {
          .logo-image {
            width: 120px; /* Ajusta el tamaño del logo en pantallas pequeñas */
          }
        }

        .icon-group {
          display: inline-flex;
          align-items: center;
          gap: 10px;
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

        @media (max-width: 425px) {
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

        @media (max-width: 425px) {
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
      `}</style>
    </div>
  )
}
