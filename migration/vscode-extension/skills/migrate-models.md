---
name: migrate-models
description: Migra entidades de Domain, Value Objects, DTOs y Records de C# a Java 21 con JPA/MongoDB, Lombok y Bean Validation
input: Clase C# que sea Entity (hereda AggregateRoot o Entity), Value Object (sealed record), DTO o Response record
output: Clase Java con @Entity (JPA) / @Document (MongoDB) o record Java 21 inmutable, con anotaciones Lombok y Bean Validation
spec-mapping: Todos los modelos de dominio definidos en SPEC-001 a SPEC-016
---

# Instrucciones para Claude

Eres un experto en migración de modelos de dominio de C# .NET 8 a Java 21 Spring Boot 3.x con MongoDB o JPA.

## Reglas de migración — Entidades de dominio

### Si hereda de `AggregateRoot` o `Entity` → `@Document` (MongoDB)
```java
@Document(collection = "policies")  // snake_case plural del nombre de la entidad
@Getter
@NoArgsConstructor(access = AccessLevel.PROTECTED)
public class Policy {
    @Id
    private String id;  // en lugar de Guid → String (MongoDB ObjectId)
    // ...
}
```

### Si es un `sealed record` Value Object → `record` Java 21
```java
public record PolicyNumber(int year, long sequence) {
    public PolicyNumber {
        if (year < 2000) throw new IllegalArgumentException("Invalid year");
        if (sequence < 1) throw new IllegalArgumentException("Invalid sequence");
    }

    public static PolicyNumber create(int year, long sequence) {
        return new PolicyNumber(year, sequence);
    }

    public String value() {
        return String.format("POL-%d-%06d", year, sequence);
    }
}
```

### Si es un DTO o Response → `record` Java 21 plano
```java
public record PolicyResponse(
    String id,
    String type,
    String status,
    @JsonProperty("insuredAmount") BigDecimal insuredAmount,
    // ...
) {}
```

## Mapeo de tipos

| C# | Java |
|----|------|
| `string` | `String` |
| `int` | `int` / `Integer` |
| `long` | `long` / `Long` |
| `decimal` | `BigDecimal` |
| `bool` | `boolean` / `Boolean` |
| `Guid` | `String` (UUID como string) |
| `DateOnly` | `LocalDate` |
| `DateTime` / `DateTimeOffset` | `LocalDateTime` / `Instant` |
| `List<T>` | `List<T>` |
| `IReadOnlyList<T>` | `List<T>` (unmodifiable en constructor) |
| `T?` (nullable) | `@Nullable T` o `Optional<T>` |

## Mapeo de atributos de validación

| C# (FluentValidation / DataAnnotations) | Java (Bean Validation) |
|----------------------------------------|------------------------|
| `.NotEmpty()` | `@NotBlank` |
| `.NotNull()` | `@NotNull` |
| `.MaximumLength(n)` | `@Size(max = n)` |
| `.MinimumLength(n)` | `@Size(min = n)` |
| `.InclusiveBetween(a, b)` | `@Min(a) @Max(b)` |
| `.EmailAddress()` | `@Email` |

## Imports obligatorios según tipo

Para `@Document` (MongoDB):
```java
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.mapping.Document;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.AccessLevel;
```

Para `record` Value Object:
```java
import com.fasterxml.jackson.annotation.JsonProperty; // si es serializable
```

Para `record` DTO/Response:
```java
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.validation.constraints.*;
```

## Convenciones de naming

- Paquete entidades: `com.insuratech.domain.<agregado>`
- Paquete DTOs: `com.insuratech.application.<feature>.dto`
- Paquete Value Objects: `com.insuratech.domain.<agregado>.vo`
- Nombre de clase: igual que en C# (sin cambios)
- Nombre de colección MongoDB: snake_case plural del agregado

## Reglas de factory methods

Si C# tiene métodos estáticos `Create(...)` → mantener en Java como métodos estáticos en la entidad:
```java
public static Policy create(PolicyNumber number, PolicyType type, InsuredPerson insured, ...) {
    Policy policy = new Policy();
    policy.id = UUID.randomUUID().toString();
    policy.number = number;
    // ...
    return policy;
}
```

## Formato de salida

Genera ÚNICAMENTE el código Java. Al inicio incluye:
```java
// Spec: SPEC-XXX — <nombre-feature>
// Origen: <NombreClaseCSharp>.cs → <NombreClaseJava>.java
// Paquete: com.insuratech.<capa>.<modulo>
```
