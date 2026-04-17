---
id: SPEC-008
status: IN_PROGRESS
feature: health-premium-fixed-period
created: 2026-04-09
updated: 2026-04-09
author: spec-generator
version: "1.1"
related-specs: ["SPEC-003", "SPEC-007"]
---

# Spec: Reingeniería del Cálculo de Prima — Periodo Fijo de 12 Meses

---

## Control de Cambios

| Versión | Fecha | Autor | Descripción |
|---------|-------|-------|-------------|
| 1.0 | 2026-04-09 | spec-generator | Creación inicial — transición de periodo manual a periodo fijo |
| 1.1 | 2026-04-09 | spec-generator | Eliminación de la regla `monthlyPremium ≤ 5% insuredAmount` en toda la plataforma. Motivo: Life usará planes de precio fijo (similar a Health); Vehicle y Home calcularán primas según coberturas seleccionadas (specs futuras). Solo permanece `monthlyPremium > 0`. Método `ValidateFinancials` eliminado de `Policy.cs`. |

---

## Resumen Ejecutivo

El módulo de cotización de pólizas de Salud presenta una falla de negocio: solicita al usuario un rango de fechas (inicio y fin de cobertura) cuando la regla de negocio exige un periodo fijo de **12 meses** para todas las pólizas de tipo Salud. Esto genera inconsistencias en el cálculo de la prima mensual (`monthlyPremium = totalAnual / 12`) y confusión en el usuario al ingresar datos que el sistema debería derivar automáticamente.

**Transición de modelo:**

| Aspecto | Modelo Actual (Manual) | Modelo Propuesto (Fijo) |
|---------|----------------------|------------------------|
| Periodo de cobertura | Usuario ingresa `startDate` + `endDate` | Usuario ingresa solo `startDate`; `endDate = startDate + 12 meses` |
| `monthlyPremium` | Calculado ad-hoc o ingresado manualmente | Siempre = `totalAnual / 12` |
| `durationDays` | Calculado en frontend por diferencia de fechas | Constante: 365 días (calculado en backend) |
| Escalabilidad | Lógica dispersa en el componente | Servicio de cotización centralizado (`QuotationService`) |

Esta reingeniería afecta el flujo de creación de pólizas en el frontend (`PolicyCreateComponent`) y el endpoint de cálculo de salud en el backend (`/api/v1/health-plans/calculate`). Los módulos de Vida y Vehículo heredarán la misma lógica en iteraciones posteriores sin duplicar código.

---

## 1. REQUERIMIENTOS

### 1.1 Descripción del Feature

Estandarizar el cálculo de cotización de pólizas de Salud a un **periodo fijo de 12 meses**, eliminando los campos de selección de rango de fechas del formulario y sustituyéndolos por un único campo obligatorio: **Fecha de Inicio de la Póliza**. La fecha de fin y la prima mensual se calculan automáticamente en el backend.

### 1.2 Historias de Usuario

---

#### HU-1: Cálculo Automático de Prima Mensual (Health)

> **Como** agente de seguros,  
> **Quiero** que el sistema calcule automáticamente la prima mensual dividiendo el total anual entre 12,  
> **Para que** no deba ingresar manualmente una prima que puede ser inconsistente con el plan seleccionado.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-1.1 | Prima calculada correctamente | **Dado que** el usuario seleccionó un plan de salud con `finalAmount = COP 1.200.000` **Cuando** confirma la cotización **Entonces** el sistema registra `monthlyPremium = 100.000` y `coveragePeriod.endDate = startDate + 12 meses` |
| CA-1.2 | El usuario no puede modificar la prima | **Dado que** el usuario está en el paso de Cobertura **Cuando** el tipo de póliza es Health **Entonces** el campo `monthlyPremium` no es visible ni editable en el formulario |
| CA-1.3 | El endpoint de cálculo retorna prima mensual | **Dado que** se realiza `GET /api/v1/health-plans/calculate?planId=X&birthDate=Y` **Cuando** el plan existe y la fecha es válida **Entonces** la respuesta incluye `monthlyPremium = finalAmount / 12` |
| CA-1.4 | Factor de edad aplicado antes de dividir | **Dado que** el asegurado tiene 60 años y `ageFactorPercentage = 20%` **Cuando** se calcula la prima **Entonces** `finalAmount` ya incluye el recargo del 20% y luego se divide entre 12 |

