'use client'

import React from 'react';
import CrearBeneficioForm from '../../src/components/CrearBeneficioForm';

const CrearBeneficioPage = () => {
    return ( <
        div className = "crear-beneficio-page" >
        <
        div className = "crear-beneficio-logo" >
        <
        div className = "crear-beneficio-logo-icon" > 🔑 < /div> <
        span className = "crear-beneficio-logo-text" > Gatekeep < /span> <
        /div> <
        CrearBeneficioForm / >
        <
        /div>
    );
};

export default CrearBeneficioPage;