# Specs — Fuente de Verdad del Proyecto ASDD

Este directorio contiene las especificaciones técnicas de cada funcionalidad. Son la fuente de verdad para todos los agentes de desarrollo.

## Ciclo de Vida

```
DRAFT → APPROVED → IN_PROGRESS → IMPLEMENTED → DEPRECATED
```

| Estado | Quién | Condición |
|--------|-------|-----------|
| `DRAFT` | spec-generator | Spec generada, pendiente de revisión humana |
| `APPROVED` | Usuario / Tech Lead | Revisada y aprobada — verde para implementar |
| `IN_PROGRESS` | orchestrator | Implementación en curso |
| `IMPLEMENTED` | orchestrator | Código + tests + QA completos |
| `DEPRECATED` | Usuario | Descartada o reemplazada por otra spec |

> **Regla:** Sin `status: APPROVED` en el frontmatter → ningún agente implementa código.

## Convención de Nombres

```
.github/specs/<nombre-feature-en-kebab-case>.spec.md
```

## Índice de Specs

| ID | Feature | Archivo | Estado | Fecha |
|----|---------|---------|--------|-------|
| SPEC-001 | Backend fixes + Notificaciones | [backend-fixes-and-notifications.spec.md](../../.github/specs/backend-fixes-and-notifications.spec.md) | `APPROVED` | 2026-03-31 |
| SPEC-002 | Frontend base structure Angular 19 | [frontend-base-structure.spec.md](../../.github/specs/frontend-base-structure.spec.md) | `APPROVED` | 2026-03-31 |
| SPEC-003 | Módulo de Planes de Salud + cálculo automático | [health-plan-pricing.spec.md](../../.github/specs/health-plan-pricing.spec.md) | `IN_PROGRESS` | 2026-04-06 |
| SPEC-007 | Rediseño visual flujo de contratación | [policy-create-visual-redesign.spec.md](policy-create-visual-redesign.spec.md) | `IMPLEMENTED` | 2026-04-09 |
| SPEC-008 | Reingeniería prima — periodo fijo 12 meses | [health-premium-fixed-period.spec.md](health-premium-fixed-period.spec.md) | `IN_PROGRESS` | 2026-04-09 |

> Actualizar esta tabla cada vez que se crea o cambia el estado de una spec.

## Requerimientos pendientes de spec

Los siguientes requerimientos están en `.github/requirements/` listos para convertirse en spec:

| Requerimiento | Archivo | Acción |
|---------------|---------|--------|
| Creación de Usuarios | `.github/requirements/user-creation.md` | `/generate-spec user-creation` |

## Cómo crear una spec nueva

**Opción 1 — Desde un requerimiento existente:**
```
/generate-spec user-creation
```

**Opción 2 — Desde cero:**
```
/generate-spec
> Descripción del feature: ...
```

**Opción 3 — Orquestación completa (spec → implementación → tests → QA):**
```
/asdd-orchestrate
> Feature: nombre del feature
```

## Frontmatter requerido en toda spec

```yaml
---
id: SPEC-001
status: DRAFT
feature: nombre-del-feature
created: YYYY-MM-DD
updated: YYYY-MM-DD
author: spec-generator
version: "1.0"
related-specs: []
---
```

## Template

Ver `.github/skills/generate-spec/spec-template.md`
