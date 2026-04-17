---
id: SPEC-006
status: IMPLEMENTED
feature: policies-list-visual-upgrade
created: 2026-04-08
updated: 2026-04-08
author: spec-generator
version: "1.0"
related-specs: ["SPEC-005"]
---

# Spec: Mejora Visual y Funcional del Listado de Pólizas

> **Estado:** `DRAFT` → aprobar con `status: APPROVED` antes de iniciar implementación.
> **Ciclo de vida:** DRAFT → APPROVED → IN_PROGRESS → IMPLEMENTED → DEPRECATED

---

## Documentación de Cambios (Actual → Propuesto)

Esta sección contrasta el estado actual con las mejoras propuestas para mitigar riesgos de regresión.

| Área | Estado actual | Estado propuesto | Impacto |
|------|--------------|-----------------|---------|
| Layout del listado | `mat-table` sin estilos propios — usa defaults de Angular Material | Tabla con cabecera en negro `#111111`, filas alternadas blancas / `#FAFAFA`, bordes sutiles | **CSS-only** — sin cambio de estructura de datos |
| Paleta de colores | Mezcla de colores Material por defecto | Blanco `#FFFFFF`, Negro `#111111`, Naranja `#FF6B2C` como acento | Impacta `policies-list-layout.component.css`, `policy-status-chip.component.css`, `policy-type-badge.component.css` |
| Chips de estado | Colores semánticos Material (verde/amarillo/rojo) | Chips rediseñados: contorno naranja para `Active`, gris oscuro para `Cancelled/Expired`, negro para `Pending/Suspended` | `policy-status-chip.component.css` + `.html` |
| Menú Kebab (acciones) | "Ver detalle", "Activar" (condicional), "Ver siniestros" | **Añadir:** "Eliminar" (mapea a cancelación con confirmación) y "Marcar con siniestros" (navega a registro de siniestro). Mantener las acciones existentes | `policies-list-layout.component.html` + `.ts` — nuevas salidas `@Output` |
| Barra de filtros | Barra de tipo + estado existente | Sin cambios funcionales — ajuste de color para alinearse con nueva paleta | Mínimo |
| Paginador | Angular Material por defecto | Estilización con paleta naranja en botones activos | `policies-list-layout.component.css` |
| Columnas visibles | policyNumber, insured, type, status, insuredAmount, actions | Mismas columnas con etiquetas y orden idénticos — mejora visual únicamente | Sin cambio de interface `Policy` |

### Componentes existentes afectados

| Componente | Archivo | Tipo de cambio |
|------------|---------|---------------|
| `PoliciesListLayoutComponent` | `ui/layouts/policies-list-layout/` | CSS + HTML (nuevas opciones menú) + TS (nuevos `@Output`) |
| `PolicyStatusChipComponent` | `ui/elements/policy-status-chip/` | CSS únicamente |
| `PolicyTypeBadgeComponent` | `ui/elements/policy-type-badge/` | CSS únicamente |
| `PoliciesFacade` | `container/policies.facade.ts` | Nuevos métodos: `cancelPolicy(id)` y `navigateToRegisterClaim(policyId)` |
| `PoliciesContainerComponent` | `container/policies-container.component.ts` | Manejar nuevos eventos `delete` y `markWithClaim` |

### Componentes NO afectados

- `PolicyCreateComponent`, `PolicyDetailComponent`, bloques de formulario, bloques de planes de salud/viaje — fuera de alcance.
- Backend — ningún endpoint nuevo requerido. La acción "Eliminar" usa `PUT /api/v1/policies/{id}/cancel` (soft delete ya existente).

---

## 1. REQUERIMIENTOS

### Descripción

Rediseño visual del componente de listado de pólizas (`PoliciesListLayoutComponent`) para adoptar una estética moderna, minimalista, en paleta Blanco/Negro/Naranja, junto con la incorporación de dos nuevas acciones en el menú Kebab: "Eliminar" (cancelación con diálogo de confirmación) y "Marcar con siniestros" (navegación al registro de siniestro pre-vinculado a la póliza). La feature no altera el modelo de datos ni crea nuevos endpoints — es un cambio de presentación y orquestación de acciones existentes en el frontend.

### Requerimiento de Negocio

