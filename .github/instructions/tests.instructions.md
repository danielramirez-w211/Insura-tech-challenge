---
applyTo: "Backend/tests/**/*.cs"
---

> **Scope**: Tests del backend de InsuraTech — xUnit, NSubstitute, FluentAssertions, .NET 8.

# Instrucciones para Archivos de Tests

## Stack de Testing

| Librería | Rol |
|---|---|
| **xUnit** | Framework de tests (`[Fact]`, `[Theory]`) |
| **NSubstitute** | Mocking de interfaces (`Substitute.For<T>()`) |
| **FluentAssertions** | Assertions expresivas (`.Should().Be()`, `.Should().ThrowAsync<T>()`) |
| **coverlet** | Cobertura de código |

Comando para ejecutar todos los tests:
```bash
dotnet test Backend/tests/InsuraTech.Application.Tests
dotnet test Backend/tests/InsuraTech.Domain.Tests
```

---

## Proyectos de Tests

| Proyecto | Qué testea |
|---|---|
| `InsuraTech.Application.Tests` | Handlers (CQRS) — repositorios mockeados con NSubstitute |
| `InsuraTech.Domain.Tests` | Entidades de dominio — lógica pura, sin mocks |

Estructura de carpetas dentro de cada proyecto:

```
InsuraTech.Application.Tests/
  Claims/
    RegisterClaimHandlerTests.cs
    ApproveClaimHandlerTests.cs
    ...
  Policies/
    CreatePolicyHandlerTests.cs
    GetPoliciesHandlerTests.cs
    ...

InsuraTech.Domain.Tests/
  Claims/
    ClaimTests.cs
  Policies/
    PolicyTests.cs
```

Cada archivo de tests corresponde a **un único handler o entidad** y se ubica en la carpeta del dominio correspondiente.

---

## Tests de Handlers (Application Layer)

### Estructura de clase obligatoria

```csharp
namespace InsuraTech.Application.Tests.Claims;

using FluentAssertions;
using InsuraTech.Application.Claims.Commands.RegisterClaim;
using InsuraTech.Domain.Interfaces;
using NSubstitute;

public sealed class RegisterClaimHandlerTests
{
    // 1. Campos de mocks — privados y readonly
    private readonly IClaimRepository  _claimRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IUnitOfWork       _unitOfWork;
    private readonly RegisterClaimHandler _handler;

    // 2. Constructor: inicializar todos los mocks y el handler
    public RegisterClaimHandlerTests()
    {
        _claimRepository  = Substitute.For<IClaimRepository>();
        _policyRepository = Substitute.For<IPolicyRepository>();
        _unitOfWork       = Substitute.For<IUnitOfWork>();
        _handler          = new RegisterClaimHandler(_claimRepository, _policyRepository, _unitOfWork);
    }

    // 3. Builder methods — private static, producen datos de prueba reutilizables
    private static Policy BuildActivePolicy() { /* ... */ }
    private static RegisterClaimCommand BuildCommand(Guid policyId) => new() { /* ... */ };

    // 4. Tests con [Fact]
    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterClaim()
    {
        // GIVEN
        var policy = BuildActivePolicy();
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);

        // WHEN
        var result = await _handler.Handle(BuildCommand(policy.Id), CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.Status.Should().Be("Registered");
    }
}
```

### Reglas de nombrado de tests

```
Handle_<Escenario>_Should<ResultadoEsperado>
```

Ejemplos:
- `Handle_WithValidCommand_ShouldRegisterClaim`
- `Handle_WithNonExistentPolicy_ShouldThrowNotFoundException`
- `Handle_WithInactivePolicy_ShouldThrowBusinessRuleException`
- `Handle_WithValidCommand_ShouldCallAddAndSave`

### Happy path

```csharp
[Fact]
public async Task Handle_WithValidCommand_ShouldReturnPagedResult()
{
    // GIVEN
    var policy = BuildActivePolicy();
    _policyRepository
        .GetAllAsync(null, null, null, null, null, null, null, 1, 10, Arg.Any<CancellationToken>())
        .Returns(new[] { policy });
    _policyRepository
        .CountAsync(null, null, null, null, null, null, null, Arg.Any<CancellationToken>())
        .Returns(1);

    // WHEN
    var result = await Sut.Handle(new GetPoliciesQuery(), CancellationToken.None);

    // THEN
    result.TotalCount.Should().Be(1);
    result.Items.Should().HaveCount(1);
}
```

### Error path — excepción de dominio

```csharp
[Fact]
public async Task Handle_WithNonExistentPolicy_ShouldThrowNotFoundException()
{
    // GIVEN
    _policyRepository
        .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
        .Returns((Policy?)null);

    // WHEN
    var act = async () => await _handler.Handle(BuildCommand(Guid.NewGuid()), CancellationToken.None);

    // THEN
    await act.Should().ThrowAsync<NotFoundException>();
}
```

Para verificar el mensaje de la excepción:
```csharp
await act.Should().ThrowAsync<BusinessRuleException>()
    .WithMessage("*not active*");   // wildcard * para match parcial
```

### Verificación de interacciones (call verification)

