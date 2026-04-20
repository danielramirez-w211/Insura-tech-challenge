---
name: migrate-repositories
description: Migra Repository implementations de C# con MongoDbContext a Spring Data MongoDB (@Repository, MongoRepository<T, ID>)
input: Clase Repository de C# que hereda MongoRepository<T> base o usa MongoDbContext directamente
output: Interface @Repository que extiende MongoRepository<T, String> o implementación con MongoTemplate para queries complejas
spec-mapping: Todos los repositorios definidos en la capa Infrastructure del proyecto
---

# Instrucciones para Claude

Eres un experto en migración de repositories MongoDB de C# .NET 8 a Spring Data MongoDB 4.x.

## Estrategia de migración por complejidad

### Caso 1 — Repositorio simple (solo CRUD) → Interface Spring Data

```java
// Origen: PolicyRepository.cs con métodos básicos
// Destino: Interface que Spring Data implementa automáticamente

public interface PolicyRepository extends MongoRepository<Policy, String> {

    Optional<Policy> findByIdAndIsDeletedFalse(String id);

    List<Policy> findByIsDeletedFalseAndCreatedByAdvisorId(String advisorId);

    @Query("{ 'insured.documentId': ?0, 'isDeleted': false }")
    Optional<Policy> findByInsuredDocumentId(String documentId);

    Page<Policy> findByIsDeletedFalse(Pageable pageable);
}
```

### Caso 2 — Repositorio complejo (filters dinámicos, aggregations) → MongoTemplate

```java
@Repository
@RequiredArgsConstructor
public class PolicyRepositoryImpl implements PolicyRepositoryCustom {

    private final MongoTemplate mongoTemplate;

    @Override
    public Page<Policy> findAll(PolicyFilters filters, Pageable pageable) {
        var criteria = Criteria.where("isDeleted").is(false);

        if (filters.documentId() != null) {
            criteria.and("insured.documentId").is(filters.documentId());
        }
        if (filters.insuredSearch() != null) {
            var pattern = Pattern.compile(filters.insuredSearch(), Pattern.CASE_INSENSITIVE);
            criteria.orOperator(
                Criteria.where("insured.firstName").regex(pattern),
                Criteria.where("insured.lastName").regex(pattern)
            );
        }
        if (filters.status() != null) {
            criteria.and("status").is(filters.status());
        }

        var query = new Query(criteria).with(pageable);
        var content = mongoTemplate.find(query, Policy.class);
        var total   = mongoTemplate.count(new Query(criteria), Policy.class);
        return new PageImpl<>(content, pageable, total);
    }
}
```

## Mapeo de operaciones

| C# (MongoRepository base) | Java (Spring Data MongoDB) |
|---------------------------|---------------------------|
| `NotDeleted()` → filtro `IsDeleted == false` | `.is(false)` en Criteria o `findByIsDeletedFalse()` en la interface |
| `ApplyPagination(query, page, pageSize)` | `PageRequest.of(page - 1, pageSize)` (Spring usa 0-indexed) |
| `context.Policies.Find(filter).ToListAsync()` | `mongoTemplate.find(query, Policy.class)` |
| `context.Policies.InsertOneAsync(entity)` | `repository.save(entity)` |
| `context.Policies.ReplaceOneAsync(filter, entity)` | `repository.save(entity)` (MongoDB upsert) |
| `context.Policies.CountDocumentsAsync(filter)` | `mongoTemplate.count(query, Policy.class)` |
| `Builders<T>.Filter.Eq(x => x.Status, value)` | `Criteria.where("status").is(value)` |
| `Builders<T>.Filter.Regex("field", regex)` | `Criteria.where("field").regex(pattern)` |
| `Builders<T>.Filter.Or(f1, f2)` | `new Criteria().orOperator(c1, c2)` |
| `Builders<T>.Filter.And(filters)` | `new Criteria().andOperator(criterias)` |
| `context.Policies.Aggregate<T>(pipeline)` | `mongoTemplate.aggregate(aggregation, Policy.class, T.class)` |

## Paginación — diferencia crítica

C# usa páginas **1-indexed**. Spring Data usa **0-indexed**:
```java
// Si C# recibe page=1 → Spring necesita page=0
Pageable pageable = PageRequest.of(page - 1, pageSize);
```

## Soft delete pattern

```java
// En lugar de deleteById(), marcar como eliminado:
public void softDelete(String id) {
    var policy = repository.findById(id)
        .orElseThrow(() -> new EntityNotFoundException(id));
    policy.setDeleted(true);
    policy.setUpdatedAt(Instant.now());
    repository.save(policy);
}
```

## Índices MongoDB

En C# los índices se definen en `IndexManager.cs`. En Spring Boot se definen con anotaciones en la entidad:
```java
@Document(collection = "policies")
@CompoundIndexes({
    @CompoundIndex(name = "policy_number_idx", def = "{'number.value': 1}", unique = true),
    @CompoundIndex(name = "advisor_status_idx", def = "{'createdByAdvisorId': 1, 'status': 1}"),
})
public class Policy { ... }
```

O bien en un `ApplicationRunner` @Bean al arrancar la aplicación (equivalente al `IndexManager.EnsureAllAsync()`).

## Imports obligatorios

```java
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.data.mongodb.core.query.Criteria;
import org.springframework.data.mongodb.core.query.Query;
import org.springframework.data.mongodb.repository.MongoRepository;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageImpl;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Repository;
import lombok.RequiredArgsConstructor;
import java.util.regex.Pattern;
```

## Convenciones

- Interface: `PolicyRepository extends MongoRepository<Policy, String>`
- Implementación custom: `PolicyRepositoryImpl implements PolicyRepositoryCustom`
- Paquete: `com.insuratech.infrastructure.persistence.repositories`
- El ID es siempre `String` (MongoDB ObjectId como string) — NO `UUID`

## Formato de salida

Genera ÚNICAMENTE el código Java (interface + implementación si aplica). Al inicio incluye:
```java
// Spec: Infraestructura — repository layer
// Origen: <NombreRepository>.cs → <NombreRepository>.java
// Paquete: com.insuratech.infrastructure.persistence.repositories
```
