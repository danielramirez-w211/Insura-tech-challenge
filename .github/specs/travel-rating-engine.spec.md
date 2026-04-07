---
id: SPEC-004
status: IMPLEMENTED
feature: travel-rating-engine
created: 2026-04-06
updated: 2026-04-06
author: spec-generator
version: "1.0"
related-specs: ["SPEC-001", "SPEC-002", "SPEC-003"]
---

# SPEC-004: Motor de Cálculo Automatizado para Pólizas de Viaje

> **Estado:** `DRAFT` → aprobar con `status: APPROVED` antes de iniciar implementación.  
> **Ciclo de vida:** DRAFT → APPROVED → IN_PROGRESS → IMPLEMENTED → DEPRECATED

---

## 1. CONTEXTO Y ALCANCE

### 1.1 Descripción del Problema (As-Is)

El módulo de creación de pólizas de tipo **Travel** (Viaje) presenta una vulnerabilidad operativa crítica: permite al agente ingresar manualmente el monto asegurado y la prima mensual sin ningún control de validación actuarial. Este comportamiento genera:

- **Inconsistencias tarifarias**: dos pólizas con idénticas condiciones pueden tener costos diferentes.
- **Riesgo financiero**: primas por debajo del valor actuarialmente correcto generan descalce entre ingresos y cobertura.
- **Riesgo regulatorio**: ausencia de un motor de rating documentado y auditable.
- **Exposición cambiaria no controlada**: las pólizas internacionales se basan en USD pero la plataforma opera en COP; la conversión manual es propensa a errores.

### 1.2 Objetivo de Negocio (To-Be)

Implementar un **Rating Engine** determinístico para pólizas de viaje que:

1. Calcule automáticamente el valor de la póliza en función del destino, la duración y la TRM vigente.
2. Bloquee la edición manual del monto asegurado y la prima en el formulario de creación.
3. Integre la API de Datos Abiertos del Gobierno Colombiano (Socrata) para obtener la TRM diaria en tiempo real.
4. Soporte dos modalidades: **Nacional** (tarifas en COP) e **Internacional** (tarifas en USD, convertidas a COP).
5. Clasifique los destinos internacionales por continente: América, Europa, África, Asia y Oceanía.
6. Aplique restricciones de vigencia: máximo **6 meses** por póliza; renovación obligatoria al día siguiente del vencimiento.

### 1.3 Alcance Funcional

| Área             | Dentro de alcance                                         | Fuera de alcance                                       |
|------------------|-----------------------------------------------------------|--------------------------------------------------------|
| Backend          | Domain Service de rating, integración TRM, persistencia   | Motor de renovación automática de pólizas vencidas     |
| Frontend         | Formulario dinámico, vista previa del cálculo en COP      | Portal de renovación de pólizas                        |
| Datos            | TRM diaria por API Socrata                                 | Histórico de TRM / curvas de tipo de cambio            |
| Testing          | Unitarios, integración, casos borde                       | Pruebas de carga o stress sobre la API de TRM          |
| Dominio          | Pólizas de viaje Nacional e Internacional                  | Rider de cobertura médica viaje / seguro de cancelación|

---

## 2. DOCUMENTACIÓN DE CAMBIOS (AS-IS vs TO-BE)

### 2.1 Módulo de Creación de Póliza — Comparativo

| Dimensión               | Estado Actual (As-Is)                                      | Estado Propuesto (To-Be)                                             |
|-------------------------|------------------------------------------------------------|----------------------------------------------------------------------|
| Ingreso del monto       | Campo libre, editable por el agente                        | Calculado automáticamente por el Rating Engine, campo de solo lectura |
| Ingreso de la prima     | Campo libre, editable por el agente                        | Derivado del cálculo; no editable                                    |
| Tipo de destino         | No existe; el agente escribe texto libre                   | Select: Nacional / Internacional                                     |
| Continente              | No existe                                                  | Select condicional, visible solo si destino = Internacional           |
| Duración del viaje      | No existe en el dominio                                    | Número de días (1–180), campo obligatorio                            |
| TRM                     | No existe                                                  | Obtenida automáticamente al momento del cálculo                      |
| Valor en COP            | El agente convierte manualmente                            | Calculado y mostrado en tiempo real en el formulario                  |
| Validación de vigencia  | Ninguna                                                    | Máximo 180 días; error de dominio si se supera                       |
| Auditoría               | No existe                                                  | Snapshot del cálculo persistido en la póliza                         |

