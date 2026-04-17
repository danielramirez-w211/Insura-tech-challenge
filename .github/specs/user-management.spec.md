---
id: SPEC-014
status: IN_PROGRESS
feature: user-management
created: 2026-04-15
updated: 2026-04-16
author: spec-generator
version: "1.1"
related-specs: ["SPEC-013"]
---

# Spec: Gestión de Usuarios, Perfiles y Roles

---

## Control de Cambios

| Versión | Fecha | Autor | Descripción |
|---------|-------|-------|-------------|
| 1.0 | 2026-04-15 | spec-generator | Creación inicial — CRUD de usuarios, perfiles enriquecidos, dashboard líder y flujo de aprobación de siniestros |
| 1.1 | 2026-04-16 | spec-generator | Añadir HU-7 módulo de login — conexión real al backend JWT, redirección por rol |

---

## Resumen Ejecutivo

SPEC-013 entregó la infraestructura JWT: autenticación, guards y protección de endpoints. SPEC-014 construye sobre esa base el **sistema completo de gestión de usuarios**: creación de Líderes (por Admin), creación de Asesores (por Líder con código auto-generado), perfiles enriquecidos, dashboard de métricas de ventas y el flujo de aprobación de siniestros (Asesor crea → Líder aprueba/rechaza).

**Estado actual (post SPEC-013):**

| Aspecto | Estado |
|---------|--------|
| Dominio `User`, `UserProfile`, `Role` | Creado — solo soporta autenticación |
| Endpoints de gestión de usuarios | No existen |
| `createdByAdvisorId` en `Policy` | No existe — necesario para salesCount |
| `PendingApproval` en `ClaimStatus` | No existe — claims arrancan directo en Active |
| Módulo frontend `features/users/` | No existe |

---

## 1. REQUERIMIENTOS

### 1.1 Descripción del Feature

Implementar el CRUD de los tres tipos de usuario, perfiles enriquecidos, dashboard del Líder con contador de ventas calculado en tiempo real, y el flujo formal de aprobación de siniestros que actualmente no existe (los claims se aprueban sin revisión).

### 1.2 Historias de Usuario

---

#### HU-1: Admin gestiona Líderes

> **Como** Administrador del sistema,
> **Quiero** crear, consultar y desactivar cuentas de Líderes,
> **Para que** pueda controlar quién tiene acceso como líder de asesores.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-1.1 | Crear Líder exitosamente | **Dado que** el usuario tiene rol Admin **Cuando** `POST /api/v1/users/leaders` con datos válidos **Entonces** HTTP 201 con el usuario creado; consola imprime password temporal |
| CA-1.2 | Email duplicado | **Cuando** `POST /api/v1/users/leaders` con email ya registrado **Entonces** HTTP 409 con mensaje "El email ya está registrado" |
| CA-1.3 | Listar todos los Líderes | **Dado que** el usuario tiene rol Admin **Cuando** `GET /api/v1/users/leaders` **Entonces** HTTP 200 con lista de líderes (activos e inactivos) |
| CA-1.4 | Desactivar Líder | **Dado que** el usuario tiene rol Admin **Cuando** `PATCH /api/v1/users/{id}/status` con `{ isActive: false }` **Entonces** HTTP 200 y el líder recibe 401 al intentar login |
| CA-1.5 | Rol incorrecto bloqueado | **Dado que** el usuario tiene rol Advisor o Leader **Cuando** intenta `POST /api/v1/users/leaders` **Entonces** HTTP 403 Forbidden |

---

#### HU-2: Líder crea Asesores

> **Como** Líder de equipo,
> **Quiero** crear asesores con código auto-generado y contraseña temporal en consola,
> **Para que** pueda incorporar nuevos miembros a mi equipo sin gestión manual de códigos.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-2.1 | Crear Asesor exitosamente | **Dado que** el usuario tiene rol Leader **Cuando** `POST /api/v1/users/advisors` con datos válidos **Entonces** HTTP 201; `advisorCode = "0001"` (o siguiente); consola imprime `[InsuraTech] Asesor 0001 creado. Password temporal: Xk3#m9Qr` |
| CA-2.2 | Asesor queda asignado al Líder | **Cuando** se crea el asesor **Entonces** `leaderId` del asesor coincide con el `id` del líder autenticado |
| CA-2.3 | advisorCode es secuencial y único | **Dado que** ya existe el asesor "0001" **Cuando** se crea otro asesor **Entonces** `advisorCode = "0002"` |
| CA-2.4 | Advisor no puede crear asesores | **Dado que** el usuario tiene rol Advisor **Cuando** `POST /api/v1/users/advisors` **Entonces** HTTP 403 Forbidden |

