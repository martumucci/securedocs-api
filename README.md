# securedocs-api

The API service of SecureDocs, a system that asynchronously processes sensitive documents through cryptographic operations performed by a separate worker. This repository contains the client-facing HTTP service.

The API accepts document submissions, persists their metadata in Postgres, stores the plain payload temporarily in Redis, publishes integration events to RabbitMQ for the worker, consumes the worker's responses back, and exposes endpoints to query document state.

---

## Architecture

The solution follows a four-layer Clean Architecture; dependencies point inward.

```
SecureDocs.API              Controllers, middleware, DI composition.
    │
SecureDocs.Infrastructure   EF Core, Redis, MassTransit, concrete repositories.
    │
SecureDocs.Application      Use cases (Commands/Queries), validators, integration
    │                       events, interfaces consumed by handlers.
    │
SecureDocs.Domain           Aggregates, value objects, invariants, domain events.
                            Depends on nothing.
```

Use cases are dispatched through MediatR (`IMediator.Send(...)`) as Commands and Queries with dedicated handlers. Cross-cutting concerns sit in MediatR pipeline behaviors — currently FluentValidation runs as a pipeline behavior before every handler.

### Request flow on submission

1. `DocumentsController.Submit` maps the request body to a `SubmitDocumentCommand` and dispatches it through MediatR.
2. `ValidationBehavior` runs the validator; on failure the handler is never invoked.
3. `SubmitDocumentHandler` creates a `Document` aggregate, stores the payload in Redis, registers the aggregate with EF Core, publishes a `DocumentSubmittedIntegrationEvent` through `IIntegrationEventPublisher` (captured by MassTransit's outbox), and commits the unit of work. The document, the outbox row, and any other tracked changes commit atomically.
4. The controller returns `201 Created` with the new `documentId` and `Pending` status.
5. MassTransit's background delivery service polls the outbox and publishes pending messages to RabbitMQ.
6. When the worker replies with `DocumentProcessedIntegrationEvent`, `DocumentProcessedConsumer` dispatches the corresponding command (`MarkDocumentAsProcessedCommand` or `MarkDocumentAsFailedCommand`), which transitions the aggregate.

The client never blocks on processing; it polls `GET /documents/{id}` for the resulting state.

---

## Tech stack

| Concern | Choice |
| --- | --- |
| Runtime | .NET 8 LTS |
| Web | ASP.NET Core (Controllers) |
| Persistence | EF Core 8 + Npgsql, PostgreSQL 16 |
| Ephemeral payload store | Redis 7 |
| Broker | RabbitMQ 3.13 |
| Messaging library | MassTransit 8 with EntityFrameworkOutbox |
| Mediator | MediatR 12 |
| Validation | FluentValidation 11 |
| Logging | Serilog with correlation IDs |
| Rate limiting | `Microsoft.AspNetCore.RateLimiting` with Redis backing store |
| Health checks | AspNetCore.HealthChecks (Npgsql, Redis, RabbitMq) |
| Unit tests | xUnit, FluentAssertions, NSubstitute |
| Integration tests | xUnit, FluentAssertions, Testcontainers |

---

## Running locally

Requires Docker and the .NET 8 SDK. The EF Core CLI is needed only if you want to manage migrations from the command line:

```bash
dotnet tool install --global dotnet-ef --version 8.0.10
```

Bring up the infrastructure:

```bash
docker compose up -d
```

This starts Postgres, Redis, RabbitMQ, and admin UIs (pgAdmin on 5050, RedisInsight on 5540, RabbitMQ management on 15672).

Apply the database migrations:

```bash
dotnet ef database update \
  --project src/SecureDocs.Infrastructure \
  --startup-project src/SecureDocs.API
```

Run the API:

```bash
dotnet run --project src/SecureDocs.API --launch-profile http
```

The API listens on `http://localhost:5245`. Swagger UI is at `/swagger`.

```bash
curl -i -X POST http://localhost:5245/Documents \
  -H "Content-Type: application/json" \
  -H "X-Correlation-Id: my-trace-001" \
  -d '{"payload": "hello"}'
```

---

## Project structure

```
src/
├── SecureDocs.Domain/          Aggregates, domain events, invariants.
├── SecureDocs.Application/     Commands, queries, validators, integration events,
│                               interfaces.
├── SecureDocs.Infrastructure/  EF Core context and configurations, repositories,
│                               Redis payload store, MassTransit setup, consumers,
│                               IIntegrationEventPublisher implementation.
└── SecureDocs.API/             Controllers, exception handlers, correlation-id
                                middleware, DI composition, Program.cs.

tests/
├── SecureDocs.UnitTests/        Handler, validator and domain tests with mocks.
└── SecureDocs.IntegrationTests/ End-to-end tests against real Postgres / Redis /
                                 RabbitMQ spun up by Testcontainers.
```

---

## Key design decisions

### Transactional outbox via MassTransit

Persisting a document and publishing its event involve two systems (Postgres and RabbitMQ) that cannot share a transaction. The outbox pattern resolves this: `IPublishEndpoint.Publish(...)` does not send to the broker — MassTransit's `SaveChanges` interceptor materializes the message into the `OutboxMessage` table in the same Postgres transaction as the document. A background delivery service polls the outbox using `SELECT ... FOR UPDATE SKIP LOCKED` (safe for horizontal scaling) and publishes pending messages to RabbitMQ. Delivered rows are deleted.

The same setup provides an inbox for consumers (`InboxState` table), giving idempotency against duplicate delivery from the broker.

### Redis as ephemeral payload store

The plain payload is never persisted in Postgres. It is stored exclusively in Redis under `payload:{documentId}` with a 5-minute TTL, written before the Postgres transaction begins. The outbox row and the broker message carry only the `documentId` and `messageId`; the worker fetches the payload from Redis and is responsible for persisting only the encrypted form.

If the Postgres commit fails after the Redis write, the orphan key disappears via TTL. If the Redis write fails, the Postgres transaction never starts. The Redis container is configured for in-memory operation only (`--save "" --appendonly no`) so the plain payload never touches disk.

### `IIntegrationEventPublisher` abstraction

Although MassTransit is itself broker-agnostic, the `MassTransit` package is not referenced from `Application`. Handlers depend on `IIntegrationEventPublisher`, an interface defined in `Application`. The MassTransit implementation lives in `Infrastructure`. This keeps the use cases free of any direct messaging library type and isolates changes to the transport in a single class.

The implementation also reads the active HTTP `X-Correlation-Id` and propagates it as an AMQP header so the correlation id follows the message across services.

### Two-layer validation

`FluentValidation` validates incoming commands at the application boundary (non-empty payload, length within bounds), enforced by a MediatR pipeline behavior. Aggregate invariants (`Id` not empty, `Status` is a defined enum value, illegal state transitions) are enforced in the aggregate itself, in its constructor and in its `MarkAsProcessed` / `MarkAsFailed` methods. Input validation can evolve with the public API; invariants belong to the model.

### Idempotent state transitions

`Document.MarkAsProcessed()` and `MarkAsFailed()` are no-ops when the document is already in the target state, and throw when the transition is illegal (`Failed → Processed` or vice versa). This is the line of defense against duplicated broker deliveries that slip past the inbox, and against any future retry mechanism.

### Distributed rate limiting

`POST /Documents` is rate-limited per client IP using `Microsoft.AspNetCore.RateLimiting` with Redis as the backing store (via `RedisRateLimiting.AspNetCore`). Counters are shared across API instances, so the limit is global rather than per-process. Thresholds are configured under `RateLimiting:SubmitDocument` in `appsettings.json`.

### Structured logging with correlation IDs

Serilog replaces the default logging provider. A `CorrelationIdMiddleware` reads `X-Correlation-Id` from incoming requests (or generates one) and pushes it into `LogContext`. Every log line emitted during the request is enriched with that property, the id is echoed in the response header, and it is forwarded to the broker as an AMQP header so consumers downstream can pick it up.

`UseSerilogRequestLogging()` emits a single structured log line per HTTP request with method, path, status code, and elapsed time.

### Health checks split into liveness and readiness

- `GET /health/live` — returns `200` if the process is responsive. Used by orchestrators to decide whether to restart the pod.
- `GET /health/ready` — verifies Postgres, Redis, and RabbitMQ reachability. Returns `503` if any dependency is unavailable. Used to decide whether to route traffic to this instance.

### Postgres schema isolation

All application tables, MassTransit's outbox/inbox tables, and the EF Core migrations history live under a dedicated `securedocs` schema. The `public` schema is left untouched.

### Exception handling

Exceptions are mapped to RFC 7807 Problem Details by a chain of `IExceptionHandler` implementations. `ValidationExceptionHandler` produces `400 Bad Request` with field-level errors for FluentValidation failures. `GlobalExceptionHandler` produces `500` with a sanitized message for anything else; the full exception is logged but never returned to the client.

### Strongly-typed configuration

Configurable values are bound to POCO classes (`MassTransitOptions`, etc.) and consumed through `IOptions<T>`. Environment variables can override any value using the `__` separator (`MassTransit__Outbox__QueryDelaySeconds=10`).

---

## Testing

Unit tests cover handlers, validators, and the `Document` aggregate with NSubstitute fakes; they run in tens of milliseconds and require no infrastructure.

```bash
dotnet test tests/SecureDocs.UnitTests
```

Integration tests boot the full application via `WebApplicationFactory<Program>` against Postgres, Redis, and RabbitMQ containers managed by Testcontainers. Migrations are applied before the first test runs. Real HTTP requests go through the in-process pipeline; assertions read directly from Postgres and Redis.

```bash
dotnet test tests/SecureDocs.IntegrationTests
```

Integration tests require Docker.

---

## Configuration

Defaults live in `appsettings.json`. Every value can be overridden through environment variables using the `__` separator. In production, connection strings must come from environment variables or a secret manager.

| Key | Purpose |
| --- | --- |
| `ConnectionStrings:Postgres` | Postgres connection string |
| `ConnectionStrings:Redis` | Redis connection string |
| `ConnectionStrings:RabbitMq` | RabbitMQ AMQP URI |
| `MassTransit:Outbox:QueryDelaySeconds` | Outbox delivery service poll interval |
| `MassTransit:Outbox:QueryMessageLimit` | Outbox delivery batch size |
| `RateLimiting:SubmitDocument:PermitLimit` | Requests allowed per window per IP |
| `RateLimiting:SubmitDocument:WindowSeconds` | Window size for rate limiting |
| `Serilog:MinimumLevel:Default` | Default log level |

---

## API surface

| Method | Path | Description |
| --- | --- | --- |
| `POST` | `/Documents` | Submit a document. Returns `201 Created` with `documentId` and `Pending` status. Rate-limited per IP. |
| `GET` | `/Documents/{id}` | Get a document's metadata. Returns `200 OK` or `404 Not Found`. |
| `GET` | `/health/live` | Liveness probe. |
| `GET` | `/health/ready` | Readiness probe. Checks Postgres, Redis, and RabbitMQ. |

Authentication is out of scope at this phase; all endpoints are open.
