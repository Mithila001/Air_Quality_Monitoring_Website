using System;
using System.ComponentModel.DataAnnotations;

namespace SDTP_Project1.Models
{
    public class AirQualityData
    {
        [Key]
        public int MeasurementID { get; set; }
        public string District { get; set; }
        public DateTime Timestamp { get; set; }
        public double? PM2_5 { get; set; }
        public double? PM10 { get; set; }
        public double? O3 { get; set; }
        public double? NO2 { get; set; }
        public double? SO2 { get; set; }
        public double? CO { get; set; }
        public int? AQI { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string SensorID { get; set; }
    }
}
