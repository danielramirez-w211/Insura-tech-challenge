---
id: SPEC-003
status: APPROVED
feature: health-plan-pricing
created: 2026-04-06
updated: 2026-04-06
author: spec-generator
version: "1.0"
related-specs: ["SPEC-001", "SPEC-002"]
---

# Spec: Módulo de Planes de Salud con Cálculo Automático de Prima

> **Estado:** `DRAFT` → aprobar con `status: APPROVED` antes de iniciar implementación.
> **Ciclo de vida:** DRAFT → APPROVED → IN_PROGRESS → IMPLEMENTED → DEPRECATED

---

## 1. REQUERIMIENTOS

### Descripción

Esta spec cubre la migración del módulo de creación de pólizas de tipo **Health** (Salud) desde un modelo de ingreso manual de montos a un modelo basado en **planes predefinidos**. El costo de la póliza se calcula automáticamente a partir del plan seleccionado y la edad del asegurado, eliminando la vulnerabilidad funcional que permitía al usuario ingresar montos arbitrarios. Para mayores de 73 años se bloquea el flujo estándar y se requiere un formulario de preexistencias.

### Requerimiento de Negocio

La selección manual del monto asegurado representa un riesgo financiero y operativo para la compañía. Se requiere estandarizar la oferta de seguros de salud mediante cuatro planes con valor base fijo y un factor de incremento determinístico basado en el rango etario del solicitante. Los agentes de seguros deben seleccionar un plan; el sistema calcula y presenta el costo final antes de confirmar la creación. Los asegurados de 74 años en adelante requieren evaluación especial mediante formulario de preexistencias y enfermedades (flujo fuera de alcance de esta spec — pendiente de definición).

---

### Historias de Usuario

---

#### HU-01: Catálogo de Planes de Salud

```
Como:        Agente de seguros
Quiero:      Ver un catálogo de planes de salud disponibles con su valor base
Para:        Seleccionar el plan adecuado para el cliente sin ingresar montos manualmente

Prioridad:   Alta
Estimación:  S
Dependencias: Ninguna
Capa:        Backend + Frontend
```

#### Criterios de Aceptación — HU-01

**Happy Path**
```gherkin
CRITERIO-1.1: Listado de planes de salud disponibles
  Dado que:  El agente accede al módulo de creación de póliza tipo Health
  Cuando:    El sistema carga el formulario de nueva póliza de salud
  Entonces:  Se muestran exactamente 4 planes disponibles:
             - Básico             → $300.000
             - Salud Global       → $380.000
             - Salud Premium      → $450.000
             - Salud Vida Total   → $600.000
             Y el campo de ingreso manual de "Monto Asegurado" no está visible
```

**Error Path**
```gherkin
CRITERIO-1.2: Intento de crear póliza de salud sin seleccionar plan
  Dado que:  El agente está en el formulario de nueva póliza tipo Health
  Cuando:    Intenta confirmar sin seleccionar un plan
  Entonces:  Se muestra el error de validación "Debe seleccionar un plan de salud"
             Y el botón de confirmación permanece deshabilitado
```

**Edge Case**
```gherkin
CRITERIO-1.3: Planes solo se muestran para pólizas de tipo Health
  Dado que:  El agente está creando una póliza de tipo Vehicle, Life, Home o Travel
  Cuando:    El sistema carga el formulario
  Entonces:  No se muestra el selector de planes de salud
             Y el campo de monto asegurado manual permanece disponible
```

---

#### HU-02: Cálculo Automático de Prima por Edad

```
Como:        Agente de seguros
Quiero:      Que el sistema calcule automáticamente el monto de la póliza
             basándose en el plan seleccionado y la edad del asegurado
Para:        Eliminar errores de cálculo manual y garantizar precios correctos

Prioridad:   Alta
Estimación:  M
Dependencias: HU-01
Capa:        Backend + Frontend
```

#### Criterios de Aceptación — HU-02

**Happy Path — Rango 18-35 años (sin incremento)**
```gherkin
CRITERIO-2.1: Cálculo para asegurado entre 18 y 35 años
  Dado que:  El asegurado tiene 28 años
             Y el agente selecciona el plan "Salud Global" ($380.000)
  Cuando:    El sistema calcula el monto final
  Entonces:  El monto asegurado = $380.000 (valor base sin incremento)
             Y se muestra el desglose: "Plan Salud Global: $380.000 | Incremento por edad: 0%"
```

