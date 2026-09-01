namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Product category master data.
    /// </summary>
    public class Category : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
