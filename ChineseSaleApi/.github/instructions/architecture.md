# Architecture Overview

## Project structure

- `ChineseSaleApi/ChineseSaleApi`: backend API project.
- `ChineseSaleApi/ChineseSaleApi/Controllers`: HTTP API endpoints.
- `ChineseSaleApi/ChineseSaleApi/Services`: business logic layer.
- `ChineseSaleApi/ChineseSaleApi/Repositories`: data access layer.
- `ChineseSaleApi/ChineseSaleApi/ServiceInterfaces` and `RepositoryInterfaces`: interface contracts for DI.
- `ChineseSaleApi/ChineseSaleApi/Data`: EF Core `DbContext` and DB definitions.
- `ChineseSaleApi/ChineseSaleApi/Mappings`: AutoMapper profiles.
- `ChineseSaleApi/ChineseSaleApi/Middleware`: global middleware.

## Primary backend flow

1. `Program.cs` configures services and middleware.
2. HTTP requests reach a controller in `Controllers/`.
3. Controllers delegate business rules to a typed service interface (`I*Service`).
4. Services coordinate validation, mapping, email/token workflows, and repository calls.
5. Repositories perform EF Core data operations against `ChineseSaleContext`.
6. Responses propagate back through controllers and middleware.

## Dependency wiring

- `Program.cs` registers repository and service implementations with scoped lifetime.
- Controllers receive services through constructor injection.
- Services receive repositories, mappers, configuration, and loggers through constructor injection.
- `AutoMapper` is registered once with `builder.Services.AddAutoMapper(typeof(Program));`.
- `IEmailService`, `ITokenService`, and other cross-cutting services are also injected.

## Error handling

- A custom `ErrorHandlingMiddleware` catches exceptions thrown from controllers and services.
- Known exceptions are translated into HTTP status codes:
  - `ArgumentException` -> 400 Bad Request
  - `KeyNotFoundException` -> 404 Not Found
  - `UnauthorizedAccessException` -> 401 Unauthorized
  - `SecurityException` -> 403 Forbidden
  - other exceptions -> 500 Internal Server Error
- Controllers still log exceptions and may return custom 500 payloads for unexpected errors.

## Configuration and middleware

- `Program.cs` loads `appsettings.json` and environment-specific JSON.
- `UseSqlServer` is configured with `DefaultConnection` from configuration.
- JWT authentication and authorization are configured via `AddAuthentication`/`AddJwtBearer`.
- `Swagger` is enabled in development.
- CORS policy `AllowAllOrigins` is globally applied.
- Static files are served via `UseStaticFiles()`.

## Folder responsibilities

- `Controllers/`: API endpoints, request validation, status codes, authorization attributes.
- `Services/`: business logic, orchestration, validation, mapping, email and token workflows.
- `Repositories/`: EF Core persistence and query patterns.
- `Data/`: `ChineseSaleContext`, `DbSet<>` declarations, entity relationships.
- `Mappings/`: DTO-entity conversion rules.
- `Attributes/`: custom authorization filter (`AdminAttribute`).
- `Validations/`: custom validation attributes (`DateValidation`).