**Happy Path — Rango 36-58 años (incremento 4%)**
```gherkin
CRITERIO-2.2: Cálculo para asegurado entre 36 y 58 años
  Dado que:  El asegurado tiene 45 años
             Y el agente selecciona el plan "Salud Premium" ($450.000)
  Cuando:    El sistema calcula el monto final
  Entonces:  El monto asegurado = $450.000 × 1.04 = $468.000
             Y se muestra el desglose: "Plan Salud Premium: $450.000 | Incremento por edad: 4% (+$18.000)"
```

**Happy Path — Rango 59-73 años (incremento 8%)**
```gherkin
CRITERIO-2.3: Cálculo para asegurado entre 59 y 73 años
  Dado que:  El asegurado tiene 67 años
             Y el agente selecciona el plan "Salud Vida Total" ($600.000)
  Cuando:    El sistema calcula el monto final
  Entonces:  El monto asegurado = $600.000 × 1.08 = $648.000
             Y se muestra el desglose: "Plan Salud Vida Total: $600.000 | Incremento por edad: 8% (+$48.000)"
```

**Happy Path — Límites exactos de rango**
```gherkin
CRITERIO-2.4: Cálculo en los límites exactos de edad
  Dado que:  Se prueba con edades límite exactas: 18, 35, 36, 58, 59, 73
  Cuando:    El sistema calcula para el plan "Básico" ($300.000)
  Entonces:
    - Edad 18 → $300.000 (0%)
    - Edad 35 → $300.000 (0%)
    - Edad 36 → $312.000 (4%)
    - Edad 58 → $312.000 (4%)
    - Edad 59 → $324.000 (8%)
    - Edad 73 → $324.000 (8%)
```

**Error Path — Menor de 18 años**
```gherkin
CRITERIO-2.5: Rechazo de asegurado menor de 18 años
  Dado que:  El asegurado tiene 17 años (o menos)
  Cuando:    El agente intenta crear la póliza de salud
  Entonces:  Se retorna HTTP 422 con mensaje:
             "El asegurado debe tener al menos 18 años para contratar un plan de salud"
             Y no se crea la póliza
```

---

#### HU-03: Bloqueo y Derivación para Mayores de 73 Años

```
Como:        Agente de seguros
Quiero:      Que el sistema bloquee el flujo estándar para asegurados de 74 años en adelante
             y muestre un mensaje de derivación al formulario de preexistencias
Para:        Cumplir con la política de evaluación de riesgo para asegurados de alto riesgo etario

Prioridad:   Alta
Estimación:  S
Dependencias: HU-01, HU-02
Capa:        Backend + Frontend
```

#### Criterios de Aceptación — HU-03

**Happy Path — Detección y bloqueo**
```gherkin
CRITERIO-3.1: Bloqueo del flujo estándar para mayores de 73 años
  Dado que:  El asegurado tiene 74 años o más
             Y el tipo de póliza seleccionado es Health
  Cuando:    El agente ingresa la fecha de nacimiento del asegurado
  Entonces:  El formulario estándar queda deshabilitado
             Y se muestra el mensaje:
             "El asegurado supera los 73 años. Para continuar, debe completar
              el Formulario de Preexistencias y Enfermedades. Contacte al área de
              suscripción especial."
             Y se muestra un botón "Solicitar formulario de preexistencias" (pendiente — fuera de alcance)
```

**Error Path — Intento de bypass vía API**
```gherkin
CRITERIO-3.2: El backend rechaza la creación para asegurados de 74+ años
  Dado que:  Se hace POST /api/v1/policies con tipo Health y un asegurado de 74 años
  Cuando:    El comando llega al handler de creación
  Entonces:  Se retorna HTTP 422 con mensaje:
             "Los asegurados de 74 años o más requieren evaluación especial de preexistencias.
              El flujo estándar de creación de pólizas de salud no está disponible para este rango etario."
             Y no se persiste ningún registro
```

**Edge Case — Límite exacto de 74 años**
```gherkin
CRITERIO-3.3: Edad exacta de 74 años activa el bloqueo
  Dado que:  El asegurado cumple 74 años hoy (fecha de nacimiento = hoy - 74 años)
  Cuando:    El sistema evalúa la elegibilidad
  Entonces:  Se activa el bloqueo (74 está incluido en el rango restringido)
             Y la edad 73 NO activa el bloqueo
```

