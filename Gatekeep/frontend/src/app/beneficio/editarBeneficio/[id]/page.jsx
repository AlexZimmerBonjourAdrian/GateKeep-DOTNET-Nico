"use client";

import React, { useState, useEffect } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { usePathname, useRouter, useParams } from 'next/navigation';
import logo from '/public/assets/LogoGateKeep.webp';
import harvard from '/public/assets/Harvard.webp';
import { SecurityService } from '../../../../services/securityService';
import { BeneficioService } from '../../../../services/BeneficioService';

export default function EditarBeneficioPage() {
	const pathname = usePathname();
	const router = useRouter();
	const params = useParams();
	const beneficioId = parseInt(params.id, 10);

	SecurityService.checkAuthAndRedirect(pathname);

	const [isAdmin, setIsAdmin] = useState(false);
	const [loading, setLoading] = useState(true);
	const [tipo, setTipo] = useState(0);
	const [vigencia, setVigencia] = useState(true);
	const [fechaDeVencimiento, setFechaDeVencimiento] = useState('');
	const [cupos, setCupos] = useState(1);
	const [submitting, setSubmitting] = useState(false);
	const [error, setError] = useState(null);
	const [success, setSuccess] = useState(false);

	useEffect(() => {
		try {
			const tipoUsuario = SecurityService.getTipoUsuario?.() || null;
			let admin = false;
			if (tipoUsuario) {
				admin = /admin|administrador/i.test(String(tipoUsuario));
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

	// Cargar datos del beneficio
	useEffect(() => {
		if (!beneficioId || isNaN(beneficioId)) return;
		const fetchBeneficio = async () => {
			try {
				const response = await BeneficioService.getBeneficioById(beneficioId);
				const beneficio = response.data;
				setTipo(beneficio.Tipo ?? beneficio.tipo ?? 0);
				setVigencia(beneficio.Vigencia ?? beneficio.vigencia ?? true);
				const fechaValue = beneficio.FechaDeVencimiento || beneficio.fechaDeVencimiento;
				if (fechaValue) {
					const fechaObj = new Date(fechaValue);
					const fechaLocal = new Date(fechaObj.getTime() - fechaObj.getTimezoneOffset() * 60000);
					setFechaDeVencimiento(fechaLocal.toISOString().slice(0, 16));
				}
				setCupos(beneficio.Cupos ?? beneficio.cupos ?? 1);
			} catch (e) {
				console.error('Error cargando beneficio:', e);
				setError('No se pudo cargar el beneficio');
			} finally {
				setLoading(false);
			}
		};
		fetchBeneficio();
	}, [beneficioId]);

	const validate = () => {
		if (!fechaDeVencimiento || cupos < 1) return 'Fecha de vencimiento y cupos (mínimo 1) son obligatorios';
		const vencimiento = new Date(fechaDeVencimiento);
		const hoy = new Date();
		if (vencimiento <= hoy) return 'La fecha de vencimiento debe ser posterior a la fecha actual';
		return null;
	};

	const handleSubmit = async (e) => {
		e.preventDefault();
		setError(null);
		setSuccess(false);
		const v = validate();
		if (v) { setError(v); return; }
		const fechaVencimientoIso = new Date(fechaDeVencimiento).toISOString();
		const payload = {
			tipo,
			vigencia,
			fechaDeVencimiento: fechaVencimientoIso,
			cupos
		};
		console.log('Enviando PUT a beneficio ID:', beneficioId);
		console.log('Payload:', payload);
		setSubmitting(true);
		try {
			const response = await BeneficioService.actualizarBeneficio(beneficioId, payload);
			console.log('Respuesta recibida:', response);
			if (response.status >= 200 && response.status < 300) {
				setSuccess(true);
				setTimeout(() => router.push('/beneficio/listadoBeneficios'), 900);
			} else {
				setError('No se pudo actualizar el beneficio.');
			}
		} catch (e) {
			console.error('Error actualizando beneficio:', e);
			console.error('Error completo:', JSON.stringify(e, null, 2));
			setError(e.response?.data?.error || e.response?.data?.message || e.message || 'Error al actualizar el beneficio');
		} finally {
			setSubmitting(false);
		}
	};

	if (!isAdmin || loading) return <div style={{color:'white', padding:'2rem'}}>Cargando...</div>;

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
					<form className="text-card" onSubmit={handleSubmit} aria-label="Formulario editar beneficio">
						<div style={{alignItems: 'center', width: '100%'}}>
							<button type="button" onClick={() => router.back()} style={{ background: 'transparent', border: '2px solid #F37426', color: '#F37426', padding: '6px 16px', borderRadius: '20px', cursor: 'pointer', marginBottom: '12px', fontSize: '0.9rem', fontWeight: '500', transition: 'all 0.2s' }} onMouseEnter={(e) => { e.target.style.background = '#F37426'; e.target.style.color = 'white'; }} onMouseLeave={(e) => { e.target.style.background = 'transparent'; e.target.style.color = '#F37426'; }}>← Regresar</button>
							<h1 className="text-3xl font-bold text-white">Editar Beneficio</h1>
							<hr />
						</div>
						<div className='input-container'>
							<div className='w-full'>
								<span>Tipo de Beneficio *</span>
								<select 
									value={tipo} 
									onChange={(e) => setTipo(parseInt(e.target.value))}
									style={{borderRadius:'20px', width:'calc(100% - 2vw)', marginLeft:'1vw', marginRight:'1vw', padding:'8px'}}
								>
									<option value={0}>Canje</option>
									<option value={1}>Consumo</option>
								</select>
							</div>
							<div className='w-full'>
								<span>Fecha de Vencimiento *</span>
								<input 
									type="datetime-local" 
									value={fechaDeVencimiento} 
									onChange={(e) => setFechaDeVencimiento(e.target.value)}
									style={{borderRadius:'20px', width:'calc(100% - 2vw)', marginLeft:'1vw', marginRight:'1vw', padding:'8px'}}
								/>
							</div>
							<div className='w-full'>
								<span>Cupos Disponibles *</span>
								<input 
									type="number" 
									min="1" 
									placeholder="Cantidad de cupos" 
									value={cupos} 
									onChange={(e) => setCupos(parseInt(e.target.value) || 1)} 
								/>
							</div>
							<div className='w-full' style={{display:'flex', alignItems:'center', gap:'8px', marginLeft:'1vw', marginRight:'1vw'}}>
								<input 
									id="vigencia-beneficio" 
									type="checkbox" 
									checked={vigencia} 
									onChange={(e) => setVigencia(e.target.checked)} 
								/>
								<label htmlFor="vigencia-beneficio" style={{margin:0, fontSize:'0.8rem'}}>Vigente</label>
							</div>
						</div>
						{error && (
							<div style={{ color: '#ffdddd', background:'#7e1e1e', borderRadius:12, padding:'8px 14px', margin:'10px 1vw', width:'calc(100% - 2vw)', fontSize:'0.85rem' }}>{error}</div>
						)}
						{success && (
							<div style={{ color: '#e9ffe9', background:'#1e7e3a', borderRadius:12, padding:'8px 14px', margin:'10px 1vw', width:'calc(100% - 2vw)', fontSize:'0.85rem' }}>Beneficio actualizado. Redirigiendo...</div>
						)}
						<div className='button-container'>
							<button type="submit" disabled={submitting} style={{ opacity: submitting ? 0.6 : 1, cursor: submitting ? 'not-allowed' : 'pointer' }}>
								{submitting ? 'Actualizando...' : 'Actualizar Beneficio'}
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
