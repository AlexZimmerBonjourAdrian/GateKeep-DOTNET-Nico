# Checklist Configuración GateKeep PWA - zimmzimmgames.com

## 🔧 Pre-Deployment

### 1. Dominio y DNS
- [ ] Dominio `zimmzimmgames.com` registrado y activo
- [ ] DNS apunta a la IP pública del servidor
- [ ] A records configurados:
  - [ ] `@` → IP Pública
  - [ ] `www` → IP Pública
  - [ ] `api` → IP Pública (opcional)
- [ ] CAA records configurados para Let's Encrypt
- [ ] Propagación DNS verificada (`dig zimmzimmgames.com A`)

### 2. Infraestructura de Servidor
- [ ] Servidor Linux (Ubuntu/Amazon Linux) listo
- [ ] Docker instalado y corriendo
- [ ] Docker Compose instalado
- [ ] Git instalado
- [ ] Puertos abiertos:
  - [ ] 80 (HTTP)
  - [ ] 443 (HTTPS)
  - [ ] 5011 (Backend API - opcional, solo si es público)

### 3. Certificados SSL
- [ ] Certbot instalado: `sudo apt install certbot python3-certbot-nginx`
- [ ] Certificado generado: `certbot certonly --standalone -d zimmzimmgames.com`
- [ ] Ruta del certificado: `/etc/letsencrypt/live/zimmzimmgames.com/fullchain.pem`
- [ ] Ruta de clave: `/etc/letsencrypt/live/zimmzimmgames.com/privkey.pem`
- [ ] Permisos correctos: `sudo chmod 755 /etc/letsencrypt/live/zimmzimmgames.com/`

---

## 📦 Código y Configuración

### 4. Backend (.NET)
- [ ] `Program.cs` actualizado con:
  - [ ] CORS Origins: `https://zimmzimmgames.com`, `https://www.zimmzimmgames.com`
  - [ ] JWT Token Validation habilitado
  - [ ] Logging configurado (Serilog)
- [ ] `appsettings.Production.json` configurado:
  - [ ] `ConnectionString` → PostgreSQL en servidor
  - [ ] `JwtSettings` → Keys y expiration
  - [ ] `Redis` → Conexión a Redis
- [ ] `Dockerfile` correctamente construido
- [ ] Variables de entorno en `docker-compose.prod.yml`:
  ```
  DATABASE_URL=postgresql://user:pass@postgres:5432/gatekeep
  REDIS_URL=redis://redis:6379
  CORS_ORIGINS=https://zimmzimmgames.com,https://www.zimmzimmgames.com
  ```

### 5. Frontend (Next.js)
- [ ] `next.config.js` configurado:
  - [ ] Headers para Service Worker
  - [ ] Cache-Control para PWA manifest
- [ ] `.env.production` configurado:
  - [ ] `NEXT_PUBLIC_API_URL=https://api.zimmzimmgames.com`
  - [ ] `NODE_ENV=production`
- [ ] `manifest.json` en `public/`:
  - [ ] `name` y `short_name` configurados
  - [ ] `start_url: "/"`
  - [ ] `icons` con rutas correctas
  - [ ] `theme_color` y `background_color`
- [ ] `service-worker.js` en `public/`:
  - [ ] Cache strategies configuradas
  - [ ] Push notifications habilitadas (opcional)
- [ ] `layout.js` incluye:
  - [ ] `SyncProvider` como wrapper
  - [ ] Registración de Service Worker
  - [ ] Metadata de PWA

### 6. Nginx Configuration
- [ ] `nginx.conf` configurado con:
  - [ ] Rutas SSL correctas: `/etc/letsencrypt/live/zimmzimmgames.com/`
  - [ ] SSL protocols: `TLSv1.2 TLSv1.3`
  - [ ] HSTS header con `preload`
  - [ ] OCSP stapling habilitado
  - [ ] Redirección HTTP → HTTPS
- [ ] Headers de seguridad:
  - [ ] `X-Content-Type-Options: nosniff`
  - [ ] `X-Frame-Options: SAMEORIGIN`
  - [ ] `X-XSS-Protection: 1; mode=block`
  - [ ] `Strict-Transport-Security` con preload

---

## 🐳 Docker Compose

### 7. docker-compose.prod.yml
- [ ] Servicios definidos:
  - [ ] PostgreSQL con volumen persistente
  - [ ] Redis con volumen persistente
  - [ ] MongoDB (si se usa)
  - [ ] Backend API (.NET)
  - [ ] Frontend (Next.js)
  - [ ] Nginx (reverse proxy)
- [ ] Networks configuradas: `gatekeep-network`
- [ ] Volúmenes persistentes:
  - [ ] `postgres_data`
  - [ ] `redis_data`
  - [ ] Certificados SSL montados en Nginx
