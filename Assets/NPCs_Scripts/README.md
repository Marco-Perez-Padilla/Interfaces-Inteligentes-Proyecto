# Proyecto Unity: Tunnel Horror / NPC Interaction 

## Encargado
**Marco Pérez Padilla**

### Puntos de desarrollo acordados
1. Desarrollo del NPC del tutorial usando Whisper para reconocimiento de voz y envío de la transcripción a la API de LLM usando eventos.  
2. Desarrollo de NPCs del túnel del terror que tengan distintos comportamientos y respondan a triggers y eventos.  
3. Los comportamientos acordados se relacionan con:  

- Persecuciones de NPC's activadas al entrar en una zona trigger.
- Reproducciones de audios al entrar en una zona trigger.

---

## NPCs_Scripts - Puntos de desarrollo realizados

### Tutorial_IA

Conjunto de scripts relacionados con el NPC del tutorial, diseñado para interactuar con el jugador durante la introducción al túnel del terror.

#### Funcionamiento básico
- El NPC escucha el audio del jugador y responde de forma personalizada, asegurándose siempre de dar la bienvenida al “túnel del terror”.
- En una segunda interacción, si el jugador indica que está preparado, el NPC proporciona un breve tutorial sobre cómo empezar la experiencia.

#### Scripts principales utilizados
- **SavWavMemory.cs**: Convierte un `AudioClip` a formato WAV. Usado internamente por `WhisperUI`.
- **WhisperUI.cs**: Permite grabar audio y transcribirlo a texto al pulsar un botón o tecla (V). Al hacerlo lanza un evento para informar que el texto ha sido transcrito.
- **OllamaUI.cs**: Toma la transcripción de `WhisperUI` y la envía a Ollama API junto con un prompt prefabricado de contexto con un segundo de retraso; muestra la respuesta en un `TextMeshPro` con estilo de escritura de RPG clásico.
- **FloatingText3D.cs**: Texto 3D que sigue al NPC y se inclina hacia la cámara.

### Movement
- **MovementWithRigidbody.cs**: Permite mover un objeto físico con teclas (WASD). Actualmente no usado en el proyecto.

### Trigger_Zones
- **TriggerNotificator.cs**: Notifica cuando un jugador entra o sale de una zona trigger. Se utiliza como base para varios eventos de zona.
- **NPCAudioEvent.cs**: Reproduce un audio cuando el jugador entra en un trigger asociado.
- **NPCChasingEvent.cs**: Inicia la persecución de un NPC hacia el jugador al entrar en un trigger; se detiene al salir o tras un tiempo.
- **VisualChasing.cs**: Similar a `NPCChasingEvent`, pero el NPC se detiene si el jugador lo mira y acelera si no.
- **PlayerEnemyCollisionEvent.cs**: Asociado al jugador; dispara un evento cuando colisiona con un objeto con tag `Enemy`.

### NoiseEvents
- **NoiseDetector.cs**: Detecta ruido del micrófono cuando el jugador entra en un trigger y lanza un evento si se supera un umbral.
- **FlickeringLightOnNoiseEvent.cs**: Una luz que parpadea al recibir un evento de `NoiseDetector`, según la intensidad y distancia.