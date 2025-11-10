"use client"

import React, { useEffect, useState } from 'react'
import Image from 'next/image'
import Link from 'next/link'
import { useRouter, usePathname } from 'next/navigation'
import logo from '/public/assets/LogoGateKeep.webp'
import harvard from '/public/assets/Harvard.webp'
import BasketballIcon from '/public/assets/basketball-icon.svg'
import { UsuarioService } from '@/services/UsuarioService'
import { SecurityService } from '../services/securityService'

export default function Header() {
  const router = useRouter();
  const pathname = usePathname();
  const [notificaciones, setNotificaciones] = useState(0);

  useEffect(() => {
 
    const isAuthenticated = SecurityService.checkAuthAndRedirect(pathname);

    // Si está autenticado, obtener datos del usuario y notificaciones
    if (isAuthenticated) {
      const storedUserId = SecurityService.getUserId();
      
      if (storedUserId) {
        const notifs = UsuarioService.getNotificacionesSinLeer(storedUserId);
        setNotificaciones(notifs);
      }
    }
  }, [pathname, router]);

  const handleLogout = (e) => {
    e.preventDefault();
    SecurityService.logout();
  };

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

          <div className="icon-group">
            <Link href="/" style={{ textDecoration: 'none', outline: 'none'}} aria-label="Home" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
              <div className="item-card">
                <i className="pi pi-home item-icon" aria-hidden={true}></i>
              </div>
            </Link>

            <Link href="/evento/listadoEventos" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Eventos" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
              <div className="item-card">
                <BasketballIcon style={{ color: '#231F20', width: 30, height: 30 }} />
              </div>
            </Link>

            <Link href="/anuncio/listadoAnuncios" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Anuncios" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
              <div className="item-card">
                <i className="pi pi-megaphone item-icon" aria-hidden={true}></i>
              </div>
            </Link>

            <Link href="/perfil" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Perfil" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
              <div className="item-card">
                <i className="pi pi-user item-icon" aria-hidden={true}></i>
              </div>
            </Link>

            <div 
              onClick={handleLogout} 
              style={{ textDecoration: 'none', outline: 'none', cursor: 'pointer' }} 
              aria-label="Salir" 
              onFocus={(e) => e.currentTarget.style.outline = 'none'}
              role="button"
              tabIndex={0}
              onKeyPress={(e) => { if (e.key === 'Enter' || e.key === ' ') handleLogout(e); }}
            >
              <div className="item-card logout-card">
                <i className="pi pi-sign-out item-icon" aria-hidden={true}></i>
              </div>
            </div>
          </div>
        </div>
        
        <div className="header-middle-bar">
          <div className="text-card">
              <h1 className="text-3xl font-bold text-white">Bienvenido a GateKeep</h1>
              <p className="text-lg text-white mt-2">Tu sistema de gestión integral</p>
          </div>
        </div>
      </div>

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

        <Link href="/perfil" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Perfil" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
          <div className="item-card">
            <i className="pi pi-user item-icon" aria-hidden={true}></i>
            <p className="item-text">Perfil</p>
          </div>
        </Link>
      </div>

      <style jsx>{`
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
        }
        .logo-image {
          width: 160px;
          height: auto;
          cursor: pointer;
          opacity: 0.9;
        }
        .icon-group {
          display: inline-flex;
          align-items: center;
          gap: 10px;
          
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

    .text-card {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      width: 42.97vw; /* 825px to vw assuming 1920px width */
      max-width: 900px;
      min-width: 280px;
      height: 246px;
      background-color: #231F20;
      opacity: 0.9;
      padding: 0.52vw; /* 10px to vw assuming 1920px width */
      border-radius: 20px;
      box-sizing: border-box;
    }

        .header-middle-bar {
          position: relative;
          z-index: 2; /* above image/overlay */
          display: flex;
          justify-content: flex-start;
        }

        .item-card {
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
        }

        .item-text {
          margin-top: 8px;
          font-size: 1.2rem; /* 16px to rem */
          font-weight: bold;
          color: white;
          text-align: center;
          width: 100%;
        }

    @media (max-width: 425px) {
            .header-bottom-bar {
                width: 100%;
                height: 80px; 
                background-color: #7e4928;
                display: flex;
                justify-content: space-evenly; /* Ensures equal spacing between items */
                align-items: center;
                position: fixed; /* Make the bottom bar fixed */
                bottom: 0; /* Stick to the bottom of the viewport */
                z-index: 4; /* Place the bottom bar above other elements */
                padding: 7px; /* Add padding for better spacing */
            }

            /* Ocultar los botones del topbar en pantallas <= 425px
               Mantener visibles las notificaciones (.notification-card) y logout (.logout-card) */
            .header-topbar .item-card:not(.notification-card):not(.logout-card) {
              display: none;
            }

            .header-bottom-bar .item-icon {
                font-size: 2.1rem;
            }

            :global(.harvard-image) {
              display: none !important;
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
              background-color: #F37426;
              opacity: 0.4;
              z-index: 2;
            }

            .header-overlay {
              box-shadow: none;         
            }

            .header-middle-bar {
              display: none; 
            }

            .header-bottom-bar .item-text {
                font-size: 0.7rem; /* 12px to rem */
                font-weight: 250; /* Cambiado a un peso de fuente más fino */
            }

            .header-bottom-bar .item-card {
              /* Use a clamped width so items don't reflow abruptly on very narrow viewports
              (original designs used fixed px or vw which caused shifts under ~345px). */
              width: clamp(56px, 18vw, 70px);
              height: 64px;
              background-color: #F37426;
              border-radius: 16px;
              display: flex;
              flex-direction: column;
              align-items: center;
              justify-content: center;
              cursor: pointer;
              opacity: 0.95;
              transition: transform 150ms ease, box-shadow 150ms ease, opacity 150ms ease;
              padding: 6px;
              box-sizing: border-box;
            }

            /* Responsive tweaks for icons and labels in the bottom bar */
            .header-bottom-bar .item-icon {
              font-size: 1.6rem; /* default for mobile */
              line-height: 1;
            }

            .header-bottom-bar .item-card svg {
              width: 28px;
              height: 28px;
            }

            .header-bottom-bar .item-text {
              font-size: 0.7rem;
              margin-top: 6px;
            }

            .header-bottom-bar .item-card:hover {
                transform: none;
                box-shadow: none;
            }

            /* Very small screens: make buttons slightly smaller */
            @media (max-width: 360px) {
              .header-bottom-bar .item-card {
                    width: 56px;
                    height: 56px;
                    border-radius: 14px;
                    padding: 4px;
                  }

                  .header-bottom-bar .item-icon {
                    font-size: 1.4rem;
                  }

                  .header-bottom-bar .item-card svg {
                    width: 24px;
                    height: 24px;
                  }

                  .header-bottom-bar .item-text {
                    font-size: 0.65rem;
                    margin-top: 4px;
                  }
                }
                :global(.harvard-image) {
                  display: none;
                  box-shadow: none;
                }
    }

    /* Tablet/layout tweaks: 426px - 768px */
    @media (min-width: 426px) and (max-width: 768px) {
      .item-card {
        height: 52px;
        min-width: 52px;
        padding: 0 10px;
        border-radius: 16px;
      }

      .item-icon {
        font-size: 1.6rem; /* slightly smaller on tablet */
      }

      .notification-badge {
        width: 24px;
        height: 24px;
        font-size: 0.75rem;
        top: -6px;
        right: -6px;
      }

      .header-bottom-bar{
        display: none; /* tablet uses topbar */
      }
    }

    @media (min-width: 769px) {
      /* Desktop: keep original large spacing but avoid vw-based widths */
      .item-card {
        height: 56px;
        min-width: 56px;
        padding: 0 14px;
      }
      
      .header-bottom-bar{
        display: none; /* tablet uses topbar */
      }
    }
      `}</style>
    </div>
  )
}
