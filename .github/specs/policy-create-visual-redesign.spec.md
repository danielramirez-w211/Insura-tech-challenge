---
id: SPEC-007
status: IMPLEMENTED
feature: policy-create-visual-redesign
created: 2026-04-09
updated: 2026-04-09
author: spec-generator
version: "1.0"
related-specs: ["SPEC-005", "SPEC-006"]
---

# Spec: Rediseño Estético y Funcional del Flujo de Contratación de Pólizas

> **Estado:** `DRAFT` → aprobar con `status: APPROVED` antes de iniciar implementación.
> **Ciclo de vida:** DRAFT → APPROVED → IN_PROGRESS → IMPLEMENTED → DEPRECATED

---

## Resumen Ejecutivo

El flujo actual de creación de pólizas en `PolicyCreateComponent` usa un `mat-stepper` de 3 pasos con selección de tipo mediante un `mat-select` dropdown genérico y una disposición de formulario en cuadrícula de 2 columnas. Esta spec introduce un rediseño visual completo que transforma la selección de tipo en tarjetas visuales interactivas (con imágenes reales desde `frontend/src/assets/Policy_images/`), añade un selector de destino geográfico con cards para seguros de viaje, y convierte el formulario de cotización a un diseño vertical de columna única de alta legibilidad.

**No hay cambios en la lógica de negocio, los modelos de datos ni los endpoints de la API.** Todos los servicios (`PoliciesCoreService`, `HealthPlansService`, `TravelPlansService`) y el estado de señales permanecen intactos.

---

## Documentación de Cambios (Actual → Propuesto)

### Elementos deprecados o reemplazados

| Elemento actual | Ubicación | Reemplazado por | Impacto |
|----------------|-----------|-----------------|---------|
| `mat-select` para tipo de póliza | `policy-create.component.html` paso 2 | `PolicyTypeSelectorComponent` — grid de cards visuales | Eliminar `mat-select[formControlName="type"]` del formulario; el tipo se selecciona en un paso dedicado |
| `mat-select` para continente | `policy-create.component.html` paso 2 | `ContinentSelectorComponent` — grid de cards con imagen | Eliminar `mat-select[data-testid="continent-select"]`; la selección es visual |
| Grid de 2 columnas `.form-grid` | `policy-create.component.css` | Layout vertical de 1 columna con `max-width: 480px` | Actualizar `.form-grid` a columna única |
| Paso 1 "Asegurado" como primer paso visible | `mat-stepper` | Se añade nuevo **Paso 0: Tipo de Póliza** antes del paso Asegurado | El stepper pasa de 3 a 4 pasos |

### Componentes impactados

| Componente | Tipo de cambio | Archivo |
|-----------|---------------|---------|
| `PolicyCreateComponent` | Modificado — integra nuevos componentes, añade paso 0 al stepper, cambia layout | `ui/Pages/policy-create/policy-create.component.{ts,html,css}` |
| `PolicyTypeSelectorComponent` | **Nuevo** | `ui/blocks/policy-type-selector/policy-type-selector.component.{ts,html,css}` |
| `PolicyTypeCardComponent` | **Nuevo** | `ui/blocks/policy-type-card/policy-type-card.component.{ts,html,css}` |
| `ContinentSelectorComponent` | **Nuevo** | `ui/blocks/continent-selector/continent-selector.component.{ts,html,css}` |
| `ContinentCardComponent` | **Nuevo** | `ui/blocks/continent-card/continent-card.component.{ts,html,css}` |

### Estructura de pasos del stepper (nuevo flujo)

```
Paso 0 (NUEVO) → Paso 1 → Paso 2 → Paso 3
Tipo de Póliza    Asegurado  Cobertura  Confirmación
```

---

## 1. REQUERIMIENTOS

### Descripción

Rediseño visual del flujo de creación de pólizas que introduce selección de tipo mediante tarjetas gráficas, un selector geográfico visual para pólizas de viaje y un formulario de datos verticalmente organizado. El objetivo es aumentar la intuitividad del proceso de contratación y alinear la experiencia con los estándares visuales del producto (paleta Blanco/Negro/Naranja de SPEC-006).

### Requerimiento de Negocio

El equipo comercial ha identificado que la tasa de abandono del formulario de creación de pólizas es alta en el paso de selección de tipo. La causa principal es que el dropdown genérico no comunica visualmente las diferencias entre los productos. Se requiere una interfaz que guíe visualmente al usuario desde el primer paso, usando imágenes de producto reales ya disponibles en `frontend/src/assets/Policy_images/`.

