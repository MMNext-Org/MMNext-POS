namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Access control role (e.g., Admin, Manager, Cashier, Warehouse).
    /// </summary>
    public class Role : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