El equipo operativo necesita un listado de pólizas de alta legibilidad que permita identificar rápidamente el estado de cada póliza y ejecutar las acciones más frecuentes (cancelar y registrar siniestros) sin salir del listado. La interfaz actual usa los defaults de Angular Material, que resultan visualmente inconsistentes con la identidad de marca y no exponen las acciones de gestión operativa en el punto de mayor tráfico del sistema.

### Historias de Usuario

---

#### HU-01: Visualizar el listado de pólizas con diseño moderno

```
Como:        Operador de seguros
Quiero:      ver el listado de pólizas con una interfaz limpia, de alta legibilidad
             y coherente con la identidad visual del producto
Para:        identificar rápidamente el estado y tipo de cada póliza sin esfuerzo visual

Prioridad:   Alta
Estimación:  S
Dependencias: Ninguna
Capa:        Frontend
```

#### Criterios de Aceptación — HU-01

**Happy Path**
```gherkin
CRITERIO-1.1: Renderizado del listado con nueva paleta
  Dado que:  el Operador navega a /policies
  Cuando:    la lista de pólizas carga exitosamente
  Entonces:  la cabecera de la tabla se muestra con fondo negro (#111111) y texto blanco (#FFFFFF)
  Y:         las filas se alternan entre blanco (#FFFFFF) y gris muy claro (#FAFAFA)
  Y:         el botón "Nueva Póliza" se muestra con fondo naranja (#FF6B2C) y texto blanco
  Y:         el paginador activo usa el color naranja como acento
```

**Error Path**
```gherkin
CRITERIO-1.2: Estado de error visible con nuevo estilo
  Dado que:  falla la carga de pólizas (error HTTP)
  Cuando:    se renderiza el banner de error
  Entonces:  el banner de error sigue siendo visible y legible
  Y:         no rompe el layout de la tabla ni el encabezado de página
```

**Edge Case**
```gherkin
CRITERIO-1.3: Lista vacía con nuevo estilo
  Dado que:  no existen pólizas que cumplan los filtros activos
  Cuando:    la tabla renderiza cero filas
  Entonces:  se muestra el componente de estado vacío (EmptyStateComponent) sin romper el layout
  Y:         el header de la tabla permanece visible con la nueva paleta
```

---

#### HU-02: Identificar el estado de una póliza de forma inmediata

```
Como:        Operador de seguros
Quiero:      que los chips de estado sean visualmente distintos y coherentes
             con la paleta Blanco/Negro/Naranja
Para:        distinguir el ciclo de vida de cada póliza sin necesidad de leer el texto

Prioridad:   Alta
Estimación:  XS
Dependencias: HU-01
Capa:        Frontend
```

#### Criterios de Aceptación — HU-02

**Happy Path**
```gherkin
CRITERIO-2.1: Chips de estado rediseñados
  Dado que:  el Operador visualiza el listado con pólizas en distintos estados
  Cuando:    observa la columna "Estado"
  Entonces:  "Active" muestra chip con borde naranja (#FF6B2C), fondo blanco y texto naranja
  Y:         "Pending" muestra chip con borde negro (#111111), fondo blanco y texto negro
  Y:         "Suspended" muestra chip con fondo naranja claro (#FFF3ED) y texto naranja oscuro
  Y:         "Cancelled" y "Expired" muestran chip con fondo gris (#F5F5F5) y texto gris (#757575)
```

**Edge Case**
```gherkin
CRITERIO-2.2: Estado desconocido o futuro
  Dado que:  una póliza llega con un estado no previsto en el catálogo actual
  Cuando:    se renderiza el chip
  Entonces:  se muestra el texto del estado tal cual, con el estilo neutro (gris/negro)
  Y:         no se produce un error de renderizado
```

---

#### HU-03: Eliminar (cancelar) una póliza desde el listado

```
Como:        Operador de seguros
Quiero:      cancelar una póliza directamente desde el menú de acciones del listado
Para:        reducir los pasos operativos necesarios para cancelar sin entrar al detalle

Prioridad:   Alta
Estimación:  S
Dependencias: HU-01
Capa:        Frontend
```

#### Criterios de Aceptación — HU-03

