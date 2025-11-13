#!/bin/bash

# Script para desplegar GateKeep en EC2
echo "🚀 Iniciando despliegue de GateKeep en EC2..."

# Variables
EC2_IP="18.231.88.101"
EC2_USER="ec2-user"  # o ubuntu según la AMI
KEY_PATH="~/.ssh/your-key.pem"

# 1. Actualizar sistema
echo "📦 Actualizando sistema..."
sudo yum update -y

# 2. Instalar Docker
echo "🐳 Instalando Docker..."
sudo yum install -y docker
sudo systemctl start docker
sudo systemctl enable docker
sudo usermod -a -G docker $USER

# 3. Instalar Docker Compose
echo "🔧 Instalando Docker Compose..."
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# 4. Crear directorio de aplicación
echo "📁 Creando directorio de aplicación..."
mkdir -p ~/gatekeep
cd ~/gatekeep

# 5. Clonar repositorio (o copiar archivos)
echo "📥 Obteniendo código fuente..."
# git clone https://github.com/tu-usuario/gatekeep.git .
# O copiar archivos manualmente

# 6. Configurar variables de entorno
echo "⚙️ Configurando variables de entorno..."
cat > .env << EOF
DB_PASSWORD=GateKeep2024SecurePassword!
JWT_KEY=tu-clave-jwt-super-secreta-de-256-bits-minimo-para-produccion
ASPNETCORE_ENVIRONMENT=Production
EOF

# 7. Construir y ejecutar contenedores
echo "🏗️ Construyendo y ejecutando aplicación..."
docker-compose -f docker-compose.prod.yml up -d --build

# 8. Verificar estado
echo "✅ Verificando estado de servicios..."
docker-compose -f docker-compose.prod.yml ps

echo "🎉 Despliegue completado!"
echo "🌐 Aplicación disponible en: http://$EC2_IP"
echo "📊 API Swagger: http://$EC2_IP/swagger"
echo "🔍 Health Check: http://$EC2_IP/health"