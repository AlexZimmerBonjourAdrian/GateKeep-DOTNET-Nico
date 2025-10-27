"use client"

import React from 'react'
import Image from 'next/image'
import Link from 'next/link'
import logo from '/public/assets/LogoGateKeep.webp'
import harvard from '/public/assets/Harvard.webp'
import BasketballIcon from '/public/assets/basketball-icon.svg'

export default function Login() {

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
        <div className="text-card">
          <h1 className="text-3xl font-bold text-white">Bienvenido a GateKeep</h1>
          <p className="text-lg text-white mt-2">Tu sistema de gestión integral</p>
        </div>
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
          gap: 80px; 
          padding: 24px; 
          box-sizing: border-box;
        }

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

        .icon-group {
          display: inline-flex;
          align-items: center;
          gap: 10px;
        }

        .text-card {
          display: flex;
          flex-direction: column;
          align-items: flex-start;
          width: 42.97vw;
          height: 246px;
          background-color: #231F20;
          opacity: 0.9;
          padding: 0.52vw;
          border-radius: 20px;
        }

        .header-middle-bar {
          position: relative;
          z-index: 2;
          display: flex;
          justify-content: flex-start;
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

          .harvard-image {
            display: none;
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