- [ ] Healthchecks configurados
- [ ] Restart policies: `unless-stopped`

---

## 🔐 Seguridad

### 8. SSL/HTTPS
- [ ] Certificado válido para `zimmzimmgames.com`
- [ ] Redirección HTTP → HTTPS funciona
- [ ] HTTPS Score A+ en SSL Labs: https://www.ssllabs.com/
- [ ] HSTS preload header incluido
- [ ] Renovación automática de certificatos habilitada:
  ```bash
  sudo systemctl enable certbot.timer
  sudo systemctl start certbot.timer
  ```

### 9. CORS
- [ ] Origins permitidos solo para los dominios necesarios
- [ ] Métodos: `GET, POST, PUT, DELETE, PATCH, OPTIONS`
- [ ] Headers: `Authorization, Content-Type, Accept`
- [ ] Credentials: `true` (si se usa autenticación por cookies)
- [ ] Exposed Headers para paginación

### 10. Authentication
- [ ] JWT token generation en backend
- [ ] Token almacenado secamente en frontend (localStorage o sessionStorage)
- [ ] Token refresh logic implementado
- [ ] HTTPS-only cookies (si aplica)

### 11. Base de Datos
- [ ] PostgreSQL con contraseña fuerte
- [ ] Backups automáticos configurados
- [ ] Replicación (si es alta disponibilidad)
- [ ] Encriptación en tránsito (SSL)
- [ ] Conexión desde frontend rechazada (solo backend)

---

## 📊 Monitoreo

### 12. Logs
- [ ] Serilog configurado en backend
- [ ] Logs enviados a CloudWatch (si usas AWS)
- [ ] Nginx logs configurados
- [ ] Frontend error tracking (Sentry, LogRocket, etc.)

### 13. Alertas
- [ ] Alertas de certificado expirando (20 días antes)
- [ ] Alertas de alta CPU/Memoria
- [ ] Alertas de errores en API
- [ ] Alertas de downtime del servicio

### 14. Performance
- [ ] CDN configurado (CloudFlare, AWS CloudFront)
- [ ] Compresión GZIP habilitada
- [ ] Minificación de assets (Next.js build)
- [ ] Cache headers optimizados
- [ ] API response times < 200ms
- [ ] Frontend load time < 3s (First Contentful Paint)

---

## 🚀 Deployment

### 15. Proceso de Deploy
- [ ] Código commiteado y pusheado a main/main branch
- [ ] CI/CD pipeline configurado (GitHub Actions, GitLab CI)
- [ ] Build de Docker images automatizado
- [ ] Deploy automático a producción
- [ ] Rollback mechanism en caso de error
- [ ] Health checks pasan después del deploy

### 16. Datos de Prueba
- [ ] Base de datos inicializada con datos de prueba
- [ ] Usuarios de prueba creados
- [ ] Roles y permisos configurados
- [ ] Datos de ejemplo en MongoDB (auditoría)

---

## ✅ Testing

### 17. Tests Automatizados
- [ ] Unit tests passing
- [ ] Integration tests passing
- [ ] E2E tests con Cypress passing
- [ ] Load testing (k6 o JMeter)
- [ ] Security testing (OWASP ZAP)

### 18. Manual Testing
- [ ] Navegación de app completa funciona
- [ ] Login/Logout funciona
- [ ] CRUD operations en todas las entidades
- [ ] Sincronización offline funciona
- [ ] PWA installable en navegadores
- [ ] Service Worker registrado
- [ ] Funciona offline (modo avión)
- [ ] Funciona en múltiples dispositivos

---

## 🌐 Post-Deployment

### 19. Configuración de Dominio
- [ ] SSL certificate en HSTS preload list (opcional pero recomendado)
- [ ] SPF/DKIM configurado para emails (si aplica)
- [ ] Subdomains redirigiendo correctamente
- [ ] www → versión sin www (o viceversa)

### 20. Monitoreo Inicial
- [ ] Error logs revisados (primeras 24 horas)
- [ ] Performance monitoreado
- [ ] User feedback recolectado
- [ ] Bugs identificados reporteados

---

## 📋 Información de Contacto

**Dominio:** zimmzimmgames.com  
**Email SSL:** tu-email@example.com  
**Certificado:** Let's Encrypt  
**Renovación:** Automática (cada 90 días)

---

## 🔗 Links Útiles

- **SSL Labs Test:** https://www.ssllabs.com/ssltest/analyze.html?d=zimmzimmgames.com
- **DNS Checker:** https://www.whatsmydns.net/
- **Certbot Docs:** https://certbot.eff.org/
- **OWASP Security:** https://owasp.org/
- **PWA Checklist:** https://web.dev/pwa-checklist/

---

**Última actualización:** 18 de Noviembre 2025  
**Versión:** 1.0
