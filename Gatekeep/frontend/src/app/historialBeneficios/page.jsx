"use client"

import React, { useState, useEffect } from 'react';
import { usePathname } from 'next/navigation';
import Header from '../../components/Header';
import { SecurityService } from '../../services/securityService';
import { BeneficioService } from '../../services/BeneficioService';

export default function HistorialBeneficios() {
  const pathname = usePathname();
  
  useEffect(() => {
    SecurityService.checkAuthAndRedirect(pathname);
  }, [pathname]);

  const [beneficios, setBeneficios] = useState([]);
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

  // Cargar beneficios del usuario
  useEffect(() => {
    if (!usuarioId) return;
    
    const fetchBeneficios = async (retryCount = 0) => {
      try {
        const response = await BeneficioService.getBeneficiosCanjeados(usuarioId);
        // Los beneficios ya vienen ordenados por fecha de canje (más reciente primero)
        setBeneficios(response.data || []);
      } catch (error) {
        console.error('Error al cargar beneficios canjeados:', error);
        if (retryCount < 2) {
          // Reintentar después de 1 segundo
          console.log(`Reintentando... (intento ${retryCount + 1}/2)`);
          setTimeout(() => fetchBeneficios(retryCount + 1), 1000);
        } else {
          setBeneficios([]);
        }
      } finally {
        if (retryCount === 0) setLoading(false);
      }
    };

    fetchBeneficios();
  }, [usuarioId]);

  function toggleOpen(beneficioId) {
    setOpenId(prev => prev === beneficioId ? null : beneficioId);
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
                            <div className="subject">Cargando historial...</div>
                          </div>
                        </div>
                      </div>
                    </div>
                  ) : beneficios.length === 0 ? (
                    <div className="notification">
                      <div className="content-btn" style={{cursor:'default'}}>
                        <div className="left">
                          <div className="text">
                            <div className="subject">No tienes beneficios canjeados</div>
                          </div>
                        </div>
                      </div>
                    </div>
                  ) : beneficios.map(b => {
                    const beneficioId = b.BeneficioId || b.beneficioId;
                    const tipo = b.Tipo || b.tipo;
                    const fechaCanje = b.FechaCanje || b.fechaCanje;
                    const fechaVencimiento = b.FechaDeVencimiento || b.fechaDeVencimiento;
                    const vigencia = b.Vigencia ?? b.vigencia ?? false;
                    const isOpen = openId === beneficioId;
                    
                    // El backend envía el nombre del enum como string
                    let tipoTexto = 'Desconocido';
                    if (tipo === 0 || tipo === 'Canje' || (typeof tipo === 'string' && tipo.toLowerCase() === 'canje')) {
                      tipoTexto = 'Canje';
                    } else if (tipo === 1 || tipo === 'Consumo' || (typeof tipo === 'string' && tipo.toLowerCase() === 'consumo')) {
                      tipoTexto = 'Consumo';
                    }
                    
                    const fechaCanjeFormateada = fechaCanje 
                      ? new Date(fechaCanje).toLocaleDateString('es-ES', { 
                          year: 'numeric', 
                          month: 'short', 
                          day: 'numeric',
                          hour: '2-digit',
                          minute: '2-digit'
                        })
                      : 'Fecha desconocida';

                    const fechaVencimientoFormateada = fechaVencimiento 
                      ? new Date(fechaVencimiento).toLocaleDateString('es-ES', { 
                          year: 'numeric', 
                          month: 'short', 
                          day: 'numeric'
                        })
                      : 'Sin fecha';
                    
                    // Determinar si el beneficio está vencido
                    const estaVencido = fechaVencimiento ? new Date(fechaVencimiento) < new Date() : false;
                    const estadoTexto = estaVencido ? 'Vencido' : (vigencia ? 'Vigente' : 'No vigente');
                    
                    return (
                      <div className={`notification ${isOpen ? 'open' : ''}`} key={beneficioId}>
                        <button className="content-btn" onClick={() => toggleOpen(beneficioId)} aria-expanded={isOpen}>
                          <div className="left">
                            <span className="new-badge seen" aria-hidden="true">Canjeado</span>
                            <div className="text">
                              <div className="subject">Beneficio #{beneficioId} - {tipoTexto}</div>
                              <div className={`mensaje ${isOpen ? 'visible' : ''}`}>
                                <p><strong>Fecha de canje:</strong> {fechaCanjeFormateada}</p>
                                <p><strong>Estado del beneficio:</strong> {estadoTexto}</p>
                                <p><strong>Fecha de vencimiento:</strong> {fechaVencimientoFormateada}</p>
                              </div>
                            </div>
                          </div>
                          <div className="right">
                            <div className="date">{fechaCanjeFormateada}</div>
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
          padding: 20px;
          width: 100%;
        }

        .notification {
          display: block;
          background: #F37426;
          color: #fff;
          width: 100%;
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

        .new-badge {
          font-size: 0.7rem;
          font-weight: 700;
          padding: 2px 8px;
          border-radius: 4px;
          text-transform: uppercase;
          letter-spacing: 0.02em;
          white-space: nowrap;
          transition: opacity 0.15s ease;
        }
        .new-badge.unseen {
          background: rgba(255,255,255,0.95);
          color: #F37426;
        }
        .new-badge.seen {
          background: rgba(255,255,255,0.3);
          color: rgba(255,255,255,0.85);
        }

        .text {
          display: flex;
          flex-direction: column;
          gap: 6px;
        }

        .subject {
          font-size: 0.95rem;
          font-weight: 600;
          margin: 0;
        }

        .mensaje {
          font-size: 0.85rem;
          line-height: 1.4;
          margin: 0;
          max-height: 0;
          overflow: hidden;
          opacity: 0;
          transition: max-height 0.3s ease, opacity 0.3s ease;
        }
        .mensaje.visible {
          max-height: 300px;
          opacity: 1;
        }
        .mensaje p {
          margin: 4px 0;
        }

        .right {
          display: flex;
          align-items: center;
          gap: 8px;
        }

        .date {
          font-size: 0.75rem;
          opacity: 0.9;
          white-space: nowrap;
        }
      `}</style>
        </div>
    )
}
