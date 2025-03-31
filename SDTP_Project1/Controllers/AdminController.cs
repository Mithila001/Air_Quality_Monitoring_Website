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

    [HttpPost]
    public async Task<IActionResult> CreateSensor(Sensor sensor)
    {
        if (ModelState.IsValid)
        {
            await _sensorRepository.AddSensorAsync(sensor);
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
        return View(sensor);
    }

    [HttpPost]
    public async Task<IActionResult> EditSensor(Sensor sensor)
    {
        if (ModelState.IsValid)
        {
            await _sensorRepository.UpdateSensorAsync(sensor);
            return RedirectToAction("Index");
        }
        return View(sensor);
    }

    // Deactivate sensor
    [HttpPost]
    public async Task<IActionResult> DeactivateSensor(string id)
    {
        await _sensorRepository.DeactivateSensorAsync(id);
        return RedirectToAction("Index");
    }

    // Similarly, add actions for Simulation Configuration, Alert Threshold Settings, and User Account Management.
}
