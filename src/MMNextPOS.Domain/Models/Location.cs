namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Physical location (warehouse, store branch, distribution center).
    /// </summary>
    public class Location : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsHeadquarter { get; set; }
        public int DisplayOrder { get; set; }
    }
}
