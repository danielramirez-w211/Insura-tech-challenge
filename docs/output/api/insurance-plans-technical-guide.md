---
title: Insurance Plans — Technical Reference
scope: Todos los módulos de planes (SPEC-008 al SPEC-012)
status: IMPLEMENTED
updated: 2026-04-15
branch: implement_polices_plan_house
---

# Guía Técnica — Módulos de Planes de Seguros

Referencia completa de los cinco módulos de cotización implementados en InsuraTech:
**Salud**, **Vida**, **Vehículo**, **Viaje** y **Hogar**.

---

## 1. Arquitectura Común

Todos los módulos siguen el mismo patrón de capas:

```
API Controller
    └── MediatR → Query/Command Handler
            └── Domain Service (Pricing/Rating)
                    └── Domain Catalog (estático)
                            └── Snapshot (ValueObject) → Policy.cs
```

### PolicyType enum

```csharp
public enum PolicyType
{
    Life    = 1,
    Health  = 2,
    Vehicle = 3,
    Home    = 4,
    Travel  = 5
}
```

### Flujo de creación de póliza (común)

```
POST /api/v1/policies
  → CreatePolicyCommand
  → CreatePolicyHandler
        ├── if Health  → HealthPlanPricingService.Calculate()
        ├── if Life    → LifePlanPricingService.Calculate()
        ├── if Vehicle → VehiclePricingService.Calculate() + Select()
        ├── if Home    → HomePricingService.Calculate() + Select()
        └── if Travel  → TravelRatingService.Calculate()
```

---

## 2. Plan Salud (SPEC-008)

### Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/health-plans` | Lista los 4 planes disponibles |
| `GET` | `/api/v1/health-plans/calculate?planId=X&birthDate=YYYY-MM-DD` | Calcula prima mensual con factor de edad |

### Catálogo de planes

| ID | Nombre | Monto base anual COP |
|----|--------|---------------------|
| `basic` | Básico | 300.000 |
| `salud-global` | Salud Global | 380.000 |
| `salud-premium` | Salud Premium | 450.000 |
| `salud-vida-total` | Salud Vida Total | 600.000 |

### Motor de cálculo — `HealthPlanPricingService`

```
finalAmount = baseAmount + (baseAmount × factorPercent / 100)
monthlyPremium = finalAmount / 12

Factor de edad:
  18–35 años → 0%
  36–58 años → +4%
  59–73 años → +8%
  < 18 o ≥ 74 → excepción de dominio
```

### Reglas de negocio

- Período fijo: 365 días (endDate = startDate + 365 días, calculado en backend)
- Prima mensual no editable por el usuario
- Rango de edad permitido: 18–73 años

### Response `GET /calculate`

```json
{
  "planId": "salud-premium",
  "planName": "Salud Premium",
  "baseAmount": 450000,
  "ageFactorPercentage": 4,
  "ageFactorAddition": 18000,
  "finalAmount": 468000,
  "monthlyPremium": 39000
}
```

---

## 3. Plan Vida (SPEC-009)

### Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/life-plans` | Lista los 3 planes con desglose de beneficios |
| `GET` | `/api/v1/life-plans/calculate?planId=X&birthDate=YYYY-MM-DD` | Valida edad y retorna cotización |

### Catálogo de planes

| ID | Nombre | Prima anual | Prima mensual | Muerte | Gastos fúnebres | Gastos entierro | Remuneración |
|----|--------|------------|---------------|--------|-----------------|-----------------|--------------|
| `plan-vida` | Plan Vida | 120.000 | 10.000 | 30.000.000 | 3.000.000 | 1.500.000 | 1.500.000 |
| `vida-familia` | Vida para la Familia | 150.000 | 12.500 | 60.000.000 | 5.000.000 | 2.500.000 | 2.500.000 |
| `vida-premium` | Vida Premium | 200.000 | 16.667 | 100.000.000 | 8.000.000 | 4.000.000 | 4.000.000 |

> Todos los valores en COP.

### Motor de cálculo — `LifePlanPricingService`

```
monthlyPremium = annualPremium / 12  (precio fijo, sin factor de edad)
```

