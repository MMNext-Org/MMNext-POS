using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Data migration configuration and history.
    /// </summary>
    public class DataMigration : EntityBase
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string SourceType { get; set; } = string.Empty; // MySQL, SQLServer, PostgreSQL, CSV, Excel, JSON, XML

        [MaxLength(500)]
        public string SourceConnectionString { get; set; } = string.Empty;

        [MaxLength(50)]
        public string TargetType { get; set; } = "MySQL";

        [MaxLength(500)]
        public string TargetConnectionString { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string TableMappings { get; set; } = string.Empty; // JSON

        [MaxLength(1000)]
        public string ColumnMappings { get; set; } = string.Empty; // JSON

        [MaxLength(1000)]
        public string TransformRules { get; set; } = string.Empty; // JSON

        [MaxLength(50)]
        public string ScheduleType { get; set; } = "Manual"; // Manual, Once, Daily, Weekly, Monthly

        public DateTime? ScheduledAt { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Running, Completed, Failed, Cancelled

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int TotalRecords { get; set; } = 0;
        public int ProcessedRecords { get; set; } = 0;
        public int FailedRecords { get; set; } = 0;

        [MaxLength(2000)]
        public string ErrorMessage { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string LogOutput { get; set; } = string.Empty;

        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;

        public bool IsScheduled { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime? LastRunAt { get; set; }
        public DateTime? NextRunAt { get; set; }
    }
}