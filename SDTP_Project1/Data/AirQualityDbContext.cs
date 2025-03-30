using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Models;

namespace SDTP_Project1.Data
{
    public class AirQualityDbContext : DbContext
    {
        public AirQualityDbContext(DbContextOptions<AirQualityDbContext> options) : base(options)
        {
        }

        public DbSet<Location> Locations { get; set; }
    }
}
