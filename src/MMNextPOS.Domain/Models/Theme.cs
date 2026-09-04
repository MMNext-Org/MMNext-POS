using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Theme configuration for UI customization.
    /// </summary>
    public class Theme : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = "#0078D4";
        public string SecondaryColor { get; set; } = "#E0E0E0";
        public string AccentColor { get; set; } = "#FF6B35";
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public string TextColor { get; set; } = "#333333";
        public string FontFamily { get; set; } = "Segoe UI";
        public string FontSize { get; set; } = "12pt";
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
