# RETO TÉCNICO

## Sistema de Gestión de Pólizas y Siniestros

> **Microservices · C# .NET 8 · DDD · Event-Driven**

| Campo | Detalle |
|---|---|
| **Nivel** | Semi Senior |
| **Duración** | 5 días hábiles |
| **Industria** | Aseguradora |

**Stack:** `.NET 8` · `Clean Architecture` · `CQRS + MediatR` · `Event Sourcing` · `Docker` · `xUnit`

---

## 1. Contexto del Negocio

Eres parte del equipo de tecnología de **InsuraTech S.A.**, una aseguradora digital en crecimiento que necesita modernizar su plataforma core. Actualmente operan con un monolito legacy que genera cuellos de botella al procesar pólizas y gestionar siniestros durante picos de carga (fin de mes, catástrofes naturales, campañas comerciales).

Tu misión es construir el núcleo de un sistema moderno, escalable y resiliente que soporte:

- Emisión y gestión del ciclo de vida de pólizas de seguros (vida, auto, hogar).
- Registro y seguimiento de siniestros con validación de cobertura.
- Notificaciones en tiempo real a asegurados y agentes.
- Trazabilidad completa de eventos para auditoría regulatoria.

> ⚠️ **Restricción de alcance**
>
> No se requiere UI. El entregable es una **API REST robusta y bien documentada**, acompañada de pruebas automatizadas que demuestren su correcto funcionamiento.

---

## 2. Requerimientos Funcionales

### 2.1 Módulo de Pólizas (Policy Service)

1. **Crear una póliza** con los siguientes datos obligatorios:
   - Número de póliza (generado automáticamente, formato: `POL-YYYY-XXXXXXXX`)
   - Tipo de seguro: Vida, Auto, Hogar
   - Asegurado: nombre, documento de identidad, fecha de nacimiento
   - Vigencia: fecha inicio, fecha fin (mínimo 30 días, máximo 5 años)
   - Prima mensual y suma asegurada
   - Estado inicial: `Pendiente` → `Activa` (requiere pago confirmado)

2. **Gestionar el ciclo de vida completo:**
   - Activar póliza (al registrar pago)
   - Suspender póliza (mora en pagos o solicitud del asegurado)
   - Cancelar póliza (con motivo y fecha efectiva)
   - Renovar póliza (genera nueva póliza vinculada a la anterior)

3. **Consultas:**
   - Listar pólizas con filtros por estado, tipo, asegurado, rango de fechas
   - Obtener detalle completo de una póliza incluyendo historial de estados

### 2.2 Módulo de Siniestros (Claims Service)

4. **Registrar un siniestro:**
   - Validar que la póliza esté activa y dentro de vigencia
   - Clasificar por tipo: Robo, Accidente, Enfermedad, Incendio, Otro
   - Monto reclamado no puede superar la suma asegurada de la póliza
   - Fecha del siniestro no puede ser anterior a la vigencia de la póliza

5. **Flujo de aprobación:**
   - Estados: `Registrado` → `En Investigación` → `Aprobado / Rechazado` → `Pagado`
   - Cada transición de estado debe registrar usuario responsable, fecha y observaciones
   - Un siniestro rechazado puede ser apelado **una sola vez**

6. **Regla de negocio crítica:**

> 📌 **Regla de Acumulación de Siniestros**
>
> - Una póliza activa no puede tener más de **3 siniestros abiertos** simultáneamente.
> - Al aprobar un siniestro, la suma asegurada disponible debe decrementarse.
> - Si la suma asegurada disponible llega a **$0**, la póliza pasa a estado **Agotada**.

### 2.3 Módulo de Notificaciones (Notification Service)

Implementar un mecanismo de notificación desacoplado basado en eventos de dominio:

| Evento | Acción |
|---|---|
| `PolicyActivated` | Notificar al asegurado con detalles de cobertura |
| `PolicyExpiringSoon` | Alertar 30 y 7 días antes del vencimiento |
| `ClaimRegistered` | Confirmar recepción al asegurado |
| `ClaimStatusChanged` | Informar cambios de estado al asegurado y al agente |

Las notificaciones deben almacenarse en base de datos con estado de envío (`Pendiente`, `Enviado`, `Fallido`) para garantizar _at-least-once delivery_. No se requiere integración real con email/SMS.

---

## 3. Requerimientos Técnicos y Arquitecturales

### 3.1 Stack Tecnológico Obligatorio

