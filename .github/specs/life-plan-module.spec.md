---
id: SPEC-009
status: IN_PROGRESS
feature: life-plan-module
created: 2026-04-09
updated: 2026-04-09
author: spec-generator
version: "1.0"
related-specs: ["SPEC-008"]
---

# Spec: Módulo de Vida — Planes de Precio Fijo

> **Estado:** `DRAFT` → aprobar con `status: APPROVED` antes de iniciar implementación.
> **Ciclo de vida:** DRAFT → APPROVED → IN_PROGRESS → IMPLEMENTED → DEPRECATED

---

## Resumen Ejecutivo

El módulo de seguros de Vida amplía la oferta de InsuraTech con tres productos de precio fijo: **Plan Vida**, **Vida para la Familia** y **Vida Premium**, con primas anuales de COP 120.000, COP 150.000 y COP 200.000 respectivamente (primas mensuales: COP 10.000, COP 12.500 y COP 16.667). Los beneficios cubiertos (muerte, gastos de entierro, gastos fúnebres y remuneración a beneficiarios) son montos fijos por plan y no son modificables por el usuario final.

La arquitectura replica el patrón establecido en el módulo de Salud (SPEC-008): el `QuotationService` existente centraliza el cálculo de prima mensual y período fijo de 365 días. A diferencia del módulo de Salud, las pólizas de Vida **no aplican factor de edad** — el costo es invariante para cualquier asegurado dentro del rango de edad permitido.

**Comparativa de módulos:**

| Aspecto | Módulo Salud (SPEC-008) | Módulo Vida (SPEC-009) |
|---------|------------------------|------------------------|
| Número de planes | 4 | 3 |
| Factor de edad | Sí (0%, 4%, 8%) | No — costo fijo |
| Prima mensual | `finalAmount / 12` | `annualPremium / 12` (constante) |
| Período de cobertura | 365 días fijos | 365 días fijos |
| Beneficios | Atención médica (monto global) | Muerte + entierro + fúnebres + remuneración (desglosados) |
| Rango de edad | 18–73 años | 18–65 años |
| `endDate` | Derivado en backend / frontend | Derivado en backend / frontend |

---

## 1. REQUERIMIENTOS

### 1.1 Descripción del Feature

Crear el **Módulo de Vida** que expone tres planes de seguro de vida de precio fijo, sin factor de edad. El agente selecciona el plan, ingresa la fecha de inicio y el sistema calcula automáticamente: prima mensual, fecha de fin (startDate + 365 días) y desglose de beneficios. La prima no es editable manualmente — proviene del catálogo de planes.

### 1.2 Historias de Usuario

---

#### HU-1: Cotización del Plan Vida (Básico)

```
Como:        Agente de seguros
Quiero:      Seleccionar el Plan Vida y generar una cotización automática con prima de COP 10.000/mes (anual: COP 120.000)
Para:        Ofrecer al cliente la opción más accesible de cobertura de vida con beneficios esenciales

Prioridad:   Alta
Estimación:  M
Dependencias: SPEC-008 (QuotationService), flujo PolicyCreateComponent existente
Capa:        Ambas
```

#### Criterios de Aceptación — HU-1

**Happy Path**
```gherkin
CRITERIO-1.1: Cotización Plan Vida exitosa
  Dado que:  el agente seleccionó "Plan Vida" en el paso de cobertura
             y el asegurado tiene entre 18 y 65 años
             y ha ingresado una fecha de inicio válida (≥ hoy)
  Cuando:    confirma la cotización y envía el formulario
  Entonces:  el sistema crea la póliza con:
             - monthlyPremium = 10.000 COP (anual: 120.000 COP)
             - insuredAmount  = 30.000.000 COP (suma asegurada por muerte)
             - coveragePeriod.endDate = startDate + 365 días
             - type = "Life"
             y retorna HTTP 201
```

**Error Path**
```gherkin
CRITERIO-1.2: Asegurado mayor de 65 años rechazado
  Dado que:  el agente ingresó la fecha de nacimiento de un asegurado de 66 años
  Cuando:    selecciona cualquier plan de vida
  Entonces:  el sistema muestra el mensaje:
             "Los planes de Vida no están disponibles para asegurados mayores de 65 años."
             y bloquea el avance al paso de confirmación
```

