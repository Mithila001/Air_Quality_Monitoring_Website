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
    }
}
