---
id: SPEC-002
status: DRAFT
feature: frontend-base-structure
created: 2026-03-31
updated: 2026-03-31
author: spec-generator
version: "1.0"
related-specs: ["SPEC-001"]
---

# Spec: Frontend Angular 20 — Estructura Base + Módulos de Negocio

> **Estado:** `DRAFT` → aprobar con `status: APPROVED` antes de iniciar implementación.
> **Ciclo de vida:** DRAFT → APPROVED → IN_PROGRESS → IMPLEMENTED → DEPRECATED

---

## 1. REQUERIMIENTOS

### Descripción

Este spec define el diseño y arquitectura completa del frontend de InsuraTech usando **Angular 20 con Standalone Components, Angular Material y TypeScript strict**. Cubre la estructura base del proyecto (scaffold, core, shared), cuatro módulos de negocio funcionales (Dashboard, Pólizas, Siniestros, Notificaciones) y la integración con el backend documentado en SPEC-001. El frontend consume los endpoints REST existentes e implementa los flujos de negocio del sector asegurador: creación idempotente de pólizas, gestión de siniestros con apelación y centro de notificaciones con retry.

### Requerimiento de Negocio

El backend InsuraTech (SPEC-001, APPROVED e IMPLEMENTADO) expone una API REST en `http://localhost:5000/api/v1` con las siguientes capacidades:
- Gestión de pólizas con creación idempotente (`Idempotency-Key` header), filtros y paginación (`TotalCount` correcto)
- Gestión de siniestros con flujo de estados: `Registered → Approved/Rejected → Appealed → Paid`
- Módulo de notificaciones con estados `Pending/Sent/Failed` y endpoint de retry
- Swagger disponible en `http://localhost:5000/swagger`

Se requiere una SPA que permita a los operadores gestionar estos recursos con una UI profesional, reactiva y alineada con Material Design.

---

### Historias de Usuario

#### HU-01: Scaffold base Angular 20

```
Como:        Desarrollador del proyecto InsuraTech
Quiero:      Un proyecto Angular 20 configurado con la arquitectura core/shared/features
Para:        Tener una base escalable que soporte el crecimiento incremental de features vía ASDD

Prioridad:   Alta
Estimación:  S
Dependencias: Ninguna
Capa:        Frontend
```

#### Criterios de Aceptación — HU-01

**Happy Path**
```gherkin
CRITERIO-1.1: Proyecto Angular 20 levanta en modo desarrollo
  Dado que:  El proyecto está configurado con Angular 20 + Angular Material
  Cuando:    Se ejecuta `ng serve`
  Entonces:  La app carga en http://localhost:4200 sin errores de compilación
             Y el layout principal (sidenav + toolbar) es visible
             Y las rutas lazy-loaded están registradas en app.routes.ts
```

**Happy Path**
```gherkin
CRITERIO-1.2: Interceptor de idempotencia actúa en POST requests
  Dado que:  La app está configurada con idempotencyInterceptor
  Cuando:    Se hace cualquier POST (ej. crear póliza)
  Entonces:  El request incluye el header Idempotency-Key con un UUID v4 generado automáticamente
```

**Edge Case**
```gherkin
CRITERIO-1.3: Variables de entorno correctamente separadas
  Dado que:  Existen environment.ts y environment.prod.ts
  Cuando:    Se hace ng build --configuration=production
  Entonces:  La apiUrl apunta al URL de producción configurado
             Y no hay URLs hardcodeadas en ningún service
```

---

#### HU-02: Dashboard de administración con métricas

```
Como:        Operador de InsuraTech
Quiero:      Ver un dashboard con métricas rápidas del sistema al iniciar sesión
Para:        Tener visibilidad inmediata del estado operativo: pólizas activas, siniestros pendientes y notificaciones fallidas

Prioridad:   Alta
Estimación:  M
Dependencias: HU-01
Capa:        Frontend
```

#### Criterios de Aceptación — HU-02

**Happy Path**
```gherkin
CRITERIO-2.1: Dashboard muestra métricas al cargar
  Dado que:  El operador navega a la ruta /dashboard
  Cuando:    La página carga
  Entonces:  Se muestran tarjetas mat-card con:
             - Total de pólizas activas (GET /api/v1/policies?status=Active)
             - Total de siniestros pendientes (GET /api/v1/claims?status=Registered)
             - Total de notificaciones fallidas (GET /api/v1/notifications?status=Failed)
             Y cada tarjeta tiene un indicador de loading (mat-spinner) mientras carga
```

