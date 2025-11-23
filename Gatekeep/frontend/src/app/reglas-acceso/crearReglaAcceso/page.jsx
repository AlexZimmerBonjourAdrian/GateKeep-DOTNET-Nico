"use client"

import React, { useState, useEffect } from 'react'
import Image from 'next/image'
import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import logo from '/public/assets/LogoGateKeep.webp'
import harvard from '/public/assets/Harvard.webp'
import { SecurityService } from '../../../services/securityService'
import { ReglaAccesoService } from '../../../services/ReglaAccesoService'

export default function crearReglaAcceso() {

  const pathname = usePathname();
  const router = useRouter();
  SecurityService.checkAuthAndRedirect(pathname);

  const [isAdmin, setIsAdmin] = useState(false);
  useEffect(() => {
    try {
      const tipo = SecurityService.getTipoUsuario?.() || null;
      let admin = false;
      if (tipo) {
        admin = /admin|administrador/i.test(String(tipo));
      } else if (typeof window !== 'undefined') {
        const raw = localStorage.getItem('user');
        if (raw) {
          const user = JSON.parse(raw);
          const role = user?.TipoUsuario || user?.tipoUsuario || user?.Rol || user?.rol;
          if (role) admin = /admin|administrador/i.test(String(role));
        }
      }
      setIsAdmin(admin);
      if (!admin) router.replace('/');
    } catch {
      setIsAdmin(false);
      router.replace('/');
    }
  }, [router]);

  const [horarioApertura, setHorarioApertura] = useState('');
  const [horarioCierre, setHorarioCierre] = useState('');
  const [vigenciaApertura, setVigenciaApertura] = useState('');
  const [vigenciaCierre, setVigenciaCierre] = useState('');
  const [espacioId, setEspacioId] = useState('');
  const [rolesPermitidos, setRolesPermitidos] = useState({
    Estudiante: false,
    Funcionario: false,
    Admin: false
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);

  const handleRoleChange = (role) => {
    setRolesPermitidos(prev => ({
      ...prev,
      [role]: !prev[role]
    }));
  };

  const handleSubmit = async () => {
    setError(null);
    setSuccess(false);
    // Validaciones básicas
    if (!espacioId || !horarioApertura || !horarioCierre || !vigenciaApertura || !vigenciaCierre) {
      setError('Completa todos los campos obligatorios.');
      return;
    }
    const rolesSeleccionados = Object.entries(rolesPermitidos)
      .filter(([, v]) => v)
      .map(([k]) => k);
    if (rolesSeleccionados.length === 0) {
      setError('Selecciona al menos un rol permitido.');
      return;
    }

    // El backend espera DateTime en formato ISO para horarios
    // Combinamos una fecha base con las horas seleccionadas
    const hoy = new Date().toISOString().split('T')[0];
    const payload = {
      horarioApertura: `${hoy}T${horarioApertura}:00.000Z`,
      horarioCierre: `${hoy}T${horarioCierre}:00.000Z`,
      vigenciaApertura: `${vigenciaApertura}T00:00:00.000Z`,
      vigenciaCierre: `${vigenciaCierre}T23:59:59.000Z`,
      rolesPermitidos: rolesSeleccionados,
      espacioId: Number(espacioId),
    };

    setSubmitting(true);
    try {
      console.log('Payload enviado:', payload);
      const response = await ReglaAccesoService.crearReglaAcceso(payload);
      if (response.status >= 200 && response.status < 300) {
        setSuccess(true);
        // Redirigir después de breve retraso para mostrar mensaje
        setTimeout(() => router.push('/reglas-acceso/listadoReglasAcceso'), 800);
      } else {
        setError('No se pudo crear la regla.');
      }
    } catch (e) {
      console.error('Error completo:', e);
      console.error('Respuesta del servidor:', e.response?.data);
      const errorMsg = e.response?.data?.message || e.response?.data?.error || e.response?.data?.title || 'Error al crear la regla';
      setError(errorMsg);
    } finally {
      setSubmitting(false);
    }
  };

  if (!isAdmin) return null;

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
        <form className="text-card" onSubmit={(e) => { e.preventDefault(); if (!submitting) handleSubmit(); }}>
          <div style={{alignItems: 'center', width: '100%'}}>
            <button type="button" onClick={() => router.back()} style={{ background: 'transparent', border: '2px solid #F37426', color: '#F37426', padding: '6px 16px', borderRadius: '20px', cursor: 'pointer', marginBottom: '12px', fontSize: '0.9rem', fontWeight: '500', transition: 'all 0.2s' }} onMouseEnter={(e) => { e.target.style.background = '#F37426'; e.target.style.color = 'white'; }} onMouseLeave={(e) => { e.target.style.background = 'transparent'; e.target.style.color = '#F37426'; }}>← Regresar</button>
            <h1 className="text-3xl font-bold text-white">Crear Regla de Acceso</h1>
            <hr />
          </div>

          <div className='input-container'>
            <div className='w-full'>
              <span>Espacio ID</span>
              <input 
                type="number" 
                placeholder="ID del Espacio" 
                value={espacioId}
                onChange={(e) => setEspacioId(e.target.value)}
              />
            </div>

            <div className='w-full'>
              <span>Horario de Apertura</span>
              <input 
                type="time" 
                placeholder="Horario de Apertura" 
                value={horarioApertura}
                onChange={(e) => setHorarioApertura(e.target.value)}
              />
            </div>

            <div className='w-full'>
              <span>Horario de Cierre</span>
              <input 
                type="time" 
                placeholder="Horario de Cierre" 
                value={horarioCierre}
                onChange={(e) => setHorarioCierre(e.target.value)}
              />
            </div>

            <div className='w-full'>
              <span>Vigencia Desde</span>
              <input 
                type="date" 
                placeholder="Vigencia Desde" 
                value={vigenciaApertura}
                onChange={(e) => setVigenciaApertura(e.target.value)}
              />
            </div>

            <div className='w-full'>
              <span>Vigencia Hasta</span>
              <input 
                type="date" 
                placeholder="Vigencia Hasta" 
                value={vigenciaCierre}
                onChange={(e) => setVigenciaCierre(e.target.value)}
              />
            </div>

            <div className='w-full'>
              <span>Roles Permitidos</span>
              <div className="roles-container">
                <label className="role-checkbox">
                  <input 
                    type="checkbox" 
                    checked={rolesPermitidos.Estudiante}
                    onChange={() => handleRoleChange('Estudiante')}
                  />
                  <span>Estudiante</span>
                </label>
                <label className="role-checkbox">
                  <input 
                    type="checkbox" 
                    checked={rolesPermitidos.Funcionario}
                    onChange={() => handleRoleChange('Funcionario')}
                  />
                  <span>Funcionario</span>
                </label>
                <label className="role-checkbox">
                  <input 
                    type="checkbox" 
                    checked={rolesPermitidos.Admin}
                    onChange={() => handleRoleChange('Admin')}
                  />
                  <span>Admin</span>
                </label>
              </div>
            </div>

          </div>
         
          {error && (
            <div style={{ color: '#ffdddd', background:'#7e1e1e', borderRadius:12, padding:'8px 14px', margin:'10px 1vw', width:'calc(100% - 2vw)', fontSize:'0.85rem' }}>
              {error}
            </div>
          )}
          {success && (
            <div style={{ color: '#e9ffe9', background:'#1e7e3a', borderRadius:12, padding:'8px 14px', margin:'10px 1vw', width:'calc(100% - 2vw)', fontSize:'0.85rem' }}>
              Regla creada correctamente. Redirigiendo...
            </div>
          )}
          <div className='button-container'>
            <button type="submit" disabled={submitting} style={{ opacity: submitting ? 0.6 : 1, cursor: submitting ? 'not-allowed' : 'pointer' }}>
              {submitting ? 'Creando...' : 'Crear Regla de Acceso'}
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

        .roles-container {
          display: flex;
          flex-direction: column;
          gap: 8px;
          margin-left: 1vw;
          margin-right: 1vw;
        }

        .role-checkbox {
          display: flex;
          align-items: center;
          gap: 8px;
          cursor: pointer;
        }

        .role-checkbox input[type="checkbox"] {
          width: auto;
          margin: 0;
        }

        .role-checkbox span {
          margin: 0;
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

