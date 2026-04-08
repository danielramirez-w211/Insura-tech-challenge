# ADR-001 — Arquitectura de módulos frontend por capas (SPEC-005)

**Fecha:** 2026-04-08
**Estado:** Aceptado

## Contexto

El frontend de InsuraTech comenzó con una estructura plana por módulo:

```
<feature>/
├── components/   # componentes con template/styles inline
├── models/       # DTOs
├── pages/        # páginas con template/styles inline
└── services/     # servicios HTTP
```

Esta estructura mezcla responsabilidades, impide la reutilización de componentes entre módulos y dificulta el testing unitario de componentes presentacionales.

## Decisión

Migrar todos los módulos (`policies`, `claims`, `dashboard`, `notifications`) a la arquitectura en capas definida en SPEC-005:

```
<feature>/
├── container/    # (solo si el módulo lo requiere) facade + container component
├── core/
│   ├── models/   # tipos de dominio sin sufijo Dto
│   ├── resource/ # shapes de la API (request/response)
│   └── service/  # servicios HTTP con signals de estado
├── guards/
└── ui/
    ├── blocks/   # componentes complejos reutilizables (@Input/@Output)
    ├── elements/ # componentes simples presentacionales
    ├── form/     # componentes de formulario
    ├── layouts/  # wrappers de layout
    └── pages/    # componentes de página (routed)
```

**Regla obligatoria:** cada componente Angular usa **3 archivos separados** (`.ts`, `.html`, `.css`). Prohibido el uso de `template:` o `styles:` inline.

## Consecuencias

**Positivas:**
- Separación clara de responsabilidades entre capa HTTP, estado y presentación
- Componentes `ui/blocks/` son 100% presentacionales — fáciles de testear con inputs
- Elimina duplicación de modelos (un único `core/models/` por módulo)
- El renombramiento de `XxxDto` → `Xxx` alinea el código con el lenguaje del dominio

**Negativas / trade-offs:**
- Mayor profundidad de carpetas (más imports relativos)
- Requirió migración manual de todos los módulos existentes

## Alternativas Descartadas

- **Mantener estructura plana**: genera acoplamiento entre capas y template inline que dificulta el mantenimiento.
- **NgModules**: descartado porque el proyecto usa Angular 19 con Standalone Components, que es el estándar actual.
