# Copilot Instructions — MMNext POS

MMNext POS is a .NET 8 WinForms (DevExpress) point-of-sale app with a layered architecture:
Domain → Infrastructure → Application → Presentation. MySQL via Dapper, xUnit + Testcontainers for tests.
This file consolidates the guidance in `AGENTS.md`, `.github/AGENTS.md`, `README.md`, `plan.md`, and CI workflows
so future Copilot sessions can work effectively without re-reading every file.

## Build, test, and lint commands

Run all commands from the repo root. Target framework: .NET 8 SDK only; WinForms targets `net8.0-windows`;
nullable reference types and warnings-as-errors are enabled. Do not move CI to .NET 10 preview (`plan.md` §1).

| Action | Command |
|--------|---------|
| Restore | `dotnet restore MMNextPOS.slnx` |
| Build (Release) | `dotnet build MMNextPOS.slnx --configuration Release` |
| Format check (CI gate) | `dotnet format MMNextPOS.slnx --verify-no-changes` |
| Apply formatting | `dotnet format MMNextPOS.slnx` |
| Unit tests (full suite) | `dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj --configuration Release` |
| **Single test / test class** | `dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj --configuration Release --filter "FullyQualifiedName~SalesServiceTests"`<br>or narrower: `--filter "FullyQualifiedName~SalesServiceTests.CreateSale_ValidRequest"` |
| Integration tests | `dotnet test tests/MMNextPOS.Infrastructure.Tests/MMNextPOS.Infrastructure.Tests.csproj --configuration Release` (needs Docker; Testcontainers spins up MySQL 8.0, no connection string env var required) |
| Run app | `dotnet run --project src/MMNextPOS.WinForms/MMNextPOS.WinForms.csproj` |

**Command order matters:** `restore → build → format --verify-no-changes → test`. CI runs `dotnet format --verify-no-changes`
as a gate — it fails if formatting would rewrite anything, so run `dotnet format` first if you changed code.

**Single-test tip:** xUnit uses `[Fact]`. Filter by `FullyQualifiedName` (namespace + class + method). Integration tests use
`[Collection(nameof(MySqlContainerCollection))]` and share one MySQL container via `MySqlContainerFixture`.

## High-level architecture

Dependency direction is strictly `Domain → Infrastructure → Application → WinForms`. Never move business rules into WinForms forms.

- **`src/MMNextPOS.Domain`** — POCO entities (`EntityBase` base), domain exceptions, value objects. Framework-independent.
- **`src/MMNextPOS.Infrastructure`** — Dapper/MySQL repositories (`RepositoryBase`, `GenericRepository`, plus per-table repos), `DatabaseInitializer` (`CREATE TABLE IF NOT EXISTS`), `IUnitOfWork` / `MySqlUnitOfWork` transaction boundary, `IMigrationRunner` migrations (`Migrations/000–009+.sql`, idempotent, checksum-verified).
- **`src/MMNextPOS.Application`** — service interfaces + implementations (e.g. `ISalesService`/`SalesService`) with business rules: stock validation, audit logging, invoice/return numbering.
- **`src/MMNextPOS.WinForms`** — WinForms UI + DI bootstrap (`Program.cs`, `MainForm` shell). Forms inherit `AsyncFormBase` (`XtraForm`).

**Request flow:** WinForms form → application service (via `IServiceProvider`-resolved scoped services) → repository (`IUnitOfWork`) → Dapper parameterized query against MySQL. Every DB call takes a `CancellationToken` and runs inside an `IUnitOfWork` transaction; an audit-log row is written per write action.

**Tests:** `tests/MMNextPOS.Application.Tests` mock repositories and test service logic in isolation (Moq). `tests/MMNextPOS.Infrastructure.Tests` spin up throwaway MySQL 8.0 containers via Testcontainers and test repository/migration behavior (shared `MySqlContainerFixture`, plus `UnitOfWorkTests` with its own container).

## Key conventions (non-obvious)

- **UI threading:** No `.Result` / `.Wait()` on async calls in WinForms. Use `await`; `AsyncFormBase.RunAsync` wraps calls with a wait cursor and `ConfigureAwait(true)`.
- **Forms:** Every form overrides `Dispose(bool)` and cancels its own `CancellationTokenSource`; `AsyncFormBase` provides `CancellationToken`, `ShowError/ShowInfo/ShowConfirm`, `SetWaitCursor`.
- **DI:** Register services as `Scoped` for WinForms; repositories `Scoped` or `Transient` (`DependencyInjection.cs`).
- **SQL:** All Dapper queries are parameterized. Never concatenate user input into SQL.
- **Connection string:** `MMNEXTPOS_CONNECTION_STRING` env var; fallback `Server=localhost;Port=3306;Database=mmnextpos;User ID=root;` (no password).
- **Secrets:** `NVIDIA_API_KEY` is a Windows **User** env var for the local NVIDIA advisory router only — never commit API keys, connection strings with passwords, or tokens. `NVIDIA_MODEL_*` pins are optional; leave blank to let the router discover names from `/v1/models`.
- **Migrations:** Do not hand-edit target rows during a migration incident; migrations are idempotent and checksum-verified.
- **Formatting:** `.editorconfig` enforces UTF-8, CRLF, 4-space indent. `dotnet format` output must be commit-clean (CI gate).
- **CI:** Two jobs — Windows (build + format + unit tests) and Ubuntu (infrastructure integration tests via Docker/Testcontainers). `concurrency` cancels in-progress runs. DevExpress release CI sets `DEVEXPRESS_LICENSE_KEY=evaluation`; confirm the package feed/license before publishing.

## Related documentation

- `AGENTS.md` — compact agent conventions (this file links to it for detail)
- `.github/AGENTS.md` — detailed AI coding agent conventions, key files, getting started
- `.github/agents/mmnextpos-{orchestrator,planner,reviewer,implementer}.agent.md` — Copilot agent profiles with Understanding → Affected areas → Task breakdown → Approval workflow
- `plan.md` — verified implementation roadmap and gap matrix
- `docs/parity/` — legacy FusionPOS parity matrix
- `.github/workflows/ci.yml` — CI pipeline
- `README.md` — getting started, database setup, NVIDIA router usage

## Safety rules (approval gate)

Present a plan (files to change, commands, tests, rollback) and get explicit approval before editing files or running mutating commands.
Never `git reset --hard`, force-push, drop databases, or remove files without separate confirmation. For database changes, show the SQL/migration plan before execution.