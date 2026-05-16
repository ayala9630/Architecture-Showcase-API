# Services Layer

## Responsibility

Services encapsulate business logic, validation, mapping, and orchestration across repositories and external services.

## Structure and patterns

- Each service implements a single interface in `ServiceInterfaces/`.
- Services are injected into controllers and other services.
- `UserService` orchestrates user creation, authentication, email notifications, and address creation.
- `DonorService` demonstrates complex orchestration: validation, repository lookup, fallback card retrieval, and aggregation.

## Common service behaviors

- Validate inputs at the start of public methods.
- Throw `ArgumentException` if required data is missing or invalid.
- Use AutoMapper to convert between DTOs and domain entities.
- Do not commit transactions directly; use repository methods that call `SaveChangesAsync`.
- Send external notifications via injected services like `IEmailService`.

## Dependency examples

- `UserService` depends on:
  - `IUserRepository`
  - `IAddressService`
  - `ITokenService`
  - `IEmailService`
  - `IConfiguration`
  - `IMapper`
  - `ILogger<UserService>`

- `DonorService` depends on:
  - `IDonorRepository`
  - `IAddressService`
  - `ICardRepository`
  - `ICardService`
  - `IMapper`
  - `ILogger<DonorService>`

## Implementation patterns

- Use repository methods for all data access.
- Map incoming DTOs to entities, but ignore properties not controlled by the payload.
- Example: `CreateMap<CreateUserDto, User>()` ignores `Password`, `AddressId`, and `Address`.
- Example: `LoginResponseDto` is built after token generation and user mapping.

## Pagination and search

- Pagination is implemented through `PaginationParamsDto` and `PaginatedResultDto<T>`.
- Services call repository methods like `GetUsersWithPagination(page, size)`.
- The service layer maps paged entity collections to DTO collections.

## Error handling

- Catch validation and domain exceptions locally, log them, then rethrow.
- Unexpected exceptions are caught, logged, and rethrown for the middleware to translate.
- Do not convert all exceptions to HTTP responses inside services.
