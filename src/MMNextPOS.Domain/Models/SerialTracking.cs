using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Value Object representing the tracking information for a serial number through inventory movements.
    /// Immutable and compared by value.
    /// </summary>
    public readonly struct SerialTracking : IEquatable<SerialTracking>
    {
        /// <summary>
        /// Creates a new SerialTracking value object.
        /// </summary>
        /// <param name="serialNumber">The serial number being tracked.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="locationId">The current location ID.</param>
        /// <param name="status">The current status of the serial number.</param>
        /// <param name="movementType">The last movement type.</param>
        /// <param name="movementDate">The date of the last movement.</param>
        /// <param name="referenceId">Optional reference ID (sale, purchase, transfer, etc.).</param>
        /// <param name="referenceType">Optional reference type.</param>
        public SerialTracking(
            SerialNumber serialNumber,
            int productId,
            int locationId,
            string status,
            string movementType,
            DateTime movementDate,
            int? referenceId = null,
            string? referenceType = null)
        {
            SerialNumber = serialNumber;
            ProductId = productId;
            LocationId = locationId;
            Status = status ?? "Available";
            MovementType = movementType ?? "Initial";
            MovementDate = movementDate;
            ReferenceId = referenceId;
            ReferenceType = referenceType;
        }

        /// <summary>
        /// The serial number being tracked.
        /// </summary>
        public SerialNumber SerialNumber { get; }

        /// <summary>
        /// The product ID.
        /// </summary>
        public int ProductId { get; }

        /// <summary>
        /// The current location ID.
        /// </summary>
        public int LocationId { get; }

        /// <summary>
        /// The current status (Available, Sold, Reserved, Damaged, Lost, Transferred, Returned).
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// The last movement type (Purchase, Sale, Transfer, Adjustment, Return, Assembly, Disassembly).
        /// </summary>
        public string MovementType { get; }

        /// <summary>
        /// The date of the last movement.
        /// </summary>
        public DateTime MovementDate { get; }

        /// <summary>
        /// Optional reference ID (sale ID, purchase ID, transfer ID, etc.).
        /// </summary>
        public int? ReferenceId { get; }

        /// <summary>
        /// Optional reference type (Sale, Purchase, Transfer, Adjustment, Return).
        /// </summary>
        public string? ReferenceType { get; }

        /// <summary>
        /// Creates a new SerialTracking with updated status and movement.
        /// </summary>
        public SerialTracking WithMovement(
            string newStatus,
            string newMovementType,
            DateTime movementDate,
            int? newLocationId = null,
            int? referenceId = null,
            string? referenceType = null)
        {
            return new SerialTracking(
                SerialNumber,
                ProductId,
                newLocationId ?? LocationId,
                newStatus,
                newMovementType,
                movementDate,
                referenceId,
                referenceType);
        }

        /// <summary>
        /// Creates a new SerialTracking for a sale movement.
        /// </summary>
        public SerialTracking WithSale(int saleId, DateTime saleDate, int? newLocationId = null)
        {
            return WithMovement("Sold", "Sale", saleDate, newLocationId, saleId, "Sale");
        }

        /// <summary>
        /// Creates a new SerialTracking for a purchase/receipt movement.
        /// </summary>
        public SerialTracking WithPurchase(int purchaseId, DateTime purchaseDate, int locationId)
        {
            return WithMovement("Available", "Purchase", purchaseDate, locationId, purchaseId, "Purchase");
        }

        /// <summary>
        /// Creates a new SerialTracking for a transfer movement.
        /// </summary>
        public SerialTracking WithTransfer(int transferId, DateTime transferDate, int toLocationId)
        {
            return WithMovement("Transferred", "Transfer", transferDate, toLocationId, transferId, "Transfer");
        }

        /// <summary>
        /// Creates a new SerialTracking for a return movement.
        /// </summary>
        public SerialTracking WithReturn(int returnId, DateTime returnDate, int locationId)
        {
            return WithMovement("Available", "Return", returnDate, locationId, returnId, "Return");
        }

        /// <summary>
        /// Creates a new SerialTracking for an adjustment movement.
        /// </summary>
        public SerialTracking WithAdjustment(int adjustmentId, DateTime adjustmentDate, string newStatus, int? locationId = null)
        {
            return WithMovement(newStatus, "Adjustment", adjustmentDate, locationId, adjustmentId, "Adjustment");
        }

        /// <summary>
        /// Creates a new SerialTracking for a damaged/lost status.
        /// </summary>
        public SerialTracking WithDamageOrLoss(string status, int adjustmentId, DateTime date)
        {
            if (status != "Damaged" && status != "Lost")
            {
                throw new ArgumentException("Status must be 'Damaged' or 'Lost'.", nameof(status));
            }
            return WithMovement(status, "Adjustment", date, null, adjustmentId, "Adjustment");
        }

        /// <summary>
        /// Checks if the serial number is currently available for sale.
        /// </summary>
        public bool IsAvailable => Status.Equals("Available", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if the serial number is sold.
        /// </summary>
        public bool IsSold => Status.Equals("Sold", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if the serial number is in a transfer.
        /// </summary>
        public bool IsInTransfer => Status.Equals("Transferred", StringComparison.OrdinalIgnoreCase) ||
                                     Status.Equals("InTransit", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if the serial number is damaged or lost.
        /// </summary>
        public bool IsDamagedOrLost => Status.Equals("Damaged", StringComparison.OrdinalIgnoreCase) ||
                                        Status.Equals("Lost", StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc />
        public bool Equals(SerialTracking other) =>
            SerialNumber.Equals(other.SerialNumber) &&
            ProductId == other.ProductId &&
            LocationId == other.LocationId &&
            string.Equals(Status, other.Status, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(MovementType, other.MovementType, StringComparison.OrdinalIgnoreCase) &&
            MovementDate == other.MovementDate &&
            ReferenceId == other.ReferenceId &&
            string.Equals(ReferenceType, other.ReferenceType, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is SerialTracking other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(SerialNumber);
            hash.Add(ProductId);
            hash.Add(LocationId);
            hash.Add(Status, StringComparer.OrdinalIgnoreCase);
            hash.Add(MovementType, StringComparer.OrdinalIgnoreCase);
            hash.Add(MovementDate);
            hash.Add(ReferenceId);
            hash.Add(ReferenceType, StringComparer.OrdinalIgnoreCase);
            return hash.ToHashCode();
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"{SerialNumber} - {Status} ({MovementType} at {MovementDate:yyyy-MM-dd HH:mm})";

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(SerialTracking left, SerialTracking right) => left.Equals(right);

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(SerialTracking left, SerialTracking right) => !left.Equals(right);
    }
}
