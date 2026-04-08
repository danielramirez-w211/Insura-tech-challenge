---
id: SPEC-005
status: IN_PROGRESS
feature: front-refactor
created: 2026-04-07
updated: 2026-04-07
author: spec-generator
version: "1.0"
related-specs: ["SPEC-001", "SPEC-002", "SPEC-003", "SPEC-004"]
---


# Spec:  Refactorización Arquitectónica Frontend e Implementación de Nuevas Funcionalidades

> **Estado:** `DRAFT` → aprobar con `status: APPROVED` antes de iniciar implementación.
> **Ciclo de vida:** DRAFT → APPROVED → IN_PROGRESS → IMPLEMENTED → DEPRECATED

---

## 1. REQUERIMIENTOS

### Descripción
Migrar el módulo de pólizas desde una estructura monolítica hacia una arquitectura desacoplada **Container → Core → UI** basada en NgRx, como prerequisito obligatorio para el desarrollo de nuevas funcionalidades. Esta refactorización garantiza mantenibilidad, testeabilidad y alineación con las mejores prácticas de arquitectura moderna en Angular.

---

## Contexto técnico

El frontend de InsuraTech presenta deuda técnica acumulada en el módulo de pólizas: lógica de negocio mezclada con presentación, estado gestionado localmente con signals sin una capa de store centralizada, y componentes con múltiples responsabilidades. Esta SPEC define la migración completa antes de incorporar nuevas features de la SPEC 5.

**Stack objetivo:**

| Tecnología | Versión mínima |
|---|---|
| Angular | 17+ |
| @ngrx/store | 17+ |
| @ngrx/effects | 17+ |
| @ngrx/entity | 17+ |
| TypeScript | 5.x |
| RxJS | 7.x |

---

## Historias de Usuario

### HU-01 — Migración de estructura de carpetas al patrón Container/Core/UI

**Como** desarrollador del equipo,
**quiero** que el módulo `policies` esté organizado bajo el patrón Container → Core → UI,
**para** que cualquier desarrollador pueda identificar y modificar responsabilidades sin ambigüedad.

**Criterios de aceptación:**
- [ ] Existe la carpeta `container/` con exactamente tres archivos: `policies-container.component.html`, `policies-container.component.ts`, `policies.facade.ts`
- [ ] Existe la carpeta `core/` con subcarpetas: `models/`, `resource/`, `service/`, `store/`
- [ ] Existe la carpeta `ui/` con subcarpetas: `form/`, `elements/`, `blocks/`, `layouts/`
- [ ] No existe lógica HTTP ni llamadas a servicios directamente en componentes UI
- [ ] La migración no rompe el flujo existente de creación y listado de pólizas

---

### HU-02 — Implementación de la capa Container

**Como** desarrollador,
**quiero** un componente Container que actúe como único punto de coordinación entre el Store y los componentes UI,
**para** que los componentes visuales sean puros y no dependan de ningún servicio o estado global.

**Criterios de aceptación:**
- [ ] `policies-container.component.ts` inyecta únicamente la `PoliciesFacade`
- [ ] Toda selección de estado se hace mediante Observables o Signals expuestos por la Facade
- [ ] Toda acción de usuario se delega a métodos de la Facade
- [ ] El HTML del Container únicamente instancia componentes UI y vincula sus `@Input`/`@Output`
- [ ] Cobertura de pruebas del Container ≥ 80%

---

### HU-03 — Implementación de la Facade de Pólizas

**Como** desarrollador,
**quiero** una Facade que encapsule toda la comunicación con el Store de NgRx,
**para** que el Container no dependa directamente de la API de Store y el código sea reemplazable sin impacto en la UI.

**Criterios de aceptación:**
- [ ] `policies.facade.ts` es un `@Injectable({ providedIn: 'root' })` o provisto en el módulo del feature
- [ ] Expone Observables/Signals derivados de selectores: `policies$`, `loading$`, `error$`, `selectedPolicy$`
- [ ] Expone métodos de despacho: `loadPolicies()`, `createPolicy(command)`, `activatePolicy(id)`, `selectPolicy(id)`
- [ ] No contiene lógica de negocio: únicamente despacha acciones y proyecta selectores
- [ ] Cobertura de pruebas de la Facade ≥ 80%

---

### HU-04 — Implementación de la capa Core / Store NgRx

