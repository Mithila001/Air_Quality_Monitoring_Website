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
using SDTP_Project1.Helpers;

namespace SDTP_Project1.Services
{
    public class SensorDataSimulationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DevModeState _devModeState; // Inject DevModeState
        private readonly IOptionsMonitor<DevModeOptions> _devModeOptions; // Inject DevModeOptions

        public SensorDataSimulationService(
            IServiceScopeFactory scopeFactory,
            DevModeState devModeState, // Receive injected DevModeState
            IOptionsMonitor<DevModeOptions> devModeOptions)

        {
            _scopeFactory = scopeFactory;
            _devModeState = devModeState;
            _devModeOptions = devModeOptions;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var period = _devModeState.Enabled // Use the in-memory state
                    ? TimeSpan.FromSeconds(_devModeOptions.CurrentValue.FastIntervalSeconds)
                    : _devModeOptions.CurrentValue.ProductionInterval;

                using var timer = new PeriodicTimer(period);

                await SimulateAndStoreReadingsAsync(stoppingToken);

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
        }

        // Convert to 2 decimal places
        private static double Round2(double val) => Math.Round(val, 2);


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
                // 1) Sample from weighted ranges
                double pm25 = SimulationHelpers.SamplePM25();
                double pm10 = SimulationHelpers.SamplePM10();
                double o3 = SimulationHelpers.SampleO3();
                double no2 = SimulationHelpers.SampleNO2();
                double so2 = SimulationHelpers.SampleSO2();
                double co = SimulationHelpers.SampleCO();

                // 2) Compute AQI using your AqiCalculator
                int aqi = AqiCalculator.ComputeAqi(pm25, pm10, o3, no2, so2, co);

                // 3) Round and store
                var entry = new AirQualityData
                {
                    SensorID = sensor.SensorID,
                    Timestamp = now,
                    PM2_5 = Round2(pm25),
                    PM10 = Round2(pm10),
                    O3 = Round2(o3),
                    NO2 = Round2(no2),
                    SO2 = Round2(so2),
                    CO = Round2(co),
                    AQI = aqi
                };

                db.AirQualityData.Add(entry);
            }

            await db.SaveChangesAsync(token);
        }
    }
}
