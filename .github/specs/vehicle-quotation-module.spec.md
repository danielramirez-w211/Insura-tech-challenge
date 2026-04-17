---
id: SPEC-010
status: IMPLEMENTED
feature: vehicle-quotation-module
created: 2026-04-10
updated: 2026-04-13
approved: 2026-04-10
author: spec-generator
version: "1.0"
related-specs: []
---

# Spec: Módulo de Cotización de Vehículo Inteligente

> **Estado:** `DRAFT` → aprobar con `status: APPROVED` antes de iniciar implementación.
> **Ciclo de vida:** DRAFT → APPROVED → IN_PROGRESS → IMPLEMENTED → DEPRECATED

---

## Resumen Ejecutivo

El módulo de Cotización de Vehículo Inteligente introduce un motor de cálculo de prima basado en **tasa técnica por antigüedad del vehículo**, tres planes de cobertura diferenciados (Estándar, Completo y Premium), y un conjunto de reglas de negocio actuariales (recargo por marca, prima mínima, redondeo al mil superior y descuento por pago anual).

A diferencia de los módulos de Salud y Vida — donde la prima es fija o depende de la edad del asegurado — el módulo de Vehículo calcula la prima a partir del **valor comercial declarado** del vehículo, la **antigüedad** y la **marca**. El plan seleccionado (Estándar / Completo / Premium) aplica un multiplicador sobre la prima base resultante. El tipo de póliza `Vehicle = 3` ya existe en el enum `PolicyType` del dominio; no requiere modificación.

**Comparativa de módulos:**

| Aspecto | Módulo Salud (SPEC-008) | Módulo Vida (SPEC-009) | Módulo Vehículo (SPEC-010) |
|---------|------------------------|------------------------|----------------------------|
| Prima base | Monto fijo por plan | Monto fijo por plan | `valorComercial × tasaTecnica` (dinámica) |
| Factor de ajuste | Edad del asegurado (0 / 4 / 8 %) | Ninguno | Antigüedad del vehículo (2.2 / 2.8 / 3.5 %) |
| Recargo adicional | No | No | 10% por marca de alta siniestralidad |
| Prima mínima | No | No | $800.000 COP |
| Redondeo | No | No | Al mil superior (CEILING) |
| Descuento pago anual | No | No | 5% sobre la prima del plan elegido |
| Planes disponibles | 4 | 3 | 3 (Estándar / Completo / Premium) |
| Período de cobertura | 365 días | 365 días | 365 días |
| `PolicyType` | `Health = 2` | `Life = 1` | `Vehicle = 3` (ya existe) |

---

## 1. REQUERIMIENTOS

### 1.1 Descripción del Feature

Crear el **Módulo de Cotización de Vehículo Inteligente** que captura los datos técnicos del vehículo (valor comercial, año de fabricación, marca), aplica el motor de cálculo de tasa técnica por antigüedad, muestra los tres planes de cobertura con su prima calculada, aplica reglas de negocio actuariales (recargo por marca, prima mínima, redondeo, descuento anual) y persiste la póliza en MongoDB a través del flujo existente `CreatePolicyCommand`.

### 1.2 Historias de Usuario

---

#### HU-1: Captura de Datos Técnicos del Vehículo

```
Como:        Agente de seguros
Quiero:      Ingresar los datos técnicos del vehículo (valor comercial, año de fabricación y marca)
             y que el sistema determine automáticamente si es Nuevo o Usado
Para:        Alimentar el motor de cálculo con los insumos necesarios para determinar la tasa técnica
             correcta antes de mostrar los planes de cobertura disponibles

Prioridad:   Alta
Estimación:  M
Dependencias: Flujo PolicyCreateComponent existente (MatStepper)
Capa:        Ambas
```

#### Criterios de Aceptación — HU-1

**Happy Path**
```gherkin
CRITERIO-1.1: Captura de datos de vehículo nuevo exitosa
  Dado que:  el agente está en el paso "Datos del Vehículo" del formulario de creación de póliza
             y seleccionó tipo de póliza "Vehículo"
  Cuando:    ingresa un valor comercial de $50.000.000 COP
             y un año de fabricación igual al año actual (vehículo nuevo)
             y una marca que NO está en la lista de alta siniestralidad
  Entonces:  el sistema clasifica el vehículo como "Nuevo"
             y habilita el avance al paso de selección de plan
             con tasa técnica preliminar = 2.2%
```

**Error Path**
```gherkin
CRITERIO-1.2: Valor comercial inválido rechazado
  Dado que:  el agente está en el paso "Datos del Vehículo"
  Cuando:    ingresa un valor comercial de $0 o un valor negativo
  Entonces:  el formulario muestra el error:
             "El valor comercial del vehículo debe ser mayor a $0."
             y el botón "Continuar" permanece deshabilitado
```

**Edge Case**
```gherkin
CRITERIO-1.3: Año de fabricación en el límite entre nuevo y usado
  Dado que:  el agente ingresa el año de fabricación igual al año actual
  Cuando:    el sistema evalúa la antigüedad
  Entonces:  el vehículo se clasifica como "Nuevo" (0 años de antigüedad)
             y se aplica la tasa técnica de 2.2%
```

---

#### HU-2: Motor de Cálculo de Prima Base (Tasa Técnica)

```
Como:        Sistema de cotización
Quiero:      Calcular la prima base bruta aplicando la tasa técnica según la antigüedad del vehículo,
             el recargo por marca (si aplica), la prima mínima y el redondeo al mil superior
Para:        Garantizar que la prima final sea técnicamente correcta, actuarialmente sostenible
             y consistente con las reglas de negocio aprobadas por el equipo actuarial

Prioridad:   Alta
Estimación:  L
Dependencias: HU-1 (datos del vehículo capturados)
Capa:        Backend
```