---

#### HU-3: Líder activa / desactiva sus Asesores

> **Como** Líder,
> **Quiero** activar o desactivar la cuenta de mis asesores,
> **Para que** pueda controlar el acceso de mi equipo sin eliminar sus datos.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-3.1 | Desactivar Asesor propio | **Dado que** el asesor pertenece al líder autenticado **Cuando** `PATCH /api/v1/users/{id}/status` con `{ isActive: false }` **Entonces** HTTP 200 y el asesor recibe 401 al login |
| CA-3.2 | Reactivar Asesor propio | **Cuando** `PATCH /api/v1/users/{id}/status` con `{ isActive: true }` **Entonces** HTTP 200 y el asesor puede volver a iniciar sesión |
| CA-3.3 | Líder no puede gestionar asesor ajeno | **Dado que** el asesor pertenece a otro líder **Cuando** `PATCH /api/v1/users/{id}/status` **Entonces** HTTP 403 Forbidden |

---

#### HU-4: Cualquier usuario ve y edita su propio perfil

> **Como** usuario autenticado (Advisor, Leader o Admin),
> **Quiero** ver y actualizar mis datos de perfil,
> **Para que** mi información esté siempre actualizada en el sistema.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-4.1 | Ver perfil propio | **Dado que** el usuario está autenticado **Cuando** `GET /api/v1/users/me` **Entonces** HTTP 200 con el perfil completo incluyendo `profile.*` |
| CA-4.2 | Editar perfil propio | **Cuando** `PUT /api/v1/users/me/profile` con datos válidos **Entonces** HTTP 200 con el perfil actualizado en MongoDB |
| CA-4.3 | No puede editar perfil de otro | **Cuando** el usuario intenta editar datos de otro usuario por ID **Entonces** no existe endpoint público — solo `/me` |

---

#### HU-5: Dashboard del Líder — tabla de asesores con ventas

> **Como** Líder,
> **Quiero** ver una tabla con todos mis asesores y su conteo de pólizas vendidas,
> **Para que** pueda monitorear el desempeño de mi equipo en tiempo real.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-5.1 | Lista solo muestra asesores del líder | **Dado que** el usuario tiene rol Leader **Cuando** `GET /api/v1/users/my-advisors` **Entonces** HTTP 200 con solo los asesores cuyo `leaderId` coincide |
| CA-5.2 | salesCount calculado en tiempo real | **Dado que** un asesor tiene 5 pólizas con `createdByAdvisorId = asesor.id` **Cuando** `GET /api/v1/users/my-advisors` **Entonces** ese asesor tiene `salesCount: 5` |
| CA-5.3 | salesCount = 0 para asesores sin pólizas | **Dado que** el asesor no ha creado ninguna póliza **Entonces** `salesCount: 0` |

---

#### HU-6: Flujo de aprobación de siniestros (Asesor → Líder)

> **Como** Asesor,
> **Quiero** registrar un siniestro que quede en estado "Pendiente de Aprobación",
> **Para que** mi líder lo revise antes de activarlo.
>
> **Como** Líder,
> **Quiero** aprobar o rechazar los siniestros de mis asesores,
> **Para que** tenga control sobre los siniestros gestionados en mi equipo.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-6.1 | Asesor crea claim en PendingApproval | **Dado que** el usuario tiene rol Advisor **Cuando** `POST /api/v1/claims` **Entonces** `claim.status = "PendingApproval"` y `claim.createdByAdvisorId = advisorId` |
| CA-6.2 | Líder aprueba claim | **Dado que** el claim tiene status PendingApproval **Cuando** `PATCH /api/v1/claims/{id}/approve` **Entonces** HTTP 200 y `claim.status = "Active"` |
| CA-6.3 | Líder rechaza claim con motivo | **Cuando** `PATCH /api/v1/claims/{id}/reject` con `{ "reason": "Documentación insuficiente" }` **Entonces** HTTP 200, `claim.status = "Rejected"` y `claim.rejectionReason` guardado |
| CA-6.4 | Advisor no puede aprobar/rechazar | **Dado que** el usuario tiene rol Advisor **Cuando** `PATCH /api/v1/claims/{id}/approve` **Entonces** HTTP 403 Forbidden |

---

#### HU-7: Login — acceso al sistema con redirección por rol

