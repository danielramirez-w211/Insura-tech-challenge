---
id: SPEC-017
status: IMPLEMENTED
feature: migration-extension
created: 2026-04-20
updated: 2026-04-20
author: spec-generator
version: "1.0"
related-specs: [SPEC-016]
---

# SPEC-017 — Extensión VSCode para Migración C# → Java 21 Spring Boot

## 1. REQUERIMIENTOS

### Descripción del feature

Extensión de VSCode que permite migrar código C# .NET 8 Clean Architecture a Java 21 Spring Boot 3.x de forma asistida por Claude AI. El desarrollador selecciona un fragmento de código C# en el editor, elige una skill de migración en el panel lateral, y recibe el equivalente Java con trazabilidad al spec ASDD original.

### Historias de usuario

**HU-01** — Como desarrollador, quiero seleccionar código C# en el editor y elegir una skill de migración para obtener el equivalente en Java 21 Spring Boot, para no tener que recordar todas las equivalencias manualmente.

**HU-02** — Como desarrollador, quiero ver el código Java generado con syntax highlighting en un panel lateral, para revisarlo antes de integrarlo al proyecto destino.

**HU-03** — Como desarrollador, quiero poder copiar o guardar el código Java generado como archivo `.java`, para integrarlo fácilmente al proyecto Spring Boot.

**HU-04** — Como desarrollador, quiero que la extensión valide el código Java migrado contra los criterios de aceptación del spec ASDD original, para garantizar que la migración es correcta.

**HU-05** — Como administrador del workspace, quiero configurar mi API key de Anthropic en VSCode Settings, para que la extensión pueda llamar a Claude sin exponer credenciales en el código.

### Criterios de aceptación (Gherkin)

```gherkin
Escenario: Migrar un Controller de C# a Java
  Dado que tengo un archivo .cs abierto en el editor
  Y tengo seleccionado el código de PoliciesController
  Y la API key de Anthropic está configurada en settings
  Cuando hago clic en "migrate-controllers" en el panel de Migration
  Entonces el panel Webview muestra el @RestController equivalente en Java
  Y el código tiene syntax highlighting correcto
  Y al inicio hay un comentario con el spec relacionado

Escenario: API key no configurada
  Dado que la API key de Anthropic no está configurada
  Cuando hago clic en cualquier skill de migración
  Entonces aparece un mensaje de error con el botón "Abrir Configuración"
  Y no se hace ninguna llamada a la API

Escenario: Guardar código Java generado
  Dado que el panel de migración muestra código Java
  Cuando hago clic en "Guardar como .java"
  Entonces aparece un diálogo de guardado con el nombre de la clase sugerido
  Y el archivo se crea con el contenido completo del código Java

Escenario: Validar migración con spec
  Dado que tengo código Java generado por cualquier skill de migración
  Cuando uso la skill "validate-migration"
  Entonces obtengo un reporte con checklist de criterios ✅/❌/⚠️
  Y un veredicto final de APROBADO o RECHAZADO
```

### Reglas de negocio

- **RN-01**: La extensión NO guarda ni registra el código enviado a Claude fuera del contexto de la llamada API.
- **RN-02**: La API key se almacena en VSCode SecretStorage (no en settings.json visible).
- **RN-03**: Si no hay texto seleccionado, se envía el archivo completo activo.
- **RN-04**: Las skills se cargan desde el directorio `skills/` de la extensión — no son hardcodeadas.
- **RN-05**: El modelo Claude por defecto es `claude-sonnet-4-6`, configurable desde Settings.

---

## 2. DISEÑO

### Arquitectura de la extensión

```
extension.ts          → Activation point, command registration
SkillLoader.ts        → Lee y parsea archivos .md de skills/
SkillTreeProvider.ts  → vscode.TreeDataProvider para el panel lateral
MigrationPanel.ts     → vscode.WebviewPanel para mostrar resultados
AnthropicClient.ts    → Llamadas HTTP a api.anthropic.com/v1/messages
```

### API Endpoints (Anthropic)

**POST** `https://api.anthropic.com/v1/messages`

Request:
```json
{
  "model": "claude-sonnet-4-6",
  "max_tokens": 4096,
  "system": "<instrucciones del skill>",
  "messages": [{ "role": "user", "content": "## Código C#\n```csharp\n<code>\n```\n\n## Contexto spec\n<specs>" }]
}
```

Response: `{ "content": [{ "type": "text", "text": "<java code>" }], "usage": { "input_tokens": N, "output_tokens": N } }`

Códigos HTTP: `200 OK`, `401 Unauthorized` (API key inválida), `429 Too Many Requests` (rate limit)

### Estructura del Webview

El panel HTML incluye:
- Tab "Java (resultado)" — código con highlight.js
- Tab "C# (origen)" — código original para referencia
- Tab "Trazabilidad" — contexto de specs del proyecto
- Botones: "Copiar", "Guardar como .java"
- Metadata: skill usada, tokens consumidos

---

## 3. LISTA DE TAREAS

### Backend / Extensión
- [x] `package.json` — manifest con commands, viewsContainers, views, configuration
- [x] `tsconfig.json` + `esbuild.js` — compilación TypeScript sin webpack
- [x] `src/extension.ts` — activation, comandos, carga de spec context
- [x] `src/SkillLoader.ts` — parseo de frontmatter YAML de archivos .md
- [x] `src/AnthropicClient.ts` — llamadas a Anthropic API con fetch nativo
- [x] `src/SkillTreeProvider.ts` — TreeDataProvider con íconos por skill
- [x] `src/MigrationPanel.ts` — Webview con tabs, syntax highlighting, copiar/guardar

### Skills
- [x] `skills/analyze-csharp.md` — Análisis y recomendación de skill
- [x] `skills/migrate-models.md` — Entidades, Value Objects, DTOs
- [x] `skills/migrate-controllers.md` — ASP.NET Controllers → @RestController
- [x] `skills/migrate-services.md` — Handlers MediatR → @Service
- [x] `skills/migrate-repositories.md` — Repositories MongoDB → Spring Data
- [x] `skills/migrate-config.md` — DI + appsettings → @Configuration + application.yml
- [x] `skills/migrate-tests.md` — xUnit + NSubstitute → JUnit 5 + Mockito
- [x] `skills/validate-migration.md` — Checklist de validación vs spec ASDD

### QA
- [ ] Instalar la extensión en modo desarrollo (`F5` en VSCode Extension Host)
- [ ] Verificar que el TreeView muestra las 8 skills con íconos correctos
- [ ] Probar cada skill con un archivo real del proyecto InsuraTech
- [ ] Verificar el flujo de error cuando la API key no está configurada
- [ ] Verificar `npm run package` genera `insuratech-migration-1.0.0.vsix` sin errores