#### Criterios de Aceptación — HU-2

**Happy Path**
```gherkin
CRITERIO-2.1: Cálculo de prima base para vehículo nuevo sin recargo de marca
  Dado que:  el valor comercial del vehículo es $50.000.000 COP
             y la antigüedad es 0 años (vehículo nuevo)
             y la marca NO está en la lista de alta siniestralidad
  Cuando:    el motor de cálculo ejecuta la fórmula de prima base
  Entonces:  primaBruta    = 50.000.000 × 0.022 = 1.100.000
             sin recargo   = 1.100.000
             ≥ prima mínima (800.000) → OK
             primaBase     = CEILING(1.100.000 / 1000) × 1000 = 1.100.000
```

**Error Path**
```gherkin
CRITERIO-2.2: Prima bruta por debajo del mínimo — se aplica piso de $800.000
  Dado que:  el valor comercial del vehículo es $10.000.000 COP
             y la antigüedad es 0 años (tasa 2.2%)
  Cuando:    el motor calcula prima bruta = 10.000.000 × 0.022 = 220.000
  Entonces:  el sistema aplica prima mínima:
             primaBruta ajustada = max(220.000, 800.000) = 800.000
             primaBase           = CEILING(800.000 / 1000) × 1000 = 800.000
```

**Edge Case**
```gherkin
CRITERIO-2.3: Vehículo de marca de alta siniestralidad con resultado exacto de mil
  Dado que:  el valor comercial es $30.000.000 COP
             y la antigüedad es 3 años (tasa 2.8%)
             y la marca tiene recargo del 10%
  Cuando:    el motor ejecuta el cálculo
  Entonces:  primaBruta     = 30.000.000 × 0.028 = 840.000
             con recargo    = 840.000 × 1.10      = 924.000
             ≥ prima mínima                        → OK
             primaBase      = CEILING(924.000 / 1000) × 1000 = 924.000
```

---

#### HU-3: Selección y Visualización de Planes de Cobertura

```
Como:        Agente de seguros
Quiero:      Ver los tres planes de cobertura (Estándar, Completo, Premium) con sus primas calculadas
             para el vehículo específico del cliente, y seleccionar el plan más adecuado
Para:        Ofrecer opciones diferenciadas de cobertura y presentar claramente el valor de cada plan
             antes de confirmar la contratación

Prioridad:   Alta
Estimación:  M
Dependencias: HU-2 (prima base calculada)
Capa:        Ambas
```

#### Criterios de Aceptación — HU-3

**Happy Path**
```gherkin
CRITERIO-3.1: Visualización de los tres planes con primas correctas
  Dado que:  la primaBase calculada por HU-2 es $1.100.000 COP
             y el agente está en el paso "Selección de Plan"
  Cuando:    el componente VehiclePlanSelectorComponent renderiza los planes
  Entonces:  el sistema muestra:
             - Plan Estándar:  prima = $1.100.000/mes  (primaBase × 1.00)
             - Plan Completo:  prima = $1.320.000/mes  (CEILING(1.100.000 × 1.20 / 1000) × 1000)
             - Plan Premium:   prima = $1.595.000/mes  (CEILING(1.100.000 × 1.45 / 1000) × 1000)
             y cada tarjeta detalla las coberturas incluidas en ese plan
```

**Error Path**
```gherkin
CRITERIO-3.2: Intento de avanzar sin seleccionar plan bloqueado
  Dado que:  el agente está en el paso "Selección de Plan"
             y ningún plan ha sido seleccionado
  Cuando:    intenta hacer clic en "Continuar"
  Entonces:  el botón permanece deshabilitado
             y el sistema muestra el mensaje de validación:
             "Debe seleccionar un plan de cobertura para continuar."
```

**Edge Case**
```gherkin
CRITERIO-3.3: Plan Premium con todas las asistencias incluidas visibles
  Dado que:  el agente seleccionó el Plan Premium
  Cuando:    el preview del plan se renderiza
  Entonces:  el componente VehiclePlanPreviewComponent muestra las tres asistencias adicionales:
             - Conductor elegido
             - Carro de repuesto
             - Grúa
             junto a la prima mensual y la prima anual con descuento del 5%
```

---

#### HU-4: Aplicación de Reglas de Negocio Pro (Recargos, Primas Mínimas, Redondeo y Descuento Anual)

```
Como:        Sistema de cotización
Quiero:      Aplicar todas las reglas de negocio actuariales: recargo por marca de alta siniestralidad,
             prima mínima de $800.000, redondeo al mil superior y descuento del 5% por pago anual
Para:        Asegurar que el precio final sea técnicamente correcto y transparente para el cliente

Prioridad:   Alta
Estimación:  M
Dependencias: HU-2, HU-3
Capa:        Backend
```

#### Criterios de Aceptación — HU-4

**Happy Path**
```gherkin
CRITERIO-4.1: Descuento del 5% por pago anual aplicado correctamente
  Dado que:  el agente seleccionó el Plan Completo con prima mensual de $1.320.000
  Cuando:    el sistema calcula la opción de pago anual
  Entonces:  primaAnualConDescuento = 1.320.000 × 0.95 = 1.254.000
             y el sistema muestra ambas opciones (mensual y anual con descuento)
```

**Error Path**
```gherkin
CRITERIO-4.2: Marca no reconocida tratada como sin recargo
  Dado que:  el agente ingresa una marca que no está en la lista configurada
             de marcas de alta siniestralidad
  Cuando:    el motor de cálculo evalúa el recargo
  Entonces:  el sistema aplica recargoPorMarca = 0% (sin recargo)
             y continúa el cálculo con la prima base sin modificación adicional
```

