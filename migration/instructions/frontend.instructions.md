---
applyTo: "frontend/src/**/*.{ts,html,css}"
---

> **Scope**: Frontend de InsuraTech — Angular 17+, TypeScript, Angular Material, Signals API, componentes standalone.

# Instrucciones para el Frontend de InsuraTech

## Stack

| Tecnología | Uso |
|---|---|
| Angular 17+ | Framework principal — standalone components, Signals API |
| Angular Material | UI components — importados individualmente por componente |
| Angular Router | Lazy loading + guards funcionales |
| HttpClient | Llamadas HTTP — con interceptores funcionales |
| RxJS | Streams para HTTP; Signals para estado local |

---

## Regla de Oro — Arquitectura de Archivos

**NUNCA** usar `template:` o `styles:` inline en el decorador `@Component`.  
**SIEMPRE** tres archivos separados por componente:

```
mi-componente.component.ts    ← solo lógica
mi-componente.component.html  ← solo template
mi-componente.component.css   ← solo estilos
```

```typescript
// ✅ Correcto
@Component({
  selector: 'app-mi-componente',
  standalone: true,
  imports: [...],
  templateUrl: './mi-componente.component.html',
  styleUrl:    './mi-componente.component.css',
})

// ❌ Incorrecto — nunca inline
@Component({
  template: `<div>...</div>`,
  styles: [`.foo { color: red }`],
})
```

---

## Estructura de Carpetas por Feature

```
features/<nombre>/
  core/
    models/
      <nombre>.model.ts          ← interfaces TypeScript (sin lógica)
    service/
      <nombre>.service.ts        ← HttpClient + signals de estado
  ui/
    pages/
      <nombre-pagina>/
        <nombre-pagina>.component.ts
        <nombre-pagina>.component.html
        <nombre-pagina>.component.css
    blocks/
      <nombre-bloque>/           ← sub-componentes reutilizables dentro del feature
        <nombre-bloque>.component.ts
        <nombre-bloque>.component.html
        <nombre-bloque>.component.css
```

**Componentes compartidos** entre features viven en:
```
shared/
  components/
    <nombre>/                   ← PageHeaderComponent, StatusBadgeComponent, etc.
  pipes/                        ← StatusLabelPipe
  directives/                   ← ThousandsSeparatorDirective
```

---

## Componentes Standalone

Todo componente es `standalone: true`. La inyección de dependencias siempre con `inject()`.

```typescript
@Component({
  selector: 'app-xyz-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    PageHeaderComponent,
    StatusBadgeComponent,
  ],
  templateUrl: './xyz-list.component.html',
  styleUrl:    './xyz-list.component.css',
})
export class XyzListComponent implements OnInit {
  private service = inject(XyzCoreService);
  private router  = inject(Router);

  // Estado local con signals
  readonly items   = signal<Xyz[]>([]);
  readonly loading = signal(false);

  ngOnInit() { this.load(); }

  private load() {
    this.loading.set(true);
    this.service.getAll({ page: 1, pageSize: 10 }).subscribe({
      next:  res => { this.items.set(res.items); this.loading.set(false); },
      error: ()  => this.loading.set(false),
    });
  }
}
```

### Signals — API obligatoria

| Caso | Uso |
|---|---|
| Estado mutable local | `signal<T>(valorInicial)` |
| Valor derivado | `computed(() => ...)` |
| Prop de entrada (Angular 17+) | `input<T>()` / `input.required<T>()` |
| Evento de salida | `output<T>()` |
| Estado de solo lectura expuesto | `miSignal.asReadonly()` |

```typescript
// inputs y outputs modernos
readonly id      = input.required<string>();
readonly clicked = output<void>();
```

---

## Servicios

Patrón estándar: `providedIn: 'root'`, `inject(HttpClient)`, signals de estado, métodos HTTP que devuelven `Observable` (sin `.subscribe()` interno).

```typescript
@Injectable({ providedIn: 'root' })
export class XyzCoreService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/xyz`;

  // Estado observable desde el servicio
  readonly items      = signal<Xyz[]>([]);
  readonly loading    = signal(false);
  readonly totalCount = signal(0);

  getAll(filters: XyzFilters) {
    return this.http.get<PagedResult<Xyz>>(this.baseUrl, { params: this.buildParams(filters) });
  }

  getById(id: string) {
    return this.http.get<Xyz>(`${this.baseUrl}/${id}`);
  }

  create(body: CreateXyzRequest) {
    return this.http.post<Xyz>(this.baseUrl, body);
  }

  private buildParams(filters: XyzFilters): HttpParams {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters as Record<string, unknown>)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return params;
  }
}
```

**Regla**: los métodos HTTP del servicio devuelven `Observable` — quien llama hace el `.subscribe()`.  
Nunca hacer `.subscribe()` dentro de un servicio.

---

## Autenticación

`AuthService` es la fuente única de verdad. Nunca crear estado de auth paralelo.

```typescript
// ✅ Consumir signals del servicio
private auth = inject(AuthService);
readonly role        = this.auth.role;         // computed signal
readonly currentUser = this.auth.currentUser;  // readonly signal
readonly token       = this.auth.token;        // computed signal
```

El token JWT se inyecta automáticamente en cada request via `authInterceptor`. Los componentes no deben adjuntar headers manualmente.

```typescript
// ❌ Nunca hacer esto en un componente
{ headers: { Authorization: `Bearer ${this.token}` } }
```

---

## Guards

```typescript
// En app.routes.ts
{
  path: 'clients',
  canActivate: [authGuard, roleGuard('Advisor')],
  loadComponent: () => import('...').then(m => m.ClientsListComponent),
}

