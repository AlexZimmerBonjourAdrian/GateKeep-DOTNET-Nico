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

### **Crear Evento**
- **URL**: `/crear-evento`
- **Archivo**: `app/crear-evento/page.js`
- **Descripción**: Formulario para crear nuevos eventos
- **Componente**: CrearEventoForm
- **Campos**: Nombre, Fecha, Resultado, Punto De Control

### **Crear Beneficio**
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
│   ├── page.js                    → /
│   ├── auth/
│   │   ├── login/page.js         → /auth/login
│   │   └── register/page.js      → /auth/register
│   ├── crear-evento/page.js      → /crear-evento
│   └── crear-beneficio/page.js   → /crear-beneficio
└── src/
    └── components/               → Componentes reutilizables
```

### **Puerto de Desarrollo**
- **URL Base**: `http://localhost:3000`
- **Comando**: `npm run dev`

---

## 📱 **Navegación**

### **Enlaces de Navegación**
- **Inicio**: `/`
- **Login**: `/auth/login`
- **Registro**: `/auth/register`
- **Crear Evento**: `/crear-evento`
- **Crear Beneficio**: `/crear-beneficio`

### **Ejemplo de Uso**
```javascript
// Navegación programática
import { useRouter } from 'next/navigation';

const router = useRouter();
router.push('/crear-evento');
```

---

## 🎯 **Notas Importantes**

1. **Rutas Dinámicas**: Todas las rutas son estáticas en este momento
2. **Autenticación**: Las rutas de auth no tienen protección implementada
3. **Responsive**: Todas las páginas son responsive
4. **Estilos**: Utilizan el archivo `globals.css` y estilos específicos

---

## 🔄 **Actualizaciones**

- **Última actualización**: Diciembre 2024
- **Versión**: 1.0.0
- **Estado**: Desarrollo activo

---

*Documento generado automáticamente para el proyecto GateKeep*