---

#### HU-2: Simplificación de la Interfaz de Cotización

> **Como** usuario final (contratante),  
> **Quiero** ingresar únicamente la fecha de inicio de mi póliza,  
> **Para que** el proceso de cotización sea más rápido y sin datos redundantes.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-2.1 | Campo `endDate` eliminado del formulario Health | **Dado que** el tipo de póliza seleccionado es Health **Cuando** el usuario accede al paso de Cobertura **Entonces** el formulario muestra solo el campo "Fecha de Inicio" y NO el campo "Fecha de Fin" |
| CA-2.2 | Fecha de inicio puede ser futura | **Dado que** el usuario ingresa una fecha de inicio 30 días en el futuro **Cuando** confirma la cotización **Entonces** el sistema acepta la fecha y calcula `endDate = startDate + 365 días` |
| CA-2.3 | Fecha de inicio no puede ser anterior al día actual | **Dado que** el usuario ingresa una fecha de inicio en el pasado **Cuando** intenta avanzar al siguiente paso **Entonces** el formulario muestra el error: "La fecha de inicio no puede ser anterior a hoy" |
| CA-2.4 | Resumen muestra periodo calculado | **Dado que** el usuario llega al paso de Confirmación **Cuando** el tipo es Health **Entonces** el resumen muestra `startDate` ingresada, `endDate` calculada (+12 meses) y `monthlyPremium` calculada |
| CA-2.5 | Campo prima mensual oculto en Health | **Dado que** el tipo es Health **Cuando** el usuario está en cobertura **Entonces** el campo "Prima mensual (COP)" está oculto y el valor se muestra solo como texto informativo en el preview del plan |

---

#### HU-3: Arquitectura Escalable — Preparación para Vida y Vehículo

> **Como** desarrollador del sistema,  
> **Quiero** que la lógica de periodo fijo esté centralizada en un servicio reutilizable (`QuotationService`),  
> **Para que** los módulos de Vida y Vehículo puedan aplicar la misma regla sin duplicar código.

**Criterios de Aceptación:**

| # | Escenario | Gherkin |
|---|-----------|---------|
| CA-3.1 | Servicio centralizado en backend | **Dado que** existe un `QuotationService` en el backend **Cuando** se realiza un cálculo de Health, Life o Vehicle **Entonces** todos usan el método `calculateFixedPeriod(totalAnual): { monthlyPremium, durationDays }` |
| CA-3.2 | Endpoint de cálculo retorna campos extendidos | **Dado que** se llama `GET /api/v1/health-plans/calculate` **Cuando** la respuesta es exitosa **Entonces** incluye `monthlyPremium`, `durationDays: 365` y `endDate` calculado |
| CA-3.3 | Tipos de póliza Life y Vehicle no afectados aún | **Dado que** el tipo seleccionado es Life o Vehicle **Cuando** el usuario accede al paso de Cobertura **Entonces** el formulario muestra los campos `insuredAmount`, `monthlyPremium`, `startDate` y `endDate` sin cambios (iteración futura) |
| CA-3.4 | Sin duplicación de lógica de cálculo | **Dado que** se revisa el código del backend **Cuando** se busca la lógica `totalAnual / 12` **Entonces** aparece únicamente en `QuotationService`, no en controllers ni en otros servicios |

---

### 1.3 Reglas de Negocio

