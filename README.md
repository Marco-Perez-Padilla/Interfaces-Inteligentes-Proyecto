# README — Colaboradores del Proyecto Unity

## Requisitos previos

Cada colaborador debe tener:

- Unity Hub instalado  
- **La MISMA versión de Unity** que el resto del equipo  
- Git instalado  
- Acceso al repositorio (GitHub / GitLab)

---

## Cómo clonar el proyecto

### Clonar el repositorio

En la carpeta donde quieras guardar el proyecto, ejecuta:

```bash
git clone URL_DEL_REPOSITORIO
```

### Abrir el proyecto en Unity Hub

1. Abre Unity Hub
2. Pulsa **Add** → **Add project from disk**
3. Selecciona la carpeta clonada (la raíz del proyecto)
4. Ábrelo con la versión de Unity acordada
5. Unity generará automáticamente carpetas locales como `Library`, `Temp`, etc.

### ¿Por qué el repositorio no se aloja en Assets/?
- Unity necesita `Packages` y `ProjectSettings` para abrir el proyecto correctamente
- Compartir solo `Assets` provoca errores y referencias rotas
- Es el estándar profesional y académico
- Evita problemas difíciles de depurar

## Qué no se sube al repositorio

Las siguientes carpetas y archivos no forman parte del proyecto compartido:

- `Library/`
- `Temp/`
- `Logs/`
- `UserSettings/`
- `.vscode/`
- `*.sln`
- `*.csproj`
