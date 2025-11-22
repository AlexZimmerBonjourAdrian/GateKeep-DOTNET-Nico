Ir a Frontend y tirar:
npm install; npm run build

en otra consola ir a ../android y actualizar build.gradle:
Cambiar las líneas 14-15 a:
resValue "string", "launchUrl", "https://zimmzimmgames.com/"
resValue "string", "hostName", "zimmzimmgames.com"

luego en ../android tirar:
./gradlew clean assembleRelease --stacktrace

