using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Represents a serial number for a specific product instance.
    /// Used for high-value items (electronics, jewelry, warranty-tracked items).
    /// </summary>
    public class SerialNumber : EntityBase
    {
        /// <summary>
        /// The unique serial number string (e.g., "SN-2024-001234").
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SerialNumberValue { get; set; } = string.Empty;

        /// <summary>
        /// Product this serial belongs to.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Batch this serial belongs to (if batch-tracked).
        /// </summary>
        public int? BatchId { get; set; }

        /// <summary>
        /// Current location of this serial.
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// Current status of this serial.
        /// </summary>
        public SerialStatus Status { get; set; } = SerialStatus.Available;

        /// <summary>
        /// Date this serial was received into inventory.
        /// </summary>
        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Cost of this specific unit.
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Warranty expiry date (if applicable).
        /// </summary>
        public DateTime? WarrantyExpiryDate { get; set; }

        /// <summary>
        /// Whether this serial is active (not soft-deleted).
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Enumeration of serial number statuses.
    /// </summary>
    public enum SerialStatus
    {
        /// <summary>Available for sale/transfer</summary>
        Available = 0,

        /// <summary>Sold to a customer</summary>
        Sold = 1,

        /// <summary>Expired (perishable items)</summary>
        Expired = 2,

        /// <summary>Damaged (written off)</summary>
        Damaged = 3,

        /// <summary>In-transit to another location</summary>
        InTransit = 4,

        /// <summary>Returned by customer</summary>
        Returned = 5,

        /// <summary>Under repair/refurbishment</summary>
        UnderRepair = 6,

        /// <summary>Reserved for a pending sale</summary>
        Reserved = 7
    }

    /// <summary>
    /// Extension methods for SerialStatus.
    /// </summary>
    public static class SerialStatusExtensions
    {
        public static bool IsAvailableForSale(this SerialStatus status)
        {
            return status == SerialStatus.Available || status == SerialStatus.Returned;
        }

        public static string ToLabel(this SerialStatus status)
        {
            return status switch
            {
                SerialStatus.Available => "Available",
                SerialStatus.Sold => "Sold",
                SerialStatus.Expired => "Expired",
                SerialStatus.Damaged => "Damaged",
                SerialStatus.InTransit => "In Transit",
                SerialStatus.Returned => "Returned",
                SerialStatus.UnderRepair => "Under Repair",
                SerialStatus.Reserved => "Reserved",
                _ => "Unknown"
            };
        }
    }
}
