---
id: SPEC-001
status: APPROVED
feature: backend-fixes-and-notifications
created: 2026-03-31
updated: 2026-03-31
author: spec-generator
version: "1.0"
related-specs: []
---

# Spec: Correcciones críticas de backend + Módulo de Notificaciones

> **Estado:** `DRAFT` → aprobar con `status: APPROVED` antes de iniciar implementación.
> **Ciclo de vida:** DRAFT → APPROVED → IN_PROGRESS → IMPLEMENTED → DEPRECATED

---

## 1. REQUERIMIENTOS

### Descripción

Esta spec cubre los bugs críticos encontrados en el sistema de Gestión de Pólizas y Siniestros de InsuraTech, y agrega el Módulo de Notificaciones completo con sus endpoints. Las correcciones afectan las capas Domain, Application e Infrastructure. El módulo de notificaciones implementa el desacoplamiento por eventos de dominio requerido en el reto técnico, incluyendo almacenamiento con estado de envío y endpoints de consulta/reintento.

### Requerimiento de Negocio

Basado en el Reto Técnico InsuraTech Semi Senior:
- Los endpoints de creación deben ser **idempotentes** usando `Idempotency-Key` en headers.
- Las notificaciones deben almacenarse en base de datos con estado (`Pendiente`, `Enviado`, `Fallido`) para garantizar _at-least-once delivery_.
- Los domain events (`PolicyActivated`, `PolicyExpiringSoon`, `ClaimRegistered`, `ClaimStatusChanged`) deben disparar notificaciones desacopladas.
- Swagger debe estar accesible al correr con docker-compose.
- La paginación debe reflejar el total correcto de registros (con filtros aplicados).

---

### Historias de Usuario

#### HU-01: Corregir idempotencia en creación de pólizas

```
Como:        Agente de seguros que consume la API
Quiero:      Que envíos duplicados con el mismo Idempotency-Key devuelvan el mismo resultado
Para:        Garantizar que reintentos de red no dupliquen pólizas en el sistema

Prioridad:   Alta
Estimación:  XS
Dependencias: Ninguna
Capa:        Backend (Application + Infrastructure)
```

#### Criterios de Aceptación — HU-01

**Happy Path**
```gherkin
CRITERIO-1.1: Segundo request con mismo Idempotency-Key retorna póliza existente
  Dado que:  Ya existe una póliza creada con Idempotency-Key = "key-abc-123"
  Cuando:    Se hace POST /api/v1/policies con el mismo header Idempotency-Key: key-abc-123
  Entonces:  Se retorna HTTP 201 con la misma póliza (sin crear duplicado)
             Y la base de datos sigue teniendo solo una póliza con ese key
```

**Edge Case**
```gherkin
CRITERIO-1.2: Request sin Idempotency-Key genera key automático
  Dado que:  Se hace POST /api/v1/policies sin el header Idempotency-Key
  Cuando:    El controller procesa el request
  Entonces:  El controller asigna un Guid.NewGuid() como IdempotencyKey
             Y la póliza se crea correctamente
```

---

#### HU-02: Corregir historial de estado en apelación de siniestros

```
Como:        Auditor del sistema
Quiero:      Que el historial de estado de un siniestro apelado refleje ClaimStatus.Appealed
Para:        Garantizar trazabilidad correcta para auditoría regulatoria

Prioridad:   Alta
Estimación:  XS
Dependencias: Ninguna
Capa:        Backend (Domain)
```

#### Criterios de Aceptación — HU-02

**Happy Path**
```gherkin
CRITERIO-2.1: Apelar un siniestro registra estado Appealed en historial
  Dado que:  Un siniestro está en estado Rejected
  Cuando:    Se llama a claim.Appeal(responsibleUser, observations)
  Entonces:  claim.Status es ClaimStatus.Appealed
             Y el último entry de StatusHistory tiene Status = ClaimStatus.Appealed (no Approved)
```

---

#### HU-03: Corregir paginación con conteo filtrado

```
Como:        Desarrollador front-end consumiendo GET /api/v1/policies
Quiero:      Que el campo TotalCount en la respuesta refleje el total con los filtros aplicados
Para:        Mostrar correctamente el número total de páginas en la UI

Prioridad:   Media
Estimación:  XS
Dependencias: Ninguna
Capa:        Backend (Application + Infrastructure)
```

