using Microsoft.AspNetCore.Mvc;
using SDTP_Project1.Models;
using SDTP_Project1.Repositories;
using System.Threading.Tasks;

public class AdminController : Controller
{
    private readonly ISensorRepository _sensorRepository;

    public AdminController(ISensorRepository sensorRepository)
    {
        _sensorRepository = sensorRepository;
    }

    // Dashboard: List all sensors
    public async Task<IActionResult> Index()
    {
        var sensors = await _sensorRepository.GetAllSensorsAsync();
        return View(sensors);
    }

    // GET: Create new sensor form
    [HttpGet]
    public IActionResult CreateSensor()
    {
        return View();
    }

    // POST: Create new sensor
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSensor(Sensor sensor)
    {
        if (ModelState.IsValid)
        {
            // Sensor values are not added at creation
            await _sensorRepository.AddSensorAsync(sensor);
            return RedirectToAction(nameof(Index));
        }
        return View(sensor);
    }

    // GET: Edit sensor popup (can be loaded via AJAX or partial view in a modal)
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

    // POST: Edit sensor (form submit from modal)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSensor(Sensor sensor)
    {
        if (ModelState.IsValid)
        {
            await _sensorRepository.UpdateSensorAsync(sensor);
            return RedirectToAction(nameof(Index));
        }
        return PartialView("_EditSensorPartial", sensor);
    }

    // POST: Remove sensor
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSensor(string id)
    {
        // You can choose to perform a soft delete (deactivate) or a complete remove.
        await _sensorRepository.DeactivateSensorAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
