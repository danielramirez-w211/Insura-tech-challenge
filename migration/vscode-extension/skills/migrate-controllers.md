---
name: migrate-controllers
description: Migra Controllers de ASP.NET Core a @RestController de Spring Boot 3.x con ResponseEntity, autorización y validación
input: Clase Controller de C# (.NET 8) con sus acciones HTTP, atributos [Authorize], [HttpGet/Post/Put/Delete], etc.
output: Clase Java @RestController con @RequestMapping, ResponseEntity<T>, @PreAuthorize, @Valid, Lombok
spec-mapping: Todos los endpoints definidos en los specs del proyecto (SPEC-001 a SPEC-016)
---

# Instrucciones para Claude

Eres un experto en migración de ASP.NET Core Controllers a Spring Boot 3.x @RestController.

## Mapeo de atributos HTTP

| C# ASP.NET Core | Java Spring Boot |
|-----------------|-----------------|
| `[HttpGet]` | `@GetMapping` |
| `[HttpGet("{id}")]` | `@GetMapping("/{id}")` |
| `[HttpPost]` | `@PostMapping` |
| `[HttpPut("{id}")]` | `@PutMapping("/{id}")` |
| `[HttpDelete("{id}")]` | `@DeleteMapping("/{id}")` |
| `[HttpPatch("{id}")]` | `@PatchMapping("/{id}")` |
| `[Route("api/v1/policies")]` | `@RequestMapping("/api/v1/policies")` |
| `[ApiController]` | `@RestController` |
| `[Authorize]` | `@PreAuthorize("isAuthenticated()")` |
| `[Authorize(Roles = "Admin")]` | `@PreAuthorize("hasRole('Admin')")` |
| `[AllowAnonymous]` | (sin anotación, o `permitAll()` en SecurityConfig) |

## Mapeo de parámetros

| C# | Java |
|----|------|
| `[FromBody] CreatePolicyRequest req` | `@RequestBody @Valid CreatePolicyRequest req` |
| `[FromQuery] string? search` | `@RequestParam(required = false) String search` |
| `[FromQuery] int page = 1` | `@RequestParam(defaultValue = "1") int page` |
| `[FromRoute] Guid id` | `@PathVariable String id` |
| `[FromHeader] string token` | `@RequestHeader("Authorization") String token` |
| `CancellationToken ct` | Omitir — Spring Boot maneja esto internamente |

## Mapeo de respuestas

| C# | Java |
|----|------|
| `return Ok(dto)` | `return ResponseEntity.ok(dto)` |
| `return Created(uri, dto)` | `return ResponseEntity.created(URI.create("/api/v1/...")).body(dto)` |
| `return NoContent()` | `return ResponseEntity.noContent().build()` |
| `return NotFound()` | `return ResponseEntity.notFound().build()` |
| `return BadRequest(msg)` | `return ResponseEntity.badRequest().body(msg)` |
| `return Unauthorized()` | `return ResponseEntity.status(401).build()` |
| `return Forbid()` | `return ResponseEntity.status(403).build()` |
| `IActionResult` | `ResponseEntity<T>` (usar tipo específico si conocido) |
| `ActionResult<T>` | `ResponseEntity<T>` |

## Inyección de dependencias

C# usa constructor injection con el handler de MediatR:
```csharp
private readonly ISender _sender;
public PoliciesController(ISender sender) => _sender = sender;
```

Java usa `@RequiredArgsConstructor` de Lombok + `@Autowired` implícito:
```java
@RestController
@RequestMapping("/api/v1/policies")
@RequiredArgsConstructor
@Tag(name = "Policies", description = "Gestión de pólizas")
public class PolicyController {
    private final PolicyService policyService;
```

## Extracción del usuario autenticado

| C# | Java |
|----|------|
| `User.FindFirst(ClaimTypes.NameIdentifier)?.Value` | `((UserPrincipal) auth.getPrincipal()).getId()` |
| Inyectado via `[HttpContext]` | `@AuthenticationPrincipal UserPrincipal principal` en el método |

## Estructura completa de ejemplo

```java
// Spec: SPEC-013 — authentication-user-system
// Origen: PoliciesController.cs → PolicyController.java
// Paquete: com.insuratech.api.controllers

package com.insuratech.api.controllers;

import com.insuratech.application.policies.dto.*;
import com.insuratech.application.policies.service.PolicyService;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;

import java.net.URI;

@RestController
@RequestMapping("/api/v1/policies")
@RequiredArgsConstructor
@Tag(name = "Policies")
public class PolicyController {

    private final PolicyService policyService;

    @GetMapping
    @PreAuthorize("isAuthenticated()")
    public ResponseEntity<PagedResponse<PolicyResponse>> getAll(
            @RequestParam(defaultValue = "1") int page,
            @RequestParam(defaultValue = "10") int pageSize,
            @RequestParam(required = false) String documentId) {
        return ResponseEntity.ok(policyService.getAll(page, pageSize, documentId));
    }

    @PostMapping
    @PreAuthorize("hasAnyRole('Advisor', 'Admin')")
    public ResponseEntity<PolicyResponse> create(
            @RequestBody @Valid CreatePolicyRequest request,
            @AuthenticationPrincipal UserPrincipal principal) {
        var result = policyService.create(request, principal.getId());
        return ResponseEntity.created(URI.create("/api/v1/policies/" + result.id())).body(result);
    }
}
```

## Manejo de excepciones

NO incluyas try-catch en los controllers. En Spring Boot el manejo de excepciones se centraliza en un `@ControllerAdvice`:
```java
// Nota: el GlobalExceptionHandler debe migrarse con migrate-services
```

## Formato de salida

Genera ÚNICAMENTE el código Java. Al inicio incluye:
```java
// Spec: SPEC-XXX — <nombre-feature>
// Origen: <NombreClaseCSharp>.cs → <NombreClaseJava>.java
// Paquete: com.insuratech.api.controllers
```
