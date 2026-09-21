using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Audit trail for serial number movements. Tracks every state change
    /// (receive, sale, return, transfer, expire, damage) for full traceability.
    /// </summary>
    public class SerialTracking : EntityBase
    {
        /// <summary>
        /// Serial number being tracked.
        /// </summary>
        public int SerialNumberId { get; set; }

        /// <summary>
        /// Type of movement that occurred.
        /// </summary>
        public SerialMovementType MovementType { get; set; }

        /// <summary>
        /// Location before this movement (null for initial receive).
        /// </summary>
        public int? FromLocationId { get; set; }

        /// <summary>
        /// Location after this movement (null if stock removed).
        /// </summary>
        public int? ToLocationId { get; set; }

        /// <summary>
        /// User who performed this movement.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Timestamp of this movement.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Reference to source document (SaleId, PurchaseId, TransferId, etc.).
        /// </summary>
        public int? ReferenceId { get; set; }

        /// <summary>
        /// Type of reference document.
        /// </summary>
        public string? ReferenceType { get; set; }

        /// <summary>
        /// Additional notes about this movement.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public SerialNumber? SerialNumber { get; set; }
    }

    /// <summary>
    /// Types of serial number movements.
    /// </summary>
    public enum SerialMovementType
    {
        /// <summary>Serial received into stock</summary>
        Received = 0,

        /// <summary>Serial sold to customer</summary>
        Sold = 1,

        /// <summary>Serial returned by customer</summary>
        Returned = 2,

        /// <summary>Serial transferred between locations</summary>
        Transferred = 3,

        /// <summary>Serial expired</summary>
        Expired = 4,

        /// <summary>Serial damaged/write-off</summary>
        Damaged = 5,

        /// <summary>Serial adjusted (correction)</summary>
        Adjusted = 6,

        /// <summary>Serial reserved for pending sale</summary>
        Reserved = 7,

        /// <summary>Serial reservation released</summary>
        ReservationReleased = 8
    }
}
