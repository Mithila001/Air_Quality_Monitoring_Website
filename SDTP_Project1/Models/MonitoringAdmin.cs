using System;
using System.ComponentModel.DataAnnotations;

namespace SDTP_Project1.Models {
    public class MonitoringAdmin
    {
        [Key]
        public int AdminId { get; set; }

        [Required, StringLength(255)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;  // Store hashed passwords only

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Role { get; set; } = "Admin"; 

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
