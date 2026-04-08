# Frontend Architecture — InsuraTech

## Stack

| Categoría | Tecnología |
|-----------|------------|
| Framework | Angular 19 (Standalone Components) |
| UI | Angular Material 19 |
| Estado | Angular Signals (`signal`, `computed`) |
| HTTP | `HttpClient` con interceptores |
| Formularios | Reactive Forms |
| Estilos | CSS por componente (`.css` separado) |
| Build | Angular CLI 19 |

---

## Estructura General

```
frontend/src/app/
├── app.routes.ts              # Rutas lazy con loadComponent
├── app.config.ts              # Bootstrap providers
├── core/
│   ├── interceptors/          # auth, error, idempotency
│   ├── models/                # api-response.model (PagedResult, etc.)
│   └── services/              # auth.service
├── shared/
│   ├── components/            # empty-state, loading-spinner, page-header,
│   │                          # sidenav, status-badge
│   └── pipes/                 # status-label.pipe
└── features/
    ├── auth/
    ├── dashboard/
    ├── policies/
    ├── claims/
    └── notifications/
```

---

## Arquitectura de Features (SPEC-005)

Cada módulo de `features/` sigue la misma estructura en capas:

```
<feature>/
├── container/          # (solo Policies) PoliciesFacade + PoliciesContainerComponent
├── core/
│   ├── models/         # Tipos de dominio (sin sufijo Dto)
│   ├── resource/       # (solo Policies) request/response shapes de la API
│   └── service/        # Servicios HTTP puros con signals de estado
├── guards/             # Route guards (placeholder)
└── ui/
    ├── blocks/         # Componentes reutilizables complejos
    ├── elements/       # (Policies) Componentes presentacionales simples
    ├── form/           # (Policies) Componentes de formulario
    ├── layouts/        # (Policies) Wrappers de layout
    └── pages/          # Componentes de página (routed)
```

### Reglas de la Arquitectura

| Capa | Responsabilidad | Prohibido |
|------|-----------------|-----------|
| `core/service/` | Llamadas HTTP + signals de estado | Lógica de negocio |
| `ui/blocks/` | UI compleja reutilizable con `@Input`/`@Output` | Inyección de servicios, llamadas HTTP |
| `ui/pages/` | Composición de layout, orquestación | Lógica de negocio, fetch directo |
| `container/` | Facade entre páginas y estado | UI, render |

---

## Módulos

### Dashboard (`features/dashboard/`)

```
dashboard/
├── core/
│   ├── models/dashboard.model.ts       # DashboardMetrics
│   └── service/dashboard.service.ts    # DashboardCoreService — forkJoin de 3 APIs
├── guards/
└── ui/
    ├── blocks/metric-card/             # MetricCardComponent (title, value, icon, color)
    └── pages/dashboard.component.*     # Página principal con métricas y accesos rápidos
```

**Ruta:** `'' → /dashboard`

---

### Policies (`features/policies/`) — arquitectura completa

```
policies/
├── container/
│   ├── policies-container.component.*  # Entrada al módulo (lista de pólizas)
│   └── policies.facade.ts              # PoliciesFacade — interfaz entre container y estado
├── core/
│   ├── models/                         # Policy, InsuredPerson, CoveragePeriod,
│   │                                   # HealthPlan, HealthPlanCalculation,
│   │                                   # TravelPlanSelection, TripType, Continent
│   ├── resource/                       # PolicyRequest, PolicyResponse (shapes API)
│   ├── service/
│   │   ├── policies.service.ts         # PoliciesCoreService — CRUD pólizas
│   │   ├── health-plans.service.ts     # HealthPlansService — planes de salud
│   │   └── travel-plans.service.ts     # TravelPlansService — cálculo viaje
│   └── state/policies.state.ts         # PoliciesState — signals de estado global
├── guards/
└── ui/
    ├── Pages/
    │   ├── policy-create/              # Stepper de 3 pasos: Asegurado → Cobertura → Confirm
    │   └── policy-detail/              # Detalle + acciones de póliza
    ├── blocks/
    │   ├── age-restriction/            # Banner de restricción de edad
    │   ├── health-plan-preview/        # Preview de plan de salud seleccionado
    │   ├── health-plan-selector/       # Grid de tarjetas de planes disponibles
    │   ├── policy-card/               # Tarjeta resumen de póliza
    │   ├── policy-summary-block/       # Bloque de resumen en confirmación
    │   ├── travel-details-block/       # Detalles del viaje
    │   ├── travel-duration-restriction/# Banner de advertencia >180 días
    │   └── travel-plan-preview/        # Preview de cálculo de viaje (TRM, total)
    ├── elements/
    │   ├── policy-filter-bar/          # Barra de filtros para la lista
    │   ├── policy-status-chip/         # Chip de estado de póliza
    │   └── policy-type-badge/          # Badge de tipo (Health / Travel)
    ├── form/
    │   ├── coverage-period-form/       # Formulario de vigencia
    │   └── insured-form/              # Formulario del asegurado
    └── layouts/
        ├── policies-list-layout/       # Layout de la lista de pólizas
        └── policy-detail-layout/       # Layout del detalle de póliza
```

