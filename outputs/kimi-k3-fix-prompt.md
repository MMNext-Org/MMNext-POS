# Kimi-K3 Prompt — MMNext POS Audit Findings Resolution

You are Kimi-K3 acting as a senior .NET 8 / WinForms / MySQL / Dapper engineer. Work directly in the existing MMNext POS repository.

## Project context

This is a .NET 8 Windows Forms POS application using:

- .NET 8 and WinForms
- DevExpress for UI and reporting
- Layered architecture: Domain → Infrastructure → Application → WinForms Presentation
- MySQL with Dapper
- Microsoft.Extensions.DependencyInjection
- xUnit application tests
- Testcontainers.MySql integration tests

Repository root:

```text
J:\Project 1\MMNext POS
```

Read and follow:

- `.github/AGENTS.md`
- `README.md`
- `docs/`
- `outputs/project-audit-report.md`

## Important working-tree rule

The repository already contains user changes. **Do not reset, checkout, revert, clean, delete, or overwrite existing modifications.** Before editing:

1. Run `git status --short --branch`.
2. Run `git diff --stat` and inspect relevant diffs.
3. Inspect all four untracked feature files:
   - `src/MMNextPOS.Application/Services/CustomerAgeingEntry.cs`
   - `src/MMNextPOS.WinForms/ConnectionTestPage.cs`
   - `src/MMNextPOS.WinForms/Reports/CustomerAgeingReport.cs`
   - `tests/MMNextPOS.Application.Tests/OutstandingServiceAgeingTests.cs`
4. Preserve the existing work and build on top of it.
5. Do not commit or push unless explicitly instructed.
6. Keep unrelated changes out of the patch.

## Main objectives

Resolve the following audit findings with the smallest coherent production-quality changes:

### Objective 1 — Remove synchronous async waits from WinForms startup/shutdown

Current problematic locations include `src/MMNextPOS.WinForms/Program.cs`:

- `dbInit.InitializeAsync().GetAwaiter().GetResult()`
- `EnforceLicenseAsync(...).GetAwaiter().GetResult()`
- `healthCheckTask.Wait(...)`

Requirements:

1. Inspect the full startup flow before editing.
2. Remove avoidable `.GetAwaiter().GetResult()`, `.Result`, and `.Wait()` usage from the WinForms startup path.
3. Preserve `[STAThread]`, WinForms initialization order, license enforcement, database migration, login/logout loop, health-check endpoint startup, cancellation, logging, and exception behavior.
4. Use an async-compatible entry point/orchestration design appropriate for .NET 8 WinForms. Avoid introducing deadlocks or changing UI thread affinity.
5. Ensure shutdown cancellation is awaited safely and does not produce false fatal errors.
6. Do not solve this by merely suppressing warnings or hiding tasks with fire-and-forget behavior.
7. Add or update tests for any extracted startup orchestration logic where practical.

### Objective 2 — Replace inappropriate `NotImplementedException` feature gaps

Audit and resolve these known locations:

- `src/MMNextPOS.Application/Services/BarcodeService.cs`
- `src/MMNextPOS.Application/Services/ReportService.cs`

Known gaps:

- Barcode image generation currently throws `NotImplementedException`.
- Application report-generation methods currently throw `NotImplementedException` because DevExpress reporting is available in the WinForms layer.

Requirements:

1. Inspect the interfaces, callers, existing WinForms report classes, DevExpress dependencies, and current tests before choosing an implementation.
2. Preserve the layer boundary: do not introduce an invalid Application → WinForms circular dependency.
3. Prefer an abstraction/adapter/port design if the functionality belongs to Presentation or Infrastructure.
4. Implement barcode image generation using an appropriate existing project dependency or a minimal, justified dependency. Do not add a large library without explaining why.
5. Make report generation work through a clean abstraction or clearly separate application-level report data generation from WinForms/DevExpress rendering.
6. Replace exceptions that are reachable from user workflows with working behavior or a deliberate, typed capability error that is handled at the UI boundary. Do not leave a silent runtime crash.
7. Preserve existing public behavior where callers already rely on a presentation-layer implementation.
8. Add unit tests for valid input, invalid input, empty data, and the main report/barcode paths.
9. Avoid weakening existing tests merely to make them pass.

### Objective 3 — Make the working tree reviewable without losing user work

The current working tree has approximately 27 changed/untracked entries, including recent bilingual UI, customer-ageing, connection-test, report, installer, and translation work.

Requirements:

