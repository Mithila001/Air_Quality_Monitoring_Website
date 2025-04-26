using SDTP_Project1.Data;
using SDTP_Project1.Models;
using Microsoft.AspNetCore.Identity;
using System;

namespace AQISystemIntegration.Tests.TestSetup
{
    public static class Utilities
    {
        // Initializes the database with a test admin user
        public static void InitializeDbForTests(AirQualityDbContext db)
        {
            var passwordHasher = new PasswordHasher<AdminUser>();

            var adminUser = new AdminUser
            {
                
                Name = "Test Admin",
                Email = "admin@example.com",
                UserRole = "System Admin",
                Gender = "Male", // Ensure Gender is set
                Age = 30, // Provide an age if required
                PhoneNumber = "123-456-7890", // Provide a valid phone number
                IsActive = true // Ensure IsActive is set if needed
            };

            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Test@123");

            db.AdminUsers.Add(adminUser);
            db.SaveChanges();
        }

        // Creates and returns a test admin user with default email and password
        public static AdminUser CreateTestAdmin(string email = "admin@example.com", string password = "Test@123")
        {
            var user = new AdminUser
            {
                
                Name = "Admin",
                Email = email,
                UserRole = "System Admin",
                Gender = "Male", // Ensure Gender is set
                Age = 30, // Provide an age if required
                PhoneNumber = "123-456-7890", // Provide a valid phone number
                IsActive = true // Ensure IsActive is set if needed
            };

            var hasher = new PasswordHasher<AdminUser>();
            user.PasswordHash = hasher.HashPassword(user, password);

            return user;
        }
    }
}
