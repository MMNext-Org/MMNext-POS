namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Expense category/type (rent, utilities, salary, etc.).
    ///</summary>
    public class ExpenseType : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}