**Edge Case**
```gherkin
CRITERIO-4.3: Redondeo al mil superior cuando prima no es múltiplo exacto de 1000
  Dado que:  la prima bruta calculada (con recargo) es $925.500
  Cuando:    el sistema aplica CEILING(925.500 / 1000) × 1000
  Entonces:  primaBase = $926.000
             y no se aplica redondeo bancario (no se usa Math.Round estándar)
```

---

### 1.3 Reglas de Negocio

| ID | Regla | Tipo | Módulos afectados |
|----|-------|------|-------------------|
| RN-01 | Tasa técnica según antigüedad: Nuevo = 2.2%, 1–5 años = 2.8%, >5 años = 3.5% | Invariante | Backend |
| RN-02 | `primaBruta = valorComercial × tasaTecnica` | Cálculo | Backend |
| RN-03 | Si la marca está en la lista de alta siniestralidad: `primaBruta = primaBruta × 1.10` (recargo 10%) | Condicional | Backend |
| RN-04 | `primaBruta = max(primaBruta, 800_000)` — prima mínima de $800.000 COP | Invariante | Backend |
| RN-05 | `primaBase = CEILING(primaBruta / 1000) × 1000` — redondeo al mil superior (no Math.Round) | Cálculo | Backend |
| RN-06 | `primaCompleto = CEILING((primaBase × 1.20) / 1000) × 1000` | Cálculo | Backend |
| RN-07 | `primaPremium  = CEILING((primaBase × 1.45) / 1000) × 1000` | Cálculo | Backend |
| RN-08 | `primaAnualConDescuento = primaDelPlanSeleccionado × 0.95` (descuento 5% pago anual) | Condicional | Backend, Frontend |
| RN-09 | La antigüedad se determina como `añoActual - añoFabricacion`. Si el resultado es 0 → Nuevo. Si es 1–5 → Usado Reciente. Si es >5 → Usado Antiguo | Derivado | Backend |
| RN-10 | La lista de marcas de alta siniestralidad es **configurable por el equipo actuarial** (no hardcodeada en el catálogo de dominio). Ejemplo ilustrativo no exhaustivo: Renault, Chevrolet en segmento masivo | Configuración | Backend |
| RN-11 | El período de cobertura es siempre 365 días fijos — reutiliza `QuotationService.FixedDurationDays` | Invariante | Backend |
| RN-12 | `endDate = startDate + 365 días` usando `QuotationService.CalculateEndDate()` | Derivado | Backend |
| RN-13 | `startDate` debe ser ≥ fecha actual (no se permiten fechas pasadas) | Validación | Frontend + Backend |
| RN-14 | El Plan Premium incluye tres asistencias adicionales: Conductor elegido, Carro de repuesto, Grúa | Invariante | Backend, Frontend |
| RN-15 | `PolicyType.Vehicle = 3` ya existe en el enum — no se requiere modificación del enum | Arquitectura | Backend |
| RN-16 | El usuario NO ingresa la prima manualmente; el motor de cálculo la deriva completamente | Derivado | Frontend |
| RN-17 | Las coberturas son acumulativas — Plan Completo incluye todas las de Estándar; Plan Premium incluye todas las de Completo | Invariante | Backend, Frontend |
| RN-18 | `tasaTecnica` es una **tasa anual**. La prima mensual = `primaAnual / 12`. El campo `annualPremiumWithDiscount` = `primaAnual × 0.95` (siempre mayor que la prima mensual) | Invariante | Backend |

---

### 1.4 Fórmulas de Cálculo (Referencia Exacta)

```
// PASO 1 — Determinar tasa técnica por antigüedad
antigüedad  = añoActual - añoFabricacion
tasaTecnica = antigüedad == 0      ? 0.022
            : antigüedad <= 5     ? 0.028
                                  : 0.035

// PASO 2 — Prima bruta ANUAL base
// La tasa técnica es una tasa ANUAL aplicada sobre el valor comercial
primaBrutaAnual = valorComercial * tasaTecnica

// PASO 3 — Recargo por marca (si aplica)
if (marcaEnListaAltaSiniestralidad)
    primaBrutaAnual = primaBrutaAnual * 1.10

// PASO 4 — Aplicar prima mínima anual
primaBrutaAnual = max(primaBrutaAnual, 800_000)

// PASO 5 — Redondeo al mil superior → prima ANUAL base por plan
primaAnualBase     = CEILING(primaBrutaAnual / 1000) * 1000
primaAnualCompleto = CEILING((primaAnualBase * 1.20) / 1000) * 1000
primaAnualPremium  = CEILING((primaAnualBase * 1.45) / 1000) * 1000

// PASO 6 — Prima mensual (prima anual / 12, redondeada a peso entero)
primaMensualEstandar = ROUND(primaAnualBase     / 12)
primaMensualCompleto = ROUND(primaAnualCompleto / 12)
primaMensualPremium  = ROUND(primaAnualPremium  / 12)

// PASO 7 — Descuento por pago anual (5% sobre la prima anual del plan seleccionado)
// Interpetación: si el cliente paga el año en un solo pago, recibe 5% de descuento sobre la prima anual
primaAnualConDescuento = primaAnualDelPlanSeleccionado * 0.95
```

> **Clarificación crítica (RN-18):** `tasaTecnica` es una **tasa anual**. `primaAnualBase` es la prima anual del plan Estándar. La prima mensual se obtiene dividiendo entre 12. El campo `annualPremiumWithDiscount` en la API es la prima anual con 5% de descuento (pago único), siempre mayor que la prima mensual.

**Tabla de verificación de cálculos (ejemplos):**

