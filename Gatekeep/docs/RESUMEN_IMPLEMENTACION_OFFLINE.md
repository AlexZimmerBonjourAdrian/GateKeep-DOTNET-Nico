# 📋 Resumen Rápido: Implementación Sistema Offline

## 🎯 Objetivo

Implementar un sistema completo que:
1. Guarde automáticamente las peticiones API cuando no hay conexión
2. Muestre el estado de sincronización al usuario
3. Cachee datos maestros periódicamente
4. Espere 2 minutos después de recuperar conexión antes de sincronizar

## 📁 Archivos a Crear

1. `frontend/src/lib/axios-offline-interceptor.ts`
2. `frontend/src/components/SyncStatusBadge.jsx`
3. `frontend/src/lib/master-data-sync.ts`

## 📝 Archivos a Modificar

1. `frontend/src/lib/sync.ts` - Agregar retraso de 2 minutos
2. `frontend/src/lib/SyncProvider.jsx` - Integrar todo
3. `frontend/src/app/layout.jsx` - Agregar badge
4. Todos los servicios en `frontend/src/services/` - Usar apiClient

## ⚡ Pasos Rápidos

### 1. Crear Interceptor (5 min)
```bash
# Crear archivo: frontend/src/lib/axios-offline-interceptor.ts
# Copiar código de IMPLEMENTACION_SISTEMA_OFFLINE.md - Paso 1
```

### 2. Modificar Sync (2 min)
```bash
# Modificar: frontend/src/lib/sync.ts
# Cambiar setupConnectivityListeners para esperar 2 minutos
```

### 3. Crear Master Data Sync (5 min)
```bash
# Crear archivo: frontend/src/lib/master-data-sync.ts
# Copiar código de IMPLEMENTACION_SISTEMA_OFFLINE.md - Paso 4
```

### 4. Actualizar SyncProvider (3 min)
```bash
# Modificar: frontend/src/lib/SyncProvider.jsx
# Agregar startMasterDataSync
```

### 5. Crear Badge (5 min)
```bash
# Crear archivo: frontend/src/components/SyncStatusBadge.jsx
# Copiar código de IMPLEMENTACION_SISTEMA_OFFLINE.md - Paso 3
```

### 6. Integrar Badge (1 min)
```bash
# Modificar: frontend/src/app/layout.jsx
# Agregar: <SyncStatusBadge />
```

### 7. Modificar Servicios (15-20 min)
```bash
# Para cada servicio en frontend/src/services/:
# 1. Reemplazar: import axios → import apiClient from '@/lib/axios-offline-interceptor'
# 2. Eliminar: const api = axios.create(...)
# 3. Reemplazar: api → apiClient en todos los métodos
```

## 🧪 Pruebas Rápidas

1. **Offline:** DevTools → Network → Offline → Crear algo → Ver badge
2. **Online:** Volver online → Esperar 2 min → Ver sincronización
3. **Cache:** Online 5 min → Offline → Verificar datos disponibles

## 📖 Documentación Completa

Ver: `docs/IMPLEMENTACION_SISTEMA_OFFLINE.md` para detalles completos.

## ⚠️ Importante

- El interceptor solo guarda POST/PUT/DELETE/PATCH (no GET)
- La sincronización espera 2 minutos después de recuperar conexión
- Los datos maestros se sincronizan cada 5 minutos cuando hay conexión
- El badge se muestra solo si hay eventos pendientes o está offline