```csharp
// Verifica que el repositorio fue llamado exactamente una vez con los argumentos correctos
await _claimRepository.Received(1)
    .AddAsync(Arg.Any<Claim>(), Arg.Any<CancellationToken>());

await _unitOfWork.Received(1)
    .SaveChangesAsync(Arg.Any<CancellationToken>());

// Verifica que un método NO fue llamado
await _policyRepository.DidNotReceive()
    .UpdateAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
```

---

## Tests de Dominio (Domain Layer)

Las entidades de dominio se testean con lógica pura — sin mocks, sin async:

```csharp
public sealed class ClaimTests
{
    // Builder method reutilizable
    private static Claim CreateValidClaim(
        int openClaimsCount = 0,
        decimal claimedAmount = 1_000m,
        decimal availableAmount = 10_000m)
    {
        return Claim.Register(/* parámetros válidos */);
    }

    [Fact]
    public void Register_WithValidData_ShouldHaveRegisteredStatus()
    {
        var claim = CreateValidClaim();
        claim.Status.Should().Be(ClaimStatus.Registered);
    }

    [Fact]
    public void Register_With3OpenClaims_ShouldThrowClaimLimitExceededException()
    {
        var act = () => CreateValidClaim(openClaimsCount: 3);
        act.Should().Throw<ClaimLimitExceededException>()
            .WithMessage("*already has 3 open claims*");
    }
}
```

Nombrado para tests de dominio:

```
<Método>_<Escenario>_Should<ResultadoEsperado>
```

Ejemplos:
- `Register_WithValidData_ShouldHaveRegisteredStatus`
- `Approve_WhenUnderInvestigation_ShouldChangeStatusToApproved`
- `Appeal_WhenAlreadyAppealed_ShouldThrowBusinessRuleException`

---

## Builder Methods — Patrón Obligatorio

Nunca construir entidades de prueba directamente dentro del body del test. Siempre usar builder methods privados y estáticos:

```csharp
// ✅ Correcto — builder reutilizable con defaults sensatos
private static Policy BuildActivePolicy()
{
    var policy = Policy.Create(
        PolicyNumber.Create(2024, 1),
        PolicyType.Life,
        InsuredPerson.Create("John", "Doe", "CC", "123456789",
            new DateOnly(1990, 1, 1), "Masculino",
            "Calle 123 # 45-67", "Bogotá", "110111", "Cundinamarca"),
        CoveragePeriod.Create(
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)),
        100m, 10_000m);
    policy.Activate();
    return policy;
}

private static CreatePolicyCommand BuildCommand() => new()
{
    Type             = PolicyType.Life,
    InsuredFirstName = "John",
    InsuredLastName  = "Doe",
    // ...
};
```

---

## Reglas Críticas

### CancellationToken en mocks

Siempre usar `Arg.Any<CancellationToken>()` en los setups de NSubstitute — nunca `CancellationToken.None` en el setup:

```csharp
// ✅ Correcto
_repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(entity);

// ❌ Incorrecto — el mock no matcheará si el handler pasa un token diferente
_repo.GetByIdAsync(id, CancellationToken.None).Returns(entity);
```

### Cambios en firmas de repositorio

Cuando se añaden parámetros a una interfaz de repositorio (`IXyzRepository`), **actualizar TODOS los mock setups** en los tests existentes que llamen a ese método. Si se omite un parámetro, el mock no matchea y el test pasa con el valor por defecto (comportamiento silencioso incorrecto).

```csharp
// Antes (firma con 7 parámetros + CT)
_policyRepository.GetAllAsync(null, null, null, null, null, 1, 10, Arg.Any<CancellationToken>())

// Después de añadir insuredSearch + insuredDocumentType (9 parámetros + CT)
_policyRepository.GetAllAsync(null, null, null, null, null, null, null, 1, 10, Arg.Any<CancellationToken>())
```

### Cobertura mínima

Cada handler debe tener al menos:
- ✅ **Happy path** — comando/query válido devuelve resultado correcto
- ❌ **Error path** — recurso no encontrado → `NotFoundException`; regla de negocio violada → `BusinessRuleException`
- 🔲 **Interaction test** — el repositorio y `IUnitOfWork` son llamados el número correcto de veces

---

## Estructura AAA — Comentarios Obligatorios

```csharp
// GIVEN — preparar mocks y datos de entrada
// WHEN  — llamar al handler
// THEN  — verificar resultado o excepción
```

O equivalentemente: `// Arrange`, `// Act`, `// Assert`.

Ambas convenciones son válidas — elegir una y ser consistente dentro del mismo archivo.

---

## Nunca Hacer

- Tests que dependen del orden de ejecución entre sí.
- Llamadas reales a MongoDB (solo mocks de `IXyzRepository`).
- `Thread.Sleep` o `Task.Delay` para sincronización.
- Lógica condicional (`if/else`) dentro del body de un test.
- Constructores de entidades de dominio directamente en el test body — usar builders.
- Omitir `Arg.Any<CancellationToken>()` en setups de NSubstitute.
- Tests que verifican detalles de implementación interna no expuestos por la interfaz pública.

---

> Para reglas de cobertura, pirámide de testing y DoR/DoD de automatización ver `.github/rules/testing.md`.