**Error Path**
```gherkin
CRITERIO-2.2: Error de API muestra estado vacío con mensaje
  Dado que:  El backend no está disponible
  Cuando:    El dashboard intenta cargar las métricas
  Entonces:  Se muestra el componente <empty-state> con mensaje "No se pudo cargar la información"
             Y un botón "Reintentar" que vuelve a llamar a la API
```

**Happy Path**
```gherkin
CRITERIO-2.3: Dashboard incluye accesos directos a módulos
  Dado que:  El operador está en el dashboard
  Cuando:    Hace clic en "Ver pólizas" o "Ver siniestros"
  Entonces:  Navega a /policies o /claims respectivamente via Angular Router
```

---

#### HU-03: Listado de pólizas con filtros y paginación reactiva

```
Como:        Operador de InsuraTech
Quiero:      Ver y filtrar el listado de pólizas con paginación correcta
Para:        Gestionar eficientemente el portafolio de pólizas de seguros

Prioridad:   Alta
Estimación:  L
Dependencias: HU-01
Capa:        Frontend
```

#### Criterios de Aceptación — HU-03

**Happy Path**
```gherkin
CRITERIO-3.1: Listado de pólizas con paginación correcta
  Dado que:  El operador navega a /policies
  Cuando:    La página carga
  Entonces:  Se muestra mat-table con columnas: Número, Asegurado, Tipo, Estado, Cobertura, Vigencia
             Y mat-paginator refleja el TotalCount real del backend (con filtros aplicados)
             Y la tabla muestra el estado con StatusBadge por colores
```

**Happy Path**
```gherkin
CRITERIO-3.2: Filtros reactivos actualizan la tabla
  Dado que:  El operador está en el listado de pólizas
  Cuando:    Selecciona Status=Active en el mat-select de filtros
  Entonces:  Se llama a GET /api/v1/policies?status=Active&page=1&pageSize=10
             Y el mat-paginator actualiza el total con el TotalCount del response
             Y los filtros persisten al cambiar de página
```

**Happy Path**
```gherkin
CRITERIO-3.3: Crear póliza con flujo mat-stepper e idempotencia
  Dado que:  El operador hace clic en "Nueva Póliza"
  Cuando:    Completa el mat-stepper (Datos del Asegurado → Cobertura → Confirmación)
  Entonces:  Se llama a POST /api/v1/policies con header Idempotency-Key automático
             Y al éxito se muestra MatSnackBar "Póliza creada exitosamente"
             Y la tabla se recarga con la nueva póliza
```

**Error Path**
```gherkin
CRITERIO-3.4: Activar póliza pendiente
  Dado que:  Existe una póliza en estado Pending
  Cuando:    El operador hace clic en "Activar" en el menú de acciones
  Entonces:  Se llama a PUT /api/v1/policies/{id}/activate
             Y al éxito se muestra MatSnackBar "Póliza activada"
             Y el estado en la tabla cambia a Active sin recargar toda la página
```

**Edge Case**
```gherkin
CRITERIO-3.5: Ver siniestros de una póliza desde el listado
  Dado que:  El operador está en el listado de pólizas
  Cuando:    Hace clic en "Ver siniestros" de una póliza específica
  Entonces:  Se llama a GET /api/v1/policies/{id}/claims
             Y se muestra un MatDialog o navegación a /claims?policyId={id}
```

---

#### HU-04: Gestión de siniestros con historial cronológico y apelación

```
Como:        Operador de InsuraTech
Quiero:      Ver y gestionar los siniestros, incluyendo aprobar, rechazar y apelar
Para:        Ejecutar el flujo completo de gestión de siniestros con trazabilidad de estados

Prioridad:   Alta
Estimación:  L
Dependencias: HU-01, HU-03
Capa:        Frontend
```

#### Criterios de Aceptación — HU-04

**Happy Path**
```gherkin
CRITERIO-4.1: Listado de siniestros con filtro por estado
  Dado que:  El operador navega a /claims
  Cuando:    La página carga
  Entonces:  Se muestra mat-table con columnas: Número, Póliza, Descripción, Monto, Estado, Fecha
             Y filtros por Status y PolicyId
             Y paginación correcta con TotalCount del backend
```

