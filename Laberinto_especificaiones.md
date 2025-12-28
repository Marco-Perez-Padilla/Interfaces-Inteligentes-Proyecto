# Especificación Interna del Sistema de Generación de Caminos y Subrutas

## Documento de restricciones internas y reglas de diseño

Este documento complementa la especificación general del sistema y está destinado al equipo de desarrollo.
Su objetivo es dejar explícitas las **restricciones internas**, **reglas no evidentes** y **casos especiales** que deben respetarse durante la implementación.

---

## 1. Alcance del sistema

El sistema genera un **grafo navegable procedural dirigido**, con:

- Un inicio definido (P0)
- Un final definido (PN)
- Múltiples rutas alternativas
- Posibilidad de bucles y retrocesos

El objetivo del jugador es alcanzar PN.  
La exploración, la desorientación y las consecuencias de las decisiones son parte intencionada del diseño.

La generación se ejecuta **una única vez por partida**, usando una semilla fija.

---

## 2. Espacio de generación

### 2.1 Grid base

- Toda la topología se genera sobre un **grid 2D (X/Z)**.
- No existe un grid 3D topológico real.
- Todas las conexiones se deciden en 2D.

### 2.2 Altura (Y)

- La altura se asigna posteriormente.
- No altera la conectividad del grafo.
- Afecta únicamente a:
  - Cruces
  - Decisiones
  - Percepción jugable

La altura es jugable y visual, pero **no estructural**.

---

## 3. Camino principal (Main Path)

### 3.1 Definición

El camino principal es una secuencia lineal de nodos:

P0 → P1 → P2 → … → PN

- No contiene ciclos internos.
- Define el progreso lógico hacia el final del juego.

### 3.2 Inmutabilidad

Una vez generado:

- El main path no se modifica.
- No se regenera.
- No depende de las decisiones del jugador.

---

## 4. DP – Distance / Safe Points

Un nodo del main path puede marcarse como DP.

Un DP:

- No puede generar subrutas.
- No puede cerrar subrutas.
- No ofrece decisiones.
- No permite retroceder más allá de él a nivel de gameplay.

Los DP existen para:

- Controlar la densidad de decisiones.
- Garantizar progreso.
- Evitar desorientación excesiva.

---

## 5. Subrutas – definición formal

Una subruta es un recorrido alternativo que:

- Sale del camino principal **una sola vez**.
- Se genera sobre el mismo grid 2D (X/Z).
- Puede subir y bajar libremente en altura (Y).
- Explora el espacio de forma no lineal.
- En algún punto vuelve a coincidir **exactamente en X, Y y Z** con el camino principal.
- Tras la coincidencia, queda absorbida por el main path.

Una subruta absorbida **deja de existir como entidad independiente**.

---

## 6. Condiciones para crear una subruta (Pi)

Un nodo del main path puede ser Pi si:

- No es DP.
- No está dentro del rango de cooldown.
- No está demasiado cerca de P0 o PN.
- No ha sido usado previamente como Pi.
- Es un nodo válido del main path.

---

## 7. Cooldown de subrutas (regla crítica)

Tras crear una subruta en un Pi:

- Durante los siguientes M nodos del main path:
  - No se pueden crear nuevas salidas de subruta.

### 7.1 Regla especial: paredes falsas por cooldown

Durante el cooldown:

- **Las salidas de subruta bloqueadas por cooldown se consideran paredes falsas**.
- Esto significa:

  - Si el jugador llega a ese nodo desde el main path:
    - La salida NO existe.
  - Si el jugador llega a ese nodo **desde la propia subruta**:
    - La salida SÍ existe.
    - Se comporta como una entrada válida.

Esta regla es obligatoria para permitir:
- Bucles
- Reentradas
- Atajos no evidentes

El cooldown **nunca invalida una entrada**, solo bloquea salidas desde el main path.

---

## 8. Generación interna de subrutas

### 8.1 Naturaleza

- DFS o Random Walk local.
- Sin ramificaciones internas.
- Sin sub-subrutas.
- Puede cruzarse y compartir tramos con otras subrutas.

### 8.2 Longitud mínima (ML)

- Una subruta no puede fusionarse antes de recorrer ML nodos.
- Antes de ML, cualquier coincidencia con el main path se ignora.

---

## 9. Fusión con el camino principal (absorción)

### 9.1 Condición de fusión

La fusión ocurre solo si:

- Coincidencia exacta en X, Y y Z con un nodo del main path.
- Se ha cumplido ML.

Cruces a distinta altura:
- No fusionan.
- No generan decisión.
- Son cruces forzados.

---

### 9.2 Efecto de la fusión

Al fusionarse:

- La subruta se cierra definitivamente.
- La generación se detiene.
- Cuenta como:
  - Una salida
  - Un cierre

---

### 9.3 Alcance de la fusión

La fusión:

- No bloquea subrutas posteriores.
- No invalida otros Pi.
- No altera cooldowns.
- No elimina decisiones futuras.

---

## 10. Interacción entre subrutas

- Las subrutas pueden cruzarse entre sí.
- Pueden compartir nodos y tramos.
- Los cruces:
  - Siempre generan decisión si están al mismo nivel.
  - Nunca generan fusión salvo coincidencia exacta con el main path.

---

## 11. Pi como entrada y salida. Bucles

Un Pi puede funcionar como:

- Salida
- Entrada
- Ambas

Dependiendo de:
- Dirección de llegada
- Decisiones del jugador
- Estado del cooldown

Los bucles:
- Son emergentes
- Son intencionados
- No se consideran errores

---

## 12. Límite global de subrutas

El número máximo de subrutas está limitado por:

(PN - DP) / PM

Reglas adicionales:

- Una ruta recorrida en sentido inverso no se considera nueva.
- Tramos compartidos no duplican conteo.

---

## 13. Decisiones del jugador

Existe decisión cuando:

- Un nodo tiene múltiples salidas válidas.
- Todas están al mismo nivel de altura.

En bifurcaciones:
- La vagoneta se detiene.
- El jugador inspecciona.
- Elige dirección.

---

## 14. Final del juego

- El juego termina al alcanzar PN.
- PN es un cierre absoluto.
- No existen decisiones posteriores.

---

## 15. Resultado del sistema

El sistema produce:

- Un laberinto dirigido.
- Progreso lógico garantizado.
- Exploración no lineal.
- Desorientación controlada.
- Bucles causados por decisiones, no por errores de generación.

---

Este documento define **reglas internas obligatorias**.
Cualquier implementación debe ajustarse estrictamente a estas condiciones.