### Reglas de Negocio

1. La lógica de validación y cálculo existente (Health plan selector, Travel calculation) **no se altera**.
2. Las imágenes usadas en las cards provienen **exclusivamente** del directorio `frontend/src/assets/Policy_images/`.
3. Los tipos Life, Vehicle y Home no tienen imágenes en assets — usan íconos de Material Icons.
4. La selección de tipo en el nuevo Paso 0 establece el valor del campo `coverageForm.type` antes de avanzar al Paso 1.
5. El formulario de datos del asegurado y cobertura mantiene todas las validaciones existentes.
6. El selector de continente visual reemplaza al `mat-select` pero llama al mismo handler `onContinentChange(continent)`.

---

### Historias de Usuario

---

#### HU-01: Selección Visual de Tipo de Póliza mediante Cards

```
Como:        Solicitante de seguro (usuario final)
Quiero:      seleccionar el tipo de póliza haciendo clic en una tarjeta visual
             con imagen y descripción corta, en lugar de un dropdown
Para:        identificar de forma intuitiva qué tipo de seguro se adapta a mi
             necesidad sin conocimiento previo de terminología técnica

Prioridad:   Alta
Estimación:  M
Dependencias: Ninguna
Capa:        Frontend
```

#### Criterios de Aceptación — HU-01

**Happy Path**
```gherkin
CRITERIO-1.1: Visualización de cards de tipo de póliza
  Dado que:  el usuario llega a /policies/new
  Cuando:    se renderiza el Paso 0 "Tipo de Póliza"
  Entonces:  se muestran 5 cards en una cuadrícula responsiva (2 cols en mobile, 3 en desktop)
  Y:         las cards de Salud y Viajes muestran imagen desde assets/Policy_images/
             (Salud.jpg y viaje.jpg respectivamente)
  Y:         las cards de Vida, Vehículo y Hogar muestran íconos de Material Icons
             (favorite, directions_car, home) sobre fondo gris neutro
  Y:         cada card incluye: imagen o ícono, nombre del tipo, descripción corta de 1 línea
```

```gherkin
CRITERIO-1.2: Selección de card y avance al siguiente paso
  Dado que:  el usuario visualiza las cards de tipo de póliza
  Cuando:    hace clic en una card
  Entonces:  la card seleccionada se resalta con borde naranja (#FF6B2C) y sombra
  Y:         el botón "Continuar" se habilita
  Cuando:    hace clic en "Continuar"
  Entonces:  el valor `coverageForm.type` se establece con el tipo seleccionado
  Y:         el stepper avanza al Paso 1 "Asegurado"
```

**Error Path**
```gherkin
CRITERIO-1.3: Intento de avanzar sin selección
  Dado que:  el Paso 0 está activo y ninguna card ha sido seleccionada
  Cuando:    el usuario intenta avanzar (botón "Continuar" inhabilitado)
  Entonces:  el botón "Continuar" permanece deshabilitado
  Y:         se muestra el hint "Selecciona un tipo de póliza para continuar"
```

**Edge Case**
```gherkin
CRITERIO-1.4: Re-selección de tipo
  Dado que:  el usuario seleccionó "Salud" y avanzó a pasos posteriores
  Cuando:    regresa al Paso 0 y selecciona "Viajes"
  Entonces:  los datos específicos de Health (selectedPlanId, healthCalculation, ageRestricted)
             se limpian automáticamente
  Y:         la nueva selección "Viajes" queda resaltada
```

#### Notas de Diseño — HU-01

- **Rutas de imagen:**
  - Salud: `assets/Policy_images/Salud.jpg`
  - Viajes: `assets/Policy_images/viaje.jpg`
- **Dimensiones card:** 200×240px mínimo; imagen cubre el 60% superior de la card con `object-fit: cover`
- **Estado hover:** escala ligera `transform: scale(1.03)` + sombra `box-shadow: 0 4px 16px rgba(255,107,44,0.2)`
- **Estado seleccionado:** `border: 2px solid #FF6B2C` + `box-shadow: 0 0 0 4px rgba(255,107,44,0.15)`
- **Estado no seleccionado:** `border: 1px solid #E0E0E0`

---

#### HU-02: Selector Visual de Destino para Pólizas de Viaje

