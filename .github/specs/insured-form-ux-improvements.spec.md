---
id: SPEC-011
status: IN_PROGRESS
feature: insured-form-ux-improvements
created: 2026-04-13
updated: 2026-04-13
author: spec-generator
version: "1.0"
related-specs: [SPEC-010]
---

# Spec: Mejoras de Calidad de Datos en el Formulario de Póliza

> **Estado:** `DRAFT` → aprobar con `status: APPROVED` antes de iniciar implementación.
> **Ciclo de vida:** DRAFT → APPROVED → IN_PROGRESS → IMPLEMENTED → DEPRECATED

---

## Resumen Ejecutivo

Este módulo resuelve seis bugs/mejoras de calidad de datos detectados en el formulario de creación de póliza.
Las mejoras abarcan: validación de tipo de documento mediante enum extensible, reglas de entrada por tipo
(numérico vs alfanumérico), formato de miles en campos numéricos, entrada manual en datepickers,
selector de marca de vehículo, y tres nuevos campos del asegurado (Género, Ciudad con código postal y
Departamento desde MongoDB, y Dirección). Los cambios impactan la capa de dominio, la API y el frontend.

---

## 1. REQUERIMIENTOS

### 1.1 Descripción del Feature

Mejorar la calidad de datos y la experiencia de usuario en el paso "Asegurado" del formulario de creación
de póliza (`PolicyCreateComponent`), corrigiendo validaciones incompletas, habilitando formatos numéricos
con separador de miles, permitiendo ingreso manual de fechas y añadiendo tres campos nuevos al modelo
`InsuredPerson`: Género, Ciudad (con código postal y departamento desde una colección MongoDB) y Dirección.

### 1.2 Historias de Usuario

---

#### HU-1: Tipo de Documento como Lista de Opciones (Enum Extensible)

```
Como:        Agente de seguros
Quiero:      Seleccionar el tipo de documento del asegurado desde una lista desplegable
             (CC, CE, TI, PP, RC) en lugar de escribirlo manualmente
Para:        Eliminar errores de digitación y garantizar que solo se ingresen tipos
             de documento válidos y reconocidos por el sistema

Prioridad:   Alta
Estimación:  S
Dependencias: Ninguna
Capa:        Ambas
```

#### Criterios de Aceptación — HU-1

**Happy Path**
```gherkin
CRITERIO-1.1: Selección de tipo de documento desde lista
  Dado que:  el agente está en el paso "Asegurado" del formulario
  Cuando:    hace clic en el campo "Tipo de documento"
  Entonces:  se despliega una lista con exactamente 5 opciones:
             - CC  → Cédula de Ciudadanía
             - CE  → Cédula de Extranjería
             - TI  → Tarjeta de Identidad
             - PP  → Pasaporte
             - RC  → Registro Civil
             y al seleccionar una opción el campo queda con el código (CC, CE, TI, PP o RC)
```

**Error Path**
```gherkin
CRITERIO-1.2: Campo obligatorio — no se puede continuar sin selección
  Dado que:  el agente no ha seleccionado ningún tipo de documento
  Cuando:    intenta avanzar al siguiente paso
  Entonces:  el formulario muestra el error "El tipo de documento es obligatorio"
             y el botón "Continuar" permanece deshabilitado
```

**Edge Case**
```gherkin
CRITERIO-1.3: El cambio de tipo de documento limpia el número de documento
  Dado que:  el agente seleccionó CC e ingresó el número "1.127.350.242"
  Cuando:    cambia el tipo de documento a CE
  Entonces:  el campo número de documento se limpia automáticamente
             y las reglas de validación se actualizan al nuevo tipo (alfanumérico, máx 11)
```

---

#### HU-2: Validación de Número de Documento por Tipo

```
Como:        Sistema de validación del formulario
Quiero:      Aplicar reglas de entrada diferentes según el tipo de documento seleccionado:
             - CC, TI, RC: solo dígitos numéricos, máximo 10 dígitos
             - CE, PP: alfanumérico (letras y números), máximo 11 caracteres
Para:        Prevenir el ingreso de datos inválidos sin necesidad de mostrar mensajes
             de error para restricciones de teclado (el campo simplemente no acepta
             el carácter inválido)

Prioridad:   Alta
Estimación:  M
Dependencias: HU-1
Capa:        Frontend
```

