using Microsoft.AspNetCore.Mvc;
using SDTP_Project1.Repositories;
using SDTP_Project1.Models;
using System.Threading.Tasks;


namespace SDTP_Project1.Controllers;
public class AdminController : Controller
{
    private readonly ISensorRepository _sensorRepository;
    // Inject additional repositories as needed, e.g., for SimulationConfiguration, AlertThresholdSetting, etc.

    public AdminController(ISensorRepository sensorRepository)
    {
        _sensorRepository = sensorRepository;
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

    // Similarly, add actions for Simulation Configuration, Alert Threshold Settings, and User Account Management.
}
