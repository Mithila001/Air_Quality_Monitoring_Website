using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SDTP_Project1;
using SDTP_Project1.Data;
using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace AQISystemIntegration.Tests.TestSetup
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private readonly DbConnection _connection;

        public CustomWebApplicationFactory()
        {
            // Create and open one single connection that stays alive for all tests
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext and DbConnection registrations
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AirQualityDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbConnection));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Always reuse the same opened connection
                services.AddSingleton<DbConnection>(_connection);

                services.AddDbContext<AirQualityDbContext>((container, options) =>
                {
                    var connection = container.GetRequiredService<DbConnection>();
                    options.UseSqlite(connection);
                });

                // Build service provider and create tables
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AirQualityDbContext>();
                    db.Database.EnsureCreated();
                }
            });

            builder.UseEnvironment("Testing");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                // Close and dispose the connection at the end
                _connection.Dispose();
            }
        }
    }
}
