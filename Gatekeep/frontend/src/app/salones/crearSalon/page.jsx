"use client";

import React, { useState, useEffect } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import logo from '/public/assets/LogoGateKeep.webp';
import harvard from '/public/assets/Harvard.webp';
import { SecurityService } from '../../../services/securityService';
import { SalonService } from '../../../services/SalonService';
import { EdificioService } from '../../../services/EdificioService';

export default function CrearSalonPage() {
	const pathname = usePathname();
	const router = useRouter();
	SecurityService.checkAuthAndRedirect(pathname);

	const [isAdmin, setIsAdmin] = useState(false);
	useEffect(() => {
		try {
			const tipo = SecurityService.getTipoUsuario?.() || null;
			let admin = false;
			if (tipo) {
				admin = /admin|administrador/i.test(String(tipo));
			} else if (typeof window !== 'undefined') {
				const raw = localStorage.getItem('user');
				if (raw) {
					const user = JSON.parse(raw);
					const role = user?.TipoUsuario || user?.tipoUsuario || user?.Rol || user?.rol;
					if (role) admin = /admin|administrador/i.test(String(role));
				}
			}
			setIsAdmin(admin);
			if (!admin) router.replace('/');
		} catch {
			setIsAdmin(false);
			router.replace('/');
		}
	}, [router]);

	const [edificios, setEdificios] = useState([]);
	useEffect(() => {
		const fetchEdificios = async () => {
			try {
				const response = await EdificioService.getEdificios();
				setEdificios(Array.isArray(response.data) ? response.data : []);
			} catch (e) {
				console.error('Error al cargar edificios:', e);
			}
		};
		fetchEdificios();
	}, []);

	const [nombre, setNombre] = useState('');
	const [descripcion, setDescripcion] = useState('');
	const [ubicacion, setUbicacion] = useState('');
	const [capacidad, setCapacidad] = useState('');
	const [numeroSalon, setNumeroSalon] = useState('');
	const [edificioId, setEdificioId] = useState('');
	const [activo, setActivo] = useState(true);
	const [submitting, setSubmitting] = useState(false);
	const [error, setError] = useState(null);
	const [success, setSuccess] = useState(false);

	const validate = () => {
		if (!nombre || !ubicacion || !capacidad || !numeroSalon || !edificioId) return 'Completa los campos obligatorios.';
		if (Number(capacidad) < 0) return 'Capacidad debe ser >= 0.';
		if (Number(numeroSalon) < 0) return 'Número de salón debe ser >= 0.';
		return null;
	};

	const handleSubmit = async () => {
		setError(null);
		setSuccess(false);
		const v = validate();
		if (v) { setError(v); return; }
		const payload = {
			nombre,
			descripcion: descripcion || undefined,
			ubicacion,
			capacidad: Number(capacidad),
			numeroSalon: Number(numeroSalon),
			edificioId: Number(edificioId),
			activo
		};
		setSubmitting(true);
		try {
			const response = await SalonService.createSalon(payload);
			if (response.status >= 200 && response.status < 300) {
				setSuccess(true);
				setTimeout(() => router.push('/salones/listadoSalones'), 900);
			} else {
				setError('No se pudo crear el salón.');
			}
		} catch (e) {
			console.error('Error creando salón:', e);
			setError(e.response?.data?.error || e.response?.data?.message || 'Error al crear el salón');
		} finally {
			setSubmitting(false);
		}
	};

	if (!isAdmin) return null;

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
					<form className="text-card" onSubmit={(e) => { e.preventDefault(); if (!submitting) handleSubmit(); }} aria-label="Formulario crear salón">
						<div style={{alignItems: 'center', width: '100%'}}>
							<h1 className="text-3xl font-bold text-white">Crear Salón</h1>
							<hr />
						</div>
						<div className='input-container'>
							<div className='w-full'>
								<span>Nombre *</span>
								<input type="text" value={nombre} onChange={(e) => setNombre(e.target.value)} placeholder="Nombre del salón" />
							</div>
							<div className='w-full'>
								<span>Ubicación *</span>
								<input type="text" value={ubicacion} onChange={(e) => setUbicacion(e.target.value)} placeholder="Ej: Piso 2, Ala Norte" />
							</div>
							<div className='w-full'>
								<span>Número de Salón *</span>
								<input type="number" value={numeroSalon} onChange={(e) => setNumeroSalon(e.target.value)} placeholder="Ej: 201" />
							</div>
							<div className='w-full'>
								<span>Capacidad *</span>
								<input type="number" value={capacidad} onChange={(e) => setCapacidad(e.target.value)} placeholder="Capacidad de personas" />
							</div>
							<div className='w-full'>
								<span>Edificio *</span>
								<select 
									value={edificioId} 
									onChange={(e) => setEdificioId(e.target.value)}
									style={{
										borderRadius:'20px', 
										width:'calc(100% - 2vw)', 
										marginLeft:'1vw', 
										marginRight:'1vw', 
										padding:'8px',
										backgroundColor: 'white'
									}}
								>
									<option value="">Selecciona un edificio</option>
									{edificios.map(e => (
										<option key={e.Id || e.id} value={e.Id || e.id}>
											{e.Nombre || e.nombre}
										</option>
									))}
								</select>
							</div>
							<div className='w-full'>
								<span>Descripción (opcional)</span>
								<textarea 
									style={{
										borderRadius:'20px', 
										width:'calc(100% - 2vw)', 
										marginLeft:'1vw', 
										marginRight:'1vw', 
										padding:'8px', 
										minHeight:'80px'
									}} 
									value={descripcion} 
									onChange={(e) => setDescripcion(e.target.value)} 
									placeholder="Descripción del salón" 
								/>
							</div>
							<div className='w-full' style={{display:'flex', alignItems:'center', gap:'8px', marginLeft:'1vw', marginRight:'1vw'}}>
								<input id="activo-salon" type="checkbox" checked={activo} onChange={(e) => setActivo(e.target.checked)} />
								<label htmlFor="activo-salon" style={{margin:0, fontSize:'0.8rem'}}>Activo</label>
							</div>
						</div>
						{error && (
							<div style={{ color: '#ffdddd', background:'#7e1e1e', borderRadius:12, padding:'8px 14px', margin:'10px 1vw', width:'calc(100% - 2vw)', fontSize:'0.85rem' }}>{error}</div>
						)}
						{success && (
							<div style={{ color: '#e9ffe9', background:'#1e7e3a', borderRadius:12, padding:'8px 14px', margin:'10px 1vw', width:'calc(100% - 2vw)', fontSize:'0.85rem' }}>Salón creado. Redirigiendo...</div>
						)}
						<div className='button-container'>
							<button type="submit" disabled={submitting} style={{ opacity: submitting ? 0.6 : 1, cursor: submitting ? 'not-allowed' : 'pointer' }}>
								{submitting ? 'Creando...' : 'Crear Salón'}
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
				input, select { border-radius: 20px; width: calc(100% - 2vw); margin-left: 1vw; margin-right: 1vw; margin-top: 0; padding: 8px; }
				@media (max-width: 425px) { input, select { padding: 6px; } }
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
