using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using SDTP_Project1.Data;
using SDTP_Project1.Helpers;
using SDTP_Project1.Hubs;
using SDTP_Project1.Models;

namespace SDTP_Project1.Services
{
    public class SensorDataSimulationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DevModeState _devModeState;
        private readonly IOptionsMonitor<DevModeOptions> _devModeOptions;
        private readonly IHubContext<AirQualityHub> _hubContext;
        private readonly ILogger<SensorDataSimulationService> _logger;

        public SensorDataSimulationService(
            IServiceScopeFactory scopeFactory,
            DevModeState devModeState,
            IOptionsMonitor<DevModeOptions> devModeOptions,
            IHubContext<AirQualityHub> hubContext,
            ILogger<SensorDataSimulationService> logger)
        {
            _scopeFactory = scopeFactory;
            _devModeState = devModeState;
            _devModeOptions = devModeOptions;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var period = _devModeState.Enabled
                    ? TimeSpan.FromSeconds(_devModeOptions.CurrentValue.FastIntervalSeconds)
                    : _devModeOptions.CurrentValue.ProductionInterval;

                using var timer = new PeriodicTimer(period);

                try
                {
                    await SimulateAndStoreReadingsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in SimulateAndStoreReadingsAsync");
                    // swallow or rethrow? Here we swallow so the service stays alive.
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
        }

        private static double Round2(double val) => Math.Round(val, 2);

        private async Task SimulateAndStoreReadingsAsync(CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AirQualityDbContext>();
            var now = DateTime.UtcNow;

            // 1) Load sensors & thresholds
            var sensors = await db.Sensors.Where(s => s.IsActive).ToListAsync(token);
            var thresholds = await db.AlertThresholdSettings.Where(t => t.IsActive).ToListAsync(token);

            // 2) Phase 1: create AirQualityData entries (but not history yet)
            var dataEntries = new List<AirQualityData>();
            foreach (var sensor in sensors)
            {
                var (pm25, pm10, o3, no2, so2, co) = SimulationHelpers.SampleCorrelatedReadings();


                dataEntries.Add(new AirQualityData
                {
                    SensorID = sensor.SensorID,
                    Timestamp = now,
                    PM2_5 = SimulationHelpers.Round2(pm25),
                    PM10 = SimulationHelpers.Round2(pm10),
                    O3 = SimulationHelpers.Round2(o3),
                    NO2 = SimulationHelpers.Round2(no2),
                    SO2 = SimulationHelpers.Round2(so2),
                    CO = SimulationHelpers.Round2(co),
                    AQI = AqiCalculator.ComputeAqi(pm25, pm10, o3, no2, so2, co)
                });
            }

            db.AirQualityData.AddRange(dataEntries);
            await db.SaveChangesAsync(token);

            // 3) Phase 2: build history + alert DTOs now that MeasurementID exists
            var historyEntries = new List<AirQualityAlertHistory>();
            var alertDtos = new List<object>();

            foreach (var entry in dataEntries)
            {
                var sensor = sensors.Single(s => s.SensorID == entry.SensorID);

                foreach (var t in thresholds)
                {
                    double current = t.Parameter switch
                    {
                        "AQI" => entry.AQI ?? double.MinValue,
                        "PM2_5" => entry.PM2_5 ?? double.MinValue,
                        "PM10" => entry.PM10 ?? double.MinValue,
                        _ => double.MinValue
                    };

                    if (current >= t.ThresholdValue)
                    {
                        // record history
                        historyEntries.Add(new AirQualityAlertHistory
                        {
                            SensorID = entry.SensorID,
                            MeasurementID = entry.MeasurementID,
                            Parameter = t.Parameter,
                            CurrentValue = current,
                            ThresholdValue = t.ThresholdValue,
                            AlertedTime = now
                        });

                        // prepare broadcast
                        alertDtos.Add(new
                        {
                            SensorId = entry.SensorID,
                            City = sensor.City,
                            Parameter = t.Parameter,
                            CurrentValue = current,
                            ThresholdValue = t.ThresholdValue,
                            Timestamp = now
                        });

                        _logger.LogInformation(
                            "Alert: {Sensor} {Param}={Val} ≥ {Thresh}",
                            entry.SensorID, t.Parameter, current, t.ThresholdValue);
                    }
                }
            }

            if (historyEntries.Any())
            {
                db.AirQualityAlertHistory.AddRange(historyEntries);
                await db.SaveChangesAsync(token);
            }

            // 4) Broadcast
            if (alertDtos.Any())
            {
                await _hubContext.Clients.All
                    .SendAsync("ReceiveAlerts", alertDtos, cancellationToken: token);
            }
        }
    }
}
