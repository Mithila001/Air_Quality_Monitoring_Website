using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDTP_Project1.Models
{
    public class AirQualityData
    {
        [Key]
        public int MeasurementID { get; set; }

        [Required]
        public string SensorID { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }
        public double? PM2_5 { get; set; }
        public double? PM10 { get; set; }
        public double? O3 { get; set; }
        public double? NO2 { get; set; }
        public double? SO2 { get; set; }
        public double? CO { get; set; }
        public int? AQI { get; set; }

        // Navigation property: sensor details are in a separate table.
        [ForeignKey("SensorID")]
        public Sensor Sensor { get; set; }
    }
}
