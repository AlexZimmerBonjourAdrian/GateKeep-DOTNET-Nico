'use client';

/**
* Componente de Estado de Sincronización
* Muestra estado offline/online y eventos pendientes
*/

'use client';

import React, { useEffect, useState } from 'react';
import { isOnline, getDeviceId } from '../lib/sync';
import { getOfflineStatus } from '../lib/sqlite-db';

export default function SyncStatus() {
  const [online, setOnline] = useState(true);
  const [offlineStatus, setOfflineStatus] = useState(getOfflineStatus());
  const [showDetails, setShowDetails] = useState(false);

  useEffect(() => {
    const updateOfflineStatus = () => {
      setOfflineStatus(getOfflineStatus());
    };

    setOnline(isOnline());
    updateOfflineStatus();

    // Setup listeners
    const handleOnline = () => {
      console.log('🌐 Online');
      setOnline(true);
      updateOfflineStatus();
    };

    const handleOffline = () => {
      console.log('📡 Offline');
      setOnline(false);
      updateOfflineStatus();
    };

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    // Actualizar estado periódicamente
    const interval = setInterval(() => {
      updateOfflineStatus();
    }, 5000); // Actualizar cada 5 segundos

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
      clearInterval(interval);
    };
  }, []);

  return (
    <div className="sync-status-container" data-cy="sync-status">
      <style jsx>{`
        .sync-status-container {
          position: fixed;
          bottom: 20px;
          right: 20px;
          z-index: 1000;
          font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen,
            Ubuntu, Cantarell, sans-serif;
        }

        .sync-badge {
          display: flex;
          align-items: center;
          gap: 8px;
          padding: 12px 16px;
          border-radius: 8px;
          font-size: 14px;
          font-weight: 500;
          cursor: pointer;
          transition: all 0.3s ease;
          box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
        }

        .sync-badge.online {
          background-color: #4caf50;
          color: white;
        }

        .sync-badge.offline {
          background-color: #ff9800;
          color: white;
        }

        .sync-badge:hover {
          transform: translateY(-2px);
          box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }

        .status-dot {
          width: 8px;
          height: 8px;
          border-radius: 50%;
          background-color: currentColor;
          animation: pulse 2s infinite;
        }

        @keyframes pulse {
          0% {
            opacity: 1;
          }
          50% {
            opacity: 0.5;
          }
          100% {
            opacity: 1;
          }
        }

        .sync-details {
          position: absolute;
          bottom: 100%;
          right: 0;
          margin-bottom: 10px;
          background-color: white;
          border-radius: 8px;
          box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
          padding: 16px;
          min-width: 250px;
          font-size: 13px;
          color: #333;
        }

        .sync-details h4 {
          margin: 0 0 12px 0;
          font-size: 14px;
          font-weight: 600;
        }

        .sync-detail-row {
          display: flex;
          justify-content: space-between;
          margin: 8px 0;
          padding: 4px 0;
        }

        .sync-detail-label {
          font-weight: 500;
          color: #666;
        }

        .sync-detail-value {
          color: #333;
          font-weight: 600;
        }

        .status-badge {
          display: inline-block;
          padding: 4px 8px;
          border-radius: 4px;
          font-size: 11px;
          font-weight: 600;
          margin-top: 8px;
        }

        .status-badge.synced {
          background-color: #e8f5e9;
          color: #2e7d32;
        }

        .status-badge.pending {
          background-color: #fff3e0;
          color: #e65100;
        }
      `}</style>

      <div
        className={`sync-badge ${online ? 'online' : 'offline'}`}
        data-cy="sync-badge"
        role="status"
        aria-live="polite"
        onClick={() => setShowDetails(!showDetails)}
      >
        <span className="status-dot" />
        {online ? '🌐 Online' : '📡 Offline'}
        {offlineStatus?.eventosOfflinePendientes > 0 && (
          <span style={{ marginLeft: '8px', fontWeight: 'bold' }}>
            {offlineStatus.eventosOfflinePendientes} pendiente{offlineStatus.eventosOfflinePendientes !== 1 ? 's' : ''}
          </span>
        )}
      </div>

      {showDetails && offlineStatus && (
        <div className="sync-details">
          <h4>Estado de Sincronización</h4>
          <div className="sync-detail-row">
            <span className="sync-detail-label">Dispositivo:</span>
            <span className="sync-detail-value">{getDeviceId().split('-')[0]}...</span>
          </div>
          <div className="sync-detail-row">
            <span className="sync-detail-label">Estado:</span>
            <span className="sync-detail-value">{online ? '✅ Online' : '⚠️ Offline'}</span>
          </div>
          <div className="sync-detail-row">
            <span className="sync-detail-label">Eventos pendientes:</span>
            <span className="sync-detail-value">{offlineStatus.eventosOfflinePendientes}</span>
          </div>
          <div className="sync-detail-row">
            <span className="sync-detail-label">Eventos totales:</span>
            <span className="sync-detail-value">{offlineStatus.totalEventosOffline}</span>
          </div>
          <div className="sync-detail-row">
            <span className="sync-detail-label">Datos en caché:</span>
            <span className="sync-detail-value">{offlineStatus.usuariosEnCache} usuarios</span>
          </div>
          {offlineStatus.ultimaSincronizacion && (
            <div className="sync-detail-row">
              <span className="sync-detail-label">Última sync:</span>
              <span className="sync-detail-value">
                {new Date(offlineStatus.ultimaSincronizacion).toLocaleTimeString()}
              </span>
            </div>
          )}

          <div style={{ marginTop: '12px' }}>
            {offlineStatus.eventosOfflinePendientes > 0 ? (
              <div className="status-badge pending">⏳ Pendiente de sincronización</div>
            ) : (
              <div className="status-badge synced">✅ Todo sincronizado</div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
