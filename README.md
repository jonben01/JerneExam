# Dead Pigeons

**3rd Semester Exam Project at EASV**  
Created by: **Jonas Bendorff** (solo)

A distributed system that digitizes Jerne IF’s “Dead Pigeons” game to make it easier to manage, scale, and track **digital participants** (while still supporting physical play alongside it).

**Deployed**
- Client: https://jonas-exam-web.fly.dev
- API: https://jonas-exam-api.fly.dev

## Demo login (seeded users)
> User creation/admin management exists in the API, but isn’t wired into the UI yet,
so in practice only these seeded accounts are usable for demo.

- **Admin**
  - Email: `admin@admin.dk`
  - Password: `123456`
- **Player**
  - Email: `active@test.dk`
  - Password: `123456`

---

## Tech stack

### Backend (API)
- ASP.NET Core Web API (**.NET 9.0**), C#
- PostgreSQL (hosted on **Neon**)
- Entity Framework Core
- Server-side validation (implemented in services/controllers, with some gaps noted below)
- ASP.NET Core Identity + JWT Bearer auth
- OpenAPI/Swagger via **NSwag** (also generates TS client in development)

### Frontend (client)
- React **19.1.1** + TypeScript **5.8.3** (Vite)
- React Router DOM **7.11.0**
- ESLint (+ react-hooks/react-refresh)
- react-toastify

---

## Repo structure
- `client/` - React + TypeScript (Vite)
- `server/Api/` - ASP.NET Core Web API + EF Core + Identity
- `server/Tests/` - xUnit tests (XUnit.DependencyInjection + Testcontainers)

---

## Secrets policy
- No production secrets are committed to git.
- Deployed configuration is provided via environment variables / Fly secrets.

---

## CI (GitHub Actions)

Workflow: `.github/workflows/ci.yml`

Runs on pushes/PRs to `master` and performs:
- Restore + build + test backend
- Install + lint + build frontend

---

## Environment, configuration & linting
- The system runs as two deployed services on Fly.io and a hosted Postgres database (Neon).
- Local config is handled via appsettings + environment variables; production uses Fly secrets.
- Frontend linting is enforced with ESLint (see `client` scripts).
- Backend uses standard dotnet build/test tooling (CI runs build + tests).

---

## Testing

Tooling:
- xUnit
- **XUnit.DependencyInjection** for test setup
- **Testcontainers.PostgreSql** for isolated DB per run

Coverage approach:
- 3-4 tests per service method tested
- At least 1 happy path + 1 unhappy path per method
- Current coverage focuses on service methods used by the client; most endpoints/services are not covered.

---

## Security policies (AuthN/AuthZ)

### Authentication
- Uses **ASP.NET Core Identity** for user management.
- Password hashing is Identity default (**PBKDF2**).
- Authentication is **JWT Bearer**.
- Token lifetime: **60 minutes**.

JWT validation enforces:
- issuer + audience
- signing key
- expiration (`ClockSkew = 0`)

### Authorization model
Roles:
- **Admin**
- **Player**

General rule:
- Most API controllers require authentication via `[Authorize]`.
- Admin-only endpoints use `[Authorize(Roles = "Admin")]`.

Client access:
- Unauthenticated users can only access the **home** and **login** pages.
- Authenticated users are routed to their role-specific UI.

### Additional policy
- `ActivePlayer` policy: requires claim `isActivePlayer = true`
  - Used to enforce: only active players may participate/buy boards (where applied).

### Access overview (high level)
- **Unauthenticated**
  - Can only access the home page and login flow.
  - No access to protected API controllers.
- **Player**
  - Can view the active game
  - Can buy boards (if allowed by game state / rules)
  - Can view board history
  - Can create deposit requests
- **Admin**
  - Can approve/reject deposits
  - Can end a game by publishing winning numbers

---

## Current state & limitations

### Current state
- API is mostly complete (major refactoring still needed).
- Frontend is **unpolished** and only implements a very small subset of the API (~25%)
- Some subscription-related logic exists, but the Board Subscription feature itself isn’t finished.

### Limitations / known issues
- **Frontend only covers a subset of the API**
  - A lot of functionality exists in the API but is not wired into the UI yet, e.g.:
    - user creation / admin user management *(so in practice only seeded users are usable through the UI right now)*
    - admin-side user updates
    - admin transaction/deposit history views
    - viewing more detailed board info / admin board tools

- **Board subscriptions are missing / incomplete**
  - The “Board Subscription” section is not finished and needs a full implementation pass.

- **Validation gaps around deleted users**
  - Some service methods are missing “deleted user” checks.
  - If a user had a board subscription and is deleted, they may continue playing until they run out of balance / total games on the subscription.
  - Deleted users cannot win, even if their previously purchased board would have been a winner.

- **Test coverage is incomplete**
  - Tests exist for service methods currently used by the client (happy + unhappy paths).
  - Most endpoints/services are not covered yet because the project is unfinished.

  ---

## API documentation
- Swagger/OpenAPI is provided via **NSwag**.
- A generated OpenAPI document is also available in the API project folder (`server/Api/openapi-with-docs.json`).


