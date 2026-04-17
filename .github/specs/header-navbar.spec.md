---
id: SPEC-015
status: IN_PROGRESS
feature: header-navbar
created: 2026-04-16
updated: 2026-04-16
author: spec-generator
version: "1.1"
related-specs: ["SPEC-013", "SPEC-014"]
---

# Spec: Header/Navbar por Rol + Búsqueda de Clientes + Cerrar Sesión

---

## Control de Cambios

| Versión | Fecha | Autor | Descripción |
|---------|-------|-------|-------------|
| 1.0 | 2026-04-16 | spec-generator | Creación inicial — header role-based, búsqueda de clientes para asesor, menús exclusivos por rol, logout |
| 1.1 | 2026-04-16 | ux-ui-designer | Mejora visual del header: search bar unificada (pill), clients chip, dropdown de usuario con header de marca, sidenav icon bubble |

---

## Resumen Ejecutivo

SPEC-014 entregó el sistema completo de usuarios y roles. El header actual es mínimo: solo toggle de menú, marca y un ícono `account_circle` no funcional. SPEC-015 reemplaza ese toolbar básico por un `HeaderComponent` completo que muestra contenido diferente según el rol del usuario autenticado, integra la búsqueda de clientes para asesores y añade un menú de usuario con cierre de sesión.

**Gap de backend identificado:** No existe endpoint para buscar clientes por nombre/apellido. Esta spec extiende `GET /api/v1/policies` con filtros de búsqueda sobre el asegurado y añade `GET /api/v1/policies/my-clients` para la página "Mis Clientes".

---

## 1. REQUERIMIENTOS

### 1.1 Descripción del Feature

Implementar un header funcional y role-aware que reemplace el toolbar actual. El asesor tendrá un buscador de clientes integrado en el header. El líder verá accesos rápidos a su equipo y claims pendientes. El admin verá acceso directo al panel de administración. Todos los roles tendrán un menú de usuario con logout.

### 1.2 Historias de Usuario

---

#### HU-1: Header con menú de usuario y logout (todos los roles)

> **Como** usuario autenticado (cualquier rol),
> **Quiero** ver mi información en el header con opción de cerrar sesión,
> **Para que** pueda identificarme en el sistema y salir de forma segura.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-1.1 | Ver avatar con iniciales | **Dado que** el usuario está autenticado **Cuando** ve el header **Entonces** aparece un círculo con las iniciales de su nombre y apellido |
| CA-1.2 | Abrir menú de usuario | **Cuando** hace clic en el avatar **Entonces** se despliega un menú con: nombre completo, email, badge de rol, "Mi Perfil" y "Cerrar Sesión" |
| CA-1.3 | Cerrar sesión | **Cuando** hace clic en "Cerrar Sesión" **Entonces** el estado se limpia, localStorage queda vacío y es redirigido a `/login` |
| CA-1.4 | Ir a perfil propio | **Cuando** hace clic en "Mi Perfil" **Entonces** navega a `/users/profile` |

---

#### HU-2: Header del Asesor — buscador de clientes

> **Como** Asesor,
> **Quiero** buscar clientes directamente desde el header,
> **Para que** pueda acceder rápidamente a sus pólizas sin navegar manualmente.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-2.1 | Búsqueda por nombre — resultados múltiples | **Dado que** el usuario tiene rol Advisor **Cuando** escribe 2+ caracteres en el campo "Buscar por nombre" **Entonces** tras 300ms aparece un dropdown con pólizas cuyo asegurado tenga ese texto en firstName o lastName (case-insensitive) |
| CA-2.2 | Búsqueda por nombre — sin resultados | **Cuando** la búsqueda no encuentra coincidencias **Entonces** el dropdown muestra "Sin resultados" |
| CA-2.3 | Búsqueda por documento — resultado único | **Dado que** el asesor selecciona tipo de documento y escribe el número **Cuando** hace clic en "Buscar" **Entonces** el dropdown muestra las pólizas exactas de esa persona |
| CA-2.4 | Clic en resultado | **Cuando** hace clic en un resultado del dropdown **Entonces** navega a `/policies/:id` y el dropdown se cierra |
| CA-2.5 | Link "Mis Clientes" visible para Asesor | **Dado que** el usuario tiene rol Advisor **Cuando** ve el header **Entonces** aparece el link "Mis Clientes" que navega a `/clients` |
| CA-2.6 | Búsqueda no visible para Leader/Admin | **Dado que** el usuario tiene rol Leader o Admin **Cuando** ve el header **Entonces** no aparece la barra de búsqueda ni el link "Mis Clientes" |

