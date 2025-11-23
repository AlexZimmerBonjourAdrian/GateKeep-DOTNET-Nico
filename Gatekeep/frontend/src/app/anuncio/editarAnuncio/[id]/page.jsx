"use client";

import React, { useState, useEffect } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { usePathname, useRouter, useParams } from 'next/navigation';
import logo from '/public/assets/LogoGateKeep.webp';
import harvard from '/public/assets/Harvard.webp';
import { SecurityService } from '../../../../services/securityService';
import { AnuncioService } from '../../../../services/AnuncioService';

export default function EditarAnuncioPage() {
	const pathname = usePathname();
	const router = useRouter();
	const params = useParams();
	const anuncioId = parseInt(params.id, 10);

	SecurityService.checkAuthAndRedirect(pathname);

	const [loading, setLoading] = useState(true);
	const [nombre, setNombre] = useState('');
	const [fecha, setFecha] = useState('');
	const [descripcion, setDescripcion] = useState('');
	const [puntoControl, setPuntoControl] = useState('');
	const [submitting, setSubmitting] = useState(false);
	const [error, setError] = useState(null);
	const [success, setSuccess] = useState(false);

	// Cargar datos del anuncio
	useEffect(() => {
		if (!anuncioId || isNaN(anuncioId)) return;
		const fetchAnuncio = async () => {
			try {
				const response = await AnuncioService.getAnuncio(anuncioId);
				const anuncio = response.data;
				setNombre(anuncio.Nombre || anuncio.nombre || '');
				const fechaValue = anuncio.Fecha || anuncio.fecha;
				if (fechaValue) {
					const fechaObj = new Date(fechaValue);
					const fechaLocal = new Date(fechaObj.getTime() - fechaObj.getTimezoneOffset() * 60000);
					setFecha(fechaLocal.toISOString().slice(0, 10));
				}
				setDescripcion(anuncio.Descripcion || anuncio.descripcion || '');
				setPuntoControl(anuncio.PuntoControl || anuncio.puntoControl || '');
			} catch (e) {
				console.error('Error cargando anuncio:', e);
				setError('No se pudo cargar el anuncio');
			} finally {
				setLoading(false);
			}
		};
		fetchAnuncio();
	}, [anuncioId]);

	const validate = () => {
		if (!nombre || !fecha) return 'Nombre y Fecha son obligatorios';
		const today = new Date();
		today.setHours(0, 0, 0, 0);
		const selected = new Date(fecha);
		selected.setHours(0, 0, 0, 0);
		if (selected < today) return 'La fecha del anuncio no puede ser anterior a hoy';
		return null;
	};

	const handleSubmit = async (e) => {
		e.preventDefault();
		setError(null);
		setSuccess(false);
		const v = validate();
		if (v) { setError(v); return; }
		const fechaIso = new Date(fecha).toISOString();
		const payload = {
			nombre,
			fecha: fechaIso,
			descripcion: descripcion || undefined,
			puntoControl: puntoControl || undefined
		};
		console.log('Enviando PUT a anuncio ID:', anuncioId);
		console.log('Payload:', payload);
		setSubmitting(true);
		try {
			const response = await AnuncioService.updateAnuncio(anuncioId, payload);
			console.log('Respuesta recibida:', response);
			if (response.status >= 200 && response.status < 300) {
				setSuccess(true);
				setTimeout(() => router.push('/anuncio/listadoAnuncios'), 900);
			} else {
				setError('No se pudo actualizar el anuncio.');
			}
		} catch (e) {
			console.error('Error actualizando anuncio:', e);
			console.error('Error completo:', JSON.stringify(e, null, 2));
			setError(e.response?.data?.error || e.response?.data?.message || e.message || 'Error al actualizar el anuncio');
		} finally {
			setSubmitting(false);
		}
	};

	if (loading) return <div style={{color:'white', padding:'2rem'}}>Cargando...</div>;

	return (
		<div className="header-root">
			<div className="header-hero">
				<Image src={harvard} alt="Harvard" fill className="harvard-image" priority />
				<div className="header-overlay" />
				<div className="header-topbar">
					<div className="icon-group">
						<Link href="/">
							<Image src={logo} alt="Logo GateKeep" width={160} priority className="logo-image" />
						</Link>
					</div>
				</div>
				<div className="header-middle-bar">
					<form className="text-card" onSubmit={handleSubmit} aria-label="Formulario editar anuncio">
						<div style={{alignItems: 'center', width: '100%'}}>
							<button type="button" onClick={() => router.back()} style={{ background: 'transparent', border: '2px solid #F37426', color: '#F37426', padding: '6px 16px', borderRadius: '20px', cursor: 'pointer', marginBottom: '12px', fontSize: '0.9rem', fontWeight: '500', transition: 'all 0.2s' }} onMouseEnter={(e) => { e.target.style.background = '#F37426'; e.target.style.color = 'white'; }} onMouseLeave={(e) => { e.target.style.background = 'transparent'; e.target.style.color = '#F37426'; }}>← Regresar</button>
							<h1 className="text-3xl font-bold text-white">Editar Anuncio</h1>
							<hr />
						</div>
						<div className='input-container'>
							<div className='w-full'>
								<span>Nombre *</span>
								<input type="text" value={nombre} onChange={(e) => setNombre(e.target.value)} placeholder="Nombre del Anuncio" />
							</div>
							<div className='w-full'>
								<span>Fecha *</span>
								<input type="date" value={fecha} onChange={(e) => setFecha(e.target.value)} placeholder="Fecha del Anuncio" />
							</div>
							<div className='w-full'>
								<span>Descripción</span>
								<input type="text" value={descripcion} onChange={(e) => setDescripcion(e.target.value)} placeholder="Descripción del Anuncio" />
							</div>
						<div className='w-full'>
							<span>Punto de Control</span>
							<input type="text" value={puntoControl} onChange={(e) => setPuntoControl(e.target.value)} placeholder="Punto de Control del Anuncio" />
						</div>
					</div>
						{error && (
							<div style={{ color: '#ffdddd', background:'#7e1e1e', borderRadius:12, padding:'8px 14px', margin:'10px 1vw', width:'calc(100% - 2vw)', fontSize:'0.85rem' }}>{error}</div>
						)}
						{success && (
							<div style={{ color: '#e9ffe9', background:'#1e7e3a', borderRadius:12, padding:'8px 14px', margin:'10px 1vw', width:'calc(100% - 2vw)', fontSize:'0.85rem' }}>Anuncio actualizado. Redirigiendo...</div>
						)}
						<div className='button-container'>
							<button type="submit" disabled={submitting} style={{ opacity: submitting ? 0.6 : 1, cursor: submitting ? 'not-allowed' : 'pointer' }}>
								{submitting ? 'Actualizando...' : 'Actualizar Anuncio'}
							</button>
						</div>
					</form>
				</div>
			</div>
			<style jsx>{`
				.header-root { width: 100%; display: block; }
				.header-hero { width: 100%; height: 768px; position: relative; display: flex; flex-direction: column; gap: 5px; padding: 24px; box-sizing: border-box; }
				@media (max-width: 768px) { .header-hero { padding: 16px; height: 600px; } }
				@media (max-width: 425px) { .header-hero { padding: 12px; height: auto; } }
				.harvard-image { object-fit: cover; position: absolute; inset: 0; z-index: 0; }
				@media (max-width: 425px) { .harvard-image { display: none; } }
				.header-overlay { position: absolute; inset: 0; z-index: 1; pointer-events: none; box-shadow: inset 0 80px 120px rgba(0, 0, 0, 0.6), inset 0 -80px 120px rgba(0, 0, 0, 0.6); }
				.header-topbar { position: relative; z-index: 2; display: flex; align-items: center; justify-content: space-between; gap: 10px; min-height: 72px; }
				.logo-image { width: 160px; height: auto; cursor: pointer; opacity: 0.9; }
				@media (max-width: 425px) { .logo-image { width: 120px; } }
				.icon-group { display: inline-flex; align-items: center; gap: 10px; }
				span { font-size: 0.8rem; margin-left: 1vw; margin-right: 1vw; margin-bottom: 0; }
				h1 { color: #F37426; margin-left: 1vw; margin-right: 1vw; text-align: center; }
				input { border-radius: 20px; width: calc(100% - 2vw); margin-left: 1vw; margin-right: 1vw; margin-top: 0; padding: 8px; }
				@media (max-width: 425px) { input { padding: 6px; } }
				hr { width: 100%; border: 1.5px solid #F37426; }
				.input-container { display: flex; flex-direction: column; gap: 16px; width: 100%; }
				.button-container { width: 100%; display: flex; justify-content: center; align-items: center; }
				button { margin-top: 30px; border-radius: 20px; width: calc(80% - 2vw); padding: 8px; background: #F37426; margin-bottom: 20px; }
				@media (max-width: 425px) { button { width: 100%; padding: 10px; } }
				.text-card { display: flex; flex-direction: column; align-items: flex-start; width: 42.97vw; height: auto; background-color: #231F20; opacity: 0.75; padding: 0vw; border-radius: 20px; border: 3px solid #F37426; }
				@media (max-width: 768px) { .text-card { width: 90%; } }
				@media (max-width: 425px) { .text-card { width: 100%; } }
				.header-middle-bar { position: relative; z-index: 2; display: flex; justify-content: center; width: 100%; }
			`}</style>
		</div>
	);
}
