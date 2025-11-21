"use client"

import React, { useState, useMemo, useRef, useEffect } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import Header from '../../../components/Header';
import { EventoService } from '../../../services/EventoService';
import { SecurityService } from '../../../services/securityService';

export default function listadoEventos() {
  const pathname = usePathname();
  useEffect(() => {
    SecurityService.checkAuthAndRedirect(pathname);
  }, [pathname]);

  const [eventos, setEventos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [isAdmin, setIsAdmin] = useState(false);
  const router = useRouter();

  // Fetch eventos al montar el componente
  useEffect(() => {
    const fetchEventos = async () => {
      try {
        const response = await EventoService.getEventos();
        setEventos(response.data || []);
      } catch (error) {
        console.error('Error al cargar eventos:', error);
        setEventos([]);
      } finally {
        setLoading(false);
      }
    };

    fetchEventos();
  }, []);

  // Determinar si el usuario es Administrador para mostrar el botón de creación
  useEffect(() => {
    try {
      // Primero, intentar con SecurityService (usa 'tipoUsuario')
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
  const inputRef = useRef(null);

  // Filter events based on search input (case-insensitive) and optional date range
  const filteredEventos = useMemo(() => {
    const q = searchInput.trim().toLowerCase();
    return eventos.filter((ev) => {
      const nombreSrc = ev.Nombre ?? ev.nombre ?? ev.title ?? '';
      const fechaSrc = ev.Fecha ?? ev.fecha ?? ev.date ?? null;
      const fechaIso = fechaSrc ? new Date(fechaSrc).toISOString().split('T')[0] : '';
      // Filter by query on Nombre (name)
      if (q) {
        const nombre = typeof nombreSrc === 'string' ? nombreSrc.toLowerCase() : '';
        if (!nombre.includes(q)) return false;
      }

      // Filter by date range if provided
      if (dateFrom) {
        if (!fechaIso || fechaIso < dateFrom) return false;
      }
      if (dateTo) {
        if (!fechaIso || fechaIso > dateTo) return false;
      }

      return true;
    });
  }, [eventos, searchInput, dateFrom, dateTo]);

  // Group filtered events in chunks of 4 so visual grouping of 4 is preserved
  const groupedEventos = [];
  for (let i = 0; i < filteredEventos.length; i += 4) {
    groupedEventos.push(filteredEventos.slice(i, i + 4));
  }

  // Called when the form is submitted (press Enter or click search)
  const handleSearchSubmit = (e) => {
    e.preventDefault();
    // trim the current input (keeps live filtering consistent)
    setSearchInput((s) => s.trim());
    // optionally blur input so user sees results; keeps focus behavior tidy
    if (inputRef.current) inputRef.current.blur();
  };

  return (
    <div className="container-nothing">
        <Header />

        <div className="container">
          <div className="container-header">
            <h2>Eventos</h2>
            <div className="filtros-container">
              <div className="field">
                <label className="field-label" htmlFor="search-input">Buscar</label>
                <form className="search-bar" onSubmit={handleSearchSubmit}>
                  <input
                    id="search-input"
                    ref={inputRef}
                    className="search-input"
                    type="text"
                    placeholder="Buscar eventos..."
                    aria-label="Buscar eventos"
                    value={searchInput}
                    onChange={(e) => setSearchInput(e.target.value)}
                  />
                  {/* El botón ahora es sólo decorativo: no dispara el submit. Enter en el input sigue funcionando. */}
                  <button className="search-button" type="button" aria-label="Buscar" onClick={(e) => e.preventDefault()}>
                    {/* Icono de lupa */}
                    <svg className="search-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <circle cx="11" cy="11" r="7"></circle>
                      <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
                    </svg>
                  </button>
                </form>
              </div>

              <div className="field">
                <label className="field-label" htmlFor="date-from">Desde</label>
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
                <label className="field-label" htmlFor="date-to">Hasta</label>
                <input
                  id="date-to"
                  className="date-input"
                  type="date"
                  value={dateTo}
                  onChange={(e) => setDateTo(e.target.value)}
                  aria-label="Fecha hasta"
                />
              </div>

              {isAdmin && (
                <div className="actions-inline">
                  <button
                    type="button"
                    className="create-button"
                    onClick={() => router.push('/evento/crearEvento')}
                    aria-label="Crear nuevo evento"
                  >
                    Crear Evento
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* Eventos list - mostrar todas las tarjetas similar al Carousel */}
          <div className="events-grid">
            {loading ? (
              <div className="event-card" style={{ background: '#fff6ee' }}>
                <h3>Cargando eventos...</h3>
              </div>
            ) : filteredEventos.length === 0 ? (
              <div className="event-card" style={{ background: '#fff6ee' }}>
                <h3>No se encontraron eventos</h3>
                <p>Prueba otro término o rango de fecha.</p>
              </div>
            ) : (
              // Render each group as its own grid so groups of 4 stay together
              groupedEventos.map((group, gi) => (
                <div className="event-group" key={`group-${gi}`}>
                  {group.map((ev) => {
                    const id = ev.Id ?? ev.id ?? `${ev.Nombre ?? ev.nombre ?? 'ev'}-${Math.random()}`;
                    const nombre = ev.Nombre ?? ev.nombre ?? ev.title;
                    const fecha = ev.Fecha ?? ev.fecha ?? ev.date;
                    const punto = ev.PuntoControl ?? ev.puntoControl;
                    const resultado = ev.Resultado ?? ev.resultado;
                    return (
                      <div
                        key={id}
                        className="event-card"
                        tabIndex={0}
                        role="button"
                        onClick={() => router.push(`/evento/${id}`)}
                        onKeyDown={(e) => { if (e.key==='Enter' || e.key===' ') { e.preventDefault(); router.push(`/evento/${id}`) }}}
                      >
                        {nombre && <h3>{nombre}</h3>}
                        {fecha && <p>{new Date(fecha).toLocaleDateString('es-ES', { 
                        year: 'numeric', 
                        month: 'long', 
                        day: 'numeric' 
                      })}</p>}
                        {punto && <p><strong>Punto Control:</strong> {punto}</p>}
                        {resultado && <p><strong>Resultado:</strong> {resultado}</p>}
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

          /* Each group keeps exactly the same 4 items together. Grid inside each group is responsive:
             - desktop (>=769px): 4 columns (1 row)
             - tablet (426-768px): 2 columns (2 rows)
             - mobile (<=425px): 1 column (4 rows)
          */
          .event-group{ display:grid; grid-template-columns: repeat(1, 1fr); gap:12px; }
          @media (min-width: 426px) and (max-width: 768px) { .event-group{ grid-template-columns: repeat(2, 1fr); gap:12px; } }
          @media (min-width: 769px) { .event-group{ grid-template-columns: repeat(4, 1fr); gap:16px; } }

          /* Event card keeps proportions via aspect-ratio so height scales with width */
          .event-card{ width:100%; aspect-ratio: 4 / 3; padding:12px; background:#f37426; border-radius:20px; box-shadow:0 2px 6px rgba(0,0,0,0.12); transition:transform 0.18s ease, box-shadow 0.18s ease; color:#231F20; box-sizing:border-box; display:flex; flex-direction:column; justify-content:center; }
          .event-card:hover{ transform: translateY(-4px) scale(1.01); box-shadow:0 8px 18px rgba(0,0,0,0.18); z-index:1; }
          .event-card:focus, .event-card:focus-visible{ transform: translateY(-4px) scale(1.01); box-shadow:0 10px 20px rgba(0,0,0,0.22); border:2px solid rgba(37,99,235,0.12); z-index:2; }

          .event-card h3{ font-size: clamp(1rem, 1.6vw, 1.2rem); margin:0 0 6px 0; }
          .event-card p{ font-size: clamp(0.85rem, 1.1vw, 1rem); margin:0; }

      `}</style>
    </div>
  )
}

