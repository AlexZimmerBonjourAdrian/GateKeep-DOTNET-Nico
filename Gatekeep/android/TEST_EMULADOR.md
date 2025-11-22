# Guía Rápida: Probar APK en Emulador Android

## Requisitos Previos
- Android Studio instalado
- Emulador Android configurado (AVD: `GateKeep_Test`)
- Node.js y npm instalados
- Ngrok instalado

## Pasos Rápidos

### 1. Iniciar Frontend
```powershell
cd Gatekeep\frontend
npm run dev
```
Frontend corriendo en: `http://localhost:3000`

### 2. Iniciar Ngrok (en otra terminal)
```powershell
cd Gatekeep\frontend
ngrok http 3000
```
**IMPORTANTE:** Copia la URL de ngrok (ej: `https://xxxxx.ngrok-free.app`)

### 3. Actualizar URL en build.gradle
Edita `Gatekeep\android\app\build.gradle`:
```gradle
resValue "string", "launchUrl", "https://TU_URL_NGROK.ngrok-free.app/"
resValue "string", "hostName", "TU_URL_NGROK.ngrok-free.app"
```

### 4. Recompilar APK
```powershell
cd Gatekeep\android
$env:JAVA_HOME = "C:\Program Files\Java\jdk-21"
.\gradlew.bat clean assembleRelease
```

### 5. Configurar Variables de Entorno (si es necesario)
```powershell
$env:ANDROID_SDK_ROOT = "$env:LOCALAPPDATA\Android\Sdk"
$env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
```

### 6. Iniciar Emulador
**Opción A: Desde Android Studio**
- Tools → Device Manager → Play en "GateKeep_Test"

**Opción B: Desde PowerShell**
```powershell
$emulatorPath = "$env:LOCALAPPDATA\Android\Sdk\emulator\emulator.exe"
& $emulatorPath -avd GateKeep_Test
```

### 7. Verificar Conexión del Emulador
```powershell
$adbPath = "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe"
& $adbPath devices
```
Debe mostrar: `emulator-5554   device`

### 8. Instalar APK
```powershell
cd Gatekeep\android
$adbPath = "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe"
& $adbPath install app\build\outputs\apk\release\app-release-unsigned.apk
```

### 9. Probar la App
- Busca el ícono "GateKeep" en el emulador
- Ábrelo y verifica que carga la aplicación web

## Solución de Problemas

### Emulador no inicia
- Verifica variables: `ANDROID_SDK_ROOT` y `ANDROID_HOME`
- Asegúrate de que la ruta no tenga doble "Sdk"
- Inicia desde Android Studio si falla desde línea de comandos

### APK no se instala
- Verifica que el emulador esté completamente iniciado (pantalla de Android visible)
- Ejecuta: `adb devices` para confirmar conexión
- Usa: `adb install -r` para reinstalar

### URL incorrecta
- Verifica que ngrok esté corriendo
- Actualiza la URL en `build.gradle`
- Recompila el APK después de cambiar la URL

## Archivos Importantes
- APK: `Gatekeep\android\app\build\outputs\apk\release\app-release-unsigned.apk`
- Config: `Gatekeep\android\app\build.gradle`
- Ngrok Panel: `http://localhost:4040`

