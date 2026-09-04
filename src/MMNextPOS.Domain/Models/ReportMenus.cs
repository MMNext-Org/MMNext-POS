using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Menu/Report configuration for role-based navigation.
    /// </summary>
    public class ReportMenus : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ParentCode { get; set; } = string.Empty;
        public string FormName { get; set; } = string.Empty;
        public string AssemblyName { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsReport { get; set; } = false;
        public string ReportFileName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
