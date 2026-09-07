-- Migration: 008_InvoiceSequences
-- Description: Per-year atomic sequence for invoice numbering. Used by IInvoiceNumberGenerator
-- to issue monotonic, gap-tolerant invoice numbers like INV-2026-000123 under concurrent
-- sales workloads. The (Year, LastValue) row is locked by MySQL during
-- INSERT ... ON DUPLICATE KEY UPDATE LastValue = LastValue + 1, so two concurrent
-- transactions cannot issue the same number.

CREATE TABLE IF NOT EXISTS InvoiceSequences (
    Year INT NOT NULL,
    Prefix VARCHAR(20) NOT NULL,
    LastValue BIGINT NOT NULL DEFAULT 0,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (Year, Prefix)
) ENGINE=InnoDB;
