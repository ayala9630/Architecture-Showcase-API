# Project Instruction Files

This folder contains focused AI instruction files derived from the server-side codebase.

## Purpose of each file

- `architecture.md`: high-level backend architecture, request flow, DI wiring, and folder responsibilities.
- `coding-standards.md`: naming, folder conventions, exception and logging patterns, `async` use, and DTO naming conventions.
- `services.md`: service layer responsibilities, dependency injection, business logic patterns, validation and mapping examples.
- `repositories.md`: repository implementation patterns, EF Core usage, query methods, persistence rules, and interface contracts.
- `controllers.md`: controller routing, authorization, request/response patterns, and error handling.
- `database.md`: Entity Framework `DbContext`, migrations, model relationships, and configuration expectations.
- `auth.md`: JWT auth flow, token generation, claim conventions, authorization rules, and custom admin guard.
- `shared-conventions.md`: cross-cutting conventions, shared folder responsibilities, rule-of-thumb should/should-not guidance.

## When AI should load each file

- Load `architecture.md` when reasoning about system layout or cross-layer flows.
- Load `coding-standards.md` when generating or reviewing code to match project conventions.
- Load `services.md` when updating or extending business rules and service interactions.
- Load `repositories.md` when working with data access, EF Core queries, and persistence patterns.
- Load `controllers.md` when changing API endpoints, request handling, or routing.
- Load `database.md` when modifying models, migrations, or DB configuration.
- Load `auth.md` when altering authentication, authorization, or JWT claims.
- Load `shared-conventions.md` for general style, folder responsibility, cross-project rules, and token-efficient context.

## Why `repositories.md` and `controllers.md` are separated

`repositories.md` and `controllers.md` are intentionally isolated because they describe large, detailed implementation areas that are often unrelated to each other.

- `repositories.md` is heavy on persistence and EF Core query behavior.
- `controllers.md` is heavy on API surface, routing, and request/response semantics.

Separating them keeps AI tasks token-efficient and prevents loading unnecessary large explanations when only one layer is relevant.

## Token optimization strategy

- Use focused files for each layer rather than a single monolithic file.
- Keep `repositories.md` and `controllers.md` isolated because they are the most verbose and layer-specific.
- Use `shared-conventions.md` for small cross-cutting rules to avoid repeating the same guidance in every file.
- Refer to concrete examples from the current codebase rather than generic patterns.
