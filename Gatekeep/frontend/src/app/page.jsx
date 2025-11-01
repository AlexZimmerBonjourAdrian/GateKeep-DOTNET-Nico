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

      <div className="container container-anuncios">
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
          /* Base min-height that adapts via media queries below */
          min-height: clamp(110px, 18vw, 220px);
          padding: 6px 0;
          align-items: center;
        }

        /* PHONE: <=425px - match Carousel small card sizing */
        @media (max-width: 425px) {
          .carrusel-container {
            gap: 2vw;
            /* Cards are around 16vw width with aspect-ratio, so container height ~23vw */
            min-height: clamp(80px, 23vw, 110px);
            padding-inline: 6px;
          }

          .container-header h2 {
            font-size: 1rem;
            margin-left: 6px;
            margin-top: 12px;
          }

          .container-anuncios {

          }
        }

        /* TABLET: 426px - 768px */
        @media (min-width: 426px) and (max-width: 768px) {
          .carrusel-container {
            gap: 1.2vw;
            /* Cards use min-width ~140px and aspect-ratio -> height around 180-220px */
            min-height: clamp(140px, 28vw, 210px);
            padding-inline: 8px;
          }

          .container-header h2 {
            font-size: 1.05rem;
            margin-left: 0.8vw;
            margin-top: 14px;
          }
        }

        /* DESKTOP: >=769px */
        @media (min-width: 769px) {
          .carrusel-container {
            gap: 0.833vw;
            min-height: clamp(160px, 22vw, 260px);
            padding-inline: 12px;
          }

          .container-header h2 {
            font-size: 1.05rem;
            margin-left: 0.833vw;
            margin-top: 16px;
          }
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

        .container-anuncios {
          padding-bottom: 90px;
        }

    `}</style>
  </div>
  );
}
