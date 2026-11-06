# MMNext POS — Project Audit Report

**စစ်ဆေးသည့်နေ့**: 2026-10-09  
**Repository**: `MMNext POS`  
**Branch**: `main` (`origin/main` နှင့် sync ဖြစ်နေ)  
**Scope**: Architecture, source structure, build, unit/integration tests, CI/CD, configuration/security hotspots, working-tree integrity

## 1. Executive Summary

| အပိုင်း | ရလဒ် | အဓိပ္ပာယ် |
|---|---|---|
| Release build | **PASS** | `dotnet build MMNextPOS.slnx -c Release --no-restore` အောင်မြင်၊ warning 0 / error 0 |
| Application unit tests | **PASS** | 314 passed, 0 failed, 0 skipped |
| Infrastructure integration tests | **BLOCKED/FAIL** | 31 tests အားလုံး Docker daemon မရရှိသဖြင့် `DockerUnavailableException` ဖြစ် |
| Format check | **PASS** | `dotnet format MMNextPOS.slnx --verify-no-changes --no-restore` အောင်မြင် |
| Diff whitespace integrity | **PASS** | `git diff --check` တွင် whitespace error မတွေ့ |
| Release readiness | **NOT READY** | Release workflow တွင် Docker မပါသော Windows runner ပေါ် full solution tests run လုပ်ထားသောကြောင့် release pipeline fail နိုင် |

### Overall assessment

Project သည် .NET 8 WinForms + DevExpress + Dapper/MySQL အခြေပြု layered POS application အဖြစ် တည်ဆောက်ထားပြီး source architecture နှင့် application-level test coverage ကောင်းမွန်သည်။ သို့သော် **database integration verification မပြီးသေးခြင်း**, **release CI test orchestration မညီခြင်း**, **production configuration တွင် placeholder/default credentials ရှိခြင်း**, နှင့် **အချို့ feature API များ runtime တွင် `NotImplementedException` ဖြင့် ကျန်ရှိခြင်း**တို့ကြောင့် production/release readiness ကို အတည်ပြု၍ မရသေးပါ။

## 2. Project Structure & Architecture

### Observed structure

- `src/MMNextPOS.Domain` — POCO domain models and domain exceptions
- `src/MMNextPOS.Infrastructure` — MySQL/Dapper repositories, migrations, unit of work, database initializer
- `src/MMNextPOS.Application` — business services and service interfaces
- `src/MMNextPOS.WinForms` — WinForms presentation layer, DevExpress reports, DI bootstrap
- `tests/MMNextPOS.Application.Tests` — application service unit tests
- `tests/MMNextPOS.Infrastructure.Tests` — Testcontainers/MySQL integration tests
- `.github/workflows/ci.yml` — build/unit/integration CI
- `.github/workflows/release.yml` — publish/release workflow
- `src/MMNextPOS.Infrastructure/Migrations` — versioned SQL migrations `000` through `016`

### Metrics observed

- Source C# files: **508**
- Source C# lines: **37,456**
- Test C# files: **53**
- Test C# lines: **7,732**
- Documentation files under `docs`: **20**
- Layered architecture boundaries are represented in the solution and project references.
- Projects use nullable reference types and `TreatWarningsAsErrors=true`.

### Positive observations

1. Solution separates Domain, Infrastructure, Application and Presentation concerns.
2. Infrastructure has versioned embedded migrations and schema validation flow.
3. Application tests cover a broad set of services including sales, purchases, inventory, users, payments, serial numbers, expiry and outstanding ageing.
4. Dapper queries generally use parameters for user values; no obvious user-input SQL concatenation was found in the reviewed hotspots.
5. CI has a separate Ubuntu job intended for Docker/Testcontainers integration tests.

## 3. Verification Evidence

### 3.1 Build

Command:

```text
dotnet build MMNextPOS.slnx -c Release --no-restore
```

Result:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

All six solution projects compiled, including `MMNextPOS.WinForms` targeting `net8.0-windows`.

### 3.2 Application unit tests

Command:

```text
dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj \
  -c Release --no-build --logger "console;verbosity=minimal"
```

Result:

```text
Passed: 314
Failed: 0
Skipped: 0
Total: 314
```

### 3.3 Infrastructure integration tests

Command:

```text
dotnet test MMNextPOS.slnx -c Release --no-build
```

Result:

```text
MMNextPOS.Application.Tests: 314 passed
MMNextPOS.Infrastructure.Tests: 31 failed
```

The 31 failures share the same environment/root cause:

```text
DockerUnavailableException
Failed to connect to Docker endpoint at npipe://./pipe/docker_engine
```

The failure occurs while `MySqlContainerFixture` and `UnitOfWorkTests` create `MySqlBuilder` Testcontainers instances. Therefore, the integration tests do **not** currently prove that migrations, repositories, transaction behavior, or sales/purchase persistence pass on this device.

### 3.4 Formatting and diff integrity

- `dotnet format ... --verify-no-changes`: **PASS**
- `git diff --check`: **PASS**
- Git emitted only normal LF-to-CRLF working-copy warnings for some modified files; no whitespace error was reported.

