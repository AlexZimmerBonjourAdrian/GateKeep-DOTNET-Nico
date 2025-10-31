"use client";

import React, { useState } from 'react';
import PropTypes from 'prop-types';
import { useRouter } from 'next/navigation';

const Carousel = ({ items, route }) => {
  const [currentIndex, setCurrentIndex] = useState(0);
  const router = useRouter();
  const itemsPerPage = 4;
  const maxIndex = Math.min(items.length, 9) - itemsPerPage;

  const handleNext = () => {
    if (currentIndex + itemsPerPage <= maxIndex) {
      setCurrentIndex(currentIndex + itemsPerPage);
    }
  };

  const handlePrev = () => {
    if (currentIndex - itemsPerPage >= 0) {
      setCurrentIndex(currentIndex - itemsPerPage);
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
        {items.slice(currentIndex, currentIndex + itemsPerPage).map((item, index) => {
          const globalIndex = currentIndex + index;

          // If there are more than 8 items, render a special 9th "Ver más" card
          if (items.length > 7 && globalIndex === 7) {
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
            );
          }

          return (
            <div key={index} className="carousel-item" tabIndex={0}>
              {item.title && <h3>{item.title}</h3>}
              {item.date && <p>{item.date}</p>}
            </div>
          );
        })}
      </div>

      {/* Only show next if there are more pages within the capped 9-item window.
          Also ensure the right arrow doesn't appear when the special 9th 'Ver más' is present on the page. */}
      {currentIndex + itemsPerPage < Math.min(items.length, 9) && !(items.length > 8 && currentIndex + itemsPerPage >= 9) && (
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
          gap: 2vw;
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
          flex: 0 0 calc((100% - 4vw) / 4);
          max-width: 360px;
          min-width: 220px;
          height: 500px;
          padding: 20px;
          background: #f37426;
          border-radius: 20px;
          box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
          transition: transform 0.2s ease, box-shadow 0.2s ease;
          outline: none; /* remove default focus outline */
          box-sizing: border-box;
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
        }

        .carousel-item p {
          margin: 0;
          color: #231F20;
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
          font-size: 1rem;
          font-weight: 700;
          margin-bottom: 8px;
          color: #231F20;
        }

        .see-more-arrow {
          font-size: 2.6rem;
          color: #231F20;
          transition: transform 0.18s ease;
          line-height: 1;
        }

        .carousel-item.see-more:hover .see-more-arrow {
          transform: translateY(-4px) scale(1.04);
        }

        /* Smaller screens: reduce item size so layout stays usable */
        @media (max-width: 640px) {
          .carousel-item {
            /* On small screens show one card almost full width */
            flex: 0 0 80vw;
            max-width: 80vw;
            min-width: 60vw;
            height: 360px;
          }

          .carousel-button {
            width: 40px;
            height: 40px;
            font-size: 1.2rem;
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