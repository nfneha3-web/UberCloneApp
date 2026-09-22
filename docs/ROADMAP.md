# RideShare Clone — Architecture Roadmap

Zero-cost, production-style ride-hailing platform: .NET Core API, Angular
frontend, SQL Server, Clean Architecture + CQRS, with an AI copilot.

## 1. Guiding constraints

- **$0 to run.** Every external service used has a genuine free tier that
  does not auto-bill: SQL Server Developer/Express edition (free, full
  feature set, non-production licensing), Stripe **test mode** (payment
  integration with fake cards — no live keys, no real money ever moves),
  Azure OpenAI free trial credit behind a swappable `IAiCopilotService`
  abstraction so the underlying model can be swapped to GitHub Models
  (free tier) or a local Ollama model with no code changes once the trial
  lapses, OpenStreetMap/Leaflet for maps (free, no API key) instead of the
  Google Maps billing API.
- **First build pass = one working vertical slice**, not every Uber
  feature. Breadth (surge pricing, promo codes, scheduled rides, driver
  KYC docs, chat, multi-tier vehicles) is listed as Phase 2+ so the
  architecture is visibly ready for it, but isn't built yet.

## 2. High-level architecture

```
                        ┌─────────────────────────┐
                        │        Angular SPA       │
                        │  Rider / Driver / Admin   │
                        │  modules + Copilot widget │
                        └───────────┬──────────────┘
                        REST (HTTP) │  WebSocket (SignalR)
                        ┌───────────▼──────────────┐
                        │      ASP.NET Core API     │
                        │  Controllers + SignalR Hub │
                        └───────────┬──────────────┘
                                    │ MediatR (CQRS)
                        ┌───────────▼──────────────┐
                        │   Application layer        │
                        │  Commands / Queries /      │
                        │  Handlers / FluentValidation│
                        └───────────┬──────────────┘
                                    │ interfaces
                        ┌───────────▼──────────────┐
                        │      Domain layer          │
                        │  Entities, Value Objects,   │
                        │  Domain Events, Enums        │
                        └───────────▲──────────────┘
                                    │ implements
                        ┌───────────┴──────────────┐
                        │    Infrastructure layer    │
                        │ EF Core (writes) + Dapper   │
                        │ (reads) + Unit of Work +    │
                        │ Stripe + AI Copilot +       │
                        │ SignalR location broadcast  │
                        └───────────┬──────────────┘
                                    │
                        ┌───────────▼──────────────┐
                        │   SQL Server (free tier)   │
                        └───────────────────────────┘
```

### Why CQRS with both EF Core and Dapper
- **Writes (Commands)** go through EF Core + a Unit of Work wrapping a
  single `DbContext` transaction — gives change tracking, migrations,
  concurrency tokens for things like "only one driver can accept a ride".
- **Reads (Queries)** go through Dapper with hand-written SQL — cheap,
  fast, no tracking overhead, ideal for ride lists, driver search,
  dashboards. Keeps read models decoupled from write-side entity shape.

## 3. Solution layout

```
UberCloneApp/
├── docs/                          # this roadmap + ADRs
├── backend/
│   ├── RideShare.sln
│   ├── src/
│   │   ├── Core/
│   │   │   ├── RideShare.Domain          # entities, enums, value objects, domain events, no dependencies
│   │   │   └── RideShare.Application     # CQRS commands/queries, MediatR handlers, FluentValidation, DTOs, interfaces (IUnitOfWork, IRideRepository, IAiCopilotService, IPaymentService...)
│   │   ├── Infrastructure/
│   │   │   ├── RideShare.Infrastructure  # EF Core DbContext + migrations, Dapper query services, Unit of Work, Stripe, AI copilot providers, SignalR notifier
│   │   │   └── RideShare.Identity        # ASP.NET Identity, JWT token issuing
│   │   └── Presentation/
│   │       └── RideShare.Api             # controllers, SignalR hubs, Swagger, DI composition root, appsettings
│   └── tests/
│       ├── RideShare.Domain.Tests
│       ├── RideShare.Application.Tests
│       └── RideShare.Api.IntegrationTests
└── frontend/
    └── rideshare-web/                    # Angular workspace
        └── src/app/
            ├── core/                     # auth guard/interceptor, api services, models
            ├── shared/                   # shared UI components, pipes
            ├── features/
            │   ├── auth/
            │   ├── rider/                # request ride, track ride, pay, rate
            │   ├── driver/                # go online, accept ride, navigate, earnings
            │   ├── admin/                 # users, rides, drivers dashboard
            │   └── copilot/               # AI chat widget (floating, role-aware)
            └── app.routes.ts
```

