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




  const handleVerMasEventos = () => {
    const encodedEventos = encodeURIComponent(JSON.stringify(eventos)); // Codifica los eventos
    router.push(`/evento/listadoEventos?eventos=${encodedEventos}`); // Pasa los eventos como parámetro en la URL
  };

  const handleVerMasAnuncios = () => {
    const encodedAnuncios = encodeURIComponent(JSON.stringify(anuncios)); // Codifica los anuncios
    router.push(`/anuncio/listadoAnuncios?anuncios=${encodedAnuncios}`); // Pasa los anuncios como parámetro en la URL
  };

  return (
    <div className="container-nothing">

      <Header/>

      <div className="container">
        <div className="container-header">
          <h2>Eventos</h2>
          <h2 style={{ cursor: 'pointer' }} onClick={handleVerMasEventos}>
            Ver más
          </h2>
        </div>
        
        <div className="carrusel-container">
          <Carousel items={eventos} />
        </div>
      </div>

      <div className="container">
        <div className="container-header">
          <h2>Anuncios</h2>
          <h2 style={{ cursor: 'pointer' }} onClick={handleVerMasAnuncios}>
            Ver más
          </h2>
        </div>
        
        <div className="carrusel-container">
          <Carousel items={anuncios} />
        </div>
      </div>
    </div>
  )
}

  <style jsx>{`
      

      .container-nothing {
        padding: 0 ;
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
        gap: 16px;
        padding-left: 20px;
        padding-right: 20px;
        box-sizing: border-box; /* Incluye el padding en el ancho total */
      }

      .container-header {
        display: flex;
        flex-direction: column;
        width: 100%;
        height: auto;
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0 20px;
      }

      .carrusel-container {
        display: flex;
        overflow-x: auto;
        gap: 16px;
        padding: 10px;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        min-height: 150px; /* Asegura que tenga un tamaño visible */
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
