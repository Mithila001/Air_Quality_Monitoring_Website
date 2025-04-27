using Microsoft.AspNetCore.Mvc;
using SDTP_Project1.Repositories;
using SDTP_Project1.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using SDTP_Project1.Services;
using Microsoft.AspNetCore.Authorization;
using SDTP_Project1.Data;
using Microsoft.EntityFrameworkCore;

namespace SDTP_Project1.Controllers
{
    [Authorize(Roles = "User Admin,System Admin")] // Fix: Uncommented, added System Admin
    public class AdminController : Controller
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly IAlertThresholdSettingRepository _alertRepo;
        private readonly ISensorService _sensorService;
        private readonly AirQualityDbContext _db;

        public AdminController(
            ISensorRepository sensorRepository,
            IAlertThresholdSettingRepository alertThresholdSettingRepository,
            ISensorService sensorService,
            AirQualityDbContext db)
        {
            _sensorRepository = sensorRepository ?? throw new ArgumentNullException(nameof(sensorRepository));
            _alertRepo = alertThresholdSettingRepository ?? throw new ArgumentNullException(nameof(alertThresholdSettingRepository));
            _sensorService = sensorService ?? throw new ArgumentNullException(nameof(sensorService));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // Dashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var sensors = await _sensorRepository.GetAllSensorsAsync();
                var averageAQI = await _sensorService.GetAverageAQILast30DaysForAllSensors();
                ViewBag.AverageAQI = averageAQI;

                // Fetch last 20 alerts
                var recentAlerts = await _db.AirQualityAlertHistory
                    .OrderByDescending(a => a.AlertedTime)
                    .Take(20)
                    .ToListAsync();

                ViewBag.RecentAlerts = recentAlerts;

                return View(sensors);
            }
            catch (Exception ex)
            {
                // Log the exception
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        //–– CreateSensor ––
        [HttpGet]
        public IActionResult CreateSensor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSensor(Sensor sensor)
        {
            try
            {
                // 1. City must be provided
                if (string.IsNullOrWhiteSpace(sensor.City))
                {
                    ModelState.AddModelError(nameof(sensor.City), "City is required.");
                    return View(sensor);
                }

                // 2. Generate SensorID and clear its ModelState error
                sensor.SensorID = $"S_{sensor.City.Substring(0, Math.Min(2, sensor.City.Length)).ToUpper()}_{DateTime.Now:yyyyMMddHHmm}";
                ModelState.Remove(nameof(sensor.SensorID));

                // 3. Set the registration date
                sensor.RegistrationDate = DateTime.Now;

                // 4. Initialize the navigation property
                sensor.AirQualityReadings = new List<AirQualityData>();
                ModelState.Remove(nameof(sensor.AirQualityReadings));

                // 5. Now check full validity
                if (!ModelState.IsValid)
                    return View(sensor);

                // 6. Persist and redirect
                await _sensorRepository.AddSensorAsync(sensor);
                TempData["SuccessMessage"] = "Sensor added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while adding the sensor: " + ex.Message);
                return View(sensor);
            }
        }

        //–– EditSensor ––
        [HttpGet]
        public async Task<IActionResult> EditSensor(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Sensor ID is required");
            }

            try
            {
                var sensor = await _sensorRepository.GetSensorByIdAsync(id);
                if (sensor == null) return NotFound();
                return PartialView("_EditSensorPartial", sensor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the sensor");
            }
        }

        // Define a specific result model for EditSensor
        public class EditSensorResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSensor(Sensor sensor)
        {
            if (sensor == null || string.IsNullOrEmpty(sensor.SensorID))
            {
                return BadRequest("Invalid sensor data");
            }

            // ── 1. Neutralize the phantom navigation‐property error ──
            sensor.AirQualityReadings = new List<AirQualityData>();
            ModelState.Remove(nameof(sensor.AirQualityReadings));

            // ── 2. Re-check ModelState now that only your posted fields remain ──
            if (!ModelState.IsValid)
            {
                // Return the partial view (with validation messages) back into the modal
                return PartialView("_EditSensorPartial", sensor);
            }

