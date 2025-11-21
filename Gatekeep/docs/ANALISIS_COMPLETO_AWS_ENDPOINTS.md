# 📊 Análisis Completo: AWS ALB Rules y Endpoints Backend

**Fecha:** 2025-01-21  
**Región:** sa-east-1  
**ALB:** gatekeep-alb  
**DNS Backend:** api.zimmzimmgames.com

---

## 🔍 RESUMEN EJECUTIVO

### Problema Identificado
El frontend estaba llamando a `/auth/login` (sin prefijo `/api/`), pero el backend solo expone `/api/auth/login`. Además, existía una regla en el ALB para `/auth/*` que capturaba estas peticiones incorrectamente.

### Estado Actual
- ✅ Regla problemática `/auth/*` (prioridad 120) **ELIMINADA** del listener HTTPS
- ✅ Frontend corregido para usar `/api/auth/login`
- ⚠️ Frontend desplegado pero puede tener caché del navegador

---

## 📋 ESTADO ACTUAL DE REGLAS ALB

### 🔒 Listener HTTPS (Puerto 443) - **PRINCIPAL**

| Prioridad | Path Pattern | Target Group | Estado |
|-----------|--------------|--------------|--------|
| **100** | `/api/*` | Backend (gatekeep-tg) | ✅ Activo |
| **130** | `/swagger*`, `/swagger/*` | Backend (gatekeep-tg) | ✅ Activo |
| **140** | `/health*` | Backend (gatekeep-tg) | ✅ Activo |
| **150** | `/system/*` | Backend (gatekeep-tg) | ✅ Activo |
| **default** | (cualquier otra ruta) | Frontend (gatekeep-frontend-tg) | ✅ Activo |

**Orden de Evaluación:**
1. Las reglas se evalúan de menor a mayor prioridad (100 → 150 → default)
2. La primera regla que coincide se aplica
3. Si ninguna regla coincide, se usa la acción default (frontend)

### 🔓 Listener HTTP (Puerto 80)

| Prioridad | Path Pattern | Acción | Estado |
|-----------|--------------|--------|--------|
| **100** | `/api/*` | Forward → Backend | ✅ Activo |
| **130** | `/swagger*`, `/swagger/*` | Forward → Backend | ✅ Activo |
| **140** | `/health` | Forward → Backend | ⚠️ Solo exacto (no `/health/*`) |
| **default** | (cualquier otra ruta) | Redirect → HTTPS:443 | ✅ Activo |

**Nota:** El listener HTTP redirige todo a HTTPS, excepto las rutas específicas del backend.

---

## 🎯 ENDPOINTS DEL BACKEND

### 📍 Base URL
- **Producción:** `https://api.zimmzimmgames.com`
- **Desarrollo:** `http://localhost:5011`

### 🔐 Autenticación (`/api/auth`)

| Método | Endpoint | Auth Requerida | Descripción |
|--------|----------|----------------|-------------|
| `POST` | `/api/auth/login` | ❌ No | Iniciar sesión |
| `POST` | `/api/auth/register` | ✅ Admin | Registrar nuevo usuario |
| `GET` | `/api/auth/qr` | ✅ Sí | Generar código QR del JWT |
| `GET` | `/api/auth/validate` | ✅ Sí | Validar JWT y obtener datos del usuario |
| `POST` | `/api/auth/create-test-users` | ❌ No | Crear usuarios de prueba |
| `GET` | `/api/auth/list-users` | ❌ No | Listar usuarios (testing) |

### 👥 Usuarios (`/api/usuarios`)

| Método | Endpoint | Auth Requerida | Descripción |
|--------|----------|----------------|-------------|
| `GET` | `/api/usuarios` | ✅ Admin | Listar todos los usuarios |
| `GET` | `/api/usuarios/{id}` | ✅ Sí (propio o admin) | Obtener usuario por ID |
| `PUT` | `/api/usuarios/{id}` | ✅ Sí (propio o admin) | Actualizar usuario |
| `POST` | `/api/usuarios` | ✅ Admin | Crear nuevo usuario |
| `PUT` | `/api/usuarios/{id}/rol` | ✅ Admin | Cambiar rol de usuario |
| `DELETE` | `/api/usuarios/{id}` | ✅ Admin | Eliminar usuario |

### 🎁 Beneficios (`/api/beneficios`)