| Valor comercial | Años | Marca | primaBrutaAnual | primaAnualBase | primaAnualCompleto | primaAnualPremium | mensualEstándar | mensualCompleto | mensualPremium |
|-----------------|------|:-----:|----------------:|---------------:|-------------------:|------------------:|----------------:|----------------:|---------------:|
| $50.000.000 | 0 (Nuevo) | No | 1.100.000 | 1.100.000 | 1.320.000 | 1.595.000 | 91.667 | 110.000 | 132.917 |
| $50.000.000 | 3 (Usado Rec.) | No | 1.400.000 | 1.400.000 | 1.680.000 | 2.030.000 | 116.667 | 140.000 | 169.167 |
| $50.000.000 | 8 (Usado Ant.) | No | 1.750.000 | 1.750.000 | 2.100.000 | 2.538.000 | 145.833 | 175.000 | 211.500 |
| $10.000.000 | 0 (Nuevo) | No | 220.000 → 800.000 | 800.000 | 960.000 | 1.160.000 | 66.667 | 80.000 | 96.667 |
| $30.000.000 | 3 (Usado Rec.) | Sí | 924.000 | 924.000 | 1.109.000 | 1.340.000 | 77.000 | 92.417 | 111.667 |
| $135.000.000 | 0 (Nuevo) | No | 2.970.000 | 2.970.000 | 3.564.000 | 4.307.000 | 247.500 | 297.000 | 358.917 |

> **Nota sobre CEILING en C#:** usar `Math.Ceiling(value / 1000m) * 1000m` con aritmética `decimal`.
> **Nota sobre ROUND en C#:** usar `Math.Round(value, 0, MidpointRounding.AwayFromZero)` para el paso 6.

---

## 2. DISEÑO

### 2.1 Modelos de Datos

#### Entidades afectadas

| Entidad | Almacén | Cambios | Descripción |
|---------|---------|---------|-------------|
| `Policy` | MongoDB `policies` | Modificada | Añadir propiedad `VehiclePlan?: VehiclePlanSelection` y factory `CreateVehiclePolicy()` |
| `VehiclePlan` | In-memory (catálogo) | Nuevo | Value Object con Id, Name, multiplicador de precio y lista de asistencias |
| `VehiclePlanCatalog` | In-memory (estático) | Nuevo | Catálogo estático de los 3 planes; análogo a `HealthPlanCatalog` |
| `VehiclePlanSelection` | Embebido en `Policy` | Nuevo | Snapshot del resultado de cotización al momento de contratación |
| `VehicleQuotation` | Value Object transitorio | Nuevo | Resultado del motor de cálculo: primaBase, primaEstandar, primaCompleto, primaPremium |
| `VehiclePricingService` | In-memory (servicio de dominio) | Nuevo | Motor de cálculo de tasa técnica — análogo a `HealthPlanPricingService` |
| `HighSinistrabilityBrands` | In-memory (configuración) | Nuevo | Lista configurable de marcas con recargo del 10% |
| `CreatePolicyCommand` | Application | Modificado | Añadir campos `VehiclePlanId?`, `VehicleCommercialValue?`, `VehicleYear?`, `VehicleBrand?` |

#### Campos — `VehiclePlan` (Domain Value Object)

| Campo | Tipo C# | Descripción |
|-------|---------|-------------|
| `Id` | `string` | Identificador único (kebab-case): `standard`, `complete`, `premium` |
| `Name` | `string` | Nombre legible: "Plan Estándar", "Plan Completo", "Plan Premium" |
| `PriceMultiplier` | `decimal` | Multiplicador: 1.00 / 1.20 / 1.45 |
| `Coverages` | `IReadOnlyList<string>` | Coberturas incluidas en el plan (acumulativas — cada plan incluye las del anterior) |
| `Assistances` | `IReadOnlyList<string>` | Servicios de asistencia adicionales (vacía en Estándar) |

#### Catálogo de Planes — `VehiclePlanCatalog`

> Las coberturas son **acumulativas**: Plan Completo incluye todo lo de Estándar + sus adicionales; Plan Premium incluye todo lo de Completo + sus adicionales.

| Plan | `planId` | Multiplicador | Coberturas incluidas | Asistencias adicionales |
|------|----------|:-------------:|----------------------|-------------------------|
| Plan Estándar | `standard` | 1.00 | Robo, Pérdida parcial, Pérdida total, Daños a terceros, Rayones a carrocería | Ninguna |
| Plan Completo | `complete` | 1.20 | Todo Estándar + Pérdida de llaves, Daño mecánico, Daño eléctrico | Grúa |
| Plan Premium | `premium` | 1.45 | Todo Completo (ídem) | Grúa, Carro de repuesto, Mecánico a casa, Conductor elegido |

#### Tabla Comparativa de Coberturas por Plan

| Cobertura / Asistencia | Plan Estándar | Plan Completo | Plan Premium |
|------------------------|:-------------:|:-------------:|:------------:|
| Robo | ✅ | ✅ | ✅ |
| Pérdida parcial | ✅ | ✅ | ✅ |
| Pérdida total | ✅ | ✅ | ✅ |
| Daños a terceros | ✅ | ✅ | ✅ |
| Rayones a carrocería | ✅ | ✅ | ✅ |
| Pérdida de llaves | ❌ | ✅ | ✅ |
| Daño mecánico | ❌ | ✅ | ✅ |
| Daño eléctrico | ❌ | ✅ | ✅ |
| Grúa | ❌ | ✅ | ✅ |
| Carro de repuesto | ❌ | ❌ | ✅ |
| Mecánico a casa | ❌ | ❌ | ✅ |
| Conductor elegido | ❌ | ❌ | ✅ |

> **Regla RN-17:** Las coberturas son acumulativas — Plan Completo incluye todas las de Estándar; Plan Premium incluye todas las de Completo. Esta regla debe validarse en el frontend mostrando el listado completo en `VehiclePlanSelectorComponent` y `VehiclePlanPreviewComponent`.

