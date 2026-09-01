using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Base class for all domain entities. Provides identity and audit fields.
    /// </summary>
    public abstract class EntityBase
    {
        [Key]
        public int Id { get; set; }

        /// <summary>UTC timestamp of creation.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp of last update.</summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>User id of the creator.</summary>
        public int? CreatedBy { get; set; }

        /// <summary>User id of the last updater.</summary>
        public int? UpdatedBy { get; set; }
    }
}
