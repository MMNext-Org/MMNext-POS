# Discovery Report for MMNext POS (Phase A)

## 1. Core Domain Entities Identified in Legacy FusionPOS

The legacy application uses a Spring XML configuration (`app.config`) to wire a large number of **Value Objects (VOs)**[cite: 2]. The most relevant VOs for the new POS system are:

| VO ID | .NET Type | Description | Notes |
|---|---|---|---|
| `saleVo` | `business.domain.vo.Sale` | Represents a sales transaction (header)[cite: 2]. | Has a corresponding view model `vSaleVo` for read‑only queries[cite: 2]. |
| `saleDetailVo` | `business.domain.vo.SaleDetail` | Line‑items of a sale (product, quantity, price)[cite: 2]. | Also has `vSaleDetailSerialVo` for serial‑number handling[cite: 2]. |
| `saleTempVo` | `business.domain.vo.SaleTemp` | Temporary sales used during live editing[cite: 2]. | Used by live‑sale UI components[cite: 2]. |
| `customerVo` | `business.domain.vo.Customer` | Customer master data[cite: 2]. | Singleton and non‑singleton versions (`customerVo` / `customerSVo`)[cite: 2]. |
| `stockVo` | `business.domain.vo.Stock` | Product / inventory item[cite: 2]. | In the new design this will become the **Product** entity[cite: 2]. |
| `supplierVo` | `business.domain.vo.Supplier` | Supplier master data[cite: 2]. | May be needed for purchase workflows[cite: 2]. |
| `purchaseVo` | `business.domain.vo.Purchase` | Purchase transaction header[cite: 2]. | Not required for the initial sales‑only MVP[cite: 2]. |
| `transactionLogVo` | `business.domain.vo.TransactionLog` | Audit log for all changes[cite: 2]. | Useful for future logging/auditing[cite: 2]. |
| `userVo` | `business.domain.vo.User` | Application user (login/registration)[cite: 2]. | Provides the current user context[cite: 2]. |
| `saleByCategoryVo`, `topTenSaleVo`, `bottomSaleVo` | Reporting VOs | Provide pre‑aggregated sales statistics used by the reporting UI[cite: 2]. |

These VOs are defined as Spring objects and are instantiated at runtime; the source code does not contain explicit class definitions for many of them, but the types are present in the compiled assemblies[cite: 2]. For the new MMNext POS we will define clean POCO classes (`Sale`, `Product`, `Customer`, `Invoice`, …) that capture the same data fields needed for the core workflow[cite: 2].

---

## 2. UI Flow Overview

The legacy UI follows a classic WinForms Ribbon pattern[cite: 2]:

1. **Startup** – `Program.Main` loads `config.xml` (theme, font, language) and starts a self‑hosted Web API (`HttpSelfHostServer`)[cite: 2].
2. **Login/Registration** – `frmLogin` or `frmRegistration` is shown depending on the registration flag[cite: 2].
3. **Main Application** – `ribbonfrmMain` (inherits `DevExpress.XtraBars.Ribbon.RibbonForm`) provides a ribbon with tabs for *Sales*, *Purchases*, *Inventory*, *Reports*, *Settings*, etc[cite: 2].
4. **Module Forms** – Each functional area opens its own form (e.g., `frmSalesEdit`, `frmPurchaseEdit`, `frmStock`, `frmCustomerImport`)[cite: 2]. Most forms inherit from `XtraForm` (DevExpress) and communicate with the backend via the Spring‑wired services[cite: 2].
5. **Reporting** – Report forms (`rpt*` classes) are generated using `DevExpress.XtraReports`[cite: 2].

All forms are tightly coupled to the legacy service layer and perform synchronous database calls; there is no explicit use of `async/await`[cite: 2].

---

## 3. Pain Points & Technical Debt

| Area | Issue | Impact | Suggested Mitigation |
|---|---|---|---|
| **Target Framework** | Uses .NET Framework 4.5.2 (missing reference assemblies)[cite: 2]. | Build fails on modern SDKs[cite: 2]. | Migrate to .NET 6/7[cite: 2]. |
| **Synchronous DB Access** | Services call MySQL synchronously (blocking UI)[cite: 2]. | UI freezes during long operations[cite: 2]. | Introduce async repository methods (`MySqlConnector`)[cite: 2]. |
| **Memory Management** | Many DevExpress controls are not explicitly disposed[cite: 2]. | Potential memory leaks over long sessions[cite: 2]. | Implement `IDisposable` pattern in all forms and controls; enforce disposal in `Dispose(bool)`[cite: 2]. |
| **Spring Configuration** | Heavy XML‑based DI (Spring.NET)[cite: 2]. | Hard to understand and refactor[cite: 2]. | Replace with built‑in .NET DI (`Microsoft.Extensions.DependencyInjection`)[cite: 2]. |
| **Lack of Layer Separation** | UI directly references VO objects and services[cite: 2]. | Tight coupling, hard to unit‑test[cite: 2]. | Introduce clean Domain, Application, Infrastructure layers as per the new architecture[cite: 2]. |
| **Hard‑coded UI Themes & Fonts** | Themes are set from `config.xml` via DevExpress skin APIs[cite: 2]. | Difficult to change at runtime[cite: 2]. | Move them to `appsettings.json` and configure via DI[cite: 2]. |
| **No Automated Tests** | No unit or integration tests in repository[cite: 2]. | Regression risk[cite: 2]. | Add XUnit tests with Moq for services; integration tests for repositories using Docker‑MySQL[cite: 2]. |
| **DevExpress Licensing** | Requires a developer license for build machines[cite: 2]. | Build may fail on machines without license[cite: 2]. | Use DevExpress Community edition for development; document license steps for production[cite: 2]. |

---

## 4. High‑Level Migration Strategy

1. **Discovery** – Completed above[cite: 2]. Produce this report[cite: 2].
2. **Scaffold New Solution** – Create `MMNextPOS.sln` with four projects (Domain, Infrastructure, Application, WinForms) targeting .NET 6[cite: 2].
3. **Domain Layer** – Implement clean POCOs (`Sale`, `Product`, `Customer`, `Invoice`, `User`)[cite: 2].
4. **Infrastructure Layer** – Async MySQL repositories, generic base repository, proper `IDisposable`[cite: 2].
5. **Application Layer** – Service interfaces and implementations encapsulating business rules (stock checks, discounts, audit logging)[cite: 2].
6. **Presentation Layer** – New WinForms project using DevExpress `FluentDesignForm`[cite: 2]. Build ribbon navigation, async data loading, and CRUD dialogs that call the service layer[cite: 2].
7. **Cross‑Cutting** – Logging (Serilog), configuration (`appsettings.json`), global exception handling, cancellation tokens[cite: 2].
8. **Testing & CI** – XUnit tests for services, integration tests for repositories, GitHub Actions workflow[cite: 2].
9. **Documentation & Release** – README, architecture diagram, migration guide, self‑contained MSI installer[cite: 2].

---

## 5. Immediate Next Steps (Phase A Output)

- Save this `Discovery-Report.md` in the repository root[cite: 2].
- Create a `TODO.md` (or use the built‑in todo list) outlining the upcoming phases[cite: 2].
- Proceed with **Phase B – Scaffold** in the next implementation sprint[cite: 2].

---

*Prepared by the assistant based on the current FusionPOS codebase and the project roadmap.*[cite: 2]
