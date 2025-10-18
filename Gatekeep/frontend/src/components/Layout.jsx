import React from 'react';
import { Outlet } from 'react-router-dom';
import { Menubar } from 'primereact/menubar';

const Layout = () => {
  const items = [
    {
      label: 'Inicio',
      icon: 'pi pi-home',
      url: '/'
    },
    {
      label: 'Acerca de',
      icon: 'pi pi-info-circle',
      command: () => {
        console.log('Acerca de');
      }
    },
    {
      label: 'Contacto',
      icon: 'pi pi-envelope',
      command: () => {
        console.log('Contacto');
      }
    }
  ];

  const start = (
    <div className="flex items-center">
      <i className="pi pi-code text-2xl text-primary mr-2"></i>
      <span className="text-xl font-bold text-primary">React Template</span>
    </div>
  );

  const end = (
    <div className="flex items-center gap-2">
      <button className="p-button p-button-outlined p-button-sm">
        <i className="pi pi-user mr-2"></i>
        Login
      </button>
    </div>
  );

  return (
    <div className="min-h-screen bg-gray-50">
      <Menubar 
        model={items} 
        start={start} 
        end={end}
        className="shadow-sm border-0 bg-white"
      />
      <main>
        <Outlet />
      </main>
      <footer className="bg-gray-800 text-white py-8 mt-12">
        <div className="container mx-auto px-4 text-center">
          <p>&copy; 2024 React Template. Todos los derechos reservados.</p>
          <p className="text-sm text-gray-400 mt-2">
            Desarrollado con React, PrimeReact y Tailwind CSS
          </p>
        </div>
      </footer>
    </div>
  );
};

export default Layout;