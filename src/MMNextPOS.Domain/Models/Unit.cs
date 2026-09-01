namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Unit of measure (e.g., pieces, boxes, dozens, kg, liters).
    /// </summary>
    public class Unit : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Symbol { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
