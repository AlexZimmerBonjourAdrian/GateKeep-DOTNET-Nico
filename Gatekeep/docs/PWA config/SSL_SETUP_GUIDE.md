# Guía de Configuración SSL/HTTPS para zimmzimmgames.com

## 📋 Resumen

Este documento proporciona instrucciones completas para configurar SSL/HTTPS usando **Let's Encrypt** con **Certbot** en tu dominio `zimmzimmgames.com`.

---

## 1. Requisitos Previos

### DNS
- Dominio: `zimmzimmgames.com`
- Debe estar apuntando a tu servidor (IP del ALB o instancia EC2)
- Records DNS necesarios:
  ```
  A record: zimmzimmgames.com -> Tu IP Pública
  A record: www.zimmzimmgames.com -> Tu IP Pública
  A record: api.zimmzimmgames.com -> Tu IP Pública (opcional, si usas subdominio)
  ```

### Servidor
- Puerto 80 (HTTP) abierto temporalmente para validación
- Puerto 443 (HTTPS) abierto para tráfico SSL
- Docker instalado
- Nginx corriendo en el contenedor

---

## 2. Opción A: Let's Encrypt + Certbot (Recomendado)

### Paso 1: Instalar Certbot

```bash
# En tu servidor (fuera de Docker)
sudo apt update
sudo apt install certbot python3-certbot-nginx -y

# O si usas Amazon Linux 2
sudo yum install certbot python3-certbot-nginx -y
```

### Paso 2: Detener temporalmente Nginx

```bash
# Si está corriendo en Docker
docker-compose down

# O si está en el sistema
sudo systemctl stop nginx
```

### Paso 3: Ejecutar Certbot

```bash
# Crear certificados para todos los dominios
sudo certbot certonly --standalone \
  -d zimmzimmgames.com \
  -d www.zimmzimmgames.com \
  -d api.zimmzimmgames.com \
  --agree-tos \
  --email tu-email@example.com \
  --non-interactive
```

### Paso 4: Verificar Certificados

```bash
# Los certificados estarán en:
sudo ls -la /etc/letsencrypt/live/zimmzimmgames.com/

# Deberías ver:
# - fullchain.pem (certificado)
# - privkey.pem (clave privada)
# - chain.pem
# - cert.pem
```

### Paso 5: Configurar Renovación Automática

```bash
# Let's Encrypt expira cada 90 días
# Configurar renovación automática
sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer

# Verificar que esté activo
sudo systemctl status certbot.timer
```

### Paso 6: Permisos para Docker

```bash
# Docker necesita acceso a los certificados
sudo chmod 755 /etc/letsencrypt/
sudo chmod 755 /etc/letsencrypt/live/
sudo chmod 755 /etc/letsencrypt/live/zimmzimmgames.com/
sudo chmod 744 /etc/letsencrypt/live/zimmzimmgames.com/privkey.pem
```

### Paso 7: Configurar docker-compose.yml

```yaml
version: '3.8'

services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl-conf.d:/etc/nginx/ssl-conf.d:ro
      - /etc/letsencrypt:/etc/letsencrypt:ro  # ← Montar certificados
    depends_on:
      - gatekeep-backend
      - gatekeep-frontend
    networks:
      - gatekeep-network
    restart: unless-stopped
```

### Paso 8: Actualizar nginx.conf

El archivo ya está configurado con:

```nginx
# Rutas de certificados
ssl_certificate /etc/letsencrypt/live/zimmzimmgames.com/fullchain.pem;
ssl_certificate_key /etc/letsencrypt/live/zimmzimmgames.com/privkey.pem;

# Renovación automática OCSP stapling
ssl_stapling on;
ssl_stapling_verify on;
resolver 8.8.8.8 8.8.4.4 valid=300s;
```

### Paso 9: Iniciar Contenedores

```bash
# Navega al directorio del proyecto
cd Gatekeep

# Inicia los contenedores
docker-compose -f docker-compose.prod.yml up -d

# Verifica que esté corriendo
docker-compose -f docker-compose.prod.yml logs nginx
```

### Paso 10: Verificar SSL

```bash
# Test la conexión HTTPS
curl -I https://zimmzimmgames.com

# Deberías ver:
# HTTP/2 200
# Conexión segura

# También puedes usar:
openssl s_client -connect zimmzimmgames.com:443 -servername zimmzimmgames.com
```

---

## 3. Opción B: AWS Certificate Manager (Si usas AWS)

Si tu ALB está en AWS, puedes usar AWS Certificate Manager (ACM):

### Paso 1: Solicitar Certificado en ACM

```bash
# En AWS Console > Certificate Manager
# Click "Request a certificate"
# Domain name: zimmzimmgames.com
# Add another name to this certificate: *.zimmzimmgames.com

# Validar por email o DNS (recomendado DNS)
```

### Paso 2: Importar Certificado en ALB

```bash
# En AWS Console > EC2 > Load Balancers
# Select tu ALB
# Add listener on 443
# Protocol: HTTPS
# Certificate: Seleccionar el certificado de ACM
# Forward to: Target group de tu backend
```

### Paso 3: Redirigir HTTP a HTTPS

