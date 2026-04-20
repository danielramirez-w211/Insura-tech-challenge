# InsuraTech Migration — Extensión VSCode

Extensión que asiste la migración de código C# .NET 8 Clean Architecture a Java 21 Spring Boot 3.x usando Claude AI.

---

## Instalación

### Requisitos previos
- VSCode 1.85.0 o superior
- Node.js 18+ y npm
- API Key de Anthropic (obtener en [console.anthropic.com](https://console.anthropic.com/))

### Opción A — Instalar desde código fuente

```bash
cd migration/vscode-extension
npm install
npm run build
npm run package
code --install-extension insuratech-migration-1.0.0.vsix
```

### Opción B — Desarrollar en modo extensión

1. Abrir la carpeta `migration/vscode-extension/` en VSCode
2. Instalar dependencias: `npm install`
3. Presionar `F5` → Se abre una ventana "Extension Development Host"
4. En esa ventana, abrir el repositorio InsuraTech

---

## Configuración

Ir a **Configuración de VSCode** (`Ctrl+,`) y buscar `insuratech`:

| Setting | Descripción | Requerido |
|---------|-------------|-----------|
| `insuratech.migration.anthropicApiKey` | API Key de Anthropic | ✅ Sí |
| `insuratech.migration.model` | Modelo Claude (default: `claude-sonnet-4-6`) | No |
| `insuratech.migration.specsFolder` | Ruta a la carpeta de specs (default: `asd/.github/specs`) | No |

---

## Uso

### 1. Seleccionar código C# en el editor
Abre cualquier archivo `.cs` del proyecto y selecciona el fragmento que deseas migrar (o deja sin selección para enviar el archivo completo).

### 2. Elegir una skill en el panel lateral
En la barra de actividad (izquierda), busca el ícono ↔️ **InsuraTech Migration**. Verás la lista de skills disponibles.

### 3. Hacer clic en la skill
El panel Webview se abre con el código Java generado, con syntax highlighting y las siguientes opciones:

- **Tab "Java (resultado)"** — Código Java migrado listo para revisar
- **Tab "C# (origen)"** — Código original para comparar
- **Tab "Trazabilidad"** — Specs del proyecto relacionados
- **Copiar** — Copia el código al portapapeles
- **Guardar como .java** — Abre diálogo para guardar el archivo

---

## Skills disponibles

| Skill | Úsala cuando... |
|-------|----------------|
| `analyze-csharp` | No sabes qué skill usar — analiza el código y recomienda |
| `migrate-models` | Tienes una Entity, Value Object, DTO o Record de C# |
| `migrate-controllers` | Tienes un Controller de ASP.NET Core |
| `migrate-services` | Tienes un Handler de MediatR o Application Service |
| `migrate-repositories` | Tienes una implementación de Repository con MongoDB |
| `migrate-config` | Tienes DependencyInjection.cs, appsettings.json o Program.cs |
| `migrate-tests` | Tienes tests xUnit con NSubstitute/FluentAssertions |
| `validate-migration` | Quieres validar que el código Java generado cumple el spec |

---

## Flujo recomendado

```
1. analyze-csharp        → identificar tipo y skill correcta
2. migrate-models        → migrar las entidades y Value Objects primero
3. migrate-repositories  → después los repositorios (dependen de los modelos)
4. migrate-services      → luego los services/handlers
5. migrate-controllers   → finalmente los controllers
6. migrate-tests         → migrar los tests de cada feature
7. validate-migration    → validar cada archivo contra el spec ASDD
```

---

## Estructura del proyecto

```
migration/
├── vscode-extension/
│   ├── src/                    # Código TypeScript de la extensión
│   ├── skills/                 # 8 archivos .md con instrucciones para Claude
│   ├── media/icon.svg          # Ícono de la extensión
│   ├── package.json            # Manifest
│   ├── esbuild.js              # Script de build
│   └── tsconfig.json
├── specs/
│   └── migration-extension.spec.md  # SPEC-017 de esta extensión
├── MIGRATION_PLAN.md           # Trazabilidad spec → archivos Java
└── README.md                   # Este archivo
```

---

## Desarrollo — agregar nuevas skills

Crea un archivo `.md` en `migration/vscode-extension/skills/` con este frontmatter:

```markdown
---
name: nombre-de-la-skill
description: Descripción breve de lo que hace
input: Qué tipo de código C# espera recibir
output: Qué genera como resultado
spec-mapping: Qué specs cubre
---

# Instrucciones para Claude
[instrucciones del sistema prompt...]
```

La extensión carga automáticamente todas las skills del directorio sin necesidad de recompilar.

---

## Solución de problemas

**"API Key no configurada"**
→ Ir a Settings → `insuratech.migration.anthropicApiKey` y agregar tu API key.

**"Error 401 de la API Anthropic"**
→ La API key es incorrecta o expiró. Verificar en [console.anthropic.com](https://console.anthropic.com/).

**"No se encontraron skills"**
→ Verificar que la carpeta `skills/` existe dentro de la extensión instalada. Si estás en modo desarrollo, verificar que `esbuild.js` no excluye la carpeta `skills/`.

**El TreeView no aparece**
→ Abrir un archivo `.cs` para activar la extensión, o ejecutar el comando `Migration: Show Panel` desde la Paleta de Comandos (`Ctrl+Shift+P`).