| ID | Regla | Tipo | Módulos afectados |
|----|-------|------|-----------------|
| RN-01 | El periodo de cobertura para Health es siempre 365 días (12 meses) | Invariante | Backend, Frontend |
| RN-02 | `monthlyPremium = finalAmount / 12` (redondeo a entero, sin decimales) | Cálculo | Backend |
| RN-03 | `endDate = startDate + 365 días` (se calcula en backend y se retorna) | Derivado | Backend |
| RN-04 | `startDate` debe ser ≥ fecha actual (no se permiten fechas pasadas) | Validación | Frontend + Backend |
| RN-05 | `startDate` puede ser hasta 1 año en el futuro | Validación | Frontend + Backend |
| RN-06 | El usuario NO ingresa `insuredAmount` para Health; proviene del plan seleccionado | Derivado | Frontend |
| RN-07 | El usuario NO ingresa `monthlyPremium` para Health; se calcula automáticamente | Derivado | Frontend |
| RN-08 | Life: mantendrá flujo temporal con monto manual hasta que se especifique su spec de planes fijos. Vehicle y Home: sin cambios hasta sus respectivas specs de coberturas. | Freezeado | — |
| RN-09 | ~~La regla `monthlyPremium ≤ 5% insuredAmount` aplica a todas las pólizas~~. **ELIMINADA en v1.1.** Motivo: ningún tipo de póliza la cumple de forma natural (Health≈8,33%; Life y futuras son planes fijos; Vehicle/Home dependen de coberturas variables). Solo se valida `monthlyPremium > 0`. | Eliminada | Backend — `Policy.cs` (`ValidateFinancials` removido) |

---

## 2. DISEÑO

### 2.1 Cambios en el Modelo de Datos (Backend)

No se modifican las colecciones de MongoDB existentes. La estructura de `Policy` permanece:

```
coveragePeriod.startDate  — string "YYYY-MM-DD"
coveragePeriod.endDate    — string "YYYY-MM-DD"  ← ahora siempre = startDate + 365 días para Health
monthlyPremium            — number               ← ahora siempre = finalAmount / 12 para Health
insuredAmount             — number               ← siempre = healthPlan.finalAmount para Health
```

**Sin migraciones de datos.** Los documentos existentes no se alteran.

---

### 2.2 API Endpoints

#### MODIFICADO: `GET /api/v1/health-plans/calculate`

**Parámetros de entrada (sin cambio):**

| Param | Tipo | Descripción |
|-------|------|-------------|
| `planId` | string | ID del plan de salud |
| `birthDate` | string (YYYY-MM-DD) | Fecha de nacimiento del asegurado |

**Response — Campos nuevos añadidos:**

```json
{
  "planId": "string",
  "planName": "string",
  "baseAmount": 1200000,
  "ageFactorPercentage": 20,
  "ageFactorAmount": 240000,
  "finalAmount": 1440000,
  "insuredAge": 60,
  "monthlyPremium": 120000,
  "durationDays": 365
}
```

| Campo nuevo | Tipo | Cálculo |
|-------------|------|---------|
| `monthlyPremium` | number | `Math.round(finalAmount / 12)` |
| `durationDays` | number | Constante `365` |

**Códigos HTTP:**

| Código | Situación |
|--------|-----------|
| 200 | Cálculo exitoso |
| 400 | `planId` o `birthDate` ausente/inválido |
| 404 | Plan no encontrado |

---

#### SIN CAMBIO: `POST /api/v1/policies`

El request `CreatePolicyRequest` ya acepta `monthlyPremium` y `coveragePeriod.endDate`. El frontend calculará `endDate = startDate + 365 días` y enviará `monthlyPremium` obtenido del endpoint de cálculo.

---

### 2.3 Diseño Frontend

#### 2.3.1 Componentes impactados

| Componente | Cambio |
|-----------|--------|
| `PolicyCreateComponent` | Eliminar campo `endDate` del formulario para Health; usar `monthlyPremium` del cálculo; derivar `endDate` antes del submit |
| `HealthPlanPreviewComponent` | Añadir visualización de `monthlyPremium` y `durationDays` en el preview |
| `HealthPlansService.calculate()` | Tipar respuesta para incluir `monthlyPremium` y `durationDays` |

#### 2.3.2 Flujo de datos actualizado (Health)

```
[Paso 0 — Tipo]
  └─ selectedType = 'Health'

[Paso 1 — Asegurado]
  └─ insuredForm: { firstName, lastName, documentType, documentId, birthDate, email, phone }

[Paso 2 — Cobertura]
  ├─ healthPlanSelector → onPlanSelected(plan)
  │   └─ healthSvc.calculate(planId, birthDate)
  │       └─ response: { finalAmount, monthlyPremium, durationDays }
  │           ├─ healthCalculation.set(response)
  │           └─ selectedPlanId.set(plan.planId)
  ├─ [Campo eliminado] endDate (no visible para Health)
  └─ startDate — ingresado por el usuario
       └─ validación: ≥ today, ≤ today + 1 año

[Paso 3 — Confirmación]
  ├─ endDate derivado = startDate + 365 días (calculado antes de submit)
  ├─ insuredAmount = healthCalculation.finalAmount
  └─ monthlyPremium = healthCalculation.monthlyPremium
```

