# Controllers Layer

> This file is intentionally isolated because controller logic is large and API-specific.

## Purpose

Controllers expose HTTP endpoints, validate request shape, enforce authorization, and translate service results into HTTP responses.

## Route conventions

- Controllers use `[ApiController]` and `[Route("api/[controller]")]`.
- Actions use route attributes like `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`.
- Common routes include:
  - `api/user/login`
  - `api/user/register`
  - `api/package/{id}`
  - `api/package/lottery/{lotteryId}`

## Authorization patterns

- Public endpoints have no authorization attribute.
- Protected endpoints use `[Authorize]`.
- Admin-only actions use both `[Authorize]` and a custom `[Admin]` attribute.

## Request binding

- Use `[FromBody]` for complex request payloads, pagination and filter param.
- Use route parameters for ids and resource selection.

## Response patterns

- Return `Ok(...)` for successful read operations.
- Return `CreatedAtAction(...)` for successful creation.
- Return `NoContent()` for successful updates/deletions.
- Return `NotFound()` when resources are missing.
- Use `StatusCode(500, ...)` only for unexpected errors not handled by the middleware.

## Error handling pattern

- Controllers wrap service calls in `try/catch`.
- Validation errors are logged and rethrown to middleware.
- Unexpected errors are logged and may return custom 500 payloads.
- Do not implement business validation in controllers; delegate to services.

## Logging

- Each controller has an injected `ILogger<T>`.
- Log warnings for invalid request data.
- Log errors for unexpected failures.

## Do

- Keep controllers thin and focused on HTTP contract.
- Delegate business rules to services.
- Use authorization attributes consistently.

## Don't

- Do not access EF Core directly in controllers.
- Do not build JWT tokens in controllers.
- Do not perform heavy computation or persistence logic inside controller actions.
