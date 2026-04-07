# SPEC-004 — Travel Rating Engine: Documentación de Implementación

> **Estado:** IMPLEMENTED  
> **Fecha:** 2026-04-07  
> **Relacionado:** [travel-rating-engine.spec.md](../travel-rating-engine.spec.md)

---

## 1. Resumen de Cambios

La SPEC-004 migra el módulo de pólizas de Viaje de entrada manual de montos a un **rating engine determinístico**. El sistema ahora calcula automáticamente el valor de la póliza según el tipo de viaje (Nacional / Internacional), la duración en días y la TRM vigente (para Internacional), bloqueando la edición manual de precios.

---

## 2. Artefactos Creados

### 2.1 Backend — Domain

| Archivo | Descripción |
|---|---|
| `Domain/Exceptions/TravelDurationInvalidException.cs` | `TRAVEL_DURATION_INVALID` — días < 1 |
| `Domain/Exceptions/TravelDurationExceededException.cs` | `TRAVEL_DURATION_EXCEEDED` — días > 180 |
| `Domain/Exceptions/TrmUnavailableException.cs` | `TRM_UNAVAILABLE` — API Socrata no disponible |
| `Domain/Exceptions/InvalidContinentException.cs` | `INVALID_CONTINENT` — continente fuera del enum |
| `Domain/Policies/TravelPlan/TripType.cs` | Enum: `Nacional = 1`, `Internacional = 2` |
| `Domain/Policies/TravelPlan/Continent.cs` | Enum: `America`, `Europe`, `Africa`, `Asia`, `Oceania` |
| `Domain/Policies/TravelPlan/TravelPlanSelection.cs` | Value Object: snapshot inmutable del cálculo |
| `Domain/Policies/TravelPlan/TravelRatingService.cs` | Domain Service estático — lógica de cálculo pura |

### 2.2 Backend — Application

| Archivo | Descripción |
|---|---|
| `Application/Common/Interfaces/ITrmService.cs` | Interfaz + record `TrmResult(decimal ValueCop, DateOnly Date)` |
| `Application/TravelPlans/DTOs/TravelPlanDto.cs` | `TravelPlanCalculationDto` + `TravelPlanSelectionDto` |
| `Application/TravelPlans/Queries/CalculateTravelPlan/CalculateTravelPlanQuery.cs` | Query MediatR |
| `Application/TravelPlans/Queries/CalculateTravelPlan/CalculateTravelPlanHandler.cs` | Obtiene TRM si Internacional, llama a `TravelRatingService` |

### 2.3 Backend — Infrastructure

| Archivo | Descripción |
|---|---|
| `Infrastructure/ExternalServices/TrmService.cs` | HTTP + caché 1h. Endpoint: `datos.gov.co/resource/mcec-87by.json` |

### 2.4 Backend — API

| Archivo | Descripción |
|---|---|
| `API/Controllers/TravelPlansController.cs` | `GET /api/v1/travel-plans/calculate` |

### 2.5 Frontend

| Archivo | Descripción |
|---|---|
| `services/travel-plans.service.ts` | `calculate(tripType, days, continent?)` |
| `components/travel-plan-preview/travel-plan-preview.component.ts` | Desglose del cálculo en tiempo real |
| `components/travel-duration-restriction/travel-duration-restriction.component.ts` | Banner de error días > 180 |

---

## 3. Archivos Modificados

### Backend

