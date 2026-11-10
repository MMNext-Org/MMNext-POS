# AGENTS.md – AI Coding Agent Customization for MMNextPOS

This file provides conventions and guidelines to help AI coding agents be immediately productive when working on the MMNextPOS codebase.

## Project Overview

**MMNextPOS** is a modern, maintainable Windows desktop Point-of-Sale (POS) system built with:
- **.NET 8** with **WinForms** (DevExpress)
- **Layered architecture**: Domain → Infrastructure → Application → Presentation
- **MySQL** database with **Dapper** for data access
- **async/await** patterns with proper `ConfigureAwait`
- **xUnit** unit tests and **Testcontainers** for integration tests

## Architecture


Domain          → Pure POCO entities & domain exceptions
Infrastructure  → Async MySQL repos (Dapper), DatabaseInitializer
Application     → Services (ISalesService, ICustomerService) with business rules
Presentation    → WinForms UI (MainForm) via Microsoft.Extensions.DI


## Workflow

1. Inspect only the context needed for the request.
2. Explain affected files, dependencies, risks, and acceptance criteria.
3. Before any edit or mutating command, show the plan and ask for explicit approval.
4. After approval, make the smallest coherent change and report exact verification evidence.

## Build & Run Commands

| Action | Command |
|--------|---------|
| **Build solution** | `dotnet build -c Release` |
| **Run WinForms app** | `dotnet run --project src/MMNextPOS.WinForms/MMNextPOS.WinForms.csproj` |
| **Run unit tests** | `dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj` |
| **Run integration tests** | `dotnet test tests/MMNextPOS.Infrastructure.Tests/MMNextPOS.Infrastructure.Tests.csproj` |
| **Format check** | `dotnet format MMNextPOS.slnx --verify-no-changes` |
| **Collect coverage** | `dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj -c Release /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=coverage.opencover.xml` |
| **Generate coverage report** | `dotnet tool install --global dotnet-reportgenerator && reportgenerator -reports:coverage.opencover.xml -targetdir:coverlet/reports -reporttypes:HtmlInline_AzurePipelines;Cobertura` |
| **Self-contained publish** | `dotnet publish src/MMNextPOS.WinForms/MMNextPOS.WinForms.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish` |
| **Build MSI (WiX)** | `candle Installer.wxs && light -ext WixUIExtension Installer.wixobj -o MMNextPOS.msi` |

## Project Conventions

### Layered Architecture
- **Domain**: Add POCO entities, domain exceptions, and value objects. Keep it framework-independent.
- **Infrastructure**: Implement repositories using Dapper. All data access is async. Use `DatabaseInitializer` for schema creation.
- **Application**: Service layer with `IServices` and implementations. Business rules (stock validation, audit logging) go here.
- **WinForms**: UI consumes services via DI. Use MVVM or code-behind with minimal UI logic.

### Naming Conventions
- C#: PascalCase for types/methods, camelCase for parameters, UPPER_SNAKE_CASE for constants
- Files: `PascalCase.cs` for classes, `pascal-case.cs` for files (if standalone)
- Projects: `MMNextPOS.{Domain,Infrastructure,Application,WinForms}.{Tests}.csproj`

### Nullable Reference Types
- `#nullable enable` is enabled project-wide
- Use `notnull` annotations where appropriate
- Avoid `var` when the type isn't clear from context

### Testing
- **Unit tests** (`tests/MMNextPOS.Application.Tests`): Test service business logic in isolation (mock repositories)
- **Integration tests** (`tests/MMNextPOS.Infrastructure.Tests`): Use Testcontainers to spin up MySQL; test repository behavior
- Follow the **Arrange-Act-Assert** pattern
- Tests should build and run on CI before PR merge

## Common Pitfalls

| Issue | Guidance |
|-------|----------|
| **Blocking UI** | Never use `.Result` or `.Wait()` on async calls in WinForms UI thread – use `await` instead or post to synchronization context |
| **Connection lifetime** | MySQL connections should be opened per-operation or via using statement; do not keep static open connections |
| **Dapper queries** | Use parametrized queries; never concatenate user input into SQL strings |
| **Configuration** | Connection string comes from `MMNEXTPOS_CONNECTION_STRING` environment variable; fall back to `Server=localhost;Port=3306;Database=mmnextpos;User ID=root;` if not set |
| **DI lifetime** | Register services as `Scoped` for WinForms apps; repositories can be `Scoped` or `Transient` |

