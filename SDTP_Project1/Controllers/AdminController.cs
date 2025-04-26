using Microsoft.AspNetCore.Mvc;
using SDTP_Project1.Repositories;
using SDTP_Project1.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using SDTP_Project1.Services;
using Microsoft.AspNetCore.Authorization;

namespace SDTP_Project1.Controllers
{
    //[Authorize(Roles = "User Admin")]
    public class AdminController : Controller
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly IAlertThresholdSettingRepository _alertRepo;
        private readonly ISensorService _sensorService;

        public AdminController(
            ISensorRepository sensorRepository,
            IAlertThresholdSettingRepository alertThresholdSettingRepository,
            ISensorService sensorService)
        {
            _sensorRepository = sensorRepository;
            _alertRepo = alertThresholdSettingRepository;
            _sensorService = sensorService;
        }

        // Dashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sensors = await _sensorRepository.GetAllSensorsAsync();
            var averageAQI = await _sensorService.GetAverageAQILast30DaysForAllSensors();
            ViewBag.AverageAQI = averageAQI;
            return View(sensors);
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
            // 1. City must be provided
            if (string.IsNullOrWhiteSpace(sensor.City))
            {
                ModelState.AddModelError(nameof(sensor.City), "City is required.");
                return View(sensor);
            }

            // 2. Generate SensorID and clear its ModelState error
            sensor.SensorID = $"S_{sensor.City.Substring(0, 2).ToUpper()}_{DateTime.Now:yyyyMMddHHmm}";
            ModelState.Remove(nameof(sensor.SensorID));

            // 3. Set the registration date
            sensor.RegistrationDate = DateTime.Now;

            // 4. Initialize the navigation property so the binder/validator sees a valid (empty) collection
            sensor.AirQualityReadings = new List<AirQualityData>();
            //   —and remove any leftover ModelState entry for it
            ModelState.Remove(nameof(sensor.AirQualityReadings));

            // 5. Now check full validity
            if (!ModelState.IsValid)
                return View(sensor);

            // 6. Persist and redirect
            await _sensorRepository.AddSensorAsync(sensor);
            TempData["SuccessMessage"] = "Sensor added successfully!";
            return RedirectToAction(nameof(Index));
        }


        //–– EditSensor ––
        [HttpGet]
        public async Task<IActionResult> EditSensor(string id)
        {
            var sensor = await _sensorRepository.GetSensorByIdAsync(id);
            if (sensor == null) return NotFound();
            return PartialView("_EditSensorPartial", sensor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSensor(Sensor sensor)
        {
            // ── 1. Neutralize the phantom navigation‐property error ──
            //    The form never binds AirQualityReadings, so clear it out:
            sensor.AirQualityReadings = new List<AirQualityData>();
            ModelState.Remove(nameof(sensor.AirQualityReadings));

            // ── 2. Re-check ModelState now that only your posted fields remain ──
            if (!ModelState.IsValid)
            {
                // Return the partial view (with validation messages) back into the modal
                return PartialView("_EditSensorPartial", sensor);
            }

            // ── 3. Fetch existing, apply updates, save ──
            var existing = await _sensorRepository.GetSensorByIdAsync(sensor.SensorID);
            if (existing == null)
                return NotFound();

            existing.City = sensor.City;
            existing.Latitude = sensor.Latitude;
            existing.Longitude = sensor.Longitude;
            existing.Description = sensor.Description;

            await _sensorRepository.UpdateSensorAsync(existing);

            // ── 4. Signal success back to your AJAX handler ──
            return Json(new { success = true });
        }


        //–– ToggleSensorStatus ––
        [HttpPost]
        [ValidateAntiForgeryToken]   // ← must have this
        public async Task<IActionResult> ToggleSensorStatus(string id, bool isActive)
        {
            var sensor = await _sensorRepository.GetSensorByIdAsync(id);
            if (sensor == null)
                return Json(new { success = false, message = "Sensor not found." });

            sensor.IsActive = isActive;
            await _sensorRepository.UpdateSensorAsync(sensor);
            return Json(new { success = true, newIsActive = sensor.IsActive });
        }

        //–– DeleteSensor ––
        [HttpPost]
        [ValidateAntiForgeryToken]   // ← must have this
        public async Task<IActionResult> DeleteSensor(string id)
        {
            var sensor = await _sensorRepository.GetSensorByIdAsync(id);
            if (sensor == null) return Json(new { success = false, message = "Sensor not found." });

            await _sensorRepository.DeleteSensorAsync(id);
            return Json(new { success = true, message = "Sensor deleted successfully!" });
        }

        //–– Alert Threshold Settings ––
        [HttpGet]
        public async Task<IActionResult> AlertThresholdSettings()
        {
            var settings = await _alertRepo.GetAllAsync();
            return PartialView("_AlertThresholdSettingsPartial", settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]   // ← must have this
        public async Task<IActionResult> UpdateAlertThresholds([FromBody] List<AlertThresholdSetting> updatedSettings)
        {
            if (updatedSettings == null || !updatedSettings.Any())
                return Json(new { success = false, message = "No settings to update." });

            try
            {
                foreach (var s in updatedSettings)
                {
                    var exist = await _alertRepo.GetByParameterAsync(s.Parameter);
                    if (exist != null)
                    {
                        exist.ThresholdValue = s.ThresholdValue;
                        exist.IsActive = s.IsActive;
                        exist.LastUpdated = DateTime.Now;
                        await _alertRepo.UpdateAsync(exist);
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
