"use client"

import React, { useState, useMemo, useRef } from 'react';
import Header from '../../../components/Header';

export default function listadoAnuncios() {

  const anuncios = [
    { id: 1, title: 'Hockey Game', date: '2024-07-01' },
    { id: 2, title: 'Soccer Match', date: '2024-07-05' },
    { id: 3, title: 'Basketball Tournament', date: '2024-07-10' },
    { id: 4, title: 'Tennis Finals', date: '2024-07-15' },
    { id: 5, title: 'Swimming Competition', date: '2024-07-20' },
    { id: 6, title: 'Marathon', date: '2024-07-25' },
    { id: 7, title: 'Cycling Race', date: '2024-07-30' },
  ]

  // Controlled states for search and date filters
  const [searchInput, setSearchInput] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const inputRef = useRef(null);

  // Filter items based on search input (case-insensitive) and optional date range
  const filteredItems = useMemo(() => {
    const q = searchInput.trim().toLowerCase();
    return anuncios.filter((ev) => {
      // Filter by query on title
      if (q) {
        const title = ev.title ? ev.title.toLowerCase() : '';
        if (!title.includes(q)) return false;
      }

      // Filter by date range if provided
      if (dateFrom) {
        if (!ev.date || ev.date < dateFrom) return false;
      }
      if (dateTo) {
        if (!ev.date || ev.date > dateTo) return false;
      }

      return true;
    });
  }, [searchInput, dateFrom, dateTo]);

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
            <h2>Anuncios</h2>
            <div className="filtros-container">
              <div className="field">
                <label className="field-label" htmlFor="search-input">Buscar</label>
                <form className="search-bar" onSubmit={handleSearchSubmit}>
                  <input
                    id="search-input"
                    ref={inputRef}
                    className="search-input"
                    type="text"
                    placeholder="Buscar anuncios..."
                    aria-label="Buscar anuncios"
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
            </div>
          </div>

          {/* items list - mostrar todas las tarjetas similar al Carousel */}
          <div className="events-grid">
            {filteredItems.length === 0 ? (
              <div className="event-card" style={{ background: '#fff6ee' }}>
                <h3>No se encontraron anuncios</h3>
                <p>Prueba otro término o rango de fecha.</p>
              </div>
            ) : (
              filteredItems.map((ev) => (
                <div key={ev.id} className="event-card" tabIndex={0}>
                  {ev.title && <h3>{ev.title}</h3>}
                  {ev.date && <p>{ev.date}</p>}
                </div>
              ))
            )}
          </div>

        </div>

        <style jsx>{`

          .container-header{
            padding-left: 1.111vw;
            width: auto;
          }

          .container-nothing {
            margin: 0;
            width: 100%;
            height: 100%;
          }

          .event-card{
            background: #f37426; /* mismo color que Carousel */
            border-radius: 20px;
            padding: 16px, 0.833vw;
            min-height: 140px;
            box-shadow: 0 2px 6px rgba(0,0,0,0.12);
            transition: transform 0.2s ease, box-shadow 0.2s ease;
            color: #231F20;
            outline: none;
          }

          /* Match Carousel hover exactly */
          .event-card:hover{
            transform: translateY(-4px) scale(1.02);
            box-shadow: 0 6px 12px rgba(0, 0, 0, 0.18);
            z-index: 1;
          }

          .event-card:focus,
          .event-card:focus-visible{
            transform: translateY(-4px) scale(1.02);
            box-shadow: 0 8px 18px rgba(0, 0, 0, 0.22);
            border: 2px solid rgba(37,99,235,0.15);
            z-index: 2;
          }

          .filtros-container{
            display: flex;
            gap: 0.785vw; 
            align-items: center;
          }

          /* Field wrapper + label */
          .field{
            display: flex;
            flex-direction: column;
            gap: 0.313vw;
          }

          .field-label{
            font-size: 0.75rem;
            color: #e5e7ebf6; 
            font-weight: 600;
            letter-spacing: 0.2px;
            margin-bottom: 0;
          }

          /* Search bar */
          .search-bar{
            display: flex;
            align-items: center;
            gap: 0.313vw;
            background: #f8fafc;
            border: 1px solid #e5e7eb;
            border-radius: 20px; /* solicitado */
            padding: 6px 0.417vw;
            height: 40px;
            box-sizing: border-box;
            transition: box-shadow 150ms ease, border-color 150ms ease, background 150ms ease;
          }

          .search-input{
            border: none;
            outline: none;
            background: transparent;
            font-size: 0.875rem;
            color: #111827;
            padding: 6px 8px;
            border-radius: 20px; /* también en input */
            width: 220px;
          }

          .search-button{
            display: inline-flex;
            align-items: center;
            justify-content: center;
            background: #f37426; /* azul agradable */
            color: white;
            border: none;
            height: 28px;
            width: 1.771vw;
            padding: 0;
            border-radius: 14px;
            cursor: pointer;
          }

          .search-button:active{
            transform: scale(0.98);
          }

          .search-icon{
            display: block;
            color: white;
          }

          /* Date input matching search proportions */
          .date-input{
            height: 40px;
            padding: 6px 0.625vw;
            border: 1px solid #e5e7eb;
            border-radius: 20px; /* igual que el search */
            background: #f8fafc;
            font-size: 0.875rem;
            color: #111827;
            outline: none;
            box-sizing: border-box;
            width: 14vw; /* proporción similar, ajustable */
          }

          /* Eventos grid (lista completa) */
          .events-grid{
            display: grid;
            /* Forzar hasta 4 columnas teniendo en cuenta espacios/márgenes */
            grid-template-columns: repeat(4, minmax(0, 1fr));
            gap: 1.47vw;
            padding: 16px;
            width: 100%;
            box-sizing: border-box;
          }

          /* Responsive: 3 / 2 / 1 columnas según ancho */
          @media (max-width: 1200px) {
            .events-grid { grid-template-columns: repeat(3, minmax(0, 1fr)); }
          }
          @media (max-width: 900px) {
            .events-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
          }
          @media (max-width: 600px) {
            .events-grid { grid-template-columns: repeat(1, minmax(0, 1fr)); }
          }

          .event-card{
            /* Allow the grid to size cards; use full column width */
            width: 100%;
            height: 360px; /* altura razonable; ajustar si hace falta */
            padding: 10px 0.833vw;
            background: #f37426;
            border-radius: 20px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
            transition: transform 0.2s ease, box-shadow 0.2s ease;
            outline: none; /* remove default focus outline */
            box-sizing: border-box;
          }

          .event-card:hover{
            transform: translateY(-4px);
            box-shadow: 0 8px 18px rgba(0,0,0,0.18);
            z-index: 1;
          }

          .event-card:focus,
          .event-card:focus-visible{
            transform: translateY(-4px);
            box-shadow: 0 10px 20px rgba(0,0,0,0.22);
            border: 2px solid rgba(37,99,235,0.12);
            z-index: 2;
          }

          
        `}</style>
    </div>
  );
}

