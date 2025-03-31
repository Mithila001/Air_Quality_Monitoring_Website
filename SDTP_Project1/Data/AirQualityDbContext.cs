using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Models;

namespace SDTP_Project1.Data
{
    public class AirQualityDbContext : DbContext
    {
        public AirQualityDbContext(DbContextOptions<AirQualityDbContext> options) : base(options) { }

        public DbSet<AirQualityData> AirQualityData { get; set; }
        public DbSet<Sensor> Sensors { get; set; }

        // New Parts
        public DbSet<MonitoringAdmin> MonitoringAdmins { get; set; }
        public DbSet<SimulationConfiguration> SimulationConfigurations { get; set; }
        public DbSet<AlertThresholdSetting> AlertThresholdSettings { get; set; }
    }
}