**Happy Path**
```gherkin
CRITERIO-4.2: Detalle de siniestro con historial cronológico de estados
  Dado que:  El operador hace clic en un siniestro
  Cuando:    Se abre el detalle (/claims/{id})
  Entonces:  Se muestra la información completa del siniestro
             Y el historial de estados en orden cronológico usando mat-stepper (read-only) o timeline
             Y los botones de acción disponibles según el estado actual:
               - Estado Registered: botones "Aprobar" y "Rechazar"
               - Estado Rejected: botón "Apelar"
               - Estado Appealed: botones "Aprobar" y "Rechazar"
               - Estado Approved: botón "Marcar como Pagado"
```

**Happy Path**
```gherkin
CRITERIO-4.3: Apelar siniestro rechazado
  Dado que:  Un siniestro está en estado Rejected
  Cuando:    El operador hace clic en "Apelar" e ingresa las observaciones en un MatDialog
  Entonces:  Se llama a PUT /api/v1/claims/{id}/appeal con { observations, responsibleUser }
             Y al éxito el estado cambia a Appealed en la UI (Signal actualizado)
             Y el historial muestra el nuevo estado Appealed con timestamp
             Y se muestra MatSnackBar "Apelación registrada"
```

**Error Path**
```gherkin
CRITERIO-4.4: Acción no permitida por estado actual
  Dado que:  Un siniestro está en estado Paid
  Cuando:    El operador intenta ejecutar una acción no disponible
  Entonces:  Los botones de acción no disponibles están deshabilitados (disabled)
             O HTTP 422 del backend se traduce en MatSnackBar con mensaje de error descriptivo
```

---

#### HU-05: Centro de notificaciones con retry manual

```
Como:        Operador de InsuraTech
Quiero:      Ver todas las notificaciones del sistema y poder reintentar las fallidas
Para:        Garantizar que ninguna notificación crítica se pierda y tener visibilidad del estado de envío

Prioridad:   Media
Estimación:  M
Dependencias: HU-01
Capa:        Frontend
```

#### Criterios de Aceptación — HU-05

**Happy Path**
```gherkin
CRITERIO-5.1: Listado de notificaciones con filtro por estado
  Dado que:  El operador navega a /notifications
  Cuando:    La página carga
  Entonces:  Se muestra mat-table con columnas: Evento, Destinatario, Asunto, Estado, Reintentos, Fecha
             Y filtros por Status (Pending / Sent / Failed)
             Y cada fila muestra el estado con color: Pending=amarillo, Sent=verde, Failed=rojo
```

**Happy Path**
```gherkin
CRITERIO-5.2: Retry manual de notificación fallida
  Dado que:  Existe una notificación con Status = Failed
  Cuando:    El operador hace clic en el botón "Reintentar" de esa fila
  Entonces:  Se llama a PUT /api/v1/notifications/{id}/retry
             Y al éxito el estado en la tabla cambia a Pending (Signal actualizado, sin reload completo)
             Y se muestra MatSnackBar "Reintento programado"
             Y el botón "Reintentar" solo es visible para notificaciones con Status = Failed
```

**Edge Case**
```gherkin
CRITERIO-5.3: Badge de notificaciones fallidas en la navegación
  Dado que:  Existen notificaciones con Status = Failed
  Cuando:    El operador está en cualquier pantalla de la app
  Entonces:  El ícono de notificaciones en el toolbar muestra un mat-badge con el count de fallidas
             Y el badge se actualiza reactivamente vía Signal cuando una notificación es reintentada
```

---

### Reglas de Negocio

