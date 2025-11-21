"use client";

import React, { useEffect, useState, useMemo, useRef } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import Header from '../../../components/Header';
import { SecurityService } from '../../../services/securityService';
import { SalonService } from '../../../services/SalonService';
import { EdificioService } from '../../../services/EdificioService';

export default function ListadoSalones() {
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

	const [salones, setSalones] = useState([]);
	const [edificios, setEdificios] = useState([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState(null);

	useEffect(() => {
		const fetchData = async (retries = 2) => {
			try {
				const [salonesRes, edificiosRes] = await Promise.all([
					SalonService.getSalones(),
					EdificioService.getEdificios()
				]);
				setSalones(Array.isArray(salonesRes.data) ? salonesRes.data : []);
				setEdificios(Array.isArray(edificiosRes.data) ? edificiosRes.data : []);
			} catch (e) {
				console.error('Error al cargar datos:', e);
				if (retries > 0) {
					console.log(`Reintentando... (${retries} intentos restantes)`);
					await new Promise(resolve => setTimeout(resolve, 1000));
					return fetchData(retries - 1);
				}
				setError('No se pudieron cargar los salones');
				setSalones([]);
			} finally {
				setLoading(false);
			}
		};
		fetchData();
	}, []);

	// Filters
	const [search, setSearch] = useState('');
	const [edificioFilter, setEdificioFilter] = useState('');
	const inputRef = useRef(null);

	const getEdificioNombre = (edificioId) => {
		const edificio = edificios.find(e => (e.Id || e.id) === edificioId);
		return edificio ? (edificio.Nombre || edificio.nombre) : 'N/A';
	};

	const filteredSalones = useMemo(() => {
		const q = search.trim().toLowerCase();
		return salones.filter(s => {
			if (edificioFilter && (s.EdificioId || s.edificioId) != edificioFilter) return false;
			if (q) {
				const nombre = (s.Nombre || s.nombre || '').toLowerCase();
				const numero = (s.NumeroSalon || s.numeroSalon || '').toLowerCase();
				if (!nombre.includes(q) && !numero.includes(q)) return false;
			}
			return true;
		});
	}, [salones, search, edificioFilter]);

	const grouped = [];
	for (let i = 0; i < filteredSalones.length; i += 4) {
		grouped.push(filteredSalones.slice(i, i + 4));
	}

	const handleSearchSubmit = (e) => {
		e.preventDefault();
		setSearch(s => s.trim());
		if (inputRef.current) inputRef.current.blur();
	};

	const handleDelete = async (id) => {
		if (!confirm('¿Estás seguro de eliminar este salón?')) return;
		try {
			await SalonService.deleteSalon(id);
			setSalones(salones.filter(s => (s.Id || s.id) !== id));
		} catch (e) {
			console.error('Error al eliminar salón:', e);
			alert('Error al eliminar el salón');
		}
	};

	if (!isAdmin) return null;

	return (
		<div className="container-nothing">
			<Header />
			<div className="container">
				<div className="container-header">
					<h2>Salones</h2>
					<div className="filtros-container">
						<div className="field">
							<label className="field-label" htmlFor="search-input">Buscar</label>
							<form className="search-bar" onSubmit={handleSearchSubmit}>
								<input
									id="search-input"
									ref={inputRef}
									className="search-input"
									type="text"
									placeholder="Nombre o número..."
									aria-label="Buscar salones"
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

						<div className="field" style={{ minWidth: '180px' }}>
							<label className="field-label" htmlFor="edificio-filter">Edificio</label>
							<div className="search-bar">
								<select
									id="edificio-filter"
									className="select-input"
									value={edificioFilter}
									onChange={(e) => setEdificioFilter(e.target.value)}
									aria-label="Filtrar por edificio"
								>
									<option value="">Todos</option>
									{edificios.map(e => (
										<option key={e.Id || e.id} value={e.Id || e.id}>
											{e.Nombre || e.nombre}
										</option>
									))}
								</select>
							</div>
						</div>

						<div className="actions-inline">
							<button
								type="button"
								className="create-button"
								onClick={() => router.push('/salones/crearSalon')}
								aria-label="Crear nuevo salón"
							>
								Crear Salón
							</button>
						</div>
					</div>
				</div>

				<div className="events-grid">
					{loading ? (
						<div className="event-card" style={{ background: '#fff6ee' }}>
							<h3>Cargando salones...</h3>
						</div>
					) : error ? (
						<div className="event-card" style={{ background: '#fff6ee' }}>
							<h3>Error</h3>
							<p>{error}</p>
						</div>
					) : filteredSalones.length === 0 ? (
						<div className="event-card" style={{ background: '#fff6ee' }}>
							<h3>Sin resultados</h3>
							<p>No se encontraron salones.</p>
						</div>
					) : (
						grouped.map((group, gi) => (
							<div className="event-group" key={`group-${gi}`}>
								{group.map((salon) => {
									const id = salon.Id || salon.id;
									const nombre = salon.Nombre || salon.nombre || 'Sin nombre';
									const numero = salon.NumeroSalon || salon.numeroSalon || 'N/A';
									const capacidad = salon.Capacidad ?? salon.capacidad ?? 'N/A';
									const edificioId = salon.EdificioId || salon.edificioId;
									
									return (
										<div 
											key={id} 
											className="event-card" 
											tabIndex={0}
											onClick={() => router.push(`/salones/${id}`)}
											style={{ cursor: 'pointer' }}
										>
											<h3>{nombre}</h3>
											<p><strong>Número:</strong> {numero}</p>
											<p><strong>Capacidad:</strong> {capacidad}</p>
											<p><strong>Edificio:</strong> {getEdificioNombre(edificioId)}</p>
											<div className="card-actions" onClick={(e) => e.stopPropagation()}>
												<button
													className="card-action-btn edit-btn"
													onClick={() => router.push(`/salones/editarSalon/${id}`)}
													aria-label={`Editar ${nombre}`}
												>
													✏️ Editar
												</button>
												<button
													className="card-action-btn delete-btn"
													onClick={() => handleDelete(id)}
													aria-label={`Eliminar ${nombre}`}
												>
													🗑️ Eliminar
												</button>
											</div>
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
				.select-input{ border:none; outline:none; background:transparent; font-size:0.95rem; color:#111827; padding:6px 8px; border-radius:20px; width:100%; cursor:pointer; }
				.search-button{ display:inline-flex; align-items:center; justify-content:center; background:#f37426; color:white; border:none; height:28px; width:36px; padding:0; border-radius:14px; cursor:pointer; }
				.search-button:active{ transform:scale(0.98); }
				.search-icon{ display:block; color:white; }
				.events-grid{ display:flex; flex-direction:column; gap:18px; padding:16px; box-sizing:border-box; }
				.event-group{ display:grid; grid-template-columns: repeat(1, 1fr); gap:12px; }
				@media (min-width: 426px) and (max-width: 768px) { .event-group{ grid-template-columns: repeat(2, 1fr); gap:12px; } }
				@media (min-width: 769px) { .event-group{ grid-template-columns: repeat(4, 1fr); gap:16px; } }
				.event-card{ width:100%; aspect-ratio: 4 / 3; padding:12px; background:#f37426; border-radius:20px; box-shadow:0 2px 6px rgba(0,0,0,0.12); transition:transform 0.18s ease, box-shadow 0.18s ease; color:#231F20; box-sizing:border-box; display:flex; flex-direction:column; justify-content:space-between; }
				.event-card:hover{ transform: translateY(-4px) scale(1.01); box-shadow:0 8px 18px rgba(0,0,0,0.18); z-index:1; }
				.event-card:focus, .event-card:focus-visible{ transform: translateY(-4px) scale(1.01); box-shadow:0 10px 20px rgba(0,0,0,0.22); border:2px solid rgba(37,99,235,0.12); z-index:2; }
				.event-card h3{ font-size: clamp(1rem, 1.6vw, 1.2rem); margin:0 0 6px 0; }
				.event-card p{ font-size: clamp(0.75rem, 1.05vw, 0.95rem); margin:0 0 4px 0; }
				.card-actions{ display:flex; gap:6px; margin-top:8px; }
				.card-action-btn{ padding:6px 10px; border:none; border-radius:12px; cursor:pointer; font-size:0.75rem; font-weight:600; transition:all 0.15s ease; }
				.edit-btn{ background:#231F20; color:#fff; }
				.edit-btn:hover{ background:#3d3739; transform:scale(1.05); }
				.delete-btn{ background:#231F20; color:#fff; }
				.delete-btn:hover{ background:#7e1e1e; transform:scale(1.05); }
			`}</style>
		</div>
	);
}