#### Criterios de Aceptación — HU-03

**Happy Path**
```gherkin
CRITERIO-3.1: TotalCount refleja registros filtrados
  Dado que:  Existen 10 pólizas (5 Active, 5 Pending)
  Cuando:    GET /api/v1/policies?status=Active&page=1&pageSize=10
  Entonces:  La respuesta tiene Items con 5 pólizas
             Y TotalCount = 5 (no 10)
```

---

#### HU-04: Swagger disponible en entorno Docker/Production

```
Como:        Evaluador del reto técnico
Quiero:      Poder acceder a http://localhost:5000/swagger al ejecutar docker-compose up
Para:        Revisar todos los endpoints documentados sin necesidad de configurar ambiente

Prioridad:   Alta
Estimación:  XS
Dependencias: Ninguna
Capa:        Backend (API)
```

#### Criterios de Aceptación — HU-04

**Happy Path**
```gherkin
CRITERIO-4.1: Swagger accesible en modo Production
  Dado que:  La app corre sin ASPNETCORE_ENVIRONMENT=Development (como en Docker)
  Cuando:    Se navega a http://localhost:5000/swagger
  Entonces:  Se muestra la UI de Swagger con todos los endpoints documentados
```

---

#### HU-05: Corregir ruta de siniestros por póliza

```
Como:        Consumidor de la API
Quiero:      Consultar siniestros de una póliza en GET /api/v1/policies/{id}/claims
Para:        Seguir el contrato de API especificado en el reto técnico

Prioridad:   Alta
Estimación:  XS
Dependencias: Ninguna
Capa:        Backend (API)
```

#### Criterios de Aceptación — HU-05

**Happy Path**
```gherkin
CRITERIO-5.1: Ruta correcta para claims por póliza
  Dado que:  Existe una póliza con siniestros registrados
  Cuando:    GET /api/v1/policies/{id}/claims
  Entonces:  HTTP 200 con lista de siniestros de esa póliza
```

---

#### HU-06: Módulo de Notificaciones con eventos de dominio

```
Como:        Sistema de InsuraTech
Quiero:      Almacenar y gestionar notificaciones generadas por eventos de dominio
Para:        Garantizar entrega at-least-once y dar visibilidad al estado de notificaciones

Prioridad:   Alta
Estimación:  L
Dependencias: HU-01 (pólizas activas generan PolicyActivated)
Capa:        Backend (Domain + Application + Infrastructure + API)
```

#### Criterios de Aceptación — HU-06

**Happy Path — PolicyActivated**
```gherkin
CRITERIO-6.1: Activar póliza genera notificación almacenada
  Dado que:  Existe una póliza en estado Pending
  Cuando:    PUT /api/v1/policies/{id}/activate
  Entonces:  Se crea una Notification en DB con:
             - Event = "PolicyActivated"
             - RecipientId = policy.Insured.DocumentId
             - Status = Pending
             - Subject = "Tu póliza {number} ha sido activada"
```

**Happy Path — ClaimRegistered**
```gherkin
CRITERIO-6.2: Registrar siniestro genera notificación al asegurado
  Dado que:  Existe una póliza activa
  Cuando:    POST /api/v1/claims
  Entonces:  Se crea una Notification con Event = "ClaimRegistered", Status = Pending
```

**Happy Path — ClaimStatusChanged**
```gherkin
CRITERIO-6.3: Cambio de estado de siniestro genera notificación
  Dado que:  Un siniestro cambia de estado (aprobado, rechazado, pagado, etc.)
  Cuando:    Se ejecuta cualquier transición de estado del siniestro
  Entonces:  Se crea una Notification con Event = "ClaimStatusChanged"
             Y Status = Pending
```

**Error Path**
```gherkin
CRITERIO-6.4: Notificación fallida puede reintentarse
  Dado que:  Existe una notificación con Status = Failed
  Cuando:    PUT /api/v1/notifications/{id}/retry
  Entonces:  HTTP 200 y la notificación vuelve a Status = Pending
             Y se intenta el reenvío
```