#### Campos — `VehiclePlanSelection` (Snapshot embebido en Policy)

| Campo | Tipo C# | Descripción |
|-------|---------|-------------|
| `PlanId` | `string` | ID del plan contratado |
| `PlanName` | `string` | Nombre del plan |
| `VehicleBrand` | `string` | Marca del vehículo asegurado |
| `VehicleYear` | `int` | Año de fabricación |
| `CommercialValue` | `decimal` | Valor comercial declarado (COP) |
| `TechnicalRate` | `decimal` | Tasa técnica aplicada (0.022 / 0.028 / 0.035) |
| `HasBrandSurcharge` | `bool` | Indica si se aplicó recargo del 10% por marca |
| `BaseMonthlyPremium` | `decimal` | Prima base (Plan Estándar) tras aplicar mínimo y redondeo |
| `FinalMonthlyPremium` | `decimal` | Prima mensual del plan seleccionado |
| `AnnualPremiumWithDiscount` | `decimal` | Prima anual con descuento del 5% por pago anual |
| `Coverages` | `IReadOnlyList<string>` | Coberturas del plan contratado (snapshot en tiempo de contratación) |
| `Assistances` | `IReadOnlyList<string>` | Servicios de asistencia del plan contratado (vacío para Estándar) |

#### Campos — `VehicleQuotation` (Value Object transitorio — resultado del motor)

| Campo | Tipo C# | Descripción |
|-------|---------|-------------|
| `CommercialValue` | `decimal` | Valor comercial de entrada |
| `VehicleYear` | `int` | Año de fabricación |
| `Brand` | `string` | Marca del vehículo |
| `TechnicalRate` | `decimal` | Tasa técnica (0.022 / 0.028 / 0.035) |
| `HasBrandSurcharge` | `bool` | Si aplicó recargo |
| `BaseMonthlyPremium` | `decimal` | primaBase (Plan Estándar) |
| `CompletePlanPremium` | `decimal` | primaCompleto |
| `PremiumPlanPremium` | `decimal` | primaPremium |

#### Modificaciones en `Policy.cs`

```csharp
// Propiedad nueva (análoga a HealthPlan y LifePlan)
public VehiclePlanSelection? VehiclePlan { get; private set; }

// Factory nueva
public static Policy CreateVehiclePolicy(
    PolicyNumber number,
    InsuredPerson insured,
    CoveragePeriod coverage,
    string vehiclePlanId,
    decimal commercialValue,
    int vehicleYear,
    string vehicleBrand,
    DateOnly today)
```

#### Modificaciones en `CreatePolicyCommand.cs`

```csharp
/// <summary>Solo para pólizas de tipo Vehicle.</summary>
public string? VehiclePlanId { get; init; }

/// <summary>Valor comercial del vehículo (COP). Solo para pólizas Vehicle.</summary>
public decimal? VehicleCommercialValue { get; init; }

/// <summary>Año de fabricación del vehículo. Solo para pólizas Vehicle.</summary>
public int? VehicleYear { get; init; }

/// <summary>Marca del vehículo. Solo para pólizas Vehicle.</summary>
public string? VehicleBrand { get; init; }
```

> No se requieren migraciones de datos. Los documentos existentes en MongoDB no son alterados; `VehiclePlan` es `null` para pólizas de tipos previos.

---

### 2.2 API Endpoints

#### NUEVO: `GET /api/v1/vehicle-plans`

- **Descripción:** Lista los tres planes de cobertura vehicular disponibles con sus multiplicadores y asistencias
- **Auth requerida:** sí (Bearer token)
- **Response 200:**
  ```json
  [
    {
      "planId": "standard",
      "planName": "Plan Estándar",
      "priceMultiplier": 1.00,
      "assistances": []
    },
    {
      "planId": "complete",
      "planName": "Plan Completo",
      "priceMultiplier": 1.20,
      "assistances": []
    },
    {
      "planId": "premium",
      "planName": "Plan Premium",
      "priceMultiplier": 1.45,
      "assistances": ["Conductor elegido", "Carro de repuesto", "Grúa"]
    }
  ]
  ```

#### NUEVO: `GET /api/v1/vehicle-plans/calculate`

- **Descripción:** Ejecuta el motor de cotización con los datos del vehículo y retorna las primas de los tres planes
- **Auth requerida:** sí (Bearer token)
- **Query params:**
  - `commercialValue` (decimal, requerido): valor comercial del vehículo en COP
  - `vehicleYear` (int, requerido): año de fabricación
  - `brand` (string, requerido): marca del vehículo
- **Response 200:**
  ```json
  {
    "commercialValue": 50000000,
    "vehicleYear": 2024,
    "brand": "Toyota",
    "vehicleAge": 0,
    "ageCategory": "Nuevo",
    "technicalRate": 0.022,
    "hasBrandSurcharge": false,
    "baseMonthlyPremium": 1100000,
    "plans": [
      {
        "planId": "standard",
        "planName": "Plan Estándar",
        "monthlyPremium": 1100000,
        "annualPremiumWithDiscount": 1045000,
        "assistances": []
      },
      {
        "planId": "complete",
        "planName": "Plan Completo",
        "monthlyPremium": 1320000,
        "annualPremiumWithDiscount": 1254000,
        "assistances": []
      },
      {
        "planId": "premium",
        "planName": "Plan Premium",
        "monthlyPremium": 1595000,
        "annualPremiumWithDiscount": 1515250,
        "assistances": ["Conductor elegido", "Carro de repuesto", "Grúa"]
      }
    ]
  }
  ```
- **Response 400:** `commercialValue` ≤ 0 o `vehicleYear` fuera de rango razonable (ej. < 1900 o > añoActual)
- **Response 422:** plan no encontrado por `planId`

#### MODIFICADO: `POST /api/v1/policies`