---

#### HU-3: Header del Líder

> **Como** Líder,
> **Quiero** accesos rápidos a mi equipo y claims pendientes en el header,
> **Para que** pueda monitorear mi equipo sin navegar por el menú lateral.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-3.1 | Link "Mi Equipo" visible | **Dado que** el usuario tiene rol Leader **Cuando** ve el header **Entonces** aparece el botón "Mi Equipo" que navega a `/users/team` |
| CA-3.2 | Badge de claims pendientes | **Cuando** existen claims en estado `PendingApproval` **Entonces** el ícono de alerta muestra un badge numérico con la cantidad; si es 0, el badge se oculta |
| CA-3.3 | Clic en badge navega a claims | **Cuando** hace clic en el ícono de alerta **Entonces** navega a `/claims` |

---

#### HU-4: Header del Admin

> **Como** Administrador,
> **Quiero** acceso directo al panel de administración desde el header,
> **Para que** pueda gestionar usuarios sin navegar por el menú lateral.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-4.1 | Link "Panel Admin" visible | **Dado que** el usuario tiene rol Admin **Cuando** ve el header **Entonces** aparece el botón "Panel Admin" que navega a `/users/admin` |

---

#### HU-5: Página "Mis Clientes" (Asesor)

> **Como** Asesor,
> **Quiero** ver una lista de todos mis clientes únicos con sus pólizas,
> **Para que** pueda gestionar mi cartera de clientes desde una vista centralizada.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-5.1 | Listar clientes únicos | **Dado que** el usuario tiene rol Advisor **Cuando** navega a `/clients` **Entonces** ve una tabla con clientes únicos (agrupados por documentId) de sus propias pólizas |
| CA-5.2 | Datos de la tabla | **Entonces** cada fila muestra: nombre completo, tipo de documento, número de documento, ciudad, cantidad de pólizas |
| CA-5.3 | Ver pólizas de un cliente | **Cuando** hace clic en "Ver Pólizas" de una fila **Entonces** navega a `/policies?documentId=X` con las pólizas filtradas |
| CA-5.4 | Acceso restringido | **Dado que** el usuario tiene rol Leader o Admin **Cuando** intenta acceder a `/clients` **Entonces** es redirigido por el roleGuard |

---

### 1.3 Reglas de Negocio

| ID | Regla | Tipo | Módulos afectados |
|----|-------|------|-------------------|
| RN-01 | La búsqueda por nombre es case-insensitive y hace match parcial (contains) | Búsqueda | Backend, Frontend |
| RN-02 | La búsqueda por documento es búsqueda exacta: documentType + documentId | Búsqueda | Backend, Frontend |
| RN-03 | `GET /policies/my-clients` solo devuelve clientes de pólizas del asesor autenticado | Datos | Backend |
| RN-04 | Los clientes se agrupan por `documentId`; no puede haber duplicados en la tabla | Datos | Backend (agregación) |
| RN-05 | La búsqueda por nombre solo se activa con 2+ caracteres y con debounce de 300ms | UX | Frontend |
| RN-06 | El badge de claims pendientes del Líder se carga una sola vez al montar el header | Rendimiento | Frontend |

---

## 2. DISEÑO

### 2.1 Cambios en el Backend

#### Extensión de `GetPoliciesQuery`

