import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext.jsx';
import { useRouter } from 'next/navigation';

const Register = () => {
    const { register, isAuthenticated, error, clearError } = useAuth();
    const router = useRouter();

    const [formData, setFormData] = useState({
        nombre: '',
        apellido: '',
        email: '',
        password: '',
        confirmPassword: '',
        telefono: ''
    });
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [localError, setLocalError] = useState(null);

    const handleFieldChange = (fieldName, value) => {
        setFormData(prev => ({
            ...prev,
            [fieldName]: value
        }));
    };
    
    // Redirigir si ya está autenticado
    useEffect(() => {
        if (isAuthenticated) {
            router.push('/');
        }
    }, [isAuthenticated, router]);

    // Limpiar errores al modificar campos
    useEffect(() => {
        setLocalError(null);
        if (error) clearError();
    }, [formData.nombre, formData.apellido, formData.email, formData.password, formData.confirmPassword, formData.telefono]);

    const validate = () => {
        if (!formData.email.includes('@')) return 'Email inválido';
        if (formData.password.length < 6) return 'La contraseña debe tener al menos 6 caracteres';
        if (formData.password !== formData.confirmPassword) return 'Las contraseñas no coinciden';
        return null;
    };

    const handleRegister = async (e) => {
        e.preventDefault();
        if (isSubmitting) return;

        const validation = validate();
        if (validation) {
            setLocalError(validation);
            return;
        }

        setIsSubmitting(true);
        clearError();
        setLocalError(null);

        try {
            const result = await register({
                nombre: formData.nombre,
                apellido: formData.apellido,
                email: formData.email,
                password: formData.password,
                telefono: formData.telefono || undefined,
            });
            if (result.success) {
                router.push('/');
            }
        } catch (err) {
            // error global manejado por el contexto
        } finally {
            setIsSubmitting(false);
        }
    };

    const styles = {
        mainContainer: {
            minHeight: '100vh',
            backgroundImage: 'url("https://images.unsplash.com/photo-1562774053-701939374585?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=2070&q=80")',
            backgroundSize: 'cover',
            backgroundPosition: 'center',
            backgroundRepeat: 'no-repeat',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            position: 'relative'
        },
        backgroundOverlay: {
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0, 0, 0, 0.3)',
            backdropFilter: 'blur(2px)'
        },
        logo: {
            position: 'absolute',
            top: '2rem',
            left: '2rem',
            zIndex: 10,
            display: 'flex',
            alignItems: 'center',
            gap: '0.5rem'
        },
        logoIcon: {
            width: '50px',
            height: '50px',
            backgroundColor: '#FF6B35',
            borderRadius: '50%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#FFFFFF',
            fontSize: '1.5rem',
            fontWeight: 'bold'
        },
        logoText: {
            color: '#FF6B35',
            fontSize: '1.8rem',
            fontWeight: 'bold',
            fontStyle: 'italic'
        },
        formContainer: {
            width: '90%',
            maxWidth: '500px',
            backgroundColor: 'rgba(60, 60, 60, 0.8)',
            borderRadius: '20px',
            border: '2px solid #FF6B35',
            padding: '2.5rem',
            boxShadow: '0 8px 32px rgba(0,0,0,0.3)',
            backdropFilter: 'blur(10px)',
            position: 'relative',
            zIndex: 5
        },
        title: {
            color: '#FFFFFF',
            textAlign: 'center',
            fontSize: '2rem',
            fontWeight: 'bold',
            marginBottom: '1rem',
            textTransform: 'uppercase'
        },
        titleUnderline: {
            width: '60px',
            height: '3px',
            backgroundColor: '#FF6B35',
            margin: '0 auto 2rem auto'
        },
        formRow: {
            display: 'flex',
            gap: '1rem',
            marginBottom: '1.5rem'
        },
        inputGroup: {
            flex: 1,
            marginBottom: '1.5rem'
        },
        label: {
            display: 'block',
            marginBottom: '0.5rem',
            color: '#FFFFFF',
            fontSize: '0.9rem',
            fontWeight: '500'
        },
        input: {
            width: '100%',
            padding: '0.75rem',
            borderRadius: '8px',
            border: '1px solid #FF6B35',
            backgroundColor: '#FFFFFF',
            fontSize: '1rem'
        },
        registerButton: {
            width: '100%',
            padding: '1rem',
            backgroundColor: '#FF6B35',
            border: 'none',
            borderRadius: '8px',
            color: '#FFFFFF',
            fontWeight: 'bold',
            cursor: 'pointer',
            fontSize: '1.1rem',
            marginTop: '1.5rem',
            textTransform: 'uppercase',
            transition: 'all 0.3s ease'
        },
        loadingButton: {
            width: '100%',
            padding: '1rem',
            backgroundColor: '#9CA3AF',
            border: 'none',
            borderRadius: '8px',
            color: '#FFFFFF',
            fontWeight: 'bold',
            cursor: 'not-allowed',
            fontSize: '1.1rem',
            marginTop: '1.5rem',
            textTransform: 'uppercase',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: '0.5rem'
        },
        loadingSpinner: {
            width: '20px',
            height: '20px',
            border: '2px solid #FFFFFF',
            borderTop: '2px solid transparent',
            borderRadius: '50%',
            animation: 'spin 1s linear infinite'
        },
        errorMessage: {
            backgroundColor: 'rgba(220, 38, 38, 0.1)',
            border: '1px solid #DC2626',
            color: '#FEE2E2',
            padding: '0.75rem',
            borderRadius: '8px',
            marginBottom: '1rem',
            fontSize: '0.9rem',
            textAlign: 'center'
        },
        separator: {
            width: '20px',
            height: '20px',
            backgroundColor: '#FF6B35',
            borderRadius: '50%',
            margin: '1rem auto',
            display: 'block'
        },
        loginLink: {
            textAlign: 'center',
            marginTop: '1rem',
            fontSize: '0.9rem',
            color: '#FFFFFF'
        },
        loginLinkText: {
            color: '#FF6B35',
            textDecoration: 'none',
            cursor: 'pointer',
            fontWeight: 'bold'
        }
    };

    return (
        <>
        <style>
            {`
                @keyframes spin {
                    0% { transform: rotate(0deg); }
                    100% { transform: rotate(360deg); }
                }
            `}
        </style>
        <div style={styles.mainContainer}>
            <div style={styles.backgroundOverlay}></div>

            <div style={styles.logo}>
                <div style={styles.logoIcon}>GK</div>
                <span style={styles.logoText}>Gatekeep</span>
            </div>
              
            <div style={styles.formContainer}>
                <h1 style={styles.title}>CREAR CUENTA</h1>
                <div style={styles.titleUnderline}></div>
                
                <form onSubmit={handleRegister}>
                    {(localError || error) && (
                        <div style={styles.errorMessage}>
                            {localError || error}
                        </div>
                    )}
                    <div style={styles.formRow}>
                        <div style={styles.inputGroup}>
                            <label htmlFor="nombre" style={styles.label}>Nombre</label>
                            <input
                                id="nombre"
                                type="text"
                                value={formData.nombre}
                                onChange={(e) => handleFieldChange('nombre', e.target.value)}
                                style={styles.input}
                                required
                                disabled={isSubmitting}
                            />
                        </div>
                        <div style={styles.inputGroup}>
                            <label htmlFor="apellido" style={styles.label}>Apellido</label>
                            <input
                                id="apellido"
                                type="text"
                                value={formData.apellido}
                                onChange={(e) => handleFieldChange('apellido', e.target.value)}
                                style={styles.input}
                                required
                                disabled={isSubmitting}
                            />
                        </div>
                    </div>

                    <div style={styles.inputGroup}>
                        <label htmlFor="email" style={styles.label}>Email</label>
                        <input
                            id="email"
                            type="email"
                            value={formData.email}
                            onChange={(e) => handleFieldChange('email', e.target.value)}
                            style={styles.input}
                            required
                            disabled={isSubmitting}
                        />
                    </div>

                    <div style={styles.inputGroup}>
                        <label htmlFor="telefono" style={styles.label}>Teléfono (opcional)</label>
                        <input
                            id="telefono"
                            type="text"
                            value={formData.telefono}
                            onChange={(e) => handleFieldChange('telefono', e.target.value)}
                            style={styles.input}
                            disabled={isSubmitting}
                        />
                    </div>

                    <div style={styles.formRow}>
                        <div style={styles.inputGroup}>
                            <label htmlFor="password" style={styles.label}>Contraseña</label>
                            <input
                                id="password"
                                type="password"
                                value={formData.password}
                                onChange={(e) => handleFieldChange('password', e.target.value)}
                                style={styles.input}
                                required
                            disabled={isSubmitting}
                            />
                        </div>
                        <div style={styles.inputGroup}>
                            <label htmlFor="confirmPassword" style={styles.label}>Repetir Contraseña</label>
                            <input
                                id="confirmPassword"
                                type="password"
                                value={formData.confirmPassword}
                                onChange={(e) => handleFieldChange('confirmPassword', e.target.value)}
                                style={styles.input}
                                required
                                disabled={isSubmitting}
                            />
                        </div>
                    </div>

                    <button
                        type="submit"
                        style={isSubmitting ? styles.loadingButton : styles.registerButton}
                        disabled={isSubmitting}
                    >
                        {isSubmitting ? (
                            <>
                                <div style={styles.loadingSpinner}></div>
                                REGISTRANDO...
                            </>
                        ) : (
                            'REGISTRARSE'
                        )}
                    </button>

                    <div style={styles.separator}></div>

                    <div style={styles.loginLink}>
                        <span>¿Ya tienes una cuenta? </span>
                        <span 
                            style={styles.loginLinkText}
                        >
                            Iniciar sesión
                        </span>
                    </div>
                </form>
            </div>
        </div>
        </>
    );
};

export default Register;
