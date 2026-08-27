# AMYL API architecture

AMYL is a single ASP.NET Core project organized as a pragmatic vertical-slice application. The top-level source folders are ownership boundaries, not separate architectural layers.

## Source boundaries

- `Features` owns user-facing use cases. Each request folder keeps its endpoint, MediatR request, handler, validator, and request-specific response model together.
- `Infrastructure` owns technology-specific implementations such as EF Core persistence, caching, JWT identity, and local file storage.
- `Shared` contains only concepts used across unrelated slices, including domain entities, result values, security vocabulary, cache keys, and MediatR behaviors.
- `Web` owns global ASP.NET Core and HTTP concerns such as authorization plumbing, Problem Details, host registration, middleware, rate limiting, and observability.

## Rules

1. Add behavior under the owning feature and use case; do not recreate global `Commands`, `Queries`, `Dtos`, `Services`, `Common`, or `Helpers` folders.
2. Keep request-specific contracts inside their slice. A contract shared by several use cases in one feature stays at that feature's root.
3. Put provider and framework implementations in `Infrastructure` or `Web`. Introduce a feature-facing interface only when a real boundary is needed.
4. Use MediatR commands for writes and queries for reads. FluentValidation runs through the single shared validation behavior.
5. Return `Result` or `Result<T>` for expected failures. Only `Web/Extensions/ResultExtensions.cs` maps those failures to RFC 7807 HTTP responses.
6. Let the global exception handler convert unexpected failures to safe Problem Details responses.
7. Pass cancellation tokens through asynchronous database, cache, file, and provider work.
8. Prefer cohesive slices and direct EF Core usage over generic repositories or premature shared abstractions.

Feature endpoints are composed through route groups in `AuthenticationEndpoints.cs` and `MemoryEndpoints.cs`; each individual endpoint remains beside its use case.
