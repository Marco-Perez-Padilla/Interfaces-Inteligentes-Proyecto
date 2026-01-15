# Proyecto Unity: Tunnel Horror / NPC Interaction 
Cuestiones importantes para el uso: Se recomienda el uso con gafas de Realidad Virtual, así como disponer de un espacio amplio para mover el brazo de forma segura. 
Hitos de programación: Se han implementado correctamente colisiones (con colliders marcados como `isTrigger`, así como la existencia de una colisión física entre el jugador y un NPC de tag `enemy` físico), así como también todos los comportamientos que se exponen sobre NPCs se manejan con eventos y notificadores. En el tutorial, se aplican llamadas a las API de Whisper y Ollama, permitiendo el reconocimiento del habla y una respuesta del mundo al mismo. Además, se reproducen audios, y se capta el micrófono del jugador tanto en el tutorial como durante distintos eventos.
Aspectos a destacar: Generación procedural del camino del túnel/laberinto, y construcción dinámica del mapa alrededor de dicho camino.
Sensores: -
GIF:
Checklist:

A continuación, se desglosa detalladamente y por integrante del grupo los puntos a desarrollar acordados en común, así como el desarrollo final que ha hecho cada integrante. 

--- 

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

---

# Sistema de Laberinto Procedural y Movimiento de Vagoneta

Encargado: Álvaro Pérez Ramos

## Puntos de desarrollo acordados
1. Implementación del desplazamiento de la vagoneta siguiendo un recorrido definido a partir de una nube de puntos generados de forma aleatoria.  
2. Desarrollo de un sistema de control de la vagoneta mediante botones o una palanca, permitiendo iniciar y detener el movimiento a través de eventos en C#.  
3. Alternativamente, implementación de un sistema de desplazamiento basado en interacciones físicas, donde el avance de la vagoneta se produzca aplicando fuerza al impulsarse sobre la barandilla con la mano.  
4. Implementación de un sistema de temblor de cámara, que podrá gestionarse de forma periódica mediante un temporizador de frames o utilizando el tiempo actual en segundos (por ejemplo, mediante comparaciones con `Time.time`) para aplicar rotaciones sutiles a la cámara.
   
## Descripción General

Este proyecto implementa un **sistema procedural de generación de laberintos dirigidos** y un **sistema de movimiento automático de una vagoneta** que recorre dicho laberinto. El diseño separa de forma estricta:

* Generación topológica
* Aplicación de altura (visual/jugable)
* Reglas de gameplay
* Movimiento y toma de decisiones
* Visualización y depuración

La generación se ejecuta **una única vez por partida** a partir de una semilla fija. El recorrido del jugador consume el grafo generado sin modificarlo.

---

## Arquitectura General

El sistema se divide en dos grandes subsistemas:

1. **Sistema de Generación del Laberinto**
2. **Sistema de Movimiento de la Vagoneta**

Ambos comparten una representación común del mundo mediante un **grafo navegable de nodos únicos por posición X/Y/Z**.

---

# 1. Sistema de Generación del Laberinto

## 1.1 PathGenerator

**Rol:** Orquestador principal del sistema procedural.

**Responsabilidades:**

* Inicializar el grid 2D
* Generar el camino principal (DFS sin ciclos)
* Aplicar reglas de nodos primordiales y cooldown
* Generar subrutas
* Aplicar altura a caminos
* Limitar pendientes máximas
* Resolver nodos de decisión

**Orden de ejecución garantizado:**

1. Reset del grafo
2. Creación del Grid2D
3. Generación del Main Path
4. Aplicación de altura al Main Path
5. Resolución de reglas de gameplay previas
6. Generación topológica de subrutas
7. Aplicación de altura a subrutas
8. Limitador de pendientes
9. Resolución final de decisiones

---

## 1.2 PathGraph

**Rol:** Contenedor global del grafo navegable.

**Características clave:**

* Un único `PathNode` por posición exacta X/Y/Z
* Registro global de nodos
* Lista explícita de camino principal
* Lista de subrutas generadas

El grafo es la fuente única de verdad para navegación y decisiones.

---

## 1.3 PathNode

**Rol:** Nodo lógico del grafo.

**Principios fundamentales:**

* La identidad del nodo está definida únicamente por su posición
* No existen nodos duplicados en el espacio

**Atributos relevantes:**

* `position`: posición mundial exacta
* `connections`: conexiones navegables
* `pathType`: Main o Sub
* Flags de gameplay:

  * `isPrimordial`
  * `isDP`
  * `isPi`
  * `canStartSubPath`
  * `canReceiveSubPath`
  * `isDecisionNode`

---

## 1.4 Grid2D

**Rol:** Espacio base de generación topológica.

**Reglas:**

