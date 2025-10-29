// Mapa de rutas centralizadas
export const ROUTES = {
    home: '/',
    auth: {
        login: '/auth/login',
        register: '/auth/register',
    },
    evento: {
        crearEvento: '/evento/crearEvento',
        listadoEventos: '/evento/listadoEventos',
    },
    crearEvento: '/crear-evento',
    crearBeneficio: '/crear-beneficio',
};

// Builders (útil si luego agregas segmentos dinámicos o querystrings)
export const path = {
    home: () => ROUTES.home,
    login: () => ROUTES.auth.login,
    register: () => ROUTES.auth.register,
    crearEvento: () => ROUTES.evento.crearEvento,
    listadoEventos: () => ROUTES.evento.listadoEventos,
    crearBeneficio: () => ROUTES.crearBeneficio,
};

// Navegación (para menús, sidebars, header)
export const navLinks = [
    { href: ROUTES.home, label: 'Inicio' },
    { href: ROUTES.auth.login, label: 'Login' },
    { href: ROUTES.auth.register, label: 'Registro' },
    { href: ROUTES.evento.crearEvento, label: 'Crear Evento' },
    { href: ROUTES.evento.listadoEventos, label: 'Listado Eventos' },
    { href: ROUTES.crearBeneficio, label: 'Crear Beneficio' },
];

// Guards simples (si luego agregas auth)
export const protectedRoutes = new Set([
    ROUTES.evento.crearEvento,
    ROUTES.evento.listadoEventos,
    ROUTES.crearBeneficio,
]);

export const publicRoutes = new Set([
    ROUTES.home,
    ROUTES.auth.login,
    ROUTES.auth.register,
]);

export function isProtected(pathname) {
    return protectedRoutes.has(pathname);
}