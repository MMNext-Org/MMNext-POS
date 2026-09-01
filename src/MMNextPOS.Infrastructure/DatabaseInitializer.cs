using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure
{
    /// <summary>
    /// Ensures that the required database tables exist. Called at application startup.
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly IUnitOfWork _unitOfWork;

        public DatabaseInitializer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
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
";
            await _unitOfWork.Connection.ExecuteAsync(sql, commandTimeout: 30).ConfigureAwait(false);
        }
    }
}