### 3.5 Package audit limitation

The following commands could not complete because the installed .NET SDK/NuGet evaluation returned `Value cannot be null. (Parameter 'path1')` while determining restore metadata:

```text
dotnet list MMNextPOS.slnx package --vulnerable --include-transitive
dotnet list MMNextPOS.slnx package --outdated
```

Dependency vulnerability/outdated status is therefore **not verified** in this audit and should be run in CI or with a clean compatible SDK/restore environment.

## 4. Findings and Risk Ranking

### High priority

#### H1 — Release workflow can fail because it runs Docker-dependent tests on Windows without Docker

**Evidence**: `.github/workflows/release.yml:72` runs:

```text
dotnet test ${{ env.SOLUTION_FILE }} --configuration Release --no-build
```

The solution includes `MMNextPOS.Infrastructure.Tests`, whose fixtures create MySQL Testcontainers. The release job is `windows-latest`; unlike the dedicated CI integration job, it does not start/verify Docker or separate the test project. The local evidence is 31/31 infrastructure test failures from the missing Docker named pipe.

**Impact**: A release tag/manual release may fail before publish, or release behavior may differ from CI.

**Recommendation**:

1. In `release.yml`, run only `MMNextPOS.Application.Tests` in the Windows release-build job, or add a separate Linux integration-test job and make release depend on it.
2. Prefer reusing the already-correct split in `ci.yml`.
3. Add an explicit release precondition stating whether integration tests are required before artifact publication.

#### H2 — Production configuration contains blank/default database password and placeholder Turnstile secrets

**Evidence**:

- `src/MMNextPOS.WinForms/appsettings.json:3` has `Password=;`
- `src/MMNextPOS.Infrastructure/appsettings.json:3` has `Password=;`
- `src/MMNextPOS.WinForms/appsettings.json:7-8` has `YOUR_SITE_KEY` and `YOUR_SECRET_KEY`

The application does support `MMNEXTPOS_CONNECTION_STRING` override, which is good, but the checked-in defaults are unsafe as deployment configuration and can cause runtime failure or accidental insecure DB setup.

**Recommendation**:

1. Replace real appsettings defaults with empty/non-secret placeholders or a clearly named development-only profile.
2. Fail fast with a clear startup error when a production connection string or Turnstile secret is missing.
3. Use environment variables/Windows Credential Manager/secret store for credentials.
4. Add a CI secret/configuration validation step that rejects placeholder values in release artifacts.

#### H3 — Full integration behavior remains unverified on the current environment

The application unit suite passes, but all 31 infrastructure tests were blocked by Docker availability. This means migration idempotence, MySQL schema alignment, transaction rollback, concurrency, Starman flows, and repository persistence are not proven in this audit.

**Recommendation**: Run the dedicated integration test project with Docker Desktop running, or provide a reachable external MySQL instance through `MMNEXTPOS_CONNECTION_STRING`. Record the resulting `.trx` output as a release artifact.

### Medium priority

#### M1 — Synchronous waits exist in WinForms startup/shutdown path

**Evidence**: `src/MMNextPOS.WinForms/Program.cs`:

- line 150: `dbInit.InitializeAsync().GetAwaiter().GetResult()`
- line 158: `EnforceLicenseAsync(...).GetAwaiter().GetResult()`
- line 196: `healthCheckTask.Wait(...)`

The repository guidance explicitly warns against blocking async calls on the WinForms UI thread. These calls are in startup/shutdown rather than ordinary button handlers, so deadlock risk is lower, but startup responsiveness and exception propagation remain concerns.

**Recommendation**: Move startup orchestration into an async-compatible entry flow or execute initialization before UI thread interaction with explicit cancellation and timeout handling. Keep the shutdown wait only if it is proven necessary, and avoid blocking the UI thread.

#### M2 — Intentional but incomplete runtime APIs use `NotImplementedException`

**Evidence**:

- `src/MMNextPOS.Application/Services/BarcodeService.cs:128` — barcode image generation
- `src/MMNextPOS.Application/Services/ReportService.cs:168,173` — report generation delegated to WinForms layer

Some are architectural boundary decisions, but barcode image generation is an actual capability gap if callers expect the Application service to generate an image. Existing tests assert the exception, so the gap is currently encoded rather than accidentally hidden.

**Recommendation**: Document these as explicit capability boundaries in public interfaces, or implement them behind an adapter. Add user-facing error handling where a UI action can reach these methods.

#### M3 — Working tree has 27 uncommitted entries and includes four untracked feature files

**Observed status**:

- 23 modified tracked files
- 4 untracked files:
  - `CustomerAgeingEntry.cs`
  - `ConnectionTestPage.cs`
  - `CustomerAgeingReport.cs`
  - `OutstandingServiceAgeingTests.cs`

There are also changes across translations, reports, installer scripts, forms, configuration, and changelog. These changes were not altered by this audit.

**Impact**: The repository is not at a clean, reviewable baseline; it is difficult to distinguish intended feature work from partial/in-progress work.

