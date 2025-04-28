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
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AQISystemIntegration.Tests.Auth
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // Prevent auto-redirects so we can see the redirect status codes
            });
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
            admin.IsActive = true; // Explicitly set IsActive to true

            dbContext.AdminUsers.Add(admin);
            await dbContext.SaveChangesAsync();
        }

        private async Task<(string token, IEnumerable<string> cookies)> GetAntiForgeryTokenAndCookiesAsync(string url = "/Auth/Login")
        {
            // Get the page to extract the anti-forgery token
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Get all cookies
            var cookies = response.Headers
                .Where(h => h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
                .SelectMany(h => h.Value);

            var content = await response.Content.ReadAsStringAsync();
            var tokenPattern = @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""";
            var tokenMatch = Regex.Match(content, tokenPattern);

            if (tokenMatch.Success)
            {
                return (tokenMatch.Groups[1].Value, cookies);
            }

            throw new InvalidOperationException($"Could not extract anti-forgery token from the page: {url}");
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
            // Get anti-forgery token and cookies
            var (token, cookies) = await GetAntiForgeryTokenAndCookiesAsync();

            // Create a request message with all cookies
            var request = new HttpRequestMessage(HttpMethod.Post, "/Auth/Login");

            // Add all cookies to the request
            foreach (var cookie in cookies)
            {
                request.Headers.Add("Cookie", cookie);
            }

            var formData = new Dictionary<string, string>
            {
                {"email", "admin@example.com"},
                {"password", "Test@123"},
                {"__RequestVerificationToken", token}
            };

            request.Content = new FormUrlEncodedContent(formData);

            // Send the request
            var response = await _client.SendAsync(request);

            // Log response content for debugging if status isn't what we expect
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.Found && response.StatusCode != HttpStatusCode.Redirect)
            {
                Console.WriteLine($"Response content: {content}");
                content.Should().NotContain("Invalid credentials");
                content.Should().NotContain("An error occurred");
            }

            // Check status code and redirection
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);
            response.Headers.Location?.ToString().Should().Contain("SystemAdmin");
        }

        [Fact]
        public async Task Login_Post_WithInvalidPassword_ShouldReturnLoginView()
        {
            // Get anti-forgery token and cookies
            var (token, cookies) = await GetAntiForgeryTokenAndCookiesAsync();

            // Create a request message with all cookies
            var request = new HttpRequestMessage(HttpMethod.Post, "/Auth/Login");
            foreach (var cookie in cookies)
            {
                request.Headers.Add("Cookie", cookie);
            }

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
            // Get anti-forgery token and cookies
            var (token, cookies) = await GetAntiForgeryTokenAndCookiesAsync();

            // Create a request message with all cookies
            var request = new HttpRequestMessage(HttpMethod.Post, "/Auth/Login");
            foreach (var cookie in cookies)
            {
                request.Headers.Add("Cookie", cookie);
            }

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
            var (loginToken, loginCookies) = await GetAntiForgeryTokenAndCookiesAsync("/Auth/Login");

            // Login first
            var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/Auth/Login");
            foreach (var cookie in loginCookies)
            {
                loginRequest.Headers.Add("Cookie", cookie);
            }

            var loginFormData = new Dictionary<string, string>
            {
                {"email", "admin@example.com"},
                {"password", "Test@123"},
                {"__RequestVerificationToken", loginToken}
            };

            loginRequest.Content = new FormUrlEncodedContent(loginFormData);
            var loginResponse = await _client.SendAsync(loginRequest);

            // Get cookies from login response
            var authCookies = loginResponse.Headers
                .Where(h => h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
                .SelectMany(h => h.Value)
                .ToList();

            // Now combine all cookies for the next request
            var allCookies = loginCookies.Concat(authCookies).ToList();

            // Get the home page to extract a fresh anti-forgery token
            var homeRequest = new HttpRequestMessage(HttpMethod.Get, "/");
            foreach (var cookie in allCookies)
            {
                homeRequest.Headers.Add("Cookie", cookie);
            }

            var homeResponse = await _client.SendAsync(homeRequest);
            var homeContent = await homeResponse.Content.ReadAsStringAsync();

            // Extract token from home page
            var logoutTokenMatch = Regex.Match(homeContent, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""");
            if (!logoutTokenMatch.Success)
            {
                // Fall back to using a form with a logout button
                logoutTokenMatch = Regex.Match(homeContent, @"<form[^>]*action=""[^""]*\/Auth\/Logout""[^>]*>.*?<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""");
            }

            // If still no match, check for logout form elsewhere
            if (!logoutTokenMatch.Success)
            {
                Assert.True(false, "Could not find logout form with anti-forgery token on home page");
            }

            var logoutToken = logoutTokenMatch.Groups[1].Value;

            // Create logout request with all cookies
            var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/Auth/Logout");
            foreach (var cookie in allCookies)
            {
                logoutRequest.Headers.Add("Cookie", cookie);
            }

            var logoutFormData = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", logoutToken}
            };

            logoutRequest.Content = new FormUrlEncodedContent(logoutFormData);

            // Send logout request
            var logoutResponse = await _client.SendAsync(logoutRequest);

            // Debug information if failing
            if (logoutResponse.StatusCode != HttpStatusCode.Found && logoutResponse.StatusCode != HttpStatusCode.Redirect)
            {
                var logoutContent = await logoutResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Logout response content: {logoutContent}");
            }

            logoutResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);
            logoutResponse.Headers.Location?.ToString().Should().Contain("/");
        }
    }
}