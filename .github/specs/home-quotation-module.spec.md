---
id: SPEC-012
status: IN_PROGRESS
feature: home-quotation-module
created: 2026-04-14
updated: 2026-04-14
author: spec-generator
version: "1.0"
related-specs: [SPEC-010, SPEC-011]
---

# Spec: Módulo de Cotización de Seguro de Hogar

> **Estado:** `DRAFT` → aprobar con `status: APPROVED` antes de iniciar implementación.
> **Ciclo de vida:** DRAFT → APPROVED → IN_PROGRESS → IMPLEMENTED → DEPRECATED

---

## Resumen Ejecutivo

El módulo de Cotización de Seguro de Hogar introduce un motor de cálculo de prima basado en el **valor comercial del predio** (1% del valor base), un sistema de **coberturas individuales seleccionables** y tres **paquetes predefinidos** (Básico, Estándar y Premium), más un conjunto de **multiplicadores de riesgo** actuariales (antigüedad del inmueble, estrato socioeconómico, número de habitantes y descuento por múltiples coberturas).

A diferencia del módulo de Vehículo — donde la prima depende de una tasa técnica fija por antigüedad — el módulo de Hogar combina un porcentaje base sobre el valor del predio con cargos fijos y variables por cada cobertura seleccionada. El tipo de póliza `Home = 4` ya existe en el enum `PolicyType` del dominio y en el frontend; el formulario de creación de póliza ya muestra "Home" como opción sin lógica de cotización. Esta spec completa ese flujo.

**Comparativa de módulos:**

| Aspecto | Módulo Vehículo (SPEC-010) | Módulo Hogar (SPEC-012) |
|---------|----------------------------|--------------------------|
| Prima base | `valorComercial × tasaTécnica` | `valorPredio × 0.01` (1% fijo) |
| Factor de ajuste | Antigüedad del vehículo | Antigüedad, estrato, habitantes |
| Coberturas | Predefinidas por plan | Seleccionables individualmente + paquetes |
| Prima mínima | $800.000 COP | No aplica |
| Redondeo | Al mil superior (CEILING) | Al mil superior (CEILING) |
| Descuento especial | 5% pago anual | 5% por seleccionar más de 5 coberturas |
| Planes disponibles | 3 | 3 paquetes + selección individual |
| Período de cobertura | 365 días | 365 días |
| `PolicyType` | `Vehicle = 3` | `Home = 4` (ya existe) |

---

## 1. REQUERIMIENTOS

### 1.1 Descripción del Feature

Crear el **Módulo de Cotización de Seguro de Hogar** que captura los datos del predio (valor comercial, año de construcción, estrato, número de habitantes, tipo de inmueble y coberturas seleccionadas), aplica el motor de cálculo de prima base más coberturas variables, aplica multiplicadores de riesgo y persiste la póliza en MongoDB a través del flujo existente `CreatePolicyCommand`. El agente puede elegir entre tres paquetes predefinidos o armar una cobertura personalizada seleccionando coberturas individuales.

### 1.2 Historias de Usuario

---

#### HU-1: Captura de Datos del Predio

```
Como:        Agente de seguros
Quiero:      Ingresar los datos del inmueble (valor comercial, año de construcción,
             estrato socioeconómico, número de habitantes y tipo de inmueble)
             y que el sistema valide cada campo antes de habilitar la cotización
Para:        Garantizar que el motor de cálculo reciba los insumos correctos y
             completos antes de mostrar las opciones de cobertura disponibles

Prioridad:   Alta
Estimación:  M
Dependencias: Flujo PolicyCreateComponent existente (MatStepper)
Capa:        Ambas
```

#### Criterios de Aceptación — HU-1

**Happy Path**
```gherkin
CRITERIO-1.1: Captura exitosa de datos del predio con todos los campos válidos
  Dado que:  el agente está en el paso "Datos del Inmueble" del formulario de creación de póliza
             y seleccionó tipo de póliza "Home"
  Cuando:    ingresa un valor comercial de $300.000.000 COP
             y un año de construcción de 2018 (antigüedad 8 años — sin recargos por antigüedad)
             y estrato socioeconómico 3
             y 3 habitantes
             y tipo de inmueble "Apartamento"
  Entonces:  el sistema habilita el paso de selección de coberturas
             y la prima base queda calculada como $3.000.000 COP (300.000.000 × 0.01)
```

**Error Path**
```gherkin
CRITERIO-1.2: Valor comercial del predio inválido rechazado
  Dado que:  el agente está en el paso "Datos del Inmueble"
  Cuando:    ingresa un valor comercial de $0 o un valor negativo
  Entonces:  el formulario muestra el error:
             "El valor comercial del predio debe ser mayor a $0."
             y el botón "Cotizar" permanece deshabilitado
```

**Error Path**
```gherkin
CRITERIO-1.3: Año de construcción fuera de rango rechazado
  Dado que:  el agente está en el paso "Datos del Inmueble"
  Cuando:    ingresa un año de construcción anterior a 1900 o superior al año actual
  Entonces:  el formulario muestra el error:
             "El año de construcción debe estar entre 1900 y el año actual."
             y el botón "Cotizar" permanece deshabilitado
```

**Error Path**
```gherkin
CRITERIO-1.4: Estrato socioeconómico fuera de rango rechazado
  Dado que:  el agente está en el paso "Datos del Inmueble"
  Cuando:    ingresa un valor de estrato menor a 1 o mayor a 6
  Entonces:  el formulario muestra el error:
             "El estrato debe ser un valor entre 1 y 6."
             y el botón "Cotizar" permanece deshabilitado
```

**Edge Case**
```gherkin
CRITERIO-1.5: Año de construcción igual al año actual
  Dado que:  el agente ingresa el año de construcción igual al año actual
  Cuando:    el sistema evalúa la antigüedad
  Entonces:  la antigüedad calculada es 0 años
             y se aplica el descuento del 5% por inmueble menor a 10 años
             sin ningún recargo adicional por antigüedad
```

---

#### HU-2: Selección de Paquete o Coberturas Individuales

```
Como:        Agente de seguros
Quiero:      Elegir entre tres paquetes predefinidos (Básico, Estándar, Premium)
             o armar una cobertura personalizada seleccionando coberturas individuales
             de un catálogo de 11 opciones disponibles
Para:        Adaptar la póliza exactamente a las necesidades del asegurado
             sin restringirlo a planes rígidos, y al mismo tiempo ofrecer
             opciones simplificadas para agilizar el proceso

Prioridad:   Alta
Estimación:  L
Dependencias: HU-1 (datos del predio capturados)
Capa:        Ambas
```

#### Criterios de Aceptación — HU-2

**Happy Path**
```gherkin
CRITERIO-2.1: Selección de paquete Básico precarga coberturas correctas
  Dado que:  el agente está en el paso "Coberturas" con la cotización base calculada
  Cuando:    selecciona el paquete "Básico"
  Entonces:  el sistema marca automáticamente las coberturas:
             - Incendio y Explosión (incluida en base, sin costo adicional)
             - Fontanería/Daños por Agua (+5% del valor base)
             y calcula el costo total del paquete
             y el agente puede continuar sin seleccionar coberturas adicionales
```

**Happy Path**
```gherkin
CRITERIO-2.2: Selección de paquete Premium activa todas las coberturas
  Dado que:  el agente está en el paso "Coberturas"
  Cuando:    selecciona el paquete "Premium"
  Entonces:  el sistema marca automáticamente las 9 coberturas del paquete Premium:
             Incendio, Robo, Fontanería, Daños Eléctricos, Responsabilidad Civil,
             Rotura de Cristales, Inhabitabilidad, Defensa Jurídica, Asistencia en el Hogar
             y aplica automáticamente el descuento multicobertura del 5% (más de 5 coberturas)
```

