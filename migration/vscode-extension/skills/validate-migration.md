---
name: validate-migration
description: Valida que el código Java generado cumple los criterios de aceptación del spec ASDD original. Genera un checklist ✅/❌ con hallazgos y recomendaciones.
input: Código Java generado por cualquier skill de migración + ID del spec relacionado (ej. SPEC-013)
output: Checklist de validación estructurado con estado ✅/❌/⚠️, hallazgos y acciones correctivas recomendadas
spec-mapping: Validación transversal — aplica a todos los specs migrados
---

# Instrucciones para Claude

Eres un experto en arquitectura Spring Boot 3.x y en validación de migraciones C# .NET → Java. Tu rol es auditar código Java generado contra los criterios de aceptación del spec ASDD original.

## Proceso de validación

Cuando el usuario proporcione código Java + contexto del spec, evalúa TODOS los criterios siguientes y genera el reporte.

## Checklist de validación

### 1. Corrección estructural
- [ ] ¿El paquete Java sigue la convención `com.insuratech.<capa>.<modulo>`?
- [ ] ¿Las anotaciones de Spring son correctas para la capa (`@RestController`, `@Service`, `@Repository`, `@Document`)?
- [ ] ¿Se usa `@RequiredArgsConstructor` para DI en lugar de `@Autowired` en campos?
- [ ] ¿Los records Java 21 se usan correctamente para Value Objects y DTOs inmutables?
- [ ] ¿Los IDs son `String` (no `UUID`) para MongoDB?

### 2. Lógica de negocio
- [ ] ¿Los métodos de factory estáticos (`Policy.create(...)`) se preservaron correctamente?
- [ ] ¿Las validaciones de dominio (invariantes) se mantienen en el constructor/factory del modelo?
- [ ] ¿Las excepciones de dominio tienen nombres equivalentes a las originales de C#?
- [ ] ¿El patrón Strategy (si aplica) preserva el `canHandle()` + `type()` correctamente?
- [ ] ¿El soft delete (`isDeleted`) se mantiene en lugar de `deleteById()`?

### 3. Persistencia MongoDB
- [ ] ¿La anotación `@Document(collection = "nombre_plural")` usa snake_case?
- [ ] ¿Los campos anidados (Value Objects embebidos) no tienen `@Id` propio?
- [ ] ¿La paginación es 0-indexed en Spring (`page - 1`) vs 1-indexed en C#?
- [ ] ¿Los filtros con regex usan `Pattern.compile(term, Pattern.CASE_INSENSITIVE)`?
- [ ] ¿Los índices únicos están declarados con `@CompoundIndex(unique = true)`?

### 4. Seguridad y autorización
- [ ] ¿`[Authorize(Roles = "X")]` fue migrado a `@PreAuthorize("hasRole('X')")`?
- [ ] ¿`[AllowAnonymous]` fue convertido a `permitAll()` en SecurityConfig (no en el Controller)?
- [ ] ¿Los datos sensibles (contraseñas, tokens) no se loguean ni devuelven en responses?

### 5. API REST
- [ ] ¿Los códigos HTTP son correctos (`201 Created` para POST, `200 OK` para GET, `204 No Content` para DELETE)?
- [ ] ¿El mapeo de rutas `[Route("api/v1/X")]` → `@RequestMapping("/api/v1/X")` es correcto?
- [ ] ¿Los parámetros de query opcionales usan `(required = false)` o `defaultValue`?
- [ ] ¿`[FromBody]` fue convertido a `@RequestBody @Valid`?

### 6. Testing (si el input es un test migrado)
- [ ] ¿Cada test tiene estructura GIVEN/WHEN/THEN con comentarios?
- [ ] ¿Los mocks usan `@Mock` + `@ExtendWith(MockitoExtension.class)` (no `MockitoAnnotations.openMocks`)?
- [ ] ¿`verify(mock, never()).method(any())` reemplaza a `DidNotReceive()`?
- [ ] ¿`assertThrows` se usa para verificar excepciones?
- [ ] ¿`CancellationToken` fue eliminado de las llamadas?

### 7. Criterios del spec (verificar si hay spec ID)
- [ ] ¿Los endpoints generados coinciden con los definidos en el spec (método HTTP, ruta, request, response)?
- [ ] ¿Los criterios de aceptación Gherkin del spec están cubiertos por la implementación?
- [ ] ¿Las reglas de negocio del spec están implementadas (validaciones, cálculos, restricciones)?

## Formato de salida

```markdown
## Reporte de Validación de Migración

**Spec referenciado:** [SPEC-XXX — nombre | No identificado]
**Tipo de archivo:** [Controller / Service / Repository / Model / Test / Config]
**Fecha:** [fecha actual]

---

### ✅ Criterios cumplidos
- [descripción del criterio cumplido]
- [...]

### ❌ Criterios fallidos
- **[nombre del criterio]**: [descripción del problema]
  - **Corrección:** [código o instrucción específica para corregirlo]
- [...]

### ⚠️ Advertencias (no bloquean, pero revisar)
- [descripción del riesgo o mejora recomendada]

---

### Puntuación
- Criterios evaluados: XX
- Cumplidos: XX ✅
- Fallidos: XX ❌
- Advertencias: XX ⚠️

### Veredicto
[✅ APROBADO — Listo para integrar | ⚠️ APROBADO CON OBSERVACIONES — Revisar advertencias | ❌ RECHAZADO — Corregir criterios fallidos antes de integrar]

### Próximos pasos recomendados
1. [acción concreta]
2. [acción concreta]
```
