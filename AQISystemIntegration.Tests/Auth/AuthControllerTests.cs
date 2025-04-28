using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using AQISystemIntegration.Tests.TestSetup;
using Microsoft.Extensions.DependencyInjection;
using SDTP_Project1.Data;
using SDTP_Project1.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

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

            // Clean database first to avoid duplicates
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            // Create a test admin with specific role for redirection testing
            var admin = Utilities.CreateTestAdmin("admin@example.com", "Test@123");
            admin.UserRole = "System Admin"; // This role will cause redirection to SystemAdmin controller

            dbContext.AdminUsers.Add(admin);
            dbContext.SaveChanges();
        }

        private async Task<(string token, string cookie)> GetAntiForgeryTokenAndCookieAsync()
        {
            // Get the login page to extract the anti-forgery token
            var response = await _client.GetAsync("/Auth/Login");
            response.EnsureSuccessStatusCode();

            // Get the cookie
            var setCookieHeaders = response.Headers.GetValues("Set-Cookie");
            var cookie = string.Join(";", setCookieHeaders);

            var content = await response.Content.ReadAsStringAsync();
            var tokenPattern = @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""";
            var tokenMatch = Regex.Match(content, tokenPattern);

            if (tokenMatch.Success)
            {
                return (tokenMatch.Groups[1].Value, cookie);
            }

            throw new InvalidOperationException("Could not extract anti-forgery token from the login page");
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
            // Get anti-forgery token and cookie
            var (token, cookie) = await GetAntiForgeryTokenAndCookieAsync();

            // Create a request message with the cookie
            var request = new HttpRequestMessage(HttpMethod.Post, "/Auth/Login");
            request.Headers.Add("Cookie", cookie);

            var formData = new Dictionary<string, string>
            {
                {"email", "admin@example.com"},
                {"password", "Test@123"},
                {"__RequestVerificationToken", token}
            };

            request.Content = new FormUrlEncodedContent(formData);

            // Send the request
            var response = await _client.SendAsync(request);

            // Log response content for debugging
            var content = await response.Content.ReadAsStringAsync();

            // Check status code and redirection
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);
            response.Headers.Location?.ToString().Should().Contain("SystemAdmin");
        }

        [Fact]
        public async Task Login_Post_WithInvalidPassword_ShouldReturnLoginView()
        {
            // Get anti-forgery token and cookie
            var (token, cookie) = await GetAntiForgeryTokenAndCookieAsync();

            // Create a request message with the cookie
            var request = new HttpRequestMessage(HttpMethod.Post, "/Auth/Login");
            request.Headers.Add("Cookie", cookie);

            var formData = new Dictionary<string, string>
            {
                {"email", "admin@example.com"},
                {"password", "wrongpassword"},
                {"__RequestVerificationToken", token}
            };

            request.Content = new FormUrlEncodedContent(formData);

            // Send the request
            var response = await _client.SendAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Invalid credentials");
        }

        [Fact]
        public async Task Login_Post_WithInvalidEmail_ShouldReturnLoginView()
        {
            // Get anti-forgery token and cookie
            var (token, cookie) = await GetAntiForgeryTokenAndCookieAsync();

            // Create a request message with the cookie
            var request = new HttpRequestMessage(HttpMethod.Post, "/Auth/Login");
            request.Headers.Add("Cookie", cookie);

            var formData = new Dictionary<string, string>
            {
                {"email", "nonexistent@example.com"},
                {"password", "whatever"},
                {"__RequestVerificationToken", token}
            };

            request.Content = new FormUrlEncodedContent(formData);

            // Send the request
            var response = await _client.SendAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Invalid credentials");
        }

        [Fact]
        public async Task Logout_ShouldRedirectToHomeIndex()
        {
            // First authenticate
            var (token, cookie) = await GetAntiForgeryTokenAndCookieAsync();

            // Login first
            var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/Auth/Login");
            loginRequest.Headers.Add("Cookie", cookie);

            var loginFormData = new Dictionary<string, string>
            {
                {"email", "admin@example.com"},
                {"password", "Test@123"},
                {"__RequestVerificationToken", token}
            };

            loginRequest.Content = new FormUrlEncodedContent(loginFormData);
            var loginResponse = await _client.SendAsync(loginRequest);

            // Get cookies from login response
            var loginCookies = string.Join(";", loginResponse.Headers.GetValues("Set-Cookie"));

            // Get a new anti-forgery token after logging in
            var homeResponse = await _client.GetAsync("/");
            var homeCookies = string.Join(";", homeResponse.Headers.GetValues("Set-Cookie"));
            var homeContent = await homeResponse.Content.ReadAsStringAsync();

            var logoutTokenMatch = Regex.Match(homeContent, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""");
            var logoutToken = logoutTokenMatch.Success ? logoutTokenMatch.Groups[1].Value : token;

            // Create logout request
            var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/Auth/Logout");
            logoutRequest.Headers.Add("Cookie", $"{cookie};{loginCookies};{homeCookies}");

            var logoutFormData = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", logoutToken}
            };

            logoutRequest.Content = new FormUrlEncodedContent(logoutFormData);

            // Send logout request
            var logoutResponse = await _client.SendAsync(logoutRequest);
            logoutResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);
            logoutResponse.Headers.Location?.ToString().Should().Contain("/Home/Index");
        }
    }
}