**Happy Path**
```gherkin
CRITERIO-2.3: Selección individual de más de 5 coberturas activa descuento multicobertura
  Dado que:  el agente está en el paso "Coberturas" con selección personalizada
  Cuando:    selecciona 6 o más coberturas individuales
  Entonces:  el sistema aplica automáticamente el descuento multicobertura del 5%
             sobre el total calculado antes del descuento
             y muestra una etiqueta "Descuento multicobertura aplicado (-5%)"
```

**Error Path**
```gherkin
CRITERIO-2.4: No se puede continuar sin seleccionar al menos una cobertura
  Dado que:  el agente está en el paso "Coberturas"
             y no ha seleccionado ningún paquete ni cobertura individual
  Cuando:    intenta hacer clic en "Cotizar"
  Entonces:  el sistema muestra el mensaje de validación:
             "Debe seleccionar al menos una cobertura para continuar."
             y el botón "Cotizar" permanece deshabilitado
```

**Edge Case**
```gherkin
CRITERIO-2.5: Cambio de paquete limpia selección anterior y aplica el nuevo
  Dado que:  el agente seleccionó el paquete "Estándar" (4 coberturas marcadas)
  Cuando:    cambia al paquete "Básico"
  Entonces:  el sistema limpia las coberturas del paquete Estándar
             y marca únicamente las coberturas del paquete Básico
             sin retener selecciones del paquete anterior
```

---

#### HU-3: Motor de Cálculo de Prima de Hogar

```
Como:        Sistema de cotización
Quiero:      Calcular la prima total aplicando la prima base (1% del valor del predio),
             los cargos por coberturas adicionales (variables y fijos), y los
             multiplicadores de riesgo (antigüedad, estrato, habitantes)
Para:        Garantizar que la prima final sea técnicamente correcta, refleje el
             perfil de riesgo real del inmueble y sea transparente para el asegurado

Prioridad:   Alta
Estimación:  L
Dependencias: HU-1, HU-2
Capa:        Backend
```

#### Criterios de Aceptación — HU-3

**Happy Path**
```gherkin
CRITERIO-3.1: Cálculo de prima base con paquete Básico sin multiplicadores de riesgo
  Dado que:  el valor del predio es $200.000.000 COP
             y el año de construcción es 2020 (antigüedad 6 años — sin recargos)
             y el estrato es 3 (sin recargos ni descuentos)
             y hay 2 habitantes (sin cargo por uso intensivo)
             y el agente seleccionó el paquete Básico
  Cuando:    el motor de cálculo ejecuta la fórmula
  Entonces:  primaBase = 200.000.000 × 0.01 = 2.000.000
             cargoFontaneria = 2.000.000 × 0.05 = 100.000
             totalBruto = 2.000.000 + 100.000 = 2.100.000
             totalFinal = CEILING(2.100.000 / 1000) × 1000 = 2.100.000
```

**Happy Path**
```gherkin
CRITERIO-3.2: Cálculo correcto con descuento por inmueble menor a 10 años
  Dado que:  el valor del predio es $150.000.000 COP
             y el año de construcción es 2019 (antigüedad 7 años — descuento 5%)
             y el estrato es 2 y 1 habitante
             y el agente seleccionó solo Incendio + Robo (12% del valor base)
  Cuando:    el motor ejecuta el cálculo
  Entonces:  primaBase = 150.000.000 × 0.01 = 1.500.000
             cargoRobo = 1.500.000 × 0.12 = 180.000
             totalBruto = 1.500.000 + 180.000 = 1.680.000
             descuentoAntiguedad = 1.680.000 × 0.05 = 84.000
             totalFinal = CEILING((1.680.000 - 84.000) / 1000) × 1000 = 1.596.000
```

**Error Path**
```gherkin
CRITERIO-3.3: Backend rechaza request con propertyValue igual a cero
  Dado que:  el sistema recibe un request al endpoint de cotización
             con propertyValue = 0
  Cuando:    el handler procesa la solicitud
  Entonces:  el sistema retorna HTTP 400
             con el mensaje: "El valor del predio debe ser mayor a cero."
```

**Edge Case**
```gherkin
CRITERIO-3.4: Inmueble con más de 30 años aplica recargo en Fontanería y Daños Eléctricos
  Dado que:  el valor del predio es $100.000.000 COP
             y el año de construcción es 1990 (antigüedad 36 años — recargo 10% en Fontanería y Eléctricos)
             y el agente seleccionó Fontanería + Daños Eléctricos
  Cuando:    el motor de cálculo ejecuta la fórmula
  Entonces:  primaBase = 100.000.000 × 0.01 = 1.000.000
             cargoFontaneria = 1.000.000 × 0.05 × 1.10 = 55.000
             cargoDanosElectricos = 1.000.000 × 0.07 × 1.10 = 77.000
             totalFinal = CEILING((1.000.000 + 55.000 + 77.000) / 1000) × 1000 = 1.132.000
```

**Edge Case**
```gherkin
CRITERIO-3.5: Estrato 5 aplica recargo del 10% en cobertura de Robo
  Dado que:  el valor del predio es $500.000.000 COP
             y el estrato es 5
             y el agente seleccionó Incendio + Robo
  Cuando:    el motor de cálculo ejecuta la fórmula
  Entonces:  primaBase = 500.000.000 × 0.01 = 5.000.000
             cargoRobo = 5.000.000 × 0.12 × 1.10 = 660.000
             totalFinal = CEILING((5.000.000 + 660.000) / 1000) × 1000 = 5.660.000
```

**Edge Case**
```gherkin
CRITERIO-3.6: Más de 5 habitantes aplica cargo fijo por uso intensivo
  Dado que:  el valor del predio es $200.000.000 COP
             y el número de habitantes es 6
             y el agente seleccionó el paquete Básico
  Cuando:    el motor de cálculo ejecuta la fórmula
  Entonces:  al total calculado se añade el cargo fijo de uso intensivo ($50.000 COP)
             antes de aplicar el redondeo al mil superior
```

---

#### HU-4: Visualización del Resultado de Cotización y Confirmación

```
Como:        Agente de seguros
Quiero:      Ver un resumen claro del resultado de cotización: prima mensual,
             coberturas incluidas, multiplicadores de riesgo aplicados y el
             nombre del paquete seleccionado (o "Personalizado" si es individual)
Para:        Informar al asegurado del costo y alcance exacto de su póliza
             antes de confirmar la contratación y evitar malentendidos post-emisión

Prioridad:   Alta
Estimación:  M
Dependencias: HU-3 (cotización calculada)
Capa:        Ambas
```

#### Criterios de Aceptación — HU-4

**Happy Path**
```gherkin
CRITERIO-4.1: Preview muestra todos los campos esperados tras cotización exitosa
  Dado que:  el motor de cálculo devolvió una cotización exitosa
  Cuando:    el componente HomePlanPreviewComponent se renderiza
  Entonces:  el preview muestra:
             - Nombre del paquete o "Personalizado"
             - Prima mensual formateada en COP (ej. $2.100.000/mes)
             - Prima anual (prima mensual × 12)
             - Lista de coberturas incluidas
             - Lista de multiplicadores de riesgo aplicados (con sus efectos)
             - Valor del predio utilizado para el cálculo
```

**Happy Path**
```gherkin
CRITERIO-4.2: Confirmación de póliza Home crea el documento con los datos correctos
  Dado que:  el agente completó todos los pasos del formulario y tiene una cotización válida
  Cuando:    hace clic en "Confirmar y Crear Póliza"
  Entonces:  el sistema llama a POST /api/v1/policies con type = "Home"
             y todos los campos de la cotización embebidos en el payload
             y navega a /policies con el mensaje "Póliza creada exitosamente"
```

**Error Path**
```gherkin
CRITERIO-4.3: No se puede confirmar la póliza sin haber cotizado previamente
  Dado que:  el agente está en el paso de confirmación
             y no existe una cotización válida en memoria (signal homeQuotation es null)
  Cuando:    intenta hacer clic en "Confirmar y Crear Póliza"
  Entonces:  el botón "Confirmar y Crear Póliza" permanece deshabilitado
             y el sistema muestra el mensaje:
             "Debe completar la cotización antes de confirmar la póliza."
```