            // ── 3. Fetch existing, apply updates, save ──
            try
            {
                var existing = await _sensorRepository.GetSensorByIdAsync(sensor.SensorID);
                if (existing == null)
                {
                    return Json(new EditSensorResult { Success = false, Message = $"Sensor with ID {sensor.SensorID} not found." });
                }

                existing.City = sensor.City;
                existing.Latitude = sensor.Latitude;
                existing.Longitude = sensor.Longitude;
                existing.Description = sensor.Description;

                await _sensorRepository.UpdateSensorAsync(existing);

                // ── 4. Signal success back to your AJAX handler ──
                return Json(new EditSensorResult { Success = true, Message = "Sensor updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new EditSensorResult { Success = false, Message = "An error occurred while updating the sensor: " + ex.Message });
            }
        }

        // Define a specific result model for ToggleSensorStatus and DeleteSensor
        public class SensorStatusResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public bool? NewIsActive { get; set; } // Nullable bool for ToggleSensorStatus
        }

        //–– ToggleSensorStatus ––
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSensorStatus(string id, bool isActive)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new SensorStatusResult { Success = false, Message = "Sensor ID is required." });
            }

            try
            {
                var sensor = await _sensorRepository.GetSensorByIdAsync(id);
                if (sensor == null)
                    return Json(new SensorStatusResult { Success = false, Message = $"Sensor with ID {id} not found." });

                sensor.IsActive = isActive;
                await _sensorRepository.UpdateSensorAsync(sensor);
                return Json(new SensorStatusResult { Success = true, NewIsActive = sensor.IsActive, Message = "Sensor status updated." });
            }
            catch (Exception ex)
            {
                return Json(new SensorStatusResult { Success = false, Message = "An error occurred while toggling sensor status: " + ex.Message });
            }
        }

        //–– DeleteSensor ––
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSensor(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new SensorStatusResult { Success = false, Message = "Sensor ID is required." });
            }

            try
            {
                var sensor = await _sensorRepository.GetSensorByIdAsync(id);
                if (sensor == null) return Json(new SensorStatusResult { Success = false, Message = $"Sensor with ID {id} not found." });

                await _sensorRepository.DeleteSensorAsync(id);
                return Json(new SensorStatusResult { Success = true, Message = "Sensor deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new SensorStatusResult { Success = false, Message = "An error occurred while deleting the sensor: " + ex.Message });
            }
        }

        //–– Alert Threshold Settings ––
        [HttpGet]
        public async Task<IActionResult> AlertThresholdSettings()
        {
            try
            {
                var settings = await _alertRepo.GetAllAsync();
                return PartialView("_AlertThresholdSettingsPartial", settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving threshold settings");
            }
        }

        // Define a specific result model for UpdateAlertThresholds
        public class AlertThresholdResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAlertThresholds([FromBody] List<AlertThresholdSetting> updatedSettings)
        {
            if (updatedSettings == null || !updatedSettings.Any())
                return Json(new AlertThresholdResult { Success = false, Message = "No settings to update." });

            try
            {
                foreach (var s in updatedSettings)
                {
                    // Validate each setting
                    if (string.IsNullOrEmpty(s.Parameter) || s.ThresholdValue <= 0)
                    {
                        return Json(new AlertThresholdResult { Success = false, Message = "Invalid alert threshold settings." });
                    }

                    var exist = await _alertRepo.GetByParameterAsync(s.Parameter);
                    if (exist != null)
                    {
                        exist.ThresholdValue = s.ThresholdValue;
                        exist.IsActive = s.IsActive;
                        exist.LastUpdated = DateTime.Now;
                        await _alertRepo.UpdateAsync(exist);
                    }
                }
                return Json(new AlertThresholdResult { Success = true, Message = "Alert thresholds updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new AlertThresholdResult { Success = false, Message = "An error occurred while updating alert thresholds: " + ex.Message });
            }
        }
    }
}