---

#### HU-04: Previsualización del Costo Antes de Confirmar

```
Como:        Agente de seguros
Quiero:      Ver un resumen del costo calculado antes de confirmar la creación de la póliza
Para:        Verificar que el plan y el monto son correctos antes de comprometer al cliente

Prioridad:   Media
Estimación:  S
Dependencias: HU-01, HU-02
Capa:        Frontend
```

#### Criterios de Aceptación — HU-04

**Happy Path**
```gherkin
CRITERIO-4.1: Resumen de costo visible antes de confirmar
  Dado que:  El agente ha seleccionado el plan y el sistema ha calculado el monto
  Cuando:    El agente hace clic en "Calcular" o avanza al paso de confirmación
  Entonces:  Se muestra un panel de resumen con:
             - Nombre del plan seleccionado
             - Monto base del plan
             - Edad del asegurado
             - Factor de incremento aplicado (0%, 4% u 8%)
             - Monto adicional por edad (en $)
             - Monto final a asegurar (resaltado)
             Y el agente debe hacer clic en "Confirmar y Crear Póliza" para continuar
```

**Edge Case**
```gherkin
CRITERIO-4.2: Recálculo automático al cambiar de plan o fecha de nacimiento
  Dado que:  El agente ya tiene un cálculo previo visible
  Cuando:    Cambia el plan seleccionado o modifica la fecha de nacimiento
  Entonces:  El resumen se actualiza automáticamente sin recargar la página
             Y el botón "Confirmar" se deshabilita hasta que el nuevo cálculo esté listo
```

---

#### HU-05: Persistencia de la Selección de Plan en la Póliza

```
Como:        Auditor o agente que consulta pólizas existentes
Quiero:      Que el plan de salud seleccionado quede registrado en la póliza creada
Para:        Mantener trazabilidad del producto contratado y facilitar renovaciones

Prioridad:   Alta
Estimación:  S
Dependencias: HU-01, HU-02, HU-03
Capa:        Backend
```

#### Criterios de Aceptación — HU-05

**Happy Path**
```gherkin
CRITERIO-5.1: El plan seleccionado se persiste en la póliza
  Dado que:  Se crea una póliza de salud con el plan "Salud Premium"
  Cuando:    Se consulta la póliza vía GET /api/v1/policies/{id}
  Entonces:  La respuesta incluye:
             {
               "healthPlan": {
                 "planId": "salud-premium",
                 "planName": "Salud Premium",
                 "baseAmount": 450000,
                 "ageFactorPercentage": 4,
                 "ageFactorAmount": 18000,
                 "finalAmount": 468000
               }
             }
             Y el campo "insuredAmount" refleja el monto calculado ($468.000)
```

**Edge Case**
```gherkin
CRITERIO-5.2: Pólizas no-Health no tienen campo healthPlan
  Dado que:  Se consulta una póliza de tipo Vehicle o Life
  Cuando:    El agente obtiene la respuesta
  Entonces:  El campo "healthPlan" es null o está ausente en la respuesta
```

---

### Reglas de Negocio

**RN-01 — Planes disponibles y valores base**
| Plan ID | Plan Name | Monto Base |
|---------|-----------|-----------|
| `basic` | Básico | $300.000 |
| `salud-global` | Salud Global | $380.000 |
| `salud-premium` | Salud Premium | $450.000 |
| `salud-vida-total` | Salud Vida Total | $600.000 |

**RN-02 — Factores de incremento por rango etario**
| Rango de Edad | Factor | Fórmula |
|--------------|--------|---------|
| 18 – 35 años | 0% | `montoFinal = montoBase` |
| 36 – 58 años | +4% | `montoFinal = montoBase × 1.04` |
| 59 – 73 años | +8% | `montoFinal = montoBase × 1.08` |
| ≥ 74 años | Bloqueado | Requiere formulario de preexistencias |

**RN-03 — Edad mínima:** El asegurado debe tener al menos 18 años al momento de crear la póliza. La edad se calcula en años completos a partir de `DateOnly` de nacimiento vs. fecha actual (`DateOnly.FromDateTime(DateTime.UtcNow)`).