#### Criterios de Aceptación — HU-2

**Happy Path**
```gherkin
CRITERIO-2.1: CC acepta exactamente 10 dígitos numéricos
  Dado que:  el agente seleccionó CC como tipo de documento
  Cuando:    intenta ingresar "1127350242" (10 dígitos)
  Entonces:  el campo acepta los 10 dígitos sin error
             y el número se muestra formateado como "1.127.350.242"
```

**Error Path**
```gherkin
CRITERIO-2.2: CC no acepta letras (bloqueo silencioso en teclado)
  Dado que:  el agente seleccionó CC como tipo de documento
  Cuando:    presiona una tecla de letra (ej. "a", "b")
  Entonces:  el carácter NO aparece en el campo
             y NO se muestra ningún mensaje de error
             (la restricción es a nivel de input, no de validación)
```

**Edge Case**
```gherkin
CRITERIO-2.3: CC con más de 10 dígitos bloquea sin mensaje
  Dado que:  el agente seleccionó CC y ya ingresó 10 dígitos
  Cuando:    intenta ingresar un dígito adicional
  Entonces:  el campo NO acepta el carácter 11
             sin mostrar mensaje de error
```

**Edge Case**
```gherkin
CRITERIO-2.4: PP acepta alfanumérico hasta 11 caracteres
  Dado que:  el agente seleccionó PP como tipo de documento
  Cuando:    ingresa "AB1234567" (9 caracteres alfanuméricos)
  Entonces:  el campo acepta la combinación de letras y números
             hasta un máximo de 11 caracteres
```

---

#### HU-3: Formato de Miles en Campos Numéricos

```
Como:        Agente de seguros
Quiero:      Ver el número de documento y el valor comercial del vehículo
             formateados con punto como separador de miles
             (ej. 1.127.350.242 / $125.000.000)
Para:        Mejorar la legibilidad y reducir errores de digitación en
             cifras grandes

Prioridad:   Media
Estimación:  S
Dependencias: HU-2
Capa:        Frontend
```

#### Criterios de Aceptación — HU-3

**Happy Path**
```gherkin
CRITERIO-3.1: Número de documento CC formateado con puntos de miles
  Dado que:  el agente ingresó "1127350242" en el campo de número de documento (CC)
  Cuando:    el campo pierde el foco o el usuario deja de escribir
  Entonces:  el valor mostrado es "1.127.350.242"
             y el valor enviado al backend es "1127350242" (sin puntos)
```

**Happy Path**
```gherkin
CRITERIO-3.2: Valor comercial del vehículo formateado con puntos de miles
  Dado que:  el agente ingresó "125000000" en el campo valor comercial del vehículo
  Cuando:    el sistema renderiza el valor
  Entonces:  el valor mostrado es "$125.000.000"
             y el valor enviado al backend es 125000000 (número sin formato)
```

---

#### HU-4: Ingreso Manual de Fecha en Datepickers

```
Como:        Agente de seguros
Quiero:      Poder ingresar fechas manualmente en formato DD-MM-YYYY en todos
             los campos de fecha del formulario (fecha de nacimiento, inicio de
             cobertura, fin de cobertura)
Para:        Agilizar el proceso de ingreso de datos sin depender exclusivamente
             del calendario visual

Prioridad:   Media
Estimación:  S
Dependencias: Ninguna
Capa:        Frontend
```

#### Criterios de Aceptación — HU-4

**Happy Path**
```gherkin
CRITERIO-4.1: Ingreso manual de fecha de nacimiento en DD-MM-YYYY
  Dado que:  el agente está en el campo "Fecha de nacimiento"
  Cuando:    escribe "15-03-1990" manualmente
  Entonces:  el sistema interpreta la fecha como 15 de marzo de 1990
             y la muestra formateada según el locale del datepicker
             y el formulario la valida correctamente
```

**Error Path**
```gherkin
CRITERIO-4.2: Fecha manual inválida muestra error
  Dado que:  el agente está en el campo "Fecha de nacimiento"
  Cuando:    escribe "32-13-1990" (día y mes inválidos)
  Entonces:  el datepicker muestra un error de formato
             y el campo se marca como inválido
```

