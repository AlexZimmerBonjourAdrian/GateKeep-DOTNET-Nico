'use client';

import React from 'react';
import { useRouter } from 'next/navigation';
import Header from '../../components/Header';

export default function OfflinePage() {
  const router = useRouter();
  const [isOnline, setIsOnline] = React.useState(false);

  React.useEffect(() => {
    const checkOnline = () => {
      setIsOnline(navigator.onLine);
    };

    checkOnline();
    window.addEventListener('online', checkOnline);
    window.addEventListener('offline', checkOnline);

    return () => {
      window.removeEventListener('online', checkOnline);
      window.removeEventListener('offline', checkOnline);
    };
  }, []);

  const handleRetry = () => {
    if (navigator.onLine) {
      router.refresh();
      router.push('/');
    } else {
      alert('Aún no hay conexión a internet. Por favor, verifica tu conexión.');
    }
  };

  return (
    <>
      <Header />
      <div style={styles.container}>
        <div style={styles.card}>
          <div style={styles.icon}>📡</div>
          <h1 style={styles.title}>Sin conexión a internet</h1>
          <p style={styles.message}>
            No se pudo cargar esta página porque no hay conexión a internet.
          </p>
          <p style={styles.submessage}>
            Algunas funcionalidades están disponibles offline. Los cambios que realices se guardarán y se sincronizarán cuando vuelva la conexión.
          </p>
          <div style={styles.actions}>
            <button onClick={handleRetry} style={styles.button}>
              Reintentar
            </button>
            <button onClick={() => router.push('/')} style={styles.buttonSecondary}>
              Ir al inicio
            </button>
          </div>
          {!isOnline && (
            <div style={styles.status}>
              <span style={styles.statusDot}></span>
              Sin conexión
            </div>
          )}
        </div>
      </div>
    </>
  );
}

const styles = {
  container: {
    minHeight: 'calc(100vh - 80px)',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    padding: '20px',
    backgroundColor: '#f5f5f5',
  },
  card: {
    backgroundColor: '#fff',
    borderRadius: '12px',
    padding: '40px',
    maxWidth: '500px',
    width: '100%',
    textAlign: 'center',
    boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)',
  },
  icon: {
    fontSize: '64px',
    marginBottom: '20px',
  },
  title: {
    fontSize: '24px',
    fontWeight: 'bold',
    marginBottom: '16px',
    color: '#333',
  },
  message: {
    fontSize: '16px',
    color: '#666',
    marginBottom: '12px',
    lineHeight: '1.5',
  },
  submessage: {
    fontSize: '14px',
    color: '#888',
    marginBottom: '24px',
    lineHeight: '1.5',
  },
  actions: {
    display: 'flex',
    gap: '12px',
    justifyContent: 'center',
    marginBottom: '20px',
  },
  button: {
    padding: '12px 24px',
    backgroundColor: '#0066cc',
    color: '#fff',
    border: 'none',
    borderRadius: '6px',
    fontSize: '16px',
    fontWeight: '500',
    cursor: 'pointer',
    transition: 'background-color 0.2s',
  },
  buttonSecondary: {
    padding: '12px 24px',
    backgroundColor: '#f0f0f0',
    color: '#333',
    border: 'none',
    borderRadius: '6px',
    fontSize: '16px',
    fontWeight: '500',
    cursor: 'pointer',
    transition: 'background-color 0.2s',
  },
  status: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    gap: '8px',
    fontSize: '14px',
    color: '#d32f2f',
    marginTop: '20px',
  },
  statusDot: {
    width: '8px',
    height: '8px',
    borderRadius: '50%',
    backgroundColor: '#d32f2f',
    display: 'inline-block',
  },
};

