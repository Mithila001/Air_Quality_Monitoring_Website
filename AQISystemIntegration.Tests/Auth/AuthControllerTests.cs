using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using AQISystemIntegration.Tests.TestSetup;
using Microsoft.Extensions.DependencyInjection;
using SDTP_Project1.Data;

namespace AQISystemIntegration.Tests.Auth
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            InitializeDatabaseAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AirQualityDbContext>();
            Utilities.InitializeDbForTests(dbContext);
        }

        [Fact]
        public async Task Login_Get_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/Auth/Login");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Login");
        }

        [Fact]
        public async Task Login_Post_WithValidCredentials_ShouldRedirect()
        {
            var postData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", "admin@example.com"),
                new KeyValuePair<string, string>("password", "Test@123")
            });

            var response = await _client.PostAsync("/Auth/Login", postData);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location?.ToString().Should().Contain("SystemAdmin");
        }

        [Fact]
        public async Task Login_Post_WithInvalidPassword_ShouldReturnLoginView()
        {
            var postData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", "admin@example.com"),
                new KeyValuePair<string, string>("password", "wrongpassword")
            });

            var response = await _client.PostAsync("/Auth/Login", postData);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Invalid credentials");
        }

        [Fact]
        public async Task Login_Post_WithInvalidEmail_ShouldReturnLoginView()
        {
            var postData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", "nonexistent@example.com"),
                new KeyValuePair<string, string>("password", "whatever")
            });

            var response = await _client.PostAsync("/Auth/Login", postData);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Invalid credentials");
        }

        [Fact]
        public async Task Logout_ShouldRedirectToHomeIndex()
        {
            var response = await _client.PostAsync("/Auth/Logout", new StringContent(""));

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location?.ToString().Should().Contain("/Home/Index");
        }
    }
}
