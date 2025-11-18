"use client"

import React, { useState, useEffect } from 'react'
import Image from 'next/image'
import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import logo from '/public/assets/LogoGateKeep.webp'
import harvard from '/public/assets/Harvard.webp'
import { SecurityService } from '../../../services/securityService'
import { BeneficioService } from '../../../services/BeneficioService'

export default function CrearBeneficio() {
  const pathname = usePathname()
  const router = useRouter()
  SecurityService.checkAuthAndRedirect(pathname)

  const [isAdmin, setIsAdmin] = useState(false)
  useEffect(() => {
    try {
      const tipo = SecurityService.getTipoUsuario?.() || null
      let admin = false
      if (tipo) {
        admin = /admin|administrador/i.test(String(tipo))
      } else if (typeof window !== 'undefined') {
        const raw = localStorage.getItem('user')
        if (raw) {
          const user = JSON.parse(raw)
          const role = user?.TipoUsuario || user?.tipoUsuario || user?.Rol || user?.rol
          if (role) admin = /admin|administrador/i.test(String(role))
        }
      }
      setIsAdmin(admin)
      if (!admin) router.replace('/')
    } catch {
      setIsAdmin(false)
      router.replace('/')
    }
  }, [router])

  const [tipo, setTipo] = useState(0) // 0=Canje, 1=Consumo
  const [vigencia, setVigencia] = useState(true)
  const [fechaDeVencimiento, setFechaDeVencimiento] = useState('')
  const [cupos, setCupos] = useState(1)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)

  const onSubmit = async (e) => {
    e.preventDefault()
    if (!fechaDeVencimiento || cupos < 1) {
      setError('Fecha de vencimiento y cupos (mínimo 1) son obligatorios')
      return
    }
    
    // Validación: fechaDeVencimiento debe ser futura
    const vencimiento = new Date(fechaDeVencimiento)
    const hoy = new Date()
    if (vencimiento <= hoy) {
      setError('La fecha de vencimiento debe ser posterior a la fecha actual')
      return
    }
    
    setError('')
    setSubmitting(true)
    try {
      const fechaVencimientoIso = new Date(fechaDeVencimiento).toISOString()
      await BeneficioService.crearBeneficio({ 
        tipo, 
        vigencia,
        fechaDeVencimiento: fechaVencimientoIso, 
        cupos
      })
      setSuccess(true)
      setTimeout(() => router.push('/beneficio/listadoBeneficios'), 800)
    } catch (err) {
      console.error('Error creando beneficio', err)
      setError(err.response?.data?.message || err.response?.data?.error || 'No se pudo crear el beneficio. Intenta nuevamente.')
    } finally {
      setSubmitting(false)
    }
  }

  if (!isAdmin) return null

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
          <form className="text-card" onSubmit={onSubmit}>
            <div style={{alignItems: 'center', width: '100%'}}>
              <h1 className="text-3xl font-bold text-white">Crear Beneficio</h1>
              <hr />
            </div>

            <div className='input-container'>
              <div className='w-full'>
                <span>Tipo de Beneficio *</span>
                <select 
                  value={tipo} 
                  onChange={(e) => setTipo(parseInt(e.target.value))}
                  style={{borderRadius:'20px', width:'calc(100% - 2vw)', marginLeft:'1vw', marginRight:'1vw', padding:'8px'}}
                >
                  <option value={0}>Canje</option>
                  <option value={1}>Consumo</option>
                </select>
              </div>

              <div className='w-full'>
                <span>Fecha de Vencimiento *</span>
                <input 
                  type="datetime-local" 
                  value={fechaDeVencimiento} 
                  onChange={(e) => setFechaDeVencimiento(e.target.value)} 
                />
              </div>

              <div className='w-full'>
                <span>Cupos Disponibles *</span>
                <input 
                  type="number" 
                  min="1" 
                  placeholder="Cantidad de cupos" 
                  value={cupos} 
                  onChange={(e) => setCupos(parseInt(e.target.value) || 1)} 
                />
              </div>

              <div className='w-full' style={{display:'flex', alignItems:'center', gap:'8px', marginLeft:'1vw', marginRight:'1vw'}}>
                <input 
                  id="vigencia-beneficio" 
                  type="checkbox" 
                  checked={vigencia} 
                  onChange={(e) => setVigencia(e.target.checked)} 
                />
                <label htmlFor="vigencia-beneficio" style={{margin:0, fontSize:'0.8rem'}}>Vigente</label>
              </div>
            </div>
         
            {error && <p style={{ color: '#ffdddd', background:'#7e1e1e', borderRadius:12, padding:'8px 14px', margin:'10px 1vw', width:'calc(100% - 2vw)', fontSize:'0.85rem' }}>{error}</p>}
            {success && <p style={{ color: '#e9ffe9', background:'#1e7e3a', borderRadius:12, padding:'8px 14px', margin:'10px 1vw', width:'calc(100% - 2vw)', fontSize:'0.85rem' }}>Beneficio creado. Redirigiendo...</p>}
            
            <div className='button-container'>
              <button type="submit" disabled={submitting} style={{ opacity: submitting ? 0.6 : 1, cursor: submitting ? 'not-allowed' : 'pointer' }}>
                {submitting ? 'Creando...' : 'Crear Beneficio'}
              </button>
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
          padding: 24px;
          box-sizing: border-box;
        }

        @media (max-width: 768px) {
          .header-hero {
            padding: 16px;
            height: 600px;
          }
        }

        @media (max-width: 425px) {
          .header-hero {
            padding: 12px;
            height: auto;
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
            display: none;
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
            width: 120px;
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
          text-align: center;
        }

        input, textarea {
          border-radius: 20px;
          width: calc(100% - 2vw);
          margin-left: 1vw;
          margin-right: 1vw;
          margin-top: 0;
          padding: 8px;
        }

        @media (max-width: 425px) {
          input, textarea {
            padding: 6px;
          }
        }

        hr {
          width: 100%;
          border: 1.5px solid #F37426;
        }

        .input-container {
          display: flex;
          flex-direction: column;
          gap: 16px;
          width: 100%;
        }

        .button-container {
          width: 100%;
          display: flex;
          justify-content: center;
          align-items: center;
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
            width: 100%;
            padding: 10px;
          }
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
            width: 90%;
          }
        }

        @media (max-width: 425px) {
          .text-card {
            width: 100%;
          }
        }

        .header-middle-bar {
          position: relative;
          z-index: 2;
          display: flex;
          justify-content: center;
          width: 100%;
        }

      `}</style>
    </div>
  )
}
