# Authentication and Authorization

## JWT authentication flow

- JWT auth is configured in `Program.cs`.
- `TokenService.GenerateToken(...)` creates tokens with claims:
  - `sub` = user id
  - `email`
  - `given_name`, `family_name`
  - `firstName`, `lastName`, `isAdmin`
  - `ClaimTypes.NameIdentifier`
- Token validation checks issuer, audience, lifetime, and signing key.
- `ClockSkew` is set to zero.

## Configuration keys

- `JwtSettings:SecretKey`
- `JwtSettings:Issuer`
- `JwtSettings:Audience`
- `JwtSettings:ExpiryMinutes`

## Authorization rules

- `[Authorize]` secures endpoints for authenticated users.
- `[Admin]` enforces admin access by checking the `isAdmin` claim.
- `AdminAttribute` is a custom authorization filter that returns `ForbidResult()` if the claim is absent.

## Token generation details

- `TokenService` uses `SymmetricSecurityKey` and HMAC-SHA256.
- The expiration is derived from configuration.
- Tokens are returned as `Bearer` tokens in `LoginResponseDto`.

## Do

- Keep auth logic inside `TokenService` and authentication middleware.
- Use claims consistently across generation and authorization checks.
- Throw exceptions from services, and let middleware handle HTTP translation.

## Don't

- Do not generate tokens in controllers.
- Do not store secret keys in code.
- Do not bypass `[Authorize]` for protected routes.