**Edge Case**
```gherkin
CRITERIO-4.4: Error en el backend durante la cotización muestra snackbar de error
  Dado que:  el agente completó el formulario de datos del predio y coberturas
  Cuando:    el backend retorna un error HTTP 400 o 500 al endpoint de cotización
  Entonces:  el sistema muestra un snackbar con el mensaje:
             "Error al calcular la cotización. Por favor revise los datos ingresados."
             y la señal homeQuotation permanece en null
             y el botón "Confirmar y Crear Póliza" permanece deshabilitado
```

---

#### HU-5: Persistencia de Póliza de Hogar en el Sistema

```
Como:        Sistema de gestión de pólizas
Quiero:      Persistir en MongoDB la póliza de hogar con un snapshot completo de
             los datos del predio, las coberturas contratadas, los multiplicadores
             aplicados y la prima mensual calculada al momento de la contratación
Para:        Contar con un registro histórico inmutable de las condiciones bajo
             las cuales se emitió la póliza, independientemente de cambios futuros
             en las reglas de cotización

Prioridad:   Alta
Estimación:  M
Dependencias: HU-3, HU-4
Capa:        Backend
```

#### Criterios de Aceptación — HU-5

**Happy Path**
```gherkin
CRITERIO-5.1: Póliza de hogar creada con snapshot completo de cotización
  Dado que:  el handler CreatePolicyHandler recibe un CreatePolicyCommand con type = "Home"
             y todos los campos de HomePlan presentes y válidos
  Cuando:    el handler ejecuta Policy.CreateHomePolicy()
  Entonces:  la póliza se persiste en MongoDB con la propiedad HomePlan embebida:
             - packageName (nombre del paquete o "Personalizado")
             - propertyValue (valor del predio)
             - constructionYear (año de construcción)
             - stratum (estrato)
             - occupants (número de habitantes)
             - propertyType (tipo de inmueble)
             - selectedCoverages (lista de enum de coberturas)
             - appliedMultipliers (lista de multiplicadores aplicados con descripción)
             - baseMonthlyPremium (prima base calculada)
             - finalMonthlyPremium (prima final tras coberturas y multiplicadores)
```

**Error Path**
```gherkin
CRITERIO-5.2: Backend rechaza creación de póliza Home sin campos obligatorios
  Dado que:  el sistema recibe un CreatePolicyCommand con type = "Home"
             y sin los campos de HomePlan (sin propertyValue, constructionYear, stratum, occupants)
  Cuando:    el validator FluentValidation evalúa el comando
  Entonces:  el sistema retorna HTTP 400
             con los errores de validación correspondientes a los campos faltantes
```

**Edge Case**
```gherkin
CRITERIO-5.3: Pólizas de otros tipos no se ven afectadas por los campos de HomePlan
  Dado que:  el sistema recibe una petición de creación de póliza con type = "Vehicle"
             y sin ningún campo de HomePlan en el payload
  Cuando:    el handler procesa el comando
  Entonces:  la póliza se crea correctamente con HomePlan = null
             y ningún error de validación relacionado con HomePlan es disparado
```

---

### 1.3 Reglas de Negocio

| ID | Regla | Tipo | Módulos afectados |
|----|-------|------|-------------------|
| RN-01 | `primaBase = propertyValue × 0.01` — el 1% del valor comercial del predio es la base de cálculo. Incendio y Explosión está incluida en esta prima base sin costo adicional. | Invariante | Backend |
| RN-02 | Las coberturas variables se calculan como porcentaje sobre `primaBase`: Robo (+10% a 15% → usar 12% como punto medio), Fontanería (+5%), Pintura/Estéticos (+3%), Daños Eléctricos (+7%), Inhabitabilidad (+4%). | Cálculo | Backend |
| RN-03 | Las coberturas fijas tienen cargo independiente del valor del predio: Rotura de Cristales ($50.000 COP), Responsabilidad Civil ($175.000 COP, escalado según estrato: E1-2 = $100.000, E3-4 = $175.000, E5-6 = $250.000), Defensa Jurídica ($30.000), Asistencia en el Hogar ($45.000), Daños por Agua/Peritaje ($65.000 COP). | Invariante | Backend |
| RN-04 | Antigüedad < 10 años: descuento del 5% sobre el total bruto calculado (antes del redondeo). | Condicional | Backend |
| RN-05 | Antigüedad > 30 años: recargo del 10% aplicado exclusivamente sobre los cargos de Fontanería y Daños Eléctricos (no sobre el total). | Condicional | Backend |
| RN-06 | Estrato 1 o 2: Responsabilidad Civil usa tarifa reducida ($100.000 COP). Estrato 5 o 6: Responsabilidad Civil usa tarifa elevada ($250.000 COP). Estrato 3 o 4: tarifa media ($175.000 COP). | Condicional | Backend |
| RN-07 | Estrato 5 o 6: recargo del 10% sobre el cargo de la cobertura Robo (si está seleccionada). | Condicional | Backend |
| RN-08 | Más de 5 habitantes: cargo fijo por "uso intensivo" de $50.000 COP sumado al total bruto. | Condicional | Backend |
| RN-09 | Más de 5 coberturas seleccionadas (excluyendo Incendio que es base): descuento multicobertura del 5% sobre el total bruto antes del cargo de uso intensivo y antes del redondeo. Los descuentos de antigüedad y multicobertura son acumulables (se aplican secuencialmente). | Condicional | Backend |
| RN-10 | `primaFinal = CEILING(totalBruto / 1000) × 1000` — redondeo al mil superior con aritmética `decimal`. La prima mensual es `primaFinal`. No existe prima anual con descuento (a diferencia de Vehículo). | Cálculo | Backend |
| RN-11 | Los tres paquetes predefinidos son inmutables en código: Básico (Incendio + Fontanería), Estándar (Básico + Robo + Daños Eléctricos), Premium (Estándar + RC + Cristales + Asistencia + Inhabitabilidad + Defensa Jurídica). | Catálogo | Backend, Frontend |
| RN-12 | Al seleccionar un paquete en el frontend, el selector de coberturas individuales refleja las coberturas del paquete marcadas. El agente puede añadir coberturas adicionales sobre el paquete seleccionado. En ese caso el nombre guardado es el del paquete base. | UX | Frontend |
| RN-13 | Si el agente selecciona coberturas sin elegir un paquete (modo "Personalizado"), el campo `packageName` en el snapshot se almacena como `"Personalizado"`. | Dominio | Backend, Frontend |
| RN-14 | `PolicyType.Home = 4` ya existe en el enum `PolicyType` del dominio — no se requiere modificación del enum. | Arquitectura | Backend |
| RN-15 | El período de cobertura es siempre 365 días fijos: `endDate = startDate + 365 días`, reutilizando `QuotationService.CalculateEndDate()`. | Invariante | Backend |
| RN-16 | `startDate` debe ser ≥ fecha actual — validación en frontend y backend. | Validación | Frontend, Backend |
| RN-17 | `insuredAmount` en la póliza Home almacena `primaBase` (1% del valor del predio) como valor asegurado de referencia. La prima mensual final se almacena en `MonthlyPremium`. | Arquitectura | Backend |
| RN-18 | La antigüedad se determina como `añoActual - constructionYear`. Si el resultado es 0, la antigüedad es 0 años (aplica descuento por < 10 años). | Derivado | Backend |

### 1.4 Catálogo de Coberturas