1. **Idempotencia en creación**: Todo POST /api/v1/policies DEBE incluir el header `Idempotency-Key` con un UUID v4. El `idempotencyInterceptor` lo agrega automáticamente; el usuario no lo ve ni lo gestiona.
2. **Paginación**: El `mat-paginator` SIEMPRE usa `TotalCount` del response del backend — nunca cuenta local. Los filtros se pasan como query params en cada página.
3. **Flujo de estados de siniestros**: Los botones de acción se renderizan condicionalmente según `claim.status`. El frontend NUNCA permite ejecutar una acción inválida para el estado actual.
4. **Retry de notificaciones**: El botón "Reintentar" solo aparece cuando `notification.status === 'Failed'`. El estado se actualiza localmente en el Signal tras el HTTP 200, sin recargar toda la lista.
5. **Estado reactivo con Signals**: El conteo de notificaciones fallidas es un `computed()` Signal derivado del estado de notificaciones. Se muestra como `mat-badge` en la navegación.
6. **Strict TypeScript**: Todas las interfaces deben corresponder exactamente a los DTOs del backend. Sin `any`. Validaciones de formularios con Reactive Forms.
7. **Standalone Components**: Todos los componentes usan `standalone: true`. No se crean NgModules. Las rutas son lazy-loaded con `loadComponent()`.
8. **Angular Material**: No se mezclan otros sistemas de diseño. Todos los componentes de UI son de Angular Material o componentes propios basados en él.

---

## 2. DISEÑO

### Modelos de Datos

#### Modelos frontend (interfaces TypeScript — espejo de DTOs del backend)

| Interfaz | Archivo | Corresponde a |
|----------|---------|---------------|
| `PolicyDto` | `features/policies/models/policy.model.ts` | `PolicyResponse` del backend |
| `PolicyFilters` | `features/policies/models/policy.model.ts` | Query params de GET /policies |
| `ClaimDto` | `features/claims/models/claim.model.ts` | `ClaimResponse` del backend |
| `ClaimStatusHistoryDto` | `features/claims/models/claim.model.ts` | Historial de estados |
| `NotificationDto` | `features/notifications/models/notification.model.ts` | `NotificationResponse` del backend |
| `PagedResult<T>` | `core/models/api-response.model.ts` | Wrapper paginado genérico |
| `ApiError` | `core/models/api-response.model.ts` | Error HTTP estándar |

#### Campos clave — `PagedResult<T>` (core)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `items` | `T[]` | Datos de la página actual |
| `totalCount` | `number` | Total de registros con filtros aplicados |
| `page` | `number` | Página actual (1-based) |
| `pageSize` | `number` | Tamaño de página |

#### Campos clave — `PolicyDto`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | `string` (UUID) | Identificador |
| `policyNumber` | `string` | Número de póliza (POL-YYYY-NNNNNNN) |
| `status` | `PolicyStatus` | `'Pending' \| 'Active' \| 'Suspended' \| 'Expired' \| 'Cancelled'` |
| `type` | `PolicyType` | `'Life' \| 'Health' \| 'Vehicle' \| 'Home' \| 'Travel'` |
| `insured` | `InsuredDto` | Datos del asegurado |
| `coveragePeriod` | `CoveragePeriodDto` | `{ startDate, endDate }` |
| `insuredAmount` | `number` | Monto asegurado |
| `createdAt` | `string` (ISO8601) | Fecha de creación |

#### Campos clave — `ClaimDto`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | `string` (UUID) | Identificador |
| `claimNumber` | `string` | Número de siniestro |
| `policyId` | `string` | ID de la póliza asociada |
| `status` | `ClaimStatus` | `'Registered' \| 'Approved' \| 'Rejected' \| 'Appealed' \| 'Paid'` |
| `claimAmount` | `number` | Monto del siniestro |
| `statusHistory` | `ClaimStatusHistoryDto[]` | Historial cronológico de estados |

#### Campos clave — `NotificationDto`
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | `string` (UUID) | Identificador |
| `event` | `NotificationEvent` | `'PolicyActivated' \| 'PolicyExpiringSoon' \| 'ClaimRegistered' \| 'ClaimStatusChanged'` |
| `recipientId` | `string` | DocumentId del destinatario |
| `recipientName` | `string` | Nombre del destinatario |
| `subject` | `string` | Asunto de la notificación |
| `status` | `NotificationStatus` | `'Pending' \| 'Sent' \| 'Failed'` |
| `retryCount` | `number` | Número de reintentos |
| `createdAt` | `string` (ISO8601) | Fecha de creación |

---

### API Endpoints consumidos por el frontend

#### Pólizas
| Método | Ruta | Uso en frontend |
|--------|------|----------------|
| `GET` | `/api/v1/policies` | Listado con filtros + paginación |
| `POST` | `/api/v1/policies` | Crear póliza (con Idempotency-Key) |
| `GET` | `/api/v1/policies/{id}` | Detalle de póliza |
| `PUT` | `/api/v1/policies/{id}/activate` | Activar póliza desde la UI |
| `GET` | `/api/v1/policies/{id}/claims` | Ver siniestros de una póliza |