**Happy Path**
```gherkin
CRITERIO-3.1: Cancelar póliza con confirmación
  Dado que:  el Operador localiza una póliza en el listado
  Cuando:    abre el menú Kebab y selecciona "Eliminar"
  Entonces:  se abre un diálogo de confirmación con el mensaje
             "¿Confirmas la cancelación de la póliza [policyNumber]?"
  Y:         el diálogo muestra botones "Cancelar" (secundario) y "Confirmar" (naranja)
  Cuando:    el Operador confirma
  Entonces:  se llama a PUT /api/v1/policies/{id}/cancel
  Y:         la póliza desaparece del listado o se actualiza su estado a "Cancelled"
  Y:         se muestra un snackbar de éxito
```

**Error Path**
```gherkin
CRITERIO-3.2: Error al cancelar póliza
  Dado que:  el Operador confirma la cancelación
  Cuando:    la API responde con error (ej. 400 — ya cancelada, 404 — no encontrada)
  Entonces:  se cierra el diálogo
  Y:         se muestra un snackbar de error con el mensaje devuelto por la API
  Y:         el listado no modifica el estado de la póliza
```

**Error Path**
```gherkin
CRITERIO-3.3: Póliza ya cancelada no permite nueva cancelación
  Dado que:  una póliza tiene estado "Cancelled"
  Cuando:    el Operador abre el menú Kebab
  Entonces:  la opción "Eliminar" está deshabilitada (disabled) o no aparece
```

**Edge Case**
```gherkin
CRITERIO-3.4: Operador cancela el diálogo de confirmación
  Dado que:  el Operador abre el diálogo de confirmación de eliminación
  Cuando:    hace clic en "Cancelar" o cierra el diálogo con ESC
  Entonces:  el diálogo se cierra sin realizar ninguna llamada a la API
  Y:         el listado permanece sin cambios
```

---

#### HU-04: Marcar una póliza con siniestros desde el listado

```
Como:        Operador de seguros
Quiero:      iniciar el registro de un siniestro para una póliza directamente desde el listado
Para:        agilizar el flujo de gestión de siniestros sin perder el contexto de la póliza

Prioridad:   Alta
Estimación:  XS
Dependencias: HU-01
Capa:        Frontend
```

#### Criterios de Aceptación — HU-04

**Happy Path**
```gherkin
CRITERIO-4.1: Navegar al registro de siniestro desde el menú Kebab
  Dado que:  el Operador localiza una póliza activa en el listado
  Cuando:    abre el menú Kebab y selecciona "Marcar con siniestros"
  Entonces:  el sistema navega a /claims (listado de siniestros)
  Y:         el policyId de la póliza seleccionada se pasa como query param (?policyId=...)
             para pre-filtrar siniestros de esa póliza
```

**Error Path**
```gherkin
CRITERIO-4.2: Póliza no elegible para registro de siniestro
  Dado que:  una póliza tiene estado "Cancelled", "Expired" o "Exhausted"
  Cuando:    el Operador abre el menú Kebab
  Entonces:  la opción "Marcar con siniestros" está deshabilitada
  Y:         un tooltip indica "La póliza no está activa"
```

### Reglas de Negocio

1. **Cancelación = soft delete**: La acción "Eliminar" de la UI mapea a `PUT /api/v1/policies/{id}/cancel`. No existe eliminación física de pólizas.
2. **Sólo pólizas cancelables**: El botón "Eliminar" se habilita únicamente para pólizas en estado `Pending`, `Active` o `Suspended`. Queda deshabilitado para `Cancelled` y `Expired`.
3. **Sólo pólizas activas admiten siniestros desde el listado**: "Marcar con siniestros" se habilita sólo para pólizas en estado `Active`. Para cualquier otro estado, la opción aparece deshabilitada con tooltip explicativo.
4. **Confirmación obligatoria antes de cancelar**: Ninguna cancelación se ejecuta sin que el Operador confirme el diálogo de confirmación.
5. **Paleta de colores obligatoria**: Blanco `#FFFFFF`, Negro `#111111`, Naranja acento `#FF6B2C`. Queda prohibido usar colores fuera de esta paleta en los componentes modificados por esta spec.
6. **Tres archivos por componente**: Cualquier componente nuevo o modificado debe mantener la separación `.ts` + `.html` + `.css`. Prohibido `template:` o `styles:` inline.

---

## 2. DISEÑO

### Modelos de Datos

