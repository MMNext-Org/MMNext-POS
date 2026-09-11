-- Migration: 010_ReturnSequences
-- Description: Add ReturnSequences table for atomic per-year return numbering. Used by IReturnNumberGenerator
-- to issue monotonic, gap-tolerant return numbers like RET-2026-000123 under concurrent
-- returns workloads. The (Year, Prefix) row is locked by MySQL during
-- INSERT ... ON DUPLICATE KEY UPDATE LastValue = LastValue + 1, so two concurrent
-- transactions cannot issue the same number.

CREATE TABLE IF NOT EXISTS ReturnSequences (
    Year INT NOT NULL,
    Prefix VARCHAR(20) NOT NULL,
    LastValue BIGINT NOT NULL DEFAULT 0,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (Year, Prefix)
) ENGINE=InnoDB;