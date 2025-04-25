using System;
using System.ComponentModel.DataAnnotations;

namespace SDTP_Project1.Models
{   
    public class Sensor
    {
        [Key]
        public string SensorID { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Latitude is required.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required.")]
        public double Longitude { get; set; }

        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string Description { get; set; }


        // For Extra Safety:
        //    Navigation property — this tells Entity Framework that a single sensor
        //    can have *many* air quality readings (one-to-many relationship).
        //    We name it "AirQualityReadings" to be clear about what it represents.
        //
        //    This is not stored in the database directly; it's used by EF Core internally
        //    to join and query related data between Sensor ↔ AirQualityData tables.
        public ICollection<AirQualityData> AirQualityReadings { get; set; }
    }
}
