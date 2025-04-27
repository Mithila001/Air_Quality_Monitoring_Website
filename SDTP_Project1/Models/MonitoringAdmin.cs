using System;
using System.ComponentModel.DataAnnotations;


// I dont think this file is used anywhere in the project
public class MonitoringAdmin
{
    [Key]
    public int AdminId { get; set; }

    [Required, StringLength(255)]
    public string Username { get; set; }

    [Required]
    public string PasswordHash { get; set; }  // Store hashed passwords only

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required, StringLength(50)]
    public string Role { get; set; } = "Admin";

    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
