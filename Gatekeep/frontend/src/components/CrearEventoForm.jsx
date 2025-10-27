import React, { useState } from 'react';

const CrearEventoForm = () => {
    const [formData, setFormData] = useState({
        nombre: '',
        fecha: '',
        resultado: '',
        puntoControl: ''
    });

    const handleFieldChange = (fieldName, value) => {
        setFormData(prev => ({
            ...prev,
            [fieldName]: value
        }));
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        console.log('Formulario de crear evento:', formData);
        // Aquí se implementaría la lógica para crear el evento
    };

    return (
        <div className="crear-evento-container">
            <div className="crear-evento-form">
                <h1 className="crear-evento-title">Crear Evento</h1>
                <div className="crear-evento-underline"></div>
                
                <form onSubmit={handleSubmit} className="crear-evento-form-content">
                    <div className="crear-evento-input-group">
                        <label htmlFor="nombre" className="crear-evento-label">Nombre</label>
                        <input
                            id="nombre"
                            type="text"
                            value={formData.nombre}
                            onChange={(e) => handleFieldChange('nombre', e.target.value)}
                            className="crear-evento-input crear-evento-input-wide"
                            required
                        />
                    </div>

                    <div className="crear-evento-input-group">
                        <label htmlFor="fecha" className="crear-evento-label">Fecha</label>
                        <input
                            id="fecha"
                            type="date"
                            value={formData.fecha}
                            onChange={(e) => handleFieldChange('fecha', e.target.value)}
                            className="crear-evento-input crear-evento-input-wide"
                            required
                        />
                    </div>

                    <div className="crear-evento-row">
                        <div className="crear-evento-input-group crear-evento-input-half">
                            <label htmlFor="resultado" className="crear-evento-label">Resultado</label>
                            <input
                                id="resultado"
                                type="text"
                                value={formData.resultado}
                                onChange={(e) => handleFieldChange('resultado', e.target.value)}
                                className="crear-evento-input"
                                required
                            />
                        </div>

                        <div className="crear-evento-input-group crear-evento-input-half">
                            <label htmlFor="puntoControl" className="crear-evento-label">Punto De Control</label>
                            <input
                                id="puntoControl"
                                type="text"
                                value={formData.puntoControl}
                                onChange={(e) => handleFieldChange('puntoControl', e.target.value)}
                                className="crear-evento-input"
                                required
                            />
                        </div>
                    </div>

                    <button
                        type="submit"
                        className="crear-evento-button"
                    >
                        Crear
                    </button>

                    <div className="crear-evento-separator"></div>

                    <div className="crear-evento-login-link">
                        <span>¿Ya tienes una cuenta? </span>
                        <span className="crear-evento-login-text">Inicia Sesión</span>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default CrearEventoForm;