- **Descripción:** Crea una póliza de tipo Vehicle — extiende el endpoint existente
- **Campos adicionales en el Request Body para `type = "Vehicle"`:**
  ```json
  {
    "type": "Vehicle",
    "vehiclePlanId": "premium",
    "vehicleCommercialValue": 50000000,
    "vehicleYear": 2021,
    "vehicleBrand": "Toyota",
    "coverageStartDate": "2026-05-01",
    "insuredFirstName": "...",
    "insuredLastName": "...",
    "insuredDocumentType": "CC",
    "insuredDocumentId": "...",
    "insuredBirthDate": "1990-01-15"
  }
  ```
- **Response 201:** póliza creada con `vehiclePlan` embebido en el documento
- **Response 400:** campos de vehículo faltantes cuando `type = "Vehicle"`
- **Response 422:** `vehiclePlanId` no reconocido

---

### 2.3 Diseño Frontend

El frontend replica el patrón implementado en los módulos de Salud y Vida dentro del componente `PolicyCreateComponent` (MatStepper). Se agrega un paso intermedio condicional para la cotización vehicular que aparece solo cuando `policyType === 'Vehicle'`.

#### Componentes nuevos

| Componente | Archivo | Descripción |
|------------|---------|-------------|
| `VehicleDataFormComponent` | `ui/blocks/vehicle-data-form/vehicle-data-form.component.ts` | Formulario reactivo para captura de valor comercial, año y marca del vehículo. Emite el resultado al componente padre vía `output()` |
| `VehiclePlanSelectorComponent` | `ui/blocks/vehicle-plan-selector/vehicle-plan-selector.component.ts` | Muestra las tres tarjetas de plan con sus primas calculadas. Análogo a `LifePlanSelectorComponent`. Usa Signals para el plan seleccionado |
| `VehiclePlanPreviewComponent` | `ui/blocks/vehicle-plan-preview/vehicle-plan-preview.component.ts` | Resumen del plan seleccionado: marca, año, prima mensual, prima anual con descuento, asistencias. Análogo a `LifePlanPreviewComponent` |

#### Servicio nuevo

| Servicio | Archivo | Endpoints que consume |
|----------|---------|----------------------|
| `VehiclePlansService` | `core/service/vehicle-plans.service.ts` | `GET /api/v1/vehicle-plans` y `GET /api/v1/vehicle-plans/calculate` |

#### Modelo nuevo

| Interfaz | Archivo | Descripción |
|----------|---------|-------------|
| `VehiclePlan` | `core/models/vehicle-plan-selection.model.ts` | Interfaz del catálogo de plan devuelto por el API |
| `VehicleQuotationResult` | `core/models/vehicle-plan-selection.model.ts` | Interfaz del resultado completo de cotización (con los tres planes calculados) |
| `VehiclePlanOption` | `core/models/vehicle-plan-selection.model.ts` | Interfaz de cada opción de plan dentro del resultado de cotización |

```typescript
export interface VehiclePlan {
  planId: string;
  planName: string;
  priceMultiplier: number;
  assistances: string[];
}

export interface VehiclePlanOption {
  planId: string;
  planName: string;
  monthlyPremium: number;
  annualPremiumWithDiscount: number;
  assistances: string[];
}

export interface VehicleQuotationResult {
  commercialValue: number;
  vehicleYear: number;
  brand: string;
  vehicleAge: number;
  ageCategory: 'Nuevo' | 'Usado Reciente' | 'Usado Antiguo';
  technicalRate: number;
  hasBrandSurcharge: boolean;
  baseMonthlyPremium: number;
  plans: VehiclePlanOption[];
}
```

#### Signals y estado en `PolicyCreateComponent`

```typescript
// Señales nuevas a agregar (análogo al patrón de lifePlanCalculation$)
vehicleQuotation     = signal<VehicleQuotationResult | null>(null);
selectedVehiclePlan  = signal<VehiclePlanOption | null>(null);
vehicleQuoteLoading  = signal(false);
vehicleQuoteError    = signal<string | null>(null);
```

#### Flujo de navegación (MatStepper)

```
Paso 1: Tipo de Póliza
        └─ [usuario selecciona "Vehículo"]
Paso 2: Datos del Asegurado (existente — sin cambios)
Paso 3: Datos del Vehículo  [NUEVO — visible solo si type === 'Vehicle']
        └─ Formulario: valorComercial, añoFabricacion, marca
        └─ Botón "Cotizar" → llama a VehiclePlansService.calculate()
        └─ Muestra spinner durante la llamada
Paso 4: Selección de Plan   [NUEVO — visible solo si type === 'Vehicle']
        └─ VehiclePlanSelectorComponent (3 tarjetas)
        └─ Al seleccionar: habilita botón "Continuar"
Paso 5: Confirmación (existente — muestra VehiclePlanPreviewComponent si type === 'Vehicle')
        └─ VehiclePlanPreviewComponent con resumen completo
        └─ Botón "Confirmar y Crear Póliza" → llama a PoliciesCoreService.create()
```

---

### 2.4 Arquitectura de Capas Backend

Siguiendo el patrón Clean Architecture + MediatR establecido en módulos anteriores:

```
VehiclePlansController
    └─ GetVehiclePlansQuery         → GetVehiclePlansHandler      → VehiclePlanCatalog
    └─ CalculateVehicleQuotationQuery → CalculateVehicleQuotationHandler → VehiclePricingService
                                                                           └─ VehiclePlanCatalog
                                                                           └─ HighSinistrabilityBrands
PoliciesController (POST existente)
    └─ CreatePolicyCommand (modificado)
        └─ CreatePolicyHandler (modificado)
            └─ Policy.CreateVehiclePolicy()
                └─ VehiclePricingService.Calculate()
```

#### Archivos a crear