> **Como** usuario del sistema (Admin, Leader o Advisor),
> **Quiero** iniciar sesión con mi email y contraseña,
> **Para que** pueda acceder a las funcionalidades correspondientes a mi rol.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-7.1 | Login exitoso — Admin | **Dado que** el usuario tiene rol Admin **Cuando** ingresa credenciales válidas **Entonces** redirige a `/users/admin` |
| CA-7.2 | Login exitoso — Leader | **Dado que** el usuario tiene rol Leader **Cuando** ingresa credenciales válidas **Entonces** redirige a `/users/team` |
| CA-7.3 | Login exitoso — Advisor | **Dado que** el usuario tiene rol Advisor **Cuando** ingresa credenciales válidas **Entonces** redirige a `/dashboard` |
| CA-7.4 | Credenciales inválidas | **Cuando** email o contraseña son incorrectos **Entonces** muestra mensaje "Email o contraseña incorrectos" sin recargar la página |
| CA-7.5 | Token persiste en localStorage | **Cuando** el usuario cierra y reabre el navegador **Entonces** sigue autenticado si el token no ha expirado |
| CA-7.6 | Logout | **Cuando** el usuario hace logout **Entonces** el token se elimina y redirige a `/login` |
| CA-7.7 | Ruta protegida sin token | **Dado que** el usuario no está autenticado **Cuando** accede a cualquier ruta protegida **Entonces** redirige a `/login` |

---

### 1.3 Reglas de Negocio

| ID | Regla | Tipo | Módulos afectados |
|----|-------|------|-------------------|
| RN-01 | Solo Admin puede crear/listar/desactivar Líderes | Autorización | Backend, Frontend |
| RN-02 | Solo Leader puede crear Asesores; el asesor hereda `leaderId` del líder autenticado | Autorización | Backend |
| RN-03 | Un Líder solo puede gestionar (`/status`) asesores cuyo `leaderId` coincida con su `id` | Validación | Backend |
| RN-04 | La contraseña temporal tiene 8 caracteres: al menos 1 mayúscula, 1 dígito, 1 símbolo | Seguridad | Backend |
| RN-05 | `advisorCode` se genera con el contador `advisor_code` de la colección `counters`; formato "D4" ("0001") | Secuencia | Backend (Infrastructure) |
| RN-06 | `GET /users/my-advisors` filtra por `leaderId = id del líder autenticado`; nunca expone asesores de otros | Datos | Backend |
| RN-07 | `salesCount` = COUNT de `policies` donde `createdByAdvisorId == advisor.id`; calculado en consulta, no almacenado | Cálculo | Backend |
| RN-08 | `POST /policies` registra `createdByAdvisorId` del usuario JWT autenticado; el campo no se acepta en el body | Auditoría | Backend |
| RN-09 | Claims creados por Advisor arrancan en `PendingApproval`, no en `Active` | Flujo | Backend, Frontend |
| RN-10 | Solo Leader puede invocar `/approve` y `/reject`; el claim debe existir y no estar ya resuelto | Autorización | Backend |

---

## 2. DISEÑO

### 2.1 Cambios en el Modelo de Datos

#### Policy — añadir `CreatedByAdvisorId`

```
Archivo: Backend/src/InsuraTech.Domain/Policies/Policy.cs
Cambio:  public Guid? CreatedByAdvisorId { get; private set; }
         Constructor / factory: recibe createdByAdvisorId como parámetro opcional
```

```
Archivo: Backend/src/InsuraTech.Infrastructure/Persistence/Documents/PolicyDocument.cs
Cambio:  public string? CreatedByAdvisorId { get; set; }
```

#### Claim — `PendingApproval`, `RejectionReason`, `CreatedByAdvisorId`

```
Archivo: Backend/src/InsuraTech.Domain/Claims/ClaimStatus.cs
Añadir:  PendingApproval  (insertar antes de Active en el enum)

Archivo: Backend/src/InsuraTech.Domain/Claims/Claim.cs
Añadir:  public Guid?   CreatedByAdvisorId { get; private set; }
Añadir:  public string? RejectionReason    { get; private set; }
Añadir:  public void Approve()              → valida status == PendingApproval → cambia a Active
Añadir:  public void Reject(string reason)  → valida PendingApproval → cambia a Rejected, guarda reason
Cambio:  factory Create() → el status inicial es PendingApproval (no Active)
```

```
Archivo: Backend/src/InsuraTech.Infrastructure/Persistence/Documents/ClaimDocument.cs
Añadir:  public string? CreatedByAdvisorId { get; set; }
Añadir:  public string? RejectionReason    { get; set; }
```

