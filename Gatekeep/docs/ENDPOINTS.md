# 🚀 Documentación de Endpoints - GateKeep API

## 📋 Resumen

Este documento contiene la documentación completa de todos los endpoints disponibles en la API de GateKeep y cómo se conectan desde el frontend.

**URL Base**: `http://localhost:5011/api/`

---

## 🔐 **Autenticación**

Todos los endpoints (excepto `/auth/login`) requieren autenticación mediante JWT Bearer Token.

### **Headers Requeridos**
```javascript
{
  "Authorization": "Bearer <token>",
  "Content-Type": "application/json"
}
```

### **Servicio de Autenticación en Frontend**
```typescript
// frontend/src/services/securityService.ts
import { SecurityService } from './securityService';

// Obtener token
const token = SecurityService.getToken();

// Headers con autenticación
const headers = {
  headers: {
    Authorization: `Bearer ${token}`,
    "Content-Type": "application/json",
  },
};
```

---

## 📝 **Endpoints por Módulo**

### 🔑 **1. Autenticación (`/auth`)**

#### **POST `/auth/login`** - Iniciar Sesión
- **Descripción**: Autentica un usuario y retorna un token JWT
- **Autenticación**: No requerida (público)
- **Request Body**:
  ```json
  {
    "email": "usuario@gatekeep.com",
    "password": "password123"
  }
  ```
- **Response 200**:
  ```json
  {
    "isSuccess": true,
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiresAt": "2024-12-31T23:59:59Z",
    "user": {
      "id": 1,
      "email": "usuario@gatekeep.com",
      "nombre": "Juan",
      "apellido": "Pérez",
      "tipoUsuario": "Estudiante",
      "telefono": "+1234567890",
      "fechaAlta": "2024-01-01T00:00:00Z"
    }
  }
  ```
- **Uso en Frontend**:
  ```typescript
  import axios from 'axios';
  
  const response = await axios.post('http://localhost:5011/auth/login', {
    email: 'usuario@gatekeep.com',
    password: 'password123'
  });
  
  // Guardar token
  localStorage.setItem('token', response.data.token);
  ```

#### **POST `/auth/register`** - Registrar Usuario
- **Descripción**: Registra un nuevo usuario en el sistema
- **Autenticación**: Requerida (AdminOnly)
- **Request Body**:
  ```json
  {
    "email": "nuevo@gatekeep.com",
    "password": "password123",
    "confirmPassword": "password123",
    "nombre": "Juan",
    "apellido": "Pérez",
    "telefono": "+1234567890",
    "rol": "Estudiante"
  }
  ```

#### **GET `/auth/qr`** - Generar Código QR del JWT
- **Descripción**: Genera una imagen PNG con el código QR que contiene el token JWT
- **Autenticación**: Requerida
- **Query Parameters**:
  - `token` (opcional): Token JWT explícito
  - `w` (opcional): Ancho de la imagen (default: 250)
  - `h` (opcional): Alto de la imagen (default: 250)
- **Response**: Imagen PNG

#### **POST `/auth/create-test-users`** - Crear Usuarios de Prueba
- **Descripción**: Crea usuarios de prueba de todos los tipos para testing
- **Autenticación**: No requerida (público)

#### **GET `/auth/list-users`** - Listar Usuarios
- **Descripción**: Lista todos los usuarios con contraseñas en texto plano para testing
- **Autenticación**: No requerida (público)

---

### 📅 **2. Eventos (`/api/eventos`)**

#### **GET `/api/eventos`** - Obtener Todos los Eventos
- **Descripción**: Obtiene todos los eventos disponibles
- **Autenticación**: No requerida (público)
- **Response 200**:
  ```json
  [
    {
      "id": 1,
      "nombre": "Evento Deportivo",
      "fecha": "2024-12-25T10:00:00Z",
      "resultado": "Pendiente",
      "puntoControl": "Entrada Principal"
    }
  ]
  ```
