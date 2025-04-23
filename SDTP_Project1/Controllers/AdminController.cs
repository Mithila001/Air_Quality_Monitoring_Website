using Microsoft.AspNetCore.Mvc;
using SDTP_Project1.Repositories;
using SDTP_Project1.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;


namespace SDTP_Project1.Controllers;
public class AdminController : Controller
{
    private readonly ISensorRepository _sensorRepository;
    private readonly IAlertThresholdSettingRepository _alertThresholdSettingRepository;

    public AdminController(ISensorRepository sensorRepository, IAlertThresholdSettingRepository alertThresholdSettingRepository)
    {
        _sensorRepository = sensorRepository;
        _alertThresholdSettingRepository = alertThresholdSettingRepository;
    }

    // Dashboard view (System Dashboard)
    public async Task<IActionResult> Index()
    {
        var sensors = await _sensorRepository.GetAllSensorsAsync();
        // You can also aggregate statistics for the dashboard
        return View(sensors);
    }

    // Sensor management actions
    [HttpGet]
    public IActionResult CreateSensor()
    {
        return View();
    }

    // Create a Sensor
    [HttpPost]
    public async Task<IActionResult> CreateSensor(Sensor sensor)
    {
        // Generate SensorID before validation
        if (!string.IsNullOrWhiteSpace(sensor.City))
        {
            sensor.SensorID = $"S_{sensor.City.Trim()}_{DateTime.Now:yyyyMMddHHmm}";
        }
        else
        {
            ModelState.AddModelError("City", "City is required.");
            return View(sensor);
        }

        // Remove ModelState error for SensorID because we are generating it manually
        ModelState.Remove("SensorID");

        // Ensure RegistrationDate is properly set
        sensor.RegistrationDate = DateTime.Now;

        // Check ModelState validity after adjustments
        if (ModelState.IsValid)
        {
            await _sensorRepository.AddSensorAsync(sensor);
            TempData["SuccessMessage"] = "Sensor added successfully!";
            return RedirectToAction("Index");
        }

        return View(sensor);
    }


    // Edit sensor
    [HttpGet]
    public async Task<IActionResult> EditSensor(string id)
    {
        var sensor = await _sensorRepository.GetSensorByIdAsync(id);
        if (sensor == null)
        {
            return NotFound();
        }
        return PartialView("_EditSensorPartial", sensor);
    }

    [HttpPost]
    public async Task<IActionResult> EditSensor(Sensor sensor)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_EditSensorPartial", sensor); // If validation fails, show errors in the modal
        }

        var existingSensor = await _sensorRepository.GetSensorByIdAsync(sensor.SensorID);
        if (existingSensor == null)
        {
            return NotFound();
        }

        // Update sensor properties
        existingSensor.City = sensor.City;
        existingSensor.Latitude = sensor.Latitude;
        existingSensor.Longitude = sensor.Longitude;
        existingSensor.Description = sensor.Description;

        await _sensorRepository.UpdateSensorAsync(existingSensor);

        // Return JSON to indicate success
        return Json(new { success = true, message = "Sensor updated successfully!" });
    }

    // Deactivate sensor
    //[HttpPost]
    //public async Task<IActionResult> DeactivateSensor(string id)
    //{
    //    await _sensorRepository.DeactivateSensorAsync(id);
    //    return RedirectToAction("Index");
    //}

    // Update Sensor Status (Enable/Disable)

    //[HttpPost]
    //public async Task<IActionResult> DisableSensor(string id)
    //{
    //    var sensor = await _sensorRepository.GetSensorByIdAsync(id);
    //    if (sensor == null)
    //    {
    //        return NotFound();
    //    }

    //    sensor.IsActive = false;
    //    await _sensorRepository.UpdateSensorAsync(sensor);

    //    return Json(new { success = true, message = "Sensor disabled successfully!" });
    //}

    [HttpPost]
    public async Task<IActionResult> ToggleSensorStatus(string id, bool isActive)
    {
        var sensor = await _sensorRepository.GetSensorByIdAsync(id);
        if (sensor == null)
        {
            return Json(new { success = false, message = "Sensor not found." });
        }

        sensor.IsActive = isActive;
        await _sensorRepository.UpdateSensorAsync(sensor);

        return Json(new { success = true });
    }



    // Delete sensor

    [HttpPost]
    public async Task<IActionResult> DeleteSensor(string id)
    {
        var sensor = await _sensorRepository.GetSensorByIdAsync(id);
        if (sensor == null)
        {
            return NotFound();
        }

        await _sensorRepository.DeleteSensorAsync(id);
        return Json(new { success = true, message = "Sensor deleted successfully!" });
    }

    public async Task<IActionResult> AlertThresholdSettings()
    {
        var settings = await _alertThresholdSettingRepository.GetAllAsync();
        return PartialView("_AlertThresholdSettingsPartial", settings);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAlertThresholds([FromBody] List<AlertThresholdSetting> updatedSettings)
    {
        if (updatedSettings == null || !updatedSettings.Any())
        {
            return Json(new { success = false, message = "No settings to update." });
        }

        try
        {
            foreach (var updatedSetting in updatedSettings)
            {
                var existingSetting = await _alertThresholdSettingRepository.GetByParameterAsync(updatedSetting.Parameter);
                if (existingSetting != null)
                {
                    existingSetting.ThresholdValue = updatedSetting.ThresholdValue;
                    existingSetting.IsActive = updatedSetting.IsActive;
                    existingSetting.LastUpdated = DateTime.Now;
                    await _alertThresholdSettingRepository.UpdateAsync(existingSetting);
                }
                else
                {
                    // Handle case where a setting for the parameter doesn't exist
                    // You might want to create a new one in this case, depending on your requirements.
                    // For now, we'll just log a warning.
                    Console.WriteLine($"Warning: No existing setting found for parameter '{updatedSetting.Parameter}'.");
                }
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error updating thresholds: {ex.Message}" });
        }
    }


}
