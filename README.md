# PF3311-Proyecto

## Diseno, implementacion y evaluacion de un agente virtual inteligente para telerehabilitacion

Repositorio del proyecto del curso **PF-3311 Sistemas de Informacion: Agentes Virtuales Inteligentes**.

El proyecto propone una aplicacion movil con un agente virtual inteligente para apoyar procesos de telerehabilitacion, con enfasis en la motivacion del paciente y el registro adecuado de tareas. La propuesta contempla interacciones adaptativas segun caracteristicas del paciente, como edad, sexo y nivel de adherencia, e incorpora una modalidad con embodiment mediante avatar y voz.

## Descripcion

La telerehabilitacion permite dar seguimiento remoto a pacientes, pero suele enfrentar dificultades relacionadas con la adherencia, la motivacion y la calidad del registro de actividades. Este proyecto explora el uso de un agente virtual inteligente como apoyo personalizado para mejorar esos procesos.

## Preguntas de investigacion

- ¿Cuál es el efecto comparativo del uso de un agente virtual inteligente adaptativo, frente a un método convencional de telerehabilitación, sobre la motivación del paciente y la calidad del registro de tareas durante el proceso de rehabilitación?
- ¿Qué estrategias de personalización, basadas en las características del paciente, puede implementar un agente virtual inteligente adaptativo, y cómo se relacionan dichas estrategias con la motivación y la calidad del registro de tareas en telerehabilitación?

## Alcance del proyecto

El proyecto esta enfocado en personas que han participado previamente en procesos de rehabilitacion. La evaluacion se plantea en un entorno simulado, comparando el metodo convencional con una experiencia asistida por un agente virtual.

El camino principal de uso contempla:

1. El usuario abre la aplicacion.
2. Registra informacion de contexto y sesiones asignadas.
3. El agente reconoce la informacion del usuario y lo saluda por su nombre.
4. El agente propone iniciar las sesiones de rehabilitacion.
5. El usuario registra la actividad realizada.
6. El sistema responde con mensajes motivadores que reconocen el avance y el esfuerzo.

Tambien se consideran escenarios dificiles, como recordatorios mediante notificaciones, reconocimiento de voz impreciso y problemas de conectividad o latencia.

## Stack tecnologico propuesto

- **LLM:** Gemini o GPT-4o.
- **Motor visual:** Unity con graficos 2D para la creacion del agente virtual.
- **Voz:** Azure Cognitive Services Speech o ElevenLabs para TTS/STT.
- **Embodiment:** avatar con voz y blendshapes.
- **Dominio de aplicacion:** fisioterapia, ejercicios personalizados y seguimiento continuo.

## Estructura del repositorio

```text
PF3311-Proyecto/
|-- README.md
`-- Carpeta/
    |-- docs/v1.pdf
    `-- papers/
        |-- 1.pdf
        |-- 2.pdf
        |-- ...
        `-- 29.pdf
```

## Autor

Cristian Martinez Hernandez