#### Entidades afectadas

| Entidad | Almacén | Cambios | Descripción |
|---------|---------|---------|-------------|
| `Policy` | `core/models/policy.model.ts` | **Sin cambios** | El modelo de dominio no varía |
| `PolicyFilters` | `core/models/policy.model.ts` | **Sin cambios** | Los filtros no varían |

#### Campos del modelo — sin modificaciones en esta spec

No se agregan, eliminan ni modifican campos del modelo `Policy`. La acción "Eliminar" usa el campo `status` existente y el endpoint de cancelación ya implementado.

### API Endpoints

#### Endpoints reutilizados (sin cambios en backend)

| Método | Endpoint | Uso en esta spec |
|--------|----------|-----------------|
| `PUT` | `/api/v1/policies/{id}/cancel` | Acción "Eliminar" del menú Kebab |
| `GET` | `/api/v1/claims?policyId={id}` | Acción "Marcar con siniestros" — navegación con query param |

> No se crean nuevos endpoints de backend en esta spec.

### Diseño Frontend

#### Directrices de Diseño Visual

| Token | Valor | Uso |
|-------|-------|-----|
| `--color-bg` | `#FFFFFF` | Fondo de filas, cards |
| `--color-surface` | `#FAFAFA` | Filas alternadas |
| `--color-header` | `#111111` | Fondo de cabecera de tabla |
| `--color-text` | `#111111` | Texto principal |
| `--color-text-muted` | `#757575` | Texto secundario, estados inactivos |
| `--color-accent` | `#FF6B2C` | CTA primario, estado activo, paginador seleccionado |
| `--color-accent-light` | `#FFF3ED` | Fondo chip estado Suspended |
| `--color-border` | `#E0E0E0` | Bordes de tabla y separadores |

#### Tabla de pólizas — columnas obligatorias

| # | Key `displayedColumns` | Cabecera visible | Contenido | Ancho sugerido |
|---|----------------------|-----------------|-----------|---------------|
| 1 | `policyNumber` | Número | Texto: `row.policyNumber` | 140px |
| 2 | `insured` | Asegurado | Texto: `row.insured.firstName + ' ' + row.insured.lastName` | flex |
| 3 | `type` | Tipo | Componente `PolicyTypeBadgeComponent` | 120px |
| 4 | `status` | Estado | Componente `PolicyStatusChipComponent` (rediseñado) | 140px |
| 5 | `insuredAmount` | Monto | Pipe `currency:'COP':'symbol':'1.0-0'` | 140px |
| 6 | `actions` | *(vacío)* | Menú Kebab `mat-menu` | 56px |

#### Especificación del Menú Kebab

```
┌───────────────────────────────┐
│  ⋮  (mat-icon: more_vert)     │  ← botón icon-only, sin borde
├───────────────────────────────┤
│  👁  Ver detalle              │  ← siempre visible
│  ▶  Activar                  │  ← solo si status === 'Pending'
│  ⚠  Ver siniestros           │  ← siempre visible (navega a /claims?policyId)
│  ──────────────────────────   │
│  🗑  Eliminar                 │  ← deshabilitado si status es 'Cancelled'|'Expired'
│  🏷  Marcar con siniestros    │  ← deshabilitado si status !== 'Active'
└───────────────────────────────┘
```

#### Componentes afectados

| Componente | Cambios requeridos |
|------------|-------------------|
| `PoliciesListLayoutComponent` | CSS completo de tabla (cabecera, filas alternas, borde, hover). HTML: añadir opciones "Eliminar" y "Marcar con siniestros" con `[disabled]`. TS: nuevos `@Output() delete` y `@Output() markWithClaim` de tipo `EventEmitter<string>` |
| `PolicyStatusChipComponent` | CSS: reemplazar paleta Material por tokens Blanco/Negro/Naranja según tabla de estados |
| `PolicyTypeBadgeComponent` | CSS: alinear badge al estilo minimalista (borde sólido, sin sombra) |
| `PoliciesFacade` | Añadir método `cancelPolicy(id: string)` que llame a `PoliciesCoreService.cancel(id)`. Añadir método `navigateToRegisterClaim(policyId: string)` que use `Router.navigate(['/claims'], { queryParams: { policyId } })` |
| `PoliciesContainerComponent` | Manejar los eventos `(delete)` y `(markWithClaim)` del layout — delegar a la Facade |

