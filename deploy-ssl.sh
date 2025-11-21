#!/bin/bash

# Deploy Script para GateKeep PWA con SSL
# Dominio: zimmzimmgames.com
# Ejecutar con: bash deploy-ssl.sh

set -e

echo "================================"
echo "Deploy GateKeep PWA con SSL"
echo "Dominio: zimmzimmgames.com"
echo "================================"

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Variables
DOMAIN="zimmzimmgames.com"
EMAIL="tu-email@example.com"  # ← CAMBIAR A TU EMAIL
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DOCKER_COMPOSE_FILE="$PROJECT_DIR/docker-compose.prod.yml"

# Funciones
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

# 1. Verificar requisitos previos
log_info "Verificando requisitos previos..."

if ! command -v docker &> /dev/null; then
    log_error "Docker no está instalado"
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    log_error "Docker Compose no está instalado"
    exit 1
fi

if ! command -v certbot &> /dev/null; then
    log_warning "Certbot no está instalado. Instalando..."
    sudo apt update
    sudo apt install -y certbot python3-certbot-nginx
fi

log_info "Requisitos previos verificados ✓"

# 2. Verificar que el dominio esté apuntando correctamente
log_info "Verificando DNS..."

DNS_IP=$(dig +short $DOMAIN A | tail -n1)
CURRENT_IP=$(curl -s https://checkip.amazonaws.com)

if [ -z "$DNS_IP" ]; then
    log_error "No se puede resolver el DNS para $DOMAIN"
    log_info "Asegurate de que el dominio esté correctamente configurado en tu proveedor DNS"
    exit 1
fi

log_info "DNS resuelve a: $DNS_IP"
log_info "IP actual del servidor: $CURRENT_IP"

if [ "$DNS_IP" != "$CURRENT_IP" ]; then
    log_warning "El DNS no está apuntando a este servidor"
    log_warning "DNS IP: $DNS_IP"
    log_warning "Servidor IP: $CURRENT_IP"
    read -p "¿Continuar de todas formas? (s/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Ss]$ ]]; then
        exit 1
    fi
fi

# 3. Generar certificado SSL
log_info "Generando certificado SSL con Let's Encrypt..."

sudo certbot certonly --standalone \
  -d $DOMAIN \
  -d www.$DOMAIN \
  -d api.$DOMAIN \
  --agree-tos \
  --email $EMAIL \
  --non-interactive \
  --preferred-challenges http || true

if [ ! -f "/etc/letsencrypt/live/$DOMAIN/fullchain.pem" ]; then
    log_error "No se pudo generar el certificado SSL"
    exit 1
fi

log_info "Certificado SSL generado ✓"

# 4. Configurar permisos para Docker
log_info "Configurando permisos para Docker..."

sudo chmod 755 /etc/letsencrypt/
sudo chmod 755 /etc/letsencrypt/live/
sudo chmod 755 /etc/letsencrypt/live/$DOMAIN/
sudo chmod 744 /etc/letsencrypt/live/$DOMAIN/privkey.pem

log_info "Permisos configurados ✓"

# 5. Crear directorio certs (si no existe)
mkdir -p "$PROJECT_DIR/certs"

# 6. Copiar certificados (opcional, solo si quieres backup local)
log_info "Haciendo backup de certificados..."

sudo cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem "$PROJECT_DIR/certs/cert.pem"
sudo cp /etc/letsencrypt/live/$DOMAIN/privkey.pem "$PROJECT_DIR/certs/key.pem"
sudo chown $(whoami):$(whoami) "$PROJECT_DIR/certs/"*

log_info "Backup completado ✓"

# 7. Detener contenedores antiguos
log_info "Deteniendo contenedores antiguos..."

cd "$PROJECT_DIR"
docker-compose -f $DOCKER_COMPOSE_FILE down || true

log_info "Contenedores detenidos ✓"

# 8. Iniciar nuevos contenedores
log_info "Iniciando nuevos contenedores..."

docker-compose -f $DOCKER_COMPOSE_FILE up -d

log_info "Contenedores iniciados ✓"

# 9. Esperar a que los servicios estén listos
log_info "Esperando a que los servicios estén listos..."

sleep 10

# 10. Verificar SSL
log_info "Verificando configuración SSL..."

if curl -s -I https://$DOMAIN | grep -q "200\|301\|302"; then
    log_info "SSL funcionando correctamente ✓"
else
    log_error "SSL no está funcionando correctamente"
    docker-compose -f $DOCKER_COMPOSE_FILE logs nginx
    exit 1
fi

# 11. Configurar renovación automática
log_info "Configurando renovación automática de certificados..."

sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer

log_info "Renovación automática configurada ✓"

# 12. Verificar servicios
log_info "Verificando servicios..."

echo ""
echo "=== Estado de Servicios ==="

echo -n "HTTP (80): "
if curl -s -I http://$DOMAIN -L | grep -q "200\|301\|302"; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${RED}FAIL${NC}"
fi

echo -n "HTTPS (443): "
if curl -s -I https://$DOMAIN | grep -q "200\|301\|302"; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${RED}FAIL${NC}"
fi

echo -n "API Backend: "
if curl -s http://localhost:5011/health &>/dev/null; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${RED}FAIL${NC}"
fi

echo -n "Frontend: "
if curl -s http://localhost:3000 &>/dev/null; then
    echo -e "${GREEN}OK${NC}"
else
    echo -e "${RED}FAIL${NC}"
fi

echo ""
echo "=== Información SSL ==="
echo "Dominio: $DOMAIN"
echo "Certificado: /etc/letsencrypt/live/$DOMAIN/fullchain.pem"
echo "Clave privada: /etc/letsencrypt/live/$DOMAIN/privkey.pem"
echo "Próxima renovación: $(sudo certbot certificates | grep "Expiry Date")"

echo ""
echo -e "${GREEN}================================${NC}"
echo -e "${GREEN}Deploy completado exitosamente!${NC}"
echo -e "${GREEN}================================${NC}"

echo ""
echo "URLs disponibles:"
echo "  - https://$DOMAIN"
echo "  - https://www.$DOMAIN"
echo "  - https://api.$DOMAIN"

echo ""
echo "Próximos pasos:"
echo "1. Verifica que todo esté funcionando correctamente"
echo "2. Configura variables de entorno en tu backend/frontend"
echo "3. Monitorea los logs: docker-compose logs -f"
echo ""