**Edge Case**
```gherkin
CRITERIO-1.3: Asegurado en el límite de edad (exactamente 65 años)
  Dado que:  el asegurado cumple 65 años exactamente hoy
  Cuando:    el agente intenta seleccionar un plan de vida
  Entonces:  el sistema permite la contratación (65 años es el límite inclusivo)
```

---

#### HU-2: Cotización del Plan Vida para la Familia

```
Como:        Agente de seguros
Quiero:      Seleccionar el Plan Vida para la Familia y generar una cotización con prima de COP 12.500/mes (anual: COP 150.000)
Para:        Ofrecer al cliente cobertura intermedia con mayor suma asegurada y beneficios ampliados
             para proteger a su grupo familiar

Prioridad:   Alta
Estimación:  S
Dependencias: HU-1 (infraestructura de planes de vida)
Capa:        Ambas
```

#### Criterios de Aceptación — HU-2

**Happy Path**
```gherkin
CRITERIO-2.1: Cotización Vida para la Familia exitosa
  Dado que:  el agente seleccionó "Vida para la Familia"
             y el asegurado tiene entre 18 y 65 años
  Cuando:    confirma la cotización
  Entonces:  el sistema crea la póliza con:
             - monthlyPremium = 12.500 COP (anual: 150.000 COP)
             - insuredAmount  = 60.000.000 COP (suma asegurada por muerte)
             - tipo = "Life"
             y el preview muestra el desglose completo de beneficios del plan
```

**Error Path**
```gherkin
CRITERIO-2.2: Fecha de inicio en el pasado rechazada
  Dado que:  el agente ingresó una fecha de inicio anterior a hoy
  Cuando:    intenta avanzar al paso de confirmación
  Entonces:  el formulario muestra el error:
             "La fecha de inicio no puede ser anterior a hoy."
             y el botón "Continuar" permanece deshabilitado
```

**Edge Case**
```gherkin
CRITERIO-2.3: Fecha de inicio igual a hoy aceptada
  Dado que:  el agente ingresa la fecha de hoy como fecha de inicio
  Cuando:    selecciona el plan y confirma
  Entonces:  el sistema acepta la póliza sin error de validación
             y endDate = hoy + 365 días
```

---

#### HU-3: Cotización del Plan Vida Premium

```
Como:        Agente de seguros
Quiero:      Seleccionar el Plan Vida Premium y generar una cotización con prima de COP 16.667/mes (anual: COP 200.000)
Para:        Ofrecer al cliente la cobertura más completa con mayor suma asegurada
             y el desglose máximo de beneficios funerarios y de remuneración

Prioridad:   Media
Estimación:  S
Dependencias: HU-1, HU-2
Capa:        Ambas
```

#### Criterios de Aceptación — HU-3

**Happy Path**
```gherkin
CRITERIO-3.1: Cotización Vida Premium exitosa
  Dado que:  el agente seleccionó "Vida Premium"
             y el asegurado tiene entre 18 y 65 años
  Cuando:    confirma la cotización
  Entonces:  el sistema crea la póliza con:
             - monthlyPremium = 16.667 COP (anual: 200.000 COP)
             - insuredAmount  = 100.000.000 COP (suma asegurada por muerte)
             - tipo = "Life"
             y el resumen muestra todos los beneficios del plan Premium
```

**Error Path**
```gherkin
CRITERIO-3.2: Asegurado menor de 18 años rechazado
  Dado que:  el agente ingresó la fecha de nacimiento de un asegurado menor de 18 años
  Cuando:    selecciona cualquier plan de vida
  Entonces:  el sistema muestra:
             "El asegurado debe ser mayor de 18 años para contratar un seguro de Vida."
             y bloquea la selección del plan
```

**Edge Case**
```gherkin
CRITERIO-3.3: Asegurado exactamente de 18 años habilitado
  Dado que:  el asegurado cumple 18 años exactamente hoy
  Cuando:    el agente selecciona el plan Vida Premium
  Entonces:  el sistema permite la contratación sin error de validación
```

---

### 1.3 Reglas de Negocio

