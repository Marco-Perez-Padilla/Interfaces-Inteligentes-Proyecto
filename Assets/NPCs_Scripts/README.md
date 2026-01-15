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
- [**SavWavMemory.cs**](Tutorial_IA/SavWavMemory.cs): Convierte un `AudioClip` a formato WAV. Usado internamente por `WhisperUI`.
- [**WhisperUI.cs**](Tutorial_IA/WhisperUI.cs): Permite grabar audio y transcribirlo a texto al pulsar un botón o tecla (V). Al hacerlo lanza un evento para informar que el texto ha sido transcrito.
- [**OllamaUI.cs**](Tutorial_IA/OllamaUI.cs): Toma la transcripción de `WhisperUI` y la envía a Ollama API junto con un prompt prefabricado de contexto con un segundo de retraso; muestra la respuesta en un `TextMeshPro` con estilo de escritura de RPG clásico.
- [**FloatingText3D.cs**](Tutorial_Ia/FloatingText3D.cs): Texto 3D que sigue al NPC y se inclina hacia la cámara.

### Movement
- [**MovementWithRigidbody.cs**](Movement/MovementWithRigidbody.cs): Permite mover un objeto físico con teclas (WASD). Usado en debug del proyecto.
- [**MoveCamera.cs**](Movement/MoveCamera.cs): Permite mover la cámara con el ratón. Usado en debug del proyecto.

### TriggerZones
- [**TriggerNotificator.cs**](TriggerZones/TriggerNotificator.cs): Notifica cuando un jugador entra o sale de una zona trigger. Se utiliza como base para varios eventos de zona.
- [**NPCAudioEvent.cs**](TriggerZones/NPCAudioEvent.cs): Reproduce un audio cuando el jugador entra en un trigger asociado.
- [**NPCChasingEvent.cs**](TriggerZones/NPCChasingEvent.cs): Inicia la persecución de un NPC hacia el jugador al entrar en un trigger; se detiene al salir o tras un tiempo.
- [**VisualChasing.cs**](TriggerZones/VisualChasing.cs): Similar a `NPCChasingEvent`, pero el NPC se detiene si el jugador lo mira y acelera si no.
- [**PlayerEnemyCollisionEvent.cs**](TriggerZones/PlayerEnemyCollisionEvent.cs): Asociado al jugador; dispara un evento cuando colisiona con un objeto con tag `Enemy`.
- [**TriggerZoneVisualizer.cs**](TriggerZones/TriggerZoneVisualizer.cs): Permite la visualización de una zona trigger en el editor de Unity. Usado en debug del proyecto.

### NoiseEvents
- [**NoiseDetector.cs**](NoiseEvents/NoiseDetector.cs): Detecta ruido del micrófono cuando el jugador entra en un trigger y lanza un evento si se supera un umbral.
- [**FlickeringLightOnNoiseEvent.cs**](NoiseEvents/FlickeringLightOnNoiseEvent.cs): Una luz que parpadea al recibir un evento de `NoiseDetector`, según la intensidad y distancia.

## Asignación de Scripts a Objetos

Para que los NPCs y triggers funcionen correctamente, cada script debe asociarse al objeto adecuado:

| Script | Objeto / Prefab | 
|--------|-----------------|
| `SavWavMemory.cs` | NPC Tutorial |
| `WhisperUI.cs` | NPC Tutorial / Canvas UI | 
| `OllamaUI.cs` | NPC Tutorial / Canvas UI |
| `FloatingText3D.cs` | NPC Tutorial | 
| `TriggerNotificator.cs` | Trigger Zones | 
| `NPCAudioEvent.cs` | Trigger de audio | 
| `NPCChasingEvent.cs` | NPC / Trigger | 
| `VisualChasing.cs` | NPC / Trigger |
| `PlayerEnemyCollisionEvent.cs` | Jugador | 
| `NoiseDetector.cs` | Trigger zones / NPC |
| `FlickeringLightOnNoiseEvent.cs` | Luz del túnel |


## Desarrollos adicionales realizados

Además de los puntos de desarrollo acordados, se implementaron las siguientes funcionalidades::

- **Luz de linterna y cámara VR**  
  - Se desarrolló un sistema de linterna con luz dinámica y ajuste de cámara VR, integrado con el jugador.  
