# AGENTS.md — MMNext POS

MMNext POS is a .NET 8 WinForms point-of-sale app (DevExpress) with a layered architecture: Domain → Infrastructure → Application → Presentation. MySQL via Dapper, xUnit + Testcontainers for tests.

## Solution & entrypoints

- `MMNextPOS.slnx` — 4 src projects + 2 test projects. Do not create a new solution.
- `src/MMNextPOS.Domain` — POCO entities, domain exceptions (framework-independent)
- `src/MMNextPOS.Infrastructure` — Dapper/MySQL repos, `DatabaseInitializer`, migrations
- `src/MMNextPOS.Application` — service interfaces + business rules
- `src/MMNextPOS.WinForms` — WinForms UI + DI bootstrap (`MainForm`, `Program.cs`)
- `tests/MMNextPOS.Application.Tests` — unit tests (mock repos)
- `tests/MMNextPOS.Infrastructure.Tests` — integration tests (Testcontainers MySQL 8.0)

## Developer commands (run from repo root)

| Action | Command |
|--------|---------|
| Restore | `dotnet restore MMNextPOS.slnx` |
| Build (Release) | `dotnet build MMNextPOS.slnx --configuration Release` |
| Format check | `dotnet format MMNextPOS.slnx --verify-no-changes` |
| Unit tests | `dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj --configuration Release` |
| Integration tests | `dotnet test tests/MMNextPOS.Infrastructure.Tests/MMNextPOS.Infrastructure.Tests.csproj --configuration Release` (needs Docker) |
| Run app | `dotnet run --project src/MMNextPOS.WinForms/MMNextPOS.WinForms.csproj` |

**Command order matters:** `restore → build → format --verify-no-changes → test`. CI runs format check as a gate; `--verify-no-changes` fails if `dotnet format` would rewrite anything.

## Target framework & toolchain

- .NET 8 SDK only. WinForms targets `net8.0-windows`. Nullable reference types and warnings-as-errors are enabled.
- **Do not** move CI or the solution to .NET 10 preview (plan.md §1 explicitly keeps .NET 8 as baseline).
- DevExpress is used; release CI sets `DEVEXPRESS_LICENSE_KEY=evaluation`. Confirm the package feed/license before publishing.

## Architecture rules (non-obvious, enforced in code review)

- Dependency direction: Domain → Infrastructure → Application → WinForms. Never move business rules into WinForms forms.
- UI pages inherit `AsyncFormBase`; long tasks never block the UI thread. No `.Result` / `.Wait()` on async calls in WinForms.
- Every DB call takes a `CancellationToken` and uses `IUnitOfWork` for the transaction boundary.
- Every form overrides `Dispose(bool)` and cancels its own `CancellationTokenSource`.
- Register services as `Scoped` for WinForms; repositories `Scoped` or `Transient`.
- All Dapper queries are parameterized. Never concatenate user input into SQL.
- Audit-log row written per write action.

## Environment variables

- `MMNEXTPOS_CONNECTION_STRING` — MySQL connection string. Fallback: `Server=localhost;Port=3306;Database=mmnextpos;User ID=root;` (no password).
- `NVIDIA_API_KEY` — set as a Windows **User** env var (never commit). Used by the local NVIDIA router for advisory only.
- `NVIDIA_API_BASE` — defaults to `https://integrate.api.nvidia.com/v1` (see `config/nvidia.env.example`).
- Model pins (`NVIDIA_MODEL_PLANNER`, `_CODER`, `_REVIEWER`, `_FAST`) are optional; leave blank to let the router discover compatible names from `/v1/models` at runtime. Never hard-code or claim a model is free without verifying.

## Database & migrations

- `DatabaseInitializer` creates the schema (`CREATE TABLE IF NOT EXISTS`).
- Migrations are tracked via `IMigrationRunner`, ordered `000`–`009+`, idempotent, checksum-verified. Do not hand-edit target rows during a migration incident.
- Integration tests spin up throwaway MySQL 8.0 containers via Testcontainers — no connection string env var is needed for them; the fixture provisions the container.
- Locally, integration tests require Docker Desktop. CI runs them on Ubuntu (windows-latest has no Docker daemon).

## CI quirks

- CI splits into two jobs: Windows (build + format + unit tests) and Ubuntu (infrastructure integration tests via Docker/Testcontainers). `concurrency` cancels in-progress runs.
- If you change formatting, run `dotnet format MMNextPOS.slnx` before pushing — CI fails on `--verify-no-changes`.

## Safety rules (approval gate)

- Present a plan (files to change, commands, tests, rollback) and get explicit approval before editing files or running mutating commands.
- Never commit API keys, connection strings with passwords, tokens, or personal data.
- Never `git reset --hard`, force-push, drop databases, or remove files without separate confirmation.
- For database changes, show the SQL/migration plan before execution.

## Existing instruction sources

- `.github/AGENTS.md` — agent conventions (this file is the compact version; link to it for detail)
- `.github/agents/mmnextpos-{orchestrator,planner,reviewer,implementer}.agent.md` — Copilot agent profiles with the Understanding → Affected areas → Task breakdown → Approval workflow
- `plan.md` — verified implementation roadmap and gap matrix
- `docs/parity/` — legacy FusionPOS parity matrix
- `README.md` — getting started, database setup, NVIDIA router usage