using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// General system settings for the POS application.
    /// </summary>
    public class SystemSetting : EntityBase
    {
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // Printer, Theme, Language, Font, Backup, Migration, etc.

        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(20)]
        public string DataType { get; set; } = "String"; // String, Int, Bool, Decimal, DateTime, Json

        public bool IsSystem { get; set; } = false; // System settings that shouldn't be deleted
        public bool IsReadOnly { get; set; } = false; // Read-only settings
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string ValidationRules { get; set; } = string.Empty; // JSON validation rules

        [MaxLength(1000)]
        public string DefaultValue { get; set; } = string.Empty;
    }
}
