"use client"

import React, { useState } from 'react';
import { useSearchParams } from 'next/navigation';
import Header from '../../../components/Header';

export default function listadoEventos() {
  const searchParams = useSearchParams();
  const eventos = JSON.parse(searchParams.get('eventos')) || []; // Recupera los eventos o usa un array vacío si no hay datos

  const [searchTerm, setSearchTerm] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  // Filtrar eventos según los filtros
  const eventosFiltrados = eventos.filter((evento) => {
    const matchesSearch = evento.title?.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesDateFrom = dateFrom ? new Date(evento.date) >= new Date(dateFrom) : true;
    const matchesDateTo = dateTo ? new Date(evento.date) <= new Date(dateTo) : true;
    return matchesSearch && matchesDateFrom && matchesDateTo;
  });

  // Divide los eventos en filas de 3
  const filasDeEventos = [];
  for (let i = 0; i < eventosFiltrados.length; i += 3) {
    filasDeEventos.push(eventosFiltrados.slice(i, i + 3));
  }

  return (
    <div className="header-root">
        <Header />

        <div className="header-topbar">
          <h1>Listado de Eventos</h1>

          <div className="filtros">
            <input
              type="text"
              placeholder="Buscar..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="filtro-input"
            />
            <input
              type="date"
              value={dateFrom}
              onChange={(e) => setDateFrom(e.target.value)}
              className="filtro-input"
            />
            <input
              type="date"
              value={dateTo}
              onChange={(e) => setDateTo(e.target.value)}
              className="filtro-input"
            />
          </div>

          <div className="eventos-container">
            {filasDeEventos.map((fila, index) => (
              <div key={index} className="fila">
                {fila.map((evento) => (
                  <div key={evento.id} className="evento-item">
                    {evento.title && <h3>{evento.title}</h3>}
                    {evento.date && <p>{evento.date}</p>}
                  </div>
                ))}
              </div>
            ))}
          </div>
        </div>

        <style jsx>{`
          .header-topbar {
            width: 100%;
          }

          .filtros {
            display: flex;
            gap: 16px;
            margin-bottom: 16px;
          }

          .filtro-input {
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 20px;
            background: white;
            flex: 1;
          }

          .eventos-container {
            display: flex;
            flex-direction: column;
            gap: 16px;
          }

          .fila {
            display: flex;
            justify-content: space-between;
            gap: 16px;
          }

          .evento-item {
            flex: 1;
            min-width: 28vw;
            height: 500px;
            padding: 16px;
            background: #f37426;
            border-radius: 20px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
            transition: transform 0.3s ease, box-shadow 0.3s ease;
          }
        `}</style>
    </div>
  );
}