**RN-04 — El monto calculado es inmutable post-creación:** Una vez creada la póliza, el `insuredAmount` calculado no puede modificarse. Para cambiar el plan, la póliza debe cancelarse y recrearse.

**RN-05 — Los planes de salud aplican exclusivamente a `PolicyType.Health`:** Intentar usar la lógica de planes en otro tipo de póliza debe resultar en error de dominio.

**RN-06 — El cálculo ocurre en el dominio, no en el frontend:** El frontend envía `planId` y la fecha de nacimiento; el backend realiza el cálculo y devuelve el monto final. El frontend no debe calcular ni enviar el monto final.

**RN-07 — Redondeo:** El monto calculado se redondea a la unidad (sin decimales) usando `Math.Round(..., MidpointRounding.AwayFromZero)`.

---

## 2. DISEÑO

### Modelos de Datos

#### Entidades afectadas

| Entidad | Capa | Cambio | Descripción |
|---------|------|--------|-------------|
| `Policy` | Domain | Modificada | Agregar `HealthPlanSelection` Value Object (nullable) |
| `HealthPlan` | Domain | Nueva (VO) | Catálogo de planes disponibles con su cálculo |
| `HealthPlanSelection` | Domain | Nuevo (VO) | Selección persistida en la póliza: planId, montos, factor |
| `PolicyResponse` | Application | Modificada | Agregar campo `HealthPlan` opcional |
| `CreatePolicyRequest` | API | Modificada | Agregar campo `HealthPlanId` string (reemplaza `InsuredAmount` para Health) |

#### Value Object: `HealthPlan` (catálogo, inmutable)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `string` | Identificador único del plan (ej. `"salud-premium"`) |
| `Name` | `string` | Nombre comercial |
| `BaseAmount` | `decimal` | Monto base fijo definido por el negocio |

Valores iniciales (hardcoded en dominio, no en base de datos):
```csharp
public static class HealthPlanCatalog
{
    public static readonly HealthPlan Basic          = new("basic",           "Básico",           300_000m);
    public static readonly HealthPlan SaludGlobal    = new("salud-global",    "Salud Global",     380_000m);
    public static readonly HealthPlan SaludPremium   = new("salud-premium",   "Salud Premium",    450_000m);
    public static readonly HealthPlan SaludVidaTotal = new("salud-vida-total","Salud Vida Total",  600_000m);

    public static readonly IReadOnlyList<HealthPlan> All = [Basic, SaludGlobal, SaludPremium, SaludVidaTotal];

    public static HealthPlan? FindById(string id) =>
        All.FirstOrDefault(p => p.Id == id);
}
```

#### Value Object: `HealthPlanSelection` (persistido en la póliza)

| Campo | Tipo | Obligatorio | Descripción |
|-------|------|-------------|-------------|
| `PlanId` | `string` | sí | ID del plan seleccionado |
| `PlanName` | `string` | sí | Nombre del plan en el momento de la contratación |
| `BaseAmount` | `decimal` | sí | Valor base del plan |
| `AgeFactorPercentage` | `int` | sí | Factor aplicado: 0, 4 u 8 |
| `AgeFactorAmount` | `decimal` | sí | Monto adicional por edad (puede ser 0) |
| `FinalAmount` | `decimal` | sí | Monto total asegurado = base + factor |

#### Servicio de dominio: `HealthPlanPricingService`

Encapsula la lógica de cálculo. Método principal:

```csharp
// Devuelve la selección calculada o lanza excepción de dominio
public static HealthPlanSelection Calculate(string planId, DateOnly birthDate, DateOnly today);
```

Lógica interna:
```csharp
int age = CalculateAge(birthDate, today);

if (age < 18) throw new DomainException("El asegurado debe tener al menos 18 años.");
if (age >= 74) throw new DomainException("Asegurado de 74+ años requiere formulario de preexistencias.");

int factor = age switch
{
    >= 18 and <= 35 => 0,
    >= 36 and <= 58 => 4,
    _               => 8   // 59-73
};

decimal addition = Math.Round(plan.BaseAmount * factor / 100m, 0, MidpointRounding.AwayFromZero);
decimal final    = plan.BaseAmount + addition;
```

#### Cambios en `Policy.Create()` para tipo Health