**Como** desarrollador,
**quiero** un Store NgRx completo para el módulo de pólizas,
**para** que el estado sea predecible, trazable y desacoplado de los componentes.

**Criterios de aceptación:**
- [ ] Existen archivos: `actions.ts`, `effects.ts`, `reducer.ts`, `selectors.ts`, `store.ts` dentro de `core/store/`
- [ ] Las acciones cubren el ciclo completo: `loadPolicies`, `loadPoliciesSuccess`, `loadPoliciesFailure`, `createPolicy`, `createPolicySuccess`, `createPolicyFailure`, `activatePolicy`, `activatePolicySuccess`, `activatePolicyFailure`
- [ ] El reducer maneja entidades con `@ngrx/entity` y persiste `loading`, `error`, `selectedId`
- [ ] Los effects consumen el `PoliciesService` de la capa Core
- [ ] Los selectores exponen: lista completa, entidad seleccionada, estado de carga, error
- [ ] Cobertura de pruebas del reducer y selectores ≥ 80%

---

### HU-05 — Implementación de la capa Core / Service y Resource

**Como** desarrollador,
**quiero** que las llamadas HTTP estén aisladas en un servicio dentro de la capa Core,
**para** que los Effects sean delgados y el servicio sea independientemente testeable.

**Criterios de aceptación:**
- [ ] `core/service/policies.service.ts` contiene todos los métodos HTTP: `getAll()`, `getById(id)`, `create(command)`, `activate(id)`
- [ ] `core/resource/` contiene los tipos de request/response raw que mapean directamente la API REST
- [ ] El servicio no transforma datos al modelo de dominio — esa responsabilidad recae en los Effects o en mappers
- [ ] Cobertura del servicio con mocks de `HttpClient` ≥ 80%

---

### HU-06 — Implementación de la capa Core / Models

**Como** desarrollador,
**quiero** que los modelos del dominio frontend estén centralizados en `core/models/`,
**para** que exista una única fuente de verdad para los tipos usados en Store, Facade y componentes UI.

**Criterios de aceptación:**
- [ ] Existen interfaces: `Policy`, `InsuredPerson`, `CoveragePeriod`, `TravelPlanSelection`, `HealthPlanSelection`
- [ ] Existe un tipo `PolicyType` como enum o string literal union: `'Life' | 'Health' | 'Travel'`
- [ ] Los modelos son interfaces puras de TypeScript (sin decoradores, sin lógica)
- [ ] Los modelos están exportados desde un barrel `index.ts`

---

### HU-07 — Refactorización de componentes UI como componentes presentacionales puros

**Como** desarrollador,
**quiero** que todos los componentes dentro de `ui/` sean puramente presentacionales,
**para** que sean reutilizables, predecibles y aislables en pruebas sin dependencias externas.

**Criterios de aceptación:**
- [ ] Ningún componente en `ui/` inyecta servicios, facades ni el Store
- [ ] Cada componente TS en `ui/` contiene únicamente: decorador `@Component`, propiedades `@Input`, eventos `@Output`, y como máximo lógica de presentación local (formateo, toggles de CSS)
- [ ] Los `@Input` usan el tipo definido en `core/models/`
- [ ] Los `@Output` emiten eventos tipados (no valores primitivos sueltos cuando el contexto requiere un objeto)
- [ ] HTML y CSS se encargan exclusivamente de presentación y estilos
- [ ] Cobertura de pruebas de componentes UI ≥ 80% mediante `@testing-library/angular` o `TestBed` con inputs simulados

---

### HU-08 — Clasificación de componentes UI en subcarpetas funcionales

**Como** desarrollador,
**quiero** que los componentes UI estén organizados en subcarpetas según su rol funcional,
**para** que cualquier colaborador pueda localizar y clasificar un componente sin ambigüedad.

**Criterios de aceptación:**
- [ ] `ui/form/` contiene componentes relacionados con formularios de entrada (e.g., `policy-form`, `insured-form`, `coverage-period-form`)
- [ ] `ui/elements/` contiene componentes atómicos reutilizables (e.g., `policy-type-badge`, `status-chip`, `currency-display`)
- [ ] `ui/blocks/` contiene componentes compuestos de mediana complejidad (e.g., `policy-card`, `policy-summary-block`, `travel-details-block`)
- [ ] `ui/layouts/` contiene componentes de estructura de página (e.g., `policies-list-layout`, `policy-detail-layout`)
- [ ] Ninguna subcarpeta contiene lógica de negocio ni dependencias de servicios

