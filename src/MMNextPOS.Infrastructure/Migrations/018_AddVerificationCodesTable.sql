-- Migration: 018_AddVerificationCodesTable.sql
-- Description: Add VerificationCodes table for storing password reset verification codes
-- Creates table to store 6-character verification codes with 10-minute expiration for password reset flows
-- Used by UserService.ResetPasswordAsync flow with in-memory dictionary replaced by persistent storage

CREATE TABLE IF NOT EXISTS VerificationCodes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Code NVARCHAR(6) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ExpiresAt DATETIME NOT NULL,
    CONSTRAINT FK_VerificationCodes_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
) ENGINE=InnoDB;