#### Siniestros
| Método | Ruta | Uso en frontend |
|--------|------|----------------|
| `GET` | `/api/v1/claims` | Listado de siniestros con filtros |
| `POST` | `/api/v1/claims` | Registrar nuevo siniestro |
| `GET` | `/api/v1/claims/{id}` | Detalle + historial de estados |
| `PUT` | `/api/v1/claims/{id}/approve` | Aprobar siniestro |
| `PUT` | `/api/v1/claims/{id}/reject` | Rechazar siniestro |
| `PUT` | `/api/v1/claims/{id}/appeal` | Apelar siniestro rechazado |

#### Notificaciones
| Método | Ruta | Uso en frontend |
|--------|------|----------------|
| `GET` | `/api/v1/notifications` | Listado con filtro por status |
| `PUT` | `/api/v1/notifications/{id}/retry` | Reintentar notificación fallida |

---

### Diseño Frontend

#### Layout principal
| Componente | Archivo | Descripción |
|------------|---------|-------------|
| `AppComponent` | `app.component.ts` | Shell: `mat-sidenav-container` + `mat-toolbar` |
| `SidenavComponent` | `shared/components/sidenav/` | `mat-nav-list` con rutas a features + badge de notificaciones fallidas |
| `PageHeaderComponent` | `shared/components/page-header/` | Título de página + breadcrumb + botón de acción principal |
| `StatusBadgeComponent` | `shared/components/status-badge/` | Chip con color según estado (policy/claim/notification) |
| `LoadingSpinnerComponent` | `shared/components/loading-spinner/` | Overlay de carga con `mat-spinner` |
| `EmptyStateComponent` | `shared/components/empty-state/` | Estado vacío con ícono + mensaje + botón retry |

#### Páginas nuevas
| Página | Archivo | Ruta | Componentes Material clave |
|--------|---------|------|---------------------------|
| `DashboardPage` | `features/dashboard/pages/dashboard.component.ts` | `/dashboard` | `mat-card`, `mat-icon`, `mat-progress-bar` |
| `PoliciesListPage` | `features/policies/pages/policies-list.component.ts` | `/policies` | `mat-table`, `mat-sort`, `mat-paginator`, `mat-select` |
| `PolicyDetailPage` | `features/policies/pages/policy-detail.component.ts` | `/policies/:id` | `mat-card`, `mat-chip` |
| `PolicyCreatePage` | `features/policies/pages/policy-create.component.ts` | `/policies/new` | `mat-stepper`, `mat-form-field`, `mat-datepicker` |
| `ClaimsListPage` | `features/claims/pages/claims-list.component.ts` | `/claims` | `mat-table`, `mat-sort`, `mat-paginator` |
| `ClaimDetailPage` | `features/claims/pages/claim-detail.component.ts` | `/claims/:id` | `mat-stepper` (readonly), `mat-timeline`, `MatDialog` |
| `NotificationsPage` | `features/notifications/pages/notifications.component.ts` | `/notifications` | `mat-table`, `mat-chip`, `mat-badge`, `MatSnackBar` |
| `LoginPage` | `features/auth/pages/login.component.ts` | `/login` | `mat-form-field`, `mat-button` |

#### Componentes hijos por feature
| Componente | Archivo | Props clave |
|------------|---------|-------------|
| `PolicyFiltersComponent` | `features/policies/components/policy-filters/` | `@Output() filtersChange: PolicyFilters` |
| `PolicyRowActionsComponent` | `features/policies/components/policy-row-actions/` | `@Input() policy`, `@Output() activate, viewClaims` |
| `ClaimStatusTimelineComponent` | `features/claims/components/claim-status-timeline/` | `@Input() statusHistory: ClaimStatusHistoryDto[]` |
| `ClaimActionsComponent` | `features/claims/components/claim-actions/` | `@Input() claim`, `@Output() approve, reject, appeal, pay` |
| `AppealDialogComponent` | `features/claims/components/appeal-dialog/` | `MatDialogRef`, `@Output() submitted: AppealRequest` |
| `NotificationRowComponent` | `features/notifications/components/notification-row/` | `@Input() notification`, `@Output() retry` |
| `MetricCardComponent` | `features/dashboard/components/metric-card/` | `@Input() title, value, icon, color, loading` |