### Reglas de negocio

- Sin factor de edad — costo invariante dentro del rango permitido
- Rango de edad permitido: 18–65 años
- Período fijo: 365 días

---

## 4. Plan Vehículo (SPEC-010)

### Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/vehicle-plans` | Lista los 3 planes con coberturas y asistencias |
| `GET` | `/api/v1/vehicle-plans/calculate?commercialValue=X&vehicleYear=Y&brand=Z` | Motor de cotización |

### Catálogo de planes

| ID | Nombre | Multiplicador | Coberturas | Asistencias |
|----|--------|--------------|------------|-------------|
| `standard` | Plan Estándar | ×1.00 | Robo, Pérdida parcial/total, Daños a terceros, Rayones | — |
| `complete` | Plan Completo | ×1.20 | + Pérdida de llaves, Daño mecánico, Daño eléctrico | Grúa |
| `premium` | Plan Premium | ×1.45 | (ídem Completo) | Grúa, Carro de repuesto, Mecánico a casa, Conductor elegido |

### Motor de cálculo — `VehiclePricingService`

```
Paso 1 — Antigüedad y tasa técnica (anual):
  Nuevo (0 años)          → 2.2%
  Usado Reciente (1–5 años) → 2.8%
  Usado Antiguo (>5 años)   → 3.5%

Paso 2 — Prima bruta base:
  gross = commercialValue × techRate

Paso 3 — Recargo por marca de alta siniestralidad (+10%):
  if (brand in HighSinistrabilityBrands) gross *= 1.10

Paso 4 — Prima mínima anual:
  gross = max(gross, 800.000 COP)

Paso 5 — Redondeo al mil superior (CEILING):
  annualBase = ceil(gross / 1000) × 1000

Paso 6 — Primas por plan:
  annualComplete = ceil(annualBase × 1.20 / 1000) × 1000
  annualPremium  = ceil(annualBase × 1.45 / 1000) × 1000

Paso 6b — Prima mensual:
  monthly = round(annual / 12)

Paso 7 — Descuento pago anual (5%):
  annualWithDiscount = finalAnnual × 0.95
```

### Response `GET /calculate`

```json
{
  "commercialValue": 50000000,
  "vehicleYear": 2020,
  "brand": "Toyota",
  "vehicleAge": 6,
  "ageCategory": "Usado Antiguo",
  "technicalRate": 0.035,
  "hasBrandSurcharge": false,
  "baseMonthlyPremium": 145833,
  "completePlanPremium": 175000,
  "premiumPlanPremium": 212500,
  "annualBase": 1750000,
  "annualComplete": 2100000,
  "annualPremium": 2537000
}
```

---

## 5. Plan Viaje (SPEC-00x)

### Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/travel-plans/calculate?tripType=X&continent=Y&durationDays=Z` | Calcula precio del seguro de viaje |

### Tipos de viaje (`TripType`)

```csharp
Nacional      = 1
Internacional = 2
```

### Continentes (`Continent`) — Solo para Internacional

```csharp
America = 1, Europe = 2, Africa = 3, Asia = 4, Oceania = 5
```

### Motor de cálculo — `TravelRatingService`

```
NACIONAL:
  effectiveDays = min(durationDays, 30)   ← cap de cálculo en 30 días
  total = 2.200 + (effectiveDays - 1) × 1.200  [COP]
  Duración máxima permitida: 365 días

INTERNACIONAL:
  totalUSD = 30 + (durationDays - 1) × 5  [USD]
  totalCOP = round(totalUSD × TRM)
  TRM obtenida de ITrmService (externa)
```

### Reglas de negocio

- Duración mínima: 1 día
- Duración máxima: 365 días (excepción `TravelDurationExceededException`)
- Internacional requiere continente no nulo y TRM disponible
- Si TRM no disponible → HTTP 503

---

## 6. Plan Hogar (SPEC-012)

### Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/home-plans` | Lista los 3 paquetes con coberturas incluidas |
| `POST` | `/api/v1/home-plans/calculate` | Motor de cotización (body: JSON) |