| ID | Regla | Tipo | Módulos afectados |
|----|-------|------|-----------------|
| RN-01 | La prima anual de cada plan es fija: Vida=120.000, Familia=150.000, Premium=200.000 (COP). Prima mensual derivada: Vida=10.000, Familia=12.500, Premium=16.667 (COP) | Invariante | Backend, Frontend |
| RN-02 | No se aplica factor de edad en los planes de Vida | Invariante | Backend |
| RN-03 | El período de cobertura es siempre 365 días fijos (reutiliza `QuotationService.FixedDurationDays`) | Invariante | Backend |
| RN-04 | `endDate = startDate + 365 días` — calculado con `QuotationService.CalculateEndDate()` | Derivado | Backend |
| RN-05 | `startDate` debe ser ≥ fecha actual (no se permiten fechas pasadas) | Validación | Frontend + Backend |
| RN-06 | Rango de edad permitido: 18 a 65 años (inclusive en ambos límites) | Validación | Backend |
| RN-07 | Los beneficios (muerte, entierro, gastos fúnebres, remuneración) son montos fijos por plan; el usuario final NO puede modificarlos | Invariante | Backend, Frontend |
| RN-08 | El usuario NO ingresa `monthlyPremium`; proviene del plan seleccionado en el catálogo | Derivado | Frontend |
| RN-09 | El usuario NO ingresa `insuredAmount`; es la suma asegurada por muerte del plan seleccionado | Derivado | Frontend |
| RN-10 | `monthlyPremium = annualPremium / 12` usando `QuotationService.CalculateMonthlyPremium()` | Cálculo | Backend |
| RN-11 | Los montos de beneficios secundarios (entierro, fúnebres, remuneración) son informativos en la UI — no se almacenan como campos separados en `Policy`; se registran dentro de `LifePlanSelection` | Arquitectura | Backend |

---

### 1.4 Catálogo de Planes y Beneficios

| Plan | `planId` | Prima anual (COP) | Prima mensual (COP) | Muerte (suma asegurada) | Gastos de entierro | Gastos fúnebres | Remuneración a beneficiarios |
|------|----------|------------------:|--------------------:|------------------------:|-------------------:|----------------:|-----------------------------:|
| Plan Vida | `plan-vida` | 120.000 | 10.000 | 30.000.000 | 3.000.000 | 1.500.000 | 1.500.000 |
| Vida para la Familia | `vida-familia` | 150.000 | 12.500 | 60.000.000 | 5.000.000 | 2.500.000 | 2.500.000 |
| Vida Premium | `vida-premium` | 200.000 | 16.667 | 100.000.000 | 8.000.000 | 4.000.000 | 4.000.000 |

> **Nota actuarial:** La suma asegurada por muerte es el `insuredAmount` almacenado en `Policy`. Los demás beneficios son coberturas complementarias fijas almacenadas en `LifePlanSelection`.

---

## 2. DISEÑO

### 2.1 Modelos de Datos

#### Entidades afectadas

| Entidad | Almacén | Cambios | Descripción |
|---------|---------|---------|-------------|
| `Policy` | MongoDB `policies` | Modificada | Añadir propiedad `LifePlan?: LifePlanSelection` y factory `CreateLifePolicy()` |
| `LifePlan` | In-memory (catálogo) | Nueva | Value Object con `Id`, `Name`, `MonthlyPremium`, `DeathBenefit`, beneficios complementarios |
| `LifePlanCatalog` | In-memory (estático) | Nuevo | Catálogo con los 3 planes; análogo a `HealthPlanCatalog` |
| `LifePlanSelection` | Embebido en `Policy` | Nuevo | Snapshot del plan seleccionado en el momento de contratación |

#### Campos — `LifePlan` (Domain Value Object)

| Campo | Tipo C# | Descripción |
|-------|---------|-------------|
| `Id` | `string` | Identificador único (kebab-case) |
| `Name` | `string` | Nombre legible del plan |
| `AnnualPremium` | `decimal` | Prima anual fija (COP) — fuente de verdad (120.000 / 150.000 / 200.000) |
| `MonthlyPremium` | `decimal` | Prima mensual derivada: `AnnualPremium / 12` (10.000 / 12.500 / 16.667) |
| `DeathBenefit` | `decimal` | Suma asegurada por muerte (COP) |
| `FuneralExpenses` | `decimal` | Gastos de entierro (COP) |
| `BurialExpenses` | `decimal` | Gastos fúnebres (COP) |
| `BeneficiaryCompensation` | `decimal` | Remuneración a beneficiarios (COP) |

