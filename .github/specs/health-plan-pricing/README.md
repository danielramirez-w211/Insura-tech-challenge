# SPEC-003 — Módulo de Planes de Salud con Cálculo Automático de Prima

> **Estado:** `APPROVED` | **Versión:** 1.0 | **Fecha:** 2026-04-06

---

## Resumen

Migración del módulo de creación de pólizas tipo **Health** desde ingreso manual de montos a un modelo basado en **planes predefinidos**. El monto asegurado se calcula automáticamente según el plan y la edad del asegurado. Los mayores de 73 años son bloqueados del flujo estándar.

---

## Planes disponibles

| Plan ID | Nombre | Valor base |
|---------|--------|-----------|
| `basic` | Básico | $300.000 |
| `salud-global` | Salud Global | $380.000 |
| `salud-premium` | Salud Premium | $450.000 |
| `salud-vida-total` | Salud Vida Total | $600.000 |

---

## Reglas de negocio — Incremento por edad

| Rango de edad | Factor | Fórmula |
|--------------|--------|---------|
| 18–35 años | 0% | `montoFinal = montoBase` |
| 36–58 años | +4% | `montoFinal = montoBase × 1.04` |
| 59–73 años | +8% | `montoFinal = montoBase × 1.08` |
| ≥ 74 años | Bloqueado | Requiere formulario de preexistencias |

**Ejemplos de cálculo:**

| Plan | Edad | Resultado |
|------|------|-----------|
| Básico ($300.000) | 25 años | $300.000 (0%) |
| Salud Premium ($450.000) | 45 años | $468.000 (+4%) |
| Salud Vida Total ($600.000) | 67 años | $648.000 (+8%) |

---

## Cambios en el esquema de datos

### Backend — Entidades modificadas

| Entidad | Cambio |
|---------|--------|
| `Policy` | Nueva propiedad `HealthPlanSelection? HealthPlan` (nullable) |
| `CreatePolicyCommand` | Nuevo campo `string? HealthPlanId` |
| `CreatePolicyRequest` (API) | Nuevo campo `string? HealthPlanId` |
| `PolicyResponse` (DTO) | Nuevo campo `HealthPlanSelectionDto? HealthPlan` (nullable) |

### Nuevos artefactos de dominio

| Artefacto | Ubicación | Descripción |
|-----------|-----------|-------------|
| `HealthPlan` (VO) | `Domain/Policies/HealthPlan/` | Catálogo de plan: id, nombre, monto base |
| `HealthPlanCatalog` | `Domain/Policies/HealthPlan/` | Clase estática con los 4 planes y `FindById()` |
| `HealthPlanSelection` (VO) | `Domain/Policies/HealthPlan/` | Selección persistida en la póliza con todos los montos |
| `HealthPlanPricingService` | `Domain/Policies/HealthPlan/` | Servicio de dominio — `Calculate(planId, birthDate, today)` |
| `UnderageInsuredException` | `Domain/Exceptions/` | Asegurado < 18 años |
| `OverageInsuredException` | `Domain/Exceptions/` | Asegurado ≥ 74 años |
| `InvalidHealthPlanException` | `Domain/Exceptions/` | Plan ID no existe |

### Nuevos artefactos de aplicación

| Artefacto | Descripción |
|-----------|-------------|
| `GetHealthPlansQuery/Handler` | Retorna catálogo de 4 planes |
| `CalculateHealthPlanQuery/Handler` | Calcula preview sin crear póliza |
| `HealthPlanDto`, `HealthPlanCalculationDto` | DTOs de respuesta |

### Nuevos endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/health-plans` | Catálogo de planes (sin auth) |
| `GET` | `/api/v1/health-plans/calculate` | Preview con `?planId=&birthDate=` |

### Cambios en endpoint existente

`POST /api/v1/policies` — Para `type = "Health"`:
- Se envía `healthPlanId` en lugar de `insuredAmount`
- El backend calcula `InsuredAmount` automáticamente
- La respuesta incluye el campo `healthPlan` con el desglose completo

### Frontend — Nuevos artefactos

| Artefacto | Ruta |
|-----------|------|
| `HealthPlansService` | `features/policies/services/health-plans.service.ts` |
| `HealthPlanSelectorComponent` | `features/policies/components/health-plan-selector/` |
| `HealthPlanPreviewComponent` | `features/policies/components/health-plan-preview/` |
| `AgeRestrictionComponent` | `features/policies/components/age-restriction/` |
| Modelos actualizados | `features/policies/models/policy.model.ts` |

`PolicyCreateComponent` modificado para detectar `type === 'Health'` y mostrar el flujo de selección de plan en lugar del campo de monto manual.

---

## Cobertura de tests

### Backend

| Suite | Casos |
|-------|-------|
| `HealthPlanPricingServiceTests` | 16 (rangos, límites, catálogo, redondeo) |
| `GetHealthPlansHandlerTests` | 4 |
| `CalculateHealthPlanHandlerTests` | 4 |
| `CreateHealthPolicyHandlerTests` | 5 (Health + regresión non-Health) |

### Frontend

| Suite | Casos |
|-------|-------|
| `HealthPlansService` | 3 (HTTP GET plans + calculate) |
| `HealthPlanSelectorComponent` | 5 (render, click, highlight) |
| `HealthPlanPreviewComponent` | 6 (null, render, montos, edad) |
| `AgeRestrictionComponent` | 3 (banner, mensaje) |

---

## Decisiones de diseño

1. **El cálculo ocurre solo en el dominio** — `HealthPlanPricingService` es estático y puro. El frontend solo llama a `/calculate` para el preview; el backend recalcula al crear la póliza con el `planId` recibido.
2. **Catálogo hardcoded** — Los 4 planes están en `HealthPlanCatalog` en el dominio. Sin endpoint de administración en esta versión.
3. **Firma `Policy.Create()` preservada** — Se agregó sobrecarga `Policy.CreateHealthPolicy()` para tipo Health sin romper la firma existente usada por tipos no-Health.
4. **Formulario de preexistencias (≥74 años)** — El bloqueo backend + frontend está implementado. El formulario en sí es alcance de SPEC-004 (pendiente de definición).

---

## Spec técnica completa

Ver: [health-plan-pricing.spec.md](../health-plan-pricing.spec.md)