#### Servicios
| Service | Archivo | Signals expuestos |
|---------|---------|-------------------|
| `PoliciesService` | `features/policies/services/policies.service.ts` | `policies`, `loading`, `totalCount`, `filters` |
| `ClaimsService` | `features/claims/services/claims.service.ts` | `claims`, `loading`, `totalCount`, `selectedClaim` |
| `NotificationsService` | `features/notifications/services/notifications.service.ts` | `notifications`, `failedCount`, `loading` |
| `DashboardService` | `features/dashboard/services/dashboard.service.ts` | `metrics`, `loading` |
| `AuthService` | `core/services/auth.service.ts` | `currentUser`, `isAuthenticated` |

#### Interceptores
| Interceptor | Archivo | Función |
|-------------|---------|---------|
| `authInterceptor` | `core/interceptors/auth.interceptor.ts` | Agrega `Authorization: Bearer {token}` (preparado para auth futura) |
| `idempotencyInterceptor` | `core/interceptors/idempotency.interceptor.ts` | Agrega `Idempotency-Key: {UUID}` en todo POST |
| `errorInterceptor` | `core/interceptors/error.interceptor.ts` | Convierte errores HTTP en mensajes de MatSnackBar |

#### Rutas lazy-loaded
```typescript
// app.routes.ts
export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard',      loadComponent: () => import('./features/dashboard/pages/dashboard.component') },
  { path: 'policies',       loadComponent: () => import('./features/policies/pages/policies-list.component') },
  { path: 'policies/new',   loadComponent: () => import('./features/policies/pages/policy-create.component') },
  { path: 'policies/:id',   loadComponent: () => import('./features/policies/pages/policy-detail.component') },
  { path: 'claims',         loadComponent: () => import('./features/claims/pages/claims-list.component') },
  { path: 'claims/:id',     loadComponent: () => import('./features/claims/pages/claim-detail.component') },
  { path: 'notifications',  loadComponent: () => import('./features/notifications/pages/notifications.component') },
  { path: 'login',          loadComponent: () => import('./features/auth/pages/login.component') },
];
```

### Arquitectura y Dependencias

#### Paquetes requeridos
```json
{
  "@angular/core": "^20.0.0",
  "@angular/material": "^20.0.0",
  "@angular/cdk": "^20.0.0",
  "@angular/router": "^20.0.0",
  "@angular/forms": "^20.0.0",
  "@angular/common": "^20.0.0",
  "rxjs": "^7.8.0",
  "typescript": "^5.4.0"
}
```

#### Configuración global (app.config.ts)
```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      withInterceptors([authInterceptor, idempotencyInterceptor, errorInterceptor])
    ),
    provideAnimationsAsync(),
  ],
};
```

#### Entornos
```typescript
// src/environments/environment.ts (desarrollo)
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000',
};

// src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'http://localhost:5000', // actualizar en deploy
};
```

### Mapeo HU Backend → Funcionalidad Frontend

| HU Backend (SPEC-001) | Funcionalidad Frontend |
|-----------------------|----------------------|
| HU-01: Idempotencia en creación de pólizas | `idempotencyInterceptor` + `PolicyCreatePage` con `mat-stepper` |
| HU-02: Historial de apelación (Appealed status) | `ClaimStatusTimelineComponent` muestra estado `Appealed` con color y timestamp |
| HU-03: TotalCount paginado filtrado | `mat-paginator` usa `response.totalCount` del backend (nunca cuenta local) |
| HU-04: Swagger en Docker | Enlace en footer/toolbar → `http://localhost:5000/swagger` (solo dev) |
| HU-05: GET /policies/{id}/claims | `PolicyRowActionsComponent` → `ClaimsListPage?policyId={id}` |
| HU-06: Notificaciones con Pending/Sent/Failed | `NotificationsPage` + `failedCount` Signal + mat-badge en sidenav |
| HU-06: Retry de notificación fallida | Botón retry en `NotificationRowComponent` → `PUT /notifications/{id}/retry` |

### Notas de Implementación

