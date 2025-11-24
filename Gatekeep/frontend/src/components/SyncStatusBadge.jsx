'use client';

import { useState, useEffect } from 'react';
import { contarEventosPendientes, getOfflineStatus } from '@/lib/sqlite-db';
import { isOnline } from '@/lib/sync';

export function SyncStatusBadge() {
  const [pendingCount, setPendingCount] = useState(0);
  const [isOnlineState, setIsOnlineState] = useState(true);
  const [lastSync, setLastSync] = useState(null);
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    // Función para actualizar estado
    const updateStatus = () => {
      try {
        const count = contarEventosPendientes();
        const status = getOfflineStatus();
        const online = isOnline();
        
        setPendingCount(count);
        setIsOnlineState(online);
        setLastSync(status.ultimaSincronizacion);
        
        // Mostrar badge si hay eventos pendientes o está offline
        setIsVisible(count > 0 || !online);
      } catch (error) {
        console.error('Error actualizando estado de sincronización:', error);
      }
    };

    // Actualizar inmediatamente
    updateStatus();

    // Actualizar cada 5 segundos
    const interval = setInterval(updateStatus, 5000);

    // Escuchar cambios de conexión
    const handleOnline = () => {
      setIsOnlineState(true);
      updateStatus();
    };
    
    const handleOffline = () => {
      setIsOnlineState(false);
      setIsVisible(true);
    };
    
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      clearInterval(interval);
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);

  // No mostrar nada si no hay eventos pendientes y está online
  if (!isVisible) {
    return null;
  }

  return (
    <div className="sync-status-badge" style={styles.container}>
      {!isOnlineState && (
        <div className="offline-indicator" style={styles.offline}>
          Sin conexión
        </div>
      )}
      
      {pendingCount > 0 && (
        <div className="pending-events" style={styles.pending}>
          {pendingCount} evento{pendingCount > 1 ? 's' : ''} pendiente{pendingCount > 1 ? 's' : ''}
        </div>
      )}
      
      {lastSync && isOnlineState && (
        <div className="last-sync" style={styles.lastSync}>
          Última sincronización: {new Date(lastSync).toLocaleTimeString()}
        </div>
      )}
    </div>
  );
}

const styles = {
  container: {
    position: 'fixed',
    top: '10px',
    right: '10px',
    backgroundColor: '#fff',
    border: '1px solid #ddd',
    borderRadius: '8px',
    padding: '12px 16px',
    boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
    zIndex: 9999,
    fontSize: '14px',
    minWidth: '200px',
  },
  offline: {
    color: '#d32f2f',
    fontWeight: 'bold',
    marginBottom: '8px',
  },
  pending: {
    color: '#f57c00',
    fontWeight: '500',
    marginBottom: '4px',
  },
  lastSync: {
    color: '#666',
    fontSize: '12px',
    marginTop: '4px',
  },
};

