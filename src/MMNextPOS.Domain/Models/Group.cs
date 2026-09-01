namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Product group/family for hierarchical categorization.
    /// </summary>
    public class Group : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentGroupId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
