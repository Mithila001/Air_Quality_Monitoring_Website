using System;
using System.ComponentModel.DataAnnotations;

namespace SDTP_Project1.Models
{
    public class AlertThresholdSetting
    {
        [Key]
        public int ThresholdId { get; set; }

        [Required, StringLength(50)]
        public string Parameter { get; set; } = string.Empty;   // e.g., AQI, PM2_5

        [Required]
        public float ThresholdValue { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

}