---

### HU-09 — Preparación de arquitectura para Guards (sin implementación operativa)

**Como** arquitecto del sistema,
**quiero** que la estructura de carpetas y el módulo de routing estén preparados para incorporar Guards de autenticación y autorización,
**para** que en la SPEC 6 puedan implementarse sin refactorización adicional de rutas.

**Criterios de aceptación:**
- [ ] Existe la carpeta `guards/` dentro del módulo `policies` (puede estar vacía o con un guard placeholder)
- [ ] El archivo de routing del módulo tiene comentarios explícitos indicando los puntos de inserción de `canActivate` y `canMatch`
- [ ] No se implementa ninguna lógica de autenticación/autorización en esta SPEC
- [ ] El guard placeholder (si existe) retorna `true` sin condiciones

---

### HU-10 — Mejora funcional: visualización de estado de póliza en tiempo real

**Como** usuario del sistema,
**quiero** ver el estado actualizado de cada póliza (Borrador, Activa, Cancelada) reflejado inmediatamente tras ejecutar una acción,
**para** no tener que recargar la página para confirmar el resultado de mis operaciones.

**Criterios de aceptación:**
- [ ] Al activar una póliza, el componente `status-chip` en la lista se actualiza reactivamente desde el Store sin recargar
- [ ] El estado de carga (`loading$`) muestra un indicador visual durante la operación
- [ ] Si la operación falla, se muestra un mensaje de error no bloqueante (snackbar o inline)
- [ ] La lógica de actualización optimista o pesimista está documentada en el Effect correspondiente

---

### HU-11 — Mejora funcional: filtrado y paginación de pólizas en lista

**Como** usuario,
**quiero** poder filtrar las pólizas por tipo (Life, Health, Travel) y paginar los resultados,
**para** gestionar eficientemente un catálogo creciente de pólizas.

**Criterios de aceptación:**
- [ ] El selector de tipo de póliza es un componente en `ui/elements/` que emite un `@Output filterChange`
- [ ] El Container captura el evento y despacha una acción al Store con el filtro seleccionado
- [ ] El selector NgRx aplica el filtro sobre la colección de entidades en memoria
- [ ] La paginación (client-side inicialmente) está implementada como un componente en `ui/elements/`
- [ ] El estado de filtro y página activa se persiste en el Store

---

## Arquitectura de carpetas objetivo

```
frontend/src/app/features/policies/
├── container/
│   ├── policies-container.component.html
│   ├── policies-container.component.ts
│   └── policies.facade.ts
├── core/
│   ├── models/
│   │   ├── policy.model.ts
│   │   ├── insured-person.model.ts
│   │   ├── coverage-period.model.ts
│   │   ├── travel-plan-selection.model.ts
│   │   ├── health-plan-selection.model.ts
│   │   └── index.ts
│   ├── resource/
│   │   ├── policy-request.resource.ts
│   │   └── policy-response.resource.ts
│   ├── service/
│   │   └── policies.service.ts
│   └── store/
│       ├── actions.ts
│       ├── effects.ts
│       ├── reducer.ts
│       ├── selectors.ts
│       └── store.ts
├── guards/
│   └── .gitkeep                  ← reservado para SPEC 6
└── ui/
    ├── elements/
    │   ├── policy-type-badge/
    │   │   ├── policy-type-badge.component.html
    │   │   ├── policy-type-badge.component.css
    │   │   └── policy-type-badge.component.ts
    │   ├── policy-status-chip/
    │   │   ├── policy-status-chip.component.html
    │   │   ├── policy-status-chip.component.css
    │   │   └── policy-status-chip.component.ts
    │   └── policy-filter-bar/
    │       ├── policy-filter-bar.component.html
    │       ├── policy-filter-bar.component.css
    │       └── policy-filter-bar.component.ts
    ├── form/
    │   ├── policy-form/
    │   │   ├── policy-form.component.html
    │   │   ├── policy-form.component.css
    │   │   └── policy-form.component.ts
    │   ├── insured-form/
    │   │   ├── insured-form.component.html
    │   │   ├── insured-form.component.css
    │   │   └── insured-form.component.ts
    │   └── coverage-period-form/
    │       ├── coverage-period-form.component.html
    │       ├── coverage-period-form.component.css
    │       └── coverage-period-form.component.ts
    ├── blocks/
    │   ├── policy-card/
    │   │   ├── policy-card.component.html
    │   │   ├── policy-card.component.css
    │   │   └── policy-card.component.ts
    │   ├── travel-details-block/
    │   │   ├── travel-details-block.component.html
    │   │   ├── travel-details-block.component.css
    │   │   └── travel-details-block.component.ts
    │   └── policy-summary-block/
    │       ├── policy-summary-block.component.html
    │       ├── policy-summary-block.component.css
    │       └── policy-summary-block.component.ts
    └── layouts/
        ├── policies-list-layout/
        │   ├── policies-list-layout.component.html
        │   ├── policies-list-layout.component.css
        │   └── policies-list-layout.component.ts
        └── policy-detail-layout/
            ├── policy-detail-layout.component.html
            ├── policy-detail-layout.component.css
            └── policy-detail-layout.component.ts
```