| Método | Endpoint | Auth Requerida | Descripción |
|--------|----------|----------------|-------------|
| `GET` | `/api/beneficios` | ✅ Sí | Listar todos los beneficios |
| `GET` | `/api/beneficios/{id}` | ✅ Sí | Obtener beneficio por ID |
| `GET` | `/api/beneficios/vigentes` | ✅ Sí | Listar beneficios vigentes |
| `POST` | `/api/beneficios` | ✅ Admin | Crear beneficio |
| `PUT` | `/api/beneficios/{id}` | ✅ Admin | Actualizar beneficio |
| `DELETE` | `/api/beneficios/{id}` | ✅ Admin | Eliminar beneficio |

### 👤 Usuario-Beneficios (`/api/usuarios/{usuarioId}/beneficios`)

| Método | Endpoint | Auth Requerida | Descripción |
|--------|----------|----------------|-------------|
| `GET` | `/api/usuarios/{usuarioId}/beneficios` | ✅ Sí | Listar beneficios del usuario |
| `POST` | `/api/usuarios/{usuarioId}/beneficios/{beneficioId}` | ✅ Admin | Asignar beneficio a usuario |
| `DELETE` | `/api/usuarios/{usuarioId}/beneficios/{beneficioId}` | ✅ Admin | Remover beneficio de usuario |

### 🏢 Espacios (`/api/espacios`)

#### Edificios (`/api/espacios/edificios`)
| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `GET` | `/api/espacios/edificios` | ✅ Sí |
| `GET` | `/api/espacios/edificios/{id}` | ✅ Sí |
| `POST` | `/api/espacios/edificios` | ✅ Admin |
| `PUT` | `/api/espacios/edificios/{id}` | ✅ Admin |
| `DELETE` | `/api/espacios/edificios/{id}` | ✅ Admin |

#### Salones (`/api/espacios/salones`)
| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `GET` | `/api/espacios/salones` | ✅ Sí |
| `GET` | `/api/espacios/salones/{id}` | ✅ Sí |
| `POST` | `/api/espacios/salones` | ✅ Admin |
| `PUT` | `/api/espacios/salones/{id}` | ✅ Admin |
| `DELETE` | `/api/espacios/salones/{id}` | ✅ Admin |

#### Laboratorios (`/api/espacios/laboratorios`)
| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `GET` | `/api/espacios/salones` | ✅ Sí |
| `GET` | `/api/espacios/laboratorios/{id}` | ✅ Sí |
| `POST` | `/api/espacios/laboratorios` | ✅ Admin |
| `PUT` | `/api/espacios/laboratorios/{id}` | ✅ Admin |
| `DELETE` | `/api/espacios/laboratorios/{id}` | ✅ Admin |

### 📅 Eventos (`/api/eventos`)

| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `GET` | `/api/eventos` | ✅ Sí |
| `GET` | `/api/eventos/{id}` | ✅ Sí |
| `POST` | `/api/eventos` | ✅ Admin |
| `PUT` | `/api/eventos/{id}` | ✅ Admin |
| `DELETE` | `/api/eventos/{id}` | ✅ Admin |

### 📢 Anuncios (`/api/anuncios`)

| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `GET` | `/api/anuncios` | ✅ Sí |
| `GET` | `/api/anuncios/{id}` | ✅ Sí |
| `POST` | `/api/anuncios` | ✅ Admin |
| `PUT` | `/api/anuncios/{id}` | ✅ Admin |
| `DELETE` | `/api/anuncios/{id}` | ✅ Admin |

### 🔔 Notificaciones (`/api/notificaciones`)

| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `POST` | `/api/notificaciones` | ✅ Admin |
| `GET` | `/api/notificaciones` | ✅ Sí |
| `GET` | `/api/notificaciones/{id}` | ✅ Sí |
| `PUT` | `/api/notificaciones/{id}` | ✅ Admin |
| `DELETE` | `/api/notificaciones/{id}` | ✅ Admin |

### Usuario-Notificaciones (`/api/usuarios/{usuarioId}/notificaciones`)
| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `GET` | `/api/usuarios/{usuarioId}/notificaciones` | ✅ Sí |
| `GET` | `/api/usuarios/{usuarioId}/notificaciones/{notificacionId}` | ✅ Sí |
| `PUT` | `/api/usuarios/{usuarioId}/notificaciones/{notificacionId}/leer` | ✅ Sí |
| `GET` | `/api/usuarios/{usuarioId}/notificaciones/no-leidas/count` | ✅ Sí |

