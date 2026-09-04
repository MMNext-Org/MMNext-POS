namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Audit log entry for tracking changes to entities.
    /// </summary>
    public class ChangeDateLog : EntityBase
    {
        public string EntityName { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Action { get; set; } = string.Empty; // Create, Update, Delete
        public string? OldValues { get; set; } // JSON serialized old values
        public string? NewValues { get; set; } // JSON serialized new values
        public int? ChangedByUserId { get; set; }
        public string? ChangedByUserName { get; set; }
        public string? Description { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }
}
