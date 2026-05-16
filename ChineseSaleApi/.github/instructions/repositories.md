# Repositories Layer

> This file is intentionally isolated because repository implementations are detailed and should not be loaded for unrelated tasks.

## Purpose

Repositories are the data access abstraction over EF Core and `ChineseSaleContext`.
They expose persistence operations and query methods without mapping to DTOs.

## Folder structure

- `RepositoryInterfaces/`: repository contracts.
- `Repositories/`: concrete EF Core implementations.

## Common repository patterns

- Use EF Core async methods: `ToListAsync()`, `FirstOrDefaultAsync()`, `AnyAsync()`, `CountAsync()`, `SaveChangesAsync()`.
- Query data via the `ChineseSaleContext` `DbSet<>` properties.
- Use `AsQueryable()` for pagination and filtering.
- Use `Include(...)` to eagerly load related entities when needed.

## Naming conventions

- Interface names: `IUserRepository`, `IPackageRepository`, `IDonorRepository`.
- Implementation names: `UserRepository`, `PackageRepository`, `DonorRepository`.
- Methods use clear CRUD verbs: `Add`, `Get`, `Update`, `Delete`, `Is*Exists`, `Get*WithPagination`.

## Example patterns

### Add entity

`AddUser(User user)`:
- Add to `DbSet`
- Call `SaveChangesAsync()`
- Return the generated entity id

### Query by id

`GetUserById(int id)` returns `User?` and may include related data:
- `return await _context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id == id);`

### Pagination

`GetUsersWithPagination(int pageNumber, int pageSize)`:
- count total results with `CountAsync()`
- apply `Skip((pageNumber - 1) * pageSize).Take(pageSize)`
- return `(items, totalCount)` tuple

### Existence checks

- Compare strings in a case-insensitive way via `ToLower()` for user/email exists checks.
- Return `bool` for `IsUserNameExists` and `IsEmailExists`.

## EF Core conventions

- `ChineseSaleContext` is injected into each repository.
- Repositories do not define explicit transactions; each save call persists immediately.
- The repository layer is responsible for shaping entity queries but not for exposing DTOs.

## Do

- Keep repository methods specific to persistence and query intent.
- Return domain entities or query tuples, not HTTP responses.
- Use async EF Core APIs.

## Don't

- Do not put business validation or mapping logic in repositories.
- Do not directly create or validate DTOs in repository implementations.
- Do not call `SaveChangesAsync()` outside repository persistence methods if the repository owns the entity lifecycle.
