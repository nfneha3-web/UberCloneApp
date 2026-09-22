# RideShare — zero-cost ride-hailing platform

.NET 9 (Clean Architecture, CQRS via MediatR, FluentValidation, EF Core + Dapper, Unit of
Work) · Angular 21 (standalone components, signals) · SQL Server · Stripe (test mode) · a
swappable AI copilot (Azure OpenAI / GitHub Models / Ollama).

Read **[docs/ROADMAP.md](docs/ROADMAP.md)** first — it covers the architecture, the AI tooling
choices and why, and the phased plan. This README is just "how do I run it."

## Prerequisites (all free)

- .NET 9 SDK
- Node.js 20+ and npm
- **SQL Server 2025** (Express/Developer edition — this repo assumes a local `SQLEXPRESS`
  instance with Windows auth; adjust `ConnectionStrings:DefaultConnection` in
  `backend/src/Presentation/RideShare.Api/appsettings.json` if yours differs). The 2025 version
  specifically matters: the copilot's RAG knowledge base uses SQL Server 2025's native `VECTOR`
  type, which older versions don't have.
- A Stripe account (free) for **test mode** API keys — dashboard.stripe.com/test/apikeys
- One AI provider, whichever you prefer (see `docs/ROADMAP.md` §6/§6a for the free-tier tradeoffs):
  Azure OpenAI (30-day free trial), GitHub Models (free tier), or Ollama (local, always free).
  You need both a **chat** deployment/model and an **embedding** one (RAG search) — see below.

## First-time setup

1. **Configure secrets** in `backend/src/Presentation/RideShare.Api/appsettings.json`:
   - `Jwt:SigningKey` — any long random string
   - `Stripe:SecretKey` / `Stripe:PublishableKey` — your **test mode** keys (`sk_test_...` / `pk_test_...`)
   - `Copilot:Provider` plus whichever provider section you're using — each provider section has
     both a chat setting (`DeploymentName`/`Model`) and an embedding setting
     (`EmbeddingDeploymentName`/`EmbeddingModel`, used for the RAG knowledge base). Keep the
     embedding model at 1536 dimensions (e.g. `text-embedding-3-small`) unless you also change
     the `VECTOR(1536)` column in `AddKnowledgeArticlesTable`'s migration.
   - Also set `STRIPE_PUBLISHABLE_KEY` in `frontend/rideshare-web/src/app/core/config/app-config.ts`
     to the same `pk_test_...` key

2. **Apply database migrations** (from `backend/`):
   ```bash
   dotnet ef database update --project src/Infrastructure/RideShare.Infrastructure --startup-project src/Presentation/RideShare.Api --context RideShareDbContext
   dotnet ef database update --project src/Infrastructure/RideShare.Identity --startup-project src/Presentation/RideShare.Api --context ApplicationIdentityDbContext
   ```
   This also creates the `KnowledgeArticles` table (SQL Server 2025's native `VECTOR` type) that
   the copilot's RAG search uses — the app seeds it with starter FAQ/policy content automatically
   on first run, once your embedding provider is configured (see docs/ROADMAP.md §6a).

3. **Install frontend packages** (from `frontend/rideshare-web/`): `npm install`

## Running it

```bash
# Terminal 1 — API (http://localhost:5080, Swagger at /swagger)
cd backend/src/Presentation/RideShare.Api
dotnet run --urls "http://localhost:5080"

# Terminal 2 — Angular dev server (http://localhost:4200)
cd frontend/rideshare-web
npm start
```

Open http://localhost:4200, register as a rider in one browser (or profile) and as a driver in
another (auth state is per-origin `localStorage`, so use two separate browser profiles or a
regular + incognito window to run both sides at once).

## Project layout

```
backend/    RideShare.sln — Domain / Application / Infrastructure / Identity / Api, plus tests/
frontend/   rideshare-web — Angular workspace (core/shared/features)
docs/       ROADMAP.md — architecture, AI tooling rationale, phased plan
```

## Tests

```bash
cd backend
dotnet test tests/RideShare.Domain.Tests
dotnet test tests/RideShare.Application.Tests
```

## What's here vs. what's next

This is Phase 1 from the roadmap: a real, working vertical slice — auth, ride request → accept →
start → complete → pay → rate, live location/status over SignalR, and an AI copilot that can both
book/cancel rides (tool calls into the same CQRS commands a button click uses) and answer
questions. Surge pricing, scheduled rides, promo codes, multi-tier vehicle fleets, driver KYC
docs, and a real admin analytics dashboard are Phase 2/3 — see the roadmap for the full list.