```
Archivo: Backend/src/InsuraTech.Application/Policies/Queries/GetPolicies/GetPoliciesQuery.cs
Añadir:  public string? InsuredSearch { get; init; }
         public string? InsuredDocumentType { get; init; }
```

#### Extensión de `IPolicyRepository`

```
Archivo: Backend/src/InsuraTech.Domain/Interfaces/IPolicyRepository.cs
Cambiar: GetAllAsync y CountAsync — añadir parámetros opcionales insuredSearch e insuredDocumentType
Añadir:  Task<IEnumerable<ClientSummaryProjection>> GetMyClientsAsync(Guid advisorId, CancellationToken ct)
```

#### Extensión de `PolicyRepository`

```
Archivo: Backend/src/InsuraTech.Infrastructure/Persistence/Repositories/PolicyRepository.cs
Cambiar: BuildFilter — añadir bloques para insuredSearch (regex OR) e insuredDocumentType (eq exacto)
Añadir:  GetMyClientsAsync — pipeline de agregación MongoDB:
           1. $match: createdByAdvisorId == advisorId AND isDeleted == false
           2. $group: _id = insured.documentId, firstName/$first, lastName/$first, documentType/$first, cityName/$first, count/$sum:1
           3. Mapeo a ClientSummaryProjection
```

#### Nuevo endpoint `GET /api/v1/policies/my-clients`

```
Archivo: Backend/src/InsuraTech.API/Controllers/PoliciesController.cs
Añadir:  [HttpGet("my-clients")] [Authorize(Roles = "Advisor")]
         Extrae advisorId del JWT, ejecuta GetMyClientsQuery
```

#### DTOs nuevos

```
ClientSummaryResponse.cs:
  - DocumentId, DocumentType, FirstName, LastName, CityName, PolicyCount
```

### 2.2 Nuevos Archivos Backend

| Archivo | Tipo |
|---------|------|
| `Application/Policies/Queries/GetMyClients/GetMyClientsQuery.cs` | Query |
| `Application/Policies/Queries/GetMyClients/GetMyClientsHandler.cs` | Handler |
| `Application/Policies/DTOs/ClientSummaryResponse.cs` | DTO |

### 2.3 Diseño Frontend

#### Nuevo componente `HeaderComponent`

```
frontend/src/app/shared/components/header/
  header.component.ts
  header.component.html
  header.component.css
  search-result-overlay/
    search-result-overlay.component.ts
    search-result-overlay.component.html
```

**Estructura del header (3 zonas) — v1.1 visual:**

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ ≡  InsureTech  │ ┌───────────────────────────────────────┐  👥Mis Clientes  ●│
│                │ │[👤 Nombre] [📄 Documento] │ 🔍 buscar..│                  │
│                │ └───────────────────────────────────────┘                   │
└──────────────────────────────────────────────────────────────────────────────┘
```

- **Zona izquierda**: botón toggle sidenav + logo marca (font-weight 800)
- **Zona central** (condicional por rol):
  - Advisor: **Search bar unificada** (pill único con mode tabs + input integrado) + "Mis Clientes" chip con borde naranja
  - Leader: link "Mi Equipo" + ícono alerta con badge claims pendientes
  - Admin: link "Panel Admin"
- **Zona derecha**: avatar con iniciales (gradiente naranja) + mat-menu con **header de marca** (fondo oscuro + borde naranja inferior)

#### Decisiones de Diseño Visual (v1.1)

| Elemento | Diseño anterior | Diseño v1.1 | Ley UX aplicada |
|---|---|---|---|
| Search toggle + input | Dos elementos separados | Un pill unificado (border-radius: 10px) con tabs internos | Gestalt Proximidad |
| "Mis Clientes" | Plain `mat-button` | Chip con borde naranja 1.5px, fondo sutil | Ley de Hick (jerarquía clara) |
| Dropdown usuario | Card blanca plana | Header de marca (fondo oscuro #1a1a1a + borde naranja inferior) | Trust signal + identidad de marca |
| Sidenav ítem activo | Solo border-right + color | Border-left + **icon bubble** (background naranja 15% + border-radius 8px) | Gestalt Figura/Fondo |
| Avatar | Círculo naranja plano | Gradiente lineal + ring de hover (`box-shadow: 0 0 0 2.5px #FF6B2C`) | Fitts (CTA reconocible) |