**Edge Case**
```gherkin
CRITERIO-4.3: Fecha de inicio de cobertura ingresada manualmente no puede ser pasada
  Dado que:  el agente escribe manualmente una fecha anterior a hoy en "Inicio de cobertura"
  Cuando:    el campo pierde el foco
  Entonces:  el sistema muestra el error "La fecha de inicio no puede ser anterior a hoy"
```

---

#### HU-5: Marca de Vehículo como Lista Desplegable

```
Como:        Agente de seguros
Quiero:      Seleccionar la marca del vehículo desde una lista predefinida de 11 opciones
             en lugar de escribirla libremente
Para:        Garantizar consistencia en los datos de marca y evitar variantes
             ortográficas que afecten el cálculo de recargo por marca

Prioridad:   Alta
Estimación:  XS
Dependencias: SPEC-010 (módulo de cotización de vehículo)
Capa:        Frontend
```

#### Criterios de Aceptación — HU-5

**Happy Path**
```gherkin
CRITERIO-5.1: Lista de 11 marcas disponibles en el selector
  Dado que:  el agente está en el paso "Datos del Vehículo"
  Cuando:    hace clic en el campo "Marca del vehículo"
  Entonces:  se despliega una lista con exactamente estas 11 marcas (orden alfabético):
             BMW, BYD, Chevrolet, Ford, Honda, Hyundai, Jeep, Nissan, Renault, Subaru, Toyota
```

**Error Path**
```gherkin
CRITERIO-5.2: No se puede avanzar sin seleccionar marca
  Dado que:  el agente no ha seleccionado la marca del vehículo
  Cuando:    intenta hacer clic en "Cotizar"
  Entonces:  el formulario muestra el error "La marca es obligatoria"
             y el botón "Cotizar" permanece deshabilitado
```

---

#### HU-6: Nuevos Campos del Asegurado (Género, Ciudad, Dirección)

```
Como:        Sistema de gestión de pólizas
Quiero:      Registrar el género, ciudad de residencia (con código postal y departamento
             desde la base de datos) y dirección del asegurado al crear una póliza
Para:        Contar con información demográfica completa que soporte análisis actuariales
             futuros y requerimientos regulatorios de ubicación

Prioridad:   Alta
Estimación:  L
Dependencias: HU-1, HU-2
Capa:        Ambas
```

#### Criterios de Aceptación — HU-6

**Happy Path**
```gherkin
CRITERIO-6.1: Selección de género desde lista desplegable
  Dado que:  el agente está en el paso "Asegurado"
  Cuando:    hace clic en el campo "Género"
  Entonces:  se despliega una lista con dos opciones:
             - Masculino
             - Femenino
             y al seleccionar una, el campo queda registrado
```

**Happy Path**
```gherkin
CRITERIO-6.2: Ciudad seleccionada recupera automáticamente código postal y departamento
  Dado que:  el agente selecciona "Bogotá" en el campo "Ciudad"
  Cuando:    confirma la selección
  Entonces:  el sistema guarda en la póliza:
             - ciudad: "Bogotá"
             - codigoPostal: "110111"
             - departamento: "Cundinamarca"
             sin que el agente deba ingresar estos datos manualmente
```

**Error Path**
```gherkin
CRITERIO-6.3: Los tres campos nuevos son obligatorios
  Dado que:  el agente no ha completado Género, Ciudad o Dirección
  Cuando:    intenta avanzar al siguiente paso
  Entonces:  el formulario muestra el error correspondiente para cada campo vacío:
             "El género es obligatorio" / "La ciudad es obligatoria" / "La dirección es obligatoria"
             y el botón "Continuar" permanece deshabilitado
```

**Edge Case**
```gherkin
CRITERIO-6.4: Lista de ciudades disponible desde el primer render del formulario
  Dado que:  el agente llega al paso "Asegurado"
  Cuando:    el componente se inicializa
  Entonces:  el servicio de ciudades carga la lista completa desde el backend
             y el campo Ciudad está listo para recibir selección sin espera adicional
```

---

### 1.3 Reglas de Negocio

