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
- [**SavWavMemory.cs**](Assets/NPCs_Scripts/Tutorial_IA/SavWavMemory.cs): Convierte un `AudioClip` a formato WAV. Usado internamente por `WhisperUI`.
- [**WhisperUI.cs**](Assets/NPCs_Scripts/Tutorial_IA/WhisperUI.cs): Permite grabar audio y transcribirlo a texto al pulsar un botón o tecla (V). Al hacerlo lanza un evento para informar que el texto ha sido transcrito.
- [**OllamaUI.cs**](Assets/NPCs_Scripts/Tutorial_IA/OllamaUI.cs): Toma la transcripción de `WhisperUI` y la envía a Ollama API junto con un prompt prefabricado de contexto con un segundo de retraso; muestra la respuesta en un `TextMeshPro` con estilo de escritura de RPG clásico.
- [**FloatingText3D.cs**](Assets/NPCs_Scripts/Tutorial_Ia/FloatingText3D.cs): Texto 3D que sigue al NPC y se inclina hacia la cámara.
- [**CarretillaUI.cs**](Assets/NPCs_Scripts/Tutorial_IA/CarretillaUI.cs): Cuando el jugador está en contacto con la carretilla, muestra los controles. El menú se puede esconder pulsando H o secondary trigger en right hand.

### Movement
- [**MovementWithRigidbody.cs**](Assets/NPCs_Scripts/Movement/MovementWithRigidbody.cs): Permite mover un objeto físico con teclas (WASD). Usado en debug del proyecto.
- [**MoveCamera.cs**](Assets/NPCs_Scripts/Movement/MoveCamera.cs): Permite mover la cámara con el ratón. Usado en debug del proyecto.

### TriggerZones
- [**TriggerNotificator.cs**](Assets/NPCs_Scripts/TriggerZones/TriggerNotificator.cs): Notifica cuando un jugador entra o sale de una zona trigger. Se utiliza como base para varios eventos de zona.
- [**NPCAudioEvent.cs**](Assets/NPCs_Scripts/TriggerZones/NPCAudioEvent.cs): Reproduce un audio cuando el jugador entra en un trigger asociado.
- [**NPCChasingEvent.cs**](Assets/NPCs_Scripts/TriggerZones/NPCChasingEvent.cs): Inicia la persecución de un NPC hacia el jugador al entrar en un trigger; se detiene al salir o tras un tiempo.
- [**VisualChasing.cs**](Assets/NPCs_Scripts/TriggerZones/VisualChasing.cs): Similar a `NPCChasingEvent`, pero el NPC se detiene si el jugador lo mira y acelera si no.
- [**PlayerEnemyCollisionEvent.cs**](Assets/NPCs_Scripts/TriggerZones/PlayerEnemyCollisionEvent.cs): Asociado al jugador; dispara un evento cuando colisiona con un objeto con tag `Enemy`.
- [**TriggerZoneVisualizer.cs**](Assets/NPCs_Scripts/TriggerZones/TriggerZoneVisualizer.cs): Permite la visualización de una zona trigger en el editor de Unity. Usado en debug del proyecto.

### NoiseEvents
- [**NoiseDetector.cs**](Assets/NPCs_Scripts/NoiseEvents/NoiseDetector.cs): Detecta ruido del micrófono cuando el jugador entra en un trigger y lanza un evento si se supera un umbral.
- [**NPCNoiseSpawner**](Assets/NPCs_Scripts/NoiseEvents/NPCNoiseSpawner): Detecta un ruido muy fuerte y genera enemigos alrededordel jugador de forma aleatoria excepto un cono de exclusión que es el vector forward de la vagoneta.
- [**FlickeringLightOnNoiseEvent.cs**](Assets/NPCs_Scripts/NoiseEvents/FlickeringLightOnNoiseEvent.cs): Una luz que parpadea al recibir un evento de `NoiseDetector`, según la intensidad y distancia.

### Organización 
Además de la organización en distintos directiorios dentro de NPCs_Scripts, se organiza el proyecto con directorios dentro de Assets llamados 'NPCs_Audios' y 'NPCs_Prefabs', en los cuales se almacenan, respectivamente, los audios y prefabs utilizados.

## Asignación de Scripts a Objetos

Para que los NPCs y triggers funcionen correctamente, cada script debe asociarse al objeto adecuado:

| Script | Objeto / Prefab | 
|--------|-----------------|
| `SavWavMemory.cs` | NPC Tutorial |
| `WhisperUI.cs` | NPC Tutorial / Canvas UI | 
| `OllamaUI.cs` | NPC Tutorial / Canvas UI |
| `FloatingText3D.cs` | NPC Tutorial | 
| `CarretillaUI.cs` | Carretilla |
| `TriggerNotificator.cs` | Trigger Zones | 
| `NPCAudioEvent.cs` | Trigger de audio | 
| `NPCChasingEvent.cs` | NPC / Trigger | 
| `VisualChasing.cs` | NPC / Trigger |
| `PlayerEnemyCollisionEvent.cs` | Jugador | 
| `NoiseDetector.cs` | Trigger zones / NPC |
| `NPCNoiseSpawner` | Trigger zones / NPC|
| `FlickeringLightOnNoiseEvent.cs` | Luz del túnel o linterna |

## Desarrollos adicionales realizados

Además de los puntos de desarrollo acordados, implemento las siguientes funcionalidades::

- **Luz de linterna y cámara VR**  
Se integra XR Origin en el proyecto y se desarrolla una linterna que sigue la mano del jugador y responde a ciertas acciones del mismo. Asimismo, se mapean correctamente todas las acciones previamente disponibles solo en teclado para poder admitir tanto teclado como controles VR, así como también Gamepads.
Se desarrollan para esto fin los siguientes scripts e inputactions:

### Flashlight
- [**FlashlightControls.inputactions**](Assets/Flashlight_InputActions/FlashlightControls.inputactions): Mapeado de acciones de la vagoneta para teclado, gamepad, y VR.
- [**FlashlightVRController.cs**](Assets/Flashlight_Scripts/FlashlightVRController.cs): Habilita las opciones de encender/apagar la linterna, así como de cambiar la linterna de mano. Se intenta detectar dinámicamente qué modelo de mano se está usando.

### Vagoneta
- [**CartControl.inputactions**](Assets/CartControl.inputactions): Mapeado de acciones de la vagoneta para teclado, gamepad, y VR.
