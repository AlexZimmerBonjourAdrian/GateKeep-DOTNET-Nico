# React Template

Un template limpio y reutilizable para proyectos React con PrimeReact y Tailwind CSS.

## Características

- ⚡ **Vite** - Build tool rápido y moderno
- ⚛️ **React 18** - Biblioteca de interfaz de usuario
- 🎨 **PrimeReact** - Biblioteca de componentes UI
- 🎯 **Tailwind CSS** - Framework de CSS utilitario
- 🛣️ **React Router DOM** - Enrutamiento del lado del cliente
- 🔧 **ESLint** - Linter para JavaScript/React
- 📱 **Responsive Design** - Diseño adaptable a todos los dispositivos

## Estructura del Proyecto

```
src/
├── components/          # Componentes reutilizables
│   └── Layout.jsx      # Layout principal de la aplicación
├── pages/              # Páginas de la aplicación
│   └── Home.jsx        # Página de inicio
├── styles/             # Estilos globales
│   └── global.css      # Estilos base y variables CSS
├── utils/              # Utilidades y configuraciones
│   └── primeReactConfig.js  # Configuración de PrimeReact
├── App.jsx             # Componente principal
└── main.jsx            # Punto de entrada de la aplicación
```

## Instalación

1. Clona o descarga este template
2. Instala las dependencias:
   ```bash
   npm install
   ```

## Desarrollo

Para iniciar el servidor de desarrollo:

```bash
npm run dev
```

La aplicación estará disponible en `http://localhost:5173`

## Scripts Disponibles

- `npm run dev` - Inicia el servidor de desarrollo
- `npm run build` - Construye la aplicación para producción
- `npm run preview` - Previsualiza la build de producción
- `npm run lint` - Ejecuta el linter

## Configuración

### PrimeReact
El template incluye PrimeReact configurado con el tema `lara-light-cyan`. Puedes cambiar el tema modificando `src/utils/primeReactConfig.js`.

### Tailwind CSS
Tailwind está configurado con colores personalizados. Puedes modificar la configuración en `tailwind.config.js`.

### Proxy API
El proxy está configurado para redirigir las peticiones `/api` a `http://localhost:5000`. Modifica `vite.config.js` para cambiar el puerto del backend.

## Personalización

1. **Cambiar el nombre del proyecto**: Modifica `package.json` y `index.html`
2. **Personalizar colores**: Edita las variables CSS en `src/styles/global.css`
3. **Agregar páginas**: Crea nuevos componentes en `src/pages/` y agrega rutas en `App.jsx`
4. **Agregar componentes**: Crea componentes reutilizables en `src/components/`

## Tecnologías

- **React 18.2.0** - Biblioteca de interfaz de usuario
- **Vite 5.4.18** - Build tool y servidor de desarrollo
- **PrimeReact 10.9.4** - Biblioteca de componentes UI
- **Tailwind CSS 3.4.1** - Framework de CSS utilitario
- **React Router DOM 6.30.0** - Enrutamiento
- **Axios 1.8.4** - Cliente HTTP

## Licencia

Este template es de código abierto y está disponible bajo la licencia MIT.