using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Starman - Reorder level report DTO.
    /// </summary>
    public class StarReorderReport : EntityBase
    {
        public int LocationId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int DaysOfStock { get; set; }
    }
}