Dependency rule: `Domain` → depended on by `Application` → depended on by
`Infrastructure` and `Api`. Nothing in `Domain`/`Application` references
EF Core, Dapper, ASP.NET, or Stripe directly — only interfaces.

## 4. Domain model (Phase 1)

- `ApplicationUser` (Identity) → `RiderProfile` / `DriverProfile` (1:1, role-based)
- `Vehicle` (owned by a Driver)
- `Ride` — aggregate root: `Requested → DriverAssigned → DriverArriving → InProgress → Completed → Cancelled`, holds pickup/dropoff `GeoPoint` value objects, fare breakdown
- `Payment` — linked 1:1 to a completed `Ride`, Stripe PaymentIntent id, status
- `Rating` — rider→driver and driver→rider, linked to a completed `Ride`
- `DriverLocationPing` — latest known location per driver (upserted, broadcast over SignalR, not fully audited in Phase 1)
- `CopilotConversation` / `CopilotMessage` — chat history per user, so the copilot has memory

Domain events: `RideRequestedEvent`, `RideAcceptedEvent`, `RideCompletedEvent` → handled in Application layer to trigger notifications/payment capture without coupling entities to infrastructure.

## 5. Core ride flow (the "vertical slice")

1. Rider registers/logs in (JWT) → `RegisterRiderCommand`
2. Rider requests a ride (pickup/dropoff lat-lng) → `RequestRideCommand` → fare estimated via `IFareCalculator` (haversine distance × rate table)
3. Nearest **available** driver found via `FindNearbyDriversQuery` (Dapper, bounding-box + haversine over `DriverLocationPing`)
4. Driver receives the offer over SignalR, accepts → `AcceptRideCommand` (concurrency-checked: first acceptor wins)
5. Driver updates location during trip → SignalR hub → broadcast to rider
6. Driver marks ride complete → `CompleteRideCommand` → final fare locked → `Payment` created, captured via Stripe test-mode PaymentIntent
7. Both parties rate each other → `SubmitRatingCommand`
8. Copilot available throughout: rider can type "book me a ride to the airport" and the copilot calls the same `RequestRideCommand` handler via a tool/function definition; anyone can ask it "where's my driver" and it answers from live ride state

## 6. AI Copilot design

- `IAiCopilotService` in Application layer — provider-agnostic contract: `SendMessageAsync(conversationId, userMessage, userContext)`.
- Infrastructure has swappable providers behind that interface:
  - `AzureOpenAiCopilotProvider` (default — uses the free 30-day trial credit)
  - `GitHubModelsCopilotProvider` (genuinely free indefinitely, drop-in fallback)
  - `OllamaCopilotProvider` (fully local/offline, zero cost forever)
  - Selected via `appsettings.json` → `Copilot:Provider`, no code change to switch.
- **Two responsibilities, one chat surface:**
  - **Task execution** — function/tool calling maps natural language to existing MediatR commands/queries (request ride, cancel ride, check ride status, view fare estimate). The copilot never bypasses validation — it issues the same `IRequest` objects a controller would, so FluentValidation and domain rules still apply.
  - **Conversational response** — FAQ/support-style answers (fare explanation, ETA, policy questions) answered directly by the model with ride/account context injected into the system prompt.
- Chat history persisted (`CopilotConversation`/`CopilotMessage`) so context carries across turns without resending the whole transcript to the model every time (windowed history).

### 6a. RAG / vector search — grounding the "answer questions" side

Tool-calling covers actions and live ride state; it doesn't cover open-ended policy/FAQ questions
("what's your cancellation policy?"). For those, the copilot retrieves grounded context before
replying, instead of letting the model improvise an answer that might not match what the product
actually does:

- **Storage: SQL Server 2025's native `VECTOR` type** — no separate vector database service, so
  this stays inside the "free services only" rule. A `KnowledgeArticles` table
  (`Title`, `Content`, `Category`, `Embedding VECTOR(1536)`) holds short policy/FAQ snippets,
  created via a hand-written raw-SQL migration (`AddKnowledgeArticlesTable`) rather than an EF
  Core `DbSet` — the EF Core mapping for this brand-new SQL type isn't something to depend on for
  correctness yet, so it's Dapper-only, consistent with the rest of the CQRS read side.