| Cobertura | Enum | Tipo de cargo | Cálculo |
|-----------|------|---------------|---------|
| Incendio y Explosión | `FireExplosion` | Incluido en base | Ya en el 1% base — sin cargo adicional |
| Robo y Hurto | `Theft` | Variable | +12% del valor base (+ 10% recargo si estrato 5-6) |
| Fontanería/Daños por Agua | `Plumbing` | Variable | +5% del valor base (+ 10% recargo si antigüedad > 30 años) |
| Pintura/Daños Estéticos | `AestheticDamage` | Variable | +3% del valor base |
| Daños Eléctricos | `ElectricalDamage` | Variable | +7% del valor base (+ 10% recargo si antigüedad > 30 años) |
| Rotura de Cristales | `GlassBreakage` | Fijo | $50.000 COP |
| Responsabilidad Civil | `CivilLiability` | Escalado por estrato | E1-2: $100.000 / E3-4: $175.000 / E5-6: $250.000 COP |
| Inhabitabilidad | `Uninhabitability` | Variable | +4% del valor base |
| Defensa Jurídica | `LegalDefense` | Fijo | $30.000 COP |
| Asistencia en el Hogar | `HomeAssistance` | Fijo | $45.000 COP |
| Daños por Agua/Peritaje | `WaterDamageExpert` | Fijo | $65.000 COP |

### 1.5 Paquetes Predefinidos

| Paquete | `packageId` | Coberturas incluidas | Coberturas adicionales sobre base |
|---------|-------------|----------------------|-----------------------------------|
| Básico | `basic` | FireExplosion + Plumbing | Plumbing (+5%) |
| Estándar | `standard` | Básico + Theft + ElectricalDamage | Plumbing (+5%), Theft (+12%), ElectricalDamage (+7%) |
| Premium | `premium` | Estándar + CivilLiability + GlassBreakage + HomeAssistance + Uninhabitability + LegalDefense | Todo Estándar + RC (escalado) + Cristales ($50k) + Asistencia ($45k) + Inhabitabilidad (+4%) + Defensa ($30k) |

> **Nota:** El paquete Premium incluye más de 5 coberturas adicionales (excluyendo Incendio), por lo que siempre activa el descuento multicobertura del 5%.

### 1.6 Fórmulas de Cálculo (Referencia Exacta)

```
// PASO 1 — Prima base
primaBase = propertyValue × 0.01

// PASO 2 — Calcular cargo por cada cobertura seleccionada
// Coberturas variables (sobre primaBase)
cargoRobo            = primaBase × 0.12
                       × (estrato >= 5 ? 1.10 : 1.00)   // RN-07

cargoFontaneria      = primaBase × 0.05
                       × (antiguedad > 30 ? 1.10 : 1.00) // RN-05

cargoEsteticos       = primaBase × 0.03

cargoDanosElectricos = primaBase × 0.07
                       × (antiguedad > 30 ? 1.10 : 1.00) // RN-05

cargoInhabitabilidad = primaBase × 0.04

// Coberturas fijas
cargoCristales          = 50_000m
cargoRC                 = estrato <= 2 ? 100_000m
                        : estrato <= 4 ? 175_000m
                        :                250_000m          // RN-03, RN-06
cargoDefensaJuridica    = 30_000m
cargoAsistenciaHogar    = 45_000m
cargoDanosAguaPeritaje  = 65_000m

// PASO 3 — Sumar coberturas seleccionadas a primaBase
// Incendio está incluida en primaBase (sin cargo adicional)
totalCoberturas = primaBase + SUMA(cargos de coberturas seleccionadas)

// PASO 4 — Descuento multicobertura (más de 5 coberturas excluyendo Incendio)
coberturasSinIncendio = coberturas.Count(c => c != FireExplosion)
if (coberturasSinIncendio > 5)
    totalCoberturas = totalCoberturas × 0.95               // RN-09

// PASO 5 — Descuento por antigüedad < 10 años
if (antiguedad < 10)
    totalCoberturas = totalCoberturas × 0.95               // RN-04

// PASO 6 — Cargo por uso intensivo (más de 5 habitantes)
if (occupants > 5)
    totalCoberturas = totalCoberturas + 50_000m            // RN-08

// PASO 7 — Redondeo al mil superior
primaFinal = CEILING(totalCoberturas / 1000m) × 1000m      // RN-10
```

**Tabla de verificación de cálculos (ejemplos):**

| propertyValue | Año Construcción | Estrato | Habitantes | Coberturas | primaBase | Cargos adicionales | Descuentos | primaFinal |
|--------------|-----------------|---------|-----------|------------|----------:|-------------------:|:----------:|----------:|
| $200.000.000 | 2020 (6 años) | 3 | 2 | Básico (Incendio+Fontanería) | 2.000.000 | +100.000 | Ninguno | 2.100.000 |
| $200.000.000 | 2020 (6 años) | 3 | 2 | Básico | 2.000.000 | +100.000 | -5% antigüedad | 1.995.000 |
| $300.000.000 | 1985 (41 años) | 4 | 3 | Estándar | 3.000.000 | +150k+360k+231k | Recargo fontan.+elect. | 3.741.000 |
| $500.000.000 | 2015 (11 años) | 5 | 7 | Premium (9 coberturas) | 5.000.000 | múltiples | -5% multicobertura + uso intensivo | ~6.207.000 |
| $100.000.000 | 2022 (4 años) | 1 | 2 | Robo+Fontanería | 1.000.000 | +120k+50k | -5% antigüedad + RC E1 | 1.121.000 |

> **Nota sobre CEILING en C#:** usar `Math.Ceiling(value / 1000m) * 1000m` con aritmética `decimal`. Los descuentos de multicobertura y antigüedad son acumulables y se aplican antes del cargo de uso intensivo.

---

## 2. DISEÑO

### 2.1 Modelos de Datos

#### Entidades afectadas

| Entidad | Almacén | Cambios | Descripción |
|---------|---------|---------|-------------|
| `Policy` | MongoDB `policies` | Modificada | Añadir propiedad `HomePlan?: HomePlanSelection` y factory `CreateHomePolicy()` |
| `HomeCoverage` | In-memory (enum) | Nuevo | Enum con los 11 tipos de cobertura |
| `HomePlanPackage` | In-memory (catálogo) | Nuevo | Value Object con Id, Name, y lista de coberturas del paquete |
| `HomePlanPackageCatalog` | In-memory (estático) | Nuevo | Catálogo estático de los 3 paquetes; análogo a `VehiclePlanCatalog` |
| `HomePlanSelection` | Embebido en `Policy` | Nuevo | Snapshot del resultado de cotización al momento de contratación |
| `HomeQuotation` | Value Object transitorio | Nuevo | Resultado del motor de cálculo |
| `HomePricingService` | In-memory (servicio de dominio) | Nuevo | Motor de cálculo de prima de hogar |
| `CreatePolicyCommand` | Application | Modificado | Añadir 7 campos Home opcionales |

#### Enum `HomeCoverage`

```csharp
public enum HomeCoverage
{
    FireExplosion      = 1,  // Incendio y Explosión (incluida en base)
    Theft              = 2,  // Robo y Hurto
    Plumbing           = 3,  // Fontanería/Daños por Agua
    AestheticDamage    = 4,  // Pintura/Daños Estéticos
    ElectricalDamage   = 5,  // Daños Eléctricos
    GlassBreakage      = 6,  // Rotura de Cristales
    CivilLiability     = 7,  // Responsabilidad Civil
    Uninhabitability   = 8,  // Inhabitabilidad
    LegalDefense       = 9,  // Defensa Jurídica
    HomeAssistance     = 10, // Asistencia en el Hogar
    WaterDamageExpert  = 11  // Daños por Agua/Peritaje
}
```

Serializado en MongoDB como string (ej. `"Theft"`) mediante configuración del serializer BSON.

#### Enum `PropertyType`

```csharp
public enum PropertyType
{
    House              = 1,  // Casa
    Apartment          = 2,  // Apartamento
    CommercialPremises = 3   // Local Comercial
}
```

#### Campos — `HomePlanPackage` (Domain Value Object)

| Campo | Tipo C# | Descripción |
|-------|---------|-------------|
| `Id` | `string` | Identificador único (kebab-case): `basic`, `standard`, `premium` |
| `Name` | `string` | Nombre legible: "Paquete Básico", "Paquete Estándar", "Paquete Premium" |
| `Coverages` | `IReadOnlyList<HomeCoverage>` | Coberturas incluidas en el paquete |