| ID | Regla | Tipo |
|----|-------|------|
| RN-01 | `DocumentType` es un enum con valores: CC, CE, TI, PP, RC. Nuevos tipos se añaden al enum en código + re-deploy (no en runtime). | Invariante |
| RN-02 | CC, TI, RC: solo acepta caracteres `[0-9]`, máximo 10 dígitos. Restricción a nivel de input (sin mensaje de error por tecla inválida). | Validación |
| RN-03 | CE, PP: acepta caracteres `[A-Za-z0-9]`, máximo 11 caracteres. | Validación |
| RN-04 | El número de documento CC se muestra con punto como separador de miles en el formulario, pero se persiste y envía al backend sin formato (solo dígitos). | Presentación |
| RN-05 | Todos los datepickers deben aceptar ingreso manual en formato `DD-MM-YYYY`. Si la fecha ingresada es inválida, el campo se marca con error. | Validación |
| RN-06 | Las marcas de vehículo válidas son exactamente 11 (catálogo fijo en frontend): BMW, BYD, Chevrolet, Ford, Honda, Hyundai, Jeep, Nissan, Renault, Subaru, Toyota. | Catálogo |
| RN-07 | Al cambiar el tipo de documento, el número de documento debe limpiarse automáticamente. | UX |
| RN-08 | Género acepta dos valores: `Masculino` y `Femenino`. Se persiste como string en MongoDB. | Dominio |
| RN-09 | La Ciudad se selecciona de una lista proveniente de la colección `cities` de MongoDB. La API devuelve: `{ name, postalCode, department }`. Solo `name` aparece visible al agente; `postalCode` y `department` se persisten automáticamente. | Dominio |
| RN-10 | La Dirección es texto libre, obligatorio, máximo 200 caracteres. | Validación |
| RN-11 | El campo `documentType` en el dominio backend (`InsuredPerson`) permanece como `string` en MongoDB, mapeado desde el enum `DocumentType`. Esto garantiza legibilidad sin romper el esquema existente. | Arquitectura |

### 1.4 Propuesta de Extensibilidad para DocumentType

**Decisión:** Enum cerrado en C# (`DocumentType` en `InsuraTech.Domain`) serializado como string en MongoDB.

**Justificación:** Los tipos de documento son regulados por ley y rara vez cambian. Una adición requiere:
1. Añadir el valor al enum `DocumentType`.
2. Añadir la opción al dropdown en el frontend (`DocumentTypeOption[]` en el modelo).
3. Re-deploy — no requiere migración de datos porque los documentos existentes siguen siendo válidos.

**Alternativa descartada:** Colección MongoDB `document_types` — añade complejidad operativa (seed, API, cache)
sin beneficio real dado que los cambios en tipos de documento siempre requieren validación legal y código.

---

## 2. DISEÑO

### 2.1 Modelos de Datos

#### Entidades afectadas

| Entidad | Almacén | Cambios | Descripción |
|---------|---------|---------|-------------|
| `InsuredPerson` | Value Object (Dominio) | Modificada | Añadir `Gender`, `Address`, `CityName`, `PostalCode`, `Department` |
| `City` | MongoDB `cities` | Nueva | Colección seeded con ciudades de Colombia |
| `CreatePolicyCommand` | CQRS Command | Modificada | Añadir `InsuredGender`, `InsuredAddress`, `InsuredCity` |
| `PolicyResponse` / DTO | DTO | Modificada | Exponer los nuevos campos en la respuesta |
| `DocumentType` | Enum (Dominio) | Nueva | Enum extensible CC, CE, TI, PP, RC |

#### Modelo `InsuredPerson` (actualizado)

| Campo | Tipo C# | Obligatorio | Validación |
|-------|---------|-------------|------------|
| `FirstName` | `string` | Sí | No vacío |
| `LastName` | `string` | Sí | No vacío |
| `DocumentType` | `DocumentType` (enum) | Sí | Enum válido |
| `DocumentId` | `string` | Sí | CC/TI/RC: `[0-9]{1,10}` · CE/PP: `[A-Za-z0-9]{1,11}` |
| `BirthDate` | `DateOnly` | Sí | Pasado, entre 18 y 100 años |
| `Gender` | `string` | Sí | "Masculino" o "Femenino" |
| `Address` | `string` | Sí | Max 200 chars |
| `CityName` | `string` | Sí | Debe existir en colección `cities` |
| `PostalCode` | `string` | Sí | Auto-resuelto desde `cities` |
| `Department` | `string` | Sí | Auto-resuelto desde `cities` |

