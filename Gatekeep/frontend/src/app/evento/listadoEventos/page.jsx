"use client"

import React from 'react';
import { useSearchParams } from 'next/navigation';
import Header from '../../../components/Header';

export default function listadoEventos() {
  const searchParams = useSearchParams();
  const eventos = JSON.parse(searchParams.get('eventos')) || []; // Recupera los eventos o usa un array vacío si no hay datos

  return (
    <div className="header-root">
        <Header />

        <div className="header-topbar">
          <h1>Listado de Eventos</h1>
          <ul>
            {eventos.map((evento) => (
              <div key={evento.id} className="carousel-item">
                {evento.title && <h3>{evento.title}</h3>}
                {evento.date && <p>{evento.date}</p>}
              </div>
            ))}
          </ul>
        </div>

        <style jsx>{`

          .header-topbar {
            width: 100%;
          }

          .carousel-item {
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
  )
}