1. Do not discard existing work.
2. Review whether the four untracked files are complete and correctly wired into the solution/DI/UI/test structure.
3. Ensure newly added files compile, are referenced appropriately, and have tests where needed.
4. Detect accidental generated artifacts, duplicate implementations, partial edits, or unrelated changes.
5. Keep the final diff focused on the three objectives and required integration fixes.
6. Do not modify changelog/parity documentation unless the implementation genuinely changes the documented status; if it does, update only the relevant entries.
7. Do not commit.

## Additional hardening to perform while touching related code

- Verify no new `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` calls are introduced in production WinForms code.
- Ensure connection strings and secrets are not hard-coded. Keep `MMNEXTPOS_CONNECTION_STRING` override support.
- Do not replace placeholders with real secrets.
- Ensure production health-check configuration does not expose detailed exception data unintentionally.
- Preserve parameterized SQL. Do not concatenate user-controlled values into SQL.
- Preserve nullable reference type correctness and `TreatWarningsAsErrors=true`.
- Keep DI registrations correct and avoid duplicate service registrations.

## Required implementation workflow

### Phase 1 — Baseline

Run and record:

```powershell
git status --short --branch
git diff --stat
dotnet build MMNextPOS.slnx -c Release --no-restore
dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj -c Release --no-build
```

If the baseline build/test cannot run, diagnose the exact reason before editing.

### Phase 2 — Investigation

Inspect the complete relevant call graph and identify:

- Who calls the startup methods
- Which methods require the UI synchronization context
- Which services own barcode generation
- Which layer owns report data versus DevExpress rendering
- Which tests currently assert `NotImplementedException`
- Whether the untracked feature files are already referenced by DI, forms, reports, and tests

Write a short implementation plan before making edits. The plan must list affected files, risks, and acceptance criteria.

### Phase 3 — Implementation

Make the smallest coherent changes. Do not perform broad refactoring. Keep changes readable and consistent with the existing code style.

### Phase 4 — Verification

Run as many of the following as the environment supports:

```powershell
dotnet build MMNextPOS.slnx -c Release --no-restore
dotnet format MMNextPOS.slnx --verify-no-changes --no-restore
dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj -c Release --no-build
dotnet test tests/MMNextPOS.Infrastructure.Tests/MMNextPOS.Infrastructure.Tests.csproj -c Release
```

If Docker is unavailable, do not fake or skip the result silently. Report infrastructure tests as environment-blocked and include the exact Docker error. If Docker is available, run the full integration suite and investigate every actual failure.

Also run:

```powershell
git diff --check
git status --short --branch
```

Search production code for remaining blocking calls:

```powershell
Get-ChildItem src -Recurse -File -Filter *.cs |
  Select-String -Pattern '\.Result|\.Wait\(|GetAwaiter\(\)\.GetResult'
```

Search for remaining feature stubs:

```powershell
Get-ChildItem src -Recurse -File -Filter *.cs |
  Select-String -Pattern 'NotImplementedException|TODO|FIXME'
```

## Acceptance criteria

The work is complete only when all applicable criteria are satisfied:

- [ ] Release build succeeds with 0 warnings and 0 errors.
- [ ] Application unit tests pass.
- [ ] No avoidable synchronous async blocking remains in the WinForms startup/shutdown path.
- [ ] Startup still performs database initialization, license enforcement, health-check lifecycle, login loop, and graceful shutdown correctly.
- [ ] Barcode image generation has a working implementation or a cleanly handled capability boundary with tests.
- [ ] Report generation no longer causes an unexpected `NotImplementedException` in a reachable user workflow.
- [ ] Application and Presentation layer boundaries remain valid.
- [ ] Existing user changes and untracked feature files are preserved and integrated, not discarded.
- [ ] No real secrets are added.
- [ ] `dotnet format --verify-no-changes` passes.
- [ ] `git diff --check` passes.
- [ ] Integration test status is explicitly verified or clearly reported as Docker-blocked.
- [ ] No commit or push is performed.

## Final response format

At the end, provide:

1. **Summary of changes** — grouped by the three objectives.
2. **Files changed** — exact relative paths and why.
3. **Architecture decisions** — especially async startup and report/barcode boundaries.
4. **Tests run** — exact commands and pass/fail counts.
5. **Remaining risks** — only evidence-based items.
6. **Working-tree status** — confirm that existing user changes were preserved.
7. **No claims without evidence** — distinguish `PASS`, `FAIL`, `BLOCKED`, and `NOT RUN`.

Do not claim the task is complete if integration tests were not executed successfully. Do not commit, push, reset, clean, or delete user files.