- **Uso en Frontend**:
  ```typescript
  // frontend/src/services/EventoService.ts
  import axios from "axios";
  import { URLService } from "./urlService";
  
  const API_URL = URLService.getLink() + "eventos";
  
  export class EventoService {
    static getEventos() {
      return axios.get(API_URL);
    }
  }
  
  // En un componente
  const eventos = await EventoService.getEventos();
  ```

#### **GET `/api/eventos/{id}`** - Obtener Evento por ID
- **Descripción**: Obtiene un evento específico por su ID
- **Autenticación**: No requerida (público)
- **Path Parameters**:
  - `id` (long): ID del evento

#### **POST `/api/eventos`** - Crear Evento
- **Descripción**: Crea un nuevo evento
- **Autenticación**: Requerida (FuncionarioOrAdmin)
- **Request Body**:
  ```json
  {
    "nombre": "Nuevo Evento",
    "fecha": "2024-12-25T10:00:00Z",
    "resultado": "Pendiente",
    "puntoControl": "Entrada Principal"
  }
  ```

#### **PUT `/api/eventos/{id}`** - Actualizar Evento
- **Descripción**: Actualiza un evento existente
- **Autenticación**: Requerida (FuncionarioOrAdmin)
- **Path Parameters**:
  - `id` (long): ID del evento
- **Request Body**: Mismo formato que POST

#### **DELETE `/api/eventos/{id}`** - Eliminar Evento
- **Descripción**: Elimina un evento
- **Autenticación**: Requerida (AdminOnly)
- **Path Parameters**:
  - `id` (long): ID del evento

---

### 📢 **3. Anuncios (`/api/anuncios`)**

#### **GET `/api/anuncios`** - Obtener Todos los Anuncios
- **Descripción**: Obtiene todos los anuncios disponibles
- **Autenticación**: No requerida (público)
- **Response 200**:
  ```json
  [
    {
      "id": 1,
      "nombre": "Anuncio Importante",
      "fecha": "2024-12-25T10:00:00Z"
    }
  ]
  ```
- **Uso en Frontend**:
  ```typescript
  // frontend/src/services/AnuncioService.ts
  import axios from "axios";
  import { URLService } from "./urlService";
  
  const API_URL = URLService.getLink() + "anuncios";
  
  export class AnuncioService {
    static getAnuncios() {
      return axios.get(API_URL);
    }
  }
  ```

#### **GET `/api/anuncios/{id}`** - Obtener Anuncio por ID
- **Descripción**: Obtiene un anuncio específico por su ID
- **Autenticación**: No requerida (público)

#### **POST `/api/anuncios`** - Crear Anuncio
- **Descripción**: Crea un nuevo anuncio
- **Autenticación**: Requerida (FuncionarioOrAdmin)
- **Request Body**:
  ```json
  {
    "nombre": "Nuevo Anuncio",
    "fecha": "2024-12-25T10:00:00Z"
  }
  ```

#### **PUT `/api/anuncios/{id}`** - Actualizar Anuncio
- **Descripción**: Actualiza un anuncio existente
- **Autenticación**: Requerida (FuncionarioOrAdmin)

#### **DELETE `/api/anuncios/{id}`** - Eliminar Anuncio
- **Descripción**: Elimina un anuncio
- **Autenticación**: Requerida (AdminOnly)

---

### 🔒 **4. Reglas de Acceso (`/api/reglas-acceso`)**

#### **GET `/api/reglas-acceso`** - Obtener Todas las Reglas
- **Descripción**: Obtiene todas las reglas de acceso
- **Autenticación**: Requerida (FuncionarioOrAdmin)
- **Response 200**:
  ```json
  [
    {
      "id": 1,
      "horarioApertura": "2024-01-01T08:00:00Z",
      "horarioCierre": "2024-01-01T18:00:00Z",
      "vigenciaApertura": "2024-01-01T00:00:00Z",
      "vigenciaCierre": "2024-12-31T23:59:59Z",
      "rolesPermitidos": ["Estudiante", "Funcionario"],
      "espacioId": 1
    }
  ]
  ```
