using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Application information for license/version management.
    /// </summary>
    public class AppInfo : EntityBase
    {
        public string AppName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string BuildNumber { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public bool IsMandatory { get; set; } = false;
        public DateTime ReleaseDate { get; set; }
        public string Platform { get; set; } = string.Empty; // Windows, Android, iOS
        public string MinimumOsVersion { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
