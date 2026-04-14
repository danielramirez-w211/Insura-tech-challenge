# InsuraTech — Sistema de Gestión de Pólizas y Siniestros

## Estructura del Proyecto

```
InsuraTech/
├── Backend/                    # .NET 8 API — Clean Architecture + DDD + CQRS
├── frontend/                   # Angular 19 SPA — Arquitectura por capas
├── docs/
│   └── output/
│       ├── frontend-architecture.md   # Arquitectura detallada del frontend
│       ├── api/
│       │   └── spec-011-insured-form-ux-improvements-api.md  # API: ciudades + campos asegurado
│       └── adr/                       # Architecture Decision Records
├── podman-compose.yml
└── README.md
```

---

## Backend — Arquitectura

Este proyecto implementa **Clean Architecture** con **DDD (Domain-Driven Design)** y **CQRS** usando MediatR.
```
InsuraTech/
├── src/
│   ├── InsuraTech.Domain/          # Entidades, VOs, Domain Events, Interfaces
│   ├── InsuraTech.Application/     # Commands, Queries, DTOs, Validators
│   ├── InsuraTech.Infrastructure/  # EF Core, Repositorios, MassTransit
│   └── InsuraTech.API/             # Controllers, Middleware, Program.cs
└── tests/
    ├── InsuraTech.Domain.Tests/    # 43 unit tests de dominio
    └── InsuraTech.Application.Tests/ # 16 unit tests de casos de uso
```

## Diagrama de Capas
```
┌─────────────────────────────────┐
│           InsuraTech.API        │  ← Controllers, Middleware, Swagger
├─────────────────────────────────┤
│       InsuraTech.Application    │  ← Commands, Queries, Validators
├─────────────────────────────────┤
│         InsuraTech.Domain       │  ← Aggregates, VOs, Domain Events
├─────────────────────────────────┤
│      InsuraTech.Infrastructure  │  ← EF Core, Repos, MassTransit
└─────────────────────────────────┘
```

## Decisiones de Diseño

- **Factory Methods en Aggregates**: `Policy.Create()` y `Claim.Register()` garantizan
  que ningún objeto pueda crearse en estado inválido.
- **Value Objects inmutables**: `PolicyNumber`, `InsuredPerson` y `CoveragePeriod`
  encapsulan sus propias reglas de validación.
- **Domain Events**: Las notificaciones están completamente desacopladas mediante
  eventos de dominio publicados por los aggregates.
- **Optimistic Concurrency**: Propiedad `Version` en todos los aggregates para
  evitar condiciones de carrera.
- **Soft Delete**: Las pólizas y siniestros nunca se eliminan físicamente.
- **Idempotencia**: El endpoint de creación de pólizas acepta `Idempotency-Key`
  en el header para garantizar operaciones seguras ante reintentos.

## Reglas de Negocio Implementadas

- Una póliza recién creada tiene estado `Pending`.
- Solo se puede activar una póliza en estado `Pending`.
- Una póliza no puede cancelarse si ya está cancelada.
- La prima mensual no puede superar el 5% de la suma asegurada.
- Una póliza no puede tener más de 3 siniestros abiertos simultáneamente.
- El monto reclamado no puede superar la suma asegurada disponible.
- La fecha del siniestro debe estar dentro de la vigencia de la póliza.
- Un siniestro rechazado solo puede apelarse una vez.
- Al aprobar un siniestro, la suma asegurada disponible se decrementa.
- Si la suma asegurada llega a $0, la póliza pasa a estado `Exhausted`.

## Requisitos Previos

- .NET 8 SDK
- SQL Server o LocalDB
- Podman + podman-compose (para entorno containerizado)

## Levantar con Podman (recomendado)
```bash
git clone <repo-url>
cd InsuraTech

# Iniciar Podman Machine (solo primera vez)
podman machine init
podman machine start

# Levantar toda la solución
podman-compose up --build

# API disponible en:
# http://localhost:5000
# http://localhost:5000/swagger
```

## Levantar en desarrollo local (LocalDB)
```bash
# 1. Clonar el repositorio
git clone <repo-url>
cd InsuraTech

# 2. Configurar connection string en appsettings.Development.json
# Server=(localdb)\MSSQLLocalDB;Database=InsuraTechDb;...

# 3. Aplicar migrations
dotnet ef database update --project src/InsuraTech.Infrastructure --startup-project src/InsuraTech.API

# 4. Correr la API
dotnet run --project src/InsuraTech.API

# Swagger: https://localhost:7xxx/swagger
```

## Ejecutar Tests
```bash
# Todos los tests
dotnet test

# Solo Domain Tests (43 tests)
dotnet test InsuraTech.Domain.Tests --verbosity normal

# Solo Application Tests (16 tests)
dotnet test InsuraTech.Application.Tests --verbosity normal

# Con reporte de cobertura
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"
```

## Endpoints disponibles