- **Embeddings: `IEmbeddingService`**, the same provider-swap pattern as `IAiCopilotService` —
  `AzureOpenAiEmbeddingService` / `GitHubModelsEmbeddingService` / `OllamaEmbeddingService`,
  selected by the same `Copilot:Provider` setting. Defaults to `text-embedding-3-small`
  (1536 dimensions, matching the column). A local Ollama embedding model is *not* a drop-in here —
  most (e.g. `nomic-embed-text`) output 768 dimensions, not 1536 — pick a matching model or resize
  the column in a new migration before relying on Ollama for this piece.
- **Retrieval flow**: `SendCopilotMessageCommandHandler` embeds the user's message, searches
  `IKnowledgeBaseRepository.SearchByEmbeddingAsync` (raw SQL `VECTOR_DISTANCE('cosine', ...)`,
  no index — brute-force is plenty fast at FAQ-table sizes; a `CREATE VECTOR INDEX ... TYPE = 'DiskANN'`
  is a Phase 2/3 addition if the table grows large), and — for matches under a distance threshold —
  injects them into the system prompt as reference material the model is told to ground its answer
  in, without blocking tool-calling for action requests. The failure mode is deliberately soft: an
  empty knowledge base or an unreachable embedding provider just means no extra context, not a
  broken chat (see `IKnowledgeBaseSeeder`, called once at API startup, wrapped in try/catch).
- Seed content (`KnowledgeBaseSeedData`) is scoped to what Phase 1 actually does — cancellation
  rules, fare formula, payment flow, vehicle types, driver onboarding, ratings — and explicitly
  calls out unbuilt features (driver KYC, surge pricing, live chat) so the copilot doesn't imply
  they exist.

## 7. Payment integration (free)

- **Stripe, test mode only.** Test publishable/secret keys, test card numbers (`4242 4242 4242 4242`), full PaymentIntent lifecycle, refunds — all free, none of it touches real money or a real bank. This exercises the *exact same integration code* a production deployment would use; going live later is a key swap, not a rewrite.

## 8. Real-time layer

- ASP.NET Core **SignalR** hub (`RideHub`) — free, in-process (no Azure SignalR Service needed at this scale): driver location broadcast, ride-status push to rider, ride-offer push to driver.

## 9. Hosting (all free tiers, for later deployment — not needed for local dev)

- API: Azure App Service Free (F1) tier *or* run locally / on a free VM.
- DB: SQL Server Developer Edition locally (free, unlimited), or Azure SQL free tier (32GB, one per subscription) when ready to deploy.
- Frontend: Azure Static Web Apps free tier, or any static host.
- None of this is provisioned yet — Phase 1 runs entirely on localhost.

## 10. Phased plan

| Phase | Scope |
|---|---|
| **0 — done** | This roadmap, solution scaffolding, clean-architecture skeleton |
| **1 — this pass** | Domain + Application core, EF Core + Dapper + UoW, Auth (JWT), ride request→accept→complete→pay→rate flow, SignalR location, Stripe test-mode payment, Copilot chat (task + Q&A) with one provider wired, Angular app with rider/driver/admin/copilot modules calling real endpoints |
| **2** | Surge pricing, scheduled rides, promo codes, multiple vehicle tiers, driver document/KYC upload, in-app chat between rider/driver, push notifications |
| **3** | Admin analytics dashboard, driver payouts, dispute handling, rate limiting/hardening, load testing, CI/CD, deploy to free-tier Azure hosting |

## 11. What "clean architecture / CQRS / patterns" means concretely here

- **Clean Architecture**: dependency arrows point inward only; `Domain` has zero package references beyond the BCL.
- **CQRS**: MediatR `IRequest<T>` per command/query, one handler each, no god-services.
- **FluentValidation**: one validator class per command/query, wired via MediatR pipeline behavior — invalid requests never reach a handler.
- **Unit of Work**: `IUnitOfWork` wraps the EF Core `DbContext`'s `SaveChangesAsync` + transaction; repositories don't commit themselves.
- **Repository pattern**: EF Core repositories for aggregate writes; Dapper "query services" (not forced into the repository interface) for read-only projections — avoids the common CQRS anti-pattern of fake read-repositories.