#### Componente nuevo — `PolicyDeleteConfirmDialogComponent`

```
policies/ui/blocks/policy-delete-confirm-dialog/
├── policy-delete-confirm-dialog.component.ts
├── policy-delete-confirm-dialog.component.html
└── policy-delete-confirm-dialog.component.css
```

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `data.policyNumber` | `string` (inyectado vía `MAT_DIALOG_DATA`) | Número de póliza a mostrar en el mensaje |
| `[mat-dialog-close]="false"` | — | Botón "Cancelar" — cierra sin acción |
| `[mat-dialog-close]="true"` | — | Botón "Confirmar" (naranja) — cierra con `true` |

**Apertura desde `PoliciesContainerComponent`:**

```typescript
// PoliciesContainerComponent
onDelete(policyId: string, policyNumber: string): void {
  const ref = this.dialog.open(PolicyDeleteConfirmDialogComponent, {
    data: { policyNumber }
  });
  ref.afterClosed().subscribe(confirmed => {
    if (confirmed) this.facade.cancelPolicy(policyId);
  });
}
```

#### Servicios afectados

| Servicio | Cambio | Método |
|----------|--------|--------|
| `PoliciesCoreService` | Ya tiene `cancel(id)` — verificar que existe y está expuesto | `cancel(id: string): Observable<void>` |
| `PoliciesFacade` | Añadir `cancelPolicy(id)` y `navigateToRegisterClaim(policyId)` | Ver sección componentes |

#### Routing — sin cambios

Las rutas existentes en `app.routes.ts` no se modifican. La acción "Marcar con siniestros" navega programáticamente a `/claims?policyId=…` usando `Router.navigate`.

### Arquitectura y Dependencias

- **Paquetes nuevos:** ninguno. Se usa `MatDialog` (ya importado), `MatSnackBar` (ya disponible en Angular Material 19).
- **Servicios externos:** ninguno.
- **Impacto en rutas:** ninguno — navegación interna programática.
- **CSS Variables:** se recomienda declarar los tokens de color en `policies-list-layout.component.css` usando custom properties (`:host { --color-accent: #FF6B2C; }`) para facilitar theming futuro.

### Notas de Implementación

- Los cambios de CSS son **aditivos**: se sobreescriben estilos de Angular Material vía `::ng-deep` o encapsulación de vista (`ViewEncapsulation.None` en el layout, si es necesario para cabecera de `mat-table`).
- Alternativamente, se pueden usar selectores de atributo de Material (`mat-header-cell`, `mat-row`) en el CSS del componente directamente ya que Angular Material los expone como clases globales.
- La acción "Activar" del menú Kebab existente **se mantiene** — esta spec no la elimina.
- Usar `MatSnackBar` para feedback de éxito/error en la acción "Eliminar".
- El componente `PolicyDeleteConfirmDialogComponent` debe usar `inject(MAT_DIALOG_DATA)` (Signal-style DI de Angular 19) en lugar del constructor tradicional.

---

## 3. LISTA DE TAREAS

> Checklist accionable para todos los agentes. Marcar cada ítem (`[x]`) al completarlo.

### Frontend

#### Implementación — Estilos

- [ ] Rediseñar `policies-list-layout.component.css`:
  - [ ] Cabecera de tabla: fondo `#111111`, texto `#FFFFFF`, font-weight 600
  - [ ] Filas: alternado blanco / `#FAFAFA`, hover `#FFF3ED`
  - [ ] Bordes: `1px solid #E0E0E0` en celdas
  - [ ] Paginador: acento `#FF6B2C` en página activa
  - [ ] Botón "Nueva Póliza": background `#FF6B2C`, color `#FFFFFF`, border-radius 8px
- [ ] Rediseñar `policy-status-chip.component.css` con nueva paleta (ver CRITERIO-2.1)
- [ ] Rediseñar `policy-type-badge.component.css` — estilo minimalista sin sombra

#### Implementación — Menú Kebab

- [ ] Añadir en `policies-list-layout.component.html`:
  - [ ] Opción "Eliminar" con `[disabled]` cuando `status === 'Cancelled' || status === 'Expired'`
  - [ ] Opción "Marcar con siniestros" con `[disabled]` cuando `status !== 'Active'` y tooltip
