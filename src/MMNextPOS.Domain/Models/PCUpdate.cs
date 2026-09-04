using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// PC Update information for auto-update mechanism.
    /// </summary>
    public class PCUpdate : EntityBase
    {
        public string Version { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string Checksum { get; set; } = string.Empty; // SHA256
        public long FileSize { get; set; }
        public string ReleaseNotes { get; set; } = string.Empty;
        public bool IsMandatory { get; set; } = false;
        public DateTime ReleaseDate { get; set; }
        public string MinimumVersion { get; set; } = string.Empty; // Minimum version required to apply this update
        public bool IsActive { get; set; } = true;
    }
}