| Categoría | Tecnología requerida |
|---|---|
| **Runtime** | .NET 8 (C# 12) — sin excepciones |
| **Arquitectura** | Clean Architecture o Hexagonal (Ports & Adapters) |
| **Patrón CQRS** | MediatR — Commands y Queries separadas |
| **ORM / Persistencia** | Entity Framework Core 8 con Code-First Migrations |
| **Base de datos** | SQL Server o PostgreSQL (dockerizada) |
| **Mensajería** | RabbitMQ o MassTransit (para eventos de dominio) |
| **Documentación API** | Swagger / OpenAPI 3.0 con ejemplos de request/response |
| **Contenedores** | Docker + docker-compose (levanta toda la solución) |
| **Testing** | xUnit + FluentAssertions + Moq / NSubstitute |
| **Validación** | FluentValidation |

### 3.2 Principios de Diseño Exigidos

- **Domain-Driven Design:** Entidades, Value Objects, Aggregates y Domain Events bien definidos.
- **SOLID:** Principios aplicados y evidenciables en la revisión de código.
- **Repository Pattern:** Abstracciones de repositorios en el dominio, implementaciones en infraestructura.
- **Idempotencia:** Los endpoints de creación deben ser idempotentes usando `Idempotency-Key` en headers.
- **Soft Delete:** Las entidades principales (pólizas, siniestros) no se eliminan físicamente.
- **Optimistic Concurrency:** Control de versión en agregados para evitar condiciones de carrera.
- **Correlation ID:** Propagación de un identificador de correlación en todos los logs y eventos.

### 3.3 Estructura de Proyecto Esperada

```
InsuraTech.sln
├── src/
│   ├── InsuraTech.Domain/          # Entidades, VOs, Domain Events, Interfaces
│   ├── InsuraTech.Application/     # Use Cases (Commands/Queries), DTOs, Validators
│   ├── InsuraTech.Infrastructure/  # EF Core, Repos, Messaging, External Services
│   └── InsuraTech.API/             # Controllers, Middleware, Program.cs
├── tests/
│   ├── InsuraTech.Domain.Tests/        # Unit tests del dominio
│   ├── InsuraTech.Application.Tests/   # Unit tests de use cases
│   └── InsuraTech.Integration.Tests/   # Tests de integración con BD real
├── docker-compose.yml
└── README.md
```

---

## 4. Criterios de Aceptación y Rúbrica de Evaluación

La evaluación total es sobre **100 puntos**. El candidato debe obtener **mínimo 70 puntos** para considerar el reto aprobado. Se requiere un **mínimo de 15 puntos** en la categoría de Testing.

| Criterio | Descripción | Peso |
|---|---|---|
| **Arquitectura y Diseño** | Clean Architecture correctamente capada, sin dependencias invertidas. Domain completamente aislado (sin referencias a Infrastructure). Value Objects inmutables. Aggregates con invariantes de negocio encapsuladas. | **25 pts** |
| **Lógica de Negocio** | Todas las reglas de negocio implementadas y validadas: ciclo de vida de pólizas, regla de acumulación de siniestros, transiciones de estado con guards, deducción de suma asegurada disponible. | **20 pts** |
| **Testing** | Cobertura mínima del 80% en Domain y Application. Al menos: 15 unit tests de dominio, 10 unit tests de use cases con mocks, 5 integration tests de flujo completo (crear póliza → activar → registrar siniestro → aprobar). | **20 pts** |
| **Calidad de Código** | Código limpio, sin code smells evidentes. Métodos con responsabilidad única. Nombres expresivos. Sin magic strings/numbers. Ausencia de comentarios redundantes. Manejo de excepciones con tipos de error de dominio. | **15 pts** |
| **API y Contratos** | Swagger completo con ejemplos. Códigos HTTP semánticamente correctos. Manejo de errores con Problem Details (RFC 7807). Paginación en listados. Validación de entradas con FluentValidation. | **10 pts** |
| **Mensajería y Eventos** | Domain Events publicados correctamente. Outbox Pattern o mecanismo equivalente para garantizar entrega de eventos. Handlers de notificaciones desacoplados y funcionales. | **10 pts** |

> 🔴 **Descalificadores automáticos**
>
> - Lógica de negocio en Controllers o en la capa de Infrastructure.
> - Uso de stored procedures o SQL crudo para reglas de negocio.
> - Ausencia total de pruebas unitarias.
> - El proyecto no compila o no levanta con `docker-compose up`.
> - Credenciales hardcodeadas (passwords, connection strings en código fuente).
> - Copiar/pegar proyectos existentes de GitHub sin adaptación al dominio asegurador.

---

## 5. Endpoints Mínimos Requeridos

### 5.1 Pólizas

```
POST   /api/v1/policies              # Crear póliza
GET    /api/v1/policies              # Listar pólizas (filtros + paginación)
GET    /api/v1/policies/{id}         # Obtener detalle
PUT    /api/v1/policies/{id}/activate  # Activar póliza
PUT    /api/v1/policies/{id}/suspend   # Suspender póliza
PUT    /api/v1/policies/{id}/cancel    # Cancelar póliza
POST   /api/v1/policies/{id}/renew    # Renovar póliza
```

### 5.2 Siniestros

```
POST   /api/v1/claims                   # Registrar siniestro
GET    /api/v1/claims/{id}              # Obtener siniestro
GET    /api/v1/policies/{id}/claims     # Siniestros de una póliza
PUT    /api/v1/claims/{id}/investigate  # Iniciar investigación
PUT    /api/v1/claims/{id}/approve      # Aprobar siniestro
PUT    /api/v1/claims/{id}/reject       # Rechazar con motivo
PUT    /api/v1/claims/{id}/appeal       # Apelar rechazo
PUT    /api/v1/claims/{id}/pay          # Registrar pago
```

### 5.3 Notificaciones

```
GET    /api/v1/notifications            # Listar notificaciones con filtro por estado
PUT    /api/v1/notifications/{id}/retry # Reintentar envío fallido
```

---

## 6. Casos de Prueba Obligatorios

### Dominio — Pólizas

- Una póliza recién creada debe tener estado `Pendiente`.
- Activar una póliza en estado `Pendiente` debe cambiar su estado a `Activa` y emitir `PolicyActivated`.
- Intentar cancelar una póliza ya cancelada debe lanzar una excepción de dominio.
- La suma asegurada debe ser mayor a $0 y la prima mensual no puede superar el 5% de la suma asegurada.

### Dominio — Siniestros

- Registrar un siniestro en póliza vencida debe lanzar `ClaimOnExpiredPolicyException`.
- El monto reclamado no puede superar la suma asegurada disponible.
- La cuarta solicitud de siniestro sobre una póliza con 3 abiertos debe ser rechazada.
- Aprobar un siniestro debe descontar el monto aprobado de la suma asegurada disponible.
- Si la suma disponible llega a $0 al aprobar, la póliza debe pasar a estado `Agotada`.

### Integración

- **Flujo completo:** Crear póliza → Activar → Registrar siniestro → Aprobar → Verificar suma asegurada decrementada.
- **Flujo de rechazo y apelación:** Rechazar siniestro → Apelar → Aprobar apelación.
- **Idempotencia:** Enviar mismo request con igual `Idempotency-Key` dos veces debe retornar el mismo resultado sin duplicar datos.

---

## 7. Puntos Bonus (No Obligatorios)

Estos puntos no compensan deficiencias en los criterios principales, pero demuestran madurez técnica:

| Implementación Bonus | Puntos Extra |
|---|---|
| Event Sourcing: almacenar eventos de dominio como fuente de verdad de los agregados. | **+10** |
| Health Checks: endpoints `/health` y `/ready` con verificación de dependencias (BD, RabbitMQ). | **+5** |
| Rate Limiting: proteger endpoints críticos con límite de solicitudes por IP/cliente. | **+5** |
| Observabilidad: integración con OpenTelemetry y exportación de trazas distribuidas. | **+10** |
| Arquitectura Multi-tenant: soporte para múltiples aseguradoras en la misma plataforma. | **+10** |

---

## 8. Instrucciones de Entrega

### 8.1 Formato

- Repositorio Git público (GitHub, GitLab o Bitbucket) con historial de commits representativo.
- Los commits deben mostrar progreso incremental — un único commit con todo el código es motivo de penalización.
- Incluir archivo `README.md` completo (ver sección 8.2).

### 8.2 README.md Obligatorio

El README debe contener, como mínimo:

1. Descripción de la arquitectura adoptada y decisiones de diseño importantes.
2. Diagrama de componentes o capas (puede ser ASCII art o imagen).
3. Instrucciones para levantar el entorno completo:

```bash
git clone <repo>
cd InsuraTech
docker-compose up --build
# API disponible en http://localhost:5000
# Swagger en http://localhost:5000/swagger
```

4. Cómo ejecutar las pruebas y visualizar cobertura:

```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"
```

5. Supuestos y decisiones tomadas durante el desarrollo.
6. Listado de mejoras que implementaría con más tiempo.

### 8.3 Checklist Final del Candidato

Antes de enviar, verificar:

- [ ] `docker-compose up` levanta sin errores
- [ ] Swagger accesible y con ejemplos en todos los endpoints
- [ ] `dotnet test` pasa sin fallos
- [ ] Cobertura de Domain + Application ≥ 80%
- [ ] Ninguna credencial hardcodeada en el código
- [ ] README completo con instrucciones claras
- [ ] Historial de Git con commits incrementales
- [ ] Regla de acumulación de 3 siniestros implementada y probada
- [ ] Idempotencia verificada con `Idempotency-Key`

---

## 9. Notas para el Evaluador

Durante la revisión técnica se valorará especialmente:

- Que el candidato sepa explicar cada decisión de arquitectura y sus trade-offs.
- La capacidad de argumentar por qué eligió determinado patrón sobre alternativas.
- Cómo manejaría el crecimiento de la solución: más tipos de póliza, más reglas de negocio, más siniestros concurrentes.
- Conocimiento de las implicaciones de consistencia eventual en sistemas event-driven.

Se recomienda agendar una **sesión de code review de 60 minutos** donde el candidato:

1. Explique el flujo completo desde un endpoint hasta la base de datos.
2. Demuestre en vivo la ejecución de los tests de integración.
3. Responda: _¿Cómo escalarías este sistema para manejar 100.000 pólizas activas?_

---

_InsuraTech S.A. · Equipo de Ingeniería · Proceso de Selección Técnica — Confidencial_