| Capa | Archivo | Descripción |
|------|---------|-------------|
| Domain | `Policies/VehiclePlan/VehiclePlan.cs` | Value Object del plan |
| Domain | `Policies/VehiclePlan/VehiclePlanCatalog.cs` | Catálogo estático de los 3 planes |
| Domain | `Policies/VehiclePlan/VehiclePlanSelection.cs` | Snapshot embebido en Policy |
| Domain | `Policies/VehiclePlan/VehicleQuotation.cs` | Value Object resultado del motor |
| Domain | `Policies/VehiclePlan/VehiclePricingService.cs` | Motor de cálculo de tasa técnica |
| Domain | `Policies/VehiclePlan/HighSinistrabilityBrands.cs` | Lista configurable de marcas |
| Domain | `Exceptions/InvalidVehiclePlanException.cs` | Excepción plan no encontrado |
| Application | `VehiclePlans/DTOs/VehiclePlanDto.cs` | DTO de respuesta del catálogo |
| Application | `VehiclePlans/DTOs/VehicleQuotationDto.cs` | DTO de respuesta de cotización |
| Application | `VehiclePlans/Queries/GetVehiclePlans/GetVehiclePlansQuery.cs` | Query MediatR |
| Application | `VehiclePlans/Queries/GetVehiclePlans/GetVehiclePlansHandler.cs` | Handler MediatR |
| Application | `VehiclePlans/Queries/CalculateVehicleQuotation/CalculateVehicleQuotationQuery.cs` | Query MediatR |
| Application | `VehiclePlans/Queries/CalculateVehicleQuotation/CalculateVehicleQuotationHandler.cs` | Handler MediatR |
| API | `Controllers/VehiclePlansController.cs` | Controller REST — análogo a `LifePlansController` |

#### Archivos a modificar

| Capa | Archivo | Cambio |
|------|---------|--------|
| Domain | `Policies/Policy.cs` | Añadir propiedad `VehiclePlan` y factory `CreateVehiclePolicy()` |
| Application | `Policies/Commands/CreatePolicy/CreatePolicyCommand.cs` | Añadir 4 campos Vehicle |
| Application | `Policies/Commands/CreatePolicy/CreatePolicyHandler.cs` | Añadir rama `case PolicyType.Vehicle` |
| Application | `Policies/Commands/CreatePolicy/CreatePolicyValidator.cs` | Añadir validaciones para campos Vehicle |

---

### 2.5 Notas de Implementación

- **`HighSinistrabilityBrands`:** Implementar como una clase estática con una propiedad `IReadOnlyHashSet<string>` de marcas en minúsculas. La comparación debe ser case-insensitive. La lista inicial es solo ejemplificativa (ej. `"renault"`, `"chevrolet"`); el equipo actuarial debe revisar y confirmar la lista definitiva antes del paso a `APPROVED`.
- **`VehiclePricingService`:** Análogo a `HealthPlanPricingService` y `LifePlanPricingService`. Recibe `commercialValue`, `vehicleYear`, `brand`, `currentYear` y retorna un `VehicleQuotation`. Toda la lógica matemática vive aquí — no en el handler ni en el controlador.
- **Aritmética decimal:** Usar exclusivamente `decimal` (no `double` ni `float`) para todos los cálculos de primas. El CEILING se implementa como `Math.Ceiling(value / 1000m) * 1000m`.
- **El año actual:** El handler inyecta `DateOnly.FromDateTime(DateTime.UtcNow)` y extrae `.Year` para el cálculo de antigüedad. No usar `DateTime.Now`.
- **`PolicyType.Vehicle = 3`** ya existe en el enum — no modificar.
- **`insuredBirthDate`:** Se sigue capturando para la póliza (campo de `InsuredPerson`), pero no interviene en el cálculo de la prima vehicular (a diferencia de Salud).
- **`insuredAmount`** en la póliza Vehicle: almacenar la `primaBase` (Plan Estándar) como referencia del valor asegurado base. La prima final del plan seleccionado se almacena en `MonthlyPremium`.
- **Frontend:** El `VehicleDataFormComponent` emite los datos del vehículo cuando el formulario es válido. El padre (`PolicyCreateComponent`) llama a `vehicleSvc.calculate()` solo cuando el usuario hace clic en "Cotizar" (no on-change), para evitar llamadas innecesarias al API.

---

## 3. LISTA DE TAREAS

> Checklist accionable para todos los agentes. Marcar cada ítem (`[x]`) al completarlo.
> El Orchestrator monitorea este checklist para determinar el progreso.

### Backend

#### Dominio
- [x] Crear `Policies/VehiclePlan/VehiclePlan.cs` — Value Object con Id, Name, PriceMultiplier, Coverages, Assistances
- [x] Crear `Policies/VehiclePlan/VehiclePlanCatalog.cs` — catálogo estático de 3 planes
- [x] Crear `Policies/VehiclePlan/VehiclePlanSelection.cs` — snapshot embebido en Policy
- [x] Crear `Policies/VehiclePlan/VehicleQuotation.cs` — Value Object transitorio del motor de cálculo
- [x] Crear `Policies/VehiclePlan/VehiclePricingService.cs` — motor de tasa técnica + reglas actuariales
- [x] Crear `Policies/VehiclePlan/HighSinistrabilityBrands.cs` — lista configurable (revisar con equipo actuarial)
- [x] Crear `Exceptions/InvalidVehiclePlanException.cs`
- [x] Modificar `Policies/Policy.cs` — añadir propiedad `VehiclePlan` y factory `CreateVehiclePolicy()`