#### Catálogo de Paquetes — `HomePlanPackageCatalog`

| Paquete | `packageId` | Coberturas incluidas |
|---------|-------------|----------------------|
| Paquete Básico | `basic` | FireExplosion, Plumbing |
| Paquete Estándar | `standard` | FireExplosion, Plumbing, Theft, ElectricalDamage |
| Paquete Premium | `premium` | FireExplosion, Plumbing, Theft, ElectricalDamage, CivilLiability, GlassBreakage, HomeAssistance, Uninhabitability, LegalDefense |

#### Campos — `HomePlanSelection` (Snapshot embebido en Policy)

| Campo | Tipo C# | Descripción |
|-------|---------|-------------|
| `PackageId` | `string?` | ID del paquete base seleccionado (null si es personalizado) |
| `PackageName` | `string` | Nombre del paquete o "Personalizado" |
| `PropertyValue` | `decimal` | Valor comercial del predio (COP) |
| `ConstructionYear` | `int` | Año de construcción del inmueble |
| `Stratum` | `int` | Estrato socioeconómico (1-6) |
| `Occupants` | `int` | Número de habitantes |
| `PropertyType` | `PropertyType` | Tipo de inmueble |
| `SelectedCoverages` | `IReadOnlyList<HomeCoverage>` | Coberturas contratadas (snapshot) |
| `AppliedMultipliers` | `IReadOnlyList<string>` | Descripción de multiplicadores aplicados |
| `BaseMonthlyPremium` | `decimal` | Prima base (1% del valor del predio) |
| `FinalMonthlyPremium` | `decimal` | Prima mensual final tras coberturas y multiplicadores |

#### Campos — `HomeQuotation` (Value Object transitorio — resultado del motor)

| Campo | Tipo C# | Descripción |
|-------|---------|-------------|
| `PropertyValue` | `decimal` | Valor del predio de entrada |
| `ConstructionYear` | `int` | Año de construcción |
| `PropertyAge` | `int` | Antigüedad calculada (`añoActual - constructionYear`) |
| `Stratum` | `int` | Estrato socioeconómico |
| `Occupants` | `int` | Número de habitantes |
| `PropertyType` | `PropertyType` | Tipo de inmueble |
| `BaseMonthlyPremium` | `decimal` | `propertyValue × 0.01` |
| `SelectedCoverages` | `IReadOnlyList<HomeCoverage>` | Coberturas calculadas |
| `AppliedMultipliers` | `IReadOnlyList<string>` | Descripciones de multiplicadores aplicados |
| `FinalMonthlyPremium` | `decimal` | Prima final redondeada al mil superior |

#### Modificaciones en `Policy.cs`

```csharp
// Propiedad nueva (análoga a VehiclePlan)
public HomePlanSelection? HomePlan { get; private set; }

// Factory nueva
public static Policy CreateHomePolicy(
    PolicyNumber number,
    InsuredPerson insured,
    CoveragePeriod coverage,
    string? packageId,
    decimal propertyValue,
    int constructionYear,
    int stratum,
    int occupants,
    PropertyType propertyType,
    IReadOnlyList<HomeCoverage> selectedCoverages,
    DateOnly today)
```

#### Modificaciones en `CreatePolicyCommand.cs`

```csharp
/// <summary>Solo para pólizas de tipo Home.</summary>
public string? HomePlanPackageId { get; init; }

/// <summary>Valor comercial del predio (COP). Solo para pólizas Home.</summary>
public decimal? HomePropertyValue { get; init; }

/// <summary>Año de construcción del inmueble. Solo para pólizas Home.</summary>
public int? HomeConstructionYear { get; init; }

/// <summary>Estrato socioeconómico (1-6). Solo para pólizas Home.</summary>
public int? HomeStratum { get; init; }

/// <summary>Número de habitantes. Solo para pólizas Home.</summary>
public int? HomeOccupants { get; init; }

/// <summary>Tipo de inmueble. Solo para pólizas Home.</summary>
public string? HomePropertyType { get; init; }

/// <summary>Coberturas seleccionadas. Solo para pólizas Home.</summary>
public IReadOnlyList<string>? HomeSelectedCoverages { get; init; }
```

> Los documentos existentes en MongoDB no se ven afectados. `HomePlan` es `null` para pólizas de tipos previos.

---

### 2.2 API Endpoints

#### NUEVO: `GET /api/v1/home-plans`

- **Descripción:** Lista los tres paquetes de cobertura de hogar disponibles con sus coberturas incluidas
- **Auth requerida:** sí (Bearer token)
- **Response 200:**
  ```json
  [
    {
      "packageId": "basic",
      "packageName": "Paquete Básico",
      "coverages": ["FireExplosion", "Plumbing"]
    },
    {
      "packageId": "standard",
      "packageName": "Paquete Estándar",
      "coverages": ["FireExplosion", "Plumbing", "Theft", "ElectricalDamage"]
    },
    {
      "packageId": "premium",
      "packageName": "Paquete Premium",
      "coverages": ["FireExplosion", "Plumbing", "Theft", "ElectricalDamage",
                    "CivilLiability", "GlassBreakage", "HomeAssistance",
                    "Uninhabitability", "LegalDefense"]
    }
  ]
  ```
- **Response 401:** token ausente o expirado

#### NUEVO: `POST /api/v1/home-plans/calculate`

- **Descripción:** Ejecuta el motor de cotización con los datos del predio y las coberturas seleccionadas, y retorna la prima mensual final
- **Auth requerida:** sí (Bearer token)
- **Request Body:**
  ```json
  {
    "propertyValue": 300000000,
    "constructionYear": 2015,
    "stratum": 3,
    "occupants": 4,
    "propertyType": "Apartment",
    "selectedCoverages": ["FireExplosion", "Plumbing", "Theft", "ElectricalDamage"]
  }
  ```
- **Response 200:**
  ```json
  {
    "propertyValue": 300000000,
    "constructionYear": 2015,
    "propertyAge": 11,
    "stratum": 3,
    "occupants": 4,
    "propertyType": "Apartment",
    "baseMonthlyPremium": 3000000,
    "selectedCoverages": ["FireExplosion", "Plumbing", "Theft", "ElectricalDamage"],
    "appliedMultipliers": [],
    "finalMonthlyPremium": 3570000
  }
  ```
- **Response 400:** `propertyValue` ≤ 0, `constructionYear` fuera de rango, `stratum` fuera de rango [1-6], `occupants` < 1, coberturas vacías o con valores inválidos
- **Response 422:** combinación de coberturas incoherente (ej. lista vacía tras parseo)

> **Nota:** Se usa `POST` en lugar de `GET` porque el payload de coberturas (lista variable) no es idiomático como query params. Es análogo al patrón de otros módulos donde el cálculo recibe múltiples parámetros complejos.

#### MODIFICADO: `POST /api/v1/policies`

- **Descripción:** Crea una póliza de tipo Home — extiende el endpoint existente
- **Campos adicionales en el Request Body para `type = "Home"`:**
  ```json
  {
    "type": "Home",
    "homePlanPackageId": "standard",
    "homePropertyValue": 300000000,
    "homeConstructionYear": 2015,
    "homeStratum": 3,
    "homeOccupants": 4,
    "homePropertyType": "Apartment",
    "homeSelectedCoverages": ["FireExplosion", "Plumbing", "Theft", "ElectricalDamage"],
    "coverageStartDate": "2026-05-01",
    "insuredFirstName": "...",
    "insuredLastName": "...",
    "insuredDocumentType": "CC",
    "insuredDocumentId": "...",
    "insuredBirthDate": "1985-06-20"
  }
  ```
- **Response 201:** póliza creada con `homePlan` embebido en el documento
- **Response 400:** campos de hogar faltantes cuando `type = "Home"`
- **Response 422:** cobertura no reconocida en `homeSelectedCoverages`

---

