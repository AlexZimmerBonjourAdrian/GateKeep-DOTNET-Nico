"use client";

import React, { useEffect, useState, useMemo, useRef } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import Header from '../../../components/Header';
import { SecurityService } from '../../../services/securityService';
import { EdificioService } from '../../../services/EdificioService';

export default function ListadoEdificios() {
	const pathname = usePathname();
	const router = useRouter();
	useEffect(() => {
		SecurityService.checkAuthAndRedirect(pathname);
	}, [pathname]);

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
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState(null);

	useEffect(() => {
		const fetchEdificios = async () => {
			try {
				const response = await EdificioService.getEdificios();
				setEdificios(Array.isArray(response.data) ? response.data : []);
			} catch (e) {
				console.error('Error al cargar edificios:', e);
				setError('No se pudieron cargar los edificios');
				setEdificios([]);
			} finally {
				setLoading(false);
			}
		};
		fetchEdificios();
	}, []);

	// Filters
	const [search, setSearch] = useState('');
	const [soloActivos, setSoloActivos] = useState(false);
	const inputRef = useRef(null);

	const filteredEdificios = useMemo(() => {
		const q = search.trim().toLowerCase();
		return edificios.filter(e => {
			const activo = e.Activo ?? e.activo ?? false;
			if (soloActivos && !activo) return false;
			if (q) {
				const nombre = (e.Nombre || e.nombre || '').toLowerCase();
				const codigo = (e.CodigoEdificio || e.codigoEdificio || '').toLowerCase();
				if (!nombre.includes(q) && !codigo.includes(q)) return false;
			}
			return true;
		});
	}, [edificios, search, soloActivos]);

	const grouped = [];
	for (let i = 0; i < filteredEdificios.length; i += 4) {
		grouped.push(filteredEdificios.slice(i, i + 4));
	}

	const handleSearchSubmit = (e) => {
		e.preventDefault();
		setSearch(s => s.trim());
		if (inputRef.current) inputRef.current.blur();
	};

	if (!isAdmin) return null;

	return (
		<div className="container-nothing">
			<Header />
			<div className="container">
				<div className="container-header">
					<h2>Edificios</h2>
					<div className="filtros-container">
						<div className="field">
							<label className="field-label" htmlFor="search-input">Buscar</label>
							<form className="search-bar" onSubmit={handleSearchSubmit}>
								<input
									id="search-input"
									ref={inputRef}
									className="search-input"
									type="text"
									placeholder="Nombre o código..."
									aria-label="Buscar edificios"
									value={search}
									onChange={(e) => setSearch(e.target.value)}
								/>
								<button className="search-button" type="button" aria-label="Buscar" onClick={(e) => e.preventDefault()}>
									<svg className="search-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
										<circle cx="11" cy="11" r="7"></circle>
										<line x1="21" y1="21" x2="16.65" y2="16.65"></line>
									</svg>
								</button>
							</form>
						</div>

						<div className="field" style={{ minWidth: '140px' }}>
							<label className="field-label" htmlFor="solo-activos">Solo activos</label>
							<div style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '6px 10px', background:'#f8fafc', border:'1px solid #e5e7eb', borderRadius: '20px' }}>
								<input
									id="solo-activos"
									type="checkbox"
									checked={soloActivos}
									onChange={(e) => setSoloActivos(e.target.checked)}
									aria-label="Filtrar activos"
								/>
								<span style={{ fontSize: '0.8rem', color:'#111827' }}>Activos</span>
							</div>
						</div>

						<div className="actions-inline">
							<button
								type="button"
								className="create-button"
								onClick={() => router.push('/edificios/crearEdificio')}
								aria-label="Crear nuevo edificio"
							>
								Crear Edificio
							</button>
						</div>
					</div>
				</div>

				<div className="events-grid">
					{loading ? (
						<div className="event-card" style={{ background: '#fff6ee' }}>
							<h3>Cargando edificios...</h3>
						</div>
					) : error ? (
						<div className="event-card" style={{ background: '#fff6ee' }}>
							<h3>Error</h3>
							<p>{error}</p>
						</div>
					) : filteredEdificios.length === 0 ? (
						<div className="event-card" style={{ background: '#fff6ee' }}>
							<h3>Sin resultados</h3>
							<p>No se encontraron edificios.</p>
						</div>
					) : (
						grouped.map((group, gi) => (
							<div className="event-group" key={`group-${gi}`}>
								{group.map((e) => {
									const nombre = e.Nombre || e.nombre || 'Sin nombre';
									const capacidad = e.Capacidad ?? e.capacidad ?? 'N/A';
									const pisos = e.NumeroPisos ?? e.numeroPisos ?? 'N/A';
									const codigo = e.CodigoEdificio || e.codigoEdificio;
									const ubicacion = e.Ubicacion || e.ubicacion || 'N/A';
									const activo = e.Activo ?? e.activo ?? false;
									
									return (
										<div key={e.Id || e.id} className="event-card" tabIndex={0}>
											<h3>{nombre}</h3>
											<p><strong>Capacidad:</strong> {capacidad}</p>
											<p><strong>Pisos:</strong> {pisos}</p>
											{codigo && (<p><strong>Código:</strong> {codigo}</p>)}
											<p><strong>Ubicación:</strong> {ubicacion}</p>
											<p><strong>Estado:</strong> {activo ? 'Activo' : 'Inactivo'}</p>
										</div>
									);
								})}
							</div>
						))
					)}
				</div>
			</div>

			<style jsx>{`
				.container-header{ padding-left: 1.111vw; width: auto; }
				.container-nothing { margin: 0; width: 100%; height: 100%; }
				.actions-inline{ display:flex; align-items:center; gap:12px; margin-left:auto; margin-right: clamp(12px, 1.111vw, 24px); }
				.create-button{ background:#f37426; color:#fff; border:none; padding:8px 16px; border-radius:20px; cursor:pointer; font-size:0.85rem; font-weight:600; letter-spacing:0.3px; box-shadow:0 2px 6px rgba(0,0,0,0.15); transition:background 0.15s ease, transform 0.15s ease; }
				.create-button:hover{ background:#ff8d45; transform:translateY(-2px); }
				.create-button:active{ transform:translateY(0); }
				.create-button:focus-visible{ outline:2px solid rgba(37,99,235,0.4); outline-offset:2px; }
				.filtros-container{ display:flex; gap:12px; align-items:center; flex-wrap:wrap; }
				.field{ display:flex; flex-direction:column; gap:6px; }
				.field-label{ font-size:0.75rem; color:#e5e7ebf6; font-weight:600; letter-spacing:0.2px; margin-bottom:0; }
				.search-bar{ display:flex; align-items:center; gap:8px; background:#f8fafc; border:1px solid #e5e7eb; border-radius:20px; padding:6px 10px; height:40px; box-sizing:border-box; }
				.search-input{ border:none; outline:none; background:transparent; font-size:0.95rem; color:#111827; padding:6px 8px; border-radius:20px; width:clamp(140px,22vw,360px); }
				.search-button{ display:inline-flex; align-items:center; justify-content:center; background:#f37426; color:white; border:none; height:28px; width:36px; padding:0; border-radius:14px; cursor:pointer; }
				.search-button:active{ transform:scale(0.98); }
				.search-icon{ display:block; color:white; }
				.events-grid{ display:flex; flex-direction:column; gap:18px; padding:16px; box-sizing:border-box; }
				.event-group{ display:grid; grid-template-columns: repeat(1, 1fr); gap:12px; }
				@media (min-width: 426px) and (max-width: 768px) { .event-group{ grid-template-columns: repeat(2, 1fr); gap:12px; } }
				@media (min-width: 769px) { .event-group{ grid-template-columns: repeat(4, 1fr); gap:16px; } }
				.event-card{ width:100%; aspect-ratio: 4 / 3; padding:12px; background:#f37426; border-radius:20px; box-shadow:0 2px 6px rgba(0,0,0,0.12); transition:transform 0.18s ease, box-shadow 0.18s ease; color:#231F20; box-sizing:border-box; display:flex; flex-direction:column; justify-content:center; }
				.event-card:hover{ transform: translateY(-4px) scale(1.01); box-shadow:0 8px 18px rgba(0,0,0,0.18); z-index:1; }
				.event-card:focus, .event-card:focus-visible{ transform: translateY(-4px) scale(1.01); box-shadow:0 10px 20px rgba(0,0,0,0.22); border:2px solid rgba(37,99,235,0.12); z-index:2; }
				.event-card h3{ font-size: clamp(1rem, 1.6vw, 1.2rem); margin:0 0 6px 0; }
				.event-card p{ font-size: clamp(0.75rem, 1.05vw, 0.95rem); margin:0; }
			`}</style>
		</div>
	);
}

// Añadir el directive correctamente separado en la primera línea
// Reemplazar comillas sin punto y coma no es necesario pero semicolon opcional