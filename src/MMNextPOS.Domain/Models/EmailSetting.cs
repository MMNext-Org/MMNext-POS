namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Email configuration (SMTP settings for transactional emails).
    /// </summary>
    public class EmailSetting : EntityBase
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableTls { get; set; } = true;
        public bool IsActive { get; set; } = true;
    }
}