### 2.2 Modelo de Datos — Comparativo

**Entidad `Policy` — campos nuevos:**

| Campo                   | Tipo           | Descripción                                               |
|-------------------------|----------------|-----------------------------------------------------------|
| `TravelPlan`            | Value Object   | Snapshot del cálculo: tipo, destino, días, TRM, montos    |

**Value Object `TravelPlanSelection` — nuevo:**

| Propiedad               | Tipo           | Descripción                                               |
|-------------------------|----------------|-----------------------------------------------------------|
| `TripType`              | enum           | `Nacional` / `Internacional`                              |
| `Continent`             | enum?          | Solo Internacional: `America`, `Europe`, `Africa`, `Asia`, `Oceania` |
| `DurationDays`          | int            | Días de cobertura (1–180)                                  |
| `BasePriceUsd`          | decimal?       | Solo Internacional: precio base en USD antes de TRM       |
| `BasePriceCop`          | decimal        | Precio base en COP                                        |
| `DailyIncrementCop`     | decimal        | Incremento diario aplicado en COP                         |
| `TotalPriceCop`         | decimal        | Precio final en COP (campo asegurado)                     |
| `TrmUsed`               | decimal?       | TRM aplicada (solo Internacional)                         |
| `TrmDate`               | DateOnly?      | Fecha de la TRM utilizada                                  |
| `CalculatedAt`          | DateTime       | Timestamp UTC del cálculo                                  |

---

## 3. HISTORIAS DE USUARIO

### HU-001 — Cálculo automático de póliza nacional

| Campo              | Detalle                                                                                   |
|--------------------|-------------------------------------------------------------------------------------------|
| **ID**             | HU-001                                                                                    |
| **Rol**            | Agente de seguros                                                                         |
| **Acción**         | Seleccionar destino Nacional e ingresar la duración en días                               |
| **Beneficio**      | Que el sistema calcule automáticamente el valor de la póliza sin posibilidad de manipulación |
| **Prioridad**      | Alta                                                                                      |
| **SPEC**           | SPEC-004                                                                                  |

**Criterios de Aceptación:**

- **AC-001-1:** Dado que el agente selecciona `Tipo: Nacional` e ingresa `30 días`, cuando confirma el cálculo, entonces el monto asegurado debe ser `$37,000 COP` _(base $2,200 + 29 días × $1,200)_.
- **AC-001-2:** Dado que el agente ingresa `45 días`, cuando el sistema calcula, entonces el monto debe ser igual al de `30 días` ($37,400 COP), ya que el techo de cálculo es 30 días.
- **AC-001-3:** Dado que el agente ingresa `181 días`, cuando intenta crear la póliza, entonces el sistema retorna error `TRAVEL_DURATION_EXCEEDED` con HTTP 422.
- **AC-001-4:** El campo "Monto Asegurado" debe estar deshabilitado (read-only) en el formulario cuando el tipo es `Travel`.
- **AC-001-5:** El sistema debe mostrar el desglose: valor base, días adicionales, incremento y total antes de confirmar.

---

### HU-002 — Cálculo automático de póliza internacional con TRM

| Campo              | Detalle                                                                                   |
|--------------------|-------------------------------------------------------------------------------------------|
| **ID**             | HU-002                                                                                    |
| **Rol**            | Agente de seguros                                                                         |
| **Acción**         | Seleccionar destino Internacional, continente y duración en días                          |
| **Beneficio**      | Que el sistema obtenga la TRM vigente y convierta el valor en USD a COP automáticamente   |
| **Prioridad**      Alta                                                                                      |
| **SPEC**           | SPEC-004                                                                                  |

**Criterios de Aceptación:**