* Grid estrictamente 2D (X/Z)
* Vecinos ortogonales
* La altura no participa en la conectividad

El grid define todas las posibles posiciones válidas del laberinto.

---

## 1.5 MainPathGenerator

**Rol:** Generador del camino principal.

**Características:**

* DFS con backtracking
* Sin ciclos internos
* Progreso lógico garantizado
* Define inicio (P0) y final (PN)

El camino principal nunca se modifica tras su generación.

---

## 1.6 SubPathGenerator

**Rol:** Generación de subrutas alternativas.

**Reglas garantizadas:**

* Sale exactamente desde un Pi
* Reentra exactamente en un nodo del Main Path
* Longitud mínima obligatoria
* Sin ramificaciones internas
* Puede cruzarse y compartir tramos con otras subrutas

La subruta deja de existir como entidad independiente tras la fusión.

---

## 1.7 SubPathCooldownResolver

**Rol:** Aplicar reglas de gameplay estructural.

**Reglas implementadas:**

* Nodos primordiales iniciales
* Cooldown tras generación de subruta
* El cooldown bloquea SALIDAS pero nunca ENTRADAS

Esta lógica habilita bucles, atajos y reentradas emergentes.

---

## 1.8 HeightModulator

**Rol:** Aplicación de altura sin alterar topología.

**Principios:**

* La altura no crea ni elimina bifurcaciones
* La altura pertenece al tramo, no al nodo
* Subrutas heredan la altura del Pi
* La fusión fuerza coincidencia exacta en Y

La altura es jugable y visual, pero no estructural.

---

## 1.9 SlopeLimiter

**Rol:** Garantizar jugabilidad física.

**Regla:**

* |ΔY| ≤ spacing

Aplica relajación iterativa para asegurar pendientes máximas de 45°.

---

## 1.10 DecisionResolver

**Rol:** Determinar decisiones jugables reales.

**Un nodo es decisión si:**

* Tiene más de una salida válida
* No es primordial
* No es DP
* No está bloqueado por cooldown

Las decisiones se resuelven tras toda la generación.

---

## 1.11 PathGizmosDrawer

**Rol:** Visualización de depuración.

**Características:**

* Dibuja trayectos, no topología
* Offset lateral visual para subrutas
* Colores estables por subruta

No afecta en ningún caso a la lógica del sistema.

---

# 2. Sistema de Movimiento de la Vagoneta

## 2.1 CartMovement

**Rol:** Movimiento automático sobre el grafo.

**Comportamiento:**

* Avanza automáticamente entre nodos
* Mantiene referencia al nodo anterior
* Se detiene únicamente en bifurcaciones reales

**Regla clave:**

* Si solo existe una salida válida, continúa sin intervención del jugador

---

## 2.2 CartDecisionController

**Rol:** Gestión de decisiones del jugador.

**Características:**

* Entrada relativa a la cámara
* No reasigna salidas automáticamente
* Si la dirección no existe, no hay movimiento

La decisión es explícita y consciente.

---

## 2.3 CartMount

**Rol:** Entrada y salida del jugador en la vagoneta.

**Responsabilidades:**

* Gestión de cámara
* Activación/desactivación del movimiento
* Sincronización con efectos visuales

---

## 2.4 CameraShake

**Rol:** Feedback visual de movimiento.

**Características:**

* Ruido Perlin
* Intensidad dinámica
* Activable/desactivable

No influye en gameplay ni navegación.

---

# Principios de Diseño Globales

* Separación estricta entre generación y consumo
* Un solo grafo inmutable por partida
* Las decisiones emergen del diseño, no de errores
* La desorientación es controlada e intencionada
* La topología nunca depende de la altura

---

## Estado del Sistema

El sistema está **completo y funcional** para:

* Generación de laberintos dirigidos
* Exploración no lineal
* Decisiones jugables claras
* Movimiento autónomo controlado

Cualquier extensión futura debe respetar esta separación de responsabilidades.

---

## Encargado
**Ezequiel Juan Canale Oliva**

### Puntos de desarrollo acordados
1. Generación procedural del túnel, incluyendo la optimización mediante la reducción de la distancia de visión para mejorar el rendimiento.  
2. Implementación de una linterna dinámica, que podrá asignarse tanto a la mano del jugador como a la cabeza, según el dispositivo utilizado.  
3. Integración de un sensor de temperatura ambiental, mostrando la información en una interfaz de usuario cuando el dispositivo lo permita (por ejemplo, en dispositivos móviles compatibles). 
4. Desarrollo de bloques de construcción y elementos del escenario, coordinados con la parte correspondiente del trabajo de Álvaro, asegurando coherencia visual y funcional.  
5. Implementación y ajuste del sistema de iluminación del entorno, con especial énfasis en la linterna como principal fuente de luz interactiva.