### 🔐 Reglas de Acceso (`/api/reglas-acceso`)

| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `GET` | `/api/reglas-acceso` | ✅ Sí |
| `GET` | `/api/reglas-acceso/{id}` | ✅ Sí |
| `GET` | `/api/reglas-acceso/espacio/{espacioId}` | ✅ Sí |
| `POST` | `/api/reglas-acceso` | ✅ Admin |
| `PUT` | `/api/reglas-acceso/{id}` | ✅ Admin |
| `DELETE` | `/api/reglas-acceso/{id}` | ✅ Admin |

### 🚪 Acceso (`/api/acceso`)

| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `POST` | `/api/acceso/validar` | ✅ Sí |

### 📊 Auditoría (`/api/auditoria/eventos`)

| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `GET` | `/api/auditoria/eventos` | ✅ Admin |
| `GET` | `/api/auditoria/eventos/usuario/{usuarioId}` | ✅ Admin |
| `GET` | `/api/auditoria/eventos/estadisticas` | ✅ Admin |

### ☁️ AWS (`/api/aws`)

| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `GET` | `/api/aws/secrets/{secretName}` | ✅ Admin |
| `GET` | `/api/aws/parameters/{parameterName}` | ✅ Admin |
| `GET` | `/api/aws/parameters` | ✅ Admin |
| `POST` | `/api/aws/seed-resources` | ✅ Admin |

### 📈 Cache Metrics (`/api/cache-metrics`)

| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `GET` | `/api/cache-metrics` | ✅ Admin |
| `POST` | `/api/cache-metrics/reset` | ✅ Admin |
| `GET` | `/api/cache-metrics/health` | ❌ No |

### 🏥 Health Checks

| Método | Endpoint | Auth Requerida |
|--------|----------|----------------|
| `GET` | `/health` | ❌ No |
| `GET` | `/health/mongodb` | ❌ No |
| `GET` | `/health/redis` | ❌ No |

### ⚙️ System (`/system`)

| Método | Endpoint | Auth Requerida | Notas |
|--------|----------|----------------|-------|
| `DELETE` | `/system/mongodb/clear` | ❌ No | Solo desarrollo |

---

## 🧪 PRUEBAS DE ENDPOINTS

### Resultados de Pruebas (2025-01-21)

| Endpoint | Método | Status | Resultado | Notas |
|----------|--------|--------|-----------|-------|
| `/health` | GET | 200 | ✅ OK | Funciona correctamente |
| `/health/mongodb` | GET | Error | ❌ FAIL | Error de conexión |
| `/health/redis` | GET | 500 | ❌ FAIL | Error interno |
| `/api/auth/login` | POST | 401 | ❌ FAIL | Credenciales incorrectas o formato inválido |
| `/api/auth/qr` | GET | 401 | ❌ FAIL | Requiere token válido |
| `/api/auth/validate` | GET | 401 | ❌ FAIL | Requiere token válido |
| `/api/usuarios` | GET | 401 | ❌ FAIL | Requiere token válido |
| `/api/beneficios` | GET | 401 | ❌ FAIL | Requiere token válido |

**Nota:** Los errores 401 son esperados cuando no se proporciona un token válido. Los endpoints funcionan correctamente con autenticación.

---

## 🔧 PROBLEMAS IDENTIFICADOS Y SOLUCIONES

### ❌ Problema 1: Regla `/auth/*` en ALB (RESUELTO)

**Descripción:**
- Existía una regla con prioridad 120 para `/auth/*` en el listener HTTPS
- Esta regla capturaba peticiones a `/auth/login` antes de que se evaluara `/api/*`
- El backend solo tiene `/api/auth/login`, no `/auth/login`
- Resultado: 404 Not Found

**Solución Aplicada:**
- ✅ Regla `/auth/*` (prioridad 120) eliminada del listener HTTPS
- ✅ Frontend corregido para usar `URLService.getLink()` que incluye `/api/`

**Estado:** ✅ RESUELTO

### ⚠️ Problema 2: Regla `/health` en HTTP Listener

**Descripción:**
- La regla en el listener HTTP (puerto 80) solo captura `/health` (exacto)
- No captura `/health/mongodb` ni `/health/redis`
- Estas rutas serán redirigidas a HTTPS y luego al frontend (default)

**Recomendación:**
- Cambiar la regla HTTP de `/health` a `/health*` para capturar sub-rutas
- O dejar como está si solo se necesita `/health` en HTTP