### 2.3 Diseño Frontend

El frontend replica exactamente el patrón implementado en el módulo de Vehículo (SPEC-010) dentro del componente `PolicyCreateComponent` (MatStepper). Se agrega un paso intermedio condicional para la cotización de hogar que aparece solo cuando `selectedType() === 'Home'`.

#### Componentes nuevos

| Componente | Archivo | Descripción |
|------------|---------|-------------|
| `HomeDataFormComponent` | `ui/blocks/home-data-form/home-data-form.component.ts` | Formulario reactivo para captura de datos del predio: valor comercial, año de construcción, estrato, número de habitantes, tipo de inmueble y selección de coberturas/paquete. Emite el resultado al padre vía `output()` |
| `HomePlanSelectorComponent` | `ui/blocks/home-plan-selector/home-plan-selector.component.ts` | Muestra los tres paquetes predefinidos como tarjetas seleccionables más la opción "Personalizado". Análogo a `VehiclePlanSelectorComponent`. Usa Signals para el paquete seleccionado |
| `HomePlanPreviewComponent` | `ui/blocks/home-plan-preview/home-plan-preview.component.ts` | Resumen de la cotización: tipo de inmueble, prima mensual, coberturas incluidas, multiplicadores de riesgo aplicados. Análogo a `VehiclePlanPreviewComponent` |

#### Servicio nuevo

| Servicio | Archivo | Endpoints que consume |
|----------|---------|----------------------|
| `HomePlansService` | `core/service/home-plans.service.ts` | `GET /api/v1/home-plans` y `POST /api/v1/home-plans/calculate` |

#### Modelos nuevos

```typescript
// core/models/home-plan-selection.model.ts

export type HomeCoverage =
  | 'FireExplosion'
  | 'Theft'
  | 'Plumbing'
  | 'AestheticDamage'
  | 'ElectricalDamage'
  | 'GlassBreakage'
  | 'CivilLiability'
  | 'Uninhabitability'
  | 'LegalDefense'
  | 'HomeAssistance'
  | 'WaterDamageExpert';

export type PropertyType = 'House' | 'Apartment' | 'CommercialPremises';

export interface HomeCoverageOption {
  value: HomeCoverage;
  label: string;
  description: string;
}

export const HOME_COVERAGE_OPTIONS: HomeCoverageOption[] = [
  { value: 'FireExplosion',     label: 'Incendio y Explosión',          description: 'Incluida en la prima base' },
  { value: 'Theft',             label: 'Robo y Hurto',                  description: '+12% del valor base' },
  { value: 'Plumbing',          label: 'Fontanería/Daños por Agua',     description: '+5% del valor base' },
  { value: 'AestheticDamage',   label: 'Pintura/Daños Estéticos',       description: '+3% del valor base' },
  { value: 'ElectricalDamage',  label: 'Daños Eléctricos',              description: '+7% del valor base' },
  { value: 'GlassBreakage',     label: 'Rotura de Cristales',           description: '$50.000 COP fijo' },
  { value: 'CivilLiability',    label: 'Responsabilidad Civil',         description: 'Escalado por estrato ($100k-$250k)' },
  { value: 'Uninhabitability',  label: 'Inhabitabilidad',               description: '+4% del valor base' },
  { value: 'LegalDefense',      label: 'Defensa Jurídica',              description: '$30.000 COP fijo' },
  { value: 'HomeAssistance',    label: 'Asistencia en el Hogar',        description: '$45.000 COP fijo' },
  { value: 'WaterDamageExpert', label: 'Daños por Agua/Peritaje',       description: '$65.000 COP fijo' },
];

export interface HomePlanPackage {
  packageId: string;
  packageName: string;
  coverages: HomeCoverage[];
}

export interface HomeQuotationRequest {
  propertyValue: number;
  constructionYear: number;
  stratum: number;
  occupants: number;
  propertyType: PropertyType;
  selectedCoverages: HomeCoverage[];
}

export interface HomeQuotationResult {
  propertyValue: number;
  constructionYear: number;
  propertyAge: number;
  stratum: number;
  occupants: number;
  propertyType: PropertyType;
  baseMonthlyPremium: number;
  selectedCoverages: HomeCoverage[];
  appliedMultipliers: string[];
  finalMonthlyPremium: number;
}
```

#### Output del `HomeDataFormComponent`

```typescript
// Interfaz del output emitido por HomeDataFormComponent al padre
export interface HomeDataFormValue {
  propertyValue: number;
  constructionYear: number;
  stratum: number;
  occupants: number;
  propertyType: PropertyType;
  selectedCoverages: HomeCoverage[];
  packageId: string | null;  // null si es selección personalizada
}
```

#### Signals y estado en `PolicyCreateComponent`

```typescript
// Señales nuevas a agregar (análogo al patrón de vehicleQuotation)
isHomeType         = signal(false);
homeQuotation      = signal<HomeQuotationResult | null>(null);
homeQuoteLoading   = signal(false);
homePackageId      = signal<string | null>(null);
```

#### Integración en `onTypeChange()`

```typescript
// Añadir al switch/if de onTypeChange() en PolicyCreateComponent
} else if (type === 'Home') {
  monthlyPremiumCtrl.clearValidators();
  monthlyPremiumCtrl.setValue(null);
  endDateCtrl.clearValidators();
  endDateCtrl.setValue(null);
  insuredAmountCtrl.clearValidators();
  insuredAmountCtrl.setValue(null);
  this.homeQuotation.set(null);
  this.homePackageId.set(null);
}
```

#### Integración en `submit()`

```typescript
// Campos adicionales en el payload de creación de póliza para Home
...(isHome && homeQuote ? {
  homePlanPackageId:      this.homePackageId() ?? undefined,
  homePropertyValue:      homeQuote.propertyValue,
  homeConstructionYear:   homeQuote.constructionYear,
  homeStratum:            homeQuote.stratum,
  homeOccupants:          homeQuote.occupants,
  homePropertyType:       homeQuote.propertyType,
  homeSelectedCoverages:  homeQuote.selectedCoverages,
} : {}),
```

#### Flujo de navegación (MatStepper)

```
Paso 1: Tipo de Póliza
        └─ [usuario selecciona "Home"]
Paso 2: Datos del Asegurado (existente — sin cambios)
Paso 3: Datos del Inmueble y Coberturas  [NUEVO — visible solo si isHomeType()]
        └─ HomeDataFormComponent
           ├─ Subformulario: propertyValue, constructionYear, stratum, occupants, propertyType
           ├─ Selector de paquete (HomePlanSelectorComponent embebido) o coberturas individuales
           └─ Botón "Cotizar" → llama a HomePlansService.calculate()
           └─ Muestra spinner durante la llamada
Paso 4: Resumen de Cotización  [NUEVO — visible solo si isHomeType() && homeQuotation()]
        └─ HomePlanPreviewComponent con resultado completo
        └─ Botón "Continuar" habilita solo si homeQuotation() !== null
Paso 5: Confirmación (existente — muestra HomePlanPreviewComponent si isHomeType())
        └─ Botón "Confirmar y Crear Póliza" → llama a PoliciesCoreService.create()
```

> **Alternativa de implementación:** El HomePlanSelectorComponent puede estar embebido dentro de HomeDataFormComponent (como un selector de paquete que precarga las coberturas) en lugar de ser un paso separado. El equipo de frontend decide durante la implementación según el espacio disponible en el stepper. El requisito es que el agente pueda ver claramente qué coberturas está seleccionando antes de cotizar.

---

### 2.4 Arquitectura de Capas Backend

Siguiendo el patrón Clean Architecture + MediatR establecido en módulos anteriores (SPEC-010):

```
HomePlansController
    └─ GetHomePlansQuery           → GetHomePlansHandler      → HomePlanPackageCatalog
    └─ CalculateHomeQuotationCommand → CalculateHomeQuotationHandler → HomePricingService
                                                                       └─ HomePlanPackageCatalog
PoliciesController (POST existente)
    └─ CreatePolicyCommand (modificado)
        └─ CreatePolicyHandler (modificado)
            └─ Policy.CreateHomePolicy()
                └─ HomePricingService.Calculate()
```