---

## Especificaciones técnicas por capa

### Capa Container

**Archivo:** `policies-container.component.ts`

```typescript
// Reglas obligatorias:
// 1. Solo inyecta PoliciesFacade — ningún otro servicio ni store directamente
// 2. Expone datos como propiedades derivadas de la Facade (Observables o Signals)
// 3. Los métodos del componente solo llaman a métodos de la Facade
// 4. No contiene lógica de transformación de datos
@Component({
  selector: 'app-policies-container',
  templateUrl: './policies-container.component.html',
})
export class PoliciesContainerComponent implements OnInit {
  constructor(private facade: PoliciesFacade) {}
  // propiedades y métodos delegados a Facade
}
```

**Archivo:** `policies.facade.ts`

```typescript
// Reglas obligatorias:
// 1. Inyecta únicamente Store<AppState>
// 2. Expone selectores como Observables públicos readonly
// 3. Expone métodos que solo llaman a this.store.dispatch(...)
// 4. No contiene lógica condicional de negocio
@Injectable({ providedIn: 'root' })
export class PoliciesFacade {
  readonly policies$ = this.store.select(PoliciesSelectors.selectAll);
  readonly loading$ = this.store.select(PoliciesSelectors.selectLoading);
  readonly error$ = this.store.select(PoliciesSelectors.selectError);
  readonly selectedPolicy$ = this.store.select(PoliciesSelectors.selectSelected);

  constructor(private store: Store<AppState>) {}

  loadPolicies(): void { this.store.dispatch(PoliciesActions.loadPolicies()); }
  createPolicy(command: CreatePolicyCommand): void { this.store.dispatch(PoliciesActions.createPolicy({ command })); }
  activatePolicy(id: string): void { this.store.dispatch(PoliciesActions.activatePolicy({ id })); }
  selectPolicy(id: string): void { this.store.dispatch(PoliciesActions.selectPolicy({ id })); }
}
```

---

### Capa Core / Store

**Archivo:** `actions.ts`

```typescript
// Grupos de acciones usando createActionGroup:
// Group: 'Policies'
// Acciones: loadPolicies, loadPoliciesSuccess, loadPoliciesFailure,
//           createPolicy, createPolicySuccess, createPolicyFailure,
//           activatePolicy, activatePolicySuccess, activatePolicyFailure,
//           selectPolicy
```

**Archivo:** `reducer.ts`

```typescript
// Requisitos:
// - Usa EntityAdapter de @ngrx/entity para la colección de Policy
// - Estado: { entities, ids, loading: boolean, error: string | null,
//             selectedId: string | null, filter: PolicyType | null, page: number }
// - Cada acción *Request activa loading: true
// - Cada acción *Success desactiva loading y actualiza entidades
// - Cada acción *Failure desactiva loading y registra error
```

**Archivo:** `selectors.ts`

```typescript
// Selectores requeridos:
// selectAll — lista completa de pólizas del EntityAdapter
// selectSelected — póliza con id === selectedId
// selectLoading — booleano de carga
// selectError — mensaje de error o null
// selectByType(type: PolicyType) — lista filtrada por tipo
// selectFilteredAndPaged — lista con filtro y paginación aplicados
```

**Archivo:** `effects.ts`

