using System;
using System.Collections.Generic;

namespace SDTP_Project1.Models
{
    public class SensorDataViewModel
    {
        public string SensorID { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        // For demonstration, here we include a list of readings. 
        // You might calculate an average or latest value for AQI.
        public List<AirQualityData> Readings { get; set; } = new List<AirQualityData>();

        // For example, you could add an aggregated property like LatestAQI.
        public int? LatestAQI => Readings?.Count > 0 ? Readings[^1].AQI : null;
    }
}