**Edge Case**
```gherkin
CRITERIO-6.5: Listar notificaciones filtrables por estado
  Dado que:  Existen notificaciones con distintos estados
  Cuando:    GET /api/v1/notifications?status=Pending
  Entonces:  Solo se devuelven notificaciones con Status = Pending
```

---

### Reglas de Negocio

1. **Idempotencia:** El `IdempotencyKey` de una póliza es un shadow property en EF. Debe asignarse mediante `_context.Entry(policy).Property("IdempotencyKey").CurrentValue` antes del SaveChanges, o bien en el handler via `UnitOfWork`.
2. **Historial de apelación:** `Claim.Appeal()` debe registrar `ClaimStatus.Appealed` (no `Approved`) en `ClaimStatusHistory`.
3. **Conteo paginado:** `IPolicyRepository` debe exponer `CountAsync(filters)` con los mismos filtros que `GetAllAsync`.
4. **Swagger global:** `app.UseSwagger()` y `app.UseSwaggerUI()` deben estar fuera del bloque `IsDevelopment`.
5. **Notificaciones:** Se crean síncronamente en la misma transacción como parte del manejo del domain event via MediatR `INotificationHandler<TEvent>`. No requiere RabbitMQ para la creación — solo para el envío futuro.
6. **Estado de notificaciones:** `Pending` → `Sent` (envío exitoso) | `Pending` → `Failed` (error). El retry vuelve a `Pending`.
7. **Soft delete**: La entidad `Notification` también debe tener `IsDeleted`.
8. **Encapsulamiento:** Los campos de `ApproveClaimHandler` deben ser `private readonly`.

---

## 2. DISEÑO

### Modelos de Datos

#### Entidades afectadas
| Entidad | Almacén | Cambios | Descripción |
|---------|---------|---------|-------------|
| `Policy` | tabla `Polices` | shadow property `IdempotencyKey` ya existe en config EF; se debe asignar el valor al crear | Póliza de seguro |
| `PolicyStatusHistory` | tabla existente | sin cambios | Historial de estados |
| `Claim` | tabla `Claims` | fix en `Appeal()`: historial usa `Appealed` | Siniestro |
| `Notification` | tabla nueva `Notifications` | nueva entidad | Notificación de dominio |

#### Campos del modelo `Notification`
| Campo | Tipo | Obligatorio | Validación | Descripción |
|-------|------|-------------|------------|-------------|
| `Id` | Guid | sí | auto-generado | Identificador único |
| `Event` | string(100) | sí | valor de enum `NotificationEvent` | Tipo de evento origen |
| `RecipientId` | string(100) | sí | no vacío | DocumentId o email del destinatario |
| `RecipientName` | string(200) | sí | no vacío | Nombre del destinatario |
| `Subject` | string(300) | sí | no vacío | Asunto de la notificación |
| `Body` | string(2000) | sí | no vacío | Cuerpo del mensaje |
| `Status` | string(20) | sí | enum `NotificationStatus` | Estado de envío |
| `CorrelationId` | string(100) | no | - | ID de correlación del request origen |
| `RetryCount` | int | sí | default 0 | Número de intentos de envío |
| `LastAttemptAt` | DateTime? | no | UTC | Timestamp del último intento |
| `CreatedAt` | DateTime | sí | auto-generado UTC | Timestamp creación |
| `UpdatedAt` | DateTime | sí | auto-generado UTC | Timestamp actualización |
| `IsDeleted` | bool | sí | default false | Soft delete |

#### Enums nuevos
```csharp
// Domain
public enum NotificationStatus { Pending = 1, Sent = 2, Failed = 3 }
public enum NotificationEvent
{
    PolicyActivated = 1,
    PolicyExpiringSoon = 2,
    ClaimRegistered = 3,
    ClaimStatusChanged = 4
}
```

#### Índices / Constraints
- `Notifications.Status` — índice simple (filtro frecuente por estado)
- `Notifications.CreatedAt` — índice DESC (ordenamiento por recientes)
- `Notifications.RecipientId` — índice (búsqueda por destinatario futura)

---

### API Endpoints