#### Enum `DocumentType`

```csharp
public enum DocumentType
{
    CC = 1,   // Cédula de Ciudadanía
    CE = 2,   // Cédula de Extranjería
    TI = 3,   // Tarjeta de Identidad
    PP = 4,   // Pasaporte
    RC = 5    // Registro Civil
}
```

Serializado en MongoDB como string (ej. `"CC"`) mediante configuración del serializer BSON.

#### Colección MongoDB `cities`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `_id` | ObjectId | Auto-generado por MongoDB |
| `name` | string | Nombre de la ciudad (ej. "Bogotá") |
| `postalCode` | string | Código postal (ej. "110111") |
| `department` | string | Departamento (ej. "Cundinamarca") |

Seed inicial (mínimo 20 ciudades principales de Colombia): Bogotá, Medellín, Cali, Barranquilla,
Cartagena, Cúcuta, Bucaramanga, Pereira, Santa Marta, Ibagué, Manizales, Pasto, Neiva, Villavicencio,
Montería, Sincelejo, Valledupar, Armenia, Popayán, Tunja.

---

### 2.2 API Endpoints

#### GET /api/v1/cities
- **Descripción**: Lista todas las ciudades disponibles para selección
- **Auth requerida**: sí (Bearer token)
- **Response 200**:
  ```json
  [
    { "name": "Bogotá", "postalCode": "110111", "department": "Cundinamarca" },
    { "name": "Medellín", "postalCode": "050001", "department": "Antioquia" }
  ]
  ```
- **Response 401**: token ausente o expirado

#### PUT /api/v1/policies (CreatePolicyCommand — campos añadidos)

Campos nuevos en el body del comando existente `POST /api/v1/policies`:

```json
{
  "insuredDocumentType": "CC",
  "insuredGender": "Masculino",
  "insuredAddress": "Calle 123 # 45-67",
  "insuredCityName": "Bogotá",
  "insuredPostalCode": "110111",
  "insuredDepartment": "Cundinamarca"
}
```

> **Nota**: `insuredPostalCode` e `insuredDepartment` los resuelve el **frontend** al seleccionar la ciudad,
> y los incluye en el request. El backend no hace lookup de la ciudad — solo persiste lo recibido.
> Esto evita una dependencia de la colección `cities` en el handler de creación de póliza.

---

### 2.3 Diseño Frontend

#### Componentes modificados

| Componente | Archivo | Cambios |
|------------|---------|---------|
| `PolicyCreateComponent` | `policy-create.component.ts/html` | Añadir campos Gender, City, Address al `insuredForm`; lógica de cities service |
| `VehicleDataFormComponent` | `vehicle-data-form.component.ts/html` | Cambiar input de brand a mat-select |

#### Componentes nuevos / directivas

| Artefacto | Archivo | Responsabilidad |
|-----------|---------|-----------------|
| `ThousandsSeparatorDirective` | `shared/directives/thousands-separator.directive.ts` | Formatear con punto como separador de miles al escribir / al blur |
| `CityOption` (interface) | `core/models/city.model.ts` | `{ name, postalCode, department }` |
| `CitiesService` | `core/service/cities.service.ts` | `GET /api/v1/cities` → `Signal<CityOption[]>` |

#### Cambios en `insuredForm` (reactive form)

| Control nuevo | Tipo | Validadores |
|---------------|------|-------------|
| `documentType` | `mat-select` (reemplaza input) | `Validators.required` |
| `documentId` | `mat-input` (con directive) | `required` + validador condicional por tipo |
| `gender` | `mat-select` | `Validators.required` |
| `city` | `mat-select` (lazy loaded) | `Validators.required` |
| `address` | `mat-input` | `required`, `maxLength(200)` |

#### Vehicle brand (VehicleDataFormComponent)

El catálogo de marcas es una constante en el componente:

