"use client"

import React from 'react';
import { useRouter } from 'next/navigation'; // Importa el hook para la navegación
import Header from '../components/Header';
import Carousel from '../components/Carousel';

export default function Home() {
  const router = useRouter(); // Inicializa el hook useRouter

  const eventos = [
    { id: 1, title: 'Evento 1', date: '2024-07-01' },
    { id: 2, title: 'Evento 2', date: '2024-07-05' },
    { id: 3, title: 'Evento 3', date: '2024-07-10' },
    { id: 4, title: 'Evento 4', date: '2024-07-01' },
    { id: 5, title: 'Evento 5', date: '2024-07-05' },
    { id: 6, title: 'Evento 6', date: '2024-07-10' },
    { id: 7, title: 'Evento 7', date: '2024-07-01' },
    { id: 8, title: 'Evento 8', date: '2024-07-05' },
    { id: 9, title: 'Evento 9', date: '2024-07-10' },
    { id: 10, title: 'Evento 10', date: '2024-07-10' },

  ];

  const anuncios = [
    { id: 1, title: 'Evento 1', date: '2024-07-01' },
    { id: 2, title: 'Evento 2', date: '2024-07-05' },
    { id: 3, title: 'Evento 3', date: '2024-07-10' },
    { id: 4, title: 'Evento 4', date: '2024-07-01' },
    { id: 5, title: 'Evento 5', date: '2024-07-05' },
    { id: 6, title: 'Evento 6', date: '2024-07-10' },
    { id: 7, title: 'Evento 7', date: '2024-07-01' },
    { id: 8, title: 'Evento 8', date: '2024-07-05' },
    { id: 9, title: 'Evento 9', date: '2024-07-10' },
    { id: 10, title: 'Evento 10', date: '2024-07-10' },

  ];

  return (
    <div className="container-nothing">

      <Header/>

      <div className="container">
        <div className="container-header">
          <h2>Eventos</h2>
        </div>
        
        <div className="carrusel-container">
          <Carousel items={eventos} route="/evento/listadoEventos" />
        </div>
      </div>

      <div className="container">
        <div className="container-header">
          <h2>Anuncios</h2>
        </div>
        
        <div className="carrusel-container">
          <Carousel items={anuncios} route="/anuncio/listadoAnuncios" />
        </div>
      </div>
    

    <style jsx>{`

        .container-nothing {
          margin: 0;
          width: 100%;
          height: 100%;
        }
        
        .container {
          width: 100%;
          max-width: 100%;
          height: auto;
          display: flex;
          flex-direction: column;
          gap: 0.313rem;
          padding: 0; 
          box-sizing: border-box;
        }

        .container-header {
          display: flex;
          flex-direction: row;
          width: auto; 
          justify-content: space-between;
          align-items: center;
          padding: 0px;
        }

        .container-header h2 {
          margin: 0;
          margin-left: 0.833vw;
          margin-top: 16px;
        }

        /* opcional: asegurar box-sizing global (styled-jsx :global) */
        :global(*) {
          box-sizing: border-box;
        }

        .carrusel-container {
          display: flex;
          overflow-x: auto;
          gap: 0.833vw;
          min-height: 150px; 
        }

        .carrusel-container::-webkit-scrollbar {
          height: 8px;
        }

        .carrusel-container::-webkit-scrollbar-thumb {
          background: #ccc;
          border-radius: 4px;
        }

        .carrusel-container::-webkit-scrollbar-track {
          background: #f0f0f0;
        }

    `}</style>
  </div>
  );
}
