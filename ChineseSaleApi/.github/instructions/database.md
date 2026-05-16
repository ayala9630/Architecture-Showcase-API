# Database Layer

## EF Core configuration

- `ChineseSaleContext` is the main `DbContext`.
- It is registered in `Program.cs` with `UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))`.
- `DbSet<>` properties define tables:
  - `Addresses`, `Users`, `Packages`, `Gifts`, `Cards`, `CardCarts`, `PackageCarts`, `Lotteries`, `Categories`, `Donors`.

## Entity relationships

- `User` has one `Address`.
- `Donor` has one `CompanyAddress`.
- `Package` belongs to a `Lottery`.
- `Gift` belongs to `Donor`, `Category`, and `Lottery`.
- `CardCart` references `User` and `Gift`.
- `Card` references `User` and `Gift`.

## Fluent API rules

- Unique indices are configured for `UserName` and `Email` on `User`.
- One-to-one relationships use `HasOne().WithOne().HasForeignKey<>()`.
- Delete behaviors vary by relationship:
  - `Cascade` for most product relationships.
  - `SetNull` for `Gift.Category`.
  - `NoAction` for `CardCart.User` and `Card.User`.

## Migrations

- Migrations are stored under `ChineseSaleApi/Migrations/`.
- Existing migration files include `InitialCreate` and subsequent schema updates.
- The codebase expects EF Core migrations to be used for schema evolution.

## Database configuration notes

- App configuration expects `JwtSettings` and `EmailSettings` sections.
- `Program.cs` reads `JwtSettings:SecretKey`, `JwtSettings:Issuer`, `JwtSettings:Audience`, and `JwtSettings:ExpiryMinutes`.
- `EmailSettingsDto` is configured from `EmailSettings`.

## Do

- Use `ChineseSaleContext` for all data access in repositories.
- Keep model relationships explicit in `OnModelCreating`.
- Maintain separate migration files for schema changes.

## Don't

- Do not place query filtering logic outside repositories.
- Do not assume automatic model binding will validate complex DB relationships.