#### Campos — `LifePlanSelection` (Snapshot embebido en Policy)

| Campo | Tipo C# | Descripción |
|-------|---------|-------------|
| `PlanId` | `string` | ID del plan contratado |
| `PlanName` | `string` | Nombre del plan |
| `AnnualPremium` | `decimal` | Prima anual al momento de contratación |
| `MonthlyPremium` | `decimal` | Prima mensual al momento de contratación (`AnnualPremium / 12`) |
| `DeathBenefit` | `decimal` | Suma asegurada por muerte |
| `FuneralExpenses` | `decimal` | Gastos de entierro |
| `BurialExpenses` | `decimal` | Gastos fúnebres |
| `BeneficiaryCompensation` | `decimal` | Remuneración a beneficiarios |

#### Modificaciones en `Policy.cs`

```csharp
// Propiedad nueva (análoga a HealthPlan)
public LifePlanSelection? LifePlan { get; private set; }

// Factory nueva
public static Policy CreateLifePolicy(
    PolicyNumber number,
    InsuredPerson insured,
    CoveragePeriod coverage,
    string lifePlanId,
    DateOnly today)
```

> No se requieren migraciones de datos. Los documentos existentes en MongoDB no son alterados; `LifePlan` es `null` para pólizas previas.

---

### 2.2 API Endpoints

#### NUEVO: `GET /api/v1/life-plans`

- **Descripción:** Lista todos los planes de vida disponibles con sus beneficios
- **Auth requerida:** sí (Bearer token)
- **Response 200:**
  ```json
  [
    {
      "planId": "plan-vida",
      "planName": "Plan Vida",
      "annualPremium": 120000,
      "monthlyPremium": 10000,
      "deathBenefit": 30000000,
      "funeralExpenses": 3000000,
      "burialExpenses": 1500000,
      "beneficiaryCompensation": 1500000
    },
    {
      "planId": "vida-familia",
      "planName": "Vida para la Familia",
      "annualPremium": 150000,
      "monthlyPremium": 12500,
      "deathBenefit": 60000000,
      "funeralExpenses": 5000000,
      "burialExpenses": 2500000,
      "beneficiaryCompensation": 2500000
    },
    {
      "planId": "vida-premium",
      "planName": "Vida Premium",
      "annualPremium": 200000,
      "monthlyPremium": 16667,
      "deathBenefit": 100000000,
      "funeralExpenses": 8000000,
      "burialExpenses": 4000000,
      "beneficiaryCompensation": 4000000
    }
  ]
  ```
- **Response 401:** token ausente o expirado

---

#### NUEVO: `GET /api/v1/life-plans/calculate`

- **Descripción:** Valida que el asegurado cumple el rango de edad y retorna el desglose completo del plan seleccionado junto con los campos de cotización calculados por `QuotationService`
- **Auth requerida:** sí
- **Query params:**

  | Param | Tipo | Obligatorio | Descripción |
  |-------|------|-------------|-------------|
  | `planId` | string | sí | ID del plan de vida |
  | `birthDate` | string (YYYY-MM-DD) | sí | Fecha de nacimiento del asegurado |

- **Response 200:**
  ```json
  {
    "planId": "plan-vida",
    "planName": "Plan Vida",
    "insuredAge": 34,
    "annualPremium": 120000,
    "monthlyPremium": 10000,
    "durationDays": 365,
    "deathBenefit": 30000000,
    "funeralExpenses": 3000000,
    "burialExpenses": 1500000,
    "beneficiaryCompensation": 1500000
  }
  ```
- **Response 400:** `planId` o `birthDate` ausente o inválido
- **Response 404:** plan no encontrado
- **Response 422:** asegurado fuera del rango de edad (< 18 o > 65 años)

  ```json
  {
    "title": "Age restriction",
    "status": 422,
    "detail": "Life plans are not available for insured persons older than 65 years."
  }
  ```

---

#### MODIFICADO (sin cambio en contrato): `POST /api/v1/policies`

El request `CreatePolicyRequest` ya soporta `type: "Life"`. Se añade el campo opcional `lifePlanId`:

```json
{
  "type": "Life",
  "insured": { "...": "..." },
  "coveragePeriod": {
    "startDate": "2026-05-01",
    "endDate": "2027-04-30"
  },
  "insuredAmount": 30000000,
  "monthlyPremium": 120000,
  "lifePlanId": "plan-vida"
}
```

