namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// User-to-Role mapping (many-to-many junction table).
    /// </summary>
    public class UserRole : EntityBase
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
}
