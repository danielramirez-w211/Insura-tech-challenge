---
id: SPEC-013
status: IN_PROGRESS
feature: authentication-user-system
created: 2026-04-15
updated: 2026-04-15
approved: 2026-04-15
author: spec-generator
version: "1.0"
related-specs: [SPEC-014]
---

# Spec: Sistema de Autenticación y Autorización (JWT)

## Resumen Ejecutivo

Implementar la fundación de seguridad de InsuraTech: autenticación JWT, tres roles de usuario
(Admin, Leader, Advisor) y protección de todos los endpoints existentes. Es prerequisito para
SPEC-014 (perfiles y gestión de usuarios).

**Estado actual:** todos los endpoints son públicos, sin autenticación ni autorización.

---

## 1. REQUERIMIENTOS

### 1.1 Historias de Usuario

#### HU-1: Login de usuario

```
Como: Usuario registrado (Admin, Leader o Advisor)
Quiero: Ingresar mis credenciales (email + contraseña) y obtener un token JWT
Para: Acceder a los recursos protegidos de la plataforma según mi rol

Prioridad: Alta | Estimación: M | Capa: Ambas
```

**Happy Path**
```gherkin
Dado que: el usuario existe en el sistema con email "advisor@test.com" y contraseña "Temp1234"
Cuando: POST /api/v1/auth/login con { "email": "advisor@test.com", "password": "Temp1234" }
Entonces: el sistema responde HTTP 200 con { "token": "eyJ...", "role": "Advisor", "name": "...", "expiresAt": "..." }
```

**Error Path**
```gherkin
Dado que: el usuario intenta login con credenciales incorrectas
Cuando: POST /api/v1/auth/login con contraseña errónea
Entonces: el sistema responde HTTP 401 con mensaje "Credenciales inválidas"
```

#### HU-2: Protección de endpoints existentes

```
Como: Sistema
Quiero: Que todos los endpoints requieran token JWT válido
Para: Garantizar que solo usuarios autenticados operen la plataforma
```

```gherkin
Dado que: un cliente hace GET /api/v1/policies sin token Bearer
Cuando: el middleware de autenticación evalúa la petición
Entonces: el sistema responde HTTP 401 Unauthorized
```

#### HU-3: Seed de Admin inicial

```gherkin
Dado que: el backend inicia sin ningún usuario en la colección "users"
Cuando: se completa el startup
Entonces: se crea automáticamente admin@insuratech.com / Admin123! con rol Admin
```

---

### 1.2 Reglas de Negocio

| ID | Regla |
|----|-------|
| RN-01 | Token JWT expira en 8 horas |
| RN-02 | Contraseñas hasheadas con BCrypt |
| RN-03 | Solo Admin puede acceder a endpoints de gestión de líderes (SPEC-014) |
| RN-04 | Solo Leader puede ver pólizas de sus asesores y aprobar siniestros |
| RN-05 | Solo Advisor puede crear pólizas y siniestros |
| RN-06 | Plans endpoints (health, life, vehicle, home, travel) accesibles para Advisor y Leader |
| RN-07 | Un usuario inactivo (isActive=false) recibe 401 aunque su contraseña sea correcta |

---

## 2. DISEÑO

### 2.1 MongoDB — colección `users`

```json
{
  "_id": "guid-string",
  "email": "advisor@test.com",
  "passwordHash": "$2a$...",
  "role": "Advisor",
  "isActive": true,
  "advisorCode": "0001",
  "createdAt": "2026-04-15T00:00:00Z",
  "lastLoginAt": null
}
```

### 2.2 API Endpoints

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/v1/auth/login` | Pública | Autenticar y obtener JWT |

### 2.3 JWT Config (appsettings.json)

```json
"Jwt": {
  "Secret": "insuratech-super-secret-key-2026-min-32chars!!",
  "Issuer": "InsuraTech",
  "Audience": "InsuraTech.Client",
  "ExpirationHours": 8
}
```

---

## 3. LISTA DE TAREAS

### Backend
- [x] Domain: `User.cs`, `Role.cs` enum, `IUserRepository.cs`
- [x] Infrastructure: `UserDocument.cs`, `UserRepository.cs`, `JwtService.cs`
- [x] Application: `LoginCommand.cs`, `LoginHandler.cs`, DTOs
- [x] API: `AuthController.cs`, Program.cs JWT middleware, seed Admin
- [x] Proteger todos los controllers con `[Authorize]`

### Frontend
- [x] `AuthService`: login(), logout(), token persistido en localStorage
- [x] `AuthGuard`, `RoleGuard`
- [x] `LoginComponent`: conectar submit
- [x] `app.routes.ts`: quitar TODOs, aplicar guards
