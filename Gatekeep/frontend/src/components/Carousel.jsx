import React, { useState } from 'react';
import PropTypes from 'prop-types';

const Carousel = ({ items }) => {
  const [currentIndex, setCurrentIndex] = useState(0);
  const itemsPerPage = 3;
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
        {items.slice(currentIndex, currentIndex + itemsPerPage).map((item, index) => (
          <div key={index} className="carousel-item">
            {item.title && <h3>{item.title}</h3>}
            {item.date && <p>{item.date}</p>}
          </div>
        ))}
      </div>
      {currentIndex + itemsPerPage < Math.min(items.length, 9) && (
        <button className="carousel-button next" onClick={handleNext}>
          &#8250;
        </button>
      )}

      <style jsx>{`
        .carousel-wrapper {
          display: flex;
          align-items: center;
          position: relative;
          overflow: hidden;
        }

        .carousel {
          display: flex;
          gap: 4vw;
          padding: 10px;
          border-radius: 8px;
          color: #231F20;
          scroll-behavior: smooth;
          transition: transform 0.5s ease-in-out; /* Animación más suave */
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

        .carousel-item:hover {
          transform: scale(1.02);
          box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
          z-index: 1;
        }

        .carousel-item h3 {
          margin: 0 0 8px;
        }

        .carousel-item p {
          margin: 0;
          color: #231F20;
        }

        .carousel-button {
          background: none;
          border: none;
          font-size: 2rem;
          cursor: pointer;
          position: absolute;
          top: 50%;
          transform: translateY(-50%);
          z-index: 2;
        }

        .carousel-button.prev {
          left: 10px;
        }

        .carousel-button.next {
          right: 10px;
        }

        .carousel-button:disabled {
          opacity: 0.5;
          cursor: not-allowed;
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
};

export default Carousel;