> `endDate` es calculado por el frontend como `startDate + 365 días` (mismo patrón que Health). `insuredAmount` y `monthlyPremium` provienen de la respuesta del endpoint `/calculate`. El backend valida que `lifePlanId` existe en el catálogo cuando `type = "Life"`.

---

### 2.3 Diseño Frontend

#### 2.3.1 Componentes nuevos

| Componente | Ruta de archivo | Descripción |
|-----------|----------------|-------------|
| `LifePlanSelectorComponent` | `ui/blocks/life-plan-selector/` | Selector de cards (3 planes). Input signal: `plans`. Output: plan seleccionado. Análogo a `HealthPlanSelectorComponent` |
| `LifePlanPreviewComponent` | `ui/blocks/life-plan-preview/` | Muestra desglose del plan: prima mensual, suma asegurada por muerte y tabla de beneficios complementarios. Input signal: `calculation`. Análogo a `HealthPlanPreviewComponent` |

#### 2.3.2 Servicios nuevos

| Servicio | Ruta | Endpoints que consume |
|----------|------|----------------------|
| `LifePlansService` | `core/service/life-plans.service.ts` | `GET /api/v1/life-plans`, `GET /api/v1/life-plans/calculate` |

#### 2.3.3 Modelos nuevos (TypeScript)

```typescript
// core/models/life-plan-selection.model.ts

export interface LifePlan {
  planId: string;
  planName: string;
  monthlyPremium: number;
  deathBenefit: number;
  funeralExpenses: number;
  burialExpenses: number;
  beneficiaryCompensation: number;
}

export interface LifePlanCalculation extends LifePlan {
  insuredAge: number;
  annualPremium: number;
  durationDays: number;  // siempre 365
}
```

#### 2.3.4 Cambios en `PolicyCreateComponent`

El tipo `Life` adopta el mismo patrón que `Health`:

| Aspecto | Comportamiento para `Life` |
|---------|---------------------------|
| Campo `monthlyPremium` | Oculto — proviene del plan seleccionado |
| Campo `insuredAmount` | Oculto — es el `deathBenefit` del plan |
| Campo `endDate` | Oculto — derivado como `startDate + 365 días` |
| Campo `startDate` | Visible, requerido, validador: ≥ hoy |
| Selector de planes | `LifePlanSelectorComponent` visible cuando `isLifeType()` |
| Preview de plan | `LifePlanPreviewComponent` visible al seleccionar un plan |
| Restricción de edad | Asegurado ≤ 65 años (análoga a `AgeRestrictionComponent` de Health) |

**Nuevo signal en el componente:**
```typescript
isLifeType      = signal(false);
lifePlans       = signal<LifePlan[]>([]);
selectedLifePlanId = signal<string | null>(null);
lifeCalculation = signal<LifePlanCalculation | null>(null);
ageRestrictedLife = signal(false);   // true si asegurado > 65 años
```

**Actualización de `onTypeChange()`:**
```typescript
} else if (type === 'Life') {
  monthlyPremiumCtrl.clearValidators();
  monthlyPremiumCtrl.setValue(null);
  endDateCtrl.clearValidators();
  endDateCtrl.setValue(null);
  this.loadLifePlans();
  this.checkLifeAgeRestriction();  // valida ≤ 65 años
}
```

**Actualización de `submit()` para Life:**
```typescript
const endDate = isLife && cv.startDate
  ? this.toDateStr(new Date(cv.startDate.getTime() + 364 * 86_400_000))
  : /* ... otros tipos ... */;

insuredAmount:  isLife ? (this.lifeCalculation()?.deathBenefit ?? 0) : /* ... */,
monthlyPremium: isLife ? (this.lifeCalculation()?.monthlyPremium ?? 0) : /* ... */,
...(isLife && this.selectedLifePlanId() ? { lifePlanId: this.selectedLifePlanId()! } : {}),
```

#### 2.3.5 Flujo de datos (Life)