#### 2.3.3 Cambios en `coverageForm`

```typescript
// ANTES
coverageForm = fb.group({
  type:           [...],
  insuredAmount:  [null, [Validators.min(1)]],
  monthlyPremium: [null, [Validators.required, Validators.min(1)]],
  startDate:      [null, Validators.required],
  endDate:        [null, Validators.required],   // ← visible siempre
});

// DESPUÉS
coverageForm = fb.group({
  type:           [...],
  insuredAmount:  [null, [Validators.min(1)]],
  monthlyPremium: [null, [Validators.min(1)]],   // ← sin required para Health (auto-calculado)
  startDate:      [null, [Validators.required, startDateValidator]],   // ← validador: ≥ today
  endDate:        [null],                          // ← hidden para Health; derivado en submit()
});
```

#### 2.3.4 Lógica en `submit()` — cálculo de `endDate` para Health

```typescript
// En submit(), para tipo Health:
const startDate = cv.startDate as Date;
const endDate = isHealth
  ? new Date(startDate.getTime() + 365 * 86_400_000)  // startDate + 365 días
  : (cv.endDate as Date);
```

#### 2.3.5 Validador de fecha de inicio

```typescript
// Validador personalizado (puede ser función inline o ValidatorFn)
function startDateValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value as Date | null;
  if (!value) return null;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const maxFuture = new Date(today.getTime() + 365 * 86_400_000);
  if (value < today) return { pastDate: true };
  if (value > maxFuture) return { tooFarFuture: true };
  return null;
}
```

#### 2.3.6 Modelo `HealthPlanCalculation` — campos extendidos

```typescript
// ANTES (health-plan-selection.model.ts)
export interface HealthPlanCalculation extends HealthPlanSelection {
  insuredAge: number;
}

// DESPUÉS
export interface HealthPlanCalculation extends HealthPlanSelection {
  insuredAge: number;
  monthlyPremium: number;   // ← nuevo
  durationDays: number;     // ← nuevo (365)
}
```

---

### 2.4 Arquitectura — `QuotationService` (Backend)

Crear un servicio centralizado en el backend que concentre la lógica de periodo fijo:

```
backend/
└── src/
    └── policies/
        └── domain/
            └── services/
                └── quotation.service.ts   ← NUEVO
```

```typescript
// Interfaz del servicio (pseudocódigo — adaptar al framework backend)
interface FixedPeriodQuotation {
  monthlyPremium: number;   // totalAnual / 12 redondeado
  durationDays: number;     // siempre 365
  endDate(startDate: Date): Date;  // startDate + 365 días
}

class QuotationService {
  calculateFixedPeriod(totalAnual: number): FixedPeriodQuotation {
    return {
      monthlyPremium: Math.round(totalAnual / 12),
      durationDays: 365,
      endDate: (startDate: Date) => new Date(startDate.getTime() + 365 * 86_400_000),
    };
  }
}
```

El `HealthPlansService` del backend invocará `QuotationService.calculateFixedPeriod(finalAmount)` e incluirá los campos en la response. Cuando se implemente Life y Vehicle, sus servicios también llamarán a `QuotationService.calculateFixedPeriod()`.

---

## 3. LISTA DE TAREAS

### 3.1 Backend

#### Implementación

- [ ] Crear `QuotationService` con método `calculateFixedPeriod(totalAnual: number)`
- [ ] Inyectar `QuotationService` en `HealthPlansController` / `HealthPlansService`
- [ ] Modificar handler de `GET /api/v1/health-plans/calculate` para incluir `monthlyPremium` y `durationDays` en la response
- [ ] Añadir validación: `startDate >= hoy` en `POST /api/v1/policies` para tipo Health
- [ ] Añadir validación: `startDate <= hoy + 1 año` en `POST /api/v1/policies` para tipo Health
- [ ] Verificar que `endDate` recibido en el create es `startDate + 365 días` (o calcularlo en backend si se prefiere)