#### Application
- [x] Crear `VehiclePlans/DTOs/VehiclePlanDto.cs`
- [x] Crear `VehiclePlans/DTOs/VehicleQuotationDto.cs` (con lista de `VehiclePlanOptionDto`)
- [x] Crear `VehiclePlans/Queries/GetVehiclePlans/GetVehiclePlansQuery.cs`
- [x] Crear `VehiclePlans/Queries/GetVehiclePlans/GetVehiclePlansHandler.cs`
- [x] Crear `VehiclePlans/Queries/CalculateVehicleQuotation/CalculateVehicleQuotationQuery.cs`
- [x] Crear `VehiclePlans/Queries/CalculateVehicleQuotation/CalculateVehicleQuotationHandler.cs`
- [x] Modificar `Policies/Commands/CreatePolicy/CreatePolicyCommand.cs` — añadir 4 campos Vehicle
- [x] Modificar `Policies/Commands/CreatePolicy/CreatePolicyHandler.cs` — añadir rama `PolicyType.Vehicle`
- [x] Modificar `Policies/Commands/CreatePolicy/CreatePolicyValidator.cs` — validaciones campos Vehicle

#### API
- [x] Crear `Controllers/VehiclePlansController.cs` — `GET /api/v1/vehicle-plans` y `GET /api/v1/vehicle-plans/calculate`
- [x] Registrar rutas en Program.cs (si aplica — verificar convención del proyecto)

#### Tests Backend
- [x] `Calculate_NewVehicle_AppliesRate2_2Percent` — prima base correcta tasa 2.2%
- [x] `Calculate_UsedRecentVehicle_AppliesRate2_8Percent` — prima base correcta tasa 2.8%
- [x] `Calculate_UsedOldVehicle_AppliesRate3_5Percent` — prima base correcta tasa 3.5%
- [x] `Calculate_HighSinistrabilityBrand_Applies10PercentSurcharge` — recargo 10% aplicado correctamente
- [x] `Calculate_LowCommercialValue_EnforcesMinimumPremium` — piso de $800.000 aplicado
- [x] `Calculate_PremiumPlan_CeilingRoundingApplied` — redondeo al mil superior correcto
- [x] `Calculate_CompletePlan_Is1_20xAnnualBase` — primaCompleto = primaBase × 1.20
- [x] `Calculate_PremiumPlan_Is1_45xAnnualBase` — primaPremium = primaBase × 1.45
- [x] `Select_*_AnnualWithDiscountIs5PercentOff*` — descuento anual correcto (3 tests)
- [x] `Handle_VehiclePolicy_Standard_CreatesPolicy` — happy path creación de póliza Vehicle
- [x] `Handle_VehiclePolicy_InvalidPlanId_ThrowsInvalidVehiclePlanException` — excepción por planId inválido
- [x] `Handle_ReturnsThreePlans_WithCorrectIds` — endpoint calculate devuelve 3 planes
- [x] `Handle_VehiclePolicy_RepositoryAddCalledOnce` — validación de persistencia
- [x] 43 tests en total — 27 Domain + 16 Application — todos pasando

---

### Frontend

#### Implementación
- [x] Crear `core/models/vehicle-plan-selection.model.ts` — interfaces `VehiclePlan`, `VehiclePlanOption`, `VehicleQuotationResult`
- [x] Crear `core/service/vehicle-plans.service.ts` — métodos `getPlans()` y `calculate()`
- [x] Crear `ui/blocks/vehicle-data-form/` — formulario reactivo para datos del vehículo
- [x] Crear `ui/blocks/vehicle-plan-selector/` — selector de plan (3 tarjetas con primas calculadas)
- [x] Crear `ui/blocks/vehicle-plan-preview/` — resumen del plan seleccionado (incluye asistencias)
- [x] Modificar `ui/Pages/policy-create/policy-create.component.ts` — añadir signals Vehicle, inyectar `VehiclePlansService`, añadir pasos condicionales al stepper
- [x] Modificar `ui/Pages/policy-create/policy-create.component.html` — añadir pasos Vehicle al MatStepper con directiva `@if (isVehicleType())`

#### Tests Frontend
- [x] `VehicleDataFormComponent renders form fields correctly`
- [x] `VehicleDataFormComponent emits data when form is valid`
- [x] `VehicleDataFormComponent does not emit when commercialValue is zero or negative`
- [x] `VehiclePlanSelectorComponent renders three plan cards`
- [x] `VehiclePlanSelectorComponent emits selected plan on click`
- [x] `VehiclePlanPreviewComponent displays monthly premium correctly`
- [x] `VehiclePlanPreviewComponent shows assistances only for Premium plan`
- [x] `VehiclePlansService.calculate() calls correct endpoint with query params`
- [x] 47 specs en total (service + 3 componentes) — 128/128 suite completa SUCCESS

---

### QA
- [ ] Ejecutar skill `/gherkin-case-generator` → criterios CRITERIO-1.1 a CRITERIO-4.3
- [ ] Ejecutar skill `/risk-identifier` → clasificación ASD de riesgos del módulo
- [ ] Verificar tabla de cálculos con los 5 ejemplos de la sección 1.4 contra la implementación real
- [ ] Validar que `HighSinistrabilityBrands` fue revisada y aprobada por el equipo actuarial antes de `APPROVED`
- [ ] Revisar cobertura de tests contra todos los criterios de aceptación (HU-1 a HU-4)
- [ ] Validar que todas las reglas de negocio RN-01 a RN-16 están cubiertas por al menos un test
- [ ] Probar flujo completo E2E: captura datos → cotización → selección plan → confirmación → póliza creada
- [ ] Actualizar estado spec a `status: IMPLEMENTED` al completar

---

> **Revisión actuarial pendiente:** La lista de marcas de alta siniestralidad en `HighSinistrabilityBrands` es un ejemplo ilustrativo. El equipo actuarial debe revisar y aprobar la lista definitiva antes de mover esta spec a `APPROVED`.
