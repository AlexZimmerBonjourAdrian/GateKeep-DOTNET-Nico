"use client"

import React, { useState } from 'react';
import { usePathname } from 'next/navigation';
import Header from '../../components/Header';
import { SecurityService } from '../../services/securityService';

export default function listadoEventos() {

  const pathname = usePathname();
  const isAuthenticated = SecurityService.checkAuthAndRedirect(pathname);

  if (isAuthenticated){
    
  }

  const initial = [
    { id: 1, subject: 'Hola amigo', date: '2024-07-01', visto: false, mensaje: 'You have a new notification for the Hockey Game.' },
    { id: 2, subject: 'Hey', date: '2024-07-05', visto: false, mensaje: 'You have a new notification for the Soccer Match.You have a new notification for the Soccer MatchYou have a new notification for the Soccer MatchYou have a new notification for the Soccer MatchYou have a new notification for the Soccer MatchYou have a new notification for the Soccer MatchYou have a new notification for the Soccer MatchYou have a new notification for the Soccer MatchYou have a new notification for the Soccer Match' },
    { id: 3, subject: 'Hi', date: '2024-07-10', visto: false, mensaje: 'You have a new notification for the Basketball Tournament.' },
    { id: 4, subject: 'Todo bien?', date: '2024-07-15', visto: false, mensaje: 'You have a new notification for the Tennis Finals.' },
    { id: 5, subject: 'Que tal?', date: '2024-07-20', visto: true, mensaje: 'You have a new notification for the Baseball Series.' },      { id: 6, subject: 'Saludos', date: '2024-07-25', visto: true, mensaje: 'You have a new notification for the Swimming Championship.' },
    { id: 7, subject: 'Buenas', date: '2024-07-30', visto: false, mensaje: 'You have a new notification for the Marathon Event.' },
  ];

    const [notificaciones, setNotificaciones] = useState(initial);
    const [openId, setOpenId] = useState(null);

  function toggleOpen(id) {
    // Use functional update so we can derive whether we're opening
    setOpenId(prev => {
      const willOpen = prev !== id;
      if (willOpen) {
        // mark as visto only when opening
        setNotificaciones(prevList => prevList.map(n => (n.id === id && !n.visto) ? { ...n, visto: true } : n));
      }
      return willOpen ? id : null;
    });
  }

    return (
        <div className="container-nothing">
            <Header />

            <div className="list-wrap">
                  {notificaciones.map(n => {
                    const isOpen = openId === n.id;
                    return (
                      <div className={`notification ${isOpen ? 'open' : ''}`} key={n.id}>
                        <button className="content-btn" onClick={() => toggleOpen(n.id)} aria-expanded={isOpen}>
                          <div className="left">
                              <span className={`new-badge ${n.visto ? 'seen' : 'unseen'}`} aria-hidden="true">Nuevo</span>
                            <div className="text">
                              <div className="subject">{n.subject}</div>
                              <div className={`mensaje ${isOpen ? 'visible' : ''}`}>{n.mensaje}</div>
                            </div>
                          </div>
                          <div className="right">
                            <div className="date">{n.date}</div>
                          </div>
                        </button>
                      </div>
                    )
                  })}
                </div>

      <style jsx>{`
        .container-nothing {
          margin: 0;
          width: 100%;
          min-height: 100%;
          box-sizing: border-box;
        }

        .list-wrap {
          padding: 20px; /* espacio exterior solicitado */
          width: 100%;
        }

        .notification {
          display: block;
          background: #F37426; /* color solicitado */
          color: #fff;
          width: 100%; /* ocupa todo el ancho disponible dentro del padding */
          margin-bottom: 10px;
          border-radius: 8px;
          box-sizing: border-box;
        }

        .content-btn {
          display: flex;
          justify-content: space-between;
          align-items: flex-start;
          gap: 12px;
          width: 100%;
          background: transparent;
          color: inherit;
          padding: 12px;
          border-radius: 8px;
          border: none;
          text-align: left;
          cursor: pointer;
          box-sizing: border-box;
          transition: box-shadow 180ms ease;
        }
        .content-btn:focus { outline: none; box-shadow: 0 0 0 3px rgba(243,116,38,0.18); }

        .left {
          display: flex;
          align-items: center;
          gap: 12px;
          max-width: 75%;
        }

        .indicator {
          width: 12px;
          height: 12px;
          border-radius: 50%;
          flex: 0 0 12px;
          box-shadow: 0 0 0 2px rgba(255,255,255,0.15) inset;
        }
        /* Hide the dot when the notification is already seen */
        .indicator.seen {
          display: none;
        }
        /* Show a red dot for not-seen notifications */
        .indicator.unseen {
          background: #F62D2D; /* rojo para no visto */
        }
        
        .new-badge {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            padding: 3px 8px;
            border-radius: 999px;
            font-size: 12px;
            font-weight: 700;
            line-height: 1.2;
            margin-top: 2px;
        }
          
        .new-badge.seen { display: none; }

         
        .new-badge.unseen {
            background: #F62D2D; /* rojo */
            color: #fff;
            box-shadow: 0 1px 0 rgba(0,0,0,0.06) inset;
        }

        .text {
          overflow: hidden;
        }

        .subject {
          font-weight: 700;
          font-size: 16px;
          white-space: nowrap;
          text-overflow: ellipsis;
          overflow: hidden;
        }

        /* mensaje oculto por defecto: colapsado y opaco. Al abrir se revela con transición. */
        .mensaje {
          font-size: 13px;
          color: rgba(255,255,255,0.95);
          margin-top: 0;
          max-height: 0;
          opacity: 0;
          overflow: hidden;
          transition: max-height 300ms ease, opacity 200ms ease, margin-top 200ms ease;
          white-space: normal;
        }
        .mensaje.visible {
          max-height: 200px; /* suficiente para el contenido */
          opacity: 1;
          margin-top: 8px;
        }

        .right {
          display: flex;
          flex-direction: column;
          align-items: flex-end;
          gap: 6px;
        }

        .date {
          font-size: 12px;
          opacity: 0.95;
        }

        .badge {
          font-size: 12px;
          padding: 4px 8px;
          border-radius: 999px;
          color: #000;
          background: rgba(255,255,255,0.85);
        }

        /* Responsive rules: desktop (>=769), tablet (426-768), mobile (<=425) */
        /* Desktop: default styles already tuned for desktop >=769px */
        @media (min-width: 769px) {
          .list-wrap {
            padding: 20px;
          }

          .content-btn {
            padding: 14px 16px; gap: 16px;
          }

          .subject {
            font-size: 16px;
          }

          .mensaje {
            font-size: 13px;
          }

          .new-badge.unseen {
            padding: 4px 10px; font-size: 12px;
          }

          .right {
            gap: 8px;
          }
        }

        /* Tablet: medium screens */
        @media (min-width: 426px) and (max-width: 768px) {
          .list-wrap {
            padding: 16px;
          }

          .content-btn {
            padding: 12px;
            gap: 12px;
          }

          .subject {
            font-size: 15px;
          }

          .mensaje {
            font-size: 13px;
          }

          .new-badge.unseen {
            padding: 3px 8px;
            font-size: 11px;
          }

          .left {
            max-width: 70%;
          }
          .right { gap: 6px; }
        }

        /* Mobile: small screens, stack content to keep proportions and readability */
        @media (max-width: 425px) {
        
          .list-wrap {
            padding: 12px;
            padding-bottom: 100px;
          }

          .content-btn {
            flex-direction: column;
            align-items: stretch;
            padding: 10px;
            gap: 10px;
          }

          .left {
            display: flex;
            align-items: flex-start;
            gap: 10px;
            max-width: 100%;
          }

          .text {
            width: calc(100% - 80px);
          }

          .subject {
            font-size: 14px;
            white-space: normal;
            overflow: visible;
          }
            
          .mensaje {
            font-size: 13px;
          }

          .right {
            flex-direction: row;
            justify-content: space-between;
            align-items: center;
            gap: 8px;
          }

          .date {
            font-size: 12px;
          }

          .new-badge.unseen {
            padding: 3px 8px;
            font-size: 11px;
          }
        }

      `}</style>
    </div>
  );
}