```typescript
// Requisitos:
// - Un Effect por acción de tipo Request
// - Cada Effect usa PoliciesService y maneja errores con catchError → dispatch de *Failure
// - Los Effects no contienen lógica de negocio: solo coordinan servicio → acción de resultado
// - loadPoliciesEffect se dispara al inicializar el módulo (inject: true / createEffect con dispatch)
```

---

### Capa Core / Service

**Archivo:** `policies.service.ts`

```typescript
// Reglas:
// - Inyecta HttpClient únicamente
// - Métodos: getAll(): Observable<PolicyResponse[]>
//            getById(id: string): Observable<PolicyResponse>
//            create(req: CreatePolicyRequest): Observable<PolicyResponse>
//            activate(id: string): Observable<PolicyResponse>
// - Usa los tipos de core/resource/ para request y response
// - No mapea a modelos de dominio — los Effects son responsables del mapeo si es necesario
```

---

### Capa UI — Regla universal

> **Todo archivo `.ts` dentro de `ui/` debe cumplir estrictamente:**
>
> - Únicamente contiene el decorador `@Component` con sus metadatos
> - Propiedades decoradas con `@Input()` tipadas con modelos de `core/models/`
> - Eventos decorados con `@Output()` tipados con `EventEmitter<T>` donde T es un tipo explícito
> - **Prohibido:** inyección de servicios, uso de `inject()`, acceso al Store, lógica HTTP, transformaciones de datos complejas
> - **Permitido:** propiedades computadas de presentación (`get formattedDate()`), toggles locales de UI (panel abierto/cerrado)

**Clasificación de subcarpetas UI:**

| Subcarpeta | Propósito | Ejemplos |
|---|---|---|
| `elements/` | Componentes atómicos, mínima composición | badge de tipo, chip de estado, barra de filtros |
| `form/` | Fragmentos de formulario, emiten valores tipados | formulario de asegurado, período de cobertura |
| `blocks/` | Composición de elements, representan una entidad | tarjeta de póliza, bloque de detalles de viaje |
| `layouts/` | Estructura de página, orquestan blocks y elements | layout de lista, layout de detalle |

---

## Documentación de cambios (cambios sobre funcionalidad existente)

### DC-01 — Eliminación de lógica de estado local en componente de creación

**Componente afectado:** `policy-create.component.ts` (actual)
**Cambio:** La gestión de estado mediante `signal()` local se migra al Store NgRx. Los signals `durationDays`, `travelCalculation`, `loading`, `error` pasan a derivarse del Store vía Facade.
**Impacto:** El componente actual se divide en un Container y múltiples componentes UI.
**Motivo:** Centralizar el estado elimina inconsistencias entre vistas y permite debugging con NgRx DevTools.

### DC-02 — Separación del formulario multi-paso en componentes UI atómicos

**Componente afectado:** `policy-create.component.html` (actual — formulario de 3 pasos en un solo template)
**Cambio:** Cada paso del formulario (tipo de póliza, datos del asegurado, período de cobertura, detalles específicos) se extrae a un componente en `ui/form/`.
**Impacto:** El template del Container simplifica a la orquestación de componentes form con bindings `@Input`/`@Output`.
**Motivo:** Cumplir el principio de responsabilidad única y hacer cada sección testeable en aislamiento.

### DC-03 — Migración del listado de pólizas al Store

**Componente afectado:** Componente de lista de pólizas (actual)
**Cambio:** La llamada HTTP directa desde el componente se reemplaza por despacho de `loadPolicies()` en `ngOnInit` del Container, con la lista derivada del selector NgRx.
**Impacto:** La lista responde reactivamente a cualquier operación de escritura (crear, activar) sin recargas manuales.

---

## Documentación de nueva funcionalidad

### NF-01 — Filtrado por tipo de póliza

**Descripción:** Barra de filtros sobre la lista de pólizas que permite seleccionar uno o todos los tipos (Life, Health, Travel).
**Componentes nuevos:** `ui/elements/policy-filter-bar/`
**Estado NgRx:** Nuevo campo `filter: PolicyType | null` en el reducer; nuevo selector `selectFilteredAndPaged`.
**Flujo:** Usuario selecciona tipo → `policy-filter-bar` emite `filterChange` → Container despacha acción → Reducer actualiza filtro → Selector recomputa lista → Lista se actualiza reactivamente.

### NF-02 — Actualización reactiva de estado de póliza tras activación