| Archivo | Cambio |
|---|---|
| `Domain/Policies/Policy.cs` | Propiedad `TravelPlan?` + factory `CreateTravelPolicy()` |
| `Application/Policies/Commands/CreatePolicy/CreatePolicyCommand.cs` | Campos: `TripType?`, `Continent?`, `DurationDays?` |
| `Application/Policies/Commands/CreatePolicy/CreatePolicyHandler.cs` | Inyecta `ITrmService`; rama condicional para Travel |
| `Application/Policies/DTOs/PolicyResponse.cs` | `TravelPlanSelectionDto? TravelPlan` |
| `Application/Policies/DTOs/PolicyMappingExtensions.cs` | Mapeo de `policy.TravelPlan` → DTO |
| `API/Models/CreatePolicyRequest.cs` | Campos: `TripType?`, `Continent?`, `DurationDays?` |
| `API/Controllers/PoliciesController.cs` | Pasa campos Travel al comando |
| `API/Middleware/ExceptionHandlingMiddleware.cs` | Casos: `TravelDurationExceededException` (422), `TrmUnavailableException` (503), `InvalidContinentException` (400) |
| `Infrastructure/Persistence/MongoDbContext.cs` | `BsonClassMap` para `TravelPlanSelection` |
| `Infrastructure/DependencyInjection.cs` | `AddMemoryCache()`, `AddHttpClient<ITrmService, TrmService>()` |
| `Infrastructure/InsuraTech.Infrastructure.csproj` | Paquetes: `Microsoft.Extensions.Caching.Memory`, `Microsoft.Extensions.Http` |

### Frontend

| Archivo | Cambio |
|---|---|
| `models/policy.model.ts` | Tipos: `TripType`, `Continent`, `TravelPlanCalculationDto`, `TravelPlanSelectionDto`; campos Travel en `CreatePolicyRequest` |
| `pages/policy-create.component.ts` | Signals Travel, flujo condicional, `submit()` actualizado |

---

## 4. Lógica de Cálculo Implementada

### Nacional

```
effectiveDays = min(durationDays, 30)
total         = 2,200 + (effectiveDays - 1) × 1,200
```

| Días | Total COP |
|------|-----------|
| 1    | $2,200    |
| 10   | $13,000   |
| 30   | $37,000   |
| 45–180 | $37,000 (techo) |

### Internacional

```
totalUSD = 30 + (durationDays - 1) × 5
totalCOP = round(totalUSD × TRM, 0, AwayFromZero)
```

| Días | USD   | COP (TRM $4,200) |
|------|-------|-----------------|
| 1    | $30   | $126,000        |
| 10   | $75   | $315,000        |
| 180  | $925  | $3,885,000      |

---

## 5. Endpoints Nuevos

```
GET /api/v1/travel-plans/calculate
  ?tripType=Nacional|Internacional
  &durationDays=10
  &continent=Europe   (solo si Internacional)

→ TravelPlanCalculationDto
```

---

## 6. Integración TRM

- **URL:** `https://www.datos.gov.co/resource/mcec-87by.json?$limit=1&$order=vigenciadesde DESC`
- **Caché:** 1 hora en `IMemoryCache`
- **Fallo:** retorna HTTP 503 con `code: TRM_UNAVAILABLE`
- **Normalización de `valor`:** maneja `"4215.24"` (punto) y `"4.215,24"` (coma europea)

---

## 7. Cobertura de Tests

| Proyecto | Tests nuevos | Total |
|---|---|---|
| `InsuraTech.Domain.Tests` | 20 | 95 ✅ |
| `InsuraTech.Application.Tests` | 9 | 84 ✅ |
| Frontend (Karma) | 12 | — |

**Total backend: 179/179 pasando.**

---

## 8. Decisiones de Diseño

| Decisión | Justificación |
|---|---|
| TRM obtenida en Application layer, no en Domain | El Domain Service debe ser puro (sin dependencias de infraestructura). La TRM se pasa como parámetro a `TravelRatingService.Calculate()`. |
| `CoveragePeriod` con constructor directo para Travel | `CoveragePeriod.Create()` exige mínimo 30 días (regla de otros productos). Para Travel (1–29 días), se usa el constructor sin validación de mínimo. La duración es validada exclusivamente por `TravelRatingService`. |
| `TravelPlanSelection` con constructor `public` | Requerido por BSON `MapCreator` para deserialización desde MongoDB. |
| Caché TRM de 1 hora | Evita saturar la API del gobierno en cargas altas; la TRM es diaria por definición. |
| `effectiveDays = min(days, 30)` para Nacional | Techo de cálculo: más de 30 días no incrementa el precio, pero la cobertura sigue vigente hasta 180. |
