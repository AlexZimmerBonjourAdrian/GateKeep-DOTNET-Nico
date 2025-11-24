"use client"

import React, { useState, useMemo, useRef, useEffect } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import Header from '../../../components/Header';
import { BeneficioService } from '../../../services/BeneficioService';
import { SecurityService } from '../../../services/securityService';

export default function listadoBeneficios() {
  const pathname = usePathname();
  useEffect(() => {
    SecurityService.checkAuthAndRedirect(pathname);
  }, [pathname]);

  const [beneficios, setBeneficios] = useState([]);
  const [loading, setLoading] = useState(true);
  const [isAdmin, setIsAdmin] = useState(false);
  const router = useRouter();

  // Fetch beneficios al montar el componente
  useEffect(() => {
    const fetchBeneficios = async () => {
      try {
        const response = await BeneficioService.getBeneficios();
        setBeneficios(response.data || []);
      } catch (error) {
        console.error('Error al cargar beneficios:', error);
        setBeneficios([]);
      } finally {
        setLoading(false);
      }
    };

    fetchBeneficios();
  }, []);

  // Determinar si el usuario es Administrador para mostrar el botón de creación
  useEffect(() => {
    try {
      const tipo = SecurityService.getTipoUsuario?.() || null;
      let isAdminRole = false;
      if (tipo) {
        isAdminRole = /admin/i.test(tipo);
      } else if (typeof window !== 'undefined') {
        const raw = localStorage.getItem('user');
        if (raw) {
          const user = JSON.parse(raw);
          const role = user?.TipoUsuario || user?.tipoUsuario || user?.Rol || user?.rol;
          if (role) isAdminRole = /admin|administrador/i.test(String(role));
        }
      }
      setIsAdmin(isAdminRole);
    } catch (e) {
      setIsAdmin(false);
    }
  }, []);

  // Controlled states for search and date filters
  const [searchInput, setSearchInput] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [soloVigentes, setSoloVigentes] = useState(false);
  const inputRef = useRef(null);

  // Filter beneficios based on search input (case-insensitive) and optional date range
  const filteredBeneficios = useMemo(() => {
    const q = searchInput.trim().toLowerCase();
    const hoy = new Date();
    return beneficios.filter((beneficio) => {
      // Solo mostrar beneficios activos
      if (!(beneficio.Activo ?? beneficio.activo)) return false;

      const tipo = beneficio.Tipo ?? beneficio.tipo;
      const fechaVencimiento = beneficio.FechaDeVencimiento ?? beneficio.fechaDeVencimiento ?? null;

      // Filtrar por vigencia (fecha) solo si el usuario marca 'Solo vigentes'
      if (soloVigentes && fechaVencimiento) {
        const fechaVenc = new Date(fechaVencimiento);
        if (fechaVenc < hoy) return false;
      }

      // Filter by query on tipo (convertir a texto y buscar)
      if (q) {
        let tipoTexto = '';
        if (tipo == 0 || tipo === 'Canje' || (typeof tipo === 'string' && tipo.toLowerCase() === 'canje')) {
          tipoTexto = 'canje';
        } else if (tipo == 1 || tipo === 'Consumo' || (typeof tipo === 'string' && tipo.toLowerCase() === 'consumo')) {
          tipoTexto = 'consumo';
        }
        if (!tipoTexto.includes(q)) return false;
      }

      // Filter by date range if provided (filtrar por fecha de vencimiento)
      if (fechaVencimiento) {
        const fechaVencimientoIso = new Date(fechaVencimiento).toISOString().split('T')[0];
        if (dateFrom && fechaVencimientoIso < dateFrom) return false;
        if (dateTo && fechaVencimientoIso > dateTo) return false;
      }

      return true;
    });
  }, [beneficios, searchInput, dateFrom, dateTo, soloVigentes]);

  // Group filtered beneficios in chunks of 4
  const groupedBeneficios = [];
  for (let i = 0; i < filteredBeneficios.length; i += 4) {
    groupedBeneficios.push(filteredBeneficios.slice(i, i + 4));
  }

  // Called when the form is submitted (press Enter or click search)
  const handleSearchSubmit = (e) => {
    e.preventDefault();
    setSearchInput((s) => s.trim());
    if (inputRef.current) inputRef.current.blur();
  };

  const handleDelete = async (id) => {
    if (!confirm('¿Estás seguro de eliminar este beneficio?')) return;
    try {
      await BeneficioService.eliminarBeneficio(id);
      setBeneficios(beneficios.filter(ben => (ben.Id || ben.id) !== id));
    } catch (e) {
      console.error('Error al eliminar beneficio:', e);
      alert('Error al eliminar el beneficio');
    }
  };

  return (
    <div className="container-nothing">
        <Header />

        <div className="container">
          <div className="container-header">
            <h2>Beneficios</h2>
            <div className="filtros-container">
              <div className="field">
                <label className="field-label" htmlFor="search-input">Buscar por tipo</label>
                <form className="search-bar" onSubmit={handleSearchSubmit}>
                  <input
                    id="search-input"
                    ref={inputRef}
                    className="search-input"
                    type="text"
                    placeholder="Buscar: canje, consumo..."
                    aria-label="Buscar beneficios"
                    value={searchInput}
                    onChange={(e) => setSearchInput(e.target.value)}
                  />
                  <button className="search-button" type="button" aria-label="Buscar" onClick={(e) => e.preventDefault()}>
                    <svg className="search-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <circle cx="11" cy="11" r="7"></circle>
                      <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
                    </svg>
                  </button>
                </form>
              </div>

              <div className="field">
                <label className="field-label" htmlFor="date-from">Vence desde</label>
                <input
                  id="date-from"
                  className="date-input"
                  type="date"
                  value={dateFrom}
                  onChange={(e) => setDateFrom(e.target.value)}
                  aria-label="Fecha desde"
                />
              </div>

              <div className="field">
                <label className="field-label" htmlFor="date-to">Vence hasta</label>
                <input
                  id="date-to"
                  className="date-input"
                  type="date"
                  value={dateTo}
                  onChange={(e) => setDateTo(e.target.value)}
                  aria-label="Fecha hasta"
                />
              </div>


              <div className="field" style={{ minWidth: '140px' }}>
                <label className="field-label" htmlFor="solo-vigentes">Solo vigentes</label>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '6px 10px', background:'#f8fafc', border:'1px solid #e5e7eb', borderRadius: '20px' }}>
                  <input
                    id="solo-vigentes"
                    type="checkbox"
                    checked={soloVigentes}
                    onChange={(e) => setSoloVigentes(e.target.checked)}
                    aria-label="Filtrar vigentes"
                  />
                  <span style={{ fontSize: '0.8rem', color:'#111827' }}>Vigentes</span>
                </div>
              </div>

              {isAdmin && (
                <div className="actions-inline">
                  <button
                    type="button"
                    className="create-button"
                    onClick={() => router.push('/beneficio/crearBeneficio')}
                    aria-label="Crear nuevo beneficio"
                  >
                    Crear Beneficio
                  </button>
                </div>
              )}

              <div className="actions-inline">
                <button
                  type="button"
                  className="history-button"
                  onClick={() => router.push('/historialBeneficios')}
                  aria-label="Ver historial de beneficios"
                >
                  <i className="pi pi-history" style={{marginRight:'6px'}}></i>
                  Mi Historial
                </button>
              </div>
            </div>
          </div>

          {/* Beneficios list */}
          <div className="events-grid">
            {loading ? (
              <div className="event-card" style={{ background: '#fff6ee' }}>
                <h3>Cargando beneficios...</h3>
              </div>
            ) : filteredBeneficios.length === 0 ? (
              <div className="event-card" style={{ background: '#fff6ee' }}>
                <h3>No se encontraron beneficios</h3>
                <p>Prueba otro término o rango de fecha.</p>
              </div>
            ) : (
              groupedBeneficios.map((group, gi) => (
                <div className="event-group" key={`group-${gi}`}>
                  {group.map((beneficio) => {
                    const id = beneficio.Id ?? beneficio.id;
                    const tipo = beneficio.Tipo ?? beneficio.tipo;
                    const vigencia = beneficio.Vigencia ?? beneficio.vigencia ?? false;
                    const fechaVencimiento = beneficio.FechaDeVencimiento ?? beneficio.fechaDeVencimiento;
                    const cupos = beneficio.Cupos ?? beneficio.cupos;
                    
                    // El backend envía el nombre del enum como string
                    let tipoTexto = 'Desconocido';
                    if (tipo == 0 || tipo === 'Canje' || (typeof tipo === 'string' && tipo.toLowerCase() === 'canje')) {
                      tipoTexto = 'Canje';
                    } else if (tipo == 1 || tipo === 'Consumo' || (typeof tipo === 'string' && tipo.toLowerCase() === 'consumo')) {
                      tipoTexto = 'Consumo';
                    }
                    
                    return (
                      <div
                        key={id}
                        className="event-card"
                        tabIndex={0}
                      >
                        <div 
                          onClick={() => router.push(`/beneficio/${id}`)}
                          onKeyDown={(e) => { if (e.key==='Enter' || e.key===' ') { e.preventDefault(); router.push(`/beneficio/${id}`) }}}
                          style={{ cursor: 'pointer', flex: 1 }}
                        >
                          <h3>Beneficio #{id}</h3>
                          <p style={{ fontSize: '0.9rem', marginBottom: '8px', fontWeight: '600' }}>{tipoTexto}</p>
                          {fechaVencimiento && (
                            <p style={{ fontSize: '0.8rem' }}>
                              <strong>Vence:</strong> {new Date(fechaVencimiento).toLocaleDateString('es-ES', { year: 'numeric', month: 'long', day: 'numeric' })}
                            </p>
                          )}
                          <p style={{ fontSize: '0.8rem' }}><strong>Cupos disponibles:</strong> {cupos}</p>
                          <p style={{ fontSize: '0.8rem' }}><strong>Estado:</strong> {vigencia ? 'Vigente' : 'No vigente'}</p>
                        </div>
                        {isAdmin && (
                          <div className="card-actions" onClick={(e) => e.stopPropagation()}>
                            <button
                              className="card-action-btn edit-btn"
                              onClick={(e) => { e.stopPropagation(); router.push(`/beneficio/editarBeneficio/${id}`); }}
                              aria-label={`Editar beneficio ${id}`}
                            >
                              ✏️ Editar
                            </button>
                            <button
                              className="card-action-btn delete-btn"
                              onClick={(e) => { e.stopPropagation(); handleDelete(id); }}
                              aria-label={`Eliminar beneficio ${id}`}
                            >
                              🗑️ Eliminar
                            </button>
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              ))
            )}
          </div>

        </div>

        <style jsx>{`

          /* Base layout tweaks */
          .container-header{ padding-left: 1.111vw; width: auto; }
          .container-nothing { margin: 0; width: 100%; height: 100%; }
          .actions-inline{ display:flex; align-items:center; gap:12px; margin-left:auto; margin-right: clamp(12px, 1.111vw, 24px); }
          .create-button{ background:#f37426; color:#fff; border:none; padding:8px 16px; border-radius:20px; cursor:pointer; font-size:0.85rem; font-weight:600; letter-spacing:0.3px; box-shadow:0 2px 6px rgba(0,0,0,0.15); transition:background 0.15s ease, transform 0.15s ease; }
          .create-button:hover{ background:#ff8d45; transform:translateY(-2px); }
          .create-button:active{ transform:translateY(0); }
          .create-button:focus-visible{ outline:2px solid rgba(37,99,235,0.4); outline-offset:2px; }

          /* Filters row */
          .filtros-container{ display:flex; gap:12px; align-items:center; flex-wrap:wrap; }
          .field{ display:flex; flex-direction:column; gap:6px; }
          .field-label{ font-size:0.75rem; color:#e5e7ebf6; font-weight:600; letter-spacing:0.2px; margin-bottom:0; }

          /* Search & date inputs */
          .search-bar{ display:flex; align-items:center; gap:8px; background:#f8fafc; border:1px solid #e5e7eb; border-radius:20px; padding:6px 10px; height:40px; box-sizing:border-box; }
          .search-input{ border:none; outline:none; background:transparent; font-size:0.95rem; color:#111827; padding:6px 8px; border-radius:20px; width:clamp(140px,22vw,360px); }
          .search-button{ display:inline-flex; align-items:center; justify-content:center; background:#f37426; color:white; border:none; height:28px; width:36px; padding:0; border-radius:14px; cursor:pointer; }
          .search-button:active{ transform:scale(0.98); }
          .search-icon{ display:block; color:white; }
          .date-input{ height:40px; padding:6px 10px; border:1px solid #e5e7eb; border-radius:20px; background:#f8fafc; font-size:0.95rem; color:#111827; outline:none; box-sizing:border-box; width:clamp(120px,14vw,220px); }

          /* Container that holds groups of 4 */
          .events-grid{ display:flex; flex-direction:column; gap:18px; padding:16px; box-sizing:border-box; }

          /* Each group keeps exactly the same 4 items together */
          .event-group{ display:grid; grid-template-columns: repeat(1, 1fr); gap:12px; }
          @media (min-width: 426px) and (max-width: 768px) { .event-group{ grid-template-columns: repeat(2, 1fr); gap:12px; } }
          @media (min-width: 769px) { .event-group{ grid-template-columns: repeat(4, 1fr); gap:16px; } }

          /* Event card keeps proportions via aspect-ratio */
          .event-card{ width:100%; aspect-ratio: 4 / 3; padding:12px; background:#f37426; border-radius:20px; box-shadow:0 2px 6px rgba(0,0,0,0.12); transition:transform 0.18s ease, box-shadow 0.18s ease; color:#231F20; box-sizing:border-box; display:flex; flex-direction:column; justify-content:space-between; }
          .event-card:hover{ transform: translateY(-4px) scale(1.01); box-shadow:0 8px 18px rgba(0,0,0,0.18); z-index:1; }
          .event-card:focus, .event-card:focus-visible{ transform: translateY(-4px) scale(1.01); box-shadow:0 10px 20px rgba(0,0,0,0.22); border:2px solid rgba(37,99,235,0.12); z-index:2; }

          .event-card h3{ font-size: clamp(1rem, 1.6vw, 1.2rem); margin:0 0 6px 0; }
          .event-card p{ font-size: clamp(0.85rem, 1.1vw, 1rem); margin:0; }
          .card-actions{ display:flex; gap:6px; margin-top:8px; }
          .card-action-btn{ padding:6px 10px; border:none; border-radius:12px; cursor:pointer; font-size:0.75rem; font-weight:600; transition:all 0.15s ease; }
          .edit-btn{ background:#231F20; color:#fff; }
          .edit-btn:hover{ background:#3d3739; transform:scale(1.05); }
          .delete-btn{ background:#231F20; color:#fff; }
          .delete-btn:hover{ background:#7e1e1e; transform:scale(1.05); }

      `}</style>
    </div>
  )
}
