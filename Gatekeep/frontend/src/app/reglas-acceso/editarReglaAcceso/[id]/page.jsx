"use client"

import React, { useState, useEffect } from 'react'
import Image from 'next/image'
import Link from 'next/link'
import { usePathname, useParams } from 'next/navigation'
import logo from '/public/assets/LogoGateKeep.webp'
import harvard from '/public/assets/Harvard.webp'
import { SecurityService } from '../../../../services/securityService'
import { ReglaAccesoService } from '../../../../services/ReglaAccesoService'

export default function editarReglaAcceso() {

  const pathname = usePathname();
  const params = useParams();
  SecurityService.checkAuthAndRedirect(pathname);

  const reglaId = params.id;

  // Estados
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
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  // Cargar datos de la regla al montar
  useEffect(() => {
    const fetchRegla = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await ReglaAccesoService.getReglaAccesoById(Number(reglaId));
        const regla = response.data;
        setHorarioApertura(regla.HorarioApertura || '');
        setHorarioCierre(regla.HorarioCierre || '');
        setVigenciaApertura(regla.VigenciaApertura ? regla.VigenciaApertura.slice(0, 10) : '');
        setVigenciaCierre(regla.VigenciaCierre ? regla.VigenciaCierre.slice(0, 10) : '');
        setEspacioId(regla.EspacioId ? String(regla.EspacioId) : '');
        // rolesPermitidos es un array de strings
        setRolesPermitidos({
          Estudiante: Array.isArray(regla.RolesPermitidos) && regla.RolesPermitidos.includes('Estudiante'),
          Funcionario: Array.isArray(regla.RolesPermitidos) && regla.RolesPermitidos.includes('Funcionario'),
          Admin: Array.isArray(regla.RolesPermitidos) && regla.RolesPermitidos.includes('Admin'),
        });
      } catch (e) {
        setError('No se pudo cargar la regla de acceso');
      } finally {
        setLoading(false);
      }
    };
    if (reglaId) fetchRegla();
  }, [reglaId]);

  const handleRoleChange = (role) => {
    setRolesPermitidos(prev => ({
      ...prev,
      [role]: !prev[role]
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setSuccess(false);
    setSubmitting(true);
    try {
      // .NET espera DateTime ISO, no solo hora. Usamos la fecha de vigenciaApertura para armar el DateTime.
      const makeDateTimeUTC = (date, time) => {
        if (!date || !time) return null;
        // Construir Date en local y convertir a UTC ISO string
        const [year, month, day] = date.split('-').map(Number);
        const [hour, minute] = time.split(':').map(Number);
        const dt = new Date(Date.UTC(year, month - 1, day, hour, minute, 0));
        return dt.toISOString();
      };
      const makeDateOnlyUTC = (date) => {
        if (!date) return null;
        const [year, month, day] = date.split('-').map(Number);
        const dt = new Date(Date.UTC(year, month - 1, day, 0, 0, 0));
        return dt.toISOString();
      };
      const payload = {
        horarioApertura: makeDateTimeUTC(vigenciaApertura, horarioApertura),
        horarioCierre: makeDateTimeUTC(vigenciaApertura, horarioCierre),
        vigenciaApertura: makeDateOnlyUTC(vigenciaApertura),
        vigenciaCierre: makeDateOnlyUTC(vigenciaCierre),
        espacioId: Number(espacioId),
        rolesPermitidos: Object.entries(rolesPermitidos).filter(([k, v]) => v).map(([k]) => k)
      };
      console.log('Payload enviado a actualizarReglaAcceso:', payload);
      await ReglaAccesoService.actualizarReglaAcceso(Number(reglaId), payload);
      setSuccess(true);
    } catch (e) {
      setError(e?.response?.data?.error || e?.response?.data?.message || 'Error al actualizar la regla');
    } finally {
      setSubmitting(false);
    }
  };

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
            <button type="button" onClick={() => window.history.back()} style={{ background: 'transparent', border: '2px solid #F37426', color: '#F37426', padding: '6px 16px', borderRadius: '20px', cursor: 'pointer', marginBottom: '12px', fontSize: '0.9rem', fontWeight: '500', transition: 'all 0.2s' }} onMouseEnter={(e) => { e.target.style.background = '#F37426'; e.target.style.color = 'white'; }} onMouseLeave={(e) => { e.target.style.background = 'transparent'; e.target.style.color = '#F37426'; }}>← Regresar</button>
            <h1 className="text-3xl font-bold text-white">Editar Regla de Acceso</h1>
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
         
          <div className='button-container'>
            <button type="submit" disabled={submitting}>{submitting ? 'Actualizando...' : 'Actualizar Regla de Acceso'}</button>
          </div>
          {error && (
            <div style={{ color: '#ffdddd', background:'#7e1e1e', borderRadius:12, padding:'8px 14px', margin:'10px 1vw', width:'calc(100% - 2vw)', fontSize:'0.85rem' }}>{error}</div>
          )}
          {success && (
            <div style={{ color: '#e9ffe9', background:'#1e7e3a', borderRadius:12, padding:'8px 14px', margin:'10px 1vw', width:'calc(100% - 2vw)', fontSize:'0.85rem' }}>Regla de acceso actualizada. Redirigiendo...</div>
          )}
          

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

