namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Value objects for domain errors.
    /// </summary>
    public class InsufficientStockException : Exception
    {
        public InsufficientStockException(string message) : base(message) { }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}
