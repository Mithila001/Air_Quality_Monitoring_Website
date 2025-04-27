using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SDTP_Project1.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore.InMemory;

namespace AQISystemIntegration.Tests.TestSetup
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // 1. Find and remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AirQualityDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 2. ALSO REMOVE AirQualityDbContext itself if directly registered
                var contextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(AirQualityDbContext));

                if (contextDescriptor != null)
                {
                    services.Remove(contextDescriptor);
                }

                // 3. Now add the InMemory database
                services.AddDbContext<AirQualityDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryAirQualityTestDb");
                });

                // 4. Fake authentication
                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "TestScheme", options => { });

                // 5. Build service provider
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AirQualityDbContext>();
                db.Database.EnsureCreated();
            });
        }


    }
}
