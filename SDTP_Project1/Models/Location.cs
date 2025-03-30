namespace SDTP_Project1.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        // Simulated air quality sensor data
        public int AirQualityIndex { get; set; }
    }
}