> Se usa POST para calculate porque el payload incluye una lista variable de coberturas.

### Catálogo de paquetes

| ID | Nombre | Coberturas incluidas |
|----|--------|---------------------|
| `basic` | Básico | Incendio/Explosión, Plomería |
| `standard` | Estándar | + Robo, Daño Eléctrico |
| `premium` | Premium | + 5 coberturas adicionales (Daño Estético, Vidrios, RC, Inhabitabilidad, Asistencia Hogar) |

### Coberturas individuales disponibles (`HomeCoverage` enum)

| Enum | Nombre |
|------|--------|
| `FireExplosion` | Incendio y Explosión (**obligatoria**) |
| `Theft` | Hurto |
| `Plumbing` | Daños por plomería |
| `AestheticDamage` | Daño estético |
| `ElectricalDamage` | Daño eléctrico |
| `GlassBreakage` | Rotura de vidrios |
| `CivilLiability` | Responsabilidad civil |
| `Uninhabitability` | Inhabitabilidad |
| `LegalDefense` | Defensa jurídica |
| `HomeAssistance` | Asistencia al hogar |
| `WaterDamageExpert` | Peritaje daños por agua |

### Motor de cálculo — `HomePricingService`

```
Paso 1 — Prima base mensual:
  basePremium = propertyValue × 0.01

Paso 2 — Coberturas variables (% sobre base):
  Theft           → +12% de base
  Plumbing        → +5% de base
  ElectricalDamage → +7% de base

Paso 3 — Coberturas fijas (COP/mes):
  GlassBreakage    → +50.000
  LegalDefense     → +30.000
  HomeAssistance   → +45.000
  WaterDamageExpert → +65.000
  CivilLiability:
    Estrato 1-2    → +100.000
    Estrato 3-4    → +175.000
    Estrato 5-6    → +250.000

Paso 4 — Multiplicadores de riesgo:
  Inmueble nuevo (<10 años)  → ×0.95  (-5%)
  Inmueble antiguo (>30 años) + Plomería → ×1.10 (+10%)
  Estrato 5-6 + Hurto        → ×1.10 (+10% sobre hurto)
  Más de 5 habitantes        → +30.000 COP fijo
  Más de 5 coberturas        → ×0.95  (-5% descuento multicobertura)

Paso 5 — Redondeo al mil superior (CEILING):
  finalMonthlyPremium = ceil(total / 1000) × 1000
```

### Request body `POST /calculate`

```json
{
  "propertyValue": 300000000,
  "constructionYear": 2018,
  "stratum": 3,
  "occupants": 3,
  "propertyType": "Apartment",
  "selectedCoverages": ["FireExplosion", "Plumbing", "Theft"]
}
```

### Tipos de inmueble (`HomePropertyType`)

```csharp
House = 1, Apartment = 2, CommercialPremises = 3
```

---

## 7. Creación de Póliza — `POST /api/v1/policies`

Todos los planes se persisten a través del mismo endpoint. Los campos requeridos varían por tipo.

### Campos comunes (todos los tipos)

```json
{
  "type": "Health|Life|Vehicle|Home|Travel",
  "insuredPersonDocumentType": "CC|TI|CE|PP",
  "insuredPersonDocumentNumber": "string",
  "insuredPersonFullName": "string",
  "insuredPersonEmail": "string",
  "insuredPersonPhone": "string",
  "insuredPersonBirthDate": "YYYY-MM-DD",
  "insuredPersonCity": "string",
  "coverageStartDate": "YYYY-MM-DD",
  "coverageEndDate": "YYYY-MM-DD"
}
```

### Campos adicionales por tipo

#### Health / Life
```json
{ "planId": "string" }
```

#### Vehicle
```json
{
  "vehiclePlanId": "string",
  "vehicleCommercialValue": 50000000,
  "vehicleYear": 2020,
  "vehicleBrand": "Toyota"
}
```

#### Home
```json
{
  "homePlanPackageId": "string",
  "homePropertyValue": 300000000,
  "homeConstructionYear": 2018,
  "homeStratum": 3,
  "homeOccupants": 3,
  "homePropertyType": "Apartment",
  "homeSelectedCoverages": ["FireExplosion", "Plumbing"]
}
```

