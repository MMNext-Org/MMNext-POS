-- Baseline migration: Create SchemaVersions table for tracking applied migrations
-- This must be the first migration run on any database
CREATE TABLE IF NOT EXISTS SchemaVersions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Version VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(500) NOT NULL,
    AppliedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    AppliedBy VARCHAR(100) NOT NULL,
    Checksum VARCHAR(64) NULL,
    Success BOOLEAN DEFAULT 1,
    ErrorMessage VARCHAR(2000) NULL,
    INDEX IX_SchemaVersions_Version (Version),
    INDEX IX_SchemaVersions_AppliedAt (AppliedAt)
) ENGINE=InnoDB;