```csharp
// Firma nueva (sobrecarga para Health):
public static Policy Create(
    PolicyNumber number,
    InsuredPerson insured,
    CoveragePeriod period,
    string healthPlanId,         // nuevo
    DateOnly today               // para calcular edad
);
```
Internamente llama a `HealthPlanPricingService.Calculate(...)` y asigna `InsuredAmount = selection.FinalAmount`.

La firma original (con `insuredAmount` manual) permanece para tipos no-Health.

---

### API Endpoints

#### POST /api/v1/policies — Cambios para tipo Health

El request body se modifica para pólizas de tipo `Health`:

**Request (Health)**
```json
{
  "type": "Health",
  "insured": {
    "name": "Juan Pérez",
    "documentId": "123456789",
    "birthDate": "1985-06-15"
  },
  "coveragePeriod": {
    "startDate": "2026-04-06",
    "endDate": "2027-04-06"
  },
  "healthPlanId": "salud-premium",
  "monthlyPremium": 45000
}
```

> Nota: `insuredAmount` ya NO se envía para tipo Health. El backend lo calcula internamente.
> Para tipos no-Health, el request no cambia (se envía `insuredAmount` como antes).

**Response 201 (Health)**
```json
{
  "id": "uuid",
  "policyNumber": "POL-2026-00000001",
  "type": "Health",
  "status": "Pending",
  "insured": { "name": "Juan Pérez", "documentId": "123456789" },
  "coveragePeriod": { "startDate": "2026-04-06", "endDate": "2027-04-06" },
  "insuredAmount": 468000,
  "monthlyPremium": 45000,
  "healthPlan": {
    "planId": "salud-premium",
    "planName": "Salud Premium",
    "baseAmount": 450000,
    "ageFactorPercentage": 4,
    "ageFactorAmount": 18000,
    "finalAmount": 468000
  }
}
```

**Response 422 — Edad fuera de rango**
```json
{
  "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "El asegurado debe tener al menos 18 años para contratar un plan de salud."
}
```

**Response 422 — Mayor de 73 años**
```json
{
  "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "Los asegurados de 74 años o más requieren evaluación especial de preexistencias. El flujo estándar no está disponible para este rango etario."
}
```

**Response 400 — Plan de salud inválido**
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Bad Request",
  "status": 400,
  "detail": "El plan de salud 'plan-xyz' no existe. Planes válidos: basic, salud-global, salud-premium, salud-vida-total."
}
```

#### GET /api/v1/health-plans — Nuevo endpoint de catálogo

- **Descripción**: Retorna los 4 planes de salud disponibles (sin cálculo de edad)
- **Auth requerida**: no
- **Response 200**:
```json
[
  { "planId": "basic",           "planName": "Básico",           "baseAmount": 300000 },
  { "planId": "salud-global",    "planName": "Salud Global",     "baseAmount": 380000 },
  { "planId": "salud-premium",   "planName": "Salud Premium",    "baseAmount": 450000 },
  { "planId": "salud-vida-total","planName": "Salud Vida Total", "baseAmount": 600000 }
]
```

#### GET /api/v1/health-plans/calculate — Cálculo previo sin crear póliza

- **Descripción**: Devuelve el monto calculado para un plan y fecha de nacimiento. Útil para el preview del frontend antes de confirmar.
- **Auth requerida**: no
- **Query params**: `planId=salud-premium&birthDate=1980-06-15`
- **Response 200**:
```json
{
  "planId": "salud-premium",
  "planName": "Salud Premium",
  "baseAmount": 450000,
  "ageFactorPercentage": 4,
  "ageFactorAmount": 18000,
  "finalAmount": 468000,
  "insuredAge": 45
}
```
- **Response 422**: Mismos errores de edad descritos arriba.

---

### Diseño Frontend

#### Componentes nuevos / modificados

| Componente | Archivo | Descripción |
|------------|---------|-------------|
| `HealthPlanSelectorComponent` | `features/policies/components/health-plan-selector/` | Grid de 4 tarjetas de plan seleccionable |
| `HealthPlanPreviewComponent` | `features/policies/components/health-plan-preview/` | Panel de resumen con desglose de cálculo |
| `PolicyCreateComponent` (modificado) | `features/policies/pages/policy-create/` | Condicional: muestra selector de plan si `type === 'Health'` |
| `AgeRestrictionComponent` | `features/policies/components/age-restriction/` | Banner de bloqueo para mayores de 73 años |

#### Flujo de interacción — Política Health

```
Step 1: Tipo de póliza → usuario selecciona "Health"
    ↓
