"use client"

import React, { useState, useEffect } from 'react';
import { usePathname } from 'next/navigation';
import Header from '../../components/Header';
import { SecurityService } from '../../services/securityService';
import { NotificacionService } from '../../services/NotificacionService';

export default function ListadoNotificaciones() {
  const pathname = usePathname();
  
  useEffect(() => {
    SecurityService.checkAuthAndRedirect(pathname);
  }, [pathname]);

  const [notificaciones, setNotificaciones] = useState([]);
  const [loading, setLoading] = useState(true);
  const [openId, setOpenId] = useState(null);
  const [usuarioId, setUsuarioId] = useState(null);

  // Obtener ID del usuario actual
  useEffect(() => {
    try {
      const id = SecurityService.getUserId();
      if (id) {
        setUsuarioId(parseInt(id, 10));
      }
    } catch (error) {
      console.error('Error obteniendo usuario:', error);
    }
  }, []);

  // Cargar notificaciones del usuario
  useEffect(() => {
    if (!usuarioId) return;
    
    const fetchNotificaciones = async (retryCount = 0) => {
      try {
        const data = await NotificacionService.getNotificacionesPorUsuario(usuarioId);
        // Ordenar por fecha más reciente primero
        const sorted = (data || []).sort((a, b) => {
          const fechaA = new Date(a.FechaCreacion || a.fechaCreacion || 0);
          const fechaB = new Date(b.FechaCreacion || b.fechaCreacion || 0);
          return fechaB - fechaA;
        });
        setNotificaciones(sorted);
      } catch (error) {
        console.error('Error al cargar notificaciones:', error);
        if (retryCount < 2) {
          // Reintentar después de 1 segundo
          console.log(`Reintentando... (intento ${retryCount + 1}/2)`);
          setTimeout(() => fetchNotificaciones(retryCount + 1), 1000);
        } else {
          setNotificaciones([]);
        }
      } finally {
        if (retryCount === 0) setLoading(false);
      }
    };

    fetchNotificaciones();
  }, [usuarioId]);

  async function toggleOpen(notificacion) {
    const notifId = notificacion.NotificacionId || notificacion.notificacionId;
    
    setOpenId(prev => {
      const willOpen = prev !== notifId;
      
      // Si se está abriendo y no está leída, marcar como leída
      if (willOpen && !(notificacion.Leida ?? notificacion.leida) && usuarioId) {
        NotificacionService.marcarComoLeida(usuarioId, notifId)
          .then(() => {
            // Actualizar estado local
            setNotificaciones(prevList => 
              prevList.map(n => {
                const nId = n.NotificacionId || n.notificacionId;
                return nId === notifId ? { ...n, Leida: true, leida: true } : n;
              })
            );
          })
          .catch(err => console.error('Error marcando como leída:', err));
      }
      
      return willOpen ? notifId : null;
    });
  }

    return (
        <div className="container-nothing">
            <Header />

            <div className="list-wrap">
                  {loading ? (
                    <div className="notification">
                      <div className="content-btn" style={{cursor:'default'}}>
                        <div className="left">
                          <div className="text">
                            <div className="subject">Cargando notificaciones...</div>
                          </div>
                        </div>
                      </div>
                    </div>
                  ) : notificaciones.length === 0 ? (
                    <div className="notification">
                      <div className="content-btn" style={{cursor:'default'}}>
                        <div className="left">
                          <div className="text">
                            <div className="subject">No tienes notificaciones</div>
                          </div>
                        </div>
                      </div>
                    </div>
                  ) : notificaciones.map(n => {
                    const notifId = n.NotificacionId || n.notificacionId;
                    const mensaje = n.Mensaje || n.mensaje || '';
                    const tipo = n.Tipo || n.tipo || 'General';
                    // Usar el primer campo de fecha disponible
                    const fechaCreacion = n.fecha;
                    // Asegurarse de que el valor sea booleano y considerar 'leido' (todo minúscula)
                    const leida = Boolean(n.Leida ?? n.leida ?? n.leido);
                    const isOpen = openId === notifId;

                    let fechaFormateada = '';
                    if (fechaCreacion) {
                      try {
                        fechaFormateada = new Date(fechaCreacion).toLocaleDateString('es-ES', {
                          year: 'numeric',
                          month: 'short',
                          day: 'numeric',
                          hour: '2-digit',
                          minute: '2-digit'
                        });
                      } catch {}
                    }

                    return (
                      <div className={`notification ${isOpen ? 'open' : ''}`} key={notifId}>
                        <button className="content-btn" onClick={() => toggleOpen(n)} aria-expanded={isOpen}>
                          <div className="left">
                            {/* Solo mostrar el badge si NO está leída */}
                            {!leida && (
                              <span className="new-badge unseen" aria-hidden="true">Nuevo</span>
                            )}
                            <div className="text">
                              <div className="subject">{tipo}</div>
                              <div className={`mensaje ${isOpen ? 'visible' : ''}`}>{mensaje}</div>
                            </div>
                          </div>
                          <div className="right">
                            {/* Solo mostrar la fecha si existe */}
                            {fechaFormateada && <div className="date">{fechaFormateada}</div>}
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

