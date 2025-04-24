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
    public DbSet<AdminUser> AdminUsers { get; set; }

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


        //// Configure the AdminUser entity
        //    modelBuilder.Entity<AdminUser>(entity =>
        //    {
        //        entity.ToTable("AdminUsers"); // Explicitly set table name (optional, but good practice)
        //        entity.HasKey(e => e.Id);     // Define primary key

        //        entity.Property(e => e.Name)
        //            .IsRequired()
        //            .HasMaxLength(100);

        //        entity.Property(e => e.Gender)
        //            .IsRequired();

        //        entity.Property(e => e.Email)
        //            .HasMaxLength(100);

        //        entity.Property(e => e.UserRole)
        //            .IsRequired();

        //        entity.Property(e => e.RegisterDate)
        //            .HasColumnType("datetime2") // Specify datetime2 for better compatibility
        //            .HasDefaultValueSql("GETDATE()");
        //    });
    }


}