```
[Paso 0 — Tipo]
  └─ selectedType = 'Life'

[Paso 1 — Asegurado]
  └─ insuredForm: { firstName, lastName, documentType, documentId, birthDate, email, phone }
       └─ ageRestrictedLife: true si birthDate → edad > 65

[Paso 2 — Cobertura]
  ├─ lifePlanSelector → onLifePlanSelected(plan)
  │   └─ lifeSvc.calculate(planId, birthDate)
  │       └─ response: { monthlyPremium, annualPremium, deathBenefit, durationDays, beneficios }
  │           └─ lifeCalculation.set(response)
  ├─ [Oculto] endDate
  ├─ [Oculto] monthlyPremium
  ├─ [Oculto] insuredAmount
  └─ startDate — ingresado por el usuario (validador: ≥ hoy)

[Paso 3 — Confirmación]
  ├─ endDate = startDate + 365 días
  ├─ insuredAmount = lifeCalculation.deathBenefit
  ├─ monthlyPremium = lifeCalculation.monthlyPremium
  └─ tabla de beneficios complementarios (solo informativa)
```

---

### 2.4 Arquitectura y Reutilización

#### Servicios reutilizados (sin modificación)

| Servicio | Reutilización |
|----------|---------------|
| `QuotationService.FixedDurationDays` | Constante `365` usada en `LifePlanPricingService` |
| `QuotationService.CalculateMonthlyPremium(annualAmount)` | No aplica directo (la prima ya es fija). El servicio se invoca para mantener consistencia arquitectural: `CalculateMonthlyPremium(monthlyPremium * 12)` retorna el mismo valor |
| `QuotationService.CalculateEndDate(startDate)` | Calcula `endDate` para Life de igual forma que Health |

> **Decisión de diseño:** Para los planes de Vida la prima mensual está almacenada directamente en el catálogo (no derivada de un `BaseAmount` con factor de edad). `QuotationService.CalculateMonthlyPremium` no aporta diferencia numérica, pero se invoca para centralizar la lógica de redondeo y mantener un único punto de cálculo en caso de cambios futuros.

#### Nuevos archivos Backend

```
Backend/src/InsuraTech.Domain/Policies/LifePlan/
├── LifePlan.cs                     ← Value Object
├── LifePlanCatalog.cs              ← Catálogo estático (3 planes)
├── LifePlanPricingService.cs       ← Validación de edad + QuotationService
└── LifePlanSelection.cs            ← Snapshot para Policy

Backend/src/InsuraTech.Application/LifePlans/
├── DTOs/
│   └── LifePlanDto.cs              ← LifePlanDto + LifePlanCalculationDto
└── Queries/
    ├── GetLifePlans/
    │   ├── GetLifePlansQuery.cs
    │   └── GetLifePlansHandler.cs
    └── CalculateLifePlan/
        ├── CalculateLifePlanQuery.cs
        └── CalculateLifePlanHandler.cs

Backend/src/InsuraTech.API/Controllers/
└── LifePlansController.cs
```

#### Nuevos archivos Frontend

```
frontend/src/app/features/policies/
├── core/
│   ├── models/
│   │   └── life-plan-selection.model.ts
│   └── service/
│       └── life-plans.service.ts
└── ui/
    └── blocks/
        ├── life-plan-selector/
        │   ├── life-plan-selector.component.ts
        │   ├── life-plan-selector.component.html
        │   └── life-plan-selector.component.css
        └── life-plan-preview/
            ├── life-plan-preview.component.ts
            ├── life-plan-preview.component.html
            └── life-plan-preview.component.css
```

### 2.5 Requerimientos No Funcionales

| ID | Categoría | Requerimiento |
|----|-----------|---------------|
| RNF-01 | Seguridad | Los montos de beneficios almacenados en `LifePlanSelection` son inmutables post-creación; ningún endpoint expone un método para modificarlos |
| RNF-02 | Seguridad | El endpoint `/api/v1/life-plans/calculate` requiere autenticación Bearer; no es público |
| RNF-03 | Rendimiento | La respuesta del catálogo (`GET /life-plans`) debe ser < 100 ms; el catálogo es estático (in-memory) sin consultas a base de datos |
| RNF-04 | Rendimiento | El cálculo de cotización (`GET /life-plans/calculate`) debe completarse en < 150 ms |
| RNF-05 | Integridad | Los montos del catálogo son constantes en código (`LifePlanCatalog.cs`); no provienen de base de datos y no pueden alterarse en tiempo de ejecución |
| RNF-06 | Auditabilidad | El campo `LifePlanSelection` es un snapshot inmutable del plan al momento de creación; si el catálogo cambia en futuras versiones, las pólizas históricas conservan los valores originales |
| RNF-07 | Extensibilidad | La arquitectura debe permitir añadir nuevos planes de vida modificando únicamente `LifePlanCatalog.cs`, sin cambios en handlers, controllers ni frontend |

