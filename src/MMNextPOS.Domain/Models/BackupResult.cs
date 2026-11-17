using System;

namespace MMNextPOS.Domain.Models
{
    public class BackupResult
    {
        public int BackupSettingId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = string.Empty; // Success, Failed, Running
        public long BackupSizeBytes { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
