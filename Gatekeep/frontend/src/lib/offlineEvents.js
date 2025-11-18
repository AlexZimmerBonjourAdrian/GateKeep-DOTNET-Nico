/**
 * Ejemplo: Cómo registrar un evento de acceso offline
 */

import { recordEvent } from '@/lib/sync';

export async function handleAccesoOfline(espacioId, usuarioId) {
  // Registrar evento localmente para sincronizar después
  const idTemporal = await recordEvent('Acceso', {
    espacioId,
    usuarioId,
    resultado: 'Permitido',
    motivo: 'Verificación offline',
    timestamp: new Date().toISOString(),
  });

  console.log(`✅ Acceso registrado offline: ${idTemporal}`);

  // Mostrar al usuario que fue registrado para sincronizar luego
  return {
    success: true,
    message: 'Acceso registrado. Se sincronizará cuando se recupere conexión.',
    idTemporal,
  };
}

/**
 * Ejemplo: Cómo registrar un evento de beneficio offline
 */
export async function handleBeneficioOfline(beneficioId, usuarioId) {
  const idTemporal = await recordEvent('Beneficio', {
    beneficioId,
    usuarioId,
    accion: 'Canje',
    timestamp: new Date().toISOString(),
  });

  console.log(`✅ Beneficio registrado offline: ${idTemporal}`);
  return idTemporal;
}

/**
 * Ejemplo: Cómo registrar un evento de notificación offline
 */
export async function handleNotificacionOfline(tipo, datos) {
  const idTemporal = await recordEvent('Notificacion', {
    tipo,
    ...datos,
    timestamp: new Date().toISOString(),
  });

  console.log(`✅ Notificación registrada offline: ${idTemporal}`);
  return idTemporal;
}