#### Travel
```json
{
  "travelTripType": "Nacional|Internacional",
  "travelContinent": "America|Europe|Africa|Asia|Oceania",
  "travelDurationDays": 15
}
```

---

## 8. Dominio — Value Objects y Snapshots

Cada plan persiste un snapshot inmutable dentro del aggregate `Policy`:

| Plan | Snapshot (ValueObject) | Propiedad en Policy |
|------|------------------------|---------------------|
| Salud | `HealthPlanSelection` | `Policy.HealthPlan` |
| Vida | `LifePlanSelection` | `Policy.LifePlan` |
| Vehículo | `VehiclePlanSelection` | `Policy.VehiclePlan` |
| Hogar | `HomePlanSelection` | `Policy.HomePlan` |
| Viaje | `TravelPlanSelection` | `Policy.TravelPlan` |

Todos los snapshots extienden `ValueObject` (base en `InsuraTech.Domain.Common`) e implementan `GetEqualityComponents()`.

---

## 9. Errores de dominio comunes

| Excepción | Plan | Condición |
|-----------|------|-----------|
| `UnderageInsuredException` | Salud, Vida | Edad < 18 |
| `OverageInsuredException` | Salud | Edad ≥ 74 |
| `InvalidHealthPlanException` | Salud | planId no existe |
| `InvalidVehiclePlanException` | Vehículo | planId no existe |
| `TravelDurationInvalidException` | Viaje | durationDays < 1 |
| `TravelDurationExceededException` | Viaje | durationDays > 365 |
| `TrmUnavailableException` | Viaje Internacional | TRM null o ≤ 0 |
| `InvalidContinentException` | Viaje Internacional | continent null |

---

## 10. Directorios clave

```
Backend/src/
├── InsuraTech.Domain/Policies/
│   ├── HealthPlan/     HealthPlan, HealthPlanCatalog, HealthPlanPricingService, HealthPlanSelection
│   ├── LifePlan/       LifePlan, LifePlanCatalog, LifePlanPricingService, LifePlanSelection
│   ├── VehiclePlan/    VehiclePlan, VehiclePlanCatalog, VehiclePricingService, VehicleQuotation, VehiclePlanSelection
│   ├── HomePlan/       HomeCoverage, HomePlanPackage, HomePlanPackageCatalog, HomePricingService, HomeQuotation, HomePlanSelection
│   └── TravelPlan/     TravelRatingService, TravelPlanSelection, Continent, TripType
│
├── InsuraTech.Application/
│   ├── HealthPlans/    Queries: GetHealthPlans, CalculateHealthPlan
│   ├── LifePlans/      Queries: GetLifePlans, CalculateLifePlan
│   ├── VehiclePlans/   Queries: GetVehiclePlans, CalculateVehiclePlans
│   ├── HomePlans/      Queries: GetHomePlans, CalculateHomeQuotation
│   └── TravelPlans/    Queries: CalculateTravelPlan
│
└── InsuraTech.API/Controllers/
    ├── HealthPlansController.cs    /api/v1/health-plans
    ├── LifePlansController.cs      /api/v1/life-plans
    ├── VehiclePlansController.cs   /api/v1/vehicle-plans
    ├── HomePlansController.cs      /api/v1/home-plans
    ├── TravelPlansController.cs    /api/v1/travel-plans
    └── PoliciesController.cs       /api/v1/policies

frontend/src/app/features/policies/
├── core/
│   ├── models/   health-plan.model, life-plan.model, vehicle-plan.model,
│   │             home-plan-selection.model, travel-plan.model
│   └── service/  health-plans.service, life-plans.service, vehicle-plans.service,
│                 home-plans.service, travel-plans.service
└── ui/blocks/
    ├── health-plan-selector/
    ├── life-plan-selector/
    ├── vehicle-data-form/ + vehicle-plan-preview/
    ├── home-data-form/    + home-plan-preview/
    └── travel-data-form/  + travel-plan-preview/
```
