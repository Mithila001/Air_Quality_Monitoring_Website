using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Data; // Assuming your DbContext is in the Data namespace
using SDTP_Project1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDTP_Project1.Services
{
    public interface ISensorService
    {
        Task<double> GetAverageAQILast30DaysForAllSensors();
    }
    public class SensorService : ISensorService
    {
        private readonly AirQualityDbContext _dbContext;

        public SensorService(AirQualityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<double> GetAverageAQILast30DaysForAllSensors()
        {
            var sensorsWithRecentReadings = await _dbContext.Sensors
                .Include(s => s.AirQualityReadings)
                .Where(s => s.AirQualityReadings.Any(r => r.Timestamp >= DateTime.Now.AddDays(-30)))
                .ToListAsync();

            if (!sensorsWithRecentReadings.Any())
            {
                return 0; // Or handle this case as needed (e.g., return null)
            }

            double totalAverageAQI = sensorsWithRecentReadings
                .Select(s => s.AirQualityReadings
                    .Where(r => r.Timestamp >= DateTime.Now.AddDays(-30))
                    .Average(r => (double?)r.AQI) ?? 0)
                .Average();

            return totalAverageAQI;
        }
    }
}
