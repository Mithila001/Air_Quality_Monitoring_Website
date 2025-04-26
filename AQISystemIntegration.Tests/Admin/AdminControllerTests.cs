using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using AQISystemIntegration.Tests.TestSetup;
using Microsoft.Extensions.DependencyInjection;
using SDTP_Project1.Data;

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
            Utilities.InitializeDbForTests(dbContext);
        }

        private async Task AuthenticateAsync()
        {
            var loginData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", "admin@example.com"),
                new KeyValuePair<string, string>("password", "Test@123")
            });

            var response = await _client.PostAsync("/Auth/Login", loginData);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Index_ShouldReturnOkAndViewContent()
        {
            var response = await _client.GetAsync("/Admin/Index");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Sensors").And.Contain("Average AQI");
        }

        [Fact]
        public async Task CreateSensor_Post_ValidData_ShouldRedirect()
        {
            var sensorForm = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("SensorID", "INT_001"),
                new KeyValuePair<string, string>("City", "Colombo"),
                new KeyValuePair<string, string>("Location", "Central Park"),
                new KeyValuePair<string, string>("IsActive", "true")
            });

            var response = await _client.PostAsync("/Admin/CreateSensor", sensorForm);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location?.ToString().Should().Contain("Admin");
        }

        [Fact]
        public async Task ToggleSensorStatus_InvalidSensor_ShouldReturnFailureJson()
        {
            var response = await _client.PostAsync("/Admin/ToggleSensorStatus?id=INVALID_ID&isActive=true", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Sensor not found");
        }

        [Fact]
        public async Task DeleteSensor_InvalidSensor_ShouldReturnFailureJson()
        {
            var response = await _client.PostAsync("/Admin/DeleteSensor?id=INVALID_ID", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Sensor not found");
        }

        [Fact]
        public async Task UpdateAlertThresholds_NullList_ShouldReturnFailureJson()
        {
            var response = await _client.PostAsync("/Admin/UpdateAlertThresholds", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("No settings to update");
        }
    }
}