#### Archivos a crear

| Capa | Archivo | Descripción |
|------|---------|-------------|
| Domain | `Policies/HomePlan/HomeCoverage.cs` | Enum con los 11 tipos de cobertura |
| Domain | `Policies/HomePlan/PropertyType.cs` | Enum con los 3 tipos de inmueble |
| Domain | `Policies/HomePlan/HomePlanPackage.cs` | Value Object del paquete de cobertura |
| Domain | `Policies/HomePlan/HomePlanPackageCatalog.cs` | Catálogo estático de los 3 paquetes |
| Domain | `Policies/HomePlan/HomePlanSelection.cs` | Snapshot embebido en Policy |
| Domain | `Policies/HomePlan/HomeQuotation.cs` | Value Object transitorio del motor de cálculo |
| Domain | `Policies/HomePlan/HomePricingService.cs` | Motor de cálculo de prima de hogar |
| Domain | `Exceptions/InvalidHomeCoverageException.cs` | Excepción cobertura no reconocida |
| Application | `HomePlans/DTOs/HomePlanPackageDto.cs` | DTO de respuesta del catálogo de paquetes |
| Application | `HomePlans/DTOs/HomeQuotationDto.cs` | DTO de respuesta de cotización |
| Application | `HomePlans/Queries/GetHomePlans/GetHomePlansQuery.cs` | Query MediatR |
| Application | `HomePlans/Queries/GetHomePlans/GetHomePlansHandler.cs` | Handler MediatR |
| Application | `HomePlans/Commands/CalculateHomeQuotation/CalculateHomeQuotationCommand.cs` | Command MediatR |
| Application | `HomePlans/Commands/CalculateHomeQuotation/CalculateHomeQuotationHandler.cs` | Handler MediatR |
| API | `Controllers/HomePlansController.cs` | Controller REST — análogo a `VehiclePlansController` |

#### Archivos a modificar

| Capa | Archivo | Cambio |
|------|---------|--------|
| Domain | `Policies/Policy.cs` | Añadir propiedad `HomePlan` y factory `CreateHomePolicy()` |
| Application | `Policies/Commands/CreatePolicy/CreatePolicyCommand.cs` | Añadir 7 campos Home |
| Application | `Policies/Commands/CreatePolicy/CreatePolicyHandler.cs` | Añadir rama `case PolicyType.Home` |
| Application | `Policies/Commands/CreatePolicy/CreatePolicyValidator.cs` | Añadir validaciones para campos Home |

---

### 2.5 Notas de Implementación

- **`HomePricingService`:** Análogo a `VehiclePricingService`. Recibe `propertyValue`, `constructionYear`, `stratum`, `occupants`, `propertyType`, `selectedCoverages` y `currentYear`. Retorna un `HomeQuotation`. Toda la lógica matemática vive aquí — no en el handler ni en el controlador.
- **Aritmética decimal:** Usar exclusivamente `decimal` para todos los cálculos. El CEILING se implementa como `Math.Ceiling(value / 1000m) * 1000m`.
- **El año actual:** El handler inyecta `DateOnly.FromDateTime(DateTime.UtcNow)` y extrae `.Year`. No usar `DateTime.Now`.
- **Serialización de `HomeCoverage`:** Serializar como string en MongoDB usando el mismo mecanismo de BSON que `DocumentType` en SPEC-011. En la API, el campo `homeSelectedCoverages` recibe strings que el handler convierte al enum con `Enum.Parse<HomeCoverage>()`.
- **`PolicyType.Home = 4`** ya existe en el enum — no modificar.
- **`CalculateHomeQuotation` como Command (no Query):** Aunque no persiste datos, se usa MediatR Command porque recibe un cuerpo POST con lista de coberturas. Si el equipo prefiere Query con parámetros individuales, es igualmente válido — lo importante es no poner lógica en el controller.
- **Frontend — `HomeDataFormComponent`:** Emite los datos cuando el formulario es válido y el usuario hace clic en "Cotizar" (no on-change). Análogo al comportamiento de `VehicleDataFormComponent`.
- **`isHome` guard en el submit del frontend:** Al igual que `isVehicle`, debe añadirse la rama `isHome` en el método `submit()` de `PolicyCreateComponent` para incluir los campos de hogar en el payload y calcular correctamente `insuredAmount` y `monthlyPremium`.

---

## 3. LISTA DE TAREAS

> Checklist accionable para todos los agentes. Marcar cada ítem (`[x]`) al completarlo.
> El Orchestrator monitorea este checklist para determinar el progreso.

### Backend

#### Dominio
- [ ] Crear `Policies/HomePlan/HomeCoverage.cs` — enum con los 11 tipos de cobertura (serialización BSON como string)
- [ ] Crear `Policies/HomePlan/PropertyType.cs` — enum con los 3 tipos de inmueble
- [ ] Crear `Policies/HomePlan/HomePlanPackage.cs` — Value Object con Id, Name, Coverages
- [ ] Crear `Policies/HomePlan/HomePlanPackageCatalog.cs` — catálogo estático de 3 paquetes
- [ ] Crear `Policies/HomePlan/HomePlanSelection.cs` — snapshot embebido en Policy (10 campos)
- [ ] Crear `Policies/HomePlan/HomeQuotation.cs` — Value Object transitorio del motor
- [ ] Crear `Policies/HomePlan/HomePricingService.cs` — motor completo (fórmula 7 pasos, RN-01 a RN-10)
- [ ] Crear `Exceptions/InvalidHomeCoverageException.cs`
- [ ] Modificar `Policies/Policy.cs` — añadir propiedad `HomePlan` y factory `CreateHomePolicy()`

#### Application
- [ ] Crear `HomePlans/DTOs/HomePlanPackageDto.cs`
- [ ] Crear `HomePlans/DTOs/HomeQuotationDto.cs`
- [ ] Crear `HomePlans/Queries/GetHomePlans/GetHomePlansQuery.cs`
- [ ] Crear `HomePlans/Queries/GetHomePlans/GetHomePlansHandler.cs`
- [ ] Crear `HomePlans/Commands/CalculateHomeQuotation/CalculateHomeQuotationCommand.cs`
- [ ] Crear `HomePlans/Commands/CalculateHomeQuotation/CalculateHomeQuotationHandler.cs`
- [ ] Modificar `Policies/Commands/CreatePolicy/CreatePolicyCommand.cs` — añadir 7 campos Home
- [ ] Modificar `Policies/Commands/CreatePolicy/CreatePolicyHandler.cs` — añadir rama `PolicyType.Home`
- [ ] Modificar `Policies/Commands/CreatePolicy/CreatePolicyValidator.cs` — validaciones campos Home (propertyValue > 0, constructionYear rango, stratum 1-6, occupants >= 1, coverages no vacía)

#### API
- [ ] Crear `Controllers/HomePlansController.cs` — `GET /api/v1/home-plans` y `POST /api/v1/home-plans/calculate`
- [ ] Registrar rutas en `Program.cs` (verificar convención del proyecto)