```
Como:        Solicitante de seguro de viaje
Quiero:      seleccionar mi destino geográfico mediante cards visuales con imagen del continente
Para:        entender claramente las opciones de destino y completar el formulario
             de forma más rápida y sin ambigüedad

Prioridad:   Alta
Estimación:  S
Dependencias: HU-01 (tipo "Viajes" debe estar seleccionado)
Capa:        Frontend
```

#### Criterios de Aceptación — HU-02

**Happy Path**
```gherkin
CRITERIO-2.1: Mostrar selector de destino tras seleccionar tipo "Viajes"
  Dado que:  el usuario seleccionó la card "Viajes" en el Paso 0
  Y:         completó el Paso 1 (datos del asegurado)
  Cuando:    llega al Paso 2 "Cobertura"
  Entonces:  en lugar del mat-select de continente, se muestran cards de destino
  Y:         si el viaje es "Nacional" se muestra la card Colombia
             con imagen `assets/Policy_images/Colombia.png`
  Y:         si el viaje es "Internacional" se muestran 4 cards de continente:
             América (America.jpg), Europa (Europa.jpg),
             África (Africa.jpg), Asia (Asia.jpg)
```

```gherkin
CRITERIO-2.2: Selección de destino activa el cálculo de prima
  Dado que:  el usuario visualiza las cards de destino Internacional
  Cuando:    selecciona la card "Europa"
  Entonces:  la card "Europa" queda resaltada con borde naranja
  Y:         se invoca `onContinentChange('Europe')`
  Y:         el cálculo de prima de viaje se ejecuta automáticamente
  Y:         se muestra el componente `TravelPlanPreviewComponent` con los montos
```

```gherkin
CRITERIO-2.3: Selección Nacional no requiere continente
  Dado que:  el usuario seleccionó "Nacional" como tipo de viaje
  Cuando:    se muestra el selector de destino
  Entonces:  se muestra únicamente la card de Colombia
  Y:         la card queda auto-seleccionada al renderizar
  Y:         el cálculo de prima se ejecuta sin interacción adicional
```

**Error Path**
```gherkin
CRITERIO-2.4: No avanzar sin selección de destino Internacional
  Dado que:  el tipo de viaje es "Internacional" y no se ha seleccionado continente
  Cuando:    el usuario intenta avanzar al paso de Confirmación
  Entonces:  el botón "Continuar" permanece deshabilitado
  Y:         se muestra el hint "Selecciona el continente de destino"
```

**Edge Case**
```gherkin
CRITERIO-2.5: Cambio de tipo de viaje limpia selección de continente
  Dado que:  el usuario seleccionó "Internacional" → "Europa"
  Cuando:    cambia el tipo de viaje a "Nacional"
  Entonces:  la selección de continente se limpia (`continent.set(null)`)
  Y:         se muestra únicamente la card de Colombia
  Y:         el cálculo de prima se reinicia
```

#### Notas de Diseño — HU-02

- **Rutas de imagen por destino:**

| Destino | Valor enum `Continent` | Archivo |
|---------|----------------------|---------|
| Colombia (Nacional) | `null` | `assets/Policy_images/Colombia.png` |
| América | `'America'` | `assets/Policy_images/America.jpg` |
| Europa | `'Europe'` | `assets/Policy_images/Europa.jpg` |
| África | `'Africa'` | `assets/Policy_images/Africa.jpg` |
| Asia | `'Asia'` | `assets/Policy_images/Asia.jpg` |

- **Layout:** cuadrícula de 2×2 para Internacional; card única centrada para Nacional
- **Dimensiones card:** 160×180px; imagen ocupa 65% superior con `object-fit: cover`
- Oceanía **no cuenta** con imagen en assets — omitir del selector visual (o usar ícono placeholder)

---

#### HU-03: Formulario de Cotización con Layout Vertical Moderno

```
Como:        Solicitante de seguro
Quiero:      completar los datos del asegurado y cobertura en un formulario de
             columna única, con campos bien espaciados y etiquetas claras
Para:        reducir el esfuerzo cognitivo de completar el formulario y disminuir
             los errores de entrada en dispositivos móviles y de escritorio

Prioridad:   Alta
Estimación:  S
Dependencias: HU-01
Capa:        Frontend
```

#### Criterios de Aceptación — HU-03

