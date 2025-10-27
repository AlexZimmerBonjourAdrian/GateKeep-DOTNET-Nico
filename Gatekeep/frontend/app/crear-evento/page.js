'use client'

import React from 'react';
import CrearEventoForm from '../../src/components/CrearEventoForm';

const CrearEventoPage = () => {
    return ( <
        div className = "crear-evento-page" >
        <
        div className = "crear-evento-logo" >
        <
        div className = "crear-evento-logo-icon" > 🔑 < /div> <
        span className = "crear-evento-logo-text" > Gatekeep < /span> <
        /div> <
        CrearEventoForm / >
        <
        /div>
    );
};

export default CrearEventoPage;