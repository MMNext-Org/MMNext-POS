using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Backup configuration settings.
    /// </summary>
    public class BackupSetting : EntityBase
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Frequency { get; set; } = "Daily"; // Daily, Weekly, Monthly, Manual

        public TimeSpan ExecutionTime { get; set; } = new TimeSpan(2, 0, 0); // 2:00 AM

        public int RetentionDays { get; set; } = 30;

        [MaxLength(500)]
        public string BackupPath { get; set; } = string.Empty; // Local or network path

        [MaxLength(20)]
        public string StorageType { get; set; } = "Local"; // Local, FTP, SFTP, S3, AzureBlob, GoogleDrive

        [MaxLength(500)]
        public string RemoteConnectionString { get; set; } = string.Empty; // For remote storage

        public bool IncludeDatabase { get; set; } = true;
        public bool IncludeFiles { get; set; } = true;
        public bool IncludeLogs { get; set; } = false;
        public bool IncludeImages { get; set; } = true;

        public bool CompressBackup { get; set; } = true;
        public bool EncryptBackup { get; set; } = false;
        [MaxLength(100)]
        public string EncryptionPassword { get; set; } = string.Empty;

        public bool SendNotificationOnSuccess { get; set; } = true;
        public bool SendNotificationOnFailure { get; set; } = true;
        [MaxLength(500)]
        public string NotificationEmails { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime? LastRunAt { get; set; }
        public DateTime? NextRunAt { get; set; }
        public string LastStatus { get; set; } = string.Empty; // Success, Failed, Running
        [MaxLength(1000)]
        public string LastErrorMessage { get; set; } = string.Empty;

        public int MaxParallelJobs { get; set; } = 1;
        public int TimeoutMinutes { get; set; } = 60;
    }
}