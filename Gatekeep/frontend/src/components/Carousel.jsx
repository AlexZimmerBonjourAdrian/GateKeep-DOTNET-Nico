"use client";

import React, { useState } from 'react';
import PropTypes from 'prop-types';
import { useRouter } from 'next/navigation';

const Carousel = ({ items, route }) => {
  const [currentIndex, setCurrentIndex] = useState(0);
  const router = useRouter();
  
  // Ordenar items por fecha (más recientes primero - izquierda)
  const sortedItems = React.useMemo(() => {
    if (!items || items.length === 0) return [];
    
    return [...items].sort((a, b) => {
      // Obtener las fechas de diferentes formatos posibles
      const dateA = a.fecha || a.Fecha || a.date || a.fechaDeVencimiento || a.FechaDeVencimiento;
      const dateB = b.fecha || b.Fecha || b.date || b.fechaDeVencimiento || b.FechaDeVencimiento;
      
      if (!dateA || !dateB) return 0;
      
      // Comparar fechas en orden descendente (más recientes primero)
      return new Date(dateB) - new Date(dateA);
    });
  }, [items]);
  
  // Derivar ruta de detalle en base a la ruta de listado provista
  const getDetailPath = (item) => {
    const id = item?.id ?? item?.Id;
    if (!id || !route) return null;
    if (route.includes('/evento')) return `/evento/${id}`;
    if (route.includes('/anuncio')) return `/anuncio/${id}`;
    if (route.includes('/beneficio')) return `/beneficio/${id}`;
    if (route.includes('/edificios')) return `/edificios/${id}`;
    if (route.includes('/reglas-acceso')) return `/reglas-acceso/${id}`;
    return null;
  };
  
  const goToDetail = (item) => {
    const path = getDetailPath(item);
    if (path) router.push(path);
  };
  
  // Lógica adaptable:
  // - Si hay <= 8 items: mostrar 3 + "Ver más"
  // - Si hay > 8 items: mostrar 4 por página, excepto la última que será 3 + "Ver más"
  const totalItems = sortedItems.length;
  const isSmallCollection = totalItems <= 8;
  
  // Determinar cuántos items mostrar en la página actual
  let itemsPerPage;
  let itemsRemaining = totalItems - currentIndex;
  
  if (isSmallCollection) {
    // Colección pequeña: siempre 3 items
    itemsPerPage = Math.min(3, totalItems);
  } else {
    // Colección grande: mostrar 4, excepto si quedan 4 o menos (entonces mostrar 3)
    if (itemsRemaining <= 4) {
      itemsPerPage = Math.min(3, itemsRemaining);
    } else {
      itemsPerPage = 4;
    }
  }
  
  // Calcular cuántos items realmente mostrar (sin contar "Ver más")
  const visibleItems = sortedItems.slice(currentIndex, currentIndex + itemsPerPage);
  
  // Calcular si hay más items después de los visibles
  const hasMoreItems = (currentIndex + itemsPerPage) < totalItems;
  
  const handleNext = () => {
    if (hasMoreItems) {
      const nextIndex = currentIndex + itemsPerPage;
      setCurrentIndex(Math.min(nextIndex, totalItems - 1));
    }
  };

  const handlePrev = () => {
    if (currentIndex > 0) {
      // Retroceder según la lógica adaptable
      const prevItemsPerPage = isSmallCollection ? 3 : 4;
      setCurrentIndex(Math.max(currentIndex - prevItemsPerPage, 0));
    }
  };

  const handleSeeMore = () => {
    if (route) router.push(route);
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      handleSeeMore();
    }
  };

  return (
    <div className="carousel-wrapper">
      {currentIndex > 0 && (
        <button className="carousel-button prev" onClick={handlePrev}>
          &#8249;
        </button>
      )}
      <div className="carousel">
        {/* Renderizar items normales */}
        {visibleItems.map((item, index) => {
          const detailPath = getDetailPath(item);
          const title = item.nombre || item.Nombre || item.title;
          const dateValue = item.fecha || item.Fecha || item.date;
          
          // Para beneficios
          const tipo = item.Tipo ?? item.tipo;
          const cupos = item.Cupos ?? item.cupos;
          const fechaVencimiento = item.FechaDeVencimiento ?? item.fechaDeVencimiento;
          const vigencia = item.Vigencia ?? item.vigencia;
          const isBeneficio = tipo !== undefined;
          
          const handleItemKeyDown = (e) => {
            if (!detailPath) return;
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              goToDetail(item);
            }
          };
          return (
            <div
              key={item.id || item.Id || `item-${currentIndex + index}`}
              className={`carousel-item${detailPath ? ' clickable' : ''}`}
              tabIndex={0}
              role={detailPath ? 'button' : undefined}
              onClick={() => detailPath && goToDetail(item)}
              onKeyDown={handleItemKeyDown}
              aria-label={detailPath ? `Ver detalle de ${title ?? 'item'}` : undefined}
            >
              {isBeneficio ? (
                <>
                  <h3>Beneficio #{item.id || item.Id}</h3>
                  <p style={{ fontSize: '1rem', fontWeight: '600', margin: '8px 0' }}>
                    {(() => {
                      if (tipo == 0 || tipo === 'Canje' || (typeof tipo === 'string' && tipo.toLowerCase() === 'canje')) return 'Canje';
                      if (tipo == 1 || tipo === 'Consumo' || (typeof tipo === 'string' && tipo.toLowerCase() === 'consumo')) return 'Consumo';
                      return 'Desconocido';
                    })()}
                  </p>
                  {fechaVencimiento && (
                    <p style={{ fontSize: '0.85rem' }}>
                      Vence: {new Date(fechaVencimiento).toLocaleDateString('es-ES', { day: 'numeric', month: 'short', year: 'numeric' })}
                    </p>
                  )}
                  <p style={{ fontSize: '0.85rem' }}>Cupos: {cupos}</p>
                  <p style={{ fontSize: '0.8rem', marginTop: '4px', opacity: 0.9 }}>
                    {vigencia ? '✓ Vigente' : '✗ No vigente'}
                  </p>
                </>
              ) : (
                <>
                  {title && <h3>{title}</h3>}
                  {dateValue && (
                    <p>
                      {new Date(dateValue).toLocaleDateString('es-ES', { 
                        year: 'numeric', 
                        month: 'long', 
                        day: 'numeric' 
                      })}
                    </p>
                  )}
                </>
              )}
            </div>
          );
        })}
        
        {/* Siempre mostrar tarjeta "Ver más" si hay ruta */}
        {route && (
          <div
            key="see-more"
            className="carousel-item see-more"
            role="button"
            tabIndex={0}
            onClick={handleSeeMore}
            onKeyDown={handleKeyDown}
            aria-label='Ver más'
          >
            <div className="see-more-top">Ver más</div>
            <div className="see-more-arrow">&#8250;</div>
          </div>
        )}
      </div>

      {/* Mostrar botón siguiente solo si hay items ocultos después de los visibles */}
      {hasMoreItems && (
        <button className="carousel-button next" onClick={handleNext}>
          &#8250;
        </button>
      )}

      <style jsx>{`
        .carousel-wrapper {
          width: 100%;
          max-width: 100%;
          display: flex;
          align-items: center;
          position: relative;
          overflow: hidden;
          /* Reserve horizontal space so the arrow buttons don't overlap the items */
          padding: 0 clamp(12px, 5vw, 60px);
          box-sizing: border-box;
        }

        .carousel {
          display: flex;
          gap: 1vw;
          padding: 12px 0;
          /* Center the visible items so left/right spacing is equal */
          justify-content: center;
          border-radius: 8px;
          color: #231F20;
          scroll-behavior: smooth;
          transition: transform 0.5s ease-in-out;
          width: 100%;
          box-sizing: border-box;
          align-items: stretch;
        }

        .carousel-item {
          /* Adaptable layout: show 4 or 5 items depending on content */
          flex: 0 0 calc((100% - ${visibleItems.length}vw) / ${visibleItems.length + 1});
          max-width: 360px;
          min-width: 120px;
          /* Use aspect-ratio instead of fixed height so height scales with width */
          aspect-ratio: 9 / 13;
          height: auto;
          padding: 20px;
          background: #f37426;
          border-radius: 20px;
          box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
          transition: transform 0.2s ease, box-shadow 0.2s ease;
          outline: none; /* remove default focus outline */
          box-sizing: border-box;
        }

        .carousel-item.clickable {
          cursor: pointer;
        }

        .carousel-item:hover {
          transform: translateY(-4px) scale(1.02);
          box-shadow: 0 6px 12px rgba(0, 0, 0, 0.18);
          z-index: 1;
        }

        /* Keyboard focus (accessibility) */
        .carousel-item:focus,
        .carousel-item:focus-visible {
          transform: translateY(-4px) scale(1.02);
          box-shadow: 0 8px 18px rgba(0, 0, 0, 0.22);
          border: 2px solid rgba(37,99,235,0.15);
          z-index: 2;
        }

        .carousel-item h3 {
          margin: 0 0 8px;
          /* Fluid font size: small on phones, larger on desktop */
          font-size: clamp(1rem, 1.6vw, 1.25rem);
          line-height: 1.2;
          margin-bottom: 0.5rem;
          /* Limit title to two lines and gracefully truncate */
          display: -webkit-box;
          -webkit-line-clamp: 2;
          -webkit-box-orient: vertical;
          overflow: hidden;
          text-overflow: ellipsis;
        }

        .carousel-item p {
          margin: 0;
          color: #231F20;
          font-size: clamp(0.85rem, 1.2vw, 1rem);
          line-height: 1.35;
          /* Allow paragraphs to take up to 3 lines then truncate */
          display: -webkit-box;
          -webkit-line-clamp: 3;
          -webkit-box-orient: vertical;
          overflow: hidden;
          text-overflow: ellipsis;
        }

        .carousel-button {
          background: rgba(255, 255, 255, 0.85);
          border: none;
          font-size: 1.4rem;
          cursor: pointer;
          position: absolute;
          top: 50%;
          transform: translateY(-50%);
          z-index: 3;
          width: 2.292vw;
          height: 2.292vw;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          box-shadow: 0 2px 6px rgba(0,0,0,0.15);
        }

        /* place buttons inside the reserved padding area so they don't cover items */
        .carousel-button.prev {
          left: 8px;
        }

        .carousel-button.next {
          right: 8px;
        }

        .carousel-button:disabled {
          opacity: 0.5;
          cursor: not-allowed;
        }

        /* Special "Ver más" card styling (matches orange items) */
        .carousel-item.see-more {
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          text-align: center;
          cursor: pointer;
          background: rgba(255, 255, 255, 0.85);
        }

        .see-more-top {
          font-size: clamp(0.9rem, 1.6vw, 1.05rem);
          font-weight: 700;
          margin-bottom: 8px;
          color: #231F20;
        }

        .see-more-arrow {
          font-size: clamp(1.8rem, 3.2vw, 2.6rem);
          color: #231F20;
          transition: transform 0.18s ease;
          line-height: 1;
        }

        .carousel-item.see-more:hover .see-more-arrow {
          transform: translateY(-4px) scale(1.04);
        }

        /* PHONE: <= 425px - adaptable items */
        @media (max-width: 425px) {
          .carousel {
            gap: 1vw;
            padding: 6px 0;
          }

          .carousel-item {
            flex: 0 0 calc((100% - ${visibleItems.length + 1}vw) / ${visibleItems.length + 1});
            max-width: none;
            min-width: 16vw;
            aspect-ratio: 9 / 13;
            padding: 8px;
            border-radius: 12px;
          }

          /* Reduce text sizes more aggressively on phones */
          .carousel-item h3 {
            font-size: clamp(0.8rem, 2.2vw, 1rem);
            -webkit-line-clamp: 2;
          }

          .carousel-item p {
            font-size: clamp(0.7rem, 1.8vw, 0.85rem);
            -webkit-line-clamp: 2;
          }

          .see-more-top { font-size: clamp(0.75rem, 1.6vw, 0.9rem); }
          .see-more-arrow { font-size: clamp(1.6rem, 2.6vw, 1.9rem); }

          .carousel-button {
            width: 32px;
            height: 32px;
            font-size: 0.95rem;
          }
        }

        /* TABLET: 426px - 768px */
        @media (min-width: 426px) and (max-width: 768px) {
          .carousel {
            gap: 1vw;
            padding: 10px 0;
          }

          .carousel-item {
            flex: 0 0 calc((100% - ${visibleItems.length}vw) / ${visibleItems.length + 1});
            max-width: 300px;
            min-width: 140px;
            aspect-ratio: 9 / 13;
            padding: 16px;
            border-radius: 18px;
          }

          .carousel-button {
            width: 42px;
            height: 42px;
            font-size: 1.1rem;
          }
        }

        /* DESKTOP: >= 769px - adaptable proportions */
        @media (min-width: 769px) {
          .carousel-item {
            flex: 0 0 calc((100% - ${visibleItems.length}vw) / ${visibleItems.length + 1});
            max-width: 360px;
            min-width: 220px;
            aspect-ratio: 9 / 13;
            padding: 20px;
          }

          .carousel-button {
            width: 2.292vw;
            height: 2.292vw;
            font-size: 1.4rem;
          }
        }
      `}</style>
    </div>
  );
};

Carousel.propTypes = {
  items: PropTypes.arrayOf(
    PropTypes.shape({
      title: PropTypes.string,
      date: PropTypes.string,
    })
  ).isRequired,
  route: PropTypes.string,
};

export default Carousel;