> 1. **Signals vs RxJS**: Usar `toSignal()` para convertir `Observable<PagedResult<T>>` de HttpClient en Signals. Los efectos secundarios (MatSnackBar al completar una acción) van en `effect()` del service.
>
> 2. **mat-stepper en creación de póliza**: El stepper tiene 3 pasos — (1) Datos del asegurado (nombre, documentId, email), (2) Tipo y cobertura (PolicyType, CoveragePeriod, InsuredAmount), (3) Confirmación. El `Idempotency-Key` se genera al abrir el formulario (no al hacer submit) para garantizar que múltiples submit del mismo formulario usen la misma key.
>
> 3. **ClaimStatusTimeline**: Usar `mat-stepper` en modo `linear=false` y `selectedIndex` calculado según el estado actual, O implementar un componente custom de timeline con `mat-divider` + `mat-icon`. El estado `Appealed` debe mostrarse entre `Rejected` y la re-evaluación.
>
> 4. **Error handling global**: El `errorInterceptor` debe interceptar 404, 422, 500 y mostrar `MatSnackBar` con mensajes en español. Los 409 (conflicto de idempotencia) no deben mostrarse como error — el backend devuelve la póliza existente y el frontend la trata como éxito.
>
> 5. **Preparación para Auth futura**: El `authInterceptor` existe pero no bloquea requests hasta que se implemente el módulo de autenticación (SPEC-003 futuro). Por ahora agrega el header solo si existe un token en `AuthService.currentUser()`.

---

## 3. LISTA DE TAREAS

> Checklist accionable. Marcar cada ítem (`[x]`) al completarlo.

### Frontend

#### Scaffold base (HU-01)
- [ ] Crear proyecto Angular 20: `ng new insuratech-frontend --standalone --routing --style=scss`
- [ ] Instalar Angular Material: `ng add @angular/material`
- [ ] Configurar `app.config.ts` con `provideRouter`, `provideHttpClient(withInterceptors(...))`, `provideAnimationsAsync`
- [ ] Crear `src/environments/environment.ts` y `environment.prod.ts` con `apiUrl`
- [ ] Crear estructura de carpetas: `core/`, `shared/`, `features/` con subcarpetas por feature
- [ ] Implementar `AppComponent` con `mat-sidenav-container` + `mat-toolbar` + `<router-outlet>`
- [ ] Implementar `app.routes.ts` con rutas lazy-loaded para los 5 features
- [ ] Crear `core/models/api-response.model.ts` con `PagedResult<T>` y `ApiError`
- [ ] Implementar `idempotencyInterceptor` — UUID v4 en header de todo POST
- [ ] Implementar `authInterceptor` — preparado para Bearer token (no-op si no hay token)
- [ ] Implementar `errorInterceptor` — MatSnackBar para errores HTTP
- [ ] Crear `shared/components/status-badge/` — chip con colores por estado
- [ ] Crear `shared/components/loading-spinner/` — overlay con mat-spinner
- [ ] Crear `shared/components/empty-state/` — estado vacío con retry
- [ ] Crear `shared/components/page-header/` — título + acción principal
- [ ] Crear `shared/pipes/status-label.pipe.ts` — traduce enum a texto legible

#### Dashboard (HU-02)
- [ ] Crear `features/dashboard/models/dashboard.model.ts` — interfaces de métricas
- [ ] Implementar `DashboardService` — llama a 3 endpoints en paralelo (`forkJoin`), expone `metrics` Signal
- [ ] Implementar `MetricCardComponent` — `mat-card` con `@Input()` title, value, icon, loading
- [ ] Implementar `DashboardPage` — grid de 3 tarjetas + accesos directos a módulos
- [ ] Registrar ruta `/dashboard` en `app.routes.ts`

#### Módulo Pólizas (HU-03)
- [ ] Crear `features/policies/models/policy.model.ts` — `PolicyDto`, `PolicyFilters`, `CreatePolicyRequest` con todas las interfaces
- [ ] Implementar `PoliciesService` — `getPolicies(filters)`, `createPolicy(req)`, `activatePolicy(id)`, `getPolicy(id)`, `getClaimsByPolicy(id)`. Signals: `policies`, `loading`, `totalCount`
- [ ] Implementar `PolicyFiltersComponent` — mat-selects para Status y Type + mat-datepicker para fechas
- [ ] Implementar `PolicyRowActionsComponent` — menú `mat-menu` con acciones contextuales por estado
- [ ] Implementar `PoliciesListPage` — mat-table + mat-paginator + filtros reactivos
- [ ] Implementar `PolicyCreatePage` — mat-stepper con 3 pasos + validación Reactive Forms
- [ ] Implementar `PolicyDetailPage` — detalle completo + botón de activación si es Pending
- [ ] Registrar rutas `/policies`, `/policies/new`, `/policies/:id`