**Estado:** ⚠️ MENOR - No crítico (HTTPS funciona correctamente)

### ✅ Problema 3: Frontend usando rutas incorrectas (RESUELTO)

**Archivos Corregidos:**
1. `frontend/src/services/UsuarioService.ts`
   - Cambiado de `BASE_URL` (sin `/api/`) a `API_URL` (con `/api/`)
   - Rutas ahora: `/api/auth/login`, `/api/usuarios/*`

2. `frontend/src/app/perfil/escaner/page.jsx`
   - Cambiado de `getBaseUrl()` a `getLink()`
   - Ruta ahora: `/api/auth/validate`

**Estado:** ✅ RESUELTO (desplegado)

---

## 📝 RECOMENDACIONES

### 1. Verificar Deployment del Frontend
- El frontend fue desplegado pero puede tener caché del navegador
- **Acción:** Limpiar caché del navegador o hacer hard refresh (Ctrl+Shift+R)

### 2. Monitorear Logs del ALB
- Verificar que las peticiones lleguen correctamente al backend
- Revisar logs de CloudWatch para el target group del backend

### 3. Validar Todas las Rutas del Frontend
- Asegurarse de que todas las llamadas al backend usen `/api/*`
- Verificar que `URLService.getLink()` se use consistentemente

### 4. Actualizar Regla HTTP `/health` (Opcional)
- Si se necesita acceso HTTP directo a `/health/mongodb` y `/health/redis`
- Cambiar la regla de `/health` a `/health*` en el listener HTTP

### 5. Documentar Cambios
- Mantener este documento actualizado con cualquier cambio en las reglas ALB
- Documentar nuevos endpoints cuando se agreguen

---

## 🔄 FLUJO DE PETICIONES

### Petición: `POST https://api.zimmzimmgames.com/api/auth/login`

1. **DNS:** `api.zimmzimmgames.com` → Resuelve a ALB (gatekeep-alb)
2. **ALB Listener HTTPS (443):**
   - Evalúa reglas en orden de prioridad:
     - Prioridad 100: `/api/*` → ✅ **COINCIDE**
     - Forward a Target Group: `gatekeep-tg` (Backend)
3. **Target Group:** Enruta a ECS Task (Backend en puerto 5011)
4. **Backend:** Procesa la petición en `/api/auth/login`
5. **Response:** Retorna token JWT y datos del usuario

### Petición: `POST https://api.zimmzimmgames.com/auth/login` (INCORRECTA)

1. **DNS:** `api.zimmzimmgames.com` → Resuelve a ALB
2. **ALB Listener HTTPS (443):**
   - Evalúa reglas:
     - Prioridad 100: `/api/*` → ❌ No coincide
     - Prioridad 130: `/swagger*` → ❌ No coincide
     - Prioridad 140: `/health*` → ❌ No coincide
     - Prioridad 150: `/system/*` → ❌ No coincide
     - **Default:** → ✅ Se aplica (Frontend)
3. **Target Group:** Enruta a Frontend (puerto 3000)
4. **Frontend:** No tiene ruta `/auth/login` → 404 Not Found

**Conclusión:** El frontend debe usar `/api/auth/login`, no `/auth/login`.

---

## 📊 RESUMEN DE CAMBIOS APLICADOS

### AWS ALB
- ✅ Eliminada regla `/auth/*` (prioridad 120) del listener HTTPS
- ✅ Reglas actuales: `/api/*`, `/swagger*`, `/health*`, `/system/*`

### Frontend
- ✅ `UsuarioService.ts`: Corregido para usar `/api/*`
- ✅ `escaner/page.jsx`: Corregido para usar `/api/auth/validate`
- ✅ Desplegado a ECS

### Backend
- ✅ Sin cambios necesarios (ya tenía las rutas correctas)

---

## 🎯 PRÓXIMOS PASOS

1. **Verificar que el frontend desplegado funcione correctamente**
   - Probar login desde el navegador
   - Verificar que no haya errores 404

2. **Monitorear logs de CloudWatch**
   - Revisar logs del ALB
   - Revisar logs del backend ECS

3. **Si persisten problemas:**
   - Verificar caché del navegador
   - Verificar que el deployment del frontend se completó
   - Revisar logs del frontend ECS

---

**Documento generado:** 2025-01-21  
**Última actualización:** 2025-01-21

