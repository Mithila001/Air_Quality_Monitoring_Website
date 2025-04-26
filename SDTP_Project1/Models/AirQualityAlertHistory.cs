using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDTP_Project1.Models
{
    public class AirQualityAlertHistory
    {
        [Key]
        public int AlertHistoryId { get; set; }

        [Required]
        public string SensorID { get; set; }

        [Required]
        public int MeasurementID { get; set; }

        [Required]
        public string Parameter { get; set; }

        [Required]
        public double CurrentValue { get; set; }

        [Required]
        public double ThresholdValue { get; set; }

        [Required]
        public DateTime AlertedTime { get; set; }

        // Navigation properties (optional)
        [ForeignKey(nameof(SensorID))]
        public Sensor Sensor { get; set; }

        [ForeignKey(nameof(MeasurementID))]
        public AirQualityData Measurement { get; set; }
    }
}
