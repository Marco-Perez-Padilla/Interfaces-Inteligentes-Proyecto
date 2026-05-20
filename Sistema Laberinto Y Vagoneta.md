# Sistema de Laberinto Procedural y Movimiento de Vagoneta

Encargado: Álvaro Pérez Ramos

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