// Múltiples roles permitidos
canActivate: [authGuard, roleGuard('Advisor', 'Admin')]
```

- `authGuard` — verifica que exista sesión activa; redirige a `/login` si no.
- `roleGuard(...roles)` — factory function que acepta uno o varios roles; redirige a `/dashboard` si el rol no coincide.

---

## Rutas — Convenciones

Todas las rutas usan `loadComponent` (lazy loading). Se registran en `app.routes.ts` antes del wildcard `**`.

```typescript
{
  path: 'mi-feature',
  canActivate: [authGuard],
  loadComponent: () =>
    import('./features/mi-feature/ui/pages/mi-lista/mi-lista.component')
      .then(m => m.MiListaComponent),
},
```

Rutas existentes de referencia:

| Path | Guard | Componente |
|---|---|---|
| `dashboard` | `authGuard` | `DashboardComponent` |
| `policies` | `authGuard` | `PoliciesContainerComponent` |
| `policies/new` | `authGuard, roleGuard('Advisor','Admin')` | `PolicyCreateComponent` |
| `policies/:id` | `authGuard` | `PolicyDetailComponent` |
| `claims` | `authGuard` | `ClaimsListComponent` |
| `claims/:id` | `authGuard` | `ClaimDetailComponent` |
| `clients` | `authGuard, roleGuard('Advisor')` | `ClientsListComponent` |
| `notifications` | `authGuard` | `NotificationsComponent` |
| `users/profile` | `authGuard` | `AdvisorProfileComponent` |
| `users/team` | `authGuard, roleGuard('Leader')` | `LeaderDashboardComponent` |
| `users/admin` | `authGuard, roleGuard('Admin')` | `AdminPanelComponent` |

---

## Interceptores (no modificar)

Configurados en `app.config.ts`:

| Interceptor | Función |
|---|---|
| `authInterceptor` | Adjunta `Authorization: Bearer <token>`; hace logout ante 401 |
| `idempotencyInterceptor` | Añade `Idempotency-Key` en POST |
| `errorInterceptor` | Manejo global de errores HTTP |

---

## Modelos TypeScript

Solo interfaces — sin lógica, sin clases:

```typescript
// models/xyz.model.ts
export interface Xyz {
  id:        string;
  name:      string;
  status:    XyzStatus;
  createdAt: string;
}

export type XyzStatus = 'Active' | 'Inactive' | 'Pending';

export interface XyzFilters {
  status?:   XyzStatus;
  page:      number;
  pageSize:  number;
}
```

---

## Angular Material — Reglas de Importación

Importar cada módulo individualmente en el array `imports` del componente que lo usa. Nunca importar módulos completos de Material como barrel:

```typescript
// ✅ Correcto — solo lo que el componente usa
imports: [MatTableModule, MatButtonModule, MatIconModule, MatTooltipModule]

// ❌ Incorrecto
imports: [MaterialModule]  // barrel no existe en este proyecto
```

---

## Componentes Shared Disponibles

Antes de crear un componente genérico, verificar si ya existe en `shared/`:

| Componente | Selector | Uso |
|---|---|---|
| `PageHeaderComponent` | `<app-page-header>` | Título de página con breadcrumb |
| `StatusBadgeComponent` | `<app-status-badge>` | Chip de estado con color |
| `LoadingSpinnerComponent` | `<app-loading-spinner>` | Spinner centrado |
| `EmptyStateComponent` | `<app-empty-state>` | Estado vacío con ilustración |
| `StatusLabelPipe` | `{{ status \| statusLabel }}` | Traduce estados a español |

---

## CSS — Convenciones

- Archivo `.css` por componente — sin CSS Modules, sin Tailwind, sin Bootstrap.
- Variables de la paleta de marca en los estilos del componente directamente:
  - Negro: `#111111`
  - Naranja: `#FF6B2C`
- El header usa `position: fixed; top: 0; z-index: 1000` — el contenido principal tiene `padding-top` para compensar.
- Para estilos del panel de `mat-menu` o overlays de Angular Material que se renderizan fuera del host, usar `::ng-deep` con selector de clase aplicado al `<mat-menu class="...">`.

---

## Convenciones de Nombrado

| Elemento | Convención | Ejemplo |
|---|---|---|
| Archivos | `kebab-case` | `claims-list.component.ts` |
| Clases | `PascalCase` | `ClaimsListComponent` |
| Selectores | `app-kebab-case` | `app-claims-list` |
| Signals / propiedades | `camelCase` | `loading`, `totalCount` |
| Métodos | `camelCase` | `loadClaims()`, `onPageChange()` |
| Interfaces | `PascalCase` | `ClaimFilters`, `CreateClaimRequest` |

---

## Agregar un Nuevo Feature — Checklist

```
1. Crear models en features/<nombre>/core/models/<nombre>.model.ts
2. Crear servicio en features/<nombre>/core/service/<nombre>.service.ts
3. Crear page component en features/<nombre>/ui/pages/<nombre>/
   (tres archivos: .ts + .html + .css)
4. Registrar ruta lazy en app.routes.ts con canActivate apropiado
5. Si necesita sub-componentes → ui/blocks/<nombre-bloque>/
```

## Nunca Hacer

- `template:` o `styles:` inline en el decorador (siempre archivos separados).
- `.subscribe()` dentro de un servicio.
- Adjuntar el token JWT manualmente en los componentes.
- Crear estado de autenticación fuera de `AuthService`.
- Usar directivas o pipes que no estén importados en el componente (`(clickOutside)`, etc.).
- `constructor()` para inyección de dependencias — siempre `inject()`.
- Importar `HttpClientModule` en componentes — está provisto globalmente en `app.config.ts`.

---

> Para reglas de testing ver `.github/rules/testing.md`.
