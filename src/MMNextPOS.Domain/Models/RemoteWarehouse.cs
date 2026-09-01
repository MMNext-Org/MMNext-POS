namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Remote warehouse (Starman partner site for stock visibility).
    ///</summary>
    public class RemoteWarehouse : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string? ApiKey { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastSyncAt { get; set; }
        public string? ContactInfo { get; set; }
    }
}