**Happy Path**
```gherkin
CRITERIO-3.1: Formulario de asegurado en columna única centrada
  Dado que:  el usuario llega al Paso 1 "Asegurado"
  Cuando:    se renderiza el formulario
  Entonces:  los campos se disponen en una sola columna vertical
  Y:         el contenedor del formulario tiene max-width: 520px y está centrado
  Y:         cada campo ocupa el 100% del ancho del contenedor
  Y:         el espaciado vertical entre campos es de 16px
```

```gherkin
CRITERIO-3.2: Validaciones visibles sin cambiar lógica existente
  Dado que:  el formulario vertical está activo
  Cuando:    el usuario deja un campo obligatorio vacío y lo abandona
  Entonces:  el mensaje de error aparece debajo del campo (comportamiento de mat-error existente)
  Y:         el botón "Continuar" permanece deshabilitado hasta que todos los campos requeridos sean válidos
```

```gherkin
CRITERIO-3.3: Campos de cobertura también en columna única
  Dado que:  el usuario avanza al Paso 2 "Cobertura"
  Cuando:    se renderiza el formulario de cobertura
  Entonces:  los campos de fecha, monto y prima se disponen en columna única
  Y:         el tipo de póliza YA está establecido (no se muestra el mat-select de tipo)
  Y:         el tipo seleccionado se muestra como badge de solo lectura en la parte superior
```

**Edge Case**
```gherkin
CRITERIO-3.4: Responsividad en pantallas pequeñas
  Dado que:  el usuario accede desde un dispositivo con ancho < 480px
  Cuando:    visualiza cualquier paso del formulario
  Entonces:  los campos ocupan el 100% del ancho disponible sin overflow horizontal
  Y:         las cards de tipo/destino se reorganizan en 1 columna
```

#### Notas de Diseño — HU-03

- **Eliminar** `.form-grid { grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)) }` del CSS
- **Reemplazar** con:
  ```css
  .form-vertical {
    display: flex;
    flex-direction: column;
    gap: 16px;
    max-width: 520px;
    margin: 0 auto;
  }
  .form-vertical mat-form-field { width: 100%; }
  ```
- **Badge de tipo seleccionado** en Paso 2: `<span class="selected-type-badge">Viajes</span>` — estilo naranja, solo lectura, con ícono del tipo

---

## 2. DISEÑO

### Modelos de Datos

No se modifican modelos ni interfaces. Los tipos `PolicyType`, `TripType`, `Continent` y los modelos de señales en `PolicyCreateComponent` permanecen sin cambios.

### API Endpoints

No se crean ni modifican endpoints. Los servicios existentes se usan sin cambios:
- `HealthPlansService.getPlans()` y `.calculate()`
- `TravelPlansService.calculate()`
- `PoliciesCoreService.create()`

### Diseño Frontend

#### Nuevos componentes

| Componente | Ruta | Inputs | Outputs | Descripción |
|-----------|------|--------|---------|-------------|
| `PolicyTypeCardComponent` | `ui/blocks/policy-type-card/` | `type: PolicyType`, `selected: boolean`, `imageSrc?: string`, `icon?: string`, `label: string`, `description: string` | `cardClick: EventEmitter<PolicyType>` | Card individual de tipo de póliza |
| `PolicyTypeSelectorComponent` | `ui/blocks/policy-type-selector/` | `selectedType: PolicyType \| null` | `typeSelected: EventEmitter<PolicyType>` | Grid de 5 cards de tipo |
| `ContinentCardComponent` | `ui/blocks/continent-card/` | `continent: Continent \| null`, `label: string`, `imageSrc: string`, `selected: boolean` | `cardClick: EventEmitter<Continent \| null>` | Card individual de continente/destino |
| `ContinentSelectorComponent` | `ui/blocks/continent-selector/` | `tripType: TripType`, `selectedContinent: Continent \| null` | `continentSelected: EventEmitter<Continent \| null>` | Grid de cards de destino según tipo de viaje |

#### Assets — Mapa de imágenes

```
frontend/src/assets/Policy_images/
├── Salud.jpg        → PolicyTypeCard type="Health"
├── viaje.jpg        → PolicyTypeCard type="Travel"
├── Colombia.png     → ContinentCard tripType="Nacional"
├── America.jpg      → ContinentCard continent="America"
├── Europa.jpg       → ContinentCard continent="Europe"
├── Africa.jpg       → ContinentCard continent="Africa"
└── Asia.jpg         → ContinentCard continent="Asia"
```

> Life, Vehicle y Home usan íconos de Material Icons (`favorite`, `directions_car`, `home`).
> Oceanía usa el ícono `public` — no hay imagen disponible en assets.