**Descripción:** Al activar una póliza desde el listado, el chip de estado (`policy-status-chip`) se actualiza inmediatamente sin recarga de página.
**Componentes nuevos:** `ui/elements/policy-status-chip/`
**Flujo:** Usuario pulsa activar → Container despacha `activatePolicy` → Effect llama API → `activatePolicySuccess` actualiza la entidad en el Store → El selector `selectAll` emite → La lista re-renderiza únicamente el chip afectado.

### NF-03 — Indicadores de carga y error no bloqueantes

**Descripción:** Durante operaciones asíncronas, el sistema muestra un spinner no bloqueante; en caso de error, un snackbar no intrusivo.
**Componentes afectados:** Container (controla visibilidad del spinner y snackbar según `loading$` y `error$` de la Facade).
**Requisito:** El error no bloquea la navegación; el usuario puede reintentar la operación.

---

## Criterios de testing

### Cobertura mínima obligatoria: 80%

| Artefacto | Tipo de prueba | Herramienta |
|---|---|---|
| `policies.facade.ts` | Unitaria — verificar dispatch y selección | Jest + MockStore |
| `reducer.ts` | Unitaria — transiciones de estado por acción | Jest |
| `selectors.ts` | Unitaria — proyección correcta del estado | Jest + `projector` |
| `effects.ts` | Unitaria — mock de servicio, verificar acciones emitidas | Jest + `provideMockActions` |
| `policies.service.ts` | Unitaria — mock de HttpClient | Jest + `HttpClientTestingModule` |
| Componentes `ui/` | Unitaria — renderizado con @Input, eventos @Output | TestBed / Testing Library |
| `policies-container` | Integración — Facade mock, flujo completo de pantalla | TestBed con MockStore |

### Reglas de testing para componentes UI

- Cada componente UI tiene un archivo `.spec.ts` en la misma carpeta
- Las pruebas proveen valores de `@Input` directamente sin instanciar stores ni servicios
- Se verifica que los `@Output` emitan los valores correctos ante interacciones del usuario
- Se prohíbe el uso de `NO_ERRORS_SCHEMA` salvo para componentes hijos de Angular Material ya estables

### Reglas de testing para el Store

- El reducer se prueba como función pura: `expect(reducer(initialState, action)).toEqual(expectedState)`
- Los selectores se prueban con el `projector` expuesto por `createSelector` para evitar dependencia del estado completo
- Los Effects se prueban con `provideMockActions` y verificando las acciones emitidas ante respuestas exitosas y erróneas del servicio mockeado

---

## Preparación para Guards (SPEC 6)

La arquitectura debe estar lista para incorporar Guards sin modificar la estructura de routing existente.

**Requisitos de preparación (sin implementación operativa):**

- [ ] La carpeta `guards/` existe dentro del feature `policies/`
- [ ] El archivo de routing tiene comentarios explícitos en cada ruta indicando el guard futuro:
  ```typescript
  {
    path: '',
    component: PoliciesContainerComponent,
    // TODO SPEC-6: canActivate: [AuthGuard]
  },
  {
    path: 'create',
    component: PoliciesContainerComponent,
    // TODO SPEC-6: canActivate: [AuthGuard], canMatch: [RoleGuard('admin', 'agent')]
  }
  ```
- [ ] Si se incluye un guard placeholder, retorna `true` de forma incondicional:
  ```typescript
  export const authGuard: CanActivateFn = () => true; // placeholder — implementación en SPEC 6
  ```
- [ ] No se implementa ninguna lógica de sesión, token ni redirección en esta SPEC

---

## Orden de implementación recomendado

```
1. core/models/            → definir interfaces antes que cualquier otra cosa
2. core/resource/          → tipos raw de API
3. core/service/           → servicio HTTP usando resource types
4. core/store/actions.ts   → definir todas las acciones
5. core/store/reducer.ts   → implementar reducer con EntityAdapter
6. core/store/selectors.ts → implementar selectores
7. core/store/effects.ts   → implementar effects usando el servicio
8. core/store/store.ts     → registrar feature state
9. container/policies.facade.ts → conectar store con componentes
10. ui/elements/           → componentes atómicos
11. ui/form/               → fragmentos de formulario
12. ui/blocks/             → composición de elements
13. ui/layouts/            → estructura de página
14. container/             → orquestar UI con Facade
15. guards/                → placeholder vacío
16. Pruebas unitarias      → cobertura ≥ 80% por artefacto
```