```bash
# En el listener de HTTP (puerto 80)
# Action: Redirect
# Protocol: HTTPS
# Port: 443
# Status code: 301 (Permanent)
```

---

## 4. Renovación Automática de Certificados

### Let's Encrypt (Opción A)

```bash
# Los certificados se renuevan automáticamente
# Verificar estado:
sudo certbot renew --dry-run

# Ver próxima renovación:
sudo certbot certificates

# Hacer renovación manual:
sudo certbot renew
```

### AWS ACM (Opción B)

```bash
# AWS renueva automáticamente
# No se requiere acción manual
# Los certificados se renuevan 30 días antes de expirar
```

---

## 5. Monitoreo y Mantenimiento

### Verificar Certificado Activo

```bash
# Fecha de expiración
openssl x509 -in /etc/letsencrypt/live/zimmzimmgames.com/fullchain.pem -text -noout | grep -A 2 "Validity"

# O:
echo | openssl s_client -servername zimmzimmgames.com -connect zimmzimmgames.com:443 2>/dev/null | openssl x509 -noout -dates
```

### Alertas de Expiración

```bash
# Let's Encrypt envía emails 20 días antes de expirar
# Asegurate de que el email en certbot esté correcto

# Verificar:
sudo cat /etc/letsencrypt/renewal/zimmzimmgames.com.conf
```

### Test SSL Rating

Usa SSL Labs para verificar la configuración:
```
https://www.ssllabs.com/ssltest/analyze.html?d=zimmzimmgames.com
```

---

## 6. Configuración de Seguridad Avanzada

### HSTS (HTTP Strict Transport Security)

Ya está en nginx.conf:

```nginx
add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;
```

Esto fuerza HTTPS en todos los navegadores.

### CAA Records (Certificado Authority Authorization)

En tu DNS, agrega:

```
CAA 0 issue "letsencrypt.org"
CAA 0 issuewild "letsencrypt.org"
CAA 0 iodef "mailto:tu-email@example.com"
```

### OCSP Stapling

Ya está configurado en nginx.conf:

```nginx
ssl_stapling on;
ssl_stapling_verify on;
resolver 8.8.8.8 8.8.4.4 valid=300s;
```

---

## 7. Troubleshooting

### Error: "Could not bind to port 80"

```bash
# Si el puerto 80 está en uso:
sudo lsof -i :80
sudo kill -9 <PID>

# O:
sudo systemctl stop nginx
```

### Error: "Validation timed out"

```bash
# Asegurate de que el DNS está correctamente configurado
dig zimmzimmgames.com

# Espera a que se propague (puede tardar 15-30 minutos)
# Y luego reintenta

sudo certbot certonly --standalone -d zimmzimmgames.com
```

### Error: "Certificate verify failed"

```bash
# Verifica que los permisos sean correctos:
sudo chmod 755 /etc/letsencrypt/
sudo chmod 755 /etc/letsencrypt/live/zimmzimmgames.com/

# Y que Docker pueda acceder:
docker exec gatekeep-nginx ls -la /etc/letsencrypt/live/zimmzimmgames.com/
```

### Renovación Manual

```bash
# Si necesitas renovar manualmente:
sudo certbot renew --force-renewal -d zimmzimmgames.com

# O regenerar todo:
sudo certbot delete --cert-name zimmzimmgames.com
sudo certbot certonly --standalone -d zimmzimmgames.com
```

---

## 8. Checklist de Configuración

- [ ] DNS apuntando correctamente a tu servidor
- [ ] Puertos 80 y 443 abiertos en Security Groups
- [ ] Certbot instalado en el servidor
- [ ] Certificados creados con `certbot certonly`
- [ ] Permisos configurados para Docker
- [ ] docker-compose.yml montando `/etc/letsencrypt`
- [ ] nginx.conf con rutas correctas de certificados
- [ ] Contenedores iniciados: `docker-compose up -d`
- [ ] HTTPS funcionando: `curl -I https://zimmzimmgames.com`
- [ ] Certificado en whitelist de navegadores
- [ ] Renovación automática habilitada: `certbot.timer`

---

## 9. Variables de Entorno para Backend

Asegurate de que tu backend tenga:

```bash
# En .env o docker-compose.prod.yml
CORS_ORIGINS=https://zimmzimmgames.com,https://www.zimmzimmgames.com,https://api.zimmzimmgames.com
NEXT_PUBLIC_API_URL=https://api.zimmzimmgames.com
NODE_ENV=production
DOTNET_ENVIRONMENT=Production
```

---

## 10. Referencias

- [Let's Encrypt Official](https://letsencrypt.org/)
- [Certbot Documentation](https://certbot.eff.org/)
- [Nginx SSL Configuration](https://nginx.org/en/docs/http/ngx_http_ssl_module.html)
- [OWASP TLS Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Transport_Layer_Protection_Cheat_Sheet.html)
- [Mozilla SSL Configuration Generator](https://ssl-config.mozilla.org/)

---

**Última actualización:** 18 de Noviembre 2025  
**Dominio:** zimmzimmgames.com  
**SSL Provider:** Let's Encrypt + Certbot