**Nota sobre datos existentes:** Los documentos en `claims` creados antes de SPEC-014 tienen
`status = "Active"`. No se migran; se consideran pre-aprobados. Solo los nuevos claims usan `PendingApproval`.

---

### 2.2 API Endpoints

#### Nuevos — `UsersController`

| Método | Ruta | Rol requerido | HTTP success | Descripción |
|--------|------|--------------|-------------|-------------|
| POST | `/api/v1/users/leaders` | Admin | 201 / 409 | Crear Líder |
| GET | `/api/v1/users/leaders` | Admin | 200 | Listar todos los Líderes |
| POST | `/api/v1/users/advisors` | Leader | 201 / 409 | Crear Asesor |
| GET | `/api/v1/users/my-advisors` | Leader | 200 | Asesores del líder + salesCount |
| PATCH | `/api/v1/users/{id}/status` | Admin, Leader | 200 / 403 | Activar/desactivar usuario |
| GET | `/api/v1/users/me` | Todos | 200 | Ver perfil propio |
| PUT | `/api/v1/users/me/profile` | Todos | 200 | Editar perfil propio |

#### Nuevos — `ClaimsController`

| Método | Ruta | Rol requerido | HTTP success | Descripción |
|--------|------|--------------|-------------|-------------|
| PATCH | `/api/v1/claims/{id}/approve` | Leader | 200 / 403 / 404 | Aprobar siniestro |
| PATCH | `/api/v1/claims/{id}/reject` | Leader | 200 / 403 / 404 | Rechazar siniestro con motivo |

#### Request / Response — ejemplos

**POST `/api/v1/users/leaders` — Request**
```json
{
  "email": "leader@insuratech.com",
  "firstName": "María",
  "lastName": "López",
  "officeLocation": "Bogotá - Sede Norte",
  "workSchedule": "Lun-Vie 8am-5pm"
}
```

**POST `/api/v1/users/leaders` — Response 201**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "leader@insuratech.com",
  "role": "Leader",
  "isActive": true,
  "advisorCode": null,
  "profile": {
    "firstName": "María",
    "lastName": "López",
    "officeLocation": "Bogotá - Sede Norte",
    "workSchedule": "Lun-Vie 8am-5pm"
  }
}
```

**POST `/api/v1/users/advisors` — Request**
```json
{
  "email": "advisor@insuratech.com",
  "firstName": "Carlos",
  "lastName": "Ramos",
  "nationality": "Colombiano",
  "birthDate": "1992-06-15",
  "yearsInCompany": 2,
  "officeLocation": "Medellín - Centro",
  "workSchedule": "Lun-Sáb 9am-6pm"
}
```

**GET `/api/v1/users/my-advisors` — Response 200**
```json
[
  {
    "id": "abc123...",
    "email": "advisor@insuratech.com",
    "advisorCode": "0001",
    "isActive": true,
    "firstName": "Carlos",
    "lastName": "Ramos",
    "salesCount": 7
  }
]
```

**PATCH `/api/v1/users/{id}/status` — Request**
```json
{ "isActive": false }
```

**PATCH `/api/v1/claims/{id}/reject` — Request**
```json
{ "reason": "Documentación insuficiente — falta acta policial" }
```

---

### 2.3 Diseño Frontend

#### Nuevo módulo `features/users/`

```
frontend/src/app/features/users/
  core/
    models/
      user.model.ts              ← interfaces: UserDetail, AdvisorSummary, UpdateProfileRequest
    services/
      users.service.ts           ← HTTP: getMe(), getLeaders(), getMyAdvisors(), createLeader(),
                                         createAdvisor(), toggleStatus(), updateProfile()
  ui/
    pages/
      advisor-profile/
        advisor-profile.component.ts    ← GET /me + PUT /me/profile (formulario editable)
      leader-dashboard/
        leader-dashboard.component.ts   ← GET /my-advisors (tabla + toggle activo/inactivo)
      admin-panel/
        admin-panel.component.ts        ← GET /leaders + POST /leaders + toggle status
    blocks/
      user-form/
        user-form.component.ts          ← formulario reutilizable (crear/editar usuario)
```

#### Modelos TypeScript

```typescript
// user.model.ts

export interface UserProfile {
  firstName:       string;
  lastName:        string;
  nationality?:    string;
  birthDate?:      string;       // "YYYY-MM-DD"
  yearsInCompany?: number;
  photoUrl?:       string;
  officeLocation?: string;
  workSchedule?:   string;
}

