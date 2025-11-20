'use client';

import { useState, useEffect } from 'react';

export default function InstallPWA() {
  const [deferredPrompt, setDeferredPrompt] = useState(null);
  const [showInstallButton, setShowInstallButton] = useState(false);

  useEffect(() => {
    // Detectar si la app ya está instalada
    const isInstalled = window.matchMedia('(display-mode: standalone)').matches ||
                       window.navigator.standalone ||
                       document.referrer.includes('android-app://');

    if (isInstalled) {
      return; // Ya está instalada, no mostrar el botón
    }

    // Escuchar el evento beforeinstallprompt
    const handleBeforeInstallPrompt = (e) => {
      // Prevenir que el navegador muestre el prompt automático
      e.preventDefault();
      // Guardar el evento para usarlo después
      setDeferredPrompt(e);
      setShowInstallButton(true);
    };

    window.addEventListener('beforeinstallprompt', handleBeforeInstallPrompt);

    // Verificar si ya se puede instalar (iOS Safari)
    if (window.navigator.standalone === false) {
      setShowInstallButton(true);
    }

    return () => {
      window.removeEventListener('beforeinstallprompt', handleBeforeInstallPrompt);
    };
  }, []);

  const handleInstallClick = async () => {
    if (!deferredPrompt) {
      // iOS Safari - mostrar instrucciones
      alert(
        'Para instalar GateKeep en iOS:\n\n' +
        '1. Toca el botón de compartir (□↑)\n' +
        '2. Selecciona "Añadir a pantalla de inicio"\n' +
        '3. Toca "Añadir"'
      );
      return;
    }

    // Mostrar el prompt de instalación
    deferredPrompt.prompt();

    // Esperar a que el usuario responda
    const { outcome } = await deferredPrompt.userChoice;

    if (outcome === 'accepted') {
      console.log('Usuario aceptó instalar la PWA');
      setShowInstallButton(false);
    } else {
      console.log('Usuario rechazó instalar la PWA');
    }

    // Limpiar el prompt
    setDeferredPrompt(null);
  };

  if (!showInstallButton) {
    return null;
  }

  return (
    <div
      style={{
        position: 'fixed',
        bottom: '20px',
        right: '20px',
        zIndex: 1000,
        background: 'white',
        padding: '16px',
        borderRadius: '12px',
        boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
        maxWidth: '300px',
      }}
    >
      <div style={{ marginBottom: '12px' }}>
        <strong style={{ display: 'block', marginBottom: '4px', color: '#333' }}>
          📱 Instalar GateKeep
        </strong>
        <p style={{ fontSize: '14px', color: '#666', margin: 0 }}>
          Instala la app para acceso rápido y funcionamiento offline
        </p>
      </div>
      <div style={{ display: 'flex', gap: '8px' }}>
        <button
          onClick={handleInstallClick}
          style={{
            flex: 1,
            padding: '10px 16px',
            background: '#0066cc',
            color: 'white',
            border: 'none',
            borderRadius: '8px',
            cursor: 'pointer',
            fontWeight: '500',
            fontSize: '14px',
          }}
        >
          Instalar
        </button>
        <button
          onClick={() => setShowInstallButton(false)}
          style={{
            padding: '10px 16px',
            background: '#f0f0f0',
            color: '#666',
            border: 'none',
            borderRadius: '8px',
            cursor: 'pointer',
            fontSize: '14px',
          }}
        >
          ✕
        </button>
      </div>
    </div>
  );
}

