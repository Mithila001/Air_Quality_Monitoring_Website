using System;
using System.ComponentModel.DataAnnotations;

namespace SDTP_Project1.Models
{
    public class Sensor
    {
        [Key]
        public string SensorID { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string Description { get; set; }
    }
}
