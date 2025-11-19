/**
 * Script para copiar sql-wasm.wasm a public/ después de instalar dependencias
 * Ejecutado automáticamente por npm postinstall
 */

import { copyFileSync, existsSync, mkdirSync } from 'fs';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const projectRoot = join(__dirname, '..');

const sourcePath = join(projectRoot, 'node_modules', 'sql.js', 'dist', 'sql-wasm.wasm');
const destDir = join(projectRoot, 'public');
const destPath = join(destDir, 'sql-wasm.wasm');

try {
  // Verificar que el archivo fuente existe
  if (!existsSync(sourcePath)) {
    console.warn('⚠️  sql-wasm.wasm no encontrado en node_modules/sql.js/dist/');
    console.warn('   Esto es normal si sql.js aún no está instalado.');
    process.exit(0);
  }

  // Crear directorio public si no existe
  if (!existsSync(destDir)) {
    mkdirSync(destDir, { recursive: true });
  }

  // Copiar archivo
  copyFileSync(sourcePath, destPath);
  console.log('✅ sql-wasm.wasm copiado a public/');
} catch (error) {
  console.error('❌ Error copiando sql-wasm.wasm:', error.message);
  // No fallar el postinstall si hay error
  process.exit(0);
}