```typescript
readonly VEHICLE_BRANDS = [
  'BMW', 'BYD', 'Chevrolet', 'Ford', 'Honda',
  'Hyundai', 'Jeep', 'Nissan', 'Renault', 'Subaru', 'Toyota'
] as const;
```

#### Datepickers — configuración para ingreso manual

Eliminar el atributo `readonly` de todos los inputs de fecha. Angular Material Datepicker
interpreta la entrada manual cuando el input no es readonly. El formato se configura via
`MAT_DATE_FORMATS` con `DD-MM-YYYY`. El validador `pastDate` existente sigue aplicando.

#### Modelo `InsuredPerson` (frontend — actualizado)

```typescript
// insured-person.model.ts
export interface InsuredPerson {
  firstName: string;
  lastName: string;
  documentType: DocumentType;  // enum
  documentId: string;
  birthDate: string;
  email: string;
  phone?: string;
  gender: 'Masculino' | 'Femenino';
  address: string;
  cityName: string;
  postalCode: string;
  department: string;
}

export type DocumentType = 'CC' | 'CE' | 'TI' | 'PP' | 'RC';

export interface DocumentTypeOption {
  code: DocumentType;
  label: string;
}

export const DOCUMENT_TYPE_OPTIONS: DocumentTypeOption[] = [
  { code: 'CC', label: 'Cédula de Ciudadanía' },
  { code: 'CE', label: 'Cédula de Extranjería' },
  { code: 'TI', label: 'Tarjeta de Identidad' },
  { code: 'PP', label: 'Pasaporte' },
  { code: 'RC', label: 'Registro Civil' },
];
```

#### Validación condicional de `documentId`

```typescript
// Validator dinámico según documentType seleccionado
function documentIdValidator(type: DocumentType): ValidatorFn {
  return (control) => {
    const value = control.value as string;
    if (!value) return { required: true };
    if (['CC', 'TI', 'RC'].includes(type)) {
      return /^\d{1,10}$/.test(value) ? null : { invalidFormat: true };
    }
    if (['CE', 'PP'].includes(type)) {
      return /^[A-Za-z0-9]{1,11}$/.test(value) ? null : { invalidFormat: true };
    }
    return null;
  };
}
```

La directiva `ThousandsSeparatorDirective` solo se aplica a `documentId` cuando el tipo es CC, TI o RC.

### 2.4 Arquitectura y Dependencias

- **Paquetes nuevos**: ninguno — Angular Material Datepicker ya soporta entrada manual sin readonly.
- **Seeding MongoDB**: Script de seed para colección `cities` (archivo separado).
- **Impacto en dominio**: `InsuredPerson.Create()` recibe 4 parámetros adicionales; todos los tests
  que construyen `InsuredPerson` deben actualizarse con los nuevos argumentos.
- **Riesgo de breaking change**: `CreatePolicyCommand` añade campos obligatorios — el frontend
  debe enviarlos siempre. El handler de creación de póliza debe validarlos.

---

## 3. LISTA DE TAREAS

### Backend

#### Implementación
- [ ] Crear enum `DocumentType` en `InsuraTech.Domain/Policies/Enums/DocumentType.cs`
- [ ] Actualizar `InsuredPerson` Value Object: añadir `Gender`, `Address`, `CityName`, `PostalCode`, `Department`
- [ ] Actualizar `InsuredPerson.Create()` con validación de `DocumentType` enum y reglas por tipo
- [ ] Crear colección MongoDB `cities` + script de seed con 20 ciudades principales de Colombia
- [ ] Crear `CityDocument` en `InsuraTech.Infrastructure/MongoDB/Documents/`
- [ ] Crear `ICityRepository` + `CityRepository` (solo `GetAll()`)
- [ ] Crear `GetCitiesQuery` + `GetCitiesHandler` en Application layer
- [ ] Crear `CityResponse` DTO (`{ name, postalCode, department }`)
- [ ] Crear `CitiesController` → `GET /api/v1/cities`
- [ ] Actualizar `CreatePolicyCommand` con campos: `InsuredDocumentType`, `InsuredGender`, `InsuredAddress`, `InsuredCityName`, `InsuredPostalCode`, `InsuredDepartment`
- [ ] Actualizar `CreatePolicyValidator` con validaciones de DocumentType y nuevos campos
- [ ] Actualizar todos los handlers (`CreatePolicyHandler`, `CreateVehiclePolicyHandler`, etc.) para pasar nuevos campos a `InsuredPerson.Create()`
- [ ] Actualizar `PolicyResponse` / `InsuredPersonDto` para exponer los nuevos campos
- [ ] Actualizar `PolicyMappingExtensions` con el mapeo de los nuevos campos