#### Tests

- [ ] Test unitario `QuotationService.calculateFixedPeriod`: `1.440.000 / 12 = 120.000`
- [ ] Test unitario `QuotationService.calculateFixedPeriod`: redondeo correcto para valores no divisibles
- [ ] Test integración `GET /health-plans/calculate`: respuesta incluye `monthlyPremium` y `durationDays: 365`
- [ ] Test integración `POST /policies`: validación `startDate` pasada retorna 400
- [ ] Test integración `POST /policies` Health: `endDate` es exactamente `startDate + 365 días`

---

### 3.2 Frontend

#### Implementación

- [ ] Extender `HealthPlanCalculation` model con `monthlyPremium: number` y `durationDays: number`
- [ ] Crear `startDateValidator` (no fecha pasada, no más de 1 año en el futuro)
- [ ] Actualizar `coverageForm`: quitar `Validators.required` de `monthlyPremium` para Health; añadir `startDateValidator`
- [ ] Ocultar campo `endDate` en el template cuando `isHealthType()`
- [ ] Ocultar campo `monthlyPremium` en el template cuando `isHealthType()`
- [ ] Mostrar `monthlyPremium` calculado como texto informativo en `HealthPlanPreviewComponent`
- [ ] Actualizar `onPlanSelected()`: leer `healthCalculation().monthlyPremium` del nuevo campo
- [ ] Actualizar `submit()`: derivar `endDate = startDate + 365 días` para Health
- [ ] Actualizar `submit()`: usar `healthCalculation().monthlyPremium` como `monthlyPremium` para Health
- [ ] Actualizar resumen (paso Confirmación): mostrar `endDate` derivado y prima mensual calculada
- [ ] Actualizar lógica `[disabled]` del botón "Continuar" en paso Cobertura para Health

#### Tests (si aplica)

- [ ] Test unitario `startDateValidator`: fecha pasada → error `pastDate`
- [ ] Test unitario `startDateValidator`: fecha futura >1 año → error `tooFarFuture`
- [ ] Test unitario `startDateValidator`: fecha válida → null
- [ ] Test componente `PolicyCreateComponent`: seleccionar Health oculta `endDate` y `monthlyPremium`
- [ ] Test componente `PolicyCreateComponent`: `submit()` calcula `endDate` correctamente para Health

---

### 3.3 QA — Verificación Manual

- [ ] **QA-01:** Seleccionar tipo Health → campos `endDate` y `monthlyPremium` no aparecen en el formulario
- [ ] **QA-02:** Seleccionar un plan de salud → el preview muestra `monthlyPremium` calculado correctamente
- [ ] **QA-03:** Ingresar fecha de inicio hoy → campo válido (sin error)
- [ ] **QA-04:** Ingresar fecha de inicio ayer → error "La fecha de inicio no puede ser anterior a hoy"
- [ ] **QA-05:** Ingresar fecha de inicio hoy + 13 meses → error de fecha futura
- [ ] **QA-06:** Ingresar fecha de inicio hoy + 6 meses → campo válido
- [ ] **QA-07:** Llegar al paso Confirmación → mostrar `endDate = startDate + 365 días`
- [ ] **QA-08:** Llegar al paso Confirmación → mostrar `monthlyPremium = finalAmount / 12`
- [ ] **QA-09:** Crear póliza Health exitosamente → verificar en listado que `coveragePeriod.endDate` es correcto
- [ ] **QA-10:** Tipo Life → formulario sin cambios (endDate visible, monthlyPremium editable)
- [ ] **QA-11:** Tipo Vehicle → formulario sin cambios (endDate visible, monthlyPremium editable)
- [ ] **QA-12:** Tipo Travel → sin cambios en su flujo específico
- [ ] **QA-13:** Cambiar tipo de Health a Life → campos `endDate` y `monthlyPremium` vuelven a ser editables
- [ ] **QA-14:** Endpoint `GET /health-plans/calculate` → response incluye `monthlyPremium` y `durationDays: 365`
- [ ] **QA-15:** `POST /policies` con `startDate` pasada → backend retorna 400

---

*Fin de SPEC-008 — `health-premium-fixed-period`*
