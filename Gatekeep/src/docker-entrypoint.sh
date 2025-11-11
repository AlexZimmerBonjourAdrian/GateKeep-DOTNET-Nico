#!/bin/sh
set -e

echo "Waiting for PostgreSQL to be ready..."

# Extraer host y puerto de las variables de entorno
DB_HOST="${DATABASE__HOST:-db}"
DB_PORT="${DATABASE__PORT:-5432}"

# Esperar hasta que PostgreSQL esté disponible
until nc -z "$DB_HOST" "$DB_PORT"; do
  echo "PostgreSQL is unavailable - sleeping"
  sleep 2
done

echo "PostgreSQL is up - starting application"

# Ejecutar el comando original
exec "$@"

