namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Enumeration of all stock movement types for type-safe movement categorization.
    /// </summary>
    public enum StockMovementType
    {
        /// <summary>Initial opening stock entry</summary>
        Opening = 0,

        /// <summary>Stock issued/outgoing (e.g., sale, issue to production)</summary>
        Issue = 1,

        /// <summary>Stock received/incoming (e.g., purchase receipt, return receipt)</summary>
        Receive = 2,

        /// <summary>Stock damaged and written off</summary>
        Damaged = 3,

        /// <summary>Stock lost/missing</summary>
        Lost = 4,

        /// <summary>Manual stock adjustment (positive or negative)</summary>
        Adjust = 5,

        /// <summary>Stock transfer out (source location)</summary>
        TransferOut = 6,

        /// <summary>Stock transfer in (destination location)</summary>
        TransferIn = 7,

        /// <summary>Assembly build - component stock decremented</summary>
        Assembly = 8,

        /// <summary>Deassembly - component stock incremented</summary>
        Deassembly = 9,

        /// <summary>Stock expired and written off</summary>
        Expired = 10,

        /// <summary>Sale movement (outgoing)</summary>
        Sale = 11,

        /// <summary>Purchase receipt (incoming)</summary>
        Purchase = 12,

        /// <summary>Return receipt (incoming)</summary>
        Return = 13,

        /// <summary>Void movement (reversal)</summary>
        Void = 14,

        /// <summary>Cycle count adjustment</summary>
        CycleCount = 15
    }

    /// <summary>
    /// Extension methods for StockMovementType
    /// </summary>
    public static class StockMovementTypeExtensions
    {
        /// <summary>
        /// Determines if the movement type increases stock quantity.
        /// </summary>
        public static bool IsStockIncrease(this StockMovementType type)
        {
            return type switch
            {
                StockMovementType.Opening => true,
                StockMovementType.Receive => true,
                StockMovementType.TransferIn => true,
                StockMovementType.Return => true,
                StockMovementType.Adjust => true, // Can be positive
                _ => false
            };
        }

        /// <summary>
        /// Determines if the movement type decreases stock quantity.
        /// </summary>
        public static bool IsStockDecrease(this StockMovementType type)
        {
            return type switch
            {
                StockMovementType.Issue => true,
                StockMovementType.Damaged => true,
                StockMovementType.Lost => true,
                StockMovementType.TransferOut => true,
                StockMovementType.Assembly => true,
                StockMovementType.Expired => true,
                StockMovementType.Sale => true,
                StockMovementType.Void => true, // Reversal of sale = increase, but movement type is void
                _ => false
            };
        }

        /// <summary>
        /// Gets the default sign for stock adjustment (+1 for increase, -1 for decrease).
        /// For Adjust type, returns 0 (determined by quantity sign).
        /// </summary>
        public static int GetDefaultStockSign(this StockMovementType type)
        {
            if (type == StockMovementType.Adjust) return 0; // Determined by quantity
            return type.IsStockIncrease() ? 1 : -1;
        }

        /// <summary>
        /// Converts to string for database storage.
        /// </summary>
        public static string ToDbString(this StockMovementType type)
        {
            return type.ToString();
        }

        /// <summary>
        /// Parses from database string.
        /// </summary>
        public static StockMovementType FromDbString(string value)
        {
            if (Enum.TryParse<StockMovementType>(value, true, out var result))
                return result;

            // Legacy string mappings
            return value?.ToLowerInvariant() switch
            {
                "opening" => StockMovementType.Opening,
                "issue" => StockMovementType.Issue,
                "receive" => StockMovementType.Receive,
                "damaged" => StockMovementType.Damaged,
                "lost" => StockMovementType.Lost,
                "adjust" => StockMovementType.Adjust,
                "transferout" => StockMovementType.TransferOut,
                "transferin" => StockMovementType.TransferIn,
                "assembly" => StockMovementType.Assembly,
                "deassembly" => StockMovementType.Deassembly,
                "expired" => StockMovementType.Expired,
                "sale" => StockMovementType.Sale,
                "purchase" => StockMovementType.Purchase,
                "return" => StockMovementType.Return,
                "void" => StockMovementType.Void,
                "cyclecount" => StockMovementType.CycleCount,
                _ => StockMovementType.Adjust
            };
        }
    }
}
