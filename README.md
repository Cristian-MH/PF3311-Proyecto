# RehaBot: agente virtual para telerehabilitación

Prueba de concepto desarrollada para el curso **PF-3311 Sistemas de
Información: Agentes Virtuales Inteligentes**.

## Diseño, implementación y evaluación de un agente virtual inteligente para telerehabilitación

Repositorio del proyecto del curso **PF-3311 Sistemas de Información: Agentes
Virtuales Inteligentes**.

El proyecto propone una aplicación móvil con un agente virtual inteligente para
apoyar procesos de telerehabilitación, con énfasis en la motivación del paciente
y el registro adecuado de tareas. La propuesta contempla interacciones
adaptativas según características del paciente, como edad, sexo y nivel de
adherencia, e incorpora una modalidad con embodiment mediante avatar y voz.

## Descripción

La telerehabilitación permite dar seguimiento remoto a pacientes, pero suele
enfrentar dificultades relacionadas con la adherencia, la motivación y la
calidad del registro de actividades. Este proyecto explora el uso de un agente
virtual inteligente como apoyo personalizado para mejorar esos procesos.

## Preguntas de investigación

- ¿Cuál es el efecto comparativo del uso de un agente virtual inteligente
  adaptativo, frente a un método convencional de telerehabilitación, sobre la
  motivación del paciente y la calidad del registro de tareas durante el proceso
  de rehabilitación?
- ¿Qué estrategias de personalización, basadas en las características del
  paciente, puede implementar un agente virtual inteligente adaptativo, y cómo
  se relacionan dichas estrategias con la motivación y la calidad del registro
  de tareas en telerehabilitación?

## Alcance del proyecto

El proyecto está enfocado en personas que han participado previamente en
procesos de rehabilitación. La evaluación se plantea en un entorno simulado,
comparando el método convencional con una experiencia asistida por un agente
virtual.

El camino principal de uso contempla:

1. El usuario abre la aplicación.
2. Registra información de contexto y sesiones asignadas.
3. El agente reconoce la información del usuario y lo saluda por su nombre.
4. El agente propone iniciar las sesiones de rehabilitación.
5. El usuario registra la actividad realizada.
6. El sistema responde con mensajes motivadores que reconocen el avance y el
   esfuerzo.

También se consideran escenarios difíciles, como recordatorios mediante
notificaciones, reconocimiento de voz impreciso y problemas de conectividad o
latencia.

## Stack tecnológico

- **Aplicación móvil:** Flutter con Dart.
- **Backend:** ASP.NET Core sobre .NET `10.0`.
- **LLM:** OpenAI Responses API con el modelo configurable
  `gpt-5.4-mini`.
- **Motor visual de App2:** Unity `2022.3.62f3`, integrado en Flutter mediante
  `flutter_embed_unity`. App1 utiliza únicamente texto.
- **Voz de App2:** Azure Speech para síntesis de voz TTS en español de Costa
  Rica.
- **Avatares y animaciones de App2:** recursos obtenidos de Mixamo e integrados
  en Unity.
- **Persistencia de la PoC:** almacenamiento temporal en memoria respaldado por
  un archivo JSON y preferencias locales mediante `shared_preferences`.
- **Despliegue:** Azure App Service.
- **Embodiment:** avatar Unity adaptado al contexto del paciente, con mensajes
  motivacionales y voz seleccionada según sexo.
- **Dominio de aplicación:** fisioterapia, ejercicios personalizados y
  seguimiento continuo.

## Avance de la PoC

RehaBot apoya un flujo simulado de telerehabilitación: registra pacientes,
genera ejercicios, almacena el avance de cada sesión y entrega mensajes
motivacionales personalizados. La PoC incorpora dos aplicaciones Flutter para
comparar una experiencia convencional con una experiencia asistida por un
agente virtual. Ambas utilizan el mismo backend ASP.NET Core y la generación de
texto con OpenAI. App2 incorpora además un avatar integrado con Unity y
síntesis de voz con Azure Speech.

## Variantes de la aplicación

| Variante | Ubicación | Experiencia |
| --- | --- | --- |
| App1 | `App1/telerehab_app/` | Aplicación convencional. Presenta los mensajes motivacionales únicamente como texto. |
| App2 | `App2/telerehab_app/` | Aplicación asistida por agente virtual. Integra avatares y animaciones de Mixamo mediante Unity, además de la modalidad con voz. |

> Esta PoC es únicamente para fines académicos y demostrativos. No debe usarse
> para almacenar expedientes médicos reales ni para sustituir el criterio de un
> profesional de salud.

## Funcionalidades implementadas

- Registro de pacientes con edad, sexo, condición y familiaridad tecnológica.
- Generación de ejercicios de rehabilitación con OpenAI.
- Registro del progreso, estado de ánimo y nivel de dolor de cada sesión.
- Mensajes motivacionales personalizados con una respuesta local de respaldo.
- Avatares y animaciones de Mixamo integrados en Unity y embebidos en la
  experiencia Flutter asistida de App2.
- Síntesis de voz para App2 con Azure Speech en español de Costa Rica:
  - `M`: `es-CR-JuanNeural`.
  - `F`: `es-CR-MariaNeural`.
- API desplegable en Azure App Service.

## Estructura del repositorio

```text
PF3311-Proyecto/
|-- App1/telerehab_app/             # App convencional basada en texto
|-- App2/telerehab_app/             # App asistida con avatar Unity y voz
|-- Backend/PF3311.TeleRehab.Api/   # API ASP.NET Core
|-- TeleRehabAvatarUnity/           # Proyecto Unity del avatar
|-- Carpeta/papers/                 # Artículos usados durante la investigación
`-- README.md
```

Documentación complementaria:

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

### 2. Ejecutar App1: experiencia convencional con texto

```bash
cd App1/telerehab_app
flutter pub get
flutter run
```

### 3. Ejecutar App2: experiencia asistida con Unity

App2 incorpora el agente virtual:

```bash
cd App2/telerehab_app
flutter pub get
flutter run
```

Ambas aplicaciones usan el backend publicado en Azure configurado en sus
respectivos archivos `lib/screens/api_service.dart`. Para trabajar únicamente
con el backend local, actualiza temporalmente `baseUrl` en la variante que
deseas ejecutar.

### 4. Exportar nuevamente el avatar Unity para App2

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

[Ver video de demostración de la PoC en YouTube](https://youtu.be/hbo0idGXSQ4).

## Autor

Cristian Martínez Hernández
