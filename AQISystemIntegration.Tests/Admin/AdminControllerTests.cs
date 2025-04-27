using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using AQISystemIntegration.Tests.TestSetup;
using Microsoft.Extensions.DependencyInjection;
using SDTP_Project1.Data;
using SDTP_Project1.Models;
using System.Collections.Generic;

namespace AQISystemIntegration.Tests.Admin
{
    public class AdminControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AdminControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            InitializeDatabaseAsync().GetAwaiter().GetResult();
            AuthenticateAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AirQualityDbContext>();

            // Clean DB first
            dbContext.AdminUsers.RemoveRange(dbContext.AdminUsers);
            dbContext.Sensors.RemoveRange(dbContext.Sensors);
            await dbContext.SaveChangesAsync();

            // Add dummy admin user
            var adminUser = new AdminUser
            {
                Name = "testadmin",
                Email = "admin@test.com",
                Gender = "Male",
                Age = 17,
                UserRole = "Admin",
                IsActive = true,
                PhoneNumber = "1234567890",
                PasswordHash = "hashedpassword" // Assume hash checking is mocked/disabled
            };
            dbContext.AdminUsers.Add(adminUser);

            // Add dummy sensor
            dbContext.Sensors.Add(new Sensor
            {
                SensorID = "SENSOR001",
                City = "Colombo",
                Latitude = 6.9271,
                Longitude = 79.8612,
                IsActive = true,
                Description = "abc"
            });

            await dbContext.SaveChangesAsync();
        }

        private async Task AuthenticateAsync()
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme");
        }

        [Fact]
        public async Task Index_ShouldReturnOkAndViewContent()
        {
            var response = await _client.GetAsync("/Admin/Index");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("<!DOCTYPE html>");
            content.Should().Contain("AQI Dashboard");
        }

        [Fact]
        public async Task CreateSensor_Post_ValidData_ShouldRedirect()
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("SensorID", "SENSOR002"),
                new KeyValuePair<string, string>("City", "Colombo 02"),
                new KeyValuePair<string, string>("Latitude", "6.915"),
                new KeyValuePair<string, string>("Longitude", "79.848"),
                new KeyValuePair<string, string>("IsActive", "true"),
                new KeyValuePair<string, string>("Description", "Test sensor for area 02")
            });

            var response = await _client.PostAsync("/Admin/CreateSensor", content);

            response.StatusCode.Should().Be(HttpStatusCode.Found); // 302 Redirect
        }

        [Fact]
        public async Task ToggleSensorStatus_InvalidSensor_ShouldReturnFailureJson()
        {
            var response = await _client.PostAsync("/Admin/ToggleSensorStatus/9999", null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            jsonResponse.Should().Contain("failure");
        }

        [Fact]
        public async Task DeleteSensor_InvalidSensor_ShouldReturnFailureJson()
        {
            var response = await _client.PostAsync("/Admin/DeleteSensor/9999", null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            jsonResponse.Should().Contain("failure");
        }

        [Fact]
        public async Task UpdateAlertThresholds_NullList_ShouldReturnFailureJson()
        {
            var content = new StringContent("null", Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/Admin/UpdateAlertThresholds", content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            jsonResponse.Should().Contain("failure");
        }
    }
}
