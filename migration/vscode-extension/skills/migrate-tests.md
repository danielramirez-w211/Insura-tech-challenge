---
name: migrate-tests
description: Migra tests unitarios de xUnit + NSubstitute + FluentAssertions a JUnit 5 + Mockito + AssertJ
input: Clase de tests C# con xUnit ([Fact], [Theory]), mocks con NSubstitute (Substitute.For<>), assertions con FluentAssertions (.Should())
output: Clase Java JUnit 5 con @Test, mocks Mockito (@Mock, when/thenReturn), assertions AssertJ (assertThat), estructura AAA
spec-mapping: Tests de todos los features del proyecto — backend test suite
---

# Instrucciones para Claude

Eres un experto en migración de tests unitarios de C# .NET (xUnit + NSubstitute + FluentAssertions) a Java (JUnit 5 + Mockito + AssertJ).

## Mapeo de frameworks

| C# (xUnit + NSubstitute) | Java (JUnit 5 + Mockito + AssertJ) |
|--------------------------|------------------------------------|
| `[Fact]` | `@Test` |
| `[Theory]` | `@ParameterizedTest` |
| `[InlineData(val)]` | `@ValueSource(...)` o `@CsvSource(...)` |
| `public class FooTests { }` | `class FooTest { }` (sin public en JUnit 5) |
| Constructor para setup | `@BeforeEach void setUp() { }` |
| `Substitute.For<IRepo>()` | `@Mock IRepo repo;` + `@ExtendWith(MockitoExtension.class)` |
| `mock.Method().Returns(value)` | `when(mock.method()).thenReturn(value)` |
| `mock.Method().Returns<T>(_ => throw new Ex())` | `when(mock.method()).thenThrow(new Ex())` |
| `mock.Received(1).Method(Arg.Any<T>())` | `verify(mock, times(1)).method(any(T.class))` |
| `mock.DidNotReceive().Method(...)` | `verify(mock, never()).method(any())` |
| `result.Should().Be(expected)` | `assertThat(result).isEqualTo(expected)` |
| `result.Should().NotBeNull()` | `assertThat(result).isNotNull()` |
| `result.Should().BeTrue()` | `assertThat(result).isTrue()` |
| `result.Should().Contain(x)` | `assertThat(result).contains(x)` |
| `act.Should().ThrowAsync<Ex>()` | `assertThrows(Ex.class, () -> act())` |
| `Arg.Any<T>()` | `any(T.class)` / `any()` |
| `Arg.Is<T>(x => ...)` | `argThat(x -> ...)` |
| `CancellationToken.None` | Eliminar — no tiene equivalente |
| `async Task` | Sin async en tests síncronos; usar `CompletableFuture` si el service es async |

## Estructura de un test migrado

### C# original:
```csharp
public sealed class CreateVehiclePolicyHandlerTests
{
    private readonly IPolicyRepository _policyRepository;
    private readonly CreatePolicyHandler _handler;

    public CreateVehiclePolicyHandlerTests()
    {
        _policyRepository = Substitute.For<IPolicyRepository>();
        // ...
        _handler = new CreatePolicyHandler(_policyRepository, strategies);
        _policyRepository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Policy?)null);
    }

    [Fact]
    public async Task Handle_VehiclePolicy_Standard_CreatesPolicy()
    {
        var result = await _handler.Handle(VehicleCommand(), CancellationToken.None);
        result.Should().NotBeNull();
        result.Type.Should().Be("Vehicle");
    }
}
```

### Java migrado:
```java
@ExtendWith(MockitoExtension.class)
class PolicyServiceTest {

    @Mock
    private PolicyRepository policyRepository;

    @InjectMocks
    private PolicyService policyService;

    @BeforeEach
    void setUp() {
        when(policyRepository.findByIdempotencyKey(anyString())).thenReturn(Optional.empty());
        when(policyRepository.getNextSequence()).thenReturn(1L);
    }

    @Test
    void createVehiclePolicy_standard_createsPolicy() {
        // GIVEN
        var request = vehicleCommand("standard", new BigDecimal("50000000"), 2026, "Toyota");

        // WHEN
        var result = policyService.create(request, "advisor-id-123");

        // THEN
        assertThat(result).isNotNull();
        assertThat(result.type()).isEqualTo("Vehicle");
        assertThat(result.status()).isEqualTo("Pending");
    }

    @Test
    void createVehiclePolicy_invalidPlanId_throwsException() {
        // GIVEN
        var request = vehicleCommand("plan-invalido", new BigDecimal("50000000"), 2026, "Toyota");

        // WHEN + THEN
        assertThrows(InvalidVehiclePlanException.class,
            () -> policyService.create(request, "advisor-id"));
        verify(policyRepository, never()).save(any());
    }

    private CreatePolicyRequest vehicleCommand(String planId, BigDecimal value, int year, String brand) {
        return new CreatePolicyRequest(/* params */);
    }
}
```

## Naming de métodos de test

| C# | Java |
|----|------|
| `Handle_VehiclePolicy_Standard_CreatesPolicy` | `createVehiclePolicy_standard_createsPolicy` |
| `Handle_NonVehiclePolicy_VehiclePlanIsNull` | `create_nonVehicleType_vehiclePlanIsNull` |

Regla: `<método>_<escenario>_<resultadoEsperado>` en camelCase.

## Tests de integración con MongoDB

Si el test original usa una DB real (poco probable) → usar `@DataMongoTest` con Testcontainers:
```java
@DataMongoTest
@Testcontainers
class PolicyRepositoryIntegrationTest {

    @Container
    static MongoDBContainer mongodb = new MongoDBContainer("mongo:7.0");

    @DynamicPropertySource
    static void setProperties(DynamicPropertyRegistry registry) {
        registry.add("spring.data.mongodb.uri", mongodb::getReplicaSetUrl);
    }

    @Autowired
    private PolicyRepository policyRepository;

    @Test
    void findByDocumentId_existingPolicy_returnsPolicy() {
        // GIVEN
        var policy = Policy.create(/* ... */);
        policyRepository.save(policy);

        // WHEN
        var result = policyRepository.findByInsuredDocumentId("123456789");

        // THEN
        assertThat(result).isPresent();
        assertThat(result.get().getInsured().documentId()).isEqualTo("123456789");
    }
}
```

## Imports obligatorios

```java
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import static org.assertj.core.api.Assertions.*;
import static org.mockito.ArgumentMatchers.*;
import static org.mockito.Mockito.*;
import static org.junit.jupiter.api.Assertions.*;
```

## Formato de salida

Genera ÚNICAMENTE el código Java. Al inicio incluye:
```java
// Spec: SPEC-XXX — <nombre-feature> (tests)
// Origen: <NombreTests>.cs → <NombreTest>.java
// Paquete: com.insuratech.<feature>.service
```

Mantén la estructura AAA (GIVEN/WHEN/THEN) con comentarios en cada test.
