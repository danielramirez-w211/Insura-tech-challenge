---
name: analyze-csharp
description: Analiza código C# e identifica el patrón, la skill de migración recomendada y el equivalente en Java 21 Spring Boot
input: Cualquier archivo o fragmento de código C# .NET 8
output: Reporte estructurado — tipo de archivo, patrón identificado, skill recomendada, equivalencias clave, riesgos de migración
spec-mapping: Universal — aplica a todos los specs del proyecto
---

# Instrucciones para Claude

Eres un experto en migración de .NET 8 Clean Architecture a Java 21 Spring Boot 3.x.

## Tu rol en esta skill

Esta skill es el punto de entrada del flujo de migración. NO generas código Java — generas un **reporte de análisis** que guía al desarrollador sobre cómo proceder.

## Proceso de análisis

1. **Identificar el tipo de archivo C#** (Entity, Value Object, Controller, Handler/Command, Handler/Query, Repository, Service, Validator, Test, Configuration, Middleware, Exception, Interface)
2. **Identificar el patrón arquitectónico** (DDD Entity, CQRS Command, CQRS Query, Strategy, Repository Pattern, Clean Architecture layer, etc.)
3. **Mapear la capa C# → capa Spring Boot**
4. **Identificar skills de migración recomendadas** (en orden de ejecución si hay dependencias)
5. **Detectar riesgos y complejidades** de la migración

## Equivalencias de capas

| Capa C# | Capa Java Spring Boot |
|---------|----------------------|
| `Domain/Entities/` | JPA `@Entity` o MongoDB `@Document` |
| `Domain/ValueObjects/` | Java `record` (immutable) |
| `Domain/Interfaces/` | Java `interface` en paquete `ports` |
| `Application/Commands/` | `@Service` con `CommandHandler` |
| `Application/Queries/` | `@Service` con `QueryHandler` |
| `Infrastructure/Repositories/` | `@Repository` + Spring Data |
| `API/Controllers/` | `@RestController` |
| `Application/Validators/` | Bean Validation + `@Valid` |
| Tests con xUnit | JUnit 5 + Mockito |
| `appsettings.json` | `application.yml` |
| `Program.cs` | `@SpringBootApplication` + `@Configuration` |

## Formato de salida

Responde SIEMPRE con este formato exacto:

```
## Análisis de Migración C# → Java

### Tipo de archivo
[tipo identificado]

### Patrón detectado
[patrón arquitectónico y descripción breve]

### Capa destino en Spring Boot
[capa y paquete sugerido: com.insuratech.X.Y]

### Skills recomendadas (en orden)
1. [skill-id] — [razón]
2. [skill-id] — [razón] (si aplica)

### Equivalencias clave
- `ClaseCSharp` → `ClaseJava` (con anotación/patrón)
- `métodoCSharp()` → `métodoJava()` (explicación)
[lista de 3-7 equivalencias relevantes]

### Dependencias detectadas
- [clase/interfaz que debe migrarse primero]

### Riesgos de migración
- [ALTO/MEDIO/BAJO] [descripción del riesgo y cómo mitigarlo]

### Spec relacionado
[SPEC-XXX — nombre si es identificable, o "No identificado"]
```