export interface UserDetail {
  id:          string;
  email:       string;
  role:        'Admin' | 'Leader' | 'Advisor';
  isActive:    boolean;
  advisorCode: string | null;
  leaderId:    string | null;
  profile:     UserProfile;
}

export interface AdvisorSummary {
  id:          string;
  email:       string;
  advisorCode: string;
  isActive:    boolean;
  firstName:   string;
  lastName:    string;
  salesCount:  number;
}
```

#### Nuevas rutas en `app.routes.ts`

```typescript
{ path: 'users/profile',
  canActivate: [authGuard],
  loadComponent: () => import('.../advisor-profile.component').then(m => m.AdvisorProfileComponent) },

{ path: 'users/team',
  canActivate: [authGuard, roleGuard('Leader')],
  loadComponent: () => import('.../leader-dashboard.component').then(m => m.LeaderDashboardComponent) },

{ path: 'users/admin',
  canActivate: [authGuard, roleGuard('Admin')],
  loadComponent: () => import('.../admin-panel.component').then(m => m.AdminPanelComponent) },
```

#### Módulo `features/auth/` — Login con redirección por rol

El `LoginComponent` ya existe en `features/auth/pages/login/`. Se ajusta para redirigir según rol al hacer login:

| Rol | Redirección post-login |
|-----|----------------------|
| Admin | `/users/admin` |
| Leader | `/users/team` |
| Advisor | `/dashboard` |

El `AuthService` ya está implementado (JWT en localStorage, signal `currentUser`, `isAuthenticated`).
El `AuthInterceptor` ya inyecta el token en cada request y maneja 401 → logout.
El `AuthGuard` y `RoleGuard` ya protegen las rutas.

**Único cambio requerido:** en `login.component.ts`, reemplazar `router.navigate(['/dashboard'])` por lógica de redirección condicional basada en el rol devuelto en la respuesta del login.

---

#### Cambios en Claims UI

| Componente | Cambio |
|-----------|--------|
| `claims-list.component.ts` | Mostrar botones "Aprobar" / "Rechazar" solo si `role === 'Leader'` y `claim.status === 'PendingApproval'` |
| `claim-detail.component.ts` | Sección "Razón de rechazo" visible si `status === 'Rejected'` |

---

## 3. LISTA DE TAREAS

### 3.1 Backend

#### Dominio
- [ ] `Policy.cs` — añadir `Guid? CreatedByAdvisorId`; actualizar factory para recibirlo
- [ ] `ClaimStatus.cs` — añadir valor `PendingApproval` (antes de `Active`)
- [ ] `Claim.cs` — añadir `CreatedByAdvisorId`, `RejectionReason`; métodos `Approve()` y `Reject(reason)`; status inicial = `PendingApproval`

#### Infrastructure
- [ ] `PolicyDocument.cs` — añadir `string? CreatedByAdvisorId`
- [ ] `ClaimDocument.cs` — añadir `string? CreatedByAdvisorId` y `string? RejectionReason`
- [ ] `PolicyRepository` — añadir `CountByAdvisorIdAsync(Guid advisorId)` para salesCount
- [ ] `ClaimRepository` — actualizar mapeo para los nuevos campos del documento
- [ ] `UserRepository` — verificar que `GetByLeaderIdAsync` y `GetAllLeadersAsync` incluyen el perfil completo

#### Application — Commands
- [ ] `Users/Commands/CreateLeader/CreateLeaderCommand.cs` + handler — genera password temporal, imprime en consola
- [ ] `Users/Commands/CreateAdvisor/CreateAdvisorCommand.cs` + handler — llama `GetNextAdvisorCodeAsync`, imprime consola
- [ ] `Users/Commands/UpdateUserStatus/UpdateUserStatusCommand.cs` + handler — valida `leaderId` si el caller es Leader
- [ ] `Users/Commands/UpdateProfile/UpdateProfileCommand.cs` + handler — actualiza solo el perfil del usuario autenticado
- [ ] `Claims/Commands/ApproveClaim/ApproveClamCommand.cs` + handler — llama `claim.Approve()`
- [ ] `Claims/Commands/RejectClaim/RejectClaimCommand.cs` + handler — llama `claim.Reject(reason)`

#### Application — Queries
- [ ] `Users/Queries/GetMyProfile/GetMyProfileQuery.cs` + handler — `GetByIdAsync` con id del JWT claim
- [ ] `Users/Queries/GetLeaders/GetLeadersQuery.cs` + handler — `GetAllLeadersAsync`
- [ ] `Users/Queries/GetMyAdvisors/GetMyAdvisorsQuery.cs` + handler — `GetByLeaderIdAsync` + `CountByAdvisorIdAsync` por cada asesor

#### Application — DTOs
- [ ] `Users/DTOs/CreateLeaderRequest.cs`
- [ ] `Users/DTOs/CreateAdvisorRequest.cs`
- [ ] `Users/DTOs/UserResponse.cs`
- [ ] `Users/DTOs/AdvisorSummaryResponse.cs`
- [ ] `Users/DTOs/UpdateProfileRequest.cs`
- [ ] `Users/DTOs/UpdateUserStatusRequest.cs`
- [ ] `Claims/DTOs/RejectClaimRequest.cs`

#### API
- [ ] `UsersController.cs` — 7 endpoints; extraer `userId` / `role` / `leaderId` del JWT via `HttpContext.User`
- [ ] `PoliciesController.cs` — extraer `userId` del JWT y pasarlo como `createdByAdvisorId` al command de creación
- [ ] `ClaimsController.cs` — extraer `userId` en `POST /claims`; añadir endpoints `PATCH /approve` y `PATCH /reject`

---

### 3.2 Frontend

#### Models & Services
- [ ] `features/users/core/models/user.model.ts` — interfaces `UserDetail`, `AdvisorSummary`, `UpdateProfileRequest`
- [ ] `features/users/core/services/users.service.ts` — métodos HTTP para todos los endpoints de usuarios

#### Páginas
- [ ] `advisor-profile.component.ts` — carga perfil propio (`GET /me`), formulario editable, guarda cambios (`PUT /me/profile`)
- [ ] `leader-dashboard.component.ts` — tabla de asesores con `salesCount`, toggle activo/inactivo por fila
- [ ] `admin-panel.component.ts` — lista de líderes, botón "Crear Líder" (abre `user-form`), toggle activo/inactivo

#### Bloques compartidos
- [ ] `user-form.component.ts` — formulario reactivo reutilizable para crear/editar usuario (Líder o Asesor)

#### Routing & Navegación
- [ ] `app.routes.ts` — añadir rutas `/users/profile`, `/users/team`, `/users/admin`
- [ ] Sidebar / navbar existente — añadir links condicionales según rol del usuario autenticado

#### Auth (Login)
- [ ] `login.component.ts` — reemplazar redirección fija `/dashboard` por redirección condicional según rol (Admin→`/users/admin`, Leader→`/users/team`, Advisor→`/dashboard`)

#### Claims UI
- [ ] `claims-list.component.ts` — botones "Aprobar" / "Rechazar" visibles solo para Leader en claims con `PendingApproval`
- [ ] `claim-detail.component.ts` — sección "Razón de rechazo" visible si `status === 'Rejected'`

---

### 3.3 QA — Verificación Manual

- [ ] **QA-01:** Admin crea Líder → HTTP 201, consola muestra password temporal → Líder hace login con esa contraseña → funciona
- [ ] **QA-02:** Líder crea Asesor → `advisorCode = "0001"`, consola imprime mensaje → Asesor hace login → funciona
- [ ] **QA-03:** Segundo Asesor creado → `advisorCode = "0002"` (secuencial)
- [ ] **QA-04:** Líder intenta desactivar asesor de otro líder → HTTP 403
- [ ] **QA-05:** Asesor desactivado intenta login → HTTP 401 "La cuenta está desactivada"
- [ ] **QA-06:** Asesor crea póliza → `GET /policies/{id}` devuelve `createdByAdvisorId` correcto
- [ ] **QA-07:** `GET /users/my-advisors` → `salesCount` coincide con el número real de pólizas del asesor
- [ ] **QA-08:** Asesor crea claim → `status = "PendingApproval"`
- [ ] **QA-09:** Líder aprueba claim → `status = "Active"`
- [ ] **QA-10:** Líder rechaza claim con razón → `status = "Rejected"`, `rejectionReason` guardado en MongoDB
- [ ] **QA-11:** Advisor intenta `PATCH /claims/{id}/approve` → HTTP 403
- [ ] **QA-12:** Frontend — rol Leader ve botones Aprobar/Rechazar en claims con estado PendingApproval
- [ ] **QA-13:** Frontend — claim rechazado muestra la razón en el detalle
- [ ] **QA-14:** `GET /users/me` devuelve perfil completo; `PUT /users/me/profile` actualiza correctamente en MongoDB

---

*Fin de SPEC-014 — `user-management`*
