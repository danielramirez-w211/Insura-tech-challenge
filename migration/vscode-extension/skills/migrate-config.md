---
name: migrate-config
description: Migra DependencyInjection.cs, appsettings.json y Program.cs a @Configuration, application.yml y @SpringBootApplication
input: Archivo de configuración C# — DependencyInjection.cs, appsettings.json, Program.cs o Middleware class
output: Clases @Configuration con @Bean, archivo application.yml y clase principal @SpringBootApplication
spec-mapping: Configuración de infraestructura del proyecto — afecta todos los specs
---

# Instrucciones para Claude

Eres un experto en migración de configuración de .NET 8 a Spring Boot 3.x.

## Mapeo de archivos

| C# | Java Spring Boot |
|----|-----------------|
| `DependencyInjection.cs` → `services.AddScoped<IRepo, Repo>()` | `@Configuration` + `@Bean` (o simplemente `@Repository`/`@Service` + component scan) |
| `appsettings.json` | `application.yml` |
| `appsettings.Development.json` | `application-dev.yml` |
| `Program.cs` | `@SpringBootApplication` main class + `SecurityConfig.java` |
| `Middleware/ExceptionMiddleware.cs` | `@RestControllerAdvice GlobalExceptionHandler.java` |
| `Middleware/AuthMiddleware.cs` | `SecurityFilterChain` en `SecurityConfig.java` |

## DependencyInjection.cs → @Configuration

En Spring Boot, la mayoría de los servicios se registran con anotaciones `@Service`, `@Repository`, `@Component` — no necesitan `@Bean` explícito. Solo necesitas `@Bean` para:
- Configuraciones externas (MongoClient, JwtDecoder, etc.)
- Beans con parámetros de construcción especiales
- Colecciones de estrategias (equivalente al `IEnumerable<IStrategy>`)

```java
@Configuration
@RequiredArgsConstructor
public class InfrastructureConfig {

    @Value("${mongodb.uri}")
    private String mongoUri;

    @Value("${mongodb.database}")
    private String databaseName;

    @Bean
    public MongoClient mongoClient() {
        return MongoClients.create(mongoUri);
    }

    @Bean
    public MongoDatabaseFactory mongoDatabaseFactory(MongoClient client) {
        return new SimpleMongoClientDatabaseFactory(client, databaseName);
    }
}
```

## appsettings.json → application.yml

```yaml
# application.yml
spring:
  application:
    name: insuratech-api
  data:
    mongodb:
      uri: ${MONGODB_URI:mongodb://localhost:27017}
      database: ${MONGODB_DATABASE:insuratech}

server:
  port: ${PORT:8080}

jwt:
  secret: ${JWT_SECRET}
  expiration: ${JWT_EXPIRATION:86400}

firebase:
  credentials-path: ${FIREBASE_CREDENTIALS_PATH}

logging:
  level:
    com.insuratech: DEBUG
    org.springframework.data.mongodb: INFO
```

## Program.cs → @SpringBootApplication

```java
// Program.cs → InsuraTechApplication.java
@SpringBootApplication
@EnableMongoRepositories(basePackages = "com.insuratech.infrastructure.persistence")
public class InsuraTechApplication {
    public static void main(String[] args) {
        SpringApplication.run(InsuraTechApplication.class, args);
    }
}
```

## SecurityConfig (equivalente al middleware de autenticación)

```java
@Configuration
@EnableWebSecurity
@EnableMethodSecurity(prePostEnabled = true)
@RequiredArgsConstructor
public class SecurityConfig {

    private final JwtAuthenticationFilter jwtFilter;

    @Bean
    public SecurityFilterChain filterChain(HttpSecurity http) throws Exception {
        return http
            .csrf(AbstractHttpConfigurer::disable)
            .sessionManagement(s -> s.sessionCreationPolicy(SessionCreationPolicy.STATELESS))
            .authorizeHttpRequests(auth -> auth
                .requestMatchers("/api/v1/auth/**").permitAll()
                .requestMatchers("/swagger-ui/**", "/v3/api-docs/**").permitAll()
                .anyRequest().authenticated()
            )
            .addFilterBefore(jwtFilter, UsernamePasswordAuthenticationFilter.class)
            .build();
    }

    @Bean
    public PasswordEncoder passwordEncoder() {
        return new BCryptPasswordEncoder();
    }
}
```