- **AC-002-1:** Dado destino `Internacional`, continente `Europa` y `10 días`, cuando el sistema obtiene TRM $4,200, entonces el monto debe ser `$(30 + 9×5) × 4,200 = $315,000 COP`.
- **AC-002-2:** El selector de `Continente` solo es visible cuando `TripType = Internacional`.
- **AC-002-3:** El sistema debe mostrar la TRM utilizada y su fecha junto al resumen del cálculo.
- **AC-002-4:** Si la API de TRM no está disponible, el sistema retorna error `TRM_UNAVAILABLE` con HTTP 503 y no permite crear la póliza.
- **AC-002-5:** No hay límite de cálculo por días para Internacional (cada día adicional siempre suma $5 USD).
- **AC-002-6:** Máximo 180 días; si se supera, retorna `TRAVEL_DURATION_EXCEEDED` (igual que Nacional).

---

### HU-003 — Restricción de vigencia máxima (6 meses)

| Campo              | Detalle                                                                                   |
|--------------------|-------------------------------------------------------------------------------------------|
| **ID**             | HU-003                                                                                    |
| **Rol**            | Agente de seguros                                                                         |
| **Acción**         | Intentar crear una póliza de viaje con vigencia mayor a 6 meses                           |
| **Beneficio**      | Que el sistema bloquee la operación y le informe que debe contratar una nueva póliza al día siguiente del vencimiento |
| **Prioridad**      | Alta                                                                                      |
| **SPEC**           | SPEC-004                                                                                  |

**Criterios de Aceptación:**

- **AC-003-1:** Si `DurationDays > 180`, el backend retorna HTTP 422 con `code: TRAVEL_DURATION_EXCEEDED` y mensaje en español.
- **AC-003-2:** El frontend valida en tiempo real y deshabilita el botón "Calcular" si los días superan 180.
- **AC-003-3:** El mensaje de error indica explícitamente que para continuidad de cobertura se debe contratar una nueva póliza a partir del día 181.

---

### HU-004 — Visualización del resumen del cálculo antes de confirmar

| Campo              | Detalle                                                                                   |
|--------------------|-------------------------------------------------------------------------------------------|
| **ID**             | HU-004                                                                                    |
| **Rol**            | Agente de seguros                                                                         |
| **Acción**         | Ver el desglose completo del cálculo antes de confirmar la creación de la póliza          |
| **Beneficio**      | Tener certeza del valor calculado y poder explicarlo al cliente                           |
| **Prioridad**      | Media                                                                                     |
| **SPEC**           | SPEC-004                                                                                  |

**Criterios de Aceptación:**

- **AC-004-1:** El componente de preview muestra: Tipo de viaje, Continente (si aplica), Días, Valor base (USD / COP), Incremento diario, Total en COP.
- **AC-004-2:** Para Internacional muestra adicionalmente: TRM utilizada, fecha de la TRM y valor en USD antes de conversión.
- **AC-004-3:** El preview se actualiza de forma reactiva cada vez que cambia el tipo, continente o duración.
- **AC-004-4:** El preview tiene `data-testid="travel-plan-preview"` y el monto final `data-testid="final-amount-cop"`.

---

### HU-005 — Integración con API TRM (Socrata — datos.gov.co)

| Campo              | Detalle                                                                                   |
|--------------------|-------------------------------------------------------------------------------------------|
| **ID**             | HU-005                                                                                    |
| **Rol**            | Sistema (proceso interno)                                                                  |
| **Acción**         | Consultar la TRM vigente desde la API de Datos Abiertos del Gobierno Colombiano           |
| **Beneficio**      | Asegurar que la conversión USD→COP sea siempre la oficial y actualizada                   |
| **Prioridad**      | Alta                                                                                      |
| **SPEC**           | SPEC-004                                                                                  |

**Criterios de Aceptación:**

- **AC-005-1:** El servicio consulta `https://www.datos.gov.co/resource/mcec-87by.json` con `$limit=1&$order=vigenciadesde DESC`.
- **AC-005-2:** Si la respuesta es exitosa, extrae `valor` (tasa COP por 1 USD) y `vigenciadesde` (fecha).
- **AC-005-3:** Si la API retorna error HTTP o timeout (>5s), lanza `TrmUnavailableException` con `code: TRM_UNAVAILABLE`.
- **AC-005-4:** La TRM se puede cachear en memoria durante máximo 1 hora para evitar llamadas repetitivas.
- **AC-005-5:** La TRM usada queda persistida en el `TravelPlanSelection` de la póliza para auditoría.