#### Tests Backend
- [ ] `InsuredPersonTests` — actualizar con nuevos campos obligatorios
- [ ] `InsuredPersonTests.Create_WithInvalidDocumentType_Throws`
- [ ] `InsuredPersonTests.Create_CC_WithLetters_Throws`
- [ ] `InsuredPersonTests.Create_CC_ExceedingMaxLength_Throws`
- [ ] `InsuredPersonTests.Create_CE_WithAlphanumeric_Succeeds`
- [ ] `GetCitiesHandlerTests.Handle_ReturnsAllCities`
- [ ] `CitiesControllerTests.GetCities_Returns200`

### Frontend

#### Implementación
- [ ] Añadir `DocumentType`, `DocumentTypeOption`, `DOCUMENT_TYPE_OPTIONS` a `insured-person.model.ts`
- [ ] Crear `city.model.ts` con `CityOption` interface
- [ ] Crear `cities.service.ts` → `GET /api/v1/cities`, signal `cities()`
- [ ] Crear `ThousandsSeparatorDirective` en `shared/directives/`
- [ ] Registrar directiva en `shared/` y exportar
- [ ] Actualizar `policy-create.component.ts`:
  - [ ] Cambiar `docuemntType` input → `mat-select` con `DOCUMENT_TYPE_OPTIONS`
  - [ ] Añadir lógica de validación condicional en `documentId` según `documentType`
  - [ ] Limpiar `documentId` al cambiar `documentType`
  - [ ] Añadir controles al `insuredForm`: `gender`, `city`, `address`
  - [ ] Inyectar `CitiesService` y cargar ciudades en `ngOnInit`
  - [ ] Al seleccionar ciudad, resolver `postalCode` y `department` automáticamente
  - [ ] Incluir `insuredPostalCode`, `insuredDepartment` en el payload del comando
- [ ] Actualizar `policy-create.component.html`:
  - [ ] Reemplazar input de `docuemntType` por `mat-select`
  - [ ] Añadir `ThousandsSeparatorDirective` a `documentId` (solo para CC/TI/RC)
  - [ ] Eliminar `readonly` de todos los datepickers
  - [ ] Añadir campos Gender (mat-select), City (mat-select), Address (mat-input)
- [ ] Actualizar `VehicleDataFormComponent`: cambiar `brand` de `mat-input` a `mat-select` con `VEHICLE_BRANDS`
- [ ] Configurar `MAT_DATE_FORMATS` para `DD-MM-YYYY` en `app.config.ts`

#### Tests Frontend
- [ ] `cities.service.spec.ts` — GET /api/v1/cities, signal cities()
- [ ] `thousands-separator.directive.spec.ts` — formateo de 1127350242 → 1.127.350.242
- [ ] `policy-create.component.spec` — documentType mat-select renderiza 5 opciones
- [ ] `policy-create.component.spec` — cambiar tipo de doc limpia documentId
- [ ] `policy-create.component.spec` — CC bloquea letras (directiva)
- [ ] `policy-create.component.spec` — gender mat-select renderiza Masculino/Femenino
- [ ] `policy-create.component.spec` — selección de ciudad resuelve postalCode y department
- [ ] `vehicle-data-form.component.spec` — brand mat-select renderiza 11 marcas

### QA
- [ ] Ejecutar skill `/gherkin-case-generator` → criterios CRITERIO-1.x a CRITERIO-6.x
- [ ] Ejecutar skill `/risk-identifier` → clasificación ASD de riesgos
- [ ] Revisar cobertura de tests contra criterios de aceptación
- [ ] Validar que todas las reglas de negocio están cubiertas
- [ ] Actualizar estado spec: `status: IMPLEMENTED`
