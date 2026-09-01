namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Role-to-Menu mapping for role-based access control.
    /// </summary>
    public class MenuRole : EntityBase
    {
        public int RoleId { get; set; }
        public string MenuCode { get; set; } = string.Empty; // e.g., "SALES", "INVENTORY", "REPORTS"
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExport { get; set; }
    }
}