**Recommendation**: Review and commit the current feature as a coherent change set, or stash/branch it before further work. Run the full test matrix after the untracked files are confirmed as intended.

#### M4 — CI and release test policy is inconsistent

`ci.yml` correctly separates Windows build/unit tests and Ubuntu Docker integration tests. `release.yml` instead runs the entire solution test set on Windows. This is a process/design inconsistency beyond the immediate Docker failure.

**Recommendation**: Define one canonical test matrix and reference it consistently for PR, main, and release workflows.

### Low priority

#### L1 — `SELECT *` is used in several repositories

This is not an SQL injection finding by itself, and the reviewed values are parameterized. However, `SELECT *` increases coupling to schema changes and can retrieve unnecessary columns.

**Recommendation**: Prefer explicit column lists in high-traffic repositories and generic repository paths, especially for reporting and large tables.

#### L2 — One TODO remains in a test-side UI stub

`tests/UsersListPage.cs:51` contains `TODO: Implement UserEditForm`. Confirm whether this is a legacy test artifact or remove it from the product test tree if obsolete.

## 5. Security and Configuration Review

### Good controls observed

- Connection string can be overridden via `MMNEXTPOS_CONNECTION_STRING`.
- Dapper repositories use parameterized values for ordinary query inputs.
- Password-related services appear to be represented as dedicated application flows; no committed real credential was detected by the pattern scan.
- CI permissions are mostly read-only in `ci.yml`.
- Release build uses self-contained Windows publishing with trimming disabled, which is safer for reflection-heavy DevExpress applications.

### Items requiring attention

- Blank passwords in checked-in runtime configuration.
- Placeholder Cloudflare Turnstile values in appsettings.
- `HealthChecks:DetailedErrors` is enabled in the checked-in configuration; detailed exception output should not be exposed in production.
- Serilog sends to local file, Seq and Elasticsearch endpoints configured in appsettings; production endpoint authentication and sensitive-field redaction should be verified.
- Package vulnerability scan was inconclusive due to NuGet command failure.

## 6. Test Coverage Assessment

### Current strengths

- 314 application unit tests pass.
- Services with significant business impact have dedicated tests: sales, purchase, inventory, payment, stock transfer, returns, users, serial numbers, expiry and outstanding ageing.
- Integration tests cover migration chain, transaction behavior, sales/purchase flows and Starman-related flows.

### Gaps / next verification steps

1. Start Docker Desktop and rerun all infrastructure tests.
2. Capture pass/fail counts per test class and inspect any failures beyond environment setup.
3. Run `dotnet test` with code coverage for the application project and publish coverage artifacts.
4. Add a smoke test for WinForms startup with a valid test DB/configuration if feasible.
5. Add a release configuration test that checks placeholders and health-check exposure.
6. Add a CI test that validates the same test matrix used by release.

## 7. Recommended Action Plan

### P0 — Before release

1. Fix `release.yml` so Docker/Testcontainers tests run on a Docker-capable Linux job or are explicitly delegated to the integration CI job.
2. Run infrastructure tests with Docker and resolve any actual test failures.
3. Remove/override blank DB credentials and placeholder Turnstile settings in release configuration.
4. Confirm the 27 working-tree entries are intentional and prepare a clean reviewable commit.

### P1 — Next engineering pass

1. Replace startup blocking waits with a controlled async startup/shutdown design.
2. Decide whether barcode image generation belongs in Application or an infrastructure/presentation adapter; implement or document the boundary.
3. Add package vulnerability/outdated checks using a clean SDK restore environment.
4. Add release smoke checks for configuration, migration status and health-check security.

### P2 — Maintainability

1. Reduce `SELECT *` usage in repositories.
2. Remove stale TODO/test artifacts.
3. Add coverage thresholds for critical business services.
4. Keep CI/release workflows derived from one canonical test matrix.

## 8. Final Verdict

**Status: Conditional / Not release-ready yet.**

The codebase is in a reasonably strong state at compile and application-service level: the Release build is clean, formatting is clean, and 314 application tests pass. The release decision must remain blocked until Docker-backed infrastructure tests are executed successfully and the release workflow/configuration issues are corrected. No source files were modified during this audit; only this report was created.

## Appendix — Key Evidence Paths

- [`.github/AGENTS.md](../.github/AGENTS.md)
- [`MMNextPOS.slnx`](../MMNextPOS.slnx)
- [`src/MMNextPOS.WinForms/Program.cs`](../src/MMNextPOS.WinForms/Program.cs)
- [`src/MMNextPOS.WinForms/appsettings.json`](../src/MMNextPOS.WinForms/appsettings.json)
- [`src/MMNextPOS.Infrastructure/DatabaseInitializer.cs`](../src/MMNextPOS.Infrastructure/DatabaseInitializer.cs)
- [`tests/MMNextPOS.Infrastructure.Tests/MySqlContainerFixture.cs`](../tests/MMNextPOS.Infrastructure.Tests/MySqlContainerFixture.cs)
- [`.github/workflows/ci.yml`](../.github/workflows/ci.yml)
- [`.github/workflows/release.yml`](../.github/workflows/release.yml)
