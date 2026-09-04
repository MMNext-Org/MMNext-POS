using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Super Admin configuration and audit log.
    /// </summary>
    public class SuperAdminLog : EntityBase
    {
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty; // UserManagement, SettingsChange, Backup, Migration, License, Security

        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        [MaxLength(100)]
        public string PerformedBy { get; set; } = string.Empty;

        public int PerformedByUserId { get; set; }

        [MaxLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        [MaxLength(500)]
        public string UserAgent { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string BeforeState { get; set; } = string.Empty; // JSON

        [MaxLength(5000)]
        public string AfterState { get; set; } = string.Empty; // JSON

        [MaxLength(100)]
        public string Module { get; set; } = string.Empty; // Settings, License, Backup, Migration, Users, Security

        [MaxLength(20)]
        public string Severity { get; set; } = "Info"; // Info, Warning, Critical, Security

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public bool IsSensitive { get; set; } = false;
    }
}