- **Uso en Frontend**:
  ```typescript
  // frontend/src/services/ReglaAccesoService.ts
  import axios from "axios";
  import { URLService } from "./urlService";
  import { SecurityService } from "./securityService";
  
  const API_URL = URLService.getLink() + "reglas-acceso";
  
  export class ReglaAccesoService {
    static getAuthHeaders() {
      const token = SecurityService.getToken();
      return {
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
      };
    }
  
    static async getReglasAcceso() {
      return axios.get(API_URL, this.getAuthHeaders());
    }
  }
  ```

#### **GET `/api/reglas-acceso/{id}`** - Obtener Regla por ID
- **Descripción**: Obtiene una regla de acceso específica por su ID
- **Autenticación**: Requerida (FuncionarioOrAdmin)
- **Uso en Frontend**:
  ```typescript
  static async getReglaAccesoById(id: number) {
    return axios.get(`${API_URL}/${id}`, this.getAuthHeaders());
  }
  ```

#### **GET `/api/reglas-acceso/espacio/{espacioId}`** - Obtener Regla por Espacio
- **Descripción**: Obtiene la regla de acceso para un espacio específico
- **Autenticación**: Requerida (FuncionarioOrAdmin)
- **Uso en Frontend**:
  ```typescript
  static async getReglaAccesoPorEspacioId(espacioId: number) {
    return axios.get(`${API_URL}/espacio/${espacioId}`, this.getAuthHeaders());
  }
  ```

#### **POST `/api/reglas-acceso`** - Crear Regla de Acceso
- **Descripción**: Crea una nueva regla de acceso
- **Autenticación**: Requerida (AdminOnly)
- **Request Body**:
  ```json
  {
    "horarioApertura": "2024-01-01T08:00:00Z",
    "horarioCierre": "2024-01-01T18:00:00Z",
    "vigenciaApertura": "2024-01-01T00:00:00Z",
    "vigenciaCierre": "2024-12-31T23:59:59Z",
    "rolesPermitidos": ["Estudiante", "Funcionario"],
    "espacioId": 1
  }
  ```
- **Uso en Frontend**:
  ```typescript
  static async crearReglaAcceso(data: {
    horarioApertura: string;
    horarioCierre: string;
    vigenciaApertura: string;
    vigenciaCierre: string;
    rolesPermitidos: string[];
    espacioId: number;
  }) {
    return axios.post(API_URL, data, this.getAuthHeaders());
  }
  ```

#### **PUT `/api/reglas-acceso/{id}`** - Actualizar Regla de Acceso
- **Descripción**: Actualiza una regla de acceso existente
- **Autenticación**: Requerida (AdminOnly)
- **Uso en Frontend**:
  ```typescript
  static async actualizarReglaAcceso(
    id: number,
    data: {
      horarioApertura: string;
      horarioCierre: string;
      vigenciaApertura: string;
      vigenciaCierre: string;
      rolesPermitidos: string[];
      espacioId: number;
    }
  ) {
    return axios.put(`${API_URL}/${id}`, data, this.getAuthHeaders());
  }
  ```

#### **DELETE `/api/reglas-acceso/{id}`** - Eliminar Regla de Acceso
- **Descripción**: Elimina una regla de acceso
- **Autenticación**: Requerida (AdminOnly)
- **Uso en Frontend**:
  ```typescript
  static async eliminarReglaAcceso(id: number) {
    return axios.delete(`${API_URL}/${id}`, this.getAuthHeaders());
  }
  ```

---

### 👥 **5. Usuarios (`/usuarios`)**

#### **GET `/usuarios`** - Obtener Todos los Usuarios
- **Descripción**: Obtiene todos los usuarios del sistema
- **Autenticación**: Requerida (AdminOnly)

