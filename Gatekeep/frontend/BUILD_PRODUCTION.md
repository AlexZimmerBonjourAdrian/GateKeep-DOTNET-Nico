1) Ir a Frontend y tirar:
npm install; npm run build

2) En otra consola ir a ../android y actualizar build.gradle:
Cambiar las líneas 14-15 a:
resValue "string", "launchUrl", "https://zimmzimmgames.com/"
resValue "string", "hostName", "zimmzimmgames.com"

3) Luego en ../android tirar:
./gradlew clean assembleRelease --stacktrace


------------------------------------------------

Si fueera con ngrok seria: 
2) En otra consola ir a ../android y actualizar build.gradle:
Cambiar las líneas 14-15 a:
resValue "string", "launchUrl", "https://fe272d47d0f6.ngrok-free.app/"
resValue "string", "hostName", "fe272d47d0f6.ngrok-free.app"

3) En otro consola ir a Frontend y tirar:
ngrok http 3000

4) Luego en ../android tirar:
./gradlew clean assembleRelease --stacktrace

