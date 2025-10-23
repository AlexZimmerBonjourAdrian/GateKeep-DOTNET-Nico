"use client"

import React from 'react'
import Image from 'next/image'
import Link from 'next/link'
import logo from '../../src/assets/LogoGateKeep.webp'
import harvard from '../../src/assets/Harvard.webp'
import BasketballIcon from '../../src/assets/basketball-icon.svg'

export default function Header() {
  const notificaciones = [
    { id: 1 }
  ]

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

            <Link href="/notificaciones" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Notificaciones" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
              <div className="item-card notification-card">
                <i className="pi pi-bell item-icon" aria-hidden={true}></i>
                {notificaciones.length > 0 && (
                  <div className="notification-badge">
                    {notificaciones.length}
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

            <Link href="/eventos" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Eventos" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
              <div className="item-card">
                <BasketballIcon style={{ color: '#231F20', width: 30, height: 30 }} />
              </div>
            </Link>

            <Link href="/anuncios" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Anuncios" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
              <div className="item-card">
                <i className="pi pi-megaphone item-icon" aria-hidden={true}></i>
              </div>
            </Link>

            <Link href="/perfil" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Perfil" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
              <div className="item-card">
                <i className="pi pi-user item-icon" aria-hidden={true}></i>
              </div>
            </Link>

            <Link href="/" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Salir" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
              <div className="item-card">
                <i className="pi pi-sign-out item-icon" aria-hidden={true}></i>
              </div>
            </Link>
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
          <div className="item-card" style={{width:"225px", height:"230px"}}>
            <i className="pi pi-home item-icon" aria-hidden={true} style={{fontSize:"140px"}}></i>
            <p className="item-text">Home</p>
          </div>
        </Link>

        <Link href="/eventos" style={{ textDecoration: 'none', outline: 'none'}} aria-label="Eventos" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
          <div className="item-card" style={{width:"225px", height:"230px"}}>
            <BasketballIcon style={{ color: '#231F20', width: 140, height: 140 }} />
            <p className="item-text">Eventos</p>
          </div>
        </Link>

        <Link href="/anuncios" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Anuncios" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
          <div className="item-card" style={{width:"225px", height:"230px"}}>
            <i className="pi pi-megaphone item-icon" aria-hidden={true} style={{fontSize:"140px"}}></i>
            <p className="item-text">Anuncios</p>
          </div>
        </Link>

        <Link href="/perfil" style={{ textDecoration: 'none', outline: 'none' }} aria-label="Perfil" onFocus={(e) => e.currentTarget.style.outline = 'none'}>
          <div className="item-card" style={{width:"225px", height:"230px"}}>
            <i className="pi pi-user item-icon" aria-hidden={true} style={{fontSize:"140px"}}></i>
            <p className="item-text">Perfil</p>
          </div>
        </Link>
      </div>

      <style jsx>{`
        .header-root {
          width: 100%;
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
        .harvard-image {
          object-fit: cover;
          position: absolute;
          inset: 0;
          z-index: 0;
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
          width: 4vw; /* 56px to vw assuming 1920px width */
          background-color: #F37426;
          border-radius: 20px;
          display: flex;
          align-items: center;
          justify-content: center;
          cursor: pointer;
          opacity: 0.9;
          transition: transform 150ms ease, box-shadow 150ms ease, opacity 150ms ease;
          z-index: 2; /* Ensure item cards are above the bottom bar */
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
          font-size: 1.875rem; /* 30px to rem */
          color: #231F20;
          line-height: 1;
        }

        .notification-card {
          position: relative;
        }

        .notification-badge {
          position: absolute;
          top: -8px;
          right: -0.42vw; /* 8px to vw assuming 1920px width */
          background-color: #F62D2D;
          color: white;
          border-radius: 50%;
          width: 1.46vw; /* 28px to vw assuming 1920px width */
          height: 1.46vw; /* 28px to vw assuming 1920px width */
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 0.875rem; /* 14px to rem */
          font-weight: bold;
        }

        .text-card {
            display: flex;
            flex-direction: column;
            align-items: flex-start;
            width: 42.97vw; /* 825px to vw assuming 1920px width */
            height: 246px;
            background-color: #231F20;
            opacity: 0.9;
            padding: 0.52vw; /* 10px to vw assuming 1920px width */
            border-radius: 20px;
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
            .header-bottom-bar{
                width: 100%;
                height: auto; /* Adjust height dynamically */
                min-height: 230px; /* Ensure enough space for the cards */
                background-color: #7e4928;
                display: flex;
                justify-content: space-evenly; /* Ensures equal spacing between items */
                align-items: center;
                position: fixed; /* Make the bottom bar fixed */
                bottom: 0; /* Stick to the bottom of the viewport */
                z-index: 4; /* Place the bottom bar above other elements */
                padding: 40px 0; /* Add padding for better spacing */
            }

            .bottom-bar-item-card {
                width: 11.67vw; /* 224px to vw assuming 1920px width */
                height: 200px; /* Adjust height to fit within the bar */
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
          .header-bottom-bar{
                display: none;
            }

            .bottom-bar-item-card {
                display: none;
            }

            .header-bottom-bar .item-card:hover {
                display: none;
            }
        }

        @media (min-width: 426px) and (max-width: 768px) {
          
        }
      `}</style>
    </div>
  )
}