#### Modificaciones a `PolicyCreateComponent`

**`policy-create.component.ts` — cambios:**
- Añadir signal `selectedType = signal<PolicyType | null>(null)`
- Añadir método `onTypeCardSelected(type: PolicyType): void` — establece `selectedType` y llama `onTypeChange(type)` + `coverageForm.patchValue({ type })`
- Importar `PolicyTypeSelectorComponent` y `ContinentSelectorComponent`
- Eliminar la lógica de `MatSelectModule` del imports array (ya no se usa para tipo ni continente)

**`policy-create.component.html` — cambios:**
- Añadir `<mat-step label="Tipo de Póliza">` como primer paso (antes de Asegurado)
- Dentro del nuevo paso: `<app-policy-type-selector [selectedType]="selectedType()" (typeSelected)="onTypeCardSelected($event)" />`
- Reemplazar `mat-select[formControlName="type"]` por badge de solo lectura del tipo seleccionado
- Reemplazar `mat-select[data-testid="continent-select"]` por `<app-continent-selector>`
- Cambiar clase `.form-grid` a `.form-vertical` en ambos formularios

**`policy-create.component.css` — cambios:**
- Reemplazar `.form-grid` (2 columnas) por `.form-vertical` (1 columna, max-width 520px, centrado)
- Añadir `.selected-type-badge` — badge naranja de solo lectura

#### Estructura de archivos resultante (nuevos bloques)

```
policies/ui/blocks/
├── policy-type-card/
│   ├── policy-type-card.component.ts
│   ├── policy-type-card.component.html
│   └── policy-type-card.component.css
├── policy-type-selector/
│   ├── policy-type-selector.component.ts
│   ├── policy-type-selector.component.html
│   └── policy-type-selector.component.css
├── continent-card/
│   ├── continent-card.component.ts
│   ├── continent-card.component.html
│   └── continent-card.component.css
└── continent-selector/
    ├── continent-selector.component.ts
    ├── continent-selector.component.html
    └── continent-selector.component.css
```

#### Requisitos técnicos de imágenes

- Las imágenes se referencian mediante rutas relativas a `assets/`: `<img src="assets/Policy_images/Salud.jpg" />`
- El `angular.json` ya debe incluir `"assets"` con el directorio `src/assets` — verificar antes de implementar
- Dimensiones de imagen de card: renderizadas a `200px × 120px` con `object-fit: cover; object-position: center`
- Las imágenes no son `lazy-loaded` en el paso inicial (carga inmediata al renderizar el paso 0)
- Añadir atributo `alt` descriptivo a todas las imágenes (accesibilidad): `alt="Seguro de Salud"`, etc.

#### Notas de Implementación

- `MatStepperModule` permanece. El stepper pasa de `[linear]="true"` con 3 pasos a 4 pasos.
- La validación del Paso 0 (tipo seleccionado) se controla con `[completed]="selectedType() !== null"` en el `mat-step`, no con un `FormGroup`.
- El `mat-select` de tipo en `coverageForm` se mantiene en el TS (`coverageForm.type`) como valor de formulario, pero ya no se renderiza en el template — su valor se establece mediante `patchValue` al seleccionar la card.
- El `mat-select` de continente se elimina del template; la lógica de `onContinentChange` se conserva y se llama desde el output de `ContinentSelectorComponent`.
- Todos los componentes nuevos son `standalone: true` con `templateUrl` y `styleUrl` separados — sin template/styles inline.
- CSS de las cards sigue la paleta de SPEC-006: `#FFFFFF`, `#111111`, `#FF6B2C`.

---

## 3. LISTA DE TAREAS

> Checklist accionable. Marcar cada ítem (`[x]`) al completarlo.

### Frontend

#### Nuevos componentes

- [ ] Crear `PolicyTypeCardComponent` — 3 archivos (ts/html/css)
  - [ ] Input `type`, `selected`, `imageSrc?`, `icon?`, `label`, `description`
  - [ ] Output `cardClick: EventEmitter<PolicyType>`
  - [ ] CSS: estado hover (scale + sombra naranja), estado selected (borde naranja)
  - [ ] HTML: imagen si `imageSrc` existe, ícono de Material si no
- [ ] Crear `PolicyTypeSelectorComponent` — 3 archivos (ts/html/css)
  - [ ] Input `selectedType: PolicyType | null`
  - [ ] Output `typeSelected: EventEmitter<PolicyType>`
  - [ ] CSS: cuadrícula 2 cols mobile / 3 cols desktop
  - [ ] HTML: 5 instancias de `PolicyTypeCardComponent` con sus assets/íconos
