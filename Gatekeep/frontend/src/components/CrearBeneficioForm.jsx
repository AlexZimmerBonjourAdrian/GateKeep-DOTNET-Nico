import React, { useState } from 'react';

const CrearBeneficioForm = () => {
    const [formData, setFormData] = useState({
        tipoBeneficio: '',
        cupos: '',
        vigencia: '',
        vencimiento: ''
    });

    const handleFieldChange = (fieldName, value) => {
        setFormData(prev => ({
            ...prev,
            [fieldName]: value
        }));
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        console.log('Formulario de crear beneficio:', formData);
        // Aquí se implementaría la lógica para crear el beneficio
    };

    // Opciones para el dropdown de tipo de beneficio
    const tiposBeneficio = [
        { value: '', label: 'Seleccione un tipo de beneficio' },
        { value: 'descuento', label: 'Descuento' },
        { value: 'acceso_gratuito', label: 'Acceso Gratuito' },
        { value: 'premio', label: 'Premio' },
        { value: 'servicio_especial', label: 'Servicio Especial' },
        { value: 'otro', label: 'Otro' }
    ];

    return (
        <div className="crear-beneficio-container">
            <div className="crear-beneficio-form">
                <h1 className="crear-beneficio-title">Crear Beneficio</h1>
                <div className="crear-beneficio-underline"></div>
                
                <form onSubmit={handleSubmit} className="crear-beneficio-form-content">
                    <div className="crear-beneficio-input-group">
                        <label htmlFor="tipoBeneficio" className="crear-beneficio-label">Tipo de Beneficio</label>
                        <select
                            id="tipoBeneficio"
                            value={formData.tipoBeneficio}
                            onChange={(e) => handleFieldChange('tipoBeneficio', e.target.value)}
                            className="crear-beneficio-select"
                            required
                        >
                            {tiposBeneficio.map((tipo) => (
                                <option key={tipo.value} value={tipo.value}>
                                    {tipo.label}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="crear-beneficio-input-group">
                        <label htmlFor="cupos" className="crear-beneficio-label">Cupos</label>
                        <input
                            id="cupos"
                            type="number"
                            min="1"
                            value={formData.cupos}
                            onChange={(e) => handleFieldChange('cupos', e.target.value)}
                            className="crear-beneficio-input"
                            placeholder="Número de cupos disponibles"
                            required
                        />
                    </div>

                    <div className="crear-beneficio-input-group">
                        <label htmlFor="vigencia" className="crear-beneficio-label">Vigencia</label>
                        <input
                            id="vigencia"
                            type="date"
                            value={formData.vigencia}
                            onChange={(e) => handleFieldChange('vigencia', e.target.value)}
                            className="crear-beneficio-input"
                            required
                        />
                    </div>

                    <div className="crear-beneficio-input-group">
                        <label htmlFor="vencimiento" className="crear-beneficio-label">Vencimiento</label>
                        <input
                            id="vencimiento"
                            type="date"
                            value={formData.vencimiento}
                            onChange={(e) => handleFieldChange('vencimiento', e.target.value)}
                            className="crear-beneficio-input"
                            required
                        />
                    </div>

                    <button
                        type="submit"
                        className="crear-beneficio-button"
                    >
                        Crear
                    </button>

                    <div className="crear-beneficio-separator"></div>

                    <div className="crear-beneficio-register-link">
                        <span>¿No tienes una cuenta? </span>
                        <span className="crear-beneficio-register-text">Registrarse</span>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default CrearBeneficioForm;
