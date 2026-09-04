using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Language configuration for multi-language support.
    /// </summary>
    public class Language : EntityBase
    {
        public string Code { get; set; } = string.Empty; // ISO 639-1 code (e.g., en, my, zh)
        public string Name { get; set; } = string.Empty; // English name
        public string NativeName { get; set; } = string.Empty; // Name in native language
        public string CultureCode { get; set; } = string.Empty; // Culture info (e.g., en-US, my-MM)
        public string FlagIcon { get; set; } = string.Empty; // Flag emoji or icon path
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
        public bool IsRTL { get; set; } = false; // Right-to-left language
    }
}
