# Integración de TeleRehabAvatarUnity

App2 utiliza `flutter_embed_unity` para mostrar el avatar dentro de la pantalla
motivacional y enviarle el contexto del paciente mediante JSON.

## Preparar Unity para Android

En Unity Hub, abre `Installs > 2022.3.62f3 > Add modules` e instala:

- Android Build Support
- Android SDK & NDK Tools
- OpenJDK

Después exporta el módulo:

```bash
./scripts/export_unity_android.sh
```

El comando genera `android/unityLibrary`. Gradle detecta ese directorio y lo
incluye automáticamente al construir App2:

```bash
flutter build apk --debug
```

Cada vez que cambie la escena o los scripts Unity, vuelve a ejecutar la
exportación antes de compilar Flutter.
