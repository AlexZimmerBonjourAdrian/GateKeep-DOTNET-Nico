"use client"

import React from 'react'
import Image from 'next/image'
import Link from 'next/link'
import { usePathname } from 'next/navigation'
import logo from '/public/assets/LogoGateKeep.webp'
import harvard from '/public/assets/Harvard.webp'
import BasketballIcon from '/public/assets/basketball-icon.svg'
import { SecurityService } from '@/services/securityService';

export default function crearEvento() {

  const pathname = usePathname();
  const isAuthenticated = SecurityService.checkAuthAndRedirect(pathname);

  if (isAuthenticated){
    
  }


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
        <form className="text-card">
          <div style={{alignItems: 'center', width: '100%'}}>
            <h1 className="text-3xl font-bold text-white">Crear Evento</h1>
            <hr />
          </div>

          <div className='input-container'>
            <div className='w-full'>
              <span>Nombre</span>
              <input type="text" placeholder="Nombre del Evento"  />
            </div>

            <div className='w-full'>
              <span>Fecha</span>
              <input type="date" placeholder="Fecha del Evento"  />
            </div>

            <div className='w-full'>
              <span>Resultado</span>
              <input type="text" placeholder="Resultado del Evento"  />
            </div>

            <div className='w-full'>
              <span>Punto de Control</span>
              <input type="text" placeholder="Punto de Control del Evento"  />
            </div>

          </div>
         
          <div className='button-container'>
            <button>Crear Evento</button>
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

        input {
          border-radius: 20px;
          width: calc(100% - 2vw);
          margin-left: 1vw;
          margin-right: 1vw;
          margin-top: 0;
          padding: 8px;
        }

        @media (max-width: 425px) {
          input {
            padding: 6px; /* Reduce el padding en pantallas pequeñas */
          }
        }

        hr {
          width: 100%;
          border: 1.5px solid #F37426;
        }

        .input-container {
          display: flex; /* Flexbox para organizar los inputs */
          flex-direction: column; /* Coloca los inputs en columna */
          gap: 16px; /* Espaciado entre los inputs */
          width: 100%; /* Asegura que ocupe todo el ancho */
        }

        .button-container {
          width: 100%;
          display: flex;
          justify-content: center;
          align-items: center; /* Centra el botón */
        }

        button {
          margin-top: 30px;
          border-radius: 20px;
          width: calc(80% - 2vw);
          padding: 8px;
          background: #F37426;
          margin-bottom: 20px;
        }

        @media (max-width: 425px) {
          button {
            width: 100%; /* Botón ocupa todo el ancho en pantallas pequeñas */
            padding: 10px; /* Ajusta el padding */
          }
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
      `}</style>
    </div>
  )
}
