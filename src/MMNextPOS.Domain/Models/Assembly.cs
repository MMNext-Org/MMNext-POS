namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Assembly header (BOM - bill of materials, combining multiple products into one).
    ///</summary>
    public class Assembly : EntityBase
    {
        public string AssemblyNo { get; set; } = string.Empty;
        public int OutputProductId { get; set; }
        public int OutputQuantity { get; set; }
        public DateTime AssemblyDate { get; set; }
        public decimal TotalCost { get; set; }
        public int? LocationId { get; set; }
        public string Status { get; set; } = "Active";
        public int? CreatedByUserId { get; set; }
        public string? Notes { get; set; }
    }
}
