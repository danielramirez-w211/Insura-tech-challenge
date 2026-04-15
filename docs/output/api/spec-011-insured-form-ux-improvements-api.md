# API: SPEC-011 — Mejoras de Calidad de Datos en el Formulario de Póliza

**Fecha:** 2026-04-14
**Spec:** [insured-form-ux-improvements.spec.md](../../../asd/.github/specs/insured-form-ux-improvements.spec.md)
**Estado:** IN_PROGRESS

---

## Resumen de cambios

SPEC-011 extiende el módulo de creación de pólizas (SPEC-010) con:
- Endpoint nuevo para listar ciudades colombianas desde MongoDB.
- Campos nuevos en el payload de `POST /api/v1/policies`: tipo de documento (enum), género, ciudad, código postal, departamento y dirección del asegurado.
- Directiva Angular `ThousandsSeparatorDirective` para formateo de separadores de miles en el frontend (sin impacto en el API — el backend recibe valores crudos).

---

## Endpoints

### GET /api/v1/cities

Lista todas las ciudades disponibles para selección en el formulario de asegurado.

**Auth:** Bearer token requerido

**Response 200:**
```json
[
  { "name": "Bogotá",       "postalCode": "110111", "department": "Cundinamarca" },
  { "name": "Medellín",     "postalCode": "050001", "department": "Antioquia" },
  { "name": "Cali",         "postalCode": "760001", "department": "Valle del Cauca" },
  { "name": "Barranquilla", "postalCode": "080001", "department": "Atlántico" },
  { "name": "Cartagena",    "postalCode": "130001", "department": "Bolívar" }
]
```

**Response 401:** Token ausente o expirado.

**Colección MongoDB:** `cities` (seed con 20 ciudades principales de Colombia)

---

### POST /api/v1/policies (campos nuevos — SPEC-011)

El endpoint existente de creación de póliza recibe campos adicionales en el objeto `insured`:

**Body (campos nuevos en `insured`):**
```json
{
  "insured": {
    "firstName": "Juan",
    "lastName": "Pérez",
    "documentType": "CC",
    "documentId": "1127350242",
    "birthDate": "1990-03-15",
    "email": "juan@example.com",
    "phone": "3001234567",
    "gender": "Masculino",
    "address": "Calle 123 # 45-67",
    "cityName": "Bogotá",
    "postalCode": "110111",
    "department": "Cundinamarca"
  }
}
```

| Campo nuevo       | Tipo   | Obligatorio | Validación                                  |
|-------------------|--------|-------------|---------------------------------------------|
| `documentType`    | string | Sí          | Enum: CC, CE, TI, PP, RC                    |
| `gender`          | string | Sí          | "Masculino" o "Femenino"                    |
| `address`         | string | Sí          | Máximo 200 caracteres                       |
| `cityName`        | string | Sí          | Debe existir en colección `cities`          |
| `postalCode`      | string | Sí          | Resuelto automáticamente por el frontend    |
| `department`      | string | Sí          | Resuelto automáticamente por el frontend    |

> **Nota sobre `documentType`:** El backend espera el código string del enum (ej. `"CC"`, `"TI"`).
> El frontend valida las reglas de entrada por tipo:
> - CC, TI, RC → solo dígitos, máximo 10.
> - CE, PP → alfanumérico, máximo 11 caracteres.

> **Nota sobre `postalCode` y `department`:** El frontend los resuelve al seleccionar la ciudad
> (lookup local contra los datos de `GET /api/v1/cities`) y los incluye en el request.
> El backend los persiste sin re-validar contra la colección `cities`.

**Response 201:** Póliza creada (sin cambios en la estructura de respuesta).

**Response 400:** Validación fallida (campo faltante o inválido).

**Response 401:** Token ausente o expirado.

---

## Modelos de Dominio actualizados

### `InsuredPerson` (Value Object — backend)

```csharp
public class InsuredPerson
{
    public string FirstName    { get; }
    public string LastName     { get; }
    public DocumentType DocumentType { get; }   // enum — nuevo
    public string DocumentId   { get; }
    public DateOnly BirthDate  { get; }
    public string Gender       { get; }         // nuevo
    public string Address      { get; }         // nuevo
    public string CityName     { get; }         // nuevo
    public string PostalCode   { get; }         // nuevo
    public string Department   { get; }         // nuevo
}
```

### `DocumentType` (Enum — backend)

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

Serializado en MongoDB como string (ej. `"CC"`) mediante BSON serializer.

---

## Modelos Frontend

### `InsuredPerson` (interface — frontend)

```typescript
// frontend/src/app/features/policies/core/models/insured-person.model.ts
export type DocumentType = 'CC' | 'CE' | 'TI' | 'PP' | 'RC';

export interface InsuredPerson {
  firstName: string;
  lastName: string;
  documentType: DocumentType;
  documentId: string;
  birthDate: string;
  email: string;
  phone: string;
  gender: 'Masculino' | 'Femenino';
  address: string;
  cityName: string;
  postalCode: string;
  department: string;
}
```

### `CityOption` (interface — frontend)

```typescript
// frontend/src/app/features/policies/core/models/city.model.ts
export interface CityOption {
  name: string;
  postalCode: string;
  department: string;
}
```

---

## Servicios Frontend

### `CitiesService`

```typescript
// frontend/src/app/features/policies/core/service/cities.service.ts
@Injectable({ providedIn: 'root' })
export class CitiesService {
  cities = signal<CityOption[]>([]);

  loadCities(): Observable<CityOption[]>   // GET /api/v1/cities → actualiza signal
}
```

---

## Directiva Angular: `ThousandsSeparatorDirective`

**Selector:** `[appThousandsSeparator]`
**Archivo:** `frontend/src/app/shared/directives/thousands-separator.directive.ts`

Comportamiento:
- Al escribir: elimina no-dígitos, limita a 10 caracteres, formatea con `.` como separador de miles.
- Actualiza el form control con el valor crudo (sin puntos) para que el backend reciba solo dígitos.
- Al perder foco (`blur`): reformatea el valor visible.

**Uso en template (solo para CC, TI, RC):**
```html
<input matInput formControlName="documentId" appThousandsSeparator />
```

Ejemplo: el usuario escribe `1127350242` → el input muestra `1.127.350.242` → el form control tiene `1127350242`.

---

## Reglas de validación condicional (`documentId`)

```typescript
// Implementado en policy-create.component.ts
function documentIdValidatorFn(type: DocumentType): ValidatorFn {
  return (control) => {
    const raw = control.value?.replace(/\./g, '') ?? '';
    if (!raw) return { required: true };
    if (['CC', 'TI', 'RC'].includes(type))
      return /^\d{1,10}$/.test(raw) ? null : { invalidFormat: true };
    if (['CE', 'PP'].includes(type))
      return /^[A-Za-z0-9]{1,11}$/.test(control.value) ? null : { invalidFormat: true };
    return null;
  };
}
```

El validador se actualiza dinámicamente cada vez que cambia `documentType`. Al cambiar de tipo, `documentId` se limpia automáticamente.