- [ ] Crear `ContinentCardComponent` — 3 archivos (ts/html/css)
  - [ ] Input `continent: Continent | null`, `label`, `imageSrc`, `selected`
  - [ ] Output `cardClick: EventEmitter<Continent | null>`
  - [ ] CSS: igual que PolicyTypeCard pero dimensiones 160×180px
- [ ] Crear `ContinentSelectorComponent` — 3 archivos (ts/html/css)
  - [ ] Input `tripType: TripType`, `selectedContinent: Continent | null`
  - [ ] Output `continentSelected: EventEmitter<Continent | null>`
  - [ ] HTML: 1 card Colombia si Nacional; 4 cards internacionales si Internacional
  - [ ] Lógica: auto-seleccionar Colombia para Nacional al renderizar

#### Modificaciones a componentes existentes

- [ ] `policy-create.component.ts`:
  - [ ] Añadir `selectedType = signal<PolicyType | null>(null)`
  - [ ] Añadir `onTypeCardSelected(type: PolicyType): void`
  - [ ] Importar `PolicyTypeSelectorComponent`, `ContinentSelectorComponent`
- [ ] `policy-create.component.html`:
  - [ ] Añadir `mat-step` "Tipo de Póliza" como primer paso con `[completed]`
  - [ ] Insertar `<app-policy-type-selector>`
  - [ ] Reemplazar `mat-select` de tipo por badge de solo lectura
  - [ ] Reemplazar `mat-select` de continente por `<app-continent-selector>`
  - [ ] Cambiar `.form-grid` → `.form-vertical` en ambos formularios
  - [ ] Hint "Selecciona un tipo de póliza" cuando `selectedType()` es null
- [ ] `policy-create.component.css`:
  - [ ] Eliminar `.form-grid` con 2 columnas
  - [ ] Añadir `.form-vertical` (columna única, max-width 520px, centrado)
  - [ ] Añadir `.selected-type-badge` (badge naranja, solo lectura)

#### Verificación pre-entrega

- [ ] Verificar que `assets/Policy_images/` está incluido en `angular.json` bajo `"assets"`
- [ ] Confirmar que las 7 imágenes se sirven correctamente en desarrollo (`ng serve`)
- [ ] Verificar que el stepper de 4 pasos avanza/retrocede correctamente
- [ ] Confirmar que `submit()` sigue funcionando con el nuevo flujo (el `coverageForm.type` tiene valor)
- [ ] Confirmar que los flujos de Health y Travel (cálculos) siguen operando tras el rediseño

### Backend

> No se requieren cambios de backend en esta spec.

### QA

- [ ] Verificar CRITERIO-1.1 — 5 cards visibles con imagen/ícono correcto
- [ ] Verificar CRITERIO-1.2 — selección de card habilita "Continuar" y actualiza tipo
- [ ] Verificar CRITERIO-1.3 — botón deshabilitado sin selección
- [ ] Verificar CRITERIO-1.4 — cambio de tipo limpia datos de Health
- [ ] Verificar CRITERIO-2.1 — cards de destino reemplazan `mat-select` en flujo Travel
- [ ] Verificar CRITERIO-2.2 — selección de continente activa cálculo y muestra `TravelPlanPreviewComponent`
- [ ] Verificar CRITERIO-2.3 — Nacional auto-selecciona Colombia
- [ ] Verificar CRITERIO-2.4 — no avanzar sin continente Internacional
- [ ] Verificar CRITERIO-2.5 — cambio Nacional→Internacional limpia continente
- [ ] Verificar CRITERIO-3.1 — formulario en columna única centrada (max-width 520px)
- [ ] Verificar CRITERIO-3.2 — validaciones de formulario intactas
- [ ] Verificar CRITERIO-3.3 — badge de tipo solo lectura visible en Paso 2
- [ ] Verificar CRITERIO-3.4 — responsividad correcta en < 480px
- [ ] Verificar no regresión: flujo completo Health (plan → cálculo → submit)
- [ ] Verificar no regresión: flujo completo Travel Internacional (card → continente → cálculo → submit)
- [ ] Verificar no regresión: tipos Life/Vehicle/Home (sin imágenes, con íconos)
- [ ] Actualizar estado spec a `IMPLEMENTED` al completar todos los ítems