### Pólizas
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/v1/policies` | Crear póliza |
| GET | `/api/v1/policies` | Listar pólizas |
| GET | `/api/v1/policies/{id}` | Obtener por ID |
| PUT | `/api/v1/policies/{id}/activate` | Activar |
| PUT | `/api/v1/policies/{id}/suspend` | Suspender |
| PUT | `/api/v1/policies/{id}/cancel` | Cancelar |
| POST | `/api/v1/policies/{id}/renew` | Renovar |

### Ciudades (SPEC-011)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/v1/cities` | Listar ciudades de Colombia (nombre, código postal, departamento) |

### Siniestros
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/v1/claims` | Registrar siniestro |
| GET | `/api/v1/claims/{id}` | Obtener por ID |
| GET | `/api/v1/claims/policy/{policyId}` | Por póliza |
| PUT | `/api/v1/claims/{id}/investigate` | Iniciar investigación |
| PUT | `/api/v1/claims/{id}/approve` | Aprobar |
| PUT | `/api/v1/claims/{id}/reject` | Rechazar |
| PUT | `/api/v1/claims/{id}/appeal` | Apelar |
| PUT | `/api/v1/claims/{id}/pay` | Registrar pago |

## Stack Tecnológico

### Backend
| Categoría | Tecnología |
|-----------|------------|
| Runtime | .NET 8 / C# 12 |
| Arquitectura | Clean Architecture + DDD |
| CQRS | MediatR 14 |
| ORM | Entity Framework Core 8 |
| Base de datos | SQL Server / LocalDB |
| Validación | FluentValidation 12 |
| Contenedores | Podman + podman-compose |
| Testing | xUnit + FluentAssertions + NSubstitute |

### Frontend
| Categoría | Tecnología |
|-----------|------------|
| Framework | Angular 19 (Standalone Components) |
| UI | Angular Material 19 |
| Estado | Angular Signals (`signal`, `computed`) |
| HTTP | `HttpClient` + interceptores |
| Formularios | Reactive Forms |
| Build | Angular CLI 19 |

---

## Frontend

### Arquitectura

El frontend sigue una **arquitectura en capas por módulo** (SPEC-005). Cada feature tiene:

```
features/<feature>/
├── core/
│   ├── models/     # tipos de dominio
│   └── service/    # servicios HTTP con signals de estado
└── ui/
    ├── blocks/     # componentes reutilizables (@Input/@Output)
    └── pages/      # componentes de página (lazy loaded)
```

> Ver [docs/output/frontend-architecture.md](docs/output/frontend-architecture.md) para la arquitectura completa.

### Módulos

| Módulo | Ruta | Descripción |
|--------|------|-------------|
| Dashboard | `/dashboard` | Métricas: pólizas activas, siniestros pendientes, notif. fallidas |
| Policies | `/policies` | Lista, creación (stepper 3 pasos), detalle y acciones |
| Claims | `/claims` | Lista con filtros, detalle con timeline y acciones de gestión |
| Notifications | `/notifications` | Centro de notificaciones con reintento de fallidas |

### Formulario de Póliza — Mejoras (SPEC-011)

| Mejora | Descripción |
|--------|-------------|
| Tipo de documento | `mat-select` con 5 opciones: CC, CE, TI, PP, RC |
| Validación por tipo | CC/TI/RC → solo dígitos, máx 10; CE/PP → alfanumérico, máx 11 |
| Separador de miles | Directiva `ThousandsSeparatorDirective` — muestra `1.127.350.242`, envía `1127350242` |
| Datepickers manuales | Ingreso manual habilitado (sin `readonly`) en todos los campos de fecha |
| Marca de vehículo | `mat-select` con 11 marcas fijas: BMW, BYD, Chevrolet, Ford, Honda, Hyundai, Jeep, Nissan, Renault, Subaru, Toyota |
| Género | `mat-select` — Masculino / Femenino |
| Ciudad | `mat-select` cargado desde `GET /api/v1/cities`; resuelve código postal y departamento automáticamente |
| Dirección | Campo de texto libre, obligatorio, máx 200 caracteres |

### Levantar el frontend

```bash
cd frontend
npm install
ng serve
# App disponible en http://localhost:4200
```

### Convenciones obligatorias

- Cada componente Angular usa **3 archivos separados** (`.ts` + `.html` + `.css`)
- Prohibido `template:` o `styles:` inline
- Standalone components con imports explícitos
- Signals para estado reactivo

## Supuestos y Decisiones

1. **Sin autenticación**: Se asume que la autenticación es manejada por un API Gateway externo.
2. **Sequence de pólizas**: Se usa el conteo total de pólizas como secuencia. En producción
   se usaría una secuencia de base de datos.
3. **MassTransit**: Configurado pero sin broker activo en desarrollo. En producción
   conecta con RabbitMQ.

## Mejoras con más tiempo

- Implementar Event Sourcing para trazabilidad completa.
- Agregar OpenTelemetry para observabilidad distribuida.
- Implementar Outbox Pattern para garantía de entrega de eventos.
- Agregar MongoDB para almacenamiento de eventos de dominio.
- Implementar Rate Limiting en endpoints críticos.
- Agregar Health Checks para monitoreo de dependencias.
- Tests de integración con base de datos real.
- Soporte Multi-tenant para múltiples aseguradoras.