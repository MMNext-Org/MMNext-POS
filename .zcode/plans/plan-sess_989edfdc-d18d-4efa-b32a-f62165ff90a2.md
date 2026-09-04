Implementation Plan for MMNext POS (Windows WinForms):

Phase A – Discovery: Analyze FusionPOS project, list core entities (Sale, Product, Customer...), extract DB schema, map UI flows, produce Discovery-Report.md.
Phase B – Scaffold: Create solution MMNextPOS.sln with four projects (Domain, Infrastructure, Application, WinForms) targeting .NET 6, add project references and NuGet packages (Microsoft.Extensions.DependencyInjection, Serilog, MySqlConnector, DevExpress.Win).
Phase C – Domain: Add clean POCO classes for Sale, Product, Customer, Invoice in Domain/Models with data annotations.
Phase D – Infrastructure: Implement generic RepositoryBase<T> with async CRUD, concrete MySql repositories, ensure IDisposable.
Phase E – Application: Define service interfaces (ISalesService etc.), implement business rules (stock checks, discounts), async methods, custom exceptions, register with DI.
Phase F – Presentation: Scaffold WinForms project with FluentDesignForm MainForm, ribbon navigation, async data‑bound GridControl for sales, CRUD dialogs calling services, global exception handling, proper disposal.
Phase G – Cross‑cutting: Configure Serilog logging to logs folder, add appsettings.json, cancellation token handling, global error handling.
Phase H – Testing & CI: Write unit tests for services (Moq) and integration tests for repositories (Docker MySQL), add GitHub Actions CI workflow.
Phase I – Documentation: Update README with build/run steps, architecture diagram (Mermaid), migration guide from FusionPOS DB.
Phase J – Release: Publish self‑contained executable, create MSI installer, provide silent install script.

Timeline: 8 weeks as detailed in the plan.
Risks and mitigations: DB incompatibility, DevExpress licensing, async deadlocks, memory leaks, team unfamiliarity with DI.

Next step: Execute Phase A – Discovery and generate Discovery-Report.md.