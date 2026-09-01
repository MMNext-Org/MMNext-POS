# 🚀 Project Name: NextGen POS (Point of Sale) System

## 1. Project Vision & Goals
This project is a complete rewrite and modernization of a legacy WinForms POS application. 
The main goals are:
- To eliminate UI-freezing and memory leak issues from the legacy system.
- To decouple the UI from the database using a clean N-Tier architecture.
- To utilize modern DevExpress UI controls for a premium, fast, and user-friendly experience.
- To make the database scalable, reliable, and EF Core ready.

## 2. Technology Stack & Architecture
- **Presentation Layer:** C# WinForms with DevExpress (Fluent Design, DocumentManager, XtraGrid).
- **Business Logic Layer (BLL):** Service classes containing core POS rules (Checkout, Discounts, Inventory Deductions).
- **Data Access Layer (DAL):** Repository Pattern using Entity Framework Core (SQL Server / SQLite).
- **Concurrency & Performance:** Heavy use of Asynchronous programming (`async/Task`) and DevExpress Server Mode (`XPServerCollectionSource`) for large datasets.

## 3. Core Modules & Feature Roadmap
### 🟢 Phase 1: Foundation & Database
- Design EF Core Code-First models (Products, Categories, Sales, SaleDetails, Users).
- Implement generic Repository and Unit of Work patterns.
- Setup Dependency Injection (DI) for WinForms.

### 🟡 Phase 2: Inventory & Product Management
- Modern Product List View using DevExpress GridControl.
- CRUD operations for items with Barcode support.
- Image storage strategy (File system vs Database).

### 🟠 Phase 3: The POS / Checkout Core
- High-speed Barcode Scanner input handling.
- Real-time cart calculation (Subtotal, Tax, Discount, Total).
- Transactional database saves (Rollback on error).
- Offline tolerance (Optional).

### 🔴 Phase 4: Reporting & Security
- Receipt generation using DevExpress `XtraReports`.
- Role-based Access Control (Admin, Cashier).
- Daily Sales Summary and Shift Management.

## 4. Key Business Rules (Do Not Violate)
1. **Soft Deletes:** Products and Categories must NEVER be permanently deleted (to preserve old sale receipts). Use `IsActive` or `IsDeleted` flags.
2. **Transactional Integrity:** A Checkout operation (deducting stock + saving sale) must be executed in a single DB Transaction. If one fails, everything rolls back.
3. **Data Binding:** Always use `BindingList<T>` or DevExpress-compatible data sources for UI binding to ensure two-way real-time updates.