---

## 3. LISTA DE TAREAS

> Checklist accionable. Marcar cada ítem (`[x]`) al completarlo.

### 3.1 Backend

#### Dominio

- [ ] Crear `LifePlan.cs` — Value Object con los 7 campos definidos en §2.1
- [ ] Crear `LifePlanCatalog.cs` — catálogo estático con los 3 planes y montos de §1.4
- [ ] Crear `LifePlanSelection.cs` — snapshot para almacenamiento en `Policy`
- [ ] Crear `LifePlanPricingService.cs` — validación de edad (18–65) + retorno de `LifePlanSelection`
- [ ] Modificar `Policy.cs` — añadir propiedad `LifePlan?: LifePlanSelection`
- [ ] Modificar `Policy.cs` — añadir factory `CreateLifePolicy(number, insured, coverage, lifePlanId, today)`
- [ ] Agregar excepción de dominio `OverageLifeInsuredException` (> 65 años)

#### Aplicación

- [ ] Crear `LifePlanDto.cs` y `LifePlanCalculationDto.cs` en `Application/LifePlans/DTOs/`
- [ ] Crear `GetLifePlansQuery` y `GetLifePlansHandler` — retorna `IReadOnlyList<LifePlanDto>`
- [ ] Crear `CalculateLifePlanQuery` y `CalculateLifePlanHandler` — valida edad y retorna `LifePlanCalculationDto`
- [ ] Actualizar `CreatePolicyValidator.cs` — When `type == "Life"`, validar que `lifePlanId` existe en catálogo
- [ ] Actualizar `CreatePolicyHandler.cs` — invocar `Policy.CreateLifePolicy()` cuando `type == "Life"`

#### API

- [ ] Crear `LifePlansController.cs` con rutas `GET /api/v1/life-plans` y `GET /api/v1/life-plans/calculate`
- [ ] Registrar `LifePlansController` en el pipeline (automático con MediatR y convenciones del proyecto)

#### Tests Backend

- [ ] `LifePlanPricingService_ValidAge_ReturnsSelection` — edad válida (30 años) → retorna plan sin error
- [ ] `LifePlanPricingService_OverageInsured_ThrowsException` — edad 66 → lanza excepción
- [ ] `LifePlanPricingService_UnderageInsured_ThrowsException` — edad 17 → lanza excepción
- [ ] `LifePlanPricingService_BoundaryAge65_Allowed` — edad exacta 65 → sin excepción
- [ ] `LifePlanPricingService_BoundaryAge18_Allowed` — edad exacta 18 → sin excepción
- [ ] `LifePlanCatalog_AllPlans_HaveCorrectMonthlyPremium` — aserta 120k, 150k, 200k
- [ ] `Policy_CreateLifePolicy_SetsCorrectFields` — type=Life, insuredAmount=deathBenefit
- [ ] `GET /life-plans` → HTTP 200 con 3 planes
- [ ] `GET /life-plans/calculate?planId=plan-vida&birthDate=...` → HTTP 200 con campos correctos
- [ ] `GET /life-plans/calculate` con edad > 65 → HTTP 422
- [ ] `POST /policies` con `type=Life` y `lifePlanId` válido → HTTP 201
- [ ] `POST /policies` con `type=Life` y `lifePlanId` inválido → HTTP 400

---

### 3.2 Frontend

#### Implementación