#### Nueva página `ClientsListComponent`

```
frontend/src/app/features/clients/
  core/
    models/client.model.ts
    service/clients.service.ts
  ui/pages/clients-list/
    clients-list.component.ts
    clients-list.component.html
    clients-list.component.css
```

#### Cambios en rutas

```typescript
// app.routes.ts — añadir antes del wildcard
{ path: 'clients', canActivate: [authGuard, roleGuard('Advisor')],
  loadComponent: () => import('...').then(m => m.ClientsListComponent) }
```

---

## 3. LISTA DE TAREAS

### 3.1 Backend
- [ ] `GetPoliciesQuery.cs` — añadir `InsuredSearch` e `InsuredDocumentType`
- [ ] `GetPoliciesHandler.cs` — pasar nuevos parámetros al repositorio
- [ ] `IPolicyRepository.cs` — extender firmas + añadir `GetMyClientsAsync`
- [ ] `PolicyRepository.cs` — `BuildFilter` extendido + implementar `GetMyClientsAsync`
- [ ] `ClientSummaryResponse.cs` — nuevo DTO
- [ ] `GetMyClientsQuery.cs` — nueva query
- [ ] `GetMyClientsHandler.cs` — nuevo handler
- [ ] `PoliciesController.cs` — parámetros nuevos en `GetAll` + nuevo endpoint `my-clients`

### 3.2 Frontend
- [ ] `policy.model.ts` — añadir `insuredSearch` e `insuredDocumentType` a `PolicyFilters`
- [ ] `client.model.ts` — interfaz `ClientSummary`
- [ ] `clients.service.ts` — `getMyClients(): Observable<ClientSummary[]>`
- [ ] `search-result-overlay.component.ts/.html` — componente presentacional
- [ ] `header.component.ts/.html/.css` — componente completo
- [ ] `app.component.html` — reemplazar toolbar con `<app-header>`
- [ ] `app.component.ts` — actualizar imports
- [ ] `clients-list.component.ts/.html/.css` — página "Mis Clientes"
- [ ] `app.routes.ts` — añadir ruta `/clients`

### 3.3 QA — Verificación Manual
- [ ] **QA-01:** `GET /api/v1/policies?insuredSearch=maria` devuelve coincidencias case-insensitive en firstName o lastName
- [ ] **QA-02:** `GET /api/v1/policies?insuredDocumentType=CC&documentId=12345` devuelve pólizas exactas
- [ ] **QA-03:** `GET /api/v1/policies/my-clients` con token Asesor → lista de clientes únicos
- [ ] **QA-04:** `GET /api/v1/policies/my-clients` con token Líder o Admin → HTTP 403
- [ ] **QA-05:** Frontend — login como Asesor → header muestra buscador + link "Mis Clientes"
- [ ] **QA-06:** Frontend — login como Líder → header muestra "Mi Equipo" + badge claims; sin buscador
- [ ] **QA-07:** Frontend — login como Admin → header muestra "Panel Admin"; sin buscador
- [ ] **QA-08:** Buscar por nombre en header → dropdown con resultados; clic → navega a `/policies/:id`
- [ ] **QA-09:** Avatar → dropdown muestra datos del usuario; "Cerrar Sesión" → logout y redirección a `/login`
- [ ] **QA-10:** `/clients` como Asesor → tabla de clientes; "Ver Pólizas" → `/policies?documentId=X`
- [ ] **QA-11:** `/clients` como Líder/Admin → redirect por roleGuard
- [ ] **QA-12:** Toggle del sidenav sigue funcionando con el nuevo header

---

*Fin de SPEC-015 — `header-navbar`*
