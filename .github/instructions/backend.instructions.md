---
applyTo: "Backend/src/**/*.cs"
---

> **Scope**: Backend de InsuraTech — .NET 8, C#, Clean Architecture, CQRS con MediatR, MongoDB.

# Instrucciones para el Backend de InsuraTech

## Stack y Proyectos

| Proyecto | Responsabilidad |
|---|---|
| `InsuraTech.Domain` | Entidades, interfaces de repositorio (`I*Repository`), excepciones de dominio, value objects |
| `InsuraTech.Application` | Commands, Queries, Handlers (MediatR), DTOs, Validators (FluentValidation) |
| `InsuraTech.Infrastructure` | Implementaciones de repositorios (MongoDB), servicios externos, Unit of Work |
| `InsuraTech.API` | Controllers, Middleware, `Program.cs`, configuración de startup |

## Flujo de una Request

```
Controller → IMediator.Send(Command|Query) → Handler → IRepository → MongoDB
```

- El controlador NO contiene lógica de negocio — solo parsea HTTP y despacha al mediator.
- El handler es el único lugar que orquesta lógica de negocio.
- El repositorio es el único lugar con acceso directo a MongoDB.

---

## CQRS con MediatR

### Estructura de archivos obligatoria

Cada comando o query vive en su propia carpeta con exactamente estos archivos:

```
Application/<Domain>/Commands/<ActionName>/
  <ActionName>Command.cs      ← sealed record : IRequest<TResponse>
  <ActionName>Handler.cs      ← sealed class  : IRequestHandler<TCommand, TResponse>
  <ActionName>Validator.cs    ← sealed class  : AbstractValidator<TCommand>  (si valida)

Application/<Domain>/Queries/<QueryName>/
  <QueryName>Query.cs         ← sealed record : IRequest<TResponse>
  <QueryName>Handler.cs       ← sealed class  : IRequestHandler<TQuery, TResponse>

Application/<Domain>/DTOs/
  <Name>Response.cs           ← sealed class o record con propiedades init
```

### Plantilla Command

```csharp
public sealed record CreateXyzCommand : IRequest<XyzResponse>
{
    public string Name { get; init; } = null!;
    // ...
}
```

### Plantilla Handler

```csharp
public sealed class CreateXyzHandler : IRequestHandler<CreateXyzCommand, XyzResponse>
{
    private readonly IXyzRepository _xyzRepository;
    private readonly IUnitOfWork    _unitOfWork;

    public CreateXyzHandler(IXyzRepository xyzRepository, IUnitOfWork unitOfWork)
    {
        _xyzRepository = xyzRepository;
        _unitOfWork    = unitOfWork;
    }

    public async Task<XyzResponse> Handle(CreateXyzCommand request, CancellationToken cancellationToken)
    {
        // lógica de dominio + persistencia
        var entity = Xyz.Create(request.Name);
        await _xyzRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
}
```

### Plantilla Validator (FluentValidation)

```csharp
public sealed class CreateXyzValidator : AbstractValidator<CreateXyzCommand>
{
    public CreateXyzValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
```

Los validators se registran automáticamente vía `AddApplication()` con el behavior de FluentValidation configurado en Application.

---

## Controllers