- [ ] Añadir en `policies-list-layout.component.ts`:
  - [ ] `@Output() delete = new EventEmitter<string>()`
  - [ ] `@Output() markWithClaim = new EventEmitter<string>()`
- [ ] Manejar `(delete)` en `PoliciesContainerComponent` — abrir diálogo de confirmación
- [ ] Manejar `(markWithClaim)` en `PoliciesContainerComponent` — delegar a Facade

#### Implementación — Diálogo de Confirmación

- [ ] Crear `policies/ui/blocks/policy-delete-confirm-dialog/policy-delete-confirm-dialog.component.ts`
- [ ] Crear `policy-delete-confirm-dialog.component.html` — mensaje + botones Cancelar / Confirmar
- [ ] Crear `policy-delete-confirm-dialog.component.css` — botón "Confirmar" en naranja `#FF6B2C`

#### Implementación — Facade y Servicios

- [ ] Añadir `cancelPolicy(id: string)` en `PoliciesFacade` — llama a `PoliciesCoreService.cancel(id)` y recarga la lista
- [ ] Añadir `navigateToRegisterClaim(policyId: string)` en `PoliciesFacade` — `Router.navigate(['/claims'], { queryParams: { policyId } })`
- [ ] Verificar que `PoliciesCoreService` expone `cancel(id): Observable<void>` — si no, añadirlo
- [ ] Añadir `MatSnackBar` en `PoliciesContainerComponent` para feedback de éxito/error en cancelación

#### Tests Frontend

- [ ] `[PoliciesListLayout] muestra opción "Eliminar" deshabilitada para estado Cancelled`
- [ ] `[PoliciesListLayout] muestra opción "Eliminar" deshabilitada para estado Expired`
- [ ] `[PoliciesListLayout] emite evento delete al hacer clic en "Eliminar" con póliza elegible`
- [ ] `[PoliciesListLayout] muestra opción "Marcar con siniestros" deshabilitada si status !== Active`
- [ ] `[PoliciesListLayout] emite evento markWithClaim con el id correcto`
- [ ] `[PolicyDeleteConfirmDialog] cierra con true al confirmar`
- [ ] `[PolicyDeleteConfirmDialog] cierra con false al cancelar`
- [ ] `[PoliciesContainer] llama a facade.cancelPolicy si el diálogo se confirma`
- [ ] `[PoliciesContainer] no llama a facade.cancelPolicy si el diálogo se cancela`
- [ ] `[PolicyStatusChip] aplica clase chip--active para estado Active`
- [ ] `[PolicyStatusChip] aplica clase chip--cancelled para estado Cancelled`

### Backend

> No se requieren cambios de backend en esta spec. El endpoint `PUT /api/v1/policies/{id}/cancel` ya existe.

- [ ] Verificar que `PUT /api/v1/policies/{id}/cancel` retorna `400` con mensaje legible si la póliza ya está cancelada (para que el snackbar de error sea informativo)

### QA

- [ ] Ejecutar smoke suite en `/policies` antes de aplicar cambios (baseline visual)
- [ ] Verificar CRITERIO-1.1 — paleta de cabecera y filas alternadas
- [ ] Verificar CRITERIO-2.1 — chips de estado con nueva paleta
- [ ] Verificar CRITERIO-3.1 — flujo completo de cancelación (abrir menú → diálogo → confirmar → snackbar)
- [ ] Verificar CRITERIO-3.3 — botón "Eliminar" deshabilitado para pólizas Cancelled/Expired
- [ ] Verificar CRITERIO-3.4 — cancelar el diálogo no ejecuta la llamada API
- [ ] Verificar CRITERIO-4.1 — "Marcar con siniestros" navega a `/claims?policyId=…`
- [ ] Verificar CRITERIO-4.2 — opción deshabilitada con tooltip para pólizas no activas
- [ ] Verificar no regresión: acciones existentes "Ver detalle", "Activar", "Ver siniestros" siguen funcionando
- [ ] Verificar no regresión: filtros de tipo y estado siguen operativos
- [ ] Verificar no regresión: paginación sigue funcionando tras rediseño CSS
- [ ] Actualizar estado spec a `IMPLEMENTED` al completar todos los ítems