- [ ] Crear `life-plan-selection.model.ts` — interfaces `LifePlan` y `LifePlanCalculation`
- [ ] Crear `life-plans.service.ts` — métodos `getPlans()` y `calculate(planId, birthDate)`
- [ ] Crear `LifePlanSelectorComponent` — cards para los 3 planes con selección visual
- [ ] Crear `LifePlanPreviewComponent` — preview con prima mensual + tabla de beneficios
- [ ] Modificar `PolicyCreateComponent` — añadir signals `isLifeType`, `lifePlans`, `selectedLifePlanId`, `lifeCalculation`, `ageRestrictedLife`
- [ ] Modificar `onTypeChange()` — rama `Life`: ocultar `endDate`/`monthlyPremium`, cargar planes, verificar edad ≤ 65
- [ ] Modificar `onTypeChange()` — eliminar la lógica transitoria de Life (que antes mostraba campos manuales)
- [ ] Actualizar template — ocultar `insuredAmount`, `monthlyPremium`, `endDate` cuando `isLifeType()`
- [ ] Actualizar template — mostrar `LifePlanSelectorComponent` cuando `isLifeType()`
- [ ] Actualizar template — mostrar `LifePlanPreviewComponent` cuando `isLifeType() && lifeCalculation()`
- [ ] Actualizar template — mostrar restricción de edad cuando `ageRestrictedLife()`
- [ ] Actualizar paso Confirmación — mostrar `endDate` derivado y tabla de beneficios para Life
- [ ] Actualizar `submit()` — usar `lifeCalculation().deathBenefit` como `insuredAmount` para Life
- [ ] Actualizar `submit()` — incluir `lifePlanId` en el payload cuando `isLifeType()`

#### Tests Frontend

- [ ] `LifePlanSelectorComponent` renderiza 3 tarjetas de plan
- [ ] `LifePlanSelectorComponent` emite plan seleccionado al hacer click
- [ ] `LifePlanPreviewComponent` muestra prima mensual correcta por plan
- [ ] `LifePlanPreviewComponent` muestra tabla de beneficios completa
- [ ] `PolicyCreateComponent`: seleccionar Life oculta `endDate`, `monthlyPremium`, `insuredAmount`
- [ ] `PolicyCreateComponent`: asegurado > 65 bloquea el selector de planes
- [ ] `PolicyCreateComponent`: `submit()` calcula `endDate = startDate + 365 días` para Life
- [ ] `PolicyCreateComponent`: `submit()` incluye `lifePlanId` en el payload

---

### 3.3 QA — Verificación Manual

- [ ] **QA-01:** Seleccionar tipo Life → campos `endDate`, `monthlyPremium`, `insuredAmount` no aparecen en el formulario
- [ ] **QA-02:** Selector de planes muestra los 3 planes con su prima mensual y descripción de beneficios
- [ ] **QA-03:** Seleccionar "Plan Vida" → preview muestra prima mensual 10.000 COP (anual 120.000 COP) y suma asegurada 30.000.000 COP
- [ ] **QA-04:** Seleccionar "Vida para la Familia" → preview muestra prima mensual 12.500 COP (anual 150.000 COP) y suma 60.000.000 COP
- [ ] **QA-05:** Seleccionar "Vida Premium" → preview muestra prima mensual 16.667 COP (anual 200.000 COP) y suma 100.000.000 COP
- [ ] **QA-06:** Asegurado con fecha de nacimiento que resulta en edad > 65 → aviso de restricción de edad visible
- [ ] **QA-07:** Asegurado con edad = 65 años exactos hoy → puede contratar sin restricción
- [ ] **QA-08:** Ingresar fecha de inicio en el pasado → error "La fecha de inicio no puede ser anterior a hoy"
- [ ] **QA-09:** Ingresar fecha de inicio igual a hoy → campo válido sin error
- [ ] **QA-10:** Llegar al paso Confirmación → `endDate = startDate + 365 días` mostrado correctamente
- [ ] **QA-11:** Crear póliza Life exitosamente → listado de pólizas muestra tipo "Life" con plan correcto
- [ ] **QA-12:** Verificar en BD que `lifePlan` está embebido en el documento de `Policy`
- [ ] **QA-13:** Tipo Health → sin cambios en su flujo (no afectado por esta spec)
- [ ] **QA-14:** Tipo Travel → sin cambios en su flujo
- [ ] **QA-15:** Cambiar de Life a otro tipo → campos `endDate`, `monthlyPremium`, `insuredAmount` vuelven a ser editables
- [ ] **QA-16:** `GET /api/v1/life-plans` → retorna 3 planes con beneficios correctos (HTTP 200)
- [ ] **QA-17:** `GET /api/v1/life-plans/calculate` con asegurado de 70 años → HTTP 422 con mensaje de restricción
- [ ] **QA-18:** Verificar que los montos de beneficios en la póliza creada son inmutables desde el listado/detalle

---

*Fin de SPEC-009 — `life-plan-module`*
