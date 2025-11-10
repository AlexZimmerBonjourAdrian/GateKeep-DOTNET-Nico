# 🚀 Rutas de Acceso - GateKeep Frontend

## 📋 Resumen de Rutas Disponibles

Este documento contiene todas las rutas disponibles en la aplicación GateKeep desarrollada con Next.js.

---

## 🏠 **Rutas Principales**

### **Página de Inicio**
- **URL**: `/`
- **Archivo**: `app/page.js`
- **Descripción**: Página principal con el header de GateKeep
- **Componente**: Header

---

## 🔐 **Rutas de Autenticación**

### **Iniciar Sesión**
- **URL**: `/auth/login`
- **Archivo**: `app/auth/login/page.js`
- **Descripción**: Formulario de inicio de sesión
- **Componente**: Login

### **Registrarse**
- **URL**: `/auth/register`
- **Archivo**: `app/auth/register/page.js`
- **Descripción**: Formulario de registro de usuarios
- **Componente**: Register

---

## 📝 **Rutas de Gestión**

### **Eventos**

#### **Listado de Eventos**
- **URL**: `/evento/listadoEventos`
- **Archivo**: `app/evento/listadoEventos/page.jsx`
- **Descripción**: Listado de todos los eventos con filtros de búsqueda y fecha
- **Componente**: listadoEventos
- **Campos mostrados**: Nombre, Fecha, Resultado, Punto de Control

#### **Crear Evento**
- **URL**: `/evento/crearEvento`
- **Archivo**: `app/evento/crearEvento/page.jsx`
- **Descripción**: Formulario para crear nuevos eventos
- **Componente**: crearEvento
- **Campos**: Nombre, Fecha, Resultado, Punto de Control

### **Anuncios**

#### **Listado de Anuncios**
- **URL**: `/anuncio/listadoAnuncios`
- **Archivo**: `app/anuncio/listadoAnuncios/page.jsx`
- **Descripción**: Listado de todos los anuncios con filtros de búsqueda y fecha
- **Componente**: listadoAnuncios
- **Campos mostrados**: Título, Fecha

#### **Crear Anuncio**
- **URL**: `/anuncio/crearAnuncio`
- **Archivo**: `app/anuncio/crearAnuncio/page.jsx`
- **Descripción**: Formulario para crear nuevos anuncios
- **Componente**: crearAnuncio
- **Campos**: Nombre, Fecha

### **Reglas de Acceso**

#### **Listado de Reglas de Acceso**
- **URL**: `/reglas-acceso/listadoReglasAcceso`
- **Archivo**: `app/reglas-acceso/listadoReglasAcceso/page.jsx`
- **Descripción**: Listado de todas las reglas de acceso con filtros de búsqueda y fecha
- **Componente**: listadoReglasAcceso
- **Campos mostrados**: Espacio ID, Horario, Vigencia, Roles Permitidos

#### **Crear Regla de Acceso**
- **URL**: `/reglas-acceso/crearReglaAcceso`
- **Archivo**: `app/reglas-acceso/crearReglaAcceso/page.jsx`
- **Descripción**: Formulario para crear nuevas reglas de acceso
- **Componente**: crearReglaAcceso
- **Campos**: Espacio ID, Horario de Apertura, Horario de Cierre, Vigencia Desde, Vigencia Hasta, Roles Permitidos

#### **Editar Regla de Acceso**
- **URL**: `/reglas-acceso/editarReglaAcceso/[id]`
- **Archivo**: `app/reglas-acceso/editarReglaAcceso/[id]/page.jsx`
- **Descripción**: Formulario para editar reglas de acceso existentes
- **Componente**: editarReglaAcceso
- **Campos**: Espacio ID, Horario de Apertura, Horario de Cierre, Vigencia Desde, Vigencia Hasta, Roles Permitidos
- **Ruta dinámica**: El parámetro `[id]` se reemplaza con el ID de la regla

### **Beneficios**

#### **Crear Beneficio**
- **URL**: `/crear-beneficio`
- **Archivo**: `app/crear-beneficio/page.js`
- **Descripción**: Formulario para crear nuevos beneficios
- **Componente**: CrearBeneficioForm
- **Campos**: Tipo de Beneficio, Cupos, Vigencia, Vencimiento

---

## 🎨 **Rutas de Componentes (Archivos de Respaldo)**

Los siguientes archivos están en la carpeta `src/pages/` como respaldo:

- `src/pages/Home.jsx`
- `src/pages/Login.jsx`
- `src/pages/Register.jsx`
- `src/pages/CrearEvento.jsx`
- `src/pages/CrearBeneficio.jsx`

