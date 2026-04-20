---
name: migrate-services
description: Migra Application Services, MediatR Command/Query Handlers y Application Services de C# a @Service de Spring Boot 3.x
input: Clase C# que sea IRequestHandler<Command, Response>, IRequestHandler<Query, Response> o ApplicationService
output: Clase Java @Service con métodos de negocio equivalentes, inyección de repositorios vía @RequiredArgsConstructor
spec-mapping: Todos los casos de uso definidos en los specs del proyecto (Commands y Queries de cada feature)
---

# Instrucciones para Claude

Eres un experto en migración de CQRS Handlers (MediatR) y Application Services de C# .NET 8 a @Service de Spring Boot 3.x.

## Mapeo arquitectónico

| Patrón C# | Patrón Java Spring Boot |
|-----------|------------------------|
| `IRequestHandler<CreatePolicyCommand, PolicyResponse>` | `@Service PolicyService.create(CreatePolicyRequest) : PolicyResponse` |
| `IRequestHandler<GetPoliciesQuery, PagedList<PolicyResponse>>` | `@Service PolicyQueryService.getAll(filters) : Page<PolicyResponse>` |
| `IUnitOfWork.CommitAsync()` | `@Transactional` en el método del service |
| `_policyRepository.AddAsync(policy, ct)` | `policyRepository.save(policy)` |
| `_policyRepository.GetByIdAsync(id, ct)` | `policyRepository.findById(id).orElseThrow(...)` |
| `MediatR ISender` | Inyección directa del @Service en el Controller |
| `FluentValidation pipeline behavior` | `@Valid` en el parámetro del método + `@Validated` en la clase |

## Estrategia de migración de Handlers

Los Handlers de MediatR en C# se convierten en métodos del `@Service` correspondiente:

```java
// C#: CreatePolicyHandler : IRequestHandler<CreatePolicyCommand, PolicyResponse>
// Java: PolicyService.create(CreatePolicyRequest, String advisorId)

@Service
@RequiredArgsConstructor
@Transactional  // si el handler hace writes
public class PolicyService {

    private final PolicyRepository policyRepository;
    private final UnitOfWork unitOfWork;

    public PolicyResponse create(CreatePolicyRequest request, String advisorId) {
        // lógica equivalente al Handle() del Handler
        var policy = Policy.create(/* params */);
        policyRepository.save(policy);
        return PolicyMapper.toResponse(policy);
    }
}
```

## Mapeo de operaciones de repositorio

| C# | Java (Spring Data MongoDB) |
|----|---------------------------|
| `repo.AddAsync(entity, ct)` | `repo.save(entity)` |
| `repo.GetByIdAsync(id, ct)` | `repo.findById(id).orElseThrow(() -> new EntityNotFoundException(id))` |
| `repo.GetAllAsync(filters, ct)` | `repo.findAll(query, pageable)` |
| `repo.UpdateAsync(entity, ct)` | `repo.save(entity)` (MongoDB upsert) |
| `repo.DeleteAsync(entity, ct)` | `entity.setDeleted(true); repo.save(entity)` (soft delete) |
| `unitOfWork.CommitAsync(ct)` | Eliminado — `@Transactional` maneja esto |

## Manejo de excepciones de dominio

| C# | Java |
|----|------|
| `throw new InvalidPlanException(...)` | `throw new InvalidPlanException(...)` (misma clase, migrar con migrate-models) |
| `throw new DomainException(msg)` | `throw new BusinessException(msg)` |
| `catch (ValidationException ex)` | `MethodArgumentNotValidException` (manejada por Spring automáticamente) |

## Patrón Strategy (si el handler usa ICreatePolicyStrategy)

```java
// Cada estrategia C# → implementación de interface Java

public interface PolicyCreationStrategy {
    PolicyType type();
    boolean canHandle(CreatePolicyRequest request);
    Policy create(CreatePolicyRequest request, PolicyNumber number, InsuredPerson insured);
}

@Component
public class VehiclePolicyStrategy implements PolicyCreationStrategy {
    @Override
    public PolicyType type() { return PolicyType.VEHICLE; }

    @Override
    public boolean canHandle(CreatePolicyRequest request) {
        return request.vehiclePlanId() != null && !request.vehiclePlanId().isBlank();
    }

    @Override
    public Policy create(CreatePolicyRequest request, PolicyNumber number, InsuredPerson insured) {
        // lógica de creación
    }
}
```

## Mapeo de validación

En C# hay `FluentValidation` con `AbstractValidator<T>`. En Java se usa Bean Validation:

```java
// Para validaciones complejas de negocio → lanzar excepción directamente en el service
// Para validaciones de input → @Valid en el request + anotaciones en el record/DTO
```

## GlobalExceptionHandler (migración del Middleware de errores)

```java
@RestControllerAdvice
public class GlobalExceptionHandler {

    @ExceptionHandler(EntityNotFoundException.class)
    public ResponseEntity<ErrorResponse> handleNotFound(EntityNotFoundException ex) {
        return ResponseEntity.status(404).body(new ErrorResponse("NOT_FOUND", ex.getMessage()));
    }

    @ExceptionHandler(BusinessException.class)
    public ResponseEntity<ErrorResponse> handleBusiness(BusinessException ex) {
        return ResponseEntity.status(422).body(new ErrorResponse("BUSINESS_ERROR", ex.getMessage()));
    }

    @ExceptionHandler(MethodArgumentNotValidException.class)
    public ResponseEntity<ErrorResponse> handleValidation(MethodArgumentNotValidException ex) {
        var errors = ex.getBindingResult().getFieldErrors().stream()
            .map(e -> e.getField() + ": " + e.getDefaultMessage())
            .toList();
        return ResponseEntity.status(400).body(new ErrorResponse("VALIDATION_ERROR", errors.toString()));
    }
}
```

## Formato de salida

Genera ÚNICAMENTE el código Java. Al inicio incluye:
```java
// Spec: SPEC-XXX — <nombre-feature>
// Origen: <NombreHandler>.cs → <NombreService>.java
// Paquete: com.insuratech.application.<feature>.service
```
