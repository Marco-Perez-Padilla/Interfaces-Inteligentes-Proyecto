# Tunnel Horror — Laberinto de Realidad Virtual

> Aplicación de Realidad Virtual desarrollada en Unity como proyecto de grupo para la
> asignatura de Interfaces Inteligentes (ULL). El jugador recorre un laberinto procedural
> en una vagoneta, enfrentando enemigos que emergen al atravesar zonas de activación y
> al hacer ruido.

---

## Índice

1. [Cuestiones importantes para el uso](#1-cuestiones-importantes-para-el-uso)
2. [Descripción general de la aplicación](#2-descripción-general-de-la-aplicación)
3. [Hitos de programación](#3-hitos-de-programación)
4. [Aspectos destacados](#4-aspectos-destacados)
5. [Sensores incluidos](#5-sensores-incluidos)
6. [GIF de ejecución](#6-gif-de-ejecución)
7. [Acta de acuerdos del grupo](#7-acta-de-acuerdos-del-grupo)
8. [Check-list de diseño de aplicaciones de RV](#8-check-list-de-diseño-de-aplicaciones-de-rv)
9. [Referencia de scripts por integrante](#9-referencia-de-scripts-por-integrante)

---

## 1. Cuestiones importantes para el uso

### Requisitos de hardware

- **Recomendado:** Gafas de Realidad Virtual compatibles con OpenXR (Meta Quest, etc.).
- **Alternativa:** Teclado + ratón para pruebas en editor (modo debug).
- **Espacio físico:** Se recomienda al menos 1 m² libre para mover el brazo con seguridad al usar controles VR.
- **Micrófono:** Obligatorio para las mecánicas de detección de ruido y el NPC del tutorial con IA.

### Servicios externos requeridos

| Servicio | URL configurada | Uso |
|----------|----------------|-----|
| Whisper (transcripción de voz) | `http://gpu2.esit.ull.es:8000/v1/audio/transcriptions` | NPC tutorial |
| Ollama (LLM) | `http://gpu1.esit.ull.es:4000/v1/chat/completions` | Respuestas del NPC tutorial |

> **Importante:** La aplicación debe ejecutarse con acceso de red a los servicios anteriores
> para que el NPC del tutorial responda. Sin ellos, el tutorial no funciona.

### Controles

#### Vagoneta

| Acción | Teclado | Gamepad | VR |
|--------|---------|---------|-----|
| Impulsar | `V` | Right Trigger | gripPressed (mano derecha) |
| Frenar | `X` | Left Trigger | gripPressed (mano izquierda) |
| Decidir en bifurcación | `A` / `D` / `W` | Stick izquierdo | Primary2DAxis (mano derecha) |
| Montar | `E` | Botón Sur | Primary Button (mano derecha) |
| Ocultar controles | `H` | Button East | Secondary button (mano derecha) |

#### Linterna

| Acción | Teclado | Gamepad | VR |
|--------|---------|---------|----|
| Encender / Apagar | `T` | Dpad Down | Primary button (mano izquierda) |
| Cambiar de mano | `J` | Dpad Right | Secondary button (mano izquierda) |

#### NPC Tutorial (Carretilla)

| Acción | Teclado | Gamepad | VR |
|--------|---------|---------|----|
| Grabar voz | `X` | Left Trigger | gripPressed (mano izquierda) |

### Advertencias de uso

- Si el jugador habla **muy alto** en zonas de detección de ruido, aparecerán enemigos a su alrededor.
- El NPC del tutorial con IA puede tardar varios segundos en responder dependiendo de la carga del servidor.
- El laberinto se genera proceduralmente con semilla fija: **cada sesión produce el mismo mapa**, aunque contempla iteraciones futuras con semilla aleatoria.

---

## 2. Descripción general de la aplicación

El jugador se sube a una vagoneta que recorre un laberinto de túneles generado de forma
procedural. A lo largo del recorrido, distintas zonas de activación (`TriggerZone`) despiertan
comportamientos de los NPCs enemigos: algunos persiguen al jugador directamente, otros se
detienen si el jugador los mira, y las luces del entorno parpadean en respuesta al ruido que
hace el jugador. Al inicio, un NPC tutorial con IA conversacional guía al jugador mediante
reconocimiento de voz.

El objetivo es llegar al final del laberinto sin ser alcanzado por ningún enemigo.

---

## 3. Hitos de programación

Los hitos se relacionan con los contenidos impartidos en la asignatura.

### 3.1 Sistema de eventos y notificadores (Interfaces multimodales — eventos)

Todos los comportamientos de NPCs y triggers se gestionan mediante el patrón
**publicador–suscriptor**. `TriggerNotificator` expone eventos C# (`OnPlayerEntered`,
`OnPlayerExited`) a los que se suscriben los scripts de comportamiento sin acoplamiento
directo entre ellos mediante colisiones entre zonas de trigger dadas por colliders.

Scripts involucrados: [`TriggerNotificator`](Assets/Scripts/NPCs_Scripts/TriggerZones/TriggerNotificator.cs), [`NPCAudioEvent`](Assets/Scripts/NPCs_Scripts/TriggerZones/NPCAudioEvent.cs), [`NPCChasingEvent`](Assets/Scripts/NPCs_Scripts/TriggerZones/NPCChasingEvent.cs),
[`VisualChasing`](Assets/Scripts/NPCs_Scripts/TriggerZones/VisualChasing.cs), [`NoiseDetector`](Assets/Scripts/NPCs_Scripts/NoiseEvents/NoiseDetector.cs), [`FlickeringLightOnNoiseEvent`](Assets/Scripts/NPCs_Scripts/NoiseEvents/FlickeringLightOnNoiseEvent.cs), [`NPCNoiseSpawner`](Assets/Scripts/NPCs_Scripts/NoiseEvents/NPCNoiseSpawner.cs), [`PlayerEnemyCollisionEvent`](Assets/Scripts/NPCs_Scripts/TriggerZones/PlayerEnemyCollisionEvent.cs).

### 3.2 Reconocimiento de voz e integración con LLM (Sensores — micrófono / IA)

El NPC del tutorial captura audio del micrófono del jugador, lo convierte a WAV y lo
envía a la API de **Whisper** para transcribirlo. La transcripción se reenvía a **Ollama**
con un prompt de contexto y la respuesta se muestra con efecto de escritura de máquina
de escribir.

Scripts involucrados: [`SavWavMemory`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/SavWavMemory.cs), [`WhisperUI`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/WhisperUI.cs), [`OllamaUI`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/OllamaUI.cs), [`FloatingText3D`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/FloatingText3D.cs).

### 3.3 Detección de ruido ambiental mediante micrófono (Sensores — audio)

`NoiseDetector` mantiene una grabación continua del micrófono mientras el jugador
permanece en una zona trigger. Analiza el nivel RMS del audio capturado y dispara
eventos diferenciados según dos umbrales: ruido normal y ruido alto. Otros sistemas
reaccionan a estos eventos de forma independiente.

Scripts involucrados: [`NoiseDetector`](Assets/Scripts/NPCs_Scripts/NoiseEvents/NoiseDetector.cs), [`NPCNoiseSpawner`](Assets/Scripts/NPCs_Scripts/NoiseEvents/NPCNoiseSpawner.cs), [`FlickeringLightOnNoiseEvent`](Assets/Scripts/NPCs_Scripts/NoiseEvents/FlickeringLightOnNoiseEvent.cs).

### 3.4 Spawn dinámico de enemigos con lógica de exclusión espacial (Programación de juego)

`NPCNoiseSpawner` genera dinámicamente en un radio alrededor del jugador excluyendo un cono frontal de la vagoneta enemigos al detectar ruido alto. Esto también ocurre con `EnemySpawner`, que se suscribe en `Start()` —tras la generación del grafo— a todos los
`TriggerNotificator` y `NoiseDetector` de la escena (estodos asignados automáticamente por `PlaceTriggerSetup`).
Cada enemigo instanciado
pasa por `EnemyInitializer`, que asigna tag, collider de contacto, Rigidbody y
comportamiento de IA aleatorio.

Scripts involucrados: [`EnemySpawner`](Assets/Scripts/NPCs_Scripts/EnemySpawner.cs), [`EnemyInitializer`](Assets/Scripts/NPCs_Scripts/EnemyInitializer.cs), [`NPCNoiseSpawner`](Assets/Scripts/NPCs_Scripts/NoiseEvents/NPCNoiseSpawner.cs), [`PlaceTriggerSetup`](Assets/Scripts/NPCs_Scripts/PlaceTriggerSetup.cs).

### 3.5 Generación procedural del laberinto (Algoritmos — generación de contenido)

El laberinto se genera mediante un pipeline en cadena de responsabilidad única, ejecutado
una sola vez por partida. El camino principal usa DFS con backtracking; las subrutas se
generan respetando reglas de cooldown y pendientes máximas de 45°. La altura es
independiente de la topología.

Scripts involucrados: [`PathGenerator`](Assets/Scripts/PathSystem/Generation/PathGenerator.cs), [`PathGraph`](Assets/Scripts/PathSystem/Core/PathGraph.cs), [`PathNode`](Assets/Scripts/PathSystem/Core/PathNode.cs), [`Grid2D`](Assets/Scripts/PathSystem/Generation/Grid2D.cs),
[`MainPathGenerator`](Assets/Scripts/PathSystem/Generation/MainPathGenerator.cs), [`SubPathGenerator`](Assets/Scripts/PathSystem/Generation/SubPathGenerator.cs), [`SubPathCooldownResolver`](Assets/Scripts/PathSystem/Generation/SubPathCooldownResolver.cs), [`HeightModulator`](Assets/Scripts/PathSystem/Generation/HeightModulator.cs),
[`SlopeLimiter`](Assets/Scripts/PathSystem/Generation/SlopeLimiter.cs), [`DecisionResolver`](Assets/Scripts/PathSystem/Generation/DecisionResolver.cs), [`PathGizmosDrawer`](Assets/Scripts/PathSystem/Unity/PathGizmosDrawer.cs).

### 3.6 Construcción visual del laberinto (Programación de juego — instanciación de prefabs)

`LegacyPieceApplier` recorre el grafo generado y coloca prefabs 3D en cada nodo
seleccionando la pieza correcta según la topología local: recta, giro, bifurcación o
final. `PathSurfaceBuilder` construye las vías de la vagoneta entre nodos
instanciando piezas de rail en modo Tile (sin escalado).

Scripts involucrados: [`LegacyPieceApplier`](Assets/PieceApplier/LegacyPieceApplier.cs), [`PathSurfaceBuilder`](Assets/Scripts/PathSurfaceBuilder/PathSurfaceBuilder.cs), [`Piece`](Assets/Pieces/Piece.cs).
Prefabs: `straightCave`, `TurnL`, `TurnR`, `forkLeftRight`, `forkLeftStraight`,
`forkRightStraight`, `forkTripleCave`, `corridorUpCave`, `corridorDownCave`,
`endCave` y sus variantes con triggers integrados.

### 3.7 Movimiento de la vagoneta sobre el grafo (Programación de juego — navegación)

`CartMovement` avanza automáticamente entre nodos del grafo. Cuando un nodo tiene
más de una salida válida, `CartDecisionController` recibe la entrada del jugador
(relativa a la orientación de la cámara) y selecciona la salida correcta. `CameraShake`
aplica vibración Perlin proporcional a la velocidad para feedback visual -- No usada en la versión VR --. `CartMount`
gestiona el montaje/desmontaje del jugador en la vagoneta.

Scripts involucrados: [`CartMovement`](Assets/Scripts/GamePlay/CartMovement.cs), [`CartDecisionController`](Assets/Scripts/GamePlay/CartDecisionController.cs), [`CameraShake`](Assets/Scripts/GamePlay/CameraShake.cs), [`CartMount`](Assets/Scripts/GamePlay/CartMount.cs).

### 3.8 Compatibilidad multi-dispositivo con Input System (Interfaces multimodales — input)

Todas las acciones del juego están mapeadas mediante el **New Input System** de Unity
en dos `.inputactions` independientes, permitiendo que el mismo juego funcione con
teclado, gamepad y controles VR sin cambios de código.

Archivos: [`FlashlightControls.inputactions`](Assets/Flashlight_InputActions/FlashlightControls.inputactions), [`CartControl.inputactions`](Assets/CartControl.inputactions).

### 3.9 NPC con mecánica Weeping Angel (Diseño de juego — comportamiento de IA)

`VisualChasing` implementa la mecánica de los Weeping Angels: el NPC persigue al
jugador pero se detiene si este lo mira. La detección se basa en el ángulo entre el
vector de la cámara y la dirección al NPC, configurable mediante `viewAngle`. Si el
jugador deja de mirar, la persecución se reanuda.

Script involucrado: [`VisualChasing`](Assets/Scripts/NPCs_Scripts/TriggerZones/VisualChasing.cs).

### 3.10 Colisión jugador–enemigo y Game Over (Programación de juego — gestión de estado)

`PlayerEnemyCollisionEvent` detecta la colisión física entre el jugador y cualquier
objeto con tag `Enemy` y llama a `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex)`, reiniciando la escena. Una alternativa es llamar a  `GameManager.Instance.GameOver()`. `GameManager`
implementa el patrón Singleton, pausa el tiempo (`Time.timeScale = 0`) y muestra
el panel de derrota o victoria según corresponda.

Scripts involucrados: [`PlayerEnemyCollisionEvent`](Assets/Scripts/NPCs_Scripts/TriggerZones/PlayerEnemyCollisionEvent.cs), [`GameManager`](Assets/Scripts/NPCs_Scripts/GameManager.cs).

---

## 4. Aspectos destacados

- **Generación procedural del laberinto dirigido:** el camino principal nunca se modifica
  tras su generación y sirve de fuente única de verdad para la navegación. Las subrutas
  generan atajos y bucles emergentes sin romper la jugabilidad.

- **Pipeline de generación en cadena de responsabilidad única:** cada etapa (`Grid2D` →
  `MainPathGenerator` → `HeightModulator` → `SubPathGenerator` → `SlopeLimiter` →
  `DecisionResolver`) tiene una única responsabilidad y orden de ejecución garantizado.

- **Variantes de prefabs con triggers integrados:** las piezas del laberinto tienen
  variantes predefinidas (`VariantChase`, `VariantNoiseDetector`, `VariantNoisePlayer`)
  con la intención que en próximas iteraciones se puedan separar responsabilidades y permitir que cada pieza maneje qué comportamiento generar (y no centralizarlo en el script `PlaceTriggerSetup`)

- **NPC conversacional con IA local:** el tutorial usa Whisper (reconocimiento de voz
  en GPU de la ULL) y Ollama para generar respuestas contextualizadas al jugador en
  tiempo real, sin depender de servicios externos de pago.

- **Mecánica de ruido como sensor real:** el micrófono del dispositivo actúa como sensor
  de juego. Hablar alto en zonas concretas del laberinto tiene consecuencias directas en
  la aparición de enemigos y el comportamiento de la iluminación.

- **Compatibilidad total teclado / gamepad / VR:** gracias al New Input System, la misma
  build funciona en escritorio para pruebas y en visor VR para la experiencia completa.

---

## 5. Sensores incluidos

Indirectamente (a través de APIS para su uso):
| Sensor | Uso en la aplicación | Script |
|--------|---------------------|--------|
| **Micrófono (audio / RMS)** | Detección de ruido del jugador en zonas trigger; activa spawn de enemigos y parpadeo de luces | [`NoiseDetector`](Assets/Scripts/NPCs_Scripts/NoiseEvents/NoiseDetector.cs) |
| **Micrófono (voz / ASR)** | Captura y transcripción de voz para el NPC tutorial mediante Whisper | [`WhisperUI`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/WhisperUI.cs), [`SavWavMemory`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/SavWavMemory.cs) |
| **Cámara / tracking de cabeza (VR)** | Detección de si el jugador mira a un enemigo (mecánica Weeping Angel); orientación relativa para decisiones en bifurcaciones | [`VisualChasing`](Assets/Scripts/NPCs_Scripts/TriggerZones/VisualChasing.cs), [`CartDecisionController`](Assets/Scripts/GamePlay/CartDecisionController.cs) |

Directamente (uso directo del giroscopio):
| Sensor | Uso en la aplicación | Script |
|--------|---------------------|--------|
| **Giroscopio** | Lectura del giroscopio del dispositivo; mostrada en interfaz de usuario para mejor orientación en el laberinto | [`gyro`](Assets/Scripts/NPCs_Scripts/Movement/gyro.cs) |

---

## 6. GIF de ejecución

> *(Pendiente de añadir — insertar GIF animado de la ejecución aquí)*

```
![Demo Tunnel Horror](docs/demo.gif)
```

---

## 7. Acta de acuerdos del grupo

### Integrantes

| Integrante | Área principal |
|-----------|---------------|
| **Marco Pérez Padilla** | NPCs, triggers, IA conversacional, VR input, linterna |
| **Álvaro Pérez Ramos** | Generación procedural del laberinto, vagoneta, spawn de enemigos |
| **Ezequiel Juan Canale Oliva** | Entorno visual 3D, piezas del laberinto, iluminación, sensor de temperatura |

### Reparto de tareas acordado

Las siguientes tareas fueron acordadas en común antes del inicio del desarrollo:

**Marco Pérez Padilla**
1. NPC tutorial con reconocimiento de voz (Whisper) y respuesta LLM (Ollama).
2. NPCs del túnel con comportamientos variados activados por triggers.
3. Persecuciones y audios de NPCs vinculados a zonas trigger.

**Álvaro Pérez Ramos**
1. Desplazamiento de la vagoneta sobre un recorrido generado proceduralmente.
2. Sistema de control de la vagoneta (impulso, freno, bifurcaciones).
3. Sistema de temblor de cámara periódico o basado en tiempo.
4. *(Alternativa acordada)* Movimiento físico por impulso sobre la barandilla.

**Ezequiel Juan Canale Oliva**
1. Generación procedural del túnel con optimización de distancia de visión.
2. Linterna dinámica asignable a mano o cabeza según dispositivo.
3. Integración del sensor de temperatura ambiental con UI.
4. Bloques de construcción y elementos del escenario.
5. Sistema de iluminación del entorno junto a linterna.

### Tareas desarrolladas individualmente

**Marco Pérez Padilla**

- [`SavWavMemory.cs`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/SavWavMemory.cs): conversión de `AudioClip` a WAV.
- [`WhisperUI.cs`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/WhisperUI.cs): grabación y transcripción de voz.
- [`OllamaUI.cs`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/OllamaUI.cs): integración con LLM y efecto máquina de escribir.
- [`FloatingText3D.cs`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/FloatingText3D.cs): texto 3D flotante orientado a cámara.
- [`CarretillaUI.cs`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/CarretillaUI.cs): menú de controles sobre la carretilla.
- [`TriggerNotificator.cs`](Assets/Scripts/NPCs_Scripts/TriggerZones/TriggerNotificator.cs): notificador genérico de zonas trigger.
- [`NPCAudioEvent.cs`](Assets/Scripts/NPCs_Scripts/TriggerZones/NPCAudioEvent.cs): audio reactivo a trigger.
- [`NPCChasingEvent.cs`](Assets/Scripts/NPCs_Scripts/TriggerZones/NPCChasingEvent.cs): persecución directa activada por trigger.
- [`VisualChasing.cs`](Assets/Scripts/NPCs_Scripts/TriggerZones/VisualChasing.cs): persecución con mecánica Weeping Angel.
- [`PlayerEnemyCollisionEvent.cs`](Assets/Scripts/NPCs_Scripts/TriggerZones/PlayerEnemyCollisionEvent.cs): colisión jugador–enemigo con Game Over.
- [`NoiseDetector.cs`](Assets/Scripts/NPCs_Scripts/NoiseEvents/NoiseDetector.cs): detección de ruido con micrófono real.
- [`NPCNoiseSpawner.cs`](Assets/Scripts/NPCs_Scripts/NoiseEvents/NPCNoiseSpawner.cs): spawn de NPCs reactivo a ruido alto.
- [`FlickeringLightOnNoiseEvent.cs`](Assets/Scripts/NPCs_Scripts/NoiseEvents/FlickeringLightOnNoiseEvent.cs): parpadeo de luz reactivo a ruido.
- [`FlashlightVRController.cs`](Assets/Flashlight_Scripts/FlashlightVRController.cs): linterna VR con cambio de mano dinámico.
- [`FlashlightControls.inputactions`](Assets/Flashlight_InputActions/FlashlightControls.inputactions): mapeado multi-dispositivo de la linterna.
- [`PlayerEnemyCollisionEvent`](Assets/Scripts/NPCs_Scripts/TriggerZones/PlayerEnemyCollisionEvent.cs): reinicio de escena cuando se colisiona con un enemigo durante 3 segundos.
- [`PlaceTriggerSetup`](Assets/Scripts/NPCs_Scripts/PlaceTriggerSetup.cs): Asignación de los notificadores de eventos a las piezas del túnel.
- [`gyro`](Assets/Scripts/NPCs_Scripts/Movement/gyro.cs): Uso de giroscopio para reconocer los grados de orientación respecto al Norte del juego.
- [`CartControl.inputactions`](Assets/CartControl.inputactions): mapeado multi-dispositivo de la vagoneta (expansiónde la versión inicial para ordenador).
- [`CartVRFeedback.cs`](Assets/Scripts/GamePlay/CartVRFeedback.cs): encargado de enviar impulsos hápticos en respuesta a eventos.
- [`XRFollowCart.cs`](Assets/Scripts/GamePlay/XRFollowCart.cs): seguimiento del jugador a la vagoneta. Compatible con cámara VR.
- Integración de XR Origin y mapeado VR de todas las acciones existentes.

Scripts desarrollados para debug:

- [`FollowTarget`](Assets/Scripts/NPCs_Scripts/Movement/FollowTarget.cs): Seguimiento de un objeto a otro manteniendo una posición relativa y la misma orientación.
- [`MoveCamera`](Assets/Scripts/NPCs_Scripts/Movement/MoveCamera.cs): Controla la rotación de la cámara en primera persona usando el ratón.
- [`MovementWithRigidbody`](Assets/Scripts/NPCs_Scripts/Movement/MovementWithRigidbody.cs): Controla el movimiento básico del jugador usando Rigidbody y entrada WASD mediante el nuevo Input System.
- [`TriggerZoneVisualizer`](Assets/Scripts/NPCs_Scripts/TriggerZones/TriggerZoneVisualizer.cs): Dibuja en el editor una representación visual del collider trigger asociado al TriggerNotificator del mismo GameObject

**Álvaro Pérez Ramos**

- [`PathGenerator.cs`](Assets/Scripts/PathSystem/Generation/PathGenerator.cs): orquestador del pipeline de generación.
- [`PathGraph.cs`](Assets/Scripts/PathSystem/Core/PathGraph.cs): grafo navegable, fuente única de verdad.
- [`PathNode.cs`](Assets/Scripts/PathSystem/Core/PathNode.cs): nodo lógico con flags de gameplay.
- [`PathType.cs`](Assets/Scripts/PathSystem/Core/PathType.cs): enumeración de tipo de camino.
- [`Grid2D.cs`](Assets/Scripts/PathSystem/Generation/Grid2D.cs): espacio base de generación topológica.
- [`MainPathGenerator.cs`](Assets/Scripts/PathSystem/Generation/MainPathGenerator.cs): DFS con backtracking, sin ciclos.
- [`SubPathGenerator.cs`](Assets/Scripts/PathSystem/Generation/SubPathGenerator.cs): subrutas con reglas de fusión.
- [`SubPathCooldownResolver.cs`](Assets/Scripts/PathSystem/Generation/SubPathCooldownResolver.cs): reglas de cooldown y nodos primordiales.
- [`HeightModulator.cs`](Assets/Scripts/PathSystem/Generation/HeightModulator.cs): altura independiente de la topología.
- [`SlopeLimiter.cs`](Assets/Scripts/PathSystem/Generation/SlopeLimiter.cs): relajación iterativa de pendientes (|ΔY| ≤ spacing).
- [`DecisionResolver.cs`](Assets/Scripts/PathSystem/Generation/DecisionResolver.cs): detección de bifurcaciones jugables reales.
- [`PathGizmosDrawer.cs`](Assets/Scripts/PathSystem/Unity/PathGizmosDrawer.cs): visualización de depuración en editor.
- [`CartMovement.cs`](Assets/Scripts/GamePlay/CartMovement.cs): movimiento automático sobre el grafo con impulso y freno.
- [`CartDecisionController.cs`](Assets/Scripts/GamePlay/CartDecisionController.cs): decisiones en bifurcaciones relativas a la cámara.
- [`CartMount.cs`](Assets/Scripts/GamePlay/CartMount.cs): montaje/desmontaje del jugador en la vagoneta.
- [`CameraShake.cs`](Assets/Scripts/GamePlay/CameraShake.cs): vibración Perlin proporcional a velocidad.
- [`EnemySpawner.cs`](Assets/Scripts/NPCs_Scripts/EnemySpawner.cs): spawn central de enemigos suscrito a triggers y ruido.
- [`EnemyInitializer.cs`](Assets/Scripts/NPCs_Scripts/EnemyInitializer.cs): inicialización automática de NPCs enemigos instanciados.
- [`GameManager.cs`](Assets/Scripts/NPCs_Scripts/GameManager.cs): Singleton de estado de partida (Game Over / Victoria).
- [`PathSurfaceBuilder.cs`](Assets/Scripts/PathSurfaceBuilder/PathSurfaceBuilder.cs): construcción de vías en modo Tile sobre el grafo.

**Ezequiel Juan Canale Oliva**

- [`LegacyPieceApplier.cs`](Assets/PieceApplier/LegacyPieceApplier.cs): construcción visual del laberinto a partir del grafo.
- [`Piece.cs`](Assets/Pieces/Piece.cs): definición de pieza instanciable y su lógica de colocación.
- Diseño y construcción de todos los prefabs de piezas del laberinto
  (`straightCave`, `TurnL`, `TurnR`, `forkLeftRight`, `forkLeftStraight`,
  `forkRightStraight`, `forkTripleCave`, `corridorUpCave`, `corridorDownCave`,
  `endCave` y sus variantes `VariantChase`, `VariantNoiseDetector`, `VariantNoisePlayer`).
- Sistema de iluminación del entorno.
- Integración del sensor de temperatura con UI.
- [`PathPieceRegistry.asset`](Assets/Pieces/PathPieceRegistry.asset): registro de asociación tipo-pieza.

### Tareas desarrolladas en grupo

- Integración del sistema de laberinto con los prefabs visuales y los triggers de NPCs.
- Coordinación entre `PathSurfaceBuilder` / `LegacyPieceApplier` / `PlaceTriggerSetup` y los prefabs variantes
  para asegurar que los triggers se colocan en las piezas correctas.
- Pruebas de integración en editor y en dispositivo VR.
- Redacción y revisión del acta del proyecto.

---

## 8. Check-list de diseño de aplicaciones de RV

| Recomendación | Estado | Observaciones |
|--------------|--------|---------------|
| Evitar movimiento artificial del jugador | ✅ Se contempla | El jugador viaja en vagoneta; no se desplaza a pie |
| Proporcionar punto de referencia visual estable | ✅ Se contempla | La cabina de la vagoneta actúa como marco de referencia, así como el giroscopio |
| Mantener frecuencia de fotogramas estable (≥ 72 fps) | ✅ Se contempla | Optimización mediante reducción de distancia de visión (*Pendiente*) |
| Evitar efectos de aceleración brusca | ⚠️ Parcialmente | El impulso de la vagoneta tiene cierta suavidad; pendiente de refinar |
| Escala del entorno acorde a la del usuario | ✅ Se contempla | Prefabs de piezas ajustados a escala humana |
| Retroalimentación auditiva de acciones | ✅ Se contempla | Audio del motor de la vagoneta y eventos de sonido de NPCs|
| Retroalimentación háptica | ✅ Se contempla | Se implementa vibración en controles VR al acelerar/frenar |
| Evitar texto pequeño difícil de leer en VR | ✅ Se contempla | `FloatingText3D` posiciona el texto frente al jugador |
| Indicadores de dirección claros en navegación | ✅ Se contempla | Las bifurcaciones requieren decisión explícita del jugador |
| Interfaz de usuario anclada al mundo (no a la pantalla) | ✅ Se contempla | Canvas del tutorial anclado al NPC; texto 3D flotante |
| Zona de seguridad para el jugador | ❌ No aplica | No es necesario que el jugador se mueva de su sitio al jugar |
| Permitir ajustar la altura del punto de vista | ❌ No aplica | El punto de vista queda fijado al asiento de la vagoneta |
| Opción para reducir efectos de movimiento | ❌ No se contempla | `CameraShake` no tiene opción de accesibilidad en runtime |
| Señales visuales para eventos importantes | ✅ Se contempla | Parpadeo de luces ante detección de ruido |
| Tutorial o introducción al control | ✅ Se contempla | NPC tutorial con IA interactiva al inicio de la experiencia |

---

## 9. Referencia de scripts por integrante

### Marco Pérez Padilla — NPCs, Triggers, IA y VR Input

#### `Assets/Scripts/NPCs_Scripts/Tutorial_IA/`

| Script | Descripción | Objeto en escena |
|--------|-------------|-----------------|
| [`SavWavMemory.cs`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/SavWavMemory.cs) | Convierte un `AudioClip` grabado en memoria a formato WAV en disco para enviarlo a Whisper. Utilizado internamente por `WhisperUI`. | No aplica (Uso interno en `WhisperUI.cs`) |
| [`WhisperUI.cs`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/WhisperUI.cs) | Graba audio del micrófono (tecla `V` o botón UI), lo convierte con `SavWavMemory` y lo envía al endpoint de Whisper. Dispara `OnWhisperTranscription` con el texto resultante u `OnWhisperError` si falla. | WhisperUI |
| [`OllamaUI.cs`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/OllamaUI.cs) | Se suscribe a `OnWhisperTranscription`, construye un prompt de contexto con la transcripción y consulta Ollama. Muestra la respuesta carácter a carácter en un `TextMeshPro` con efecto máquina de escribir. Dispara un evento adicional si detecta la palabra clave `"palanca"`. | NPC Tutorial |
| [`FloatingText3D.cs`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/FloatingText3D.cs) | Texto 3D que sigue la posición del NPC y se orienta automáticamente hacia la cámara del jugador en cada frame. | OllamaText (hijo del NPC del tutorial) |
| [`CarretillaUI.cs`](Assets/Scripts/NPCs_Scripts/Tutorial_IA/CarretillaUI.cs) | Muestra el panel de controles cuando el jugador entra en contacto con la carretilla. Se oculta con `H`, Gamepad Button East,  o secondary button VR (mano derecha). | seatPoint (hijo de Vagoneta) |

#### `Assets/Scripts/NPCs_Scripts/TriggerZones/`

| Script | Descripción | Objeto en escena |
|--------|-------------|-----------------|
| [`TriggerNotificator.cs`](Assets/Scripts/NPCs_Scripts/TriggerZones/TriggerNotificator.cs) | Componente base que expone `OnPlayerEntered` y `OnPlayerExited` cuando un objeto con tag `Player` entra o sale del `BoxCollider` configurado como trigger. Todos los scripts de comportamiento se suscriben a este componente para desacoplarse de la detección. | Zonas trigger |
| [`NPCAudioEvent.cs`](Assets/Scripts/NPCs_Scripts/TriggerZones/NPCAudioEvent.cs) | Se suscribe a `OnPlayerEntered` de un `TriggerNotificator` y reproduce un `AudioClip` asignado. El audio se reproduce una única vez por activación. Se le asigna a los prefabs de NPCs | NPCs |
| [`NPCChasingEvent.cs`](Assets/Scripts/NPCs_Scripts/TriggerZones/NPCChasingEvent.cs) | Persecución directa: al entrar en la zona trigger, el NPC persigue al jugador adaptando su velocidad. Si `chaseDuration < 1 s`, la persecución se detiene al salir. Si `chaseDuration ≥ 1 s`, continúa hasta agotar el temporizador. Velocidad mínima garantizada por `speedThreshold`. | NPCs |
| [`VisualChasing.cs`](Assets/Scripts/NPCs_Scripts/TriggerZones/VisualChasing.cs) | Persecución con mecánica Weeping Angel: el NPC avanza mientras el jugador no lo mira. La detección de mirada usa el ángulo entre la cámara y la dirección al NPC, configurable mediante `viewAngle`. La suscripción al trigger se hace en `Start()` para respetar el orden de inicialización con `EnemyInitializer`. | NPCs |
| [`PlayerEnemyCollisionEvent.cs`](Assets/Scripts/NPCs_Scripts/TriggerZones/PlayerEnemyCollisionEvent.cs) | Detecta colisión física del jugador con objetos de tag `Enemy` y llama a `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex)`. | Enemigos |
| [`TriggerZoneVisualizer.cs`](Assets/Scripts/NPCs_Scripts/TriggerZones/TriggerZoneVisualizer.cs) | Herramienta de depuración: dibuja visualmente los límites de una zona trigger en el editor de Unity. Sin efecto en runtime. | Zonas trigger (editor) |

#### `Assets/Scripts/NPCs_Scripts/NoiseEvents/`

| Script | Descripción | Objeto en escena |
|--------|-------------|-----------------|
| [`NoiseDetector.cs`](Assets/Scripts/NPCs_Scripts/NoiseEvents/NoiseDetector.cs) | Inicia la grabación del micrófono al entrar el jugador en la zona trigger. Analiza el nivel RMS del audio capturado en ventanas de `windowSize` muestras. Dispara `OnNoiseDetected` si supera `threshold` y `OnHighNoiseDetected` si supera `highNoiseThreshold`. La grabación se detiene al salir de la zona. | Zonas trigger |
| [`NPCNoiseSpawner.cs`](Assets/Scripts/NPCs_Scripts/NoiseEvents/NPCNoiseSpawner.cs) | Se suscribe a `OnHighNoiseDetected` de todos los `NoiseDetector` en escena. Al detectar ruido alto (respetando `spawnCooldown`), instancia parejas de NPCs en un radio alrededor del jugador excluyendo un cono frontal de la vagoneta (`coneAngle`) y un radio interno de exclusión (`innerExclusionRadius`). Los NPCs se instancian con `VisualChasing` y persecución de 20 segundos. | Objeto global en escena |
| [`FlickeringLightOnNoiseEvent.cs`](Assets/Scripts/NPCs_Scripts/NoiseEvents/FlickeringLightOnNoiseEvent.cs) | Se suscribe a `OnNoiseDetected` de un `NoiseDetector`. Si el ruido ocurre dentro de `reactionRadius`, hace parpadear la luz durante `flickerDuration` segundos usando ruido Perlin con velocidad `flickerSpeed`. La intensidad varía entre `0` y `maxIntensity`. | Luz de la linterna |

#### `Assets/Scripts/NPCs_Scripts/Movement/`

| Script | Descripción |
|--------|-------------|
| [`MovementWithRigidbody.cs`](Assets/Scripts/NPCs_Scripts/Movement/MovementWithRigidbody.cs) | Herramienta de depuración: Mueve un objeto físico con WASD. Solo para pruebas en editor. |
| [`MoveCamera.cs`](Assets/Scripts/NPCs_Scripts/Movement/MoveCamera.cs) | Herramienta de depuración: Mueve la cámara con el ratón. Solo para pruebas en editor. |
| [`FollowTarget.cs`](Assets/Scripts/NPCs_Scripts/Movement/FollowTarget.cs) | Seguimiento de un objeto a otro manteniendo una posición relativa y la misma orientación. |
| [`gyro.cs`](Assets/Scripts/NPCs_Scripts/Movement/gyro.cs) | Reconociemto de los grados de orientación respecto al Norte del juego mediante giroscopio.

#### `Assets/Scripts/NPCs_Scripts/`

| Script | Descripción |
|--------|-------------|
| [`PlaceTriggerSetup.cs`](Assets/Scripts/NPCs_Scripts/PlaceTriggerSetup.cs) | Asignación de notificadores a cada pieza generada |

#### `Assets/Scripts/Gameplay/`

| Script | Descripción | Objeto en escena |
|--------|-------------|-----------------|
| [`XRFollowCart.cs`](Assets/Scripts/Gameplay/XRFollowCart.cs) | Lógica que hace que el jugador siga a la vagoneta cuando éste se encuentra montado en la misma. Da la sensación de estar sentado en la misma, y es compatible con VR | XRFollowConroller |
| [`CartVRFeedback.cs`](Assets/Scripts/Gameplay/CartVRFeedback.cs) | Envía impulsos hápticos a los mandos cuando se disparan los eventos de aceleración y frenado de la vagoneta | Vagoneta |

#### `Assets/Flashlight_Scripts/` y `Assets/Flashlight_InputActions/`

| Archivo | Descripción |
|---------|-------------|
| [`FlashlightVRController.cs`](Assets/Flashlight_Scripts/FlashlightVRController.cs) | Controla el encendido/apagado de la linterna y el cambio de mano. Detecta dinámicamente el modelo de mano en uso. Soporta teclado, gamepad y VR mediante el New Input System. |
| [`FlashlightControls.inputactions`](Assets/Flashlight_InputActions/FlashlightControls.inputactions) | Mapeado de acciones de la linterna para teclado, gamepad y controles VR. |

#### `Assets/`

| Archivo | Descripción |
|---------|-------------|
| [`CartControl.inputactions`](Assets/CartControl.inputactions) | Mapeado de acciones de la vagoneta (impulso, freno, decisiones) para teclado, gamepad y VR. |

#### Scripts adaptados tras versión inicial

|Carpeta | Archivo | Modificación |
|--------|---------|-------------|
| `Assets/Scripts/GamePlay/` | [`CartMovement.cs`](Assets/Scripts/GamePlay/CartMovement.cs) | Modificación para permitir acciones por inputs que no sean únicamente por teclado. |
| `Assets/Scripts/GamePlay/` | [`CartDecisionController.cs`](Assets/Scripts/GamePlay/CartDecisionController.cs) | Modificación para permitir acciones por inputs que no sean únicamente por teclado. |
| `Assets/Scripts/GamePlay/` | [`CartMount.cs`](Assets/Scripts/GamePlay/CartMount.cs) | Modificación para permitir acciones por inputs que no sean únicamente por teclado. |
| `Assets/Scripts/NPCs_Scripts/` | [`EnemyInitializer.cs`](Assets/Scripts/NPCs_Scripts/EnemyInitializer.cs) | Modificación para añadir NPC Audio Event y collider para que los enemigos choquen con el jugador (Adición también de PlayerEnemyCollisionEvent) |

---

### Álvaro Pérez Ramos — Sistema de Laberinto, Vagoneta y Spawn de Enemigos

#### `Assets/Scripts/PathSystem/Core/`

| Script | Descripción |
|--------|-------------|
| [`PathGraph.cs`](Assets/Scripts/PathSystem/Core/PathGraph.cs) | Contenedor global del grafo navegable. Garantiza que no existan dos nodos en la misma posición X/Y/Z. Expone la lista del camino principal y las subrutas generadas. Es la **fuente única de verdad** para navegación y decisiones. |
| [`PathNode.cs`](Assets/Scripts/PathSystem/Core/PathNode.cs) | Nodo lógico del grafo. Su identidad es su posición mundial exacta. Almacena conexiones navegables, tipo de camino (`PathType`) y flags de gameplay: `isPrimordial`, `isDP`, `isPi`, `canStartSubPath`, `canReceiveSubPath`, `isDecisionNode`. |
| [`PathType.cs`](Assets/Scripts/PathSystem/Core/PathType.cs) | Enumeración que distingue nodos de camino principal (`Main`) de nodos de subruta (`Sub`). |

#### `Assets/Scripts/PathSystem/Generation/`

| Script | Descripción |
|--------|-------------|
| [`PathGenerator.cs`](Assets/Scripts/PathSystem/Generation/PathGenerator.cs) | Orquestador principal del pipeline de generación. Ejecuta en orden garantizado: reset del grafo → `Grid2D` → `MainPathGenerator` → altura del camino principal → reglas previas → `SubPathGenerator` → altura de subrutas → `SlopeLimiter` → `DecisionResolver`. La generación ocurre **una única vez por partida**. |
| [`Grid2D.cs`](Assets/Scripts/PathSystem/Generation/Grid2D.cs) | Define el espacio base de generación como un grid estrictamente 2D (X/Z). Calcula vecinos ortogonales. La altura no participa en la conectividad. |
| [`MainPathGenerator.cs`](Assets/Scripts/PathSystem/Generation/MainPathGenerator.cs) | Genera el camino principal mediante DFS con backtracking. Garantiza ausencia de ciclos internos y progreso lógico desde P0 hasta PN. El camino principal nunca se modifica tras su generación. |
| [`SubPathGenerator.cs`](Assets/Scripts/PathSystem/Generation/SubPathGenerator.cs) | Genera subrutas alternativas que salen exactamente desde un nodo `Pi` y reingresan en un nodo del camino principal. Longitud mínima obligatoria. Sin ramificaciones internas. Las subrutas pierden su identidad independiente tras la fusión. |
| [`SubPathCooldownResolver.cs`](Assets/Scripts/PathSystem/Generation/SubPathCooldownResolver.cs) | Aplica reglas estructurales de gameplay: nodos primordiales iniciales y cooldown tras generación de subruta. El cooldown bloquea salidas pero nunca entradas, habilitando bucles, atajos y reentradas emergentes. |
| [`HeightModulator.cs`](Assets/Scripts/PathSystem/Generation/HeightModulator.cs) | Aplica altura a los nodos sin alterar la topología del grafo. La altura pertenece al tramo, no al nodo. Las subrutas heredan la altura del `Pi` del que parten. La fusión con el camino principal fuerza coincidencia exacta en Y. |
| [`SlopeLimiter.cs`](Assets/Scripts/PathSystem/Generation/SlopeLimiter.cs) | Garantiza jugabilidad física aplicando relajación iterativa para que ningún tramo tenga `|ΔY| > spacing` (pendiente máxima de 45°). |
| [`DecisionResolver.cs`](Assets/Scripts/PathSystem/Generation/DecisionResolver.cs) | Determina qué nodos son bifurcaciones jugables reales. Un nodo es de decisión si tiene más de una salida válida, no es primordial, no es DP y no está bloqueado por cooldown. Se ejecuta al final del pipeline. |

#### `Assets/Scripts/PathSystem/Unity/`

| Script | Descripción |
|--------|-------------|
| [`PathGenerator.cs`](Assets/Scripts/PathSystem/Unity/PathGenerator.cs) | Componente Unity que dispara el pipeline de generación desde `Start()` y expone el `PathGraph` resultante al resto de sistemas. |
| [`PathGizmosDrawer.cs`](Assets/Scripts/PathSystem/Unity/PathGizmosDrawer.cs) | Visualización de depuración en el editor. Dibuja trayectos con offset lateral visual para distinguir subrutas. Colores estables por subruta. Sin efecto en runtime. |

#### `Assets/Scripts/GamePlay/`

| Script | Descripción | Objeto en escena |
|--------|-------------|-----------------|
| [`CartMovement.cs`](Assets/Scripts/GamePlay/CartMovement.cs) | Avanza la vagoneta automáticamente entre nodos del grafo. En nodos con una sola salida continúa sin intervención. Gestiona impulso (`V`), freno bloqueante (`X`), audio del motor y `CameraShake` proporcional a la velocidad. | Vagoneta |
| [`CartDecisionController.cs`](Assets/Scripts/GamePlay/CartDecisionController.cs) | Gestiona la decisión del jugador en bifurcaciones reales. La entrada es relativa a la orientación de la cámara. Si la dirección elegida no existe, la vagoneta no se mueve. No reasigna salidas automáticamente. | Vagoneta |
| [`CartMount.cs`](Assets/Scripts/GamePlay/CartMount.cs) | Gestiona el montaje y desmontaje del jugador en la vagoneta. Al montar, posiciona la cámara sobre el `SeatPoint` con un offset vertical configurable. Activa y desactiva `CartMovement` y sincroniza efectos visuales. | Vagoneta |
| [`CameraShake.cs`](Assets/Scripts/GamePlay/CameraShake.cs) | Aplica vibración a la cámara usando ruido Perlin bidimensional. El desplazamiento en Y es siempre positivo para evitar que la cámara baje por debajo del borde de la vagoneta. `dynamicFactor` controla la intensidad proporcional a la velocidad actual. | Cámara de la vagoneta (No usado para versión VR por problemas de compatibilidad) |

#### `Assets/Scripts/NPCs_Scripts/`

| Script | Descripción | Objeto en escena |
|--------|-------------|-----------------|
| [`EnemySpawner.cs`](Assets/Scripts/NPCs_Scripts/EnemySpawner.cs) | Spawner central de enemigos. Se suscribe en `Start()` a todos los `TriggerNotificator` y `NoiseDetector` en escena. Ante entrada en trigger, instancia enemigos cerca de la zona. Ante ruido alto, instancia enemigos en radio alrededor del jugador con cono de exclusión frontal de la vagoneta y radio interno de exclusión. Respeta cooldown entre spawns por ruido. | Objeto global en escena |
| [`EnemyInitializer.cs`](Assets/Scripts/NPCs_Scripts/EnemyInitializer.cs) | Inicializa automáticamente un prefab de NPC enemigo al instanciarlo: asigna tag `Enemy`, añade `SphereCollider` de contacto, asegura `Rigidbody`, busca el `TriggerNotificator` más cercano (o usa el inyectado por `EnemySpawner` para evitar búsqueda redundante) y asigna aleatoriamente un comportamiento de IA (`NPCChasingEvent` o `VisualChasing`). | Prefab de enemigo |
| [`GameManager.cs`](Assets/Scripts/NPCs_Scripts/GameManager.cs) | Singleton que controla el estado global de la partida. Pausa el tiempo (`Time.timeScale = 0`) y muestra el panel de derrota o victoria. Expone `GameOver()` y `Victory()` para ser llamados por otros sistemas. | GameManager (escena) |

#### `Assets/Scripts/PathSurfaceBuilder/`

| Script | Descripción | Objeto en escena |
|--------|-------------|-----------------|
| [`PathSurfaceBuilder.cs`](Assets/Scripts/PathSurfaceBuilder/PathSurfaceBuilder.cs) | Construye las vías de la vagoneta instanciando piezas de rail en modo Tile (sin escalar el prefab) a lo largo de cada corredor entre nodos. Respeta un margen (`nodeMargin`) en cada extremo para las piezas de nodo. Se reconstruye automáticamente al regenerarse el grafo mediante `PathGenerator.OnGraphRegenerated`. | PathSurfaceBuilder |

---

### Ezequiel Juan Canale Oliva — Entorno Visual, Piezas y Sistema de Iluminación

#### `Assets/PieceApplier/`

| Script | Descripción | Objeto en escena |
|--------|-------------|-----------------|
| [`LegacyPieceApplier.cs`](Assets/PieceApplier/LegacyPieceApplier.cs) | Recorre el grafo generado por `PathGenerator` y coloca prefabs 3D en cada nodo. Selecciona la pieza correcta según la topología local (recta, giro izquierda/derecha, bifurcación doble/triple, final) y la instancia con orientación y escala adecuadas. Coloca pasillos entre nodos (planos, ascendentes o descendentes según diferencia de altura). Controla duplicados mediante lista de nodos ya procesados. | LegacyPieceApplier |

#### `Assets/Pieces/`

| Elemento | Descripción |
|----------|-------------|
| [`Piece.cs`](Assets/Pieces/Piece.cs) | Clase de datos para una pieza del laberinto. Define el prefab a instanciar y la lógica de posicionamiento y orientación. |
| [`PathPieceRegistry.asset`](Assets/Pieces/PathPieceRegistry.asset) | ScriptableObject que asocia cada tipo de pieza con su prefab, permitiendo que `LegacyPieceApplier` lo consulte sin referencias directas en código. |
| `straightCave.prefab` + variantes | Pieza recta del túnel. Variantes: `VariantChase` (trigger de persecución), `VariantNoiseDetector` (detector de ruido), `VariantNoisePlayer` (reproductor de audio). |
| `TurnL.prefab` / `TurnR.prefab` | Giro a la izquierda y a la derecha. |
| `forkLeftRight.prefab` + variantes | Bifurcación izquierda-derecha con variantes de trigger. |
| `forkLeftStraight.prefab` + variantes | Bifurcación izquierda-recto con variantes de trigger. |
| `forkRightStraight.prefab` + variantes | Bifurcación derecha-recto con variantes de trigger. |
| `forkTripleCave.prefab` + variantes | Bifurcación triple con variantes de trigger. |
| `corridorUpCave.prefab` / `corridorDownCave.prefab` + variantes | Pasillo ascendente y descendente entre nodos de diferente altura. |
| `endCave.prefab` + variantes | Pieza de cierre del camino (nodo final). |

---