#### **GET `/usuarios/{id}`** - Obtener Usuario por ID
- **Descripción**: Obtiene un usuario específico por su ID
- **Autenticación**: Requerida (AllUsers - solo propio perfil o Admin)

#### **POST `/usuarios`** - Crear Usuario
- **Descripción**: Crea un nuevo usuario con rol
- **Autenticación**: Requerida (AdminOnly)
- **Request Body**:
  ```json
  {
    "email": "nuevo@gatekeep.com",
    "contrasenia": "password123",
    "nombre": "Juan",
    "apellido": "Pérez",
    "telefono": "+1234567890",
    "rol": "Estudiante"
  }
  ```

#### **PUT `/usuarios/{id}/rol`** - Actualizar Rol de Usuario
- **Descripción**: Actualiza el rol de un usuario
- **Autenticación**: Requerida (AdminOnly)
- **Request Body**:
  ```json
  {
    "rol": "Funcionario"
  }
  ```

#### **DELETE `/usuarios/{id}`** - Eliminar Usuario
- **Descripción**: Elimina un usuario (borrado lógico)
- **Autenticación**: Requerida (AdminOnly)

---

### 🎁 **6. Beneficios (`/beneficios`)**

#### **GET `/beneficios`** - Obtener Todos los Beneficios
- **Descripción**: Obtiene todos los beneficios disponibles
- **Autenticación**: Requerida (AllUsers)

#### **GET `/beneficios/{id}`** - Obtener Beneficio por ID
- **Descripción**: Obtiene un beneficio específico por su ID
- **Autenticación**: Requerida (AllUsers)

#### **POST `/beneficios`** - Crear Beneficio
- **Descripción**: Crea un nuevo beneficio
- **Autenticación**: Requerida (FuncionarioOrAdmin)

#### **PUT `/beneficios/{id}`** - Actualizar Beneficio
- **Descripción**: Actualiza un beneficio existente
- **Autenticación**: Requerida (FuncionarioOrAdmin)

#### **DELETE `/beneficios/{id}`** - Eliminar Beneficio
- **Descripción**: Elimina un beneficio (borrado lógico)
- **Autenticación**: Requerida (AdminOnly)

#### **GET `/api/usuarios/{usuarioId}/beneficios`** - Obtener Beneficios de Usuario
- **Descripción**: Obtiene todos los beneficios asignados a un usuario
- **Autenticación**: Requerida (AllUsers)

#### **POST `/api/usuarios/{usuarioId}/beneficios/{beneficioId}`** - Asignar Beneficio
- **Descripción**: Asigna un beneficio a un usuario
- **Autenticación**: Requerida (FuncionarioOrAdmin)

#### **DELETE `/api/usuarios/{usuarioId}/beneficios/{beneficioId}`** - Desasignar Beneficio
- **Descripción**: Desasigna un beneficio de un usuario
- **Autenticación**: Requerida (FuncionarioOrAdmin)

---

### 🔔 **7. Notificaciones (`/api/notificaciones`)**

#### **GET `/api/notificaciones`** - Obtener Todas las Notificaciones
- **Descripción**: Obtiene todas las notificaciones del sistema
- **Autenticación**: Requerida (AllUsers)

#### **GET `/api/notificaciones/{id}`** - Obtener Notificación por ID
- **Descripción**: Obtiene una notificación específica por su ID
- **Autenticación**: Requerida (AllUsers)

#### **POST `/api/notificaciones`** - Crear Notificación
- **Descripción**: Crea una nueva notificación
- **Autenticación**: Requerida (FuncionarioOrAdmin)
- **Request Body**:
  ```json
  {
    "mensaje": "Nueva notificación importante",
    "tipo": "Info"
  }
  ```

#### **PUT `/api/notificaciones/{id}`** - Actualizar Notificación
- **Descripción**: Actualiza una notificación existente
- **Autenticación**: Requerida (FuncionarioOrAdmin)

#### **DELETE `/api/notificaciones/{id}`** - Eliminar Notificación
- **Descripción**: Elimina una notificación
- **Autenticación**: Requerida (AdminOnly)

