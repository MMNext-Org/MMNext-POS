-- Migration: 017_JournalVouchers
-- Description: Add JournalVouchers and JournalVoucherDetails tables for double-entry accounting.
-- Journal voucher header stores voucher number, date, status (Draft/Posted/Voided) and totals.
-- Details store per-account debit/credit amounts.

CREATE TABLE IF NOT EXISTS JournalVouchers (
    Id INT NOT NULL AUTO_INCREMENT,
    VoucherNo VARCHAR(50) NOT NULL,
    VoucherDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Description TEXT,
    ReferenceNo VARCHAR(100),
    Status VARCHAR(20) NOT NULL DEFAULT 'Draft',
    TotalDebit DECIMAL(18,4) NOT NULL DEFAULT 0,
    TotalCredit DECIMAL(18,4) NOT NULL DEFAULT 0,
    CreatedBy INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedBy INT NULL,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (Id),
    UNIQUE KEY UQ_JournalVouchers_VoucherNo (VoucherNo)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS JournalVoucherDetails (
    Id INT NOT NULL AUTO_INCREMENT,
    JournalVoucherId INT NOT NULL,
    AccountCode VARCHAR(50) NOT NULL,
    AccountName VARCHAR(100) NOT NULL,
    DebitAmount DECIMAL(18,4) NOT NULL DEFAULT 0,
    CreditAmount DECIMAL(18,4) NOT NULL DEFAULT 0,
    Description TEXT,
    LineNo INT NOT NULL DEFAULT 0,
    CreatedBy INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedBy INT NULL,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (Id),
    KEY IX_JournalVoucherDetails_JournalVoucherId (JournalVoucherId)
) ENGINE=InnoDB;
