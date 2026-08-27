# AMYL API architecture

AMYL is a single ASP.NET Core project organized as a lean vertical-slice application. The four top-level source folders are ownership boundaries, not architectural layers.

Reference design: `DOCUMENTATION_TECH/TOOLS_DOCS/DESIGN_PATTERN/vertical_slice/design_2_lean/`.

## Source boundaries

```text
Features/   what the application does      one file per use case, verb-named
Domain/     what it is about               entities + Domain/Common primitives
Data/       how it persists                EF Core only
Shared/     how it is wired                small and boring
```

- `Features` owns use cases. Each slice is a single verb-named file holding its `Command`/`Query`, `Validator`, `internal Handler`, and `Map` together. A feature also owns any provider with only one consuming feature.
- `Domain` owns entities and invariants, plus `Domain/Common` for `Result`, `Error`, `BaseEntity`, `AuditableEntity`, and `PaginatedList`. Entities are shared by slices; slices own their own request and response types.
- `Data` owns `AppDbContext`, entity configurations, migrations, and shared query extensions.
- `Shared` owns cross-cutting plumbing only: the validation behavior, the global exception handler, authorization plumbing, and DI/host extensions.

## Dependency rule

```text
Features  ->  Domain, Data, Shared
Data      ->  Domain
Shared    ->  Domain
Domain    ->  nothing
```

`AppDbContext` is the one legal exception: it names every entity because it is the composition root for the data model, exactly as `Program.cs` is for endpoints.

## Provider ownership

A provider is a class that talks to something outside the process.

- One consuming feature, so the provider lives in that feature: `JwtTokenService` and `PasswordHasherService` in `Features/Authentication`, `MemoryFileStorage` in `Features/Memories`.
- Two or more consuming features and it graduates to `Shared`. Move it when the second consumer appears, not before.
- Pure DI wiring is not a provider. Redis, HybridCache, OpenTelemetry, JWT bearer, and rate limiting are extension methods in `Shared/Extensions`.

## Rules

1. Slice files are verbs: `CreateMemory.cs`, never `MemoryService.cs`. Never recreate global `Commands`, `Queries`, `Dtos`, `Services`, `Common`, or `Helpers` folders.
2. Handlers are `internal`. The endpoint is the only entry point into a slice.
3. A response used by one slice nests in that slice. A response shared by sibling slices lives in `Features/{Feature}/Shared/`.
4. Split a slice into its own folder only past roughly 150-200 lines, or when it needs private helpers.
5. Return `Result` or `Result<T>` for expected failures. Only `Shared/Extensions/ResultExtensions.cs` maps them to RFC 7807 responses. `Result` never carries HTTP status codes.
6. The global exception handler converts unexpected failures to safe Problem Details responses. Expected and unexpected paths never mix.
7. Pass cancellation tokens through all database, cache, file, and provider work.
8. Endpoint authorization does not enforce row ownership. Handlers must additionally scope every query and command to the authenticated user.
9. Use EF Core directly. No generic repositories or unit-of-work wrappers.
10. No slice references another slice. Use the database, a domain service, or a domain event.

Endpoints are composed per feature in `AuthenticationModule.cs` and `MemoriesModule.cs`; each `Map` stays beside its use case.

## Known open items

- **`IFormFile` on commands.** `CreateMemory.Command` and `UpdateMemory.Command` carry `IFormFile`, which breaks the transport-agnostic request rule and does not bind from a JSON body. Bind multipart input at the endpoint and map to a feature-owned model.
- **Unapplied rate limit policy.** `RateLimitPolicies.Login` is defined but not attached to the login endpoint.
- **`AuditableEntity.DeletedAt` and `UpdatedAt` are non-nullable `DateTime`.** Unset rows therefore carry a `0001-01-01` sentinel rather than `NULL`. Making both `DateTime?` would model "never updated" and "not deleted" honestly, but requires another migration.
- **Soft delete is dead scaffolding.** `IsDeleted`, `DeletedAt`, `DeletedBy`, and `SoftDelete()` exist on `AuditableEntity` but nothing calls or filters on them, and `DeleteMemory` performs a hard `Remove()`. Either commit to it (call `SoftDelete()` and add an EF global query filter) or delete the fields. Leaving it is the worst option because the fields imply a guarantee the code does not provide. See `advanced_patterns/07_specification_pattern.md`.
- **Audit fields are never populated.** `CreatedBy`, `UpdatedAt`, and `UpdatedBy` are written by no code path. `UpdateMemory` does not touch `UpdatedAt`. A `SaveChangesAsync` override or an EF interceptor is the usual fix.

## Resolved

- **Un-migrated model drift** — fixed by `20260827053153_AuditFields_And_ExplicitUserFk`, hand-written to rename `CreateAt` to `CreatedAt` rather than to `UpdatedAt` as the scaffolder proposed. `ef migrations has-pending-model-changes` now reports none.