#### Módulo Siniestros (HU-04)
- [ ] Crear `features/claims/models/claim.model.ts` — `ClaimDto`, `ClaimStatusHistoryDto`, `AppealRequest`
- [ ] Implementar `ClaimsService` — `getClaims(filters)`, `getClaim(id)`, `approveClaim(id)`, `rejectClaim(id)`, `appealClaim(id, req)`. Signals: `claims`, `loading`, `totalCount`, `selectedClaim`
- [ ] Implementar `ClaimStatusTimelineComponent` — visualización cronológica de `statusHistory`
- [ ] Implementar `ClaimActionsComponent` — botones condicionales según `claim.status`
- [ ] Implementar `AppealDialogComponent` — MatDialog con campo `observations` + `responsibleUser`
- [ ] Implementar `ClaimsListPage` — mat-table + filtros + paginación
- [ ] Implementar `ClaimDetailPage` — historial + acciones + dialog de apelación
- [ ] Registrar rutas `/claims`, `/claims/:id`

#### Módulo Notificaciones (HU-05)
- [ ] Crear `features/notifications/models/notification.model.ts` — `NotificationDto`, `NotificationFilters`
- [ ] Implementar `NotificationsService` — `getNotifications(filters)`, `retryNotification(id)`. Signals: `notifications`, `failedCount` (computed), `loading`
- [ ] Implementar `NotificationRowComponent` — fila con estado coloreado + botón retry condicional
- [ ] Implementar `NotificationsPage` — mat-table + filtro por Status + badge en sidenav
- [ ] Registrar ruta `/notifications`
- [ ] Conectar `failedCount` Signal de `NotificationsService` al `mat-badge` del SidenavComponent

#### Auth (base para futura HU)
- [ ] Implementar `AuthService` — Signals: `currentUser`, `isAuthenticated` (preparado, sin lógica de login real)
- [ ] Implementar `LoginPage` — formulario básico con `mat-form-field` (preparado para conectar en SPEC-003)

---

### Tests Frontend

#### Unitarios — Services
- [ ] `PoliciesService` — `getPolicies() makes GET with correct query params`
- [ ] `PoliciesService` — `createPolicy() includes Idempotency-Key header`
- [ ] `ClaimsService` — `appealClaim() calls PUT with correct body`
- [ ] `NotificationsService` — `retryNotification() updates failedCount signal`
- [ ] `idempotencyInterceptor` — `adds Idempotency-Key header on POST`
- [ ] `idempotencyInterceptor` — `does NOT add header on GET`

#### Unitarios — Componentes
- [ ] `StatusBadgeComponent` — renders correct color for each status value
- [ ] `MetricCardComponent` — shows loading spinner when loading=true
- [ ] `PolicyFiltersComponent` — emits filtersChange on select change
- [ ] `ClaimActionsComponent` — shows appeal button only when status=Rejected
- [ ] `NotificationRowComponent` — shows retry button only when status=Failed
- [ ] `ClaimStatusTimelineComponent` — renders all history entries in order

#### Unitarios — Pipes
- [ ] `StatusLabelPipe` — transforms PolicyStatus enum to Spanish label
- [ ] `StatusLabelPipe` — transforms NotificationStatus enum to Spanish label

---

### QA

- [ ] Verificar que mat-paginator siempre usa `totalCount` del backend (nunca `.length` del array)
- [ ] Verificar que todo POST incluye header `Idempotency-Key` (revisar Network tab)
- [ ] Verificar flujo completo: crear póliza → activar → registrar siniestro → rechazar → apelar → aprobar
- [ ] Verificar que notificación Failed muestra botón retry y cambia a Pending tras llamada exitosa
- [ ] Verificar que `mat-badge` de notificaciones se actualiza reactivamente sin recargar la página
- [ ] Validar accesibilidad básica (A11y): labels en form fields, aria-labels en iconos de acción
- [ ] Verificar que la app compila sin errores en modo producción (`ng build --configuration=production`)
- [ ] Actualizar estado spec: `status: IMPLEMENTED`
