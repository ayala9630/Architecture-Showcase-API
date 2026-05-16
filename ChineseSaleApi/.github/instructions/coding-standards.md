# Coding Standards

## Naming conventions

- Interfaces use `I` prefix: `IUserService`, `IUserRepository`.
- DTO classes end with `Dto`: `CreateUserDto`, `UpdateUserDto`, `LoginResponseDto`.
- Create/update payload classes use `Create*` / `Update*` prefixes.
- Controller classes end with `Controller`: `UserController`, `PackageController`.
- Service classes end with `Service`: `UserService`, `TokenService`.
- Repository classes end with `Repository`: `UserRepository`, `PackageRepository`.

## File organization

- Keep one top-level public type per file when possible.
- Group interfaces in `ServiceInterfaces/` and `RepositoryInterfaces/`.
- Place mapping profiles in `Mappings/` and keep them focused on entity/DTO transformations.

## Asynchronous and logging patterns

- Use `async`/`await` for all I/O operations and EF Core calls.
- Log exceptions at the layer where they occur, then rethrow for centralized middleware handling.
- Use `ILogger<T>` via DI in controllers and services.
- Prefer `LogWarning` for validation-caused exceptions and `LogError` for unexpected failures.

## Exception usage

- Throw `ArgumentException` for invalid input or business validation failures.
- Throw `UnauthorizedAccessException` for authorization failures.
- Use `KeyNotFoundException` for missing resources when appropriate.
- Do not catch exceptions silently; controllers and middleware should manage response translation.

## DTO and model conventions

- Use data annotations for request validation in DTOs: `[Required]`, `[MaxLength]`, `[EmailAddress]`, `[Phone]`.
- Keep DTOs separate from EF entities.
- Map DTOs to entities using AutoMapper, and ignore fields that should not be set directly (`Password`, `AddressId`).

## Dependency patterns

- Prefer constructor injection for services, repositories, mappers, config, and loggers.
- Avoid service locators or static references.
- Register scoped dependencies in `Program.cs`.

## Avoid

- Avoid embedding business logic in controllers.
- Avoid hardcoding strings for routes; use attribute routing consistently.
- Avoid direct entity projection in controllers; use DTOs.
- Avoid suppressing exceptions without logging.