---

## 🛠️ **Configuración Técnica**

### **Framework**
- **Next.js 13+** con App Router
- **React 18+**
- **PrimeReact** para componentes UI

### **Estructura de Rutas**
```
frontend/
├── app/
│   ├── page.jsx                          → /
│   ├── auth/
│   │   ├── login/page.jsx              → /auth/login
│   │   └── register/page.jsx            → /auth/register
│   ├── evento/
│   │   ├── crearEvento/page.jsx        → /evento/crearEvento
│   │   └── listadoEventos/page.jsx     → /evento/listadoEventos
│   ├── anuncio/
│   │   ├── crearAnuncio/page.jsx       → /anuncio/crearAnuncio
│   │   └── listadoAnuncios/page.jsx    → /anuncio/listadoAnuncios
│   ├── reglas-acceso/
│   │   ├── crearReglaAcceso/page.jsx   → /reglas-acceso/crearReglaAcceso
│   │   ├── listadoReglasAcceso/page.jsx → /reglas-acceso/listadoReglasAcceso
│   │   └── editarReglaAcceso/
│   │       └── [id]/page.jsx           → /reglas-acceso/editarReglaAcceso/[id]
│   ├── notificaciones/page.jsx         → /notificaciones
│   ├── perfil/page.jsx                 → /perfil
│   └── crear-beneficio/page.js         → /crear-beneficio
└── src/
    └── components/                      → Componentes reutilizables
```

### **Puerto de Desarrollo**
- **URL Base**: `http://localhost:3000`
- **Comando**: `npm run dev`

---

## 📱 **Navegación**

### **Enlaces de Navegación**

#### **Rutas Principales**
- **Inicio**: `/`
- **Perfil**: `/perfil`
- **Notificaciones**: `/notificaciones`

#### **Rutas de Autenticación**
- **Login**: `/auth/login`
- **Registro**: `/auth/register`

#### **Rutas de Eventos**
- **Listado de Eventos**: `/evento/listadoEventos`
- **Crear Evento**: `/evento/crearEvento`

#### **Rutas de Anuncios**
- **Listado de Anuncios**: `/anuncio/listadoAnuncios`
- **Crear Anuncio**: `/anuncio/crearAnuncio`

#### **Rutas de Reglas de Acceso**
- **Listado de Reglas de Acceso**: `/reglas-acceso/listadoReglasAcceso`
- **Crear Regla de Acceso**: `/reglas-acceso/crearReglaAcceso`
- **Editar Regla de Acceso**: `/reglas-acceso/editarReglaAcceso/[id]` (ruta dinámica)

#### **Rutas de Beneficios**
- **Crear Beneficio**: `/crear-beneficio`

### **Ejemplo de Uso**

#### **Navegación Programática**
```javascript
import { useRouter } from 'next/navigation';
import { path } from '@/utils/routes';

const router = useRouter();

// Navegación simple
router.push('/evento/listadoEventos');

// Usando el mapping de rutas
router.push(path.listadoEventos());
router.push(path.crearReglaAcceso());

// Ruta dinámica
router.push(path.editarReglaAcceso(123));
```

#### **Uso del Mapping de Rutas**
```javascript
import { ROUTES, path } from '@/utils/routes';

// Acceso directo a rutas
const eventoRoute = ROUTES.evento.listadoEventos;
const reglaRoute = ROUTES.reglasAcceso.crearReglaAcceso;

// Usando builders (útil para rutas dinámicas)
const editarRoute = path.editarReglaAcceso(5); // → /reglas-acceso/editarReglaAcceso/5
```

---

## 🎯 **Notas Importantes**

1. **Rutas Dinámicas**: Las rutas de edición usan parámetros dinámicos `[id]` (ej: `/reglas-acceso/editarReglaAcceso/[id]`)
2. **Autenticación**: Las rutas están protegidas con `SecurityService.checkAuthAndRedirect()`
3. **Responsive**: Todas las páginas son responsive y adaptan su diseño según el tamaño de pantalla
4. **Estilos**: Utilizan el archivo `globals.css` y estilos específicos con `styled-jsx`
5. **Mapping de Rutas**: Todas las rutas están centralizadas en `src/utils/routes.js` para facilitar el mantenimiento
6. **Rutas Protegidas**: Las rutas de gestión requieren autenticación (ver `protectedRoutes` en `routes.js`)

---

## 🔄 **Actualizaciones**

- **Última actualización**: Diciembre 2024
- **Versión**: 1.0.0
- **Estado**: Desarrollo activo

---

*Documento generado automáticamente para el proyecto GateKeep*