Step 2: Datos del asegurado → usuario ingresa fecha de nacimiento
    ↓ (cálculo reactivo automático)
    IF edad >= 74 → mostrar AgeRestrictionComponent → FIN (no se puede avanzar)
    IF edad < 18  → mostrar error de validación
    ELSE          → habilitar Step 3
    ↓
Step 3: Selección de plan → usuario elige uno de los 4 planes
    ↓ (llamada a GET /api/v1/health-plans/calculate)
    → mostrar HealthPlanPreviewComponent con desglose
    ↓
Step 4: Período de cobertura y prima mensual
    ↓
Step 5: Confirmación → POST /api/v1/policies con { type: "Health", healthPlanId: "...", ... }
```

#### Services modificados

| Función | Endpoint |
|---------|---------|
| `getHealthPlans()` | `GET /api/v1/health-plans` |
| `calculateHealthPlan(planId, birthDate)` | `GET /api/v1/health-plans/calculate` |
| `createPolicy(data)` — ya existe, se modifica el payload | `POST /api/v1/policies` |

#### Model changes

```typescript
// Actualizar policy.model.ts:
export interface PolicyResponse {
  // ... campos existentes ...
  healthPlan?: HealthPlanSelection;  // null para tipos no-Health
}

export interface HealthPlan {
  planId: string;
  planName: string;
  baseAmount: number;
}

