# AMYL API architecture

AMYL is a single ASP.NET Core project organized as a vertical-slice application with MediatR, Serilog, and OpenTelemetry.

**Reference design:** `DOCUMENTATION_TECH/TOOLS_AND_DOCS/DESIGN_PATTERN/vertical_slice/design_vertical_slice_mediatr/project.Api.md`, which inherits from `design_vertical_slice/project.Api.md`.

That pair is the specification — folder boundaries, the provider-ownership rule, `Result` semantics, naming, and the do/do-not list all live there and are not repeated here. **This file records only what is specific to AMYL:** where it deviates, and what is known to be wrong.

## Layout

```text
Abstractions/    ICommand, IQuery, handler interfaces (on MediatR), IEndpoint
Features/        one folder per use case: Command/Query, Validator, Handler, Endpoint
Domain/          entities, Domain/Common primitives, Domain/Errors catalogs
Infrastructure/  Persistence (EF Core), Authentication, Observability
Behaviors/       ValidationBehavior, LoggingBehavior
Middleware/      GlobalExceptionHandler
Extensions/      DI registration, EndpointExtensions, ResultExtensions
```

Request flow:

```text
Endpoint (IEndpoint, discovered by assembly scan)
   -> ISender.Send
   -> LoggingBehavior      span + one structured log line
   -> ValidationBehavior   invalid -> failed Result, handler never runs
   -> Handler
   -> AppDbContext
```

## Deviations from the reference

**1. `ValidationBehavior` validates queries as well as commands.** The reference constrains it to `IBaseCommand` so it applies to writes only. AMYL cannot: `Memories/List` and `Memories/ListByType` validate their own `PageNumber`/`PageSize`, and that constraint would silently stop running those validators — no error, no failing test, just unvalidated pagination. The constraint here is `where TResponse : Result`.

`IBaseCommand` still exists in `Abstractions/Messaging` and is the correct hook for a behaviour that genuinely must be write-only, such as a transaction or outbox dispatch. Never constrain such a behaviour on `ICommand<TResponse>` — MediatR closes behaviours with `TResponse = Result<T>`, so that constraint never matches and disables the behaviour with no error.

**2. `Login` returns 401 for an unknown username.** Previously 404 "User not found" for a missing user and 401 for a bad password, which let anyone enumerate valid usernames. Both cases now return `UserErrors.InvalidCredentials`.

## Safety nets specific to this design

Dispatch and routing are both resolved at runtime, so two mistakes are invisible until a request arrives: a message with no handler throws on `Send`, and an endpoint the scan misses is a silent 404. `AMYL.Tests/WiringTests.cs` covers both — every `IRequest<>` has exactly one handler, and the `IEndpoint` count equals the use-case count. Do not delete those two tests.

`AddValidatorsFromAssembly(assembly, includeInternalTypes: true)` in `ApplicationServices` is load-bearing: the validators are `internal`, and without the flag none of them register, `ValidationBehavior` finds nothing, and **every request succeeds**. Nothing else fails.

`AddSource(ApplicationDiagnostics.SourceName)` in `ObservabilityServices` is equally load-bearing: without it `StartActivity` returns `null`, `LoggingBehavior` keeps working, and no command spans are ever exported.

## Known open items

- **`IFormFile` on commands.** `Memories/Create/Command.cs` and `Memories/Update/Command.cs` carry `IFormFile`, which keeps a transport type in the message and does not bind from a JSON body. Bind multipart at the endpoint and map to a feature-owned upload model. Both files carry a `MIGRATION ITEM` comment.
- **Unapplied rate limit policy.** `RateLimitPolicies.Login` is defined in `Extensions/RateLimitingServices.cs` but attached to nothing. `Memories/Create` is the only endpoint with a limiter (`General`). Login is the endpoint that most needs one.
- **`AuditableEntity.DeletedAt` and `UpdatedAt` are non-nullable `DateTime`.** Unset rows carry a `0001-01-01` sentinel rather than `NULL`. Making both `DateTime?` would model "never updated" and "not deleted" honestly, but requires another migration.
- **Soft delete is dead scaffolding.** `IsDeleted`, `DeletedAt`, `DeletedBy`, and `SoftDelete()` exist on `AuditableEntity`, but nothing calls or filters on them and `Memories/Delete` performs a hard `Remove()`. Either commit to it (call `SoftDelete()` plus an EF global query filter) or delete the fields. Leaving it is the worst option, because the fields imply a guarantee the code does not provide.
- **Audit fields are never populated.** `CreatedBy`, `UpdatedAt`, and `UpdatedBy` are written by no code path; `Memories/Update` does not touch `UpdatedAt`. A `SaveChangesAsync` override or an EF interceptor is the usual fix.
- **Error codes are inconsistent across slices.** `Domain/Errors/{MemoryErrors,UserErrors}` now own the codes used by the converted slices, but the string-based `Result.Failure/NotFound/Conflict` factories remain and still emit generic codes such as `request.failure`. Migrate remaining call sites onto catalogs, then consider removing the string overloads.
- **Not run end to end since the restructure.** The build and tests are green, but the Serilog OTLP sink, `AddNpgsql()` spans, and `LoggingBehavior` spans have not been observed against a live database and collector.

## Resolved

- **Un-migrated model drift** — fixed by `20260827053153_AuditFields_And_ExplicitUserFk`, hand-written to rename `CreateAt` to `CreatedAt` rather than to `UpdatedAt` as the scaffolder proposed. `ef migrations has-pending-model-changes` reports none.
- **Validation threw for expected failures** — `ValidationBehavior` now returns a failed `Result` carrying per-property errors instead of throwing `ValidationException`. `ResultExtensions` maps it to the same `ValidationProblemDetails` shape.
- **Building a failed `Result<T>` needed reflection** — replaced by `IValidationResult<TSelf>` in `Domain/Common`, a static abstract interface member. `Result` implements `IValidationResult<Result>` and `Result<T>` implements `IValidationResult<Result<T>>`, so the behaviour calls `TResponse.ValidationFailure(errors)` and the compiler binds the correct factory. `ValidationResultFactory` and its `ConcurrentDictionary<Type, MethodInfo>` are deleted; a result type that forgets the interface is now a build error.
- **Routing was hand-maintained** — feature modules listed every `Map` call. Endpoints are now discovered by assembly scan; the modules keep only their DI registration.