#### GET /api/v1/notifications
- **Descripción**: Lista notificaciones con filtro opcional por estado
- **Query params**: `status` (opcional, enum string), `page` (default 1), `pageSize` (default 10)
- **Response 200**:
  ```json
  {
    "items": [{
      "id": "uuid",
      "event": "PolicyActivated",
      "recipientId": "123456789",
      "recipientName": "John Doe",
      "subject": "Tu póliza POL-2026-00000001 ha sido activada",
      "body": "...",
      "status": "Pending",
      "retryCount": 0,
      "createdAt": "2026-03-31T12:00:00Z"
    }],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

#### PUT /api/v1/notifications/{id}/retry
- **Descripción**: Reintenta el envío de una notificación fallida
- **Response 200**: `NotificationResponse` con Status = Pending
- **Response 404**: notificación no encontrada
- **Response 422**: la notificación no está en estado Failed

---

### Correcciones de rutas existentes

#### GET /api/v1/policies/{id}/claims  ← MOVER desde ClaimsController
- Actualmente está en: `GET /api/v1/claims/policy/{policyId}` (ClaimsController)
- Debe moverse a `PoliciesController` como sub-recurso
- Mantener el endpoint anterior como deprecated (opcional) o eliminarlo

---

### Arquitectura y Dependencias

#### Nuevos artefactos

**Domain Layer (`InsuraTech.Domain`):**
- `Notifications/Notification.cs` — Aggregate root con factory `Notification.Create()`
- `Notifications/NotificationStatus.cs` — enum
- `Notifications/NotificationEvent.cs` — enum
- `Interfaces/INotificationRepository.cs` — contrato del repositorio

**Application Layer (`InsuraTech.Application`):**
- `Notifications/EventHandlers/PolicyActivatedHandler.cs` — `INotificationHandler<PolicyActivatedEvent>`
- `Notifications/EventHandlers/ClaimRegisteredHandler.cs` — `INotificationHandler<ClaimRegisteredEvent>`
- `Notifications/EventHandlers/ClaimStatusChangedHandler.cs` — `INotificationHandler<ClaimStatusChangedEvent>`
- `Notifications/Queries/GetNotifications/GetNotificationsQuery.cs` + Handler
- `Notifications/Commands/RetryNotification/RetryNotificationCommand.cs` + Handler
- `Notifications/DTOs/NotificationResponse.cs` + `NotificationMappingExtensions.cs`

**Infrastructure Layer (`InsuraTech.Infrastructure`):**
- `Persistence/Configurations/NotificationConfiguration.cs`
- `Persistence/Repositories/NotificationRepository.cs`
- Nueva migración EF Core: `AddNotificationsTable`
- Registrar `INotificationRepository → NotificationRepository` en `DependencyInjection.cs`

**API Layer (`InsuraTech.API`):**
- `Controllers/NotificationsController.cs`

#### Cambios en archivos existentes

| Archivo | Cambio |
|---------|--------|
| `InsuraTech.Domain/Claims/Claim.cs:122` | `AddStatusHistory(ClaimStatus.Appealed, ...)` (fix typo) |
| `InsuraTech.Application/Policies/Commands/CreatePolicy/CreatePolicyHandler.cs` | Asignar `IdempotencyKey` shadow property tras `AddAsync` |
| `InsuraTech.Domain/Interfaces/IPolicyRepository.cs` | Agregar `CountAsync(filters)` con parámetros de filtro |
| `InsuraTech.Infrastructure/Persistence/Repositories/PolicyRepository.cs` | Implementar `CountAsync(filters)` filtrado + corregir encapsulamiento |
| `InsuraTech.Application/Claims/Commands/ApproveClaim/ApproveClaimHandler.cs` | Cambiar campos `public readonly` a `private readonly` |
| `InsuraTech.API/Program.cs` | Mover `UseSwagger()` fuera del bloque `IsDevelopment` |
| `InsuraTech.API/Controllers/PoliciesController.cs` | Agregar `GET {id}/claims` |
| `InsuraTech.API/Controllers/ClaimsController.cs` | Eliminar o deprecar `GET policy/{policyId}` |
| `InsuraTech.Infrastructure/DependencyInjection.cs` | Registrar `INotificationRepository` |
| `InsuraTech.Application/DependencyInjection.cs` | Registrar MediatR notification handlers |

#### Paquetes nuevos
- Ninguno nuevo. MassTransit ya está instalado (no se configura en esta spec; el envío real queda pendiente — las notificaciones se crean en BD y el status queda `Pending`).

#### Integración con MediatR para domain events
Los domain events actuales (`IDomainEvent`) **no implementan** `INotification` de MediatR, por lo que los handlers no pueden suscribirse con `INotificationHandler<T>`.

**Solución:** Hacer que `IDomainEvent` extienda `INotification`, o crear un `DomainEventDispatcher` que publique los eventos via `IPublisher` (MediatR) en `SaveChangesAsync` del DbContext — antes de limpiarlos.

**Enfoque recomendado — DomainEventDispatcher en DbContext:**
```csharp
// En InsuraTechDbContext.SaveChangesAsync — ANTES de ClearDomainEvent()
var result = await base.SaveChangesAsync(cancellationToken);
foreach (var aggregate in aggregates)
{
    foreach (var domainEvent in aggregate.DomainEvents)
        await _publisher.Publish(domainEvent, cancellationToken); // IPublisher de MediatR
    aggregate.ClearDomainEvent();
}
```
El `DbContext` recibirá `IPublisher` por constructor injection.

---

### Notas de Implementación

> 1. El `IdempotencyKey` es un shadow property de EF. Para asignarlo desde el handler, se debe exponer en el repositorio un método `SetIdempotencyKey(policy, key)` que use `_context.Entry(policy).Property("IdempotencyKey").CurrentValue = key`, o bien agregar `IdempotencyKey` como propiedad privada de `Policy` accesible solo a través de EF.
>
> 2. Los `Notification` handlers deben ser idempotentes: si ya existe una notificación para el mismo evento + policy/claim + timestamp reciente, no crear duplicado.
>
> 3. La entidad `Notification` no debe exponer lógica de negocio compleja — es un registro de auditoría de notificación. Solo el cambio de `Status` se hace mediante métodos.
>
> 4. El `INotificationService` ya definido en Application puede reutilizarse como abstracción del envío real (stub/noop en Infrastructure por ahora).
>
> 5. Los typos en mensajes de error deben corregirse como parte de este fix: "Suspenson" → "Suspension", "resgistered" → "registered", "_domaninEvents" → "_domainEvents", "peirod" → "period".

---

## 3. LISTA DE TAREAS

> Checklist accionable para todos los agentes. Marcar cada ítem (`[x]`) al completarlo.

### Backend

#### Correcciones de bugs

- [ ] `Claim.cs:122` — Cambiar `ClaimStatus.Approved` → `ClaimStatus.Appealed` en `Appeal()`
- [ ] `ApproveClaimHandler.cs` — Cambiar campos `public readonly` → `private readonly`
- [ ] `Program.cs` — Mover `UseSwagger()` y `UseSwaggerUI()` fuera de `IsDevelopment`
- [ ] `IPolicyRepository.cs` — Agregar overload `CountAsync(status, type, documentId, startDate, endDate)`
- [ ] `PolicyRepository.cs` — Implementar `CountAsync` con filtros equivalentes a `GetAllAsync`
- [ ] `GetPoliciesHandler.cs` — Usar `CountAsync(filters)` en lugar de `CountAsync()` para `TotalCount`
- [ ] `CreatePolicyHandler.cs` — Asignar shadow property `IdempotencyKey` al entity tras `AddAsync`
- [ ] Typos: corregir "Suspenson", "resgistered", "_domaninEvents", "peirod"

#### Módulo de Notificaciones — Domain

- [ ] Crear `InsuraTech.Domain/Notifications/NotificationStatus.cs` (enum)
- [ ] Crear `InsuraTech.Domain/Notifications/NotificationEvent.cs` (enum)
- [ ] Crear `InsuraTech.Domain/Notifications/Notification.cs` (aggregate root con factory `Create()` y métodos `MarkAsSent()`, `MarkAsFailed()`, `ResetForRetry()`)
- [ ] Crear `InsuraTech.Domain/Interfaces/INotificationRepository.cs`
- [ ] Hacer que `IDomainEvent` extienda `MediatR.INotification`

#### Módulo de Notificaciones — Application

- [ ] Crear `NotificationResponse.cs` + `NotificationMappingExtensions.cs`
- [ ] Crear `GetNotificationsQuery.cs` + `GetNotificationsHandler.cs`
- [ ] Crear `RetryNotificationCommand.cs` + `RetryNotificationHandler.cs`
- [ ] Crear `PolicyActivatedHandler.cs` — `INotificationHandler<PolicyActivatedEvent>`
- [ ] Crear `ClaimRegisteredHandler.cs` — `INotificationHandler<ClaimRegisteredEvent>`
- [ ] Crear `ClaimStatusChangedHandler.cs` — `INotificationHandler<ClaimStatusChangedEvent>`
- [ ] Registrar handlers en `DependencyInjection.cs` de Application

#### Módulo de Notificaciones — Infrastructure

- [ ] Crear `NotificationConfiguration.cs` (EF fluent config)
- [ ] Crear `NotificationRepository.cs`
- [ ] Agregar `IPublisher` (MediatR) al `InsuraTechDbContext` y despachar domain events antes de limpiarlos
- [ ] Registrar `INotificationRepository → NotificationRepository` en `DependencyInjection.cs`
- [ ] Crear migración EF: `dotnet ef migrations add AddNotificationsTable`
- [ ] Agregar `DbSet<Notification>` al `InsuraTechDbContext`

#### API

- [ ] Crear `NotificationsController.cs` con `GET /api/v1/notifications` y `PUT /{id}/retry`
- [ ] Agregar `GET {id}/claims` en `PoliciesController.cs` (delegando a `GetClaimsByPolicyQuery`)
- [ ] Eliminar o deprecar `GET /api/v1/claims/policy/{policyId}` en `ClaimsController.cs`

---

### Tests Backend

#### Correcciones de tests existentes
- [ ] `ClaimTests.cs` — Verificar que `Appeal_WhenRejected_ShouldChangeStatusToAppealed` también valide `StatusHistory.Last().Status == Appealed` (actualmente no lo hace)

#### Nuevos tests de dominio (Domain.Tests)
- [ ] `NotificationTests.cs` — `Create_WithValidData_ShouldHavePendingStatus`
- [ ] `NotificationTests.cs` — `MarkAsSent_WhenPending_ShouldChangeTalStatusToSent`
- [ ] `NotificationTests.cs` — `MarkAsFailed_WhenPending_ShouldChangeStatusToFailed`
- [ ] `NotificationTests.cs` — `ResetForRetry_WhenFailed_ShouldReturnToPending`
- [ ] `NotificationTests.cs` — `ResetForRetry_WhenNotFailed_ShouldThrowInvalidStateException`

#### Nuevos tests de application (Application.Tests)
- [ ] `GetNotificationsHandlerTests.cs` — retorna lista paginada filtrada por status
- [ ] `RetryNotificationHandlerTests.cs` — happy path y error cuando no está en Failed
- [ ] `PolicyActivatedHandlerTests.cs` — crea notificación al recibir evento

#### Integration Tests — proyecto nuevo
- [ ] Crear proyecto `InsuraTech.Integration.Tests` (xUnit + EF InMemory o SQLite + WebApplicationFactory)
- [ ] `FullFlow_CreateActivateClaim_Approve_ShouldDeductInsuredAmount`
- [ ] `FullFlow_RejectClaim_Appeal_Approve_ShouldWork`
- [ ] `FullFlow_Idempotency_SameKey_ShouldNotDuplicate`
- [ ] `FullFlow_CreatePolicy_ShouldGenerateNotification_OnActivate`
- [ ] `FullFlow_ClaimLimit_FourthClaim_ShouldReturn422`

---

### QA

- [ ] Ejecutar skill `/gherkin-case-generator` → criterios 1.1, 2.1, 3.1, 4.1, 5.1, 6.1..6.5
- [ ] Ejecutar skill `/risk-identifier` → clasificación ASD de riesgos
- [ ] Revisar cobertura de Domain + Application ≥ 80%
- [ ] Validar que docker-compose up levanta sin errores y swagger es accesible
- [ ] Actualizar estado spec: `status: IMPLEMENTED`