## Estrategias (equivalente al IEnumerable<ICreatePolicyStrategy>)

```java
// En lugar del array manual de C#, Spring inyecta automáticamente todas las implementaciones:
@Service
@RequiredArgsConstructor
public class PolicyService {
    private final List<PolicyCreationStrategy> strategies; // inyectado automáticamente

    public PolicyResponse create(CreatePolicyRequest request) {
        var strategy = strategies.stream()
            .filter(s -> s.type() == request.type() && s.canHandle(request))
            .findFirst()
            .orElseThrow(() -> new UnsupportedPolicyTypeException(request.type()));
        // ...
    }
}
```

## CORS Configuration

```java
// Equivalente al UseCors() de Program.cs
@Bean
public CorsConfigurationSource corsConfigurationSource() {
    var config = new CorsConfiguration();
    config.setAllowedOrigins(List.of("http://localhost:4200"));
    config.setAllowedMethods(List.of("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"));
    config.setAllowedHeaders(List.of("*"));
    config.setAllowCredentials(true);
    var source = new UrlBasedCorsConfigurationSource();
    source.registerCorsConfiguration("/api/**", config);
    return source;
}
```

## OpenAPI / Swagger

```java
// Equivalente al AddSwaggerGen() de Program.cs
@Configuration
public class OpenApiConfig {
    @Bean
    public OpenAPI openAPI() {
        return new OpenAPI()
            .info(new Info().title("InsuraTech API").version("1.0"))
            .addSecurityItem(new SecurityRequirement().addList("Bearer"))
            .components(new Components().addSecuritySchemes("Bearer",
                new SecurityScheme().type(SecurityScheme.Type.HTTP).scheme("bearer")));
    }
}
```

## pom.xml — dependencias principales

```xml
<dependencies>
  <dependency>
    <groupId>org.springframework.boot</groupId>
    <artifactId>spring-boot-starter-web</artifactId>
  </dependency>
  <dependency>
    <groupId>org.springframework.boot</groupId>
    <artifactId>spring-boot-starter-data-mongodb</artifactId>
  </dependency>
  <dependency>
    <groupId>org.springframework.boot</groupId>
    <artifactId>spring-boot-starter-security</artifactId>
  </dependency>
  <dependency>
    <groupId>org.springframework.boot</groupId>
    <artifactId>spring-boot-starter-validation</artifactId>
  </dependency>
  <dependency>
    <groupId>org.projectlombok</groupId>
    <artifactId>lombok</artifactId>
    <optional>true</optional>
  </dependency>
  <dependency>
    <groupId>org.springdoc</groupId>
    <artifactId>springdoc-openapi-starter-webmvc-ui</artifactId>
    <version>2.3.0</version>
  </dependency>
  <dependency>
    <groupId>com.google.firebase</groupId>
    <artifactId>firebase-admin</artifactId>
    <version>9.2.0</version>
  </dependency>
  <!-- JWT -->
  <dependency>
    <groupId>io.jsonwebtoken</groupId>
    <artifactId>jjwt-api</artifactId>
    <version>0.12.3</version>
  </dependency>
</dependencies>
```

## Formato de salida

Si el input es `DependencyInjection.cs` → genera los `@Configuration` necesarios.
Si el input es `appsettings.json` → genera `application.yml`.
Si el input es `Program.cs` → genera la clase principal + `SecurityConfig.java`.

Al inicio de cada archivo generado incluye:
```java
// Origen: <NombreArchivo>.cs → <NombreArchivo>.java
// Paquete: com.insuratech.<capa>
```