#### **GET `/api/usuarios/{usuarioId}/notificaciones`** - Obtener Notificaciones de Usuario
- **Descripción**: Obtiene todas las notificaciones de un usuario
- **Autenticación**: Requerida (AllUsers)

#### **GET `/api/usuarios/{usuarioId}/notificaciones/{notificacionId}`** - Obtener Notificación de Usuario
- **Descripción**: Obtiene una notificación específica de un usuario
- **Autenticación**: Requerida (AllUsers)

#### **PUT `/api/usuarios/{usuarioId}/notificaciones/{notificacionId}/leer`** - Marcar como Leída
- **Descripción**: Marca una notificación como leída
- **Autenticación**: Requerida (AllUsers)

#### **GET `/api/usuarios/{usuarioId}/notificaciones/no-leidas/count`** - Contar No Leídas
- **Descripción**: Cuenta las notificaciones no leídas de un usuario
- **Autenticación**: Requerida (AllUsers)
- **Response 200**:
  ```json
  {
    "count": 5,
    "usuarioId": 1
  }
  ```

---

### 🚪 **8. Acceso (`/api/acceso`)**

#### **POST `/api/acceso/validar`** - Validar Acceso
- **Descripción**: Valida si un usuario tiene permisos para acceder a un espacio
- **Autenticación**: Requerida (AllUsers)
- **Request Body**:
  ```json
  {
    "usuarioId": 1,
    "espacioId": 1,
    "puntoControl": "Entrada Principal"
  }
  ```
- **Response 200 (Permitido)**:
  ```json
  {
    "permitido": true,
    "razon": null,
    "usuarioId": 1,
    "espacioId": 1,
    "puntoControl": "Entrada Principal",
    "fecha": "2024-12-25T10:00:00Z"
  }
  ```
- **Response 403 (Denegado)**:
  ```json
  {
    "tipoError": "ROL_NO_PERMITIDO",
    "mensaje": "El rol del usuario no está permitido para este espacio",
    "codigoError": "ROL_NO_PERMITIDO",
    "usuarioId": 1,
    "espacioId": 1,
    "puntoControl": "Entrada Principal"
  }
  ```

---

### 📊 **9. Auditoría (`/api/auditoria/eventos`)**

#### **GET `/api/auditoria/eventos`** - Obtener Eventos Históricos
- **Descripción**: Obtiene eventos históricos con paginación y filtros
- **Autenticación**: Requerida (FuncionarioOrAdmin)
- **Query Parameters**:
  - `page` (int, default: 1): Número de página
  - `pageSize` (int, default: 50): Tamaño de página
  - `fechaDesde` (DateTime?, optional): Fecha desde
  - `fechaHasta` (DateTime?, optional): Fecha hasta
  - `usuarioId` (long?, optional): ID de usuario
  - `tipoEvento` (string?, optional): Tipo de evento
  - `resultado` (string?, optional): Resultado
- **Response 200**:
  ```json
  {
    "eventos": [
      {
        "id": "507f1f77bcf86cd799439011",
        "tipoEvento": "Acceso",
        "fecha": "2024-12-25T10:00:00Z",
        "usuarioId": 1,
        "espacioId": 1,
        "resultado": "Permitido",
        "puntoControl": "Entrada Principal",
        "datos": {}
      }
    ],
    "paginacion": {
      "pagina": 1,
      "tamanoPagina": 50,
      "totalCount": 100,
      "totalPaginas": 2
    }
  }
  ```

#### **GET `/api/auditoria/eventos/usuario/{usuarioId}`** - Obtener Eventos por Usuario
- **Descripción**: Obtiene eventos históricos de un usuario específico
- **Autenticación**: Requerida (AllUsers - solo propio usuario o Funcionario/Admin)