#### Tests Backend
- [ ] `HomePricingService_BasicPackage_NoRiskMultipliers_ReturnsCorrectPremium` — happy path paquete básico sin multiplicadores
- [ ] `HomePricingService_PropertyAge_LessThan10Years_Applies5PercentDiscount` — descuento antigüedad < 10 años
- [ ] `HomePricingService_PropertyAge_MoreThan30Years_AppliesPlumbingAndElectricalSurcharge` — recargo > 30 años
- [ ] `HomePricingService_Stratum5_AppliesTheftSurcharge10Percent` — recargo estrato 5 en Robo
- [ ] `HomePricingService_Stratum6_AppliesTheftSurcharge10Percent` — recargo estrato 6 en Robo
- [ ] `HomePricingService_Stratum1_CivilLiabilityUsesReducedRate` — RC estrato 1 = $100.000
- [ ] `HomePricingService_Stratum4_CivilLiabilityUsesMediumRate` — RC estrato 4 = $175.000
- [ ] `HomePricingService_Stratum6_CivilLiabilityUsesHighRate` — RC estrato 6 = $250.000
- [ ] `HomePricingService_MoreThan5Occupants_AddsFixedUsageCharge` — cargo uso intensivo
- [ ] `HomePricingService_MoreThan5CoveragesExcludingFire_AppliesMultiCoverageDiscount` — descuento multicobertura
- [ ] `HomePricingService_PremiumPackage_AlwaysActivatesMultiCoverageDiscount` — Premium activa multicobertura siempre
- [ ] `HomePricingService_AgeAndMultiCoverageDiscounts_AreAccumulated` — descuentos acumulables
- [ ] `HomePricingService_CeilingRounding_AppliedToFinalPremium` — redondeo al mil superior
- [ ] `HomePricingService_FireExplosionIncludedInBase_NoAdditionalCharge` — Incendio sin cargo extra
- [ ] `CalculateHomeQuotationHandler_ValidRequest_ReturnsQuotation` — happy path handler
- [ ] `CalculateHomeQuotationHandler_InvalidPropertyValue_Throws` — validación propertyValue <= 0
- [ ] `CalculateHomeQuotationHandler_EmptyCoverages_Throws` — validación coberturas vacías
- [ ] `CreatePolicyHandler_HomePolicy_PersistsWithHomePlanSnapshot` — póliza Home creada con snapshot
- [ ] `CreatePolicyHandler_HomePolicy_InvalidCoverage_ThrowsInvalidHomeCoverageException` — excepción cobertura inválida
- [ ] `GetHomePlansHandler_ReturnsThreePackages_WithCorrectCoverages` — catálogo de paquetes correcto

### Frontend

#### Implementación
- [ ] Crear `core/models/home-plan-selection.model.ts` — interfaces `HomePlanPackage`, `HomeQuotationRequest`, `HomeQuotationResult`, `HomeDataFormValue`, constante `HOME_COVERAGE_OPTIONS`
- [ ] Crear `core/service/home-plans.service.ts` — métodos `getPackages()` y `calculate(data: HomeQuotationRequest)`
- [ ] Crear `ui/blocks/home-data-form/home-data-form.component.ts/.html/.css` — formulario reactivo: propertyValue, constructionYear, stratum, occupants, propertyType; selector de paquete o coberturas individuales; `@Output() quoteRequested = output<HomeDataFormValue>()`
- [ ] Crear `ui/blocks/home-plan-selector/home-plan-selector.component.ts/.html/.css` — 3 tarjetas de paquete + opción "Personalizado"; emite paquete seleccionado
- [ ] Crear `ui/blocks/home-plan-preview/home-plan-preview.component.ts/.html/.css` — resumen: tipo inmueble, prima mensual, prima anual, coberturas, multiplicadores aplicados
- [ ] Modificar `ui/Pages/policy-create/policy-create.component.ts`:
  - [ ] Añadir signals: `isHomeType`, `homeQuotation`, `homeQuoteLoading`, `homePackageId`
  - [ ] Inyectar `HomePlansService`
  - [ ] Añadir `isHomeType.set(type === 'Home')` en `onTypeChange()`
  - [ ] Añadir rama `type === 'Home'` que limpia validadores en `onTypeChange()`
  - [ ] Implementar `onHomeQuoteRequested(data: HomeDataFormValue)` que llama a `homeSvc.calculate()`
  - [ ] Añadir rama `isHome` en `submit()` para incluir campos de hogar en el payload
  - [ ] Calcular `insuredAmount` como `homeQuote.baseMonthlyPremium` e `monthlyPremium` como `homeQuote.finalMonthlyPremium` para pólizas Home
  - [ ] Importar `HomeDataFormComponent`, `HomePlanSelectorComponent`, `HomePlanPreviewComponent`
- [ ] Modificar `ui/Pages/policy-create/policy-create.component.html`:
  - [ ] Añadir paso "Datos del Inmueble" con directiva `@if (isHomeType())` que incluye `<app-home-data-form>`
  - [ ] Añadir paso "Resumen de Cotización Home" con directiva `@if (isHomeType() && homeQuotation())` que incluye `<app-home-plan-preview>`
  - [ ] Deshabilitar botón "Continuar" en paso de cotización si `!homeQuotation()`

#### Tests Frontend
- [ ] `HomeDataFormComponent renders all form fields` — los 5 campos + selector de cobertura se renderizan
- [ ] `HomeDataFormComponent disables cotizar button when form invalid` — botón deshabilitado con form inválido
- [ ] `HomeDataFormComponent emits quoteRequested when form is valid and button clicked` — emisión correcta
- [ ] `HomeDataFormComponent does not emit when propertyValue is zero or negative` — validación valor
- [ ] `HomeDataFormComponent selecting a package preloads package coverages` — paquete precarga coberturas
- [ ] `HomeDataFormComponent switching packages clears previous selection` — cambio de paquete limpia selección
- [ ] `HomeDataFormComponent selecting 6+ coverages shows multicobertura hint` — hint de descuento visible
- [ ] `HomePlanSelectorComponent renders three package cards plus custom option` — 4 opciones renderizadas
- [ ] `HomePlanSelectorComponent emits selected package on click` — emisión de paquete seleccionado
- [ ] `HomePlanPreviewComponent displays monthly premium correctly` — prima mensual formateada en COP
- [ ] `HomePlanPreviewComponent displays applied multipliers list` — lista de multiplicadores visible
- [ ] `HomePlanPreviewComponent shows annual premium as monthlyPremium * 12` — prima anual calculada
- [ ] `HomePlansService.calculate() calls POST /api/v1/home-plans/calculate with correct body` — payload correcto
- [ ] `HomePlansService.getPackages() calls GET /api/v1/home-plans` — endpoint correcto
- [ ] `PolicyCreateComponent isHomeType signal set to true when Home selected` — signal activada
- [ ] `PolicyCreateComponent homeQuotation populated after successful calculate` — signal poblada tras cotización
- [ ] `PolicyCreateComponent submit includes home fields when isHomeType is true` — payload incluye campos Home

### QA
- [ ] Ejecutar skill `/gherkin-case-generator` → criterios CRITERIO-1.x a CRITERIO-5.x
- [ ] Ejecutar skill `/risk-identifier` → clasificación ASD de riesgos del módulo
- [ ] Verificar tabla de cálculos de la sección 1.6 contra la implementación real del `HomePricingService`
- [ ] Validar que los descuentos de antigüedad y multicobertura son acumulables (RN-09 + RN-04)
- [ ] Validar que el recargo de antigüedad > 30 años aplica SOLO sobre Fontanería y Daños Eléctricos (RN-05), no sobre el total
- [ ] Validar que Incendio y Explosión no genera cargo adicional más allá de la prima base (RN-01)
- [ ] Revisar cobertura de tests contra todos los criterios de aceptación (HU-1 a HU-5)
- [ ] Validar que todas las reglas de negocio RN-01 a RN-18 están cubiertas por al menos un test
- [ ] Probar flujo completo E2E: tipo "Home" → datos asegurado → datos predio + coberturas → cotización → preview → confirmación → póliza creada
- [ ] Verificar que pólizas de otros tipos (Vehicle, Health, Life, Travel) no se ven afectadas y tienen `homePlan = null`
- [ ] Actualizar estado spec a `status: IMPLEMENTED` al completar

---

> **Revisión pendiente antes de pasar a APPROVED:** Confirmar con el equipo actuarial los valores exactos de los rangos de coberturas variables (Robo: 10-15%, se propone usar 12%) y los montos fijos de coberturas de rango (Cristales: $40k-$60k, se propone $50k; RC: $100k-$250k, se escala por estrato; Daños Agua/Peritaje: $50k-$80k, se propone $65k).