---

## 4. LÓGICA DE CÁLCULO FUNCIONAL

### 4.1 Lógica Nacional (COP)

**Parámetros:**

| Parámetro           | Valor         |
|---------------------|---------------|
| Precio base (día 1) | $2,200 COP    |
| Incremento diario   | $1,200 COP    |
| Techo de cálculo    | 30 días       |
| Duración máxima     | 180 días      |

**Fórmula:**

Sea $d$ = número de días de cobertura.

$$
d_{efectivo} = \min(d,\ 30)
$$

$$
\text{TotalCOP} = 2200 + (d_{efectivo} - 1) \times 1200
$$

**Tabla de referencia:**

| Días ($d$) | $d_{efectivo}$ | Total COP     |
|------------|----------------|---------------|
| 1          | 1              | $2,200        |
| 2          | 2              | $3,400        |
| 10         | 10             | $13,000       |
| 15         | 15             | $19,000       |
| 30         | 30             | $37,000       |
| 45         | 30             | $37,000 ← techo |
| 180        | 30             | $37,000 ← techo |
| 181        | —              | ERROR: `TRAVEL_DURATION_EXCEEDED` |

---

### 4.2 Lógica Internacional (USD → COP)

**Parámetros:**

| Parámetro           | Valor         |
|---------------------|---------------|
| Precio base (día 1) | 30 USD        |
| Incremento diario   | 5 USD         |
| Techo de cálculo    | Sin límite    |
| Duración máxima     | 180 días      |

**Continentes soportados:**

| Continente | Código enum  |
|------------|--------------|
| América    | `America`    |
| Europa     | `Europe`     |
| África     | `Africa`     |
| Asia       | `Asia`       |
| Oceanía    | `Oceania`    |

> En esta versión el continente es informativo (auditoría y futura diferenciación de tarifas). El cálculo base aplica igual para todos los continentes.

**Fórmula:**

Sea $d$ = días de cobertura, $T$ = TRM vigente (COP por 1 USD).

$$
\text{TotalUSD} = 30 + (d - 1) \times 5
$$

$$
\text{TotalCOP} = \text{TotalUSD} \times T
$$

**Tabla de referencia** (TRM ejemplo = $4,200 COP/USD):

| Días ($d$) | Total USD | Total COP (TRM $4,200) |
|------------|-----------|------------------------|
| 1          | $30       | $126,000               |
| 5          | $50       | $210,000               |
| 10         | $75       | $315,000               |
| 30         | $175      | $735,000               |
| 60         | $325      | $1,365,000             |
| 90         | $475      | $1,995,000             |
| 180        | $925      | $3,885,000             |
| 181        | —         | ERROR: `TRAVEL_DURATION_EXCEEDED` |

---

### 4.3 Restricciones de Dominio

| Regla                    | Condición                        | Excepción de Dominio             | HTTP |
|--------------------------|----------------------------------|----------------------------------|------|
| Duración mínima          | `days < 1`                       | `TravelDurationInvalidException` | 422  |
| Duración máxima          | `days > 180`                     | `TravelDurationExceededException`| 422  |
| TRM no disponible        | API Socrata no responde          | `TrmUnavailableException`        | 503  |
| Continente inválido      | Valor no en enum para Internacional | `InvalidContinentException`   | 400  |
| Tipo de viaje inválido   | Valor no en enum `TripType`      | `InvalidTripTypeException`       | 400  |

---

## 5. INTEGRACIÓN TRM — PROTOCOLO TÉCNICO

### 5.1 API Objetivo

