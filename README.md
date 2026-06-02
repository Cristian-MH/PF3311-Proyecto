# RehaBot: agente virtual para telerehabilitación

Prueba de concepto desarrollada para el curso **PF-3311 Sistemas de
Información: Agentes Virtuales Inteligentes**.

RehaBot apoya un flujo simulado de telerehabilitación: registra pacientes,
genera ejercicios, almacena el avance de cada sesión y entrega mensajes
motivacionales personalizados mediante un agente virtual. La PoC incorpora una
aplicación Flutter, un avatar integrado con Unity, un backend ASP.NET Core,
generación de texto con OpenAI y síntesis de voz con Azure Speech.

> Esta PoC es únicamente para fines académicos y demostrativos. No debe usarse
> para almacenar expedientes médicos reales ni para sustituir el criterio de un
> profesional de salud.

## Funcionalidades implementadas

- Registro de pacientes con edad, sexo, condición y familiaridad tecnológica.
- Generación de ejercicios de rehabilitación con OpenAI.
- Registro del progreso, estado de ánimo y nivel de dolor de cada sesión.
- Mensajes motivacionales personalizados con una respuesta local de respaldo.
- Avatar Unity embebido en la experiencia Flutter asistida.
- Síntesis de voz con Azure Speech en español de Costa Rica:
  - `M`: `es-CR-JuanNeural`.
  - `F`: `es-CR-MariaNeural`.
- API desplegable en Azure App Service.

## Estructura del repositorio

```text
PF3311-Proyecto/
|-- App1/telerehab_app/             # Aplicación Flutter base
|-- App2/telerehab_app/             # PoC Flutter con agente virtual
|-- Backend/PF3311.TeleRehab.Api/   # API ASP.NET Core
|-- TeleRehabAvatarUnity/           # Proyecto Unity del avatar
|-- docs/                           # Documentos de avance en PDF
|-- Carpeta/papers/                 # Artículos usados durante la investigación
`-- README.md
```

Documentación complementaria:

- [Documento de avance](docs/Tarea10_CMH.pdf)
- [Documento inicial](docs/v1.pdf)
- [Backend y endpoints](Backend/PF3311.TeleRehab.Api/README.md)
- [Integración Flutter y Unity](App2/telerehab_app/UNITY_INTEGRATION.md)
- [Despliegue del backend en Azure](Backend/PF3311.TeleRehab.Api/AZURE_DEPLOYMENT.md)

## Requisitos

- Flutter SDK compatible con Dart `^3.12.0`.
- .NET SDK `10.0`.
- Unity `2022.3.62f3` con Android Build Support para exportar nuevamente el
  avatar.
- Una API key de OpenAI.
- Un recurso Azure Speech con su llave y región.

## Ejecutar la PoC

### 1. Configurar y ejecutar el backend

Desde la raíz del repositorio:

```bash
cd Backend/PF3311.TeleRehab.Api
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "TU_OPENAI_API_KEY"
dotnet user-secrets set "AzureSpeech:Key" "TU_AZURE_SPEECH_KEY"
dotnet user-secrets set "AzureSpeech:Region" "eastus"
dotnet restore
dotnet run
```

La API imprime su URL local al iniciar. La interfaz Swagger está disponible en
`/swagger`.

Las credenciales deben mantenerse fuera del repositorio. Para desplegar en
Azure App Service, configura estas variables de entorno:

```text
OpenAI__ApiKey=<tu-api-key>
OpenAI__Model=gpt-5.4-mini
AzureSpeech__Key=<tu-speech-key>
AzureSpeech__Region=eastus
AzureSpeech__MaleVoiceName=es-CR-JuanNeural
AzureSpeech__FemaleVoiceName=es-CR-MariaNeural
```

### 2. Ejecutar la aplicación Flutter

La variante principal de la PoC está en `App2`:

```bash
cd App2/telerehab_app
flutter pub get
flutter run
```

La aplicación usa el backend publicado en Azure configurado en
`App2/telerehab_app/lib/screens/api_service.dart`. Para trabajar únicamente con
el backend local, actualiza temporalmente `baseUrl` en ese archivo.

### 3. Exportar nuevamente el avatar Unity

El módulo Android exportado solo necesita regenerarse cuando cambie la escena o
los scripts de Unity:

```bash
cd App2/telerehab_app
./scripts/export_unity_android.sh
flutter build apk --debug
```

Consulta [UNITY_INTEGRATION.md](App2/telerehab_app/UNITY_INTEGRATION.md) para
instalar los módulos requeridos de Unity.

## Probar síntesis de voz

El backend expone `POST /api/speech/synthesize`. El campo `sex` selecciona la
voz masculina o femenina:

```bash
curl -X POST https://TU-APP.azurewebsites.net/api/speech/synthesize \
  -H "Content-Type: application/json" \
  -d '{"text":"Excelente trabajo. Continúa a tu ritmo.","sex":"M"}' \
  --output speech.mp3
```

Usa `"F"` para probar la voz femenina.

## Video de demostración

**Pendiente:** agregar aquí el enlace público o institucional al video de
demostración de la PoC.

## Autor

Cristian Martínez Hernández
