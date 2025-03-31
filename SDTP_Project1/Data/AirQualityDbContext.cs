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
}
