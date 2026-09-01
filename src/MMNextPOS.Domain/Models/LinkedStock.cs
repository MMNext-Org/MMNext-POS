namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Linked products (e.g., accessories, add-ons, related items).
    ///</summary>
    public class LinkedStock : EntityBase
    {
        public int PrimaryProductId { get; set; }
        public int LinkedProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public string? LinkType { get; set; } // Accessory, Bundle, Related
        public bool IsActive { get; set; } = true;
    }
}
