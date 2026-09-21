-- Migration: 015_SerialTables
-- Description: Add SerialNumber, SerialBatch, and SerialTracking tables for serial/batch tracking
-- addresses Phase 4 requirement for serial number lifecycle management

-- SerialBatches must exist before SerialNumbers (FK relationship)
CREATE TABLE IF NOT EXISTS SerialBatches (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    BatchNumber VARCHAR(50) NOT NULL,
    ProductId INT NOT NULL,
    ManufactureDate DATETIME NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    Quantity INT NOT NULL,
    RemainingQuantity INT NOT NULL DEFAULT 0,
    SupplierId INT NULL,
    CostPerUnit DECIMAL(18,2) NOT NULL DEFAULT 0,
    LocationId INT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    INDEX IX_SerialBatches_BatchNumber (BatchNumber),
    INDEX IX_SerialBatches_ProductId (ProductId),
    INDEX IX_SerialBatches_ExpiryDate (ExpiryDate),
    INDEX IX_SerialBatches_IsActive (IsActive),
    CONSTRAINT FK_SerialBatches_ProductId FOREIGN KEY (ProductId) REFERENCES Products(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- SerialNumbers for individual item tracking
CREATE TABLE IF NOT EXISTS SerialNumbers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SerialNumberValue VARCHAR(100) NOT NULL,
    ProductId INT NOT NULL,
    BatchId INT NULL,
    LocationId INT NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Available',
    ReceivedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Cost DECIMAL(18,2) NOT NULL DEFAULT 0,
    WarrantyExpiryDate DATETIME NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    UNIQUE KEY IX_SerialNumbers_SerialNumberValue (SerialNumberValue),
    INDEX IX_SerialNumbers_ProductId (ProductId),
    INDEX IX_SerialNumbers_BatchId (BatchId),
    INDEX IX_SerialNumbers_LocationId (LocationId),
    INDEX IX_SerialNumbers_Status (Status),
    INDEX IX_SerialNumbers_IsActive (IsActive),
    CONSTRAINT FK_SerialNumbers_ProductId FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT FK_SerialNumbers_BatchId FOREIGN KEY (BatchId) REFERENCES SerialBatches(Id),
    CONSTRAINT FK_SerialNumbers_LocationId FOREIGN KEY (LocationId) REFERENCES Locations(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- SerialTracking for audit trail of serial movements
CREATE TABLE IF NOT EXISTS SerialTrackings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SerialNumberId INT NOT NULL,
    MovementType VARCHAR(30) NOT NULL,
    FromLocationId INT NULL,
    ToLocationId INT NULL,
    UserId INT NOT NULL,
    Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ReferenceId INT NULL,
    ReferenceType VARCHAR(50) NULL,
    Notes VARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    INDEX IX_SerialTrackings_SerialNumberId (SerialNumberId),
    INDEX IX_SerialTrackings_MovementType (MovementType),
    INDEX IX_SerialTrackings_Timestamp (Timestamp),
    INDEX IX_SerialTrackings_ReferenceId (ReferenceId),
    CONSTRAINT FK_SerialTrackings_SerialNumberId FOREIGN KEY (SerialNumberId) REFERENCES SerialNumbers(Id),
    CONSTRAINT FK_SerialTrackings_FromLocationId FOREIGN KEY (FromLocationId) REFERENCES Locations(Id),
    CONSTRAINT FK_SerialTrackings_ToLocationId FOREIGN KEY (ToLocationId) REFERENCES Locations(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;