## Quality gates

MMNextPOS enforces the following quality gates in CI (`.github/workflows/ci.yml`) and locally:

| Gate | Command | Purpose |
|------|---------|---------|
| Format | `dotnet format MMNextPOS.slnx --verify-no-changes` | Enforces `.editorconfig` whitespace/style rules; fails CI on drift |
| Build | `dotnet build MMNextPOS.slnx -c Release -warnaserror` | Compiles all projects; warnings-as-errors (DevExpress DX1000 eval warning exempted) |
| Analyze | SDK analyzers via `EnableNETAnalyzers=true` + `AnalysisLevel=latest` (all 6 csproj) + `.editorconfig` severity rules | Static analysis; code-style conventions documented but not enforced (`:none`) |
| Unit tests | `dotnet test tests/MMNextPOS.Application.Tests/... -c Release` | 330 xUnit tests, mock repos, must be green |
| Integration tests | `dotnet test tests/MMNextPOS.Infrastructure.Tests/... -c Release` | Testcontainers MySQL 8.0 (Ubuntu CI only; needs Docker) |
| Coverage | coverlet.collector (opencover) + ReportGenerator report, published as CI artifact | Application-layer coverage measured per Windows CI job; no hard threshold gate yet (Phase 2 baseline) |

Analyzer/tool choices:
- **SDK analyzers only** — no StyleCop/Roslynator/Meziantou. The codebase passes clean with `TreatWarningsAsErrors=true`; third-party analyzers would add failures without closing a real Phase-2 gap. Revisit if the team wants stricter style enforcement.
- **No NetArchTest architecture tests yet** — layer boundaries (Domain → Infrastructure → Application → WinForms) are enforced by code review per the architecture rules above. Documented as a future consideration.
- **Coverage:** `coverlet.collector` is already referenced in both test projects; CI collects opencover output and generates an HTML/Cobertura report via the `dotnet-reportgenerator` tool.

## Key Files to Know

| File/Path | Purpose |
|-----------|---------|
| `src/MMNextPOS.Domain/` | Entity POCOs, domain exceptions |
| `src/MMNextPOS.Infrastructure/` | Dapper repositories, `DatabaseInitializer.cs` |
| `src/MMNextPOS.Application/` | Service interfaces and implementations |
| `src/MMNextPOS.WinForms/` | MainForm, Program.cs (DI bootstrap) |
| `tests/MMNextPOS.Application.Tests/` | Unit tests for services |
| `tests/MMNextPOS.Infrastructure.Tests/` | Integration tests with Testcontainers |
| `.github/workflows/ci.yml` | CI pipeline (build, test, integration tests) |
| `README.md` | Getting started, database setup, running the app |

## Getting Started for New Agents

1. **Fork and clone** the repository
2. **Install .NET 8 SDK**: https://dotnet.microsoft.com/download/dotnet/8.0
3. **Set up MySQL** locally or ensure Docker is running for integration tests
4. **Set environment variable**: `$env:MMNEXTPOS_CONNECTION_STRING = "Server=localhost;Port=3306;Database=mmnextpos;User ID=root;Password=yourPassword;Pooling=true;"`
5. **Build**: `dotnet build -c Release`
6. **Run**: `dotnet run --project src/MMNextPOS.WinForms/MMNextPOS.WinForms.csproj`
7. **Tests**: `dotnet test --filter FullyQualifiedName~ServiceTests` (run specific test categories)

## Linking to Documentation

When referencing external documentation or conventions, use Markdown links:
- Architecture overview: `[README.md](../../README.md#architecture-overview)`
- CI pipeline: `[.github/workflows/ci.yml](.github/workflows/ci.yml)`
- Database schema: `[DatabaseInitializer.cs](src/MMNextPOS.Infrastructure/DatabaseInitializer.cs)`

Do not duplicate existing documentation – link to it instead when possible.