```csharp
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class XyzController : ControllerBase
{
    private readonly IMediator _mediator;
    public XyzController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(typeof(XyzResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateXyzRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateXyzCommand { Name = request.Name }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

**Reglas de controllers:**
- Siempre heredar de `ControllerBase`, nunca de `Controller`.
- Siempre `[ApiController]` + `[Authorize]` a nivel de clase; usar `[AllowAnonymous]` o `[Authorize(Roles = "...")]` por acción cuando aplique.
- Decorar cada acción con `[ProducesResponseType]` para todos los códigos HTTP posibles.
- Extraer el `userId` o `role` del JWT con `User.FindFirstValue(ClaimTypes.NameIdentifier)` o `ClaimTypes.Role`.
- Siempre pasar `CancellationToken` a `_mediator.Send()`.
- Nunca exponer `Guid` del advisor directamente en el request body si puede extraerse del JWT.

---

## Repositorios

### Interfaz (Domain)

```
Domain/Interfaces/I<Name>Repository.cs
```

```csharp
public interface IXyzRepository
{
    Task<Xyz?>               GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Xyz>>   GetAllAsync(/* filtros opcionales */, int page, int pageSize, CancellationToken ct = default);
    Task<int>                CountAsync(/* mismos filtros */, CancellationToken ct = default);
    Task                     AddAsync(Xyz entity, CancellationToken ct = default);
    Task                     UpdateAsync(Xyz entity, CancellationToken ct = default);
}
```

### Implementación (Infrastructure)

```
Infrastructure/Persistence/Repositories/<Name>Repository.cs
```

- Usa `MongoDB.Driver` con `IMongoCollection<T>`.
- Filtros compuestos se construyen con `Builders<T>.Filter.And(filters)`.
- Búsqueda case-insensitive: `Builders<T>.Filter.Regex("field", new BsonRegularExpression(value, "i"))`.
- Agregaciones para proyecciones complejas: pipeline `$match → $group → $sort`.
- Nunca acceder a MongoDB desde Application o Domain.

---

## Excepciones de Dominio

Lanzar desde handlers o entidades de dominio — nunca desde controllers:

```csharp
throw new NotFoundException($"Policy '{id}' was not found.");
throw new BusinessRuleException("POLICY_NOT_ACTIVE", "Policy is not active.");
```

El `ExceptionHandlingMiddleware` en `InsuraTech.API` intercepta estas excepciones y devuelve `ProblemDetails` con el código HTTP apropiado. No usar `try/catch` en controllers para flujos de negocio esperados.

---

## Convenciones de Código

- **Nomenclatura**: PascalCase para clases, métodos, propiedades; `_camelCase` para campos privados.
- **Tipos**: Usar `sealed` en clases concretas que no se van a heredar.
- **Registros**: Preferir `sealed record` para Commands, Queries y DTOs inmutables.
- **Nulos**: El proyecto tiene `<Nullable>enable</Nullable>` — sin `!` innecesarios; usar `?` explícito.
- **Async**: Todos los métodos que tocan la DB son `async Task<T>` y reciben `CancellationToken`.
- **Enums en JSON**: Se serializan como strings (`JsonStringEnumConverter` configurado en Program.cs).
- **Ruta base**: `api/v1/[controller]` — no cambiar el patrón de versionado.

---

## Agregar un Nuevo Feature — Checklist

```
1. Domain:    Entidad o value object en InsuraTech.Domain/<Domain>/
2. Domain:    Interfaz I<Name>Repository en Domain/Interfaces/
3. Infra:     Implementar <Name>Repository en Infrastructure/Persistence/Repositories/
4. App:       Command/Query + Handler + Validator en Application/<Domain>/Commands|Queries/<Name>/
5. App:       DTO Response en Application/<Domain>/DTOs/
6. API:       Acción en el controlador correspondiente (o nuevo controller si aplica)
7. Tests:     Test unitario del Handler con repositorio mockeado (NSubstitute)
```

## Nunca Hacer

- Lógica de negocio en controllers o repositorios.
- Acceso directo a MongoDB desde Application o Domain.
- Inyectar `IMongoDatabase` fuera de Infrastructure.
- Omitir `CancellationToken` en métodos async.
- Lanzar excepciones genéricas (`Exception`, `ApplicationException`) desde el dominio.
- Usar `await` dentro de un `.Select()` de LINQ — materializar con `.ToListAsync()` o usar bucles.

---

> Para reglas de testing ver `.github/rules/testing.md`.
> Para convenciones de API REST, seguridad y observabilidad ver `.github/docs/lineamientos/dev-guidelines.md`.
