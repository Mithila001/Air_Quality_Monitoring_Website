// Models/AdminUser.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace SDTP_Project1.Models
{
    public class AdminUser
    {
        [Key] // Specifies that Id is the primary key
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        [Range(16, 100, ErrorMessage = "Age must be between 16 and 100.")]
        public int Age { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "User Role is required.")]
        public string UserRole { get; set; }

        public DateTime RegisterDate { get; set; } = DateTime.Now; // Default to current date and time

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
        public string Email { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters.")]
        public string PhoneNumber { get; set; }

        // Passwprd Hash
        [Required, DataType(DataType.Password)]
        [StringLength(100)]
        public string PasswordHash { get; set; }

    }
}