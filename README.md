# MMNextPOS – Modern Windows POS Application

**MMNextPOS** is a modern, maintainable Windows desktop Point‑of‑Sale (POS) system built with .NET 8, WinForms, and DevExpress. It replaces the legacy FusionPOS codebase with a clean layered architecture (Domain, Infrastructure, Application, Presentation) and asynchronous, test‑driven development.

---

## Table of Contents
- [Architecture Overview](#architecture-overview)
- [Getting Started](#getting-started)
- [Database Setup & Migration Guide](#database-setup--migration-guide)
- [Running the Application](#running-the-application)
- [Testing](#testing)
- [Continuous Integration (GitHub Actions)](#continuous-integration-github-actions)
- [Packaging & Installer](#packaging--installer)
- [Contributing](#contributing)

---

## Architecture Overview

```mermaid
flowchart TB
    subgraph Domain
        Product[Product POCO]
        Customer[Customer POCO]
        Sale[Sale POCO]
        SaleDetail[SaleDetail POCO]
        Invoice[Invoice POCO]
    end

    subgraph Infrastructure
        RepoBase[RepositoryBase]
        ProductRepo[ProductRepository]
        SaleRepo[SaleRepository]
        CustomerRepo[CustomerRepository]
        DBInit[DatabaseInitializer]
    end

    subgraph Application
        SalesService[SalesService]
        CustomerService[CustomerService]
        DI[DependencyInjection]
    end

    subgraph Presentation
        MainForm[MainForm (WinForms)]
        Program[Program.cs – DI bootstrap]
    end

    Domain --> Infrastructure
    Infrastructure --> Application
    Application --> Presentation
    Presentation --> Program
```

- **Domain** – Pure POCO entities and domain‑specific exceptions.
- **Infrastructure** – Async MySQL repositories built with **Dapper**, plus a `DatabaseInitializer` that creates tables on first run.
- **Application** – Service layer (`ISalesService`, `ICustomerService`) containing business rules (stock validation, audit logging, etc.).
- **Presentation** – WinForms UI (`MainForm`) that consumes services via **Microsoft.Extensions.DependencyInjection**.

---

## Getting Started

### Prerequisites
- Windows 10/11 (or any Windows that supports .NET 8).
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) installed.
- MySQL 8.0 server (local, Docker, or remote).
- (Optional) DevExpress WinForms components – a free community license is sufficient for development. See the DevExpress documentation for installation.

### Clone the Repository
```bash
git clone https://github.com/<your‑org>/MMNextPOS.git
cd MMNextPOS
```

### Set up the Connection String
The application reads a connection string from the environment variable `MMNEXTPOS_CONNECTION_STRING`. Create it locally (PowerShell example):
```powershell
$env:MMNEXTPOS_CONNECTION_STRING = "Server=localhost;Port=3306;Database=mmnextpos;User ID=root;Password=yourPassword;Pooling=true;"
```
If the variable is not present, the app falls back to a default `root` connection (useful for local Docker testing).

---

## Database Setup & Migration Guide

The first time the application starts, **DatabaseInitializer** runs and creates the required tables if they do not already exist. The schema includes:
- `Products`
- `Customers`
- `Sales`
- `SaleDetails`

### Manual Migration (optional)
If you need to apply the schema manually (e.g., on a production server), run the following SQL script (found in `src/MMNextPOS.Infrastructure/DatabaseInitializer.cs`):
```sql
CREATE TABLE IF NOT EXISTS Products (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Sku VARCHAR(50) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Customers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    Address VARCHAR(200) NULL,
    Phone VARCHAR(20) NULL,
    Email VARCHAR(100) NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Sales (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CustomerId INT NOT NULL,
    SaleDate DATETIME NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_Sales_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS SaleDetails (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SaleId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_SaleDetails_Sale FOREIGN KEY (SaleId) REFERENCES Sales(Id) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_SaleDetails_Product FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;
```
Run the script against your MySQL instance before launching the application if you prefer a manual approach.

---

## Running the Application

```bash
# Build the solution
dotnet build -c Release

# Run the WinForms app
dotnet run --project src/MMNextPOS.WinForms/MMNextPOS.WinForms.csproj```

The UI opens to a simple **Sales Dashboard** showing recent sales. Use the **Refresh** button to reload data. Future extensions will add product, customer, and reporting screens.

---

## Testing

### Unit Tests (Application Layer)
```bash
# From the repository root
dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj```

### Integration Tests (Infrastructure Layer)
The integration tests spin up a MySQL Docker container automatically via **Testcontainers**.
```bash
# Ensure Docker is running
dotnet test tests/MMNextPOS.Infrastructure.Tests/MMNextPOS.Infrastructure.Tests.csproj```

All tests run on the CI pipeline as well (see below).

---

## Continuous Integration (GitHub Actions)

The repository includes a GitHub Actions workflow (`.github/workflows/ci.yml`) that:
1. Restores, builds, and runs unit tests.
2. Starts a MySQL service container and runs integration tests.

![CI status](https://github.com/<owner>/<repo>/actions/workflows/ci.yml/badge.svg?branch=main)

---

## Packaging & Installer

### Self‑Contained Publish
```bash
# Publish a self‑contained Windows executable
dotnet publish src/MMNextPOS.WinForms/MMNextPOS.WinForms.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish```
The output folder (`publish`) contains `MMNextPOS.WinForms.exe` and all required runtime files – you can zip this folder for distribution.

### MSI Installer (WiX Toolset)
If you prefer an MSI installer, you can use the **WiX Toolset**. Below is a minimal WiX fragment (save as `Installer.wxs`):
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" Name="MMNextPOS" Language="1033" Version="1.0.0.0" Manufacturer="YourCompany" UpgradeCode="{11111111-2222-3333-4444-555555555555}">
    <Package InstallerVersion="500" Compressed="yes" InstallScope="perMachine" />
    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFilesFolder">
        <Directory Id="INSTALLFOLDER" Name="MMNextPOS">
          <Component Id="MainExe" Guid="{AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE}">
            <File Source="publish\MMNextPOS.WinForms.exe" KeyPath="yes" />
          </Component>
        </Directory>
      </Directory>
    </Directory>
    <Feature Id="DefaultFeature" Level="1">
      <ComponentRef Id="MainExe" />
    </Feature>
  </Product>
</Wix>
```
Build the MSI with the WiX tools:
```bash
candle Installer.wxs
light -ext WixUIExtension Installer.wixobj -o MMNextPOS.msi```
Distribute `MMNextPOS.msi` to end users – it installs the self‑contained executable in `Program Files\MMNextPOS`.

---

## Contributing

1. Fork the repo and create a feature branch.
2. Follow the **layered architecture** – add new entities in `Domain`, repositories in `Infrastructure`, services in `Application`, and UI in `WinForms`.
3. Write unit tests for any new service logic and integration tests for repository changes.
4. Ensure the CI pipeline passes before opening a Pull Request.

---

## License

MMNextPOS is released under the MIT License. See the `LICENSE` file for details.
