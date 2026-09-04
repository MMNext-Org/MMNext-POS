using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Represents a subscription/license for the POS system.
    /// </summary>
    public class Subscription : EntityBase
    {
        [Required]
        [MaxLength(100)]
        public string SubscriptionKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime ExpiryDate { get; set; }

        public int MaxUsers { get; set; } = 1;

        public int MaxDevices { get; set; } = 1;

        public int MaxLocations { get; set; } = 1;

        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // Active, Expired, Suspended, Cancelled, Trial

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActivated { get; set; } = false;

        public DateTime? ActivatedDate { get; set; }

        [MaxLength(200)]
        public string? ActivatedDeviceId { get; set; }

        public int CurrentUserCount { get; set; } = 0;

        public int CurrentDeviceCount { get; set; } = 0;

        public int CurrentLocationCount { get; set; } = 0;

        [MaxLength(50)]
        public string SubscriptionType { get; set; } = "Standard"; // Trial, Standard, Professional, Enterprise

        public decimal MonthlyPrice { get; set; } = 0;

        public decimal YearlyPrice { get; set; } = 0;

        public int BillingCycleDays { get; set; } = 30;

        public DateTime? LastBillingDate { get; set; }

        public DateTime? NextBillingDate { get; set; }

        [MaxLength(100)]
        public string? PaymentMethod { get; set; }

        [MaxLength(100)]
        public string? PaymentReference { get; set; }

        public bool AutoRenew { get; set; } = true;

        public int GracePeriodDays { get; set; } = 7;

        public bool IsTrial { get; set; } = false;

        public int TrialDays { get; set; } = 30;
    }
}