**Rutas:**
- `/policies` → `PoliciesContainerComponent`
- `/policies/new` → `PolicyCreateComponent`
- `/policies/:id` → `PolicyDetailComponent`

---

### Claims (`features/claims/`)

```
claims/
├── core/
│   ├── models/claim.model.ts           # Claim, ClaimStatusHistory, ClaimFilters,
│   │                                   # CreateClaimRequest, ClaimActionRequest
│   └── service/claims.service.ts       # ClaimsCoreService — getAll, getById,
│                                       # create, approve, reject, appeal
├── guards/
└── ui/
    ├── blocks/
    │   ├── appeal-dialog/              # Dialog reactivo para registrar apelación
    │   ├── claim-actions/              # Botones de acción según estado del siniestro
    │   └── claim-status-timeline/      # Timeline visual del historial de estados
    └── pages/
        ├── claim-list/                 # Lista con filtro por estado y paginación
        └── claim-detail/              # Detalle + historial + acciones
```

**Rutas:**
- `/claims` → `ClaimsListComponent`
- `/claims/:id` → `ClaimDetailComponent`

---

### Notifications (`features/notifications/`)

```
notifications/
├── core/
│   ├── models/notification.model.ts    # Notification, NotificationFilters,
│   │                                   # NotificationStatus, NotificationEvent
│   └── service/notifications.service.ts # NotificationsCoreService — getNotifications,
│                                         # retryNotification, failedCount (computed)
├── guards/
└── ui/
    └── pages/notifications/            # Lista con filtro de estado y botón de reintento
```

**Ruta:** `/notifications` → `NotificationsComponent`

---

## Shared

```
shared/
├── components/
│   ├── empty-state/        # Estado vacío con icono, mensaje y botón de retry opcional
│   ├── loading-spinner/    # Spinner de carga
│   ├── page-header/        # Encabezado de página con título y acción opcional
│   ├── sidenav/            # Navegación lateral con links a módulos
│   └── status-badge/       # Badge de estado con color semántico
└── pipes/
    └── status-label.pipe   # Traduce keys de estado/evento a etiquetas legibles en español
```

---

## Interceptores (`core/interceptors/`)

| Interceptor | Qué hace |
|-------------|----------|
| `auth.interceptor` | Adjunta `Authorization: Bearer <token>` a cada request |
| `error.interceptor` | Captura errores HTTP globales y los normaliza |
| `idempotency.interceptor` | Agrega `Idempotency-Key` UUID en requests POST |

---

## Convenciones de Componentes

- **Siempre 3 archivos separados:** `.ts` + `.html` + `.css`
- **No inline `template:` ni `styles:`**
- **Standalone components** con `imports` explícitos
- **Signals** para estado reactivo: `signal<T>()`, `computed()`
- **`@Input()` / `@Output()`** en componentes `ui/blocks/` y `ui/elements/`
- **`input()` / `output()`** Signal API en componentes nuevos de Policies

---

## Routing

Todas las rutas usan **lazy loading** con `loadComponent`:

```typescript
// app.routes.ts — pattern de cada módulo
{
  path: 'claims',
  loadComponent: () =>
    import('./features/claims/ui/pages/claim-list/claims-list.component')
      .then(m => m.ClaimsListComponent),
}
```

La ruta raíz redirige a `dashboard`.
