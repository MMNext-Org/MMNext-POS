namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Component consumed by an assembly.
    ///</summary>
    public class AssemblyDetail : EntityBase
    {
        public int AssemblyId { get; set; }
        public int ComponentProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }
    }
}