#### **GET `/api/auditoria/eventos/estadisticas`** - Obtener Estadísticas
- **Descripción**: Obtiene estadísticas agregadas de eventos
- **Autenticación**: Requerida (FuncionarioOrAdmin)
- **Query Parameters**:
  - `fechaDesde` (DateTime, required): Fecha desde
  - `fechaHasta` (DateTime, required): Fecha hasta

---

## 🔧 **Configuración del Frontend**

### **URL Service**
```typescript
// frontend/src/services/urlService.ts
import axios from "axios";

const API_URL = "http://localhost:5011/api/";

export class URLService {
  static getLink() {
    return API_URL;
  }
}
```

### **Security Service**
```typescript
// frontend/src/services/securityService.ts
export class SecurityService {
  static getToken(): string | null {
    return localStorage.getItem('token');
  }
  
  static setToken(token: string): void {
    localStorage.setItem('token', token);
  }
  
  static removeToken(): void {
    localStorage.removeItem('token');
  }
  
  static getAuthHeaders() {
    const token = this.getToken();
    return {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    };
  }
}
```

---

## 📝 **Ejemplos de Uso Completo**

### **Ejemplo 1: Obtener Eventos**
```typescript
import { EventoService } from '@/services/EventoService';

// En un componente React
const fetchEventos = async () => {
  try {
    const response = await EventoService.getEventos();
    const eventos = response.data;
    console.log('Eventos:', eventos);
  } catch (error) {
    console.error('Error al obtener eventos:', error);
  }
};
```

### **Ejemplo 2: Crear Regla de Acceso**
```typescript
import { ReglaAccesoService } from '@/services/ReglaAccesoService';

const crearRegla = async () => {
  try {
    const data = {
      horarioApertura: "2024-01-01T08:00:00Z",
      horarioCierre: "2024-01-01T18:00:00Z",
      vigenciaApertura: "2024-01-01T00:00:00Z",
      vigenciaCierre: "2024-12-31T23:59:59Z",
      rolesPermitidos: ["Estudiante", "Funcionario"],
      espacioId: 1
    };
    
    const response = await ReglaAccesoService.crearReglaAcceso(data);
    console.log('Regla creada:', response.data);
  } catch (error) {
    console.error('Error al crear regla:', error);
  }
};
```

### **Ejemplo 3: Validar Acceso**
```typescript
import axios from 'axios';
import { URLService } from '@/services/urlService';
import { SecurityService } from '@/services/securityService';

const validarAcceso = async (usuarioId: number, espacioId: number, puntoControl: string) => {
  try {
    const response = await axios.post(
      `${URLService.getLink()}acceso/validar`,
      {
        usuarioId,
        espacioId,
        puntoControl
      },
      SecurityService.getAuthHeaders()
    );
    
    if (response.data.permitido) {
      console.log('Acceso permitido');
    } else {
      console.log('Acceso denegado:', response.data.mensaje);
    }
  } catch (error) {
    console.error('Error al validar acceso:', error);
  }
};
```

---

## 🎯 **Políticas de Autorización**

### **AllUsers**
- Todos los usuarios autenticados pueden acceder

### **FuncionarioOrAdmin**
- Solo funcionarios y administradores pueden acceder

### **AdminOnly**
- Solo administradores pueden acceder

### **Público**
- No requiere autenticación

---

## ⚠️ **Códigos de Estado HTTP**

- **200 OK**: Operación exitosa
- **201 Created**: Recurso creado exitosamente
- **204 No Content**: Operación exitosa sin contenido
- **400 Bad Request**: Solicitud inválida
- **401 Unauthorized**: No autenticado
- **403 Forbidden**: No autorizado
- **404 Not Found**: Recurso no encontrado
- **412 Precondition Failed**: Precondición fallida
- **500 Internal Server Error**: Error interno del servidor

---

## 🔄 **Actualizaciones**

- **Última actualización**: Diciembre 2024
- **Versión**: 1.0.0
- **Estado**: Desarrollo activo

---

*Documento generado automáticamente para el proyecto GateKeep*