export interface HealthPlanSelection {
  planId: string;
  planName: string;
  baseAmount: number;
  ageFactorPercentage: number;
  ageFactorAmount: number;
  finalAmount: number;
}
```

---

### Arquitectura y Dependencias

- **No se requieren paquetes nuevos** en backend ni frontend.
- **No se requiere migración de base de datos**: los planes son catálogo hardcoded en dominio. `HealthPlanSelection` (VO) se persistirá como campo adicional en el documento/registro de `Policy` — si se usa MongoDB es un subdocumento embebido; si se usa SQL Server es un owned entity (configurar con `OwnsOne`).
- **Compatibilidad hacia atrás**: El endpoint `POST /api/v1/policies` sigue aceptando el payload antiguo (con `insuredAmount`) para tipos no-Health. La serialización es condicional basada en `type`.
- **Frontend**: El `PolicyCreateComponent` existente se refactoriza con condicional `*ngIf / @if (type === 'Health')`. El stepper de 4 pasos se mantiene.

### Documentación de Cambios en Esquema de Datos

| Elemento | Cambio | Impacto |
|----------|--------|---------|
| `Policy` entity | Nuevo campo `HealthPlanSelection? HealthPlan` (nullable) | Pólizas existentes quedan con `null` — no rompe compatibilidad |
| `CreatePolicyRequest` DTO | Nuevo campo `string? HealthPlanId` | Opcional; requerido solo cuando `Type == Health` |
| `PolicyResponse` DTO | Nuevo campo `HealthPlanSelectionDto? HealthPlan` | Nullable; presente solo en pólizas Health |
| `PolicyType` enum | Sin cambios — `Health` ya existe con valor `2` | — |
| Base de datos (SQL) | `ALTER TABLE Policies ADD HealthPlanId nvarchar(50) NULL, ...` | Migración EF Core requerida |
| Base de datos (Mongo) | Campo `healthPlan` subdocumento opcional en colección `policies` | Sin migración — schemaless |

---

### Notas de Implementación

> 1. **La lógica de cálculo NUNCA debe residir en el frontend.** El frontend puede hacer la llamada a `/health-plans/calculate` para mostrar el preview, pero el backend siempre recalcula al crear la póliza con el `planId` recibido.
> 2. **El catálogo de planes es inmutable en esta versión.** No existe endpoint de administración de planes. Los valores están hardcoded en `HealthPlanCatalog` dentro del dominio.
> 3. **Edad al momento de la creación.** La edad se calcula con la fecha de `DateTime.UtcNow` al momento del request. No se recalcula en renovaciones (HU futura).
> 4. **El formulario de preexistencias (74+ años) está fuera de alcance de SPEC-003.** El bloqueo debe implementarse completamente (backend + frontend) pero el formulario en sí se especificará en SPEC-004.
> 5. **Prima mensual:** La prima mensual sigue siendo ingresada manualmente por el agente. La restricción de que no puede superar el 5% del monto asegurado se aplica sobre el `FinalAmount` calculado.

---

## 3. LISTA DE TAREAS

> Checklist accionable para todos los agentes. Marcar cada ítem (`[x]`) al completarlo.

### Backend

#### Implementación — Dominio

- [ ] Crear `HealthPlan` Value Object (`Id`, `Name`, `BaseAmount`)
- [ ] Crear `HealthPlanCatalog` clase estática con los 4 planes y método `FindById`
- [ ] Crear `HealthPlanSelection` Value Object con todos los campos de la selección persistida
- [ ] Crear `HealthPlanPricingService` estático con método `Calculate(planId, birthDate, today)`
- [ ] Implementar lógica de rangos etarios (0%, 4%, 8%) y bloqueo 74+
- [ ] Agregar excepciones de dominio: `UnderageInsuredException`, `OverageInsuredException`, `InvalidHealthPlanException`
- [ ] Modificar `Policy` aggregate: agregar propiedad `HealthPlanSelection? HealthPlan`
- [ ] Agregar sobrecarga `Policy.Create(...)` para tipo Health que invoca el `HealthPlanPricingService`

#### Implementación — Application

- [ ] Modificar `CreatePolicyCommand` / `CreatePolicyRequest`: agregar `HealthPlanId?` y hacer condicional `InsuredAmount`
- [ ] Modificar `CreatePolicyHandler`: despachar hacia la sobrecarga Health cuando `Type == Health`
- [ ] Modificar `PolicyResponse` DTO: agregar `HealthPlanSelectionDto? HealthPlan`
- [ ] Crear query `GetHealthPlansQuery` + `GetHealthPlansHandler` (retorna catálogo)
- [ ] Crear query `CalculateHealthPlanQuery` + `CalculateHealthPlanHandler` (retorna preview)

#### Implementación — API

- [ ] Modificar `PoliciesController.Create`: adaptación para routing del nuevo campo `HealthPlanId`
- [ ] Crear `HealthPlansController` con:
  - `GET /api/v1/health-plans` → catálogo
  - `GET /api/v1/health-plans/calculate` → cálculo previo con query params
- [ ] Registrar controlador en `Program.cs`
- [ ] Agregar manejo de `UnderageInsuredException` y `OverageInsuredException` en middleware de errores → HTTP 422

#### Implementación — Infrastructure / Persistencia

- [ ] Si SQL Server: crear migración EF Core con campos `HealthPlanId`, `HealthPlanName`, `HealthPlanBaseAmount`, `HealthPlanAgeFactorPercentage`, `HealthPlanAgeFactorAmount`, `HealthPlanFinalAmount` (todos nullable)
- [ ] Si MongoDB: actualizar configuración/índices si aplica
- [ ] Actualizar `PolicyConfiguration` para mapear `OwnsOne<HealthPlanSelection>` (SQL) o subdocumento (Mongo)

---

#### Tests Backend — Dominio (`InsuraTech.Domain.Tests`)

- [ ] `HealthPlanPricingService_Calculate_Age18_ReturnsBaseAmount`
- [ ] `HealthPlanPricingService_Calculate_Age35_ReturnsBaseAmount`
- [ ] `HealthPlanPricingService_Calculate_Age36_Returns4PercentIncrement`
- [ ] `HealthPlanPricingService_Calculate_Age58_Returns4PercentIncrement`
- [ ] `HealthPlanPricingService_Calculate_Age59_Returns8PercentIncrement`
- [ ] `HealthPlanPricingService_Calculate_Age73_Returns8PercentIncrement`
- [ ] `HealthPlanPricingService_Calculate_Age74_ThrowsOverageInsuredException`
- [ ] `HealthPlanPricingService_Calculate_Age17_ThrowsUnderageInsuredException`
- [ ] `HealthPlanPricingService_Calculate_InvalidPlanId_ThrowsInvalidHealthPlanException`
- [ ] `HealthPlanPricingService_Calculate_Rounds_FinalAmount_Correctly`
- [ ] `HealthPlanCatalog_FindById_ReturnsCorrectPlan` (4 casos, 1 por plan)
- [ ] `HealthPlanCatalog_FindById_InvalidId_ReturnsNull`
- [ ] `Policy_Create_WithHealthPlan_SetsInsuredAmountToFinalAmount`
- [ ] `Policy_Create_WithHealthPlan_PersistsHealthPlanSelection`
- [ ] `Policy_Create_NonHealth_WithManualAmount_DoesNotSetHealthPlan`

#### Tests Backend — Application (`InsuraTech.Application.Tests`)

- [ ] `CreatePolicyHandler_Health_ValidPlanAndAge_CreatesPolicy`
- [ ] `CreatePolicyHandler_Health_Age74_Returns422`
- [ ] `CreatePolicyHandler_Health_Age17_Returns422`
- [ ] `CreatePolicyHandler_Health_InvalidPlanId_Returns400`
- [ ] `CreatePolicyHandler_NonHealth_IgnoresHealthPlanId`
- [ ] `GetHealthPlansHandler_ReturnsAll4Plans`
- [ ] `CalculateHealthPlanHandler_ValidInput_ReturnsCorrectAmounts`
- [ ] `CalculateHealthPlanHandler_Age74_ThrowsOverageInsuredException`

---

### Frontend

#### Implementación

- [ ] Crear `HealthPlanService` con métodos `getPlans()` y `calculate(planId, birthDate)`
- [ ] Crear `HealthPlanSelectorComponent`: grid 2×2 de tarjetas seleccionables, emite `planSelected` output
- [ ] Crear `HealthPlanPreviewComponent`: recibe `HealthPlanSelection`, muestra desglose
- [ ] Crear `AgeRestrictionComponent`: banner informativo, muestra mensaje de derivación
- [ ] Modificar `PolicyCreateComponent`:
  - Detectar `type === 'Health'` y ocultar campo manual `insuredAmount`
  - Mostrar `HealthPlanSelectorComponent`
  - Llamar a `calculate()` reactivamente cuando cambia plan o fecha de nacimiento
  - Mostrar `HealthPlanPreviewComponent` con resultado
  - Detectar edad >= 74 y mostrar `AgeRestrictionComponent` bloqueando avance
  - Enviar `healthPlanId` en lugar de `insuredAmount` en el payload
- [ ] Actualizar `policy.model.ts` con `HealthPlan`, `HealthPlanSelection` interfaces
- [ ] Actualizar `PoliciesService.createPolicy()` para soportar el nuevo payload condicional

#### Tests Frontend (`*.spec.ts`)

- [ ] `HealthPlanSelectorComponent - renders all 4 plans`
- [ ] `HealthPlanSelectorComponent - emits selected plan on click`
- [ ] `HealthPlanSelectorComponent - highlights selected plan`
- [ ] `HealthPlanPreviewComponent - displays correct base amount`
- [ ] `HealthPlanPreviewComponent - displays correct factor and addition`
- [ ] `HealthPlanPreviewComponent - displays final amount`
- [ ] `AgeRestrictionComponent - shows block message for 74+ insured`
- [ ] `HealthPlanService - getPlans calls correct endpoint`
- [ ] `HealthPlanService - calculate calls correct endpoint with params`
- [ ] `HealthPlanService - handles 422 error gracefully`
- [ ] `PolicyCreateComponent - shows health plan selector when type is Health`
- [ ] `PolicyCreateComponent - hides insuredAmount input when type is Health`
- [ ] `PolicyCreateComponent - shows age restriction for 74+ insured`
- [ ] `PolicyCreateComponent - sends healthPlanId in payload for Health policies`
- [ ] `PolicyCreateComponent - recalculates on plan change`

---

### QA

- [ ] Ejecutar skill `/gherkin-case-generator` → criterios 1.1 a 5.2
- [ ] Ejecutar skill `/risk-identifier` → clasificar HU-03 (bloqueo 74+) como riesgo Alto
- [ ] Verificar cobertura backend ≥ 80% en `HealthPlanPricingService`, handlers y controller
- [ ] Verificar cobertura frontend ≥ 80% en componentes y service nuevos
- [ ] Prueba de regresión: verificar que pólizas no-Health siguen funcionando con el flujo anterior
- [ ] Validar que el campo `healthPlan` en la respuesta es `null` para tipos no-Health
- [ ] Validar todos los criterios de aceptación CRITERIO-1.1 a CRITERIO-5.2
- [ ] Documentar cambios de esquema en `README.md` (sección "Reglas de Negocio")
- [ ] Actualizar estado spec: `status: IMPLEMENTED`
