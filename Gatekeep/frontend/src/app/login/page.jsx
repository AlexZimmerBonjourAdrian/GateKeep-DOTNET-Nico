"use client"

export const dynamic = 'force-dynamic'

import React, { useState, useEffect } from 'react'
import Image from 'next/image'
import Link from 'next/link'
import logo from '/public/assets/LogoGateKeep.webp'
import harvard from '/public/assets/Harvard.webp'
import BasketballIcon from '/public/assets/basketball-icon.svg'
import { UsuarioService } from '../..//services/UsuarioService'
import { SecurityService } from '../..//services/securityService'

export default function Login() {
  
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')


  const handleSubmit = (e) => {
    e.preventDefault()
    
    console.log('Intentando login con:', { email, password: '***' });
    
    UsuarioService.login({ email, password })
      .then(response => {
        console.log('Login successful:', response.data);
        
        // Guardar datos de autenticación usando SecurityService
        if (response.data.isSuccess && response.data.user) {
          const user = response.data.user;
          const normalizedId = user.id ?? user.Id;
          const normalizedTipo = user.tipoUsuario ?? user.TipoUsuario ?? user.rol ?? user.Rol;
          // Persistir datos clave
          SecurityService.saveAuthData(
            normalizedId,
            normalizedTipo,
            response.data.token,
            response.data.refreshToken
          );
          // Guardar objeto de usuario completo también
          try {
            localStorage.setItem('user', JSON.stringify(user));
          } catch {}
          
          // Redirigir al usuario después del login exitoso
          window.location.href = '/';
        } else {
          console.error('Respuesta del servidor sin éxito:', response.data);
          alert('Error en el login: ' + (response.data.message || 'Respuesta inválida del servidor'));
        }
      })
      .catch(error => {
        console.error('Login failed:', error);
        
        // Mostrar información más detallada del error
        if (error.response) {
          // El servidor respondió con un código de error
          console.error('Error response:', error.response.data);
          console.error('Error status:', error.response.status);
          alert(`Error en el login: ${error.response.data?.message || error.response.statusText || 'Error del servidor'}`);
        } else if (error.request) {
          // La petición se hizo pero no hubo respuesta
          console.error('No response received:', error.request);
          // Construir la URL correcta del backend
          let apiUrl = process.env.NEXT_PUBLIC_API_URL;
          if (!apiUrl && typeof window !== 'undefined' && window.location?.origin) {
            const origin = window.location.origin;
            if (origin.startsWith('https://') && !origin.includes('localhost')) {
              const domain = origin.replace(/^https?:\/\/(www\.)?/, '');
              apiUrl = `https://api.${domain}`;
            } else {
              apiUrl = origin;
            }
          }
          apiUrl = apiUrl || (process.env.NODE_ENV === 'production' 
            ? 'https://api.zimmzimmgames.com'
            : 'http://localhost:5011');
          alert(`Error: No se pudo conectar con el servidor. Verifica que el backend esté corriendo en ${apiUrl}`);
        } else {
          // Algo pasó al configurar la petición
          console.error('Error setting up request:', error.message);
          alert('Error al procesar la petición: ' + error.message);
        }
      });
  }

  return (
    <div className="header-root">
      <div className="header-hero">
      <Image src={harvard} alt="Harvard" fill className="harvard-image" priority />
        <div className="header-overlay" />
        <div className="harvard-placeholder" />
          <div className="header-topbar">
            <div className="icon-group">
              <Link href="/login">
                <Image src={logo} alt="Logo GateKeep" width={160} priority className="logo-image" />
              </Link>
            </div>

          
        </div>
        
        <div className="header-middle-bar">
          <form className="text-card" onSubmit={handleSubmit}>
          <div style={{alignItems: 'center', width: '100%'}}>
            <h1 className="text-3xl font-bold text-white">Login</h1>
            <hr />
          </div>

          <div className="profile-form">
            

              <div className="fields-section">
                

                <label className="field">
                  <span>Correo</span>
                  <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Correo electrónico" />
                </label>

                <label className="field">
                  <span>Contraseña</span>
                  <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Contraseña" />
                </label>
                
              </div>
          </div>

          <div style={{ width: '100%', display: 'flex', justifyContent: 'center', marginTop: 8 }}>
            <button type="submit" className="save-btn">Login</button>
          </div>
          <div style={{ width: '100%', display: 'flex', justifyContent: 'center', marginTop: 12 }}>
            <span style={{ color: '#eee', fontSize: 14 }}>
              ¿No tienes cuenta?{' '}
              <Link href="/register" style={{ color: '#ffd7c2', textDecoration: 'underline' }}>Regístrate</Link>
            </span>
          </div>
         
        </form>
        </div>
      </div>

      
    
      <style jsx>{`

        h1 {
          margin: 12px 0;
        }

        /* Global layout fixes to avoid viewport-width shifts when scrollbars appear/disappear
           and to ensure consistent box-sizing. */
        :global(html), :global(body) {
          box-sizing: border-box;
          -webkit-font-smoothing: antialiased;
          -moz-osx-font-smoothing: grayscale;
          /* Reserve vertical scrollbar space to avoid horizontal layout shifts */
          overflow-y: scroll;
        }

        :global(*), :global(*::before), :global(*::after) {
          box-sizing: inherit;
        }
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
          gap: 80px; 
          padding: 24px; 
          box-sizing: border-box;
        }

        /* next/image fill will position itself; force object-fit via class */
        :global(.harvard-image) {
          object-fit: cover;
          position: absolute;
          inset: 0;
          z-index: 0;
        }

        .harvard-placeholder {
          display: none; /* por defecto oculto, sólo se muestra en mobile */
          box-shadow: none;
        }

        .header-overlay {
          position: absolute;
          inset: 0;
          z-index: 1;
          pointer-events: none;
          box-shadow: inset 0 80px 120px rgba(0,0,0,0.6), inset 0 -80px 120px rgba(0,0,0,0.6);
        }

        .header-topbar {
          position: relative;
          z-index: 2; /* above image/overlay */
          display: flex;
          align-items: center;
          justify-content: space-between; /* first group left, second group right */
          gap: 10px;
          min-height: 72px; /* reserve enough height for the topbar */
          margin-bottom: 12px;
        }
        .logo-image {
          width: 160px;
          height: auto;
          cursor: pointer;
          opacity: 0.9;
        }

        .header-middle-bar {
          position: relative;
          z-index: 2; /* above image/overlay */
          display: flex;
          justify-content: flex-start;
        }

        @media (max-width: 425px) {

          .header-root {
            padding-bottom: 90px;
          }

          :global(.harvard-image) {
            display: none !important;
            box-shadow: none !important;
          }

          /* Make header more compact on very small screens so content below rises */
          .header-hero {
            height: auto;      /* allow content to define height */
            min-height: 200px; /* ensure some visual area */
            gap: 8px;
            padding-top: 8px;
            padding-bottom: 8px;
          }

          /* Mostrar el placeholder blanco con la mitad de la altura del header-hero */
          /* smaller fixed placeholder so page content is closer */
          .harvard-placeholder {
            display: block;
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 160px; /* fixed height to reduce vertical space */
            max-height: 45vh;
            width: 100%;
            background-color: #f37426;
            opacity: 0.4;
            z-index: 2;
          }

          .header-overlay {
            box-shadow: none;         
          }

            .header-middle-bar {
              /* Keep the middle bar visible on small screens and center content
                 so the form remains inside the harvard image height. */
              display: flex;
              align-items: center;
              justify-content: center;
              height: 100%;
              box-shadow: none !important;
            }

           

           
           
      
                :global(.harvard-image) {
                  display: none;
                  box-shadow: none;
                }
        }

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
            /* Keep a minimum height so the absolute harvard image keeps space
               and the form can stay visually inside it on narrow screens. */
            min-height: 420px;
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


        span {
          font-size: 0.8rem;
          margin-left: 1vw;
          margin-right: 1vw;
          margin-bottom: 0;
        }

        h1 {
          color: #f37426;
          margin-left: 1vw;
          margin-right: 1vw;
          text-align: center; /* Centra el texto "Log in" */
        }


        hr {
          width: 100%;
          border: 1.5px solid #f37426;
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

        


        .header-middle-bar {
          position: relative;
          z-index: 2;
          display: flex;
          justify-content: center;
          width: 100%;
        }

        /* Perfil form styles */
        .profile-form {
          display: flex;
          gap: 20px;
          padding: 8px 18px;
          width: 100%;
          box-sizing: border-box;
        }

        .preview-img {
          width: 100%;
          height: 100%;
          object-fit: cover;
          display: block;
        }

        .file-label input[type="file"] {
          display: none;
          box-shadow: none;
        }

        .file-label span {
          cursor: pointer;
          background: #f37426;
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

        /* two-column rows inside fields-section */
        .fields-section .row {
          display: flex;
          gap: 12px;
        }

        .fields-section .row .field {
          flex: 1;
        }

        @media (max-width: 425px) {
          .fields-section .row {
            flex-direction: column;
          }
        }

        .field {
          display: flex;
          flex-direction: column;
          gap: 5px;
          color: #fff;
        }

        .field input[type="text"], .field input[type="email"], .field input[type="date"], .field input[type="password"], .field select {
          display: block;
          width: 100%;
          box-sizing: border-box;
          padding: 8px 12px;
          border-radius: 8px;
          border: 1px solid #ddd;
          background: #fff;
          color: #000;
          font-size: 1rem;
          line-height: 1.2;
        }

        /* Remove native arrow on some browsers and keep consistent padding for custom arrows */
        .field select {
          appearance: none;
          -webkit-appearance: none;
          -moz-appearance: none;
          padding-right: 36px; /* room for dropdown arrow if added */
          background-image: linear-gradient(45deg, transparent 50%, #231F20 50%), linear-gradient(135deg, #231F20 50%, transparent 50%);
          background-position: calc(100% - 16px) calc(50% - 6px), calc(100% - 10px) calc(50% - 6px);
          background-size: 6px 6px, 6px 6px;
          background-repeat: no-repeat;
        }

        .field input:focus, .field select:focus {
          outline: none;
          box-shadow: 0 0 0 4px rgba(243,116,38,0.12);
          border-color: #f37426;
        }

        /* add a small wrapper arrow for select via background image in future if desired */

        /* estilos para inputs de solo lectura */
        .field input[readonly] {
          background: #f3f3f3;
          color: #333;
          cursor: default;
        }

        .save-btn {
          background: #f37426;
          color: #fff;
          padding: 10px 18px;
          border-radius: 10px;
          border: none;
          cursor: pointer;
          font-weight: 700;
          min-width: 160px;
          max-width: 320px;
          width: auto;
          height: 40px;
          display: inline-flex;
          align-items: center;
          justify-content: center;
          transition: transform 120ms ease, box-shadow 120ms ease, opacity 120ms ease;
          margin-bottom: 10px;
          }

        .save-btn:hover {
          transform: translateY(-3px);
          box-shadow: 0 8px 20px rgba(0,0,0,0.12);
          opacity: 0.98;
        }

        .save-btn:active {
          transform: translateY(-1px);
        }

        .save-btn:focus-visible {
          outline: none;
          box-shadow: 0 0 0 4px rgba(243,116,38,0.16);
        }

        /* Responsive: make button full-width on small viewports */
        @media (max-width: 425px) {
          .save-btn {
            width: 100%;
            max-width: none;
            min-width: 0;
          }
        }

        /* Divider row with centered white dot */
        .divider-row {
          display: flex;
          align-items: center;
          gap: 12px;
          width: 100%;
          margin: 8px 0;
        }

        p{
          margin-top: 2px;
          margin-bottom: 2px;
        }

        .pi-circle {
          padding-top: 2px;
        }

        .auth-redirect {
          display: flex;
          justify-content: center;
          align-items: center;
          gap: 8px;
          color: #fff;
          font-size: 0.95rem;
          margin-bottom: 8px;
        }

        .auth-redirect a {
          text-decoration: none;
        }

        .auth-redirect {
          display: flex;
          justify-content: center;
          align-items: center;
          gap: 2px;
          color: #fff;
          font-size: 0.95rem;
          margin-bottom: 8px;
          width: 100%;
          align-items: center;
        }

        .auth-redirect a {
          text-decoration: none;
        }

        .text-card {
          display: flex;
          flex-direction: column;
          align-items: flex-start;
          /* Increase width by 100px while keeping responsive vw behavior */
          width: calc(42.97vw + 100px); /* base VW plus 100px */
          max-width: 980px; /* increased 100px from 880px */
          min-width: 280px;
          background-color: #231F20;
          opacity: 0.9;
          padding: 0.52vw; /* 10px to vw assuming 1920px width */
          border-radius: 20px;
          box-sizing: border-box;
          border: 2px solid #f37426;
        }

        /* Mobile: stack the form like the rest of the page (<=425px) */
        @media (max-width: 425px) {
          .text-card {
            width: calc(100% - 32px);
            max-width: none;
            padding: 12px;
            border-radius: 12px;
            /* Ensure the form never grows taller than the header hero image
               and becomes scrollable instead. This keeps it visually inside. */
            max-height: calc(100% - 24px);
            overflow: auto;
          }

          .profile-form {
            flex-direction: column;
            align-items: center;
            gap: 12px;
          }

          .fields-section {
            width: 100%;
          }
        }

        /* Tablet: keep a compact row layout and adjust widths (426px - 768px) */
        @media (min-width: 426px) and (max-width: 768px) {
          .text-card {
            width: 70%;
            max-width: 820px; /* increased 100px from 720px */
            padding: 14px;
            border-radius: 16px;
          }

          .profile-form {
            flex-direction: row;
            align-items: flex-start;
            gap: 12px;
            
          }

          .fields-section {
            flex: 1;
            width: auto;
          }
        }

        
      `}</style>
    </div>
  )
}
