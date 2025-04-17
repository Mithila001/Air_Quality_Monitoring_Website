using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SDTP_Project1.Data;
using SDTP_Project1.Models;

namespace SDTP_Project1.Services
{
    public class SensorDataSimulationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<DevModeOptions> _settings;

        public SensorDataSimulationService(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<DevModeOptions> settings)
        {
            _scopeFactory = scopeFactory;
            _settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SimulateAndStoreReadingsAsync(stoppingToken);

                // choose interval based on DevMode flag
                var cfg = _settings.CurrentValue;
                var wait = cfg.Enabled
                    ? TimeSpan.FromSeconds(cfg.FastIntervalSeconds)
                    : cfg.ProductionInterval;

            }
        }

        private async Task SimulateAndStoreReadingsAsync(CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AirQualityDbContext>();

            var now = DateTime.UtcNow;
            var activeSensors = await db.Sensors
                                      .Where(s => s.IsActive)
                                      .ToListAsync(token);

            foreach (var sensor in activeSensors)
            {
                // 1) Generate random pollutant values
                double pm25 = Random.Shared.NextDouble() * 150;   // 0–150 µg/m³
                double pm10 = Random.Shared.NextDouble() * 200;   // 0–200 µg/m³
                double o3 = Random.Shared.NextDouble() * 180;   // 0–180 ppb
                double no2 = Random.Shared.NextDouble() * 200;   // 0–200 ppb
                double so2 = Random.Shared.NextDouble() * 75;    // 0–75 ppb
                double co = Random.Shared.NextDouble() * 10;    // 0–10 ppm

                // 2) Simple AQI placeholder (e.g. max of normalized sub-indices)
                int aqi = (int)new[] { pm25 / 150, pm10 / 200, o3 / 180, no2 / 200, so2 / 75, co / 10 }
                          .Max() * 500;  // scale to 0–500

                var entry = new AirQualityData
                {
                    SensorID = sensor.SensorID,
                    Timestamp = now,
                    PM2_5 = pm25,
                    PM10 = pm10,
                    O3 = o3,
                    NO2 = no2,
                    SO2 = so2,
                    CO = co,
                    AQI = aqi
                };

                db.AirQualityData.Add(entry);
            }

            await db.SaveChangesAsync(token);
        }
    }
}
