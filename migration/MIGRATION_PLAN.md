# InsuraTech — Plan de Migración C# .NET 8 → Java 21 Spring Boot

> Trazabilidad entre specs ASDD originales y los artefactos Java destino.
> Estado: ⬜ Pendiente | 🔄 En progreso | ✅ Completado

---

## Tabla de trazabilidad

| Spec | Feature | Estado Spec | Archivos C# principales | Archivos Java destino | Skill recomendada | Estado migración |
|------|---------|-------------|------------------------|----------------------|-------------------|-----------------|
| SPEC-001 | conversiones | DRAFT | — | — | analyze-csharp | ⬜ |
| SPEC-006 | policies-list-visual-upgrade | IMPLEMENTED | `PoliciesController.cs`, `GetPoliciesHandler.cs` | `PolicyController.java`, `PolicyQueryService.java` | migrate-controllers, migrate-services | ⬜ |
| SPEC-007 | policy-create-visual-redesign | IMPLEMENTED | `CreatePolicyHandler.cs`, `CreatePolicyCommand.cs` | `PolicyService.java`, `CreatePolicyRequest.java` | migrate-services, migrate-models | ⬜ |
| SPEC-008 | health-premium-fixed-period | IN_PROGRESS | `CreateHealthPolicyStrategy.cs`, `HealthPlanCatalog.cs` | `HealthPolicyStrategy.java`, `HealthPlanConfig.java` | migrate-services, migrate-models | ⬜ |
| SPEC-009 | life-plan-module | IN_PROGRESS | `CreateLifePolicyStrategy.cs` | `LifePolicyStrategy.java` | migrate-services | ⬜ |
| SPEC-010 | vehicle-quotation-module | IMPLEMENTED | `CreateVehiclePolicyStrategy.cs`, `VehiclePlanCatalog.cs` | `VehiclePolicyStrategy.java` | migrate-services | ⬜ |
| SPEC-011 | insured-form-ux-improvements | IN_PROGRESS | `InsuredPerson.cs` (Value Object) | `InsuredPerson.java` (record) | migrate-models | ⬜ |
| SPEC-012 | home-quotation-module | IN_PROGRESS | `CreateHomePolicyStrategy.cs` | `HomePolicyStrategy.java` | migrate-services | ⬜ |
| SPEC-013 | authentication-user-system | IN_PROGRESS | `AuthController.cs`, `JwtService.cs`, `FirebaseAuthService.cs` | `AuthController.java`, `JwtService.java`, `FirebaseAuthService.java` | migrate-controllers, migrate-services, migrate-config | ⬜ |
| SPEC-014 | user-management | IN_PROGRESS | `UsersController.cs`, `CreateAdvisorHandler.cs`, `UserRepository.cs` | `UserController.java`, `UserService.java`, `UserRepository.java` | migrate-controllers, migrate-services, migrate-repositories | ⬜ |
| SPEC-015 | header-navbar | IN_PROGRESS | Frontend Angular — no aplica | Frontend Angular — no aplica | — (solo frontend) | N/A |
| SPEC-016 | backend-refactor | APPROVED | `MongoDbContext.cs`, `ClassMapRegistry.cs`, `MongoRepository.cs` | `MongoConfig.java`, `BaseRepository.java` | migrate-config, migrate-repositories | ⬜ |

---

## Estructura de paquetes Java destino

```
com.insuratech/
├── api/
│   ├── controllers/          ← migrar desde InsuraTech.API/Controllers/
│   └── middleware/           ← migrar GlobalExceptionHandler
├── application/
│   ├── policies/
│   │   ├── service/          ← migrar Handlers de Application/Policies/
│   │   ├── dto/              ← migrar DTOs / Response records
│   │   └── strategy/         ← migrar ICreatePolicyStrategy + implementaciones
│   ├── auth/
│   │   └── service/
│   ├── users/
│   │   └── service/
│   └── claims/
│       └── service/
├── domain/
│   ├── policies/             ← migrar Domain/Policies/
│   │   └── vo/               ← PolicyNumber, InsuredPerson, CoveragePeriod
│   ├── users/                ← migrar Domain/Users/
│   ├── claims/               ← migrar Domain/Claims/
│   └── common/               ← Entity base class, excepciones
└── infrastructure/
    ├── persistence/
    │   ├── config/           ← migrar MongoDbContext, ClassMapRegistry
    │   └── repositories/     ← migrar PolicyRepository, ClaimRepository, etc.
    ├── auth/                 ← migrar JwtService, FirebaseAuthService
    └── external/             ← migrar ITrmService, TrmService
```

---

## Secuencia de migración recomendada

### Fase 1 — Domain Layer (sin dependencias)
1. `InsuredPerson.cs` → `InsuredPerson.java` (record)
2. `PolicyNumber.cs` → `PolicyNumber.java` (record)
3. `CoveragePeriod.cs` → `CoveragePeriod.java` (record)
4. `Policy.cs` → `Policy.java` (@Document)
5. `Claim.cs`, `User.cs`, `Notification.cs` → equivalentes Java

**Skill:** `migrate-models`

### Fase 2 — Infrastructure Layer
6. `MongoDbContext.cs` + `ClassMapRegistry.cs` → `MongoConfig.java` (@Configuration)
7. `PolicyRepository.cs` → `PolicyRepository.java` (interface + MongoTemplate impl)
8. `ClaimRepository.cs`, `UserRepository.cs`, `NotificationRepository.cs`

**Skill:** `migrate-repositories`, `migrate-config`

### Fase 3 — Application Layer (Strategies + Services)
9. `ICreatePolicyStrategy.cs` + 5 implementaciones → interface + @Component
10. `CreatePolicyHandler.cs` → `PolicyService.create()`
11. `GetPoliciesHandler.cs` → `PolicyQueryService.getAll()`
12. Demás Handlers de Claims, Users, Auth

**Skill:** `migrate-services`

### Fase 4 — API Layer
13. `PoliciesController.cs` → `PolicyController.java`
14. `ClaimsController.cs`, `UsersController.cs`, `AuthController.cs`
15. `GlobalExceptionHandler.cs` middleware → `GlobalExceptionHandler.java` (@RestControllerAdvice)

**Skill:** `migrate-controllers`

### Fase 5 — Configuración y arranque
16. `DependencyInjection.cs` (Application) → component scan automático
17. `DependencyInjection.cs` (Infrastructure) → `InfrastructureConfig.java`
18. `Program.cs` → `InsuraTechApplication.java` + `SecurityConfig.java`
19. `appsettings.json` → `application.yml`

**Skill:** `migrate-config`

### Fase 6 — Tests
20. Migrar tests de `InsuraTech.Application.Tests/` → JUnit 5 + Mockito

**Skill:** `migrate-tests`

### Fase 7 — Validación
21. Validar cada archivo migrado contra su spec ASDD con `validate-migration`

---

## Notas de migración

- **Frontend Angular**: No se migra — permanece en Angular 19 consumiendo la nueva API Spring Boot
- **MongoDB**: El mismo cluster MongoDB puede ser usado por ambas aplicaciones durante la transición
- **Firebase Auth**: Firebase Admin SDK está disponible para Java (`firebase-admin:9.2.0`)
- **TRM Service**: Migrar `ITrmService` + `TrmService` implementación HTTP a `@Service` + `RestClient`
- **Paginación**: C# usa páginas 1-indexed → Spring Data usa 0-indexed. Ajustar en todos los queries.