**Proveedor:** Datos Abiertos Colombia — [datos.gov.co](https://www.datos.gov.co)  
**Dataset:** Tasa de Cambio Representativa del Mercado (TRM)  
**Protocolo:** Socrata Open Data API (SODA)

### 5.2 Endpoint de Consulta

```
GET https://www.datos.gov.co/resource/mcec-87by.json
    ?$limit=1
    &$order=vigenciadesde DESC
```

**Respuesta esperada:**

```json
[
  {
    "valor": "4215.24",
    "unidad": "COP",
    "vigenciadesde": "2026-04-06T00:00:00.000",
    "vigenciahasta": "2026-04-06T00:00:00.000"
  }
]
```

**Campos utilizados:**

| Campo           | Tipo     | Uso                              |
|-----------------|----------|----------------------------------|
| `valor`         | string   | TRM (parsear a `decimal`)        |
| `vigenciadesde` | string   | Fecha de vigencia (parsear a `DateOnly`) |

### 5.3 Contrato del Servicio (Interfaz)

```csharp
public interface ITrmService
{
    /// <summary>
    /// Retorna la TRM vigente del día. Lanza TrmUnavailableException si la API falla.
    /// </summary>
    Task<TrmResult> GetCurrentTrmAsync(CancellationToken cancellationToken = default);
}

public record TrmResult(decimal ValueCop, DateOnly Date);
```

### 5.4 Estrategia de Caché

- Caché en memoria (`IMemoryCache`) con TTL de **1 hora**.
- Clave de caché: `"trm:current"`.
- Si el caché tiene valor vigente, no se llama a la API.
- Si expira o no existe, se consulta la API y se actualiza el caché.
- Si la API falla y no hay valor en caché, se lanza `TrmUnavailableException`.

### 5.5 Manejo de Errores TRM

```csharp
// TrmUnavailableException
public sealed class TrmUnavailableException : DomainException
{
    public TrmUnavailableException()
        : base("TRM_UNAVAILABLE", "La TRM no está disponible en este momento. Intente de nuevo más tarde.") { }
}
```

Middleware debe mapear `TrmUnavailableException` → HTTP 503 Service Unavailable.

---

## 6. DISEÑO DE INTERFAZ FRONTEND

### 6.1 Flujo del Formulario

```
[Tipo de Póliza: Travel]
         │
         ▼
[Tipo de Viaje] ── Nacional ──► [Días de cobertura (1-180)]
                                         │
                                         ▼
                               [Preview: Cálculo Nacional]
                               ┌─────────────────────────┐
                               │ Valor base:   $2,200 COP │
                               │ Días adic.:   29 × $1,200│
                               │ Total:       $37,400 COP │
                               └─────────────────────────┘

         └──Internacional──► [Continente] + [Días (1-180)]
                                         │
                                         ▼
                               [Preview: Cálculo Internacional]
                               ┌─────────────────────────────────┐
                               │ Valor base:       30 USD         │
                               │ Días adic.:        9 × 5 USD     │
                               │ Total USD:        75 USD         │
                               │ TRM (06/04/2026): $4,215 COP/USD │
                               │ Total COP:   $316,125 COP        │
                               └─────────────────────────────────┘
```

### 6.2 Señales (Angular Signals) del Componente

```typescript
// Estado reactivo del componente
tripType     = signal<'Nacional' | 'Internacional' | null>(null);
continent    = signal<string | null>(null);
durationDays = signal<number | null>(null);
calculation  = signal<TravelPlanCalculationDto | null>(null);
trmError     = signal<boolean>(false);
durationError= signal<string | null>(null);

// Computed
showContinentSelector = computed(() => tripType() === 'Internacional');
canCalculate = computed(() =>
  tripType() !== null &&
  durationDays() !== null &&
  (durationDays()! >= 1 && durationDays()! <= 180) &&
  (tripType() !== 'Internacional' || continent() !== null)
);
```

### 6.3 Componentes Nuevos

| Componente                     | Responsabilidad                                           |
|--------------------------------|-----------------------------------------------------------|
| `travel-plan-form`             | Formulario principal: selección de tipo, continente, días |
| `travel-plan-preview`          | Desglose del cálculo en tiempo real                       |
| `travel-duration-restriction`  | Banner de error si días > 180                             |

### 6.4 Data-testids requeridos

| Elemento                  | `data-testid`                  |
|---------------------------|--------------------------------|
| Selector tipo de viaje    | `trip-type-select`             |
| Selector continente       | `continent-select`             |
| Input días de cobertura   | `duration-days-input`          |
| Preview del cálculo       | `travel-plan-preview`          |
| Monto final en COP        | `final-amount-cop`             |
| TRM utilizada             | `trm-value`                    |
| Banner duración excedida  | `duration-exceeded-banner`     |

---

## 7. PLAN DE PRUEBAS

### 7.1 Cobertura Mínima Requerida

| Capa               | Objetivo  | Tipo                          |
|--------------------|-----------|-------------------------------|
| Domain Service     | ≥ 90%     | Unit                          |
| TRM Service        | ≥ 85%     | Unit + Integration            |
| Application Layer  | ≥ 80%     | Unit (mocks)                  |
| API Controller     | ≥ 75%     | Integration                   |
| Frontend           | ≥ 80%     | Karma + Jasmine               |

---

### 7.2 Casos de Prueba — Domain Service (`TravelRatingService`)

| ID    | Escenario                                             | Input                                 | Expected Output            | Tipo   |
|-------|-------------------------------------------------------|---------------------------------------|----------------------------|--------|
| UT-01 | Cálculo Nacional día 1                                | Nacional, 1 día                       | $2,200 COP                 | Unit   |
| UT-02 | Cálculo Nacional 10 días                              | Nacional, 10 días                     | $13,000 COP                | Unit   |
| UT-03 | Cálculo Nacional 30 días (techo exacto)               | Nacional, 30 días                     | $37,400 COP                | Unit   |
| UT-04 | Cálculo Nacional 45 días (sobre techo)                | Nacional, 45 días                     | $37,400 COP (igual a 30)   | Unit   |
| UT-05 | Cálculo Nacional 180 días (máximo)                    | Nacional, 180 días                    | $37,400 COP                | Unit   |
| UT-06 | Cálculo Nacional 181 días → excepción                 | Nacional, 181 días                    | `TravelDurationExceededException` | Unit |
| UT-07 | Cálculo Nacional 0 días → excepción                   | Nacional, 0 días                      | `TravelDurationInvalidException`  | Unit |
| UT-08 | Cálculo Internacional día 1, TRM fija                 | Internacional, Europa, 1 día, TRM $4,000 | $120,000 COP           | Unit   |
| UT-09 | Cálculo Internacional 10 días, TRM fija               | Internacional, Asia, 10 días, TRM $4,200 | $315,000 COP           | Unit   |
| UT-10 | Cálculo Internacional 30 días, TRM fija               | Internacional, América, 30 días, TRM $4,100 | $717,500 COP        | Unit   |
| UT-11 | Cálculo Internacional 180 días (máximo), TRM fija     | Internacional, Oceanía, 180 días, TRM $4,200 | $3,885,000 COP     | Unit   |
| UT-12 | Cálculo Internacional 181 días → excepción            | Internacional, Europa, 181 días       | `TravelDurationExceededException` | Unit |
| UT-13 | Continente inválido → excepción                       | Internacional, "Marte", 5 días        | `InvalidContinentException`       | Unit |
| UT-14 | Todos los continentes soportados (Theory)             | [America, Europe, Africa, Asia, Oceania] | No exception              | Unit   |
| UT-15 | Snapshot completo: todos los campos del VO            | Internacional, Europa, 5 días, TRM $4,200 | PlanId, Continent, TotalUSD, TotalCOP, TrmUsed, TrmDate, CalculatedAt | Unit |
| UT-16 | Incremento diario Nacional es exactamente $1,200      | Nacional, 2 días vs 3 días            | Diferencia = $1,200 COP    | Unit   |
| UT-17 | Incremento diario Internacional es exactamente $5 USD | Internacional, 2 días vs 3 días       | Diferencia = $5 USD        | Unit   |
| UT-18 | Precio base Internacional siempre es $30 USD          | Internacional, 1 día                  | BaseUSD = 30               | Unit   |

---

### 7.3 Casos de Prueba — TRM Service

| ID    | Escenario                                              | Condición                          | Expected                              | Tipo        |
|-------|--------------------------------------------------------|------------------------------------|---------------------------------------|-------------|
| IT-01 | Obtener TRM exitosamente desde API                    | API responde 200 con `valor: 4215` | `TrmResult(4215m, DateOnly.Parse(...))` | Integration |
| IT-02 | API devuelve array vacío                              | `[]`                               | `TrmUnavailableException`             | Integration |
| IT-03 | API devuelve HTTP 500                                 | Status 500                         | `TrmUnavailableException`             | Integration |
| IT-04 | API timeout (> 5 segundos)                            | Timeout simulado                   | `TrmUnavailableException`             | Integration |
| IT-05 | TRM obtenida del caché (no llama a la API segunda vez) | Segunda llamada dentro de 1h      | Retorna valor cacheado, 0 HTTP calls  | Unit        |
| IT-06 | Caché expirado → se consulta la API nuevamente         | TTL expirado                       | Nueva llamada a la API                | Unit        |
| IT-07 | Campo `valor` con decimales ("4215.24")               | Parseo correcto                    | `4215.24m`                            | Unit        |

---

### 7.4 Casos de Prueba — Application Layer (Handler)

| ID    | Escenario                                             | Expected                                              | Tipo  |
|-------|-------------------------------------------------------|-------------------------------------------------------|-------|
| AT-01 | Crear póliza Travel Nacional con datos válidos        | Policy creada con `TravelPlan.TotalPriceCop` correcto | Unit  |
| AT-02 | Crear póliza Travel Internacional con TRM mockeada   | Policy creada con campos USD y COP correctos          | Unit  |
| AT-03 | TRM no disponible → Handler lanza excepción           | `TrmUnavailableException` propagada                   | Unit  |
| AT-04 | Días inválidos → Handler lanza excepción de dominio  | `TravelDurationExceededException` propagada           | Unit  |
| AT-05 | Póliza no-Travel usa flujo original (sin TravelPlan) | `Policy.TravelPlan` es null                           | Unit  |

---

### 7.5 Casos de Prueba — Frontend (Karma + Jasmine)

| ID    | Componente              | Escenario                                                | Expected                                         |
|-------|-------------------------|----------------------------------------------------------|--------------------------------------------------|
| FT-01 | `travel-plan-form`      | Seleccionar Nacional oculta selector de continente       | `[data-testid=continent-select]` no visible      |
| FT-02 | `travel-plan-form`      | Seleccionar Internacional muestra selector de continente | `[data-testid=continent-select]` visible         |
| FT-03 | `travel-plan-form`      | Ingresar 181 días muestra banner de error                | `[data-testid=duration-exceeded-banner]` visible |
| FT-04 | `travel-plan-form`      | Botón calcular deshabilitado si días > 180               | `button[disabled]` presente                      |
| FT-05 | `travel-plan-preview`   | Sin cálculo muestra estado vacío                         | No renderiza montos                              |
| FT-06 | `travel-plan-preview`   | Cálculo Nacional muestra desglose COP                    | `final-amount-cop` contiene valor formateado     |
| FT-07 | `travel-plan-preview`   | Cálculo Internacional muestra TRM y valor USD            | `trm-value` visible con valor numérico           |
| FT-08 | `travel-plan-preview`   | Todos los campos del cálculo son visibles                | BasePrice, Increment, Total presentes            |
| FT-09 | `health-plans.service`  | `calculate()` llama al endpoint correcto                 | GET `/api/v1/travel-plans/calculate` invocado    |
| FT-10 | `travel-duration-restriction` | Banner visible cuando `showRestriction = true`   | Mensaje de renovación visible                    |

---

### 7.6 Casos Borde Críticos

| ID    | Escenario                                              | Detalle                                                                 |
|-------|--------------------------------------------------------|-------------------------------------------------------------------------|
| CB-01 | Año bisiesto en fecha de inicio (29-feb-XXXX)          | `CoveragePeriod` debe calcular 180 días correctamente incluyendo el 29/feb |
| CB-02 | TRM con valor igual a cero                             | Lanzar `TrmUnavailableException` (TRM de 0 es inválida)                 |
| CB-03 | TRM con valor negativo en respuesta de la API          | Lanzar `TrmUnavailableException`                                        |
| CB-04 | Duración = 1 (mínimo absoluto)                         | Nacional: $2,200 COP; Internacional: 30 USD × TRM                      |
| CB-05 | Duración = 30 (techo Nacional exacto)                  | Verificar que no hay off-by-one en el incremento: (30-1) × 1,200       |
| CB-06 | API Socrata devuelve `valor` como string con coma      | Normalizar separadores antes de parsear: "4.215,24" → 4215.24          |
| CB-07 | Póliza Internacional con fecha inicio = fecha fin TRM  | La fecha de vigencia de la TRM debe guardarse correctamente             |

---

## 8. ARTEFACTOS A CREAR

### 8.1 Backend — Nuevos Archivos

```
InsuraTech.Domain/
├── Exceptions/
│   ├── TravelDurationInvalidException.cs
│   ├── TravelDurationExceededException.cs
│   ├── TrmUnavailableException.cs
│   └── InvalidContinentException.cs
├── Policies/
│   └── TravelPlan/
│       ├── TripType.cs                  ← enum
│       ├── Continent.cs                 ← enum
│       ├── TravelPlanSelection.cs       ← Value Object (snapshot del cálculo)
│       └── TravelRatingService.cs       ← Domain Service

InsuraTech.Application/
├── TravelPlans/
│   └── Queries/
│       └── CalculateTravelPlan/
│           ├── CalculateTravelPlanQuery.cs
│           └── CalculateTravelPlanHandler.cs
├── Common/
│   └── Interfaces/
│       └── ITrmService.cs              ← interfaz (ICurrentUserService pattern)

InsuraTech.Infrastructure/
└── ExternalServices/
    └── TrmService.cs                   ← implementación + caché

InsuraTech.API/
└── Controllers/
    └── TravelPlansController.cs        ← GET /api/v1/travel-plans/calculate
```

### 8.2 Backend — Archivos Modificados

| Archivo                              | Cambio                                                    |
|--------------------------------------|-----------------------------------------------------------|
| `Policy.cs`                          | Propiedad `TravelPlan?` + factory `CreateTravelPolicy()`  |
| `CreatePolicyCommand.cs`             | Agregar `TripType?`, `Continent?`, `DurationDays?`        |
| `CreatePolicyHandler.cs`             | Rama condicional para Travel (llama `CreateTravelPolicy`) |
| `PolicyResponse.cs`                  | Agregar `TravelPlanSelectionDto?`                         |
| `PolicyMappingExtensions.cs`         | Mapear `policy.TravelPlan` → DTO                          |
| `CreatePolicyRequest.cs`             | Agregar campos Travel                                     |
| `PoliciesController.cs`              | Pasar campos Travel al comando                            |
| `ExceptionHandlingMiddleware.cs`     | Casos: `TravelDurationExceededException` (422), `TrmUnavailableException` (503) |
| `MongoDbContext.cs`                  | `BsonClassMap` para `TravelPlanSelection`                 |

### 8.3 Frontend — Nuevos Archivos

```
features/policies/
├── services/
│   └── travel-plans.service.ts
├── components/
│   ├── travel-plan-form/
│   │   └── travel-plan-form.component.ts
│   ├── travel-plan-preview/
│   │   └── travel-plan-preview.component.ts
│   └── travel-duration-restriction/
│       └── travel-duration-restriction.component.ts
└── models/
    └── (extensión de policy.model.ts con TravelPlanDto, etc.)
```

### 8.4 Documentación

| Artefacto                                       | Ubicación                                          |
|-------------------------------------------------|----------------------------------------------------|
| SPEC-004 (este documento)                       | `.github/specs/travel-rating-engine.spec.md`       |
| README de implementación                        | `.github/specs/travel-rating-engine/README.md`     |

---

## 9. DEFINICIÓN DE TERMINADO (DoD)

- [ ] Todos los casos de prueba del Plan de Pruebas (Sección 7) están implementados y pasan en verde.
- [ ] Cobertura de tests ≥ 80% en backend y frontend.
- [ ] `dotnet build` sin errores ni warnings de compilación.
- [ ] `ng build` sin errores.
- [ ] `POST /api/v1/policies` con tipo Travel crea la póliza con `travelPlan` en la respuesta.
- [ ] `GET /api/v1/travel-plans/calculate` retorna el desglose correcto.
- [ ] La TRM queda persistida en el documento MongoDB.
- [ ] `ExceptionHandlingMiddleware` retorna HTTP 503 cuando la TRM no está disponible.
- [ ] El formulario de creación de pólizas bloquea los campos de monto cuando el tipo es Travel.
- [ ] README de implementación creado en `.github/specs/travel-rating-engine/README.md`.
- [ ] SPEC actualizada a estado `IMPLEMENTED`.

---

*Documento generado el 2026-04-06 — InsuraTech Platform v1.4*
