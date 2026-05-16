# Shared Conventions

## Cross-cutting rules

- Keep explanations focused on actual repo structure and implementation.
- Prefer concrete examples from the code instead of abstract design patterns.
- Keep `repositories.md` and `controllers.md` separate to reduce token usage for unrelated tasks.
- Place general rules in this file rather than repeating them in every layer-specific file.

## Common naming rules

- Use PascalCase for class, interface, and method names.
- Use camelCase for method parameters and local variables.
- Keep DTO names self-descriptive.
- Use `Api` only in project names or host-level concepts, not in controller names.

## Shared folder responsibilities

- Backend `ChineseSaleApi` is the API implementation.
- Avoid mixing conventions from other projects in the same analysis.

## Error handling

- Use centralized middleware for error translation.
- Controllers and services should log and rethrow exceptions, not swallow them.

## Dependency patterns

- Prefer constructor injection across backend services and repositories.
- Use scoped lifetimes for database and service dependencies.
- Configure DI in `Program.cs` only; do not register services in controllers.

## Token optimization guidance

- Load only the specific instruction file needed for the current task.
- Use `shared-conventions.md` for small reusable rules.
- Keep large layer-specific files out of unrelated contexts.
