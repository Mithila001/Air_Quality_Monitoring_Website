using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Models; // Adjust namespace if needed

namespace SDTP_Project1.Data;
public class AirQualityDbContext : DbContext
{
    public AirQualityDbContext(DbContextOptions<AirQualityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<AirQualityData> AirQualityData { get; set; }
    public DbSet<MonitoringAdmin> MonitoringAdmins { get; set; }
    public DbSet<SimulationConfiguration> SimulationConfigurations { get; set; }
    public DbSet<AlertThresholdSetting> AlertThresholdSettings { get; set; }

    // For Extra Safety:
    //    This method is used to configure model relationships using Fluent API.
    //    It's cleaner and more flexible than just using data annotations.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //  Define the one-to-many relationship between Sensor and AirQualityData
        modelBuilder.Entity<AirQualityData>()
            .HasOne(aq => aq.Sensor)  // Target entity: AirQualityData
            .WithMany(s => s.AirQualityReadings) // Each Sensor has many AirQualityReadings
            .HasForeignKey(aq => aq.SensorID) // Link via SensorID foreign key
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade deletion
                                                // i.e., don’t auto-delete sensor readings
                                                // when a sensor